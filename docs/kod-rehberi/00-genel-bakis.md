# Kod Rehberi — Genel Bakış

Bu klasör, projedeki her dosyanın ve fonksiyonun **ne işe yaradığını** katman katman açıklar. Mimarinin genel yapısı için repo kökündeki `CLAUDE.md` dosyasına bakabilirsin; burada odak, dosya/fonksiyon seviyesinde detaydır.

İstek akışı şu sırayla ilerler:

```
İstemci (Postman/tarayıcı)
   -> Controller (KariyerNet/Controllers)
      -> Service (KariyerNet.Application/Services)
         -> Repository interface (KariyerNet.Application/Interfaces)
            -> Repository implementasyonu (KariyerNet.Infrastructure/Repositories)
               -> AppDbContext (KariyerNet.Infrastructure/Persistence) -> Veritabanı
```

## Dosyalar

1. [01-domain-katmani.md](01-domain-katmani.md) — Varlıklar (entity'ler)
2. [02-application-katmani.md](02-application-katmani.md) — DTO'lar, arayüzler, servisler, validator'lar (Auth)
3. [03-infrastructure-katmani.md](03-infrastructure-katmani.md) — Veritabanı bağlamı, repository, JWT üretici
4. [04-api-katmani.md](04-api-katmani.md) — `Program.cs` ve `AuthController`
5. [05-ilan-ve-basvuru-akislari.md](05-ilan-ve-basvuru-akislari.md) — CandidateProfile, JobPosting, JobApplication akışları ve yetkilendirme

## Yetkilendirme özeti

| Endpoint | Rol |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | Herkes |
| `GET /api/jobpostings`, `GET /api/jobpostings/{id}` | Herkes |
| `POST /api/jobpostings`, `GET /api/jobpostings/mine` | Employer |
| `POST /api/candidateprofile`, `GET /api/candidateprofile/me` | Candidate |
| `POST /api/jobapplications`, `GET /api/jobapplications/me` | Candidate |
| `GET /api/jobapplications/posting/{id}`, `PUT /api/jobapplications/{id}/status` | Employer (sadece kendi ilanı) |
