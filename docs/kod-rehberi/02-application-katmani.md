# Application Katmanı (`KariyerNet.Application`)

İş mantığının yaşadığı katman. EF Core veya ASP.NET'e dair hiçbir şey bilmez; sadece arayüzler (interface) üzerinden çalışır.

## `DTOs/RegisterUserDto.cs`
Kayıt (register) isteğinde istemciden gelen veriyi taşıyan basit bir veri sınıfı. Fonksiyonu yok, `FullName`, `Email`, `Password` ve `Role` (`Employer`/`Candidate`) alanlarını tutar — kullanıcı kayıt olurken hangi rolde olacağını kendisi seçer.

## `Validators/RegisterUserDtoValidator.cs`
`RegisterUserDto`'yu doğrular: `FullName`/`Password` boş olamaz, `Password` en az 6 karakter, `Email` geçerli formatta olmalı, `Role` geçerli bir enum değeri (`Employer`/`Candidate`) olmalı.

## `Validators/ValidationResultExtensions.cs`
FluentValidation'ın ürettiği `ValidationResult`'ı, controller'ların `ValidationProblem`'e verebileceği `Dictionary<string, string[]>` formatına çeviren bir yardımcı (extension) metot: **`ToErrorDictionary()`**. Her controller'da aynı "hataları grupla, diziye çevir" kodunu tekrar yazmamak için eklendi; tüm controller'lar (`AuthController`, `CandidateProfileController`, `JobPostingsController`, `JobApplicationsController`) bunu kullanır.

## `DTOs/LoginUserDto.cs`
Giriş (login) isteğinde istemciden gelen `Email` ve `Password` alanlarını taşır.

## `Interfaces/IUserRepository.cs`
Veritabanı erişiminin **sözleşmesini** tanımlar (gerçek implementasyonu Infrastructure katmanında). Servis katmanı, veritabanının SQL Server mı yoksa başka bir şey mi olduğunu bilmeden bu arayüzü kullanır.
- `GetByEmailAsync(email)`: verilen email'e sahip kullanıcıyı bulur, yoksa `null` döner.
- `AddAsync(user)`: yeni bir kullanıcıyı (henüz kaydetmeden) EF Core'un takibine ekler.
- `SaveChangesAsync()`: bekleyen değişiklikleri veritabanına gerçekten yazar.

## `Interfaces/IJwtTokenGenerator.cs`
JWT üretiminin sözleşmesi.
- `GenerateToken(user)`: verilen kullanıcı için imzalı bir JWT (access token) string'i üretir.

## `Validators/LoginUserDtoValidator.cs`
FluentValidation ile `LoginUserDto`'nun doğruluğunu kontrol eder (controller'a hiç girmeden önce çalışır).
- `Email`: boş olamaz, geçerli bir e-posta formatında olmalı.
- `Password`: boş olamaz.

## `Services/UserService.cs`
Kullanıcıyla ilgili iş kurallarının uygulandığı yer. Controller'lar doğrudan veritabanına değil, buraya istek atar.

- **`RegisterAsync(dto)`**:
  1. `dto.Email` ile zaten kayıtlı bir kullanıcı var mı diye `IUserRepository.GetByEmailAsync` ile kontrol eder.
  2. Varsa `InvalidOperationException` fırlatır ("bu email zaten kayıtlı").
  3. Yoksa yeni bir `User` nesnesi oluşturur, şifreyi `BCrypt.Net.BCrypt.HashPassword` ile hash'ler, rolünü `dto.Role`'den alır (kullanıcının kayıt sırasında seçtiği rol).
  4. `AddAsync` + `SaveChangesAsync` ile veritabanına kaydeder ve oluşturulan kullanıcıyı döner.

> `CandidateProfileService`, `JobPostingService` ve `JobApplicationService` için bkz. [05-ilan-ve-basvuru-akislari.md](05-ilan-ve-basvuru-akislari.md).

- **`LoginAsync(dto)`**:
  1. `dto.Email` ile kullanıcıyı veritabanından bulur.
  2. Kullanıcı yoksa **veya** `BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)` şifreyi doğrulamazsa, `UnauthorizedAccessException` fırlatır ("email veya şifre hatalı"). Kullanıcının var olup olmadığını ayrı ayrı belirtmemesi bilinçlidir — bu, saldırganların "bu email kayıtlı mı" bilgisini sızdırmayı önler.
  3. Doğruysa `IJwtTokenGenerator.GenerateToken(user)` ile bir JWT üretip döner.
