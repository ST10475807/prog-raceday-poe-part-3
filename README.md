# RaceDay — Part 3: MVC Web Application, Azure Blob Storage and Docker

RaceDay is a .NET 8 race-event platform with an ASP.NET Core REST API and an ASP.NET Core MVC front end. The MVC application performs all data operations through the Part 2 API. It does not access SQL Server or Azure directly.

## Features implemented

### Authentication and navigation
- Registration and login through `POST /api/auth/register` and `POST /api/auth/login`.
- JWT access token, role and user ID are retained in ASP.NET Core session.
- Unauthenticated users are redirected to login for protected actions; Organiser and Participant routes are role checked.
- Logout clears the session.

### Organiser
- Dashboard with own events, active enrolment totals and upcoming event count.
- Create, edit and delete events (API ownership rules and enrolment constraints still apply).
- Upload event banners through the API.
- Create categories, assign existing categories to events, and remove unused assignments.
- View participant enrolments and selected categories.
- Enter/update finish positions, finish times and result status.

### Participant
- Browse upcoming Open events and filter by type/date.
- View event details, banners and available categories.
- Enrol in a selected category through the API.
- View own enrolments and statuses.
- View personal results, including event date, category, finish time and finishing position/out-of-finishers count.
- View/update profile details and upload a profile picture through the API.

### Azure Blob Storage
`RaceDay.Api/Services/BlobStorageService.cs` handles file uploads. The MVC application sends multipart uploads to API routes; it never uses Azure SDK credentials. Configure the API environment variables below to store files in Azure Blob Storage. If no connection string is configured, the API service uses its local `wwwroot/uploads` fallback for development.

### Docker and CI
- `RaceDay.Mvc/Dockerfile` uses a multi-stage .NET 8 build/runtime image and runs as the non-root runtime user.
- GitHub Actions restores and builds the complete solution (API and MVC) and runs the API unit tests.

## Requirements
- .NET 8 SDK
- SQL Server / SQL Server Express / LocalDB for the API database
- Docker Desktop (optional, for container run)
- Azure Storage account for cloud image storage (optional for local development; required to demonstrate actual Blob Storage upload)

## Configure the API

1. Update `RaceDay.Api/appsettings.json` `ConnectionStrings:RaceDayConnection` for your SQL Server instance.
2. Configure a private JWT key (at least 32 bytes) using user secrets or environment variables; do not commit production secrets.
3. For Azure Blob Storage, set these environment variables on the API process:

```powershell
$env:AzureBlob__ConnectionString="<your Azure Storage connection string>"
$env:AzureBlob__Container="raceday-images"
```

4. Create/apply the EF Core database migration if this database has not already been created:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef migrations add InitialRaceDay --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
dotnet ef database update --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
```

If `InitialRaceDay` already exists, do not add it again; run `dotnet ef database update` only. When changing EF models later, create a new migration.

## Run locally (Windows PowerShell)

Open a terminal in the repository root (the folder containing `RaceDay.sln`):

```powershell
dotnet restore .\RaceDay.sln
dotnet build .\RaceDay.sln
```

Start the API in terminal 1:

```powershell
dotnet run --project .\RaceDay.Api\RaceDay.Api.csproj
```

The supplied launch profile uses `http://localhost:5080`; Swagger is at `http://localhost:5080/swagger`.

Start MVC in terminal 2:

```powershell
dotnet run --project .\RaceDay.Mvc\RaceDay.Mvc.csproj
```

MVC defaults to `http://localhost:5000` (or the URL printed by `dotnet run`). It calls the API at `http://localhost:5080/`. Override with `ApiBaseUrl` if the API is hosted elsewhere.

Register one Organiser and one Participant through the MVC registration page to test both role experiences. For an event banner or profile image to be stored in Azure, ensure the API has the Azure environment variables configured before starting it.

## Docker — build and run MVC

The API must be running and reachable by the container. From the repository root:

```bash
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc ./RaceDay.Mvc
docker run --rm -p 8080:8080 -e ApiBaseUrl=http://host.docker.internal:5080/ raceday-mvc
```

Then open `http://localhost:8080`. `host.docker.internal` lets the MVC container reach an API running on the host with Docker Desktop. For Linux Docker Engine, configure a reachable API host address or use a Docker network and API service DNS name. The MVC container does not include or connect directly to SQL Server or Azure; those remain behind the API.

## Important API routes used by MVC

| Feature | API route |
|---|---|
| Register / login | `POST /api/auth/register`, `POST /api/auth/login` |
| Event list / details | `GET /api/events`, `GET /api/events/{id}` |
| Event create/update/delete | `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` |
| Categories | `GET/POST /api/categories` |
| Assign/remove event category | `POST/DELETE /api/events/{eventId}/categories/{categoryId}` |
| Event enrolments | `GET /api/events/{eventId}/enrolments` |
| Participant enrolment | `POST /api/events/{eventId}/categories/{categoryId}/enrolments` |
| My enrolments/results | `GET /api/enrolments/me`, `GET /api/results/me` |
| Capture result | `POST/PUT /api/enrolments/{enrolmentId}/result` |
| Profile | `GET/PUT /api/users/me` |
| Banner/profile uploads | `POST /api/files/events/{eventId}/banner`, `POST /api/files/profile-picture` |

## GitHub and submission checklist

- [ ] Push this work to a clearly named Part 3 branch or folder in your own GitHub repository.
- [ ] Make at least 20 meaningful commits that describe actual changes (do not create empty/fake commits).
- [ ] Confirm the GitHub Actions workflow succeeds on your repository.
- [ ] Test both Organiser and Participant accounts against your own SQL Server/API configuration.
- [ ] Configure Azure Blob Storage and demonstrate a real banner and profile-picture upload.
- [ ] Build and run the Docker image; show the MVC site communicating with the API.
- [ ] Record an unlisted YouTube demonstration covering the application, MVC structure, API-only data access, Blob Storage upload flow and Docker run. Add the real video URL here:

**YouTube demonstration:** _Add your unlisted video URL after recording._

**GitHub repository:** _Add your repository URL before submitting on ARC._

## Verification note

The project source has been updated to cover the MVC workflows above. Run `dotnet restore`, `dotnet build`, database migrations and the end-to-end checks on your own machine before submission. This workspace did not have the .NET SDK or an Azure account configured, so a successful local build, live database test, Azure upload, Docker run and GitHub Actions run cannot be claimed from this package alone.
