# Domain Katmanı (`KariyerNet.Domain`)

Bu katmanda fonksiyon/metot yoktur — sadece veriyi temsil eden **entity** (varlık) sınıfları vardır. Hiçbir dış bağımlılığı (EF Core, ASP.NET vs.) yoktur, saf C# sınıflarıdır.

## `Entities/User.cs`
- **`UserRole` enum**: Kullanıcının rolünü tutar → `Employer` (işveren) veya `Candidate` (iş arayan).
- **`User` sınıfı**: Sistemdeki her kullanıcıyı temsil eder.
  - `Id`: benzersiz kimlik.
  - `FullName`, `Email`: kullanıcı bilgileri.
  - `PasswordHash`: şifrenin **düz hali değil**, BCrypt ile hash'lenmiş hali.
  - `Role`: `UserRole` enum değeri.
  - `CreatedAt`: kayıt zamanı.

## `Entities/CandidateProfile.cs`
- Bir adayın (Candidate rolündeki kullanıcının) özgeçmiş bilgisini tutar.
- `UserId`/`User`: hangi kullanıcıya ait olduğunu gösteren foreign key + navigasyon.
- `CvFilePath`, `CvUploadedAt`: CV dosya yolu ve yüklenme zamanı.

## `Entities/JobPosting.cs`
- Bir iş ilanını temsil eder.
- `EmployerId`/`Employer`: ilanı kimin (hangi işveren kullanıcının) açtığını gösterir.
- `Title`, `Description`, `Requirements`: ilan içeriği.
- `CreatedAt`: ilanın oluşturulma zamanı.

## `Entities/JobApplication.cs`
- **`ApplicationStatus` enum**: `Pending` (beklemede), `Accepted` (kabul edildi), `Rejected` (reddedildi).
- **`JobApplication` sınıfı**: Bir adayın bir iş ilanına yaptığı başvuruyu temsil eder.
  - `CandidateId`/`Candidate`: başvuruyu yapan kullanıcı.
  - `JobPostingId`/`JobPosting`: başvurulan ilan.
  - `Status`: başvurunun durumu (varsayılan `Pending`).
  - `AppliedAt`: başvuru zamanı.
