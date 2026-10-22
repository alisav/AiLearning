# AiLearning

.NET 10 Web API (EF Core + SQLite) ve React + TypeScript + Vite'tan oluşan full-stack To-Do uygulaması.

## Proje yapısı

- `src/AiLearning.Api/`: .NET 10 Minimal API (`http://localhost:5139`)
  - `Program.cs`: servis kaydı, geliştirme ortamında `EnsureCreated` ile veritabanı oluşturma, endpoint eşleme
  - `Todos/`: `TodoItem`, `TodoDbContext`, `TodoEndpoints` (`/api/todos`)
  - `appsettings.json`: `ConnectionStrings:Todos` (`Data Source=ailearning.db`, proje dizinine göre çözülür)
  - `AiLearning.Api.http`: elle API testi için örnek istekler
- `src/AiLearning.Web/`: React + TypeScript + Vite (`http://localhost:5173`)
  - `/api` istekleri geliştirmede Vite proxy ile `http://localhost:5139` adresine gider (`vite.config.ts`)
  - `src/todoApi.ts`: API istemcisi; `src/TodoList.tsx`: To-Do arayüzü
- `tests/AiLearning.Api.Tests/`: xUnit API entegrasyon testleri (`WebApplicationFactory`, test başına geçici SQLite dosyası)
- `.github/workflows/ci.yml`: PR ve `main` push'larında `backend` (restore, build, test) ve `frontend` (`npm ci`, lint, build; Node 22) job'ları
- `AiLearning.slnx`: .NET solution

## Komutlar

Backend (repo kökünden):

```bash
dotnet build AiLearning.slnx
dotnet run --project src/AiLearning.Api
```

Frontend (`src/AiLearning.Web` içinden):

```bash
npm install
npm run build   # tsc -b && vite build
npm run lint    # oxlint
npm run dev
```

Backend testleri (repo kökünden):

```bash
dotnet test AiLearning.slnx
```

Testler `tests/AiLearning.Api.Tests` altındadır ve endpoint'leri, doğrulamaları, durum kodlarını ve yeniden başlatma sonrası SQLite kalıcılığını kapsar. `TodoApiFactory` her test için ayrı bir geçici SQLite dosyası kullanır; geliştirme veritabanına dokunan test yazma. Yeni endpoint veya davranış eklerken buraya entegrasyon testi ekle.

## Çalışma kuralları

1. **`main` her zaman çalışan ve doğrulanmış sürümdür.** `main` üzerinde doğrudan kod değişikliği, commit veya push yapma.
2. **Her iş için ayrı branch aç.** Yeni özellikler için `feature/<kisa-ad>`, hata düzeltmeleri için `fix/<kisa-ad>` kullan (ör. `feature/todo-edit`, `fix/todo-validation`). Branch'i güncel `main`'den oluştur.
3. **Branch'te geliştir, test et, commit et ve push et.** Uzak depoya ilk push'u `git push -u origin <branch>` ile yap. Commit mesajlarında Conventional Commits kullan (`feat:`, `fix:`, `chore:`, `docs:`).
4. **`main` ile birleştirmeyi yalnızca kullanıcı istediğinde yap.** Testler geçince branch adını ve yapılan değişiklikleri bildir; merge, PR veya `main`'e push için kullanıcının talebini bekle.
5. **Kullanıcının çalışmasını koru.** Commit'lenmemiş değişiklikleri, commit'leri veya Git geçmişini izin almadan silme ya da yeniden yazma. Buna `reset --hard`, `push --force`, rebase/amend ile yayımlanmış geçmişi değiştirmek ve kullanıcının çalışan süreçlerini sonlandırmak dahildir.
6. **Her değişiklikten sonra ilgili kontrolleri çalıştır.**
   - Backend değiştiyse: `dotnet build AiLearning.slnx` (0 hata) ve `dotnet test AiLearning.slnx` (tümü geçmeli).
   - Frontend değiştiyse: `npm run build` ve `npm run lint`.
   - Uçtan uca etkisi olan değişikliklerde (ör. endpoint veya proxy değişiklikleri) API ve Vite'ı birlikte çalıştırıp `http://localhost:5173/api/...` üzerinden test et.
7. **Sürümleri gereksiz yere yükseltme.** Mevcut sürümler: .NET 10 (`net10.0`), EF Core SQLite 10.0.x, React 19, TypeScript ~6.0, Vite 8. Gereksiz NuGet/npm paketi veya mimari katman ekleme.
8. **Gereksiz onay isteme.** Dosya düzenleme, build/test, branch oluşturma, branch'e commit ve push gibi rutin, geri alınabilir işlemleri kendin tamamla. Şu durumlarda önce sor:
   - yıkıcı işlemler (veri, dosya veya geçmiş silme)
   - önemli mimari kararlar (ör. yeni katman, veritabanı değişikliği, EF migration'larına geçiş)
   - `main` ile birleştirme
9. **İş bitince kısa rapor ver.** Raporda şunlar olsun: yapılanlar, test sonuçları, branch adı, commit hash'i ve push durumu.

## Ortama özel notlar

- SQLite dosyası `src/AiLearning.Api/ailearning.db` konumundadır ve Git'e eklenmez (`*.db`, `*.db-shm`, `*.db-wal` `.gitignore`'da). Testte oluşan verinin kullanıcının veritabanına karışmaması için gerekirse test sonrası temizle. Kullanıcının mevcut veritabanını silme.
- Veritabanı migration olmadan `EnsureCreated` ile oluşturulur. Şema değişikliği gerekirse (ör. yeni kolon) mevcut veritabanı otomatik güncellenmez. Migration'a geçmeden önce kullanıcıya sor.
- Kullanıcının kendi API'si 5139 portunda açık olabilir ve `bin/` altındaki exe'yi kilitleyebilir. Bu süreci kapatma. Derleme ve test için `--artifacts-path <geçici-dizin>` kullan (ör. `dotnet test AiLearning.slnx --artifacts-path <geçici-dizin>`). API'yi elle denemek gerekirse `dotnet build src/AiLearning.Api -o <geçici-dizin>` ile derleyip `dotnet <geçici-dizin>/AiLearning.Api.dll --urls http://localhost:<başka-port> --environment Development` ile, çalışma dizini `src/AiLearning.Api` olacak şekilde yap.
- Kabuk Windows (PowerShell / Git Bash). Bash'te `cd` ile çalışma dizinini değiştirmekten kaçın, mutlak yol kullan.
