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

### Admin Settings
- **Agency Management**: Add/Edit/Deactivate Agencies.
- **Certification Management**: Add/Edit/Deactivate Certifications (Linked to Agencies).
- **Data Seeding**: Pre-load data from `SpecializedEngineering-HR-Portal-Drop-Down-Lists.pdf`.

### Certifications Catalog (Global List)
- View all certifications and which employees hold them.
- Export options for reports.

## 3. Technical & Workflow Requirements
- **Database**: PostgreSQL with Entity Framework Core.
- **Testing**: 100% Unit Test coverage for logic. Regression testing required.
- **Deployment**: Local IIS on SpecEng network. No Authentication required (Internal only).
- **Workflow**:
    - Feature Branches (`feat/name`).
    - Standardized Agent Skills for tasks.
    - Database Seeding Strategy (PgDump for prod).

## 4. Assets
- **Logo**: `wwwroot/img/branding-assets/Specialized-Engineering-Logo-white.webp`
- **Dropdown Data**: `docs/SpecializedEngineering-HR-Portal-Drop-Down-Lists.pdf`
