# Project Requirements & Design Specifications

## 1. Brand Identity & Design System
### Color Palette
- **Primary Backgrounds/Accents**:
    - Light Gray: `#f4f6f9` (Main Body Background)
    - Black: `#000000` (Sidebar/Header accents)
    - Tan (Main Accent/Primary Brand): `#a19482`
    - Dark Tan (Secondary Brand): `#66615c`
    - Yellow-400 (Active/Warning states): `#fbbf24`
- **Theme**: "Clean Light Industrial"
- **Highlights/Borders/Icons**: Clean layout with standard Bootstrap 5 secondary warning (`#fbbf24`), danger (`#dc3545`), and success (`#198754`) semantic colors mimicking industrial safety statuses.

### Layout Customizations
- **Sidebar (Width: 260px Expanded, 80px Collapsed)**:
    - **Expanded**:
        - Header: Contains `Specialized-Engineering-Logo-white.webp`.
        - Content: Navigation links.
        - Floating Toggle: Right-aligned toggle arrow overlaps the sidebar and main content boundary.
    - **Collapsed**:
        - Header: Custom SVG Logo (White Rectangle + Stacked "S" "E").
        - Content: Icons only, natively transitioned using `grid-template-columns`.
- **Header (Top Bar)**:
    - Page Title (e.g., "Dashboard", "Admin").
    - Notification Bell & Fullscreen toggles.

## 2. Core Functional Requirements
### Dashboard
- **Header**: Standard App Header.
- **Top Cards**: Counts (Total Requests, Active Certifications, Pending Approvals, Expiring Soon).
    - **Action**: Direct links mapping to respective views (e.g., `Requests/Index?status=Pending`). Cards utilize semantic state styling (warning logic) if > 0.
- **Recent Requests Table**:
    - Columns: Req ID, Employee, Manager, Agency, Certification, Type, Status, Request Date, Actions.
    - Features: Includes inline form input for text search and a direct "New Request" button in the table header.

### Certification Requests
- **Form**:
    - Fields: Employee Name, Request Date, Manager, Certification Agency, Requested Certification, Request Type (Review, Written, Practical, Reciprocity, Recertification).
    - **Dynamic Dropdowns**: Agency selection filters Certification list.
    - **"Other" Support**: Allow custom entry if Agency/Cert not listed.
- **List View**:
    - Filtering: Status, Agency, Employee.
    - Search: Global search.
    - Actions: View, Edit, Remove.

### Admin Settings (System Management)
- **Top Section**: Domain Management (Cards/Data Grids).
    - **Agency Management**: List Agencies using ClientSideDataGrid implementation. Actions: Quick search, toggle Soft Delete `IsActive` state. Active states are designated visually with Lucide icons.
    - **Certification Management**: List Certifications with relationships to Agencies. Quick search, toggle Soft Delete `IsActive` state.
    - **Export list**: QuestPDF export engine that explicitly highlights in-active elements natively via document structure.
- **Bottom Section**: General Configuration.
    - **Admin Email**: Field to set the contact for system notifications (e.g., `hr@speceng.com`).
- **Future/Suggested Features**:
    - **Audit Log**: View history of changes.
    - **Data Seeding**: Button to "Reset to Default" for testing.
    - **Notification Templates**: Edit email subject/body text.

### Certifications Catalog (Employee Certifications)
- **Table Name**: "Employee Certifications"
- **Columns**: Employee Name, Agency, Certification, Status, Expiration Date.
- **Filters**:
    - Global Search Bar.
    - Status Dropdown (Active, Expiring, Expired).
    - Agency Dropdown (ACI, WACEL, VDEQ, etc.).
- **Export**: "Generate Report" button (Action: Download CSV/PDF of active/expiring certs).
- **Styling**: Must match Dashboard/Requests table styles exactly.

### Compliance & Coverage Analytics Dashboard
- **Compliance & Coverage Analytics Dashboard:** A dynamic, visual analytics section utilizing Chart.js to display Total Active, Expiring Soon, and Critical Lapses.
  - *Data Sync:* Analytics must perfectly respect client-side table filters via a unified toggle state.
  - *WYSIWYG Export:* Includes a "Download Report" feature powered by QuestPDF that captures client-side visual states and generates a master Coverage-First layout.

## 3. Global Reporting & Export Features (Future)
- **Data Tables (Admin, Requests, Certifications)**: 
    - All data tables must support **Export to CSV/Excel** and **Print View**.
    - **Admin Page**: Specifically for "Agencies" and "Certifications" lists.
- **Certification Request Form**:
    - **Digital Export**: Ability to download a filled request as PDF.
    - **Blank Form**: Feature to generate/print a **Blank Certification Request PDF** that matches the physical paper form (based on provided screenshots) for manual filling.
    - *Note*: This requires a dedicated PDF generation service or template.

## 4. Technical & Workflow Requirements
- **Tech Stack**: ASP.NET Core 10 (LTS) with Razor Pages.
- **Architecture Strategy**: **Frontend-First**.
    1.  **UI Scaffolding**: Build all Razor Pages and visuals first.
    2.  **Database**: Design schema and seed data *after* UI approval.
    3.  **Wiring**: Connect UI to DB and implement backend logic last.
- **Database**: PostgreSQL with Entity Framework Core 10.
    - **Migration**: Use `pg_dump` for production schema promotion.
    - **Seeding**: Realistic mock data required for dev/testing.
- **Testing**: 100% Unit Test coverage for logic. Regression testing required.
- **Deployment**: Local IIS on SpecEng network. No Authentication required (Internal only).
- **Workflow**:
    - Feature Branches (`feat/name`).
    - Standardized Agent Skills for tasks.

## 4. Specific View Requirements
### Dashboard Table ("Recent Requests")
- **Columns**: Employee ID, Employee Name, Agency, Certification, Request Type, Status, Date.
- **Actions**: New Request (Header), Edit/Delete (Row).
- **Restrictions**: No "View All" or "Download" buttons on Dashboard (keep it simple).

## 4. Assets
- **Logo**: `wwwroot/img/branding-assets/Specialized-Engineering-Logo-white.webp`
- **Dropdown Data**: `docs/SpecializedEngineering-HR-Portal-Drop-Down-Lists.pdf`

## 5. Architectural Implementations
- **Server-Side Sorting**: A global, reusable table sorting architecture using DOM Hydration and EF Core `IQueryable` routing to handle large datasets accurately.
- **Dynamic Configuration Engine**: A database-driven settings UI allowing HR Admins to manage "Suggested Departments" and "Job Roles" as dynamic HTML5 `<datalist>` combo-boxes without requiring database migrations.
- **Contextual Export Pipelines**: An in-memory CSV generation utility that dynamically evaluates active UI filters (search, agency, status) and streams precise datasets directly to the client.
- **SRE Text-Based Fallback**: Pure Vanilla JS and native HTML implementations are prioritized over heavy third-party libraries (e.g., DataTables) to ensure absolute functionality in offline, air-gapped environments.
- **Viewport Preservation**: Employs a custom Cookie Bridge and native anchor targeting to persist user preferences (rows per page) and table scroll positions across Server-Side Rendered (SSR) page reloads.
