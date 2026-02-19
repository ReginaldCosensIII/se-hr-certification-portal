# Project Requirements & Design Specifications

## 1. Brand Identity & Design System
### Color Palette
- **Primary Backgrounds/Accents**:
    - Dark Gray: `#66615c` (Sidebar/Backgrounds)
    - Tan (Main Accent): `#a19482`
    - Sage Green: `#a1aba0`
- **Theme**: "Dull Industrial Concrete"
- **Highlights/Borders/Icons**: Fluorescent bright colors (Safety Orange, High-Vis Yellow) - mimicking construction sites.

### Layout Customizations
- **Sidebar**:
    - **Expanded**:
        - Header: Taller to fit `Specialized-Engineering-Logo-white.webp`.
        - Content: Navigation links.
        - Footer: Toggle arrow icon (moved from top).
    - **Collapsed**:
        - Header: Custom SVG Logo (White Rectangle + Stacked "S" "E").
        - Content: Icons only.
- **Header (Top Bar)**:
    - Page Title (e.g., "Dashboard" or "HR Certification Portal").
    - Search Bar (Right aligned).
    - Notification Bell & Dropdown.

## 2. Core Functional Requirements
### Dashboard
- **Header**: Standard App Header.
- **Top Cards**: Counts (Requests, Pending, Expiring, Active).
    - **Action**: "More Info" buttons must link to `Requests/Index` with *specific filters* applied (e.g., Status='Pending').
- **Recent Certifications Table**:
    - Columns: Employee, Agency, Certification, Requests Type, Rec Date, Exp Date, Status.
    - Location: Below top cards.

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
- **Top Section**: Domain Management (Cards/Tables).
    - **Agency Management**: List Agencies. Actions: Add New, Edit Name, Toggle Active status.
    - **Certification Management**: List Certifications. Actions: Add New, Edit Name, Assign Agency, Set Validity Period (Years).
- **Bottom Section**: General Configuration.
    - **Admin Email**: Field to set the contact for system notifications (e.g., `hr@speceng.com`).
    - **Application Name**: *Removed* (Hardcoded to brand).
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

### Employee Detail & Certification History View (Master-Detail)
- **Trigger**: User clicks a row in the "Employee Certifications" table.
- **Location**: Top of page (Collapsible/Accordion style), consistent with Requests page.
- **Content**:
    1.  **Employee Profile**: Name, ID, Manager, Role.
    2.  **Selected Certification**: Details of the specific cert clicked (Agency, Issue Date, Expiration, PDF Copy link).
    3.  **Certification History**: List of *other* certifications held by this employee.
    4.  **Interaction**: Clicking a cert in the history list updates the "Selected Certification" view.
- **Status**: UI Scaffolding only (Backend wiring in Phase 5).

## 3. Global Reporting & Export Features (Future)
- **Data Tables (Admin, Requests, Certifications)**: 
    - All data tables must support **Export to CSV/Excel** and **Print View**.
    - **Admin Page**: Specifically for "Agencies" and "Certifications" lists.
- **Certification Request Form**:
    - **Digital Export**: Ability to download a filled request as PDF.
    - **Blank Form**: Feature to generate/print a **Blank Certification Request PDF** that matches the physical paper form (based on provided screenshots) for manual filling.
    - *Note*: This requires a dedicated PDF generation service or template.

## 4. Technical & Workflow Requirements
- **Architecture Strategy**: **Frontend-First**.
    1.  **UI Scaffolding**: Build all Razor Pages and visuals first.
    2.  **Database**: Design schema and seed data *after* UI approval.
    3.  **Wiring**: Connect UI to DB and implement backend logic last.
- **Database**: PostgreSQL with Entity Framework Core.
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
