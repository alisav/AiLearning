# AiLearning

.NET 10 Web API + EF Core SQLite + React (TypeScript, Vite) full-stack proje.

## Yapı

```text
AiLearning/
├── src/
│   ├── AiLearning.Api/   # .NET 10 Web API (http://localhost:5139)
│   └── AiLearning.Web/   # React + TypeScript + Vite (http://localhost:5173)
└── AiLearning.slnx
```

## Gereksinimler

- .NET 10 SDK
- Node.js 22+ ve npm

## Çalıştırma

İki ayrı terminalde:

**Backend**

```bash
dotnet run --project src/AiLearning.Api
```

Kontrol: `curl http://localhost:5139/api/health` → `{"status":"ok"}`

**Frontend**

```bash
cd src/AiLearning.Web
npm install
npm run dev
```

Tarayıcıda http://localhost:5173 adresini açın. Geliştirme ortamında `/api` istekleri Vite proxy ile backend'e (`http://localhost:5139`) yönlendirilir.

## API

| Metot | Yol | Açıklama |
|---|---|---|
| GET | `/api/health` | Sağlık kontrolü |
| GET | `/api/todos` | Görevleri listeler |
| POST | `/api/todos` | Görev ekler (`{"title":"..."}`) |
| PUT | `/api/todos/{id}` | Görevi günceller (`{"title":"...","isCompleted":true}`) |
| DELETE | `/api/todos/{id}` | Görevi siler |

## Veritabanı

Görevler EF Core ile SQLite veritabanında kalıcı olarak tutulur.

- Dosya: `src/AiLearning.Api/ailearning.db` (Git'e eklenmez)
- Bağlantı ayarı: `appsettings.json` → `ConnectionStrings:Todos`
- Geliştirme ortamında (`ASPNETCORE_ENVIRONMENT=Development`) API ilk açılışta veritabanını otomatik oluşturur.
- Veritabanını sıfırlamak için API'yi durdurup `ailearning.db` dosyasını silmeniz yeterlidir.

## Derleme

```bash
dotnet build AiLearning.slnx
cd src/AiLearning.Web && npm run build
```
