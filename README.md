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
- **Framework**: ASP.NET Core 10 (Razor Pages)
- **Database**: PostgreSQL (Entity Framework Core)
- **Frontend**: Custom UI mapped to Brand Guidelines (Bootstrap 5, Lucide Icons, Chart.js)
- **Reporting**: QuestPDF
- **Deployment**: Local IIS Server

## Getting Started

### Prerequisites
- .NET 10 SDK
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
The application implements SRE principles to ensure frontend resilience against backend exceptions:
- **Structured Logging (`ILogger<T>`)**: Captures trace telemetry to isolate runtime exceptions.
- **Fail-Safe Data Wrappers**: Database interactions are wrapped in `try-catch` blocks that fallback to empty model collections, preventing native `NullReferenceExceptions` in Razor views.
- **UI Notifications**: Utilizes `TempData` to display dismissible Bootstrap alerts for system status communication.
- **Global Exception Middleware**: `app.UseExceptionHandler("/Error")` provides a branded fallback UI in the event of a catastrophic failure.
- **Localized Assets**: All critical JavaScript and CSS assets are stored locally in `wwwroot/lib` to ensure deployment independence from external CDNs.

Learn more in the [SRE Error Handling Documentation](docs/sre-error-handling.md).

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

## 📊 Data Grid & Sorting Architecture
To ensure extreme performance and accurate pagination, this application utilizes **Server-Side Sorting** combined with an air-gapped **DOM Hydration** frontend strategy. 

* **The Backend (`.cshtml.cs`):** Tables are bound to a `sortOrder` URL parameter. Entity Framework Core processes the `OrderBy` sorting directly in the PostgreSQL database *before* slicing the pagination (`.Take()`), ensuring perfect data accuracy.
* **The Frontend (`table-sort.js`):** A lightweight, zero-dependency Vanilla JS script handles click routing and UX. It relies strictly on HTML data attributes:
    * `data-column="emp"`: Maps the UI column to the C# `switch` statement string.
    * `data-sort-default="asc"`: Allows the C# server to asynchronously pass its state down to the HTML. The JS engine reads this on load to "hydrate" the UI.
    * `data-master-default="true"`: Applied to fallback columns (like Request Date) so the script toggles infinitely (Asc <-> Desc) instead of clearing the URL state.
* **Layout Stability & UX:** All tables enforce `table-layout: fixed;` to prevent the auto-layout algorithm from shifting column sizes. The JS engine utilizes a 300ms UX delay to allow loading overlays to render, and injects URL hash anchors (`#table-id`) to preserve the user's scroll position upon page reload.
