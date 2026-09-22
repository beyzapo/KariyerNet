# Infrastructure Katmanı (`KariyerNet.Infrastructure`)

Application katmanındaki arayüzlerin **gerçek** (EF Core / SQL Server'a bağlı) implementasyonlarını içerir.

## `Persistence/AppDbContext.cs`
EF Core'un veritabanıyla konuştuğu ana sınıf.
- `Users`, `CandidateProfiles`, `JobPostings`, `JobApplications`: her biri ilgili tabloyu temsil eden `DbSet` özellikleri — sorgu yazarken bu koleksiyonlar üzerinden LINQ kullanılır.
- **`OnModelCreating(modelBuilder)`**: EF Core'a tablolar arası ilişkilerin nasıl kurulacağını söyler. Her ilişkide `DeleteBehavior.Restrict` kullanılmış — yani örneğin bir `User` silinmeye çalışıldığında, ona bağlı `JobApplication`/`JobPosting`/`CandidateProfile` kayıtları varsa veritabanı bu silme işlemini reddeder (kademeli/otomatik silme — cascade delete — yapılmaz). Bu, yanlışlıkla ilişkili verilerin topluca silinmesini engellemek için bilinçli bir tercih.

## `Repositories/UserRepository.cs`
`IUserRepository` arayüzünün EF Core ile çalışan implementasyonu.
- **`GetByEmailAsync(email)`**: `Users` tablosunda `Email` alanı eşleşen ilk kaydı bulur (`FirstOrDefaultAsync`), yoksa `null` döner.
- **`AddAsync(user)`**: yeni kullanıcıyı EF Core'un değişiklik takipçisine (change tracker) ekler — henüz veritabanına yazmaz.
- **`SaveChangesAsync()`**: takip edilen tüm değişiklikleri (INSERT/UPDATE/DELETE) tek seferde veritabanına gönderir.

## `Security/JwtTokenGenerator.cs`
`IJwtTokenGenerator` arayüzünün implementasyonu; kullanıcı bilgisinden imzalı bir JWT üretir.
- **`GenerateToken(user)`**:
  1. Kullanıcının kimliğini, e-postasını, adını ve rolünü **claim** (JWT içine gömülecek bilgi parçası) olarak bir listeye ekler.
  2. `appsettings.json`'daki `Jwt:Key` değerinden simetrik bir imzalama anahtarı (`SymmetricSecurityKey`) oluşturur ve HMAC-SHA256 ile imzalama kimlik bilgisi (`SigningCredentials`) hazırlar.
  3. `Jwt:ExpiresInMinutes` süresi kadar geçerli olacak şekilde token'ın son kullanma zamanını ayarlar.
  4. `Jwt:Issuer`/`Jwt:Audience` ile birlikte tüm bu bilgilerden bir `JwtSecurityToken` oluşturur ve bunu string'e çevirip (`WriteToken`) döner.
  - Bu token, `AuthController.Login` tarafından istemciye döndürülür; istemci sonraki isteklerde `Authorization: Bearer <token>` header'ı ile kimliğini kanıtlar.
