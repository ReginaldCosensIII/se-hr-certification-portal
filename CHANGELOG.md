# Changelog

## [Branch Closeout: feat/dashboard-and-core-ui]
### Added
- **Epic 4: Dashboard & Core UI Polish**
  - Integrated dynamic EF Core 10 KPIs mapped directly to `SystemSettings` threshold parameters.
  - Intercepted the native browser F11 Fullscreen API and synced state directly into the unified theme header toggle.
  - Refactored grid layouts for optimal UI space utilization and removed redundant UI inputs.
  - Implemented advanced Request ID string stripping (e.g., parsing "REQ-123" input cleanly down to integer `123`).
  - Standardized the core action icons across all pages to utilize a unified Lucide + Bootstrap semantic color language.
## [Branch Closeout: feat/certifications-management]
### Added
- **Epic 2: Admin Management Module**
  - Implemented `ClientSideDataGrid` Vanilla JS class with native `sessionStorage` state retention across PRG reloads.
  - Standardized Entity Framework endpoints to utilize an `IsActive` Soft-Delete and Reactivation pattern.
  - Engineered a C# QuestPDF export engine for generating stylized Agency & Certification rosters.
- **Epic 3: Certifications Tracking & Analytics**
  - Engineered a "Compliance & Coverage Analytics" dashboard featuring Chart.js integration.
  - Implemented a WYSIWYG (What You See Is What You Get) Client-to-Server PDF export pipeline, capturing canvas states as Base64 strings for QuestPDF rendering.
  - Added intelligent "Pre-Flight" warning modals to explicitly communicate filter constraints before PDF generation.

## [Branch Closeout: feat/backend-integration]
### Accomplishments
- **Full EF Core PostgreSQL relational mapping**: Completely mapped the internal data architecture and schema context.
- **Admin Management UI**: Added CRUD interfaces for Agencies and Certifications endpoints.
- **Certification Request lifecycle**: Delivered full Request workflow UI complete with state-preserving search and structured filters.
