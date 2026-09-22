# İlan ve Başvuru Akışları — CandidateProfile, JobPosting, JobApplication

Bu üç entity, `Auth` akışıyla aynı Controller → Service → Repository desenini takip eder. Hepsi JWT ile korunur; rol kontrolü `[Authorize(Roles = "...")]` ile, "sahiplik" kontrolü (örn. bir işverenin sadece kendi ilanını görmesi) ise servis katmanında yapılır.

## CandidateProfile — Aday CV Profili

**Amaç:** Bir adayın (Candidate) CV dosya yolunu tutması. Gerçek dosya yükleme (blob storage) henüz yok; istemci CV'yi başka bir yerde barındırıp buraya sadece dosya yolunu/URL'sini gönderir.

- `DTOs/CreateOrUpdateCandidateProfileDto.cs`: sadece `CvFilePath` alanı.
- `Validators/CreateOrUpdateCandidateProfileDtoValidator.cs`: `CvFilePath` boş olamaz.
- `Interfaces/ICandidateProfileRepository.cs` / `Infrastructure/Repositories/CandidateProfileRepository.cs`:
  - `GetByUserIdAsync(userId)`: kullanıcının profilini bulur.
  - `AddAsync`, `SaveChangesAsync`: yeni profil ekleme/kaydetme.
- `Services/CandidateProfileService.cs`:
  - **`CreateOrUpdateAsync(userId, dto)`**: kullanıcının profili yoksa yeni oluşturur, varsa `CvFilePath`'i günceller (yani bu endpoint hem "oluştur" hem "güncelle" görevi görür — **upsert**).
  - **`GetByUserIdAsync(userId)`**: kullanıcının profilini döner, yoksa `null`.
- `Controllers/CandidateProfileController.cs` (**tüm endpoint'ler `[Authorize(Roles = "Candidate")]`**):
  - `POST /api/candidateprofile`: giriş yapmış adayın kendi profilini oluşturur/günceller.
  - `GET /api/candidateprofile/me`: giriş yapmış adayın kendi profilini döner, yoksa `404`.

## JobPosting — İş İlanı

**Amaç:** İşverenlerin (Employer) iş ilanı açması; herkesin ilanları görebilmesi.

- `DTOs/CreateJobPostingDto.cs`: `Title`, `Description`, `Requirements`.
- `Validators/CreateJobPostingDtoValidator.cs`: üçü de boş olamaz.
- `Interfaces/IJobPostingRepository.cs` / `Infrastructure/Repositories/JobPostingRepository.cs`:
  - `GetByIdAsync(id)`, `GetAllAsync()` (en yeniden eskiye sıralı), `GetByEmployerIdAsync(employerId)` (bir işverenin kendi ilanları), `AddAsync`, `SaveChangesAsync`.
- `Services/JobPostingService.cs`:
  - **`CreateAsync(employerId, dto)`**: yeni ilanı, ilanı açan işverenin id'siyle (`employerId` — JWT'den gelir, istemci gönderemez) oluşturur.
  - **`GetAllAsync()`**: tüm ilanları döner (herkese açık).
  - **`GetMineAsync(employerId)`**: sadece o işverenin kendi ilanlarını döner.
  - **`GetByIdAsync(id)`**: tek bir ilanı döner, yoksa `KeyNotFoundException` fırlatır (controller bunu `404`'e çevirir).
- `Controllers/JobPostingsController.cs`:
  - `POST /api/jobpostings` — **`[Authorize(Roles = "Employer")]`**: yeni ilan açar.
  - `GET /api/jobpostings` — **herkese açık**: tüm ilanları listeler.
  - `GET /api/jobpostings/mine` — **`[Authorize(Roles = "Employer")]`**: giriş yapmış işverenin kendi ilanlarını listeler.
  - `GET /api/jobpostings/{id}` — **herkese açık**: tek bir ilanın detayını döner.

## JobApplication — İş Başvurusu

**Amaç:** Adayların ilanlara başvurması, işverenlerin kendi ilanlarına gelen başvuruları görüp durumunu (kabul/red) güncellemesi.

- `DTOs/CreateJobApplicationDto.cs`: sadece `JobPostingId` (hangi ilana başvurulduğu; `CandidateId` istemciden alınmaz, JWT'den okunur).
- `DTOs/UpdateJobApplicationStatusDto.cs`: sadece `Status`.
- `Validators/CreateJobApplicationDtoValidator.cs`: `JobPostingId` boş (`Guid.Empty`) olamaz.
- `Validators/UpdateJobApplicationStatusDtoValidator.cs`: `Status` geçerli bir enum değeri olmalı **ve** sadece `Accepted` veya `Rejected` olabilir — `Pending`'e geri döndürülemez (bilinçli iş kuralı).
- `Interfaces/IJobApplicationRepository.cs` / `Infrastructure/Repositories/JobApplicationRepository.cs`:
  - `GetByIdAsync(id)`, `GetByCandidateAndPostingAsync(candidateId, jobPostingId)` (aynı adayın aynı ilana daha önce başvurup başvurmadığını kontrol etmek için), `GetByCandidateIdAsync(candidateId)`, `GetByJobPostingIdAsync(jobPostingId)`, `AddAsync`, `SaveChangesAsync`.
- `Services/JobApplicationService.cs` (hem `IJobApplicationRepository` hem `IJobPostingRepository`'ye bağımlı — ilan sahipliğini doğrulamak için):
  - **`ApplyAsync(candidateId, dto)`**: önce ilanın var olup olmadığını kontrol eder (`KeyNotFoundException`), sonra adayın bu ilana **daha önce başvurup başvurmadığını** kontrol eder (`InvalidOperationException` → controller `409 Conflict`'e çevirir), yoksa `Pending` durumunda yeni bir başvuru oluşturur.
  - **`GetMineAsync(candidateId)`**: adayın kendi başvurularını döner.
  - **`GetForPostingAsync(employerId, jobPostingId)`**: bir ilana gelen başvuruları döner — ama önce ilanın var olduğunu ve **gerçekten bu işverene ait olduğunu** kontrol eder (`UnauthorizedAccessException` → controller `403 Forbidden`'a çevirir). Böylece bir işveren başka bir işverenin ilanına gelen başvuruları göremez.
  - **`UpdateStatusAsync(employerId, applicationId, dto)`**: aynı sahiplik kontrolünü başvurunun bağlı olduğu ilan üzerinden yapar, sonra `Status`'u günceller.
- `Controllers/JobApplicationsController.cs` (sınıf seviyesinde `[Authorize]` var, her action kendi rolünü de ekliyor):
  - `POST /api/jobapplications` — **Candidate**: bir ilana başvurur.
  - `GET /api/jobapplications/me` — **Candidate**: kendi başvurularını listeler.
  - `GET /api/jobapplications/posting/{jobPostingId}` — **Employer**: kendi ilanına gelen başvuruları listeler.
  - `PUT /api/jobapplications/{id}/status` — **Employer**: kendi ilanına ait bir başvurunun durumunu `Accepted`/`Rejected` yapar.
