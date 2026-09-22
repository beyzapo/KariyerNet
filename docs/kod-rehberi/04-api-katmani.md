# API Katmanı (`KariyerNet` projesi — `KariyerNet.API`)

Dış dünyaya açılan HTTP uçlarını ve uygulamanın başlangıç (bootstrap) yapılandırmasını içerir.

## `Program.cs`
Uygulama ayağa kalkarken (startup) çalışan, servisleri "DI container"a kaydeden ve HTTP pipeline'ını kuran dosya. Fonksiyon değil ama adım adım ne yaptığı:

1. `AddControllers()`: MVC controller'ları etkinleştirir.
2. `AddDbContext<AppDbContext>(...)`: `appsettings.json`'daki `DefaultConnection` ile SQL Server'a bağlanacak şekilde `AppDbContext`'i DI'a kaydeder.
3. `AddScoped<IUserRepository, UserRepository>()`: her HTTP isteğinde `IUserRepository` istenildiğinde `UserRepository` örneği verilmesini sağlar.
4. `AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()`: aynı mantıkla JWT üretici kaydı.
5. `AddScoped<UserService>()`: `UserService`'i DI'a kaydeder (controller'lar constructor'da alabilsin diye).
6. `AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>()`: `Application` projesindeki tüm FluentValidation validator'larını otomatik tarayıp DI'a kaydeder.
7. `AddAuthentication(...).AddJwtBearer(...)`: gelen isteklerdeki `Authorization: Bearer <token>` header'ını `Jwt:Key/Issuer/Audience` ile doğrulayacak şekilde JWT authentication'ı yapılandırır.
8. `AddAuthorization()`: `[Authorize]` attribute'unun çalışabilmesi için yetkilendirme sistemini etkinleştirir.
9. `app.UseAuthentication()` / `app.UseAuthorization()`: gelen her isteğin önce kimliğinin (authentication), sonra yetkisinin (authorization) kontrol edilmesini sağlayan ara katmanlar (middleware). **Sıra önemlidir** — authentication her zaman authorization'dan önce çalışmalı.
10. `app.MapControllers()`: gelen istekleri, route'larına göre ilgili controller metoduna yönlendirir.

Ayrıca `CandidateProfileService`, `JobPostingService`, `JobApplicationService` ve bunların repository'leri (`ICandidateProfileRepository`, `IJobPostingRepository`, `IJobApplicationRepository`) burada DI'a kaydedilir; `AddControllers()` çağrısına eklenen `.AddJsonOptions(...)` satırı, enum'ların (`UserRole`, `ApplicationStatus`) JSON'da sayı yerine okunaklı string (`"Employer"`, `"Accepted"` gibi) olarak dönmesini/parse edilmesini sağlar.

## `Extensions/ClaimsPrincipalExtensions.cs`
- **`GetUserId(this ClaimsPrincipal user)`**: JWT doğrulandıktan sonra ASP.NET Core'un oluşturduğu `User` (`ClaimsPrincipal`) nesnesinden, token'a gömülü `ClaimTypes.NameIdentifier` claim'ini okuyup `Guid`'e çevirir. Her controller'da "giriş yapmış kullanıcının id'si nedir" tekrarını önlemek için eklendi; `User.GetUserId()` şeklinde çağrılır.

## `Controllers/AuthController.cs`
Kimlik doğrulama ile ilgili HTTP uçlarını (endpoint) barındırır. `UserService` ve `IValidator<LoginUserDto>`'yu constructor'dan alır (dependency injection).

- **`Register(dto)`** — `POST /api/auth/register`:
  1. Önce `_registerValidator.ValidateAsync(dto)` ile `RegisterUserDto`'yu doğrular (isim/şifre/email/rol kontrolü); geçersizse `400 Bad Request`.
  2. `UserService.RegisterAsync(dto)`'yu çağırır.
  3. Başarılıysa yeni kullanıcının bilgilerini (`Id`, `FullName`, `Email`, `Role`, `CreatedAt`) `200 OK` ile döner. **Şifre asla döndürülmez.**
  4. `UserService` "bu email zaten kayıtlı" hatası (`InvalidOperationException`) fırlatırsa, bunu yakalayıp `409 Conflict` döner.

- **`Login(dto)`** — `POST /api/auth/login`:
  1. Önce `_loginValidator.ValidateAsync(dto)` ile `LoginUserDto`'yu doğrular (Email formatı, boş alan kontrolü).
  2. Doğrulama başarısızsa, hangi alanın neden geçersiz olduğunu gösteren bir sözlük (dictionary) hazırlayıp `400 Bad Request` (`ValidationProblem`) döner.
  3. Doğrulama geçerse `UserService.LoginAsync(dto)`'yu çağırır.
  4. Başarılıysa üretilen JWT'yi `{ token: "..." }` şeklinde `200 OK` ile döner.
  5. Email/şifre yanlışsa (`UnauthorizedAccessException`) `401 Unauthorized` döner.
