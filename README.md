# HR Certification Portal (Internal IIS Web Portal)

## Project Overview
A simple internal HR web portal for Specialized Engineering (SPE) to track employee certification requests and manage the certification agency + certification catalog that powers the request form dropdowns. The portal will run on the SPE IIS server, be accessible only on the local network, and will not require user authentication.

## Key Features
- **Zero Authentication**: Designed for internal network access only.
- **Certification Request Form**: Capture employee name, request date, manager, agency, certification, and request type.
- **Agency & Certification Catalog**: Admin-managed lists for dropdown consistency.
- **Compliance Dashboard**: KPI counts, recent activity tracking, and Chart.js integration for coverage insights.
- **Admin Settings**: Manage agencies and certification types (Soft Delete/Deactivation preference).
- **PDF Export Engine**: Built-in QuestPDF reporting engine with WYSIWYG export pipeline.

## Technology Stack
- **Framework**: ASP.NET Core 8 (Razor Pages)
- **Database**: PostgreSQL (Entity Framework Core)
- **Frontend**: Custom UI mapped to Brand Guidelines (Bootstrap 5, Lucide Icons, Chart.js)
- **Reporting**: QuestPDF
- **Deployment**: Local IIS Server

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL Server
- Git

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/ReginaldCosensIII/se-hr-certification-portal.git
   ```
2. Configure the database connection in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=se-hr-certification-portal;"
   }
   ```
3. Apply migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Architecture & Site Reliability (SRE)
Our proactive SRE design guarantees the frontend remains bulletproof against internal backend exceptions:
- **`ILogger<T>` structured backend logging**: Captures deterministic trace telemetry securely isolating variable crash events inline.
- **Fail-safe `try-catch` database wrappers**: Eradicates unhandled native `NullReferenceExceptions` allowing dynamic fallback to empty model collections.
- **Responsive `TempData` UI Notifications**: Broadcasts graceful, dismissible Bootstrap alerts to instantly communicate system status securely.
- **Global Exception Middleware**: `app.UseExceptionHandler("/Error")` enforces a polished, branded user fallback rendering in extreme catastrophe avoiding raw `500` stack traces output.
- **Air-Gapped CDNs**: Localized all critical JavaScript and CSS assets into `wwwroot/lib` to guarantee resilient deployment independence from external CDNs.

Learn more in our [SRE Error Handling Architecture Documentation](docs/sre-error-handling.md).

## Development Workflow
- **Branching**: Use feature branches (`feat/name`, `fix/name`) for all new work.
- **Testing**:
  - All new features must include unit tests.
  - Verification of regression testing before merging.

## Deployment
See `docs/DEPLOYMENT.md` for detailed IIS deployment instructions.

## Documentation
- `docs/REQUIREMENTS.md`: Brand identity, database rules, and feature requirements.
- `docs/TESTING.md`: Testing strategy and requirements.
