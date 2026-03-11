# Specialized Engineering - HR Certification Portal User Guide

Welcome to the user manual for the Specialized Engineering HR Certification & Request Portal. This guide provides step-by-step instructions for utilizing every major functional area of the application.

---

## 🛠️ Phase 1: Infrastructure & Portal Access

**Accessing the Portal**
The HR Certification Portal is deployed as an internal IIS web application designed for desktop or Laptop use.
- **Production URL:** `http://10.1.0.47:8080` (Please check with your IT administrator for the specific server hostname or IP address).
- **Authentication:** This is an internal tool on the company intranet. It does not require a traditional username/password login screen; instead, access is restricted to the internal network.

**Browser Compatibility**
The User Interface is optimized for desktop and laptop displays (Google Chrome, Microsoft Edge). 

*Please note that the portal is not optimized for use on mobile devices but most tablets should work with the left hand sidebar collapsed.*

![Screenshot of the initial landing page/dashboard view](images/landing-dashboard-view.png)

---

## 📊 Phase 2: The Executive Dashboard

The Executive Dashboard provides high-level KPI monitoring and visibility into the workforce's health regarding certifications.

**KPI Cards Explained**
- **Total Requests:** The aggregate count of all historical certification request entries submitted.
- **Active Certifications:** Only records marked as "Passed" that currently have a valid, future expiration date.
- **Pending Approvals:** Records that have been submitted and are awaiting HR review.
- **Expiring Soon:** The count of active certifications that will expire within the configured threshold (default is 30 days).

**Critical Action Items Table**
Below the KPIs, the **Recent Requests** table highlights the latest activities. This table allows you to quickly spot requests requiring immediate attention (e.g., Pending Approvals). You can sort this table by clicking the column headers.

![Screenshot of the full dashboard view with populated data](images/dashboard-kpis.png)

---

## 📝 Phase 3: Request Lifecycle Management

This section covers managing a certification request's transition from "Requested" to "Passed Certification."

**1. Creating a New Request (Validation & Cascading Logic)**
- Navigate to the **Requests** page.
- Click **New Request** to expand the form. 
- *Dual-Layer Validation:* The system ensures all required fields are filled. Attempting to submit an empty form will trigger a validation error notification.
- *Cascading Options:* Select an Agency from the "Certification Agency" dropdown. The "Certification Desired" dropdown will immediately update to only show certifications offered by that specific agency.

**2. Request Actions & Workflows**
- **Approving:** Find a request in the "Pending" state and click the checkmark icon (`Approve Request`) to change it to "Approved."
- **Marking Passed:** Once a request is Approved, a ribbon icon (`Mark as Passed`) becomes available. Clicking it opens the "Mark Passed" modal. 
  - *Auto-Calculation:* Simply enter the Date Passed. The system references the certification's configured "Validity Period" and automatically calculates the exact "Calculated Expiration Date."
- **Editing:** You can update manager names, request types, or correct typos by clicking the blue pencil icon (`Edit Request`) on any record.

**3. Data Search & Filtering**
- **Search Bar:** Located in the top right of the Requests table. You can search by **Employee Name**, **Manager Name**, or the exact **REQ ID**.
- **Status Filter:** Use the dropdown next to the search bar to filter requests by their current lifecycle state (Pending, Approved, Passed, Failed, Rejected).
- **Clearing Filters:** Once a search or filter is applied, an `X` icon will appear next to the filter button. Click it to clear all active filters and return to the default view.

**4. Data Export**
- **Exporting to CSV:** Use the **Export CSV** button in the top right corner of the Requests card to generate a spreadsheet artifact matching your currently filtered view. This is useful for data analysis in Excel.

![Screenshot of the Validation Errors on New Request Form](images/validation-errors.png)
![Screenshot of the "Mark Passed" calculation modal](images/mark-passed-modal.png)
![Screenshot of the Search, Filter, and Export buttons on the Requests page](images/search-filter-export-buttons.png)

---

## 🗂️ Global Feature: Table Sorting & Navigation

Across all major data tables in the portal (Dashboard, Requests, Certifications, Admin Grid), you have powerful sorting and navigation controls:

**Column Sorting Logic**
Most column headers are clickable and feature a three-state sorting cycle:
1. **First Click:** Sorts the data **Ascending** (A-Z, or oldest to newest). An up-arrow icon will appear.
2. **Second Click:** Sorts the data **Descending** (Z-A, or newest to oldest). A down-arrow icon will appear.
3. **Third Click (Reset):** Clears the custom sort and returns the table to its **Default Sort State** (e.g., restoring the default "Request Date - Descending" view). The sort icon will revert to the default unsorted state.

**Pagination & Page Sizing**
At the bottom of every major table, you will find navigation controls:
- Use the **"Show [ X ] entries"** dropdown to change how many records are displayed per page (e.g., 10, 25, 50, 100).
- Use the pagination numbers to jump between pages of results.
- *Note: If you enable "Remember Table States" in your personal System Settings, the application will remember your page size and current page number even if you navigate away.*

---

## 📚 Phase 4: Relational Dictionary (Agencies & Certs)

The Relational Dictionary maintains the data engine that drives the dropdown lists throughout the application.

**1. Agency Management**
- Navigate to the **Admin Management** section and select the **Agencies** tab.
- Here you can manage the list of governing bodies (e.g., ACI, MARTCP). 
- To add a new regulating authority, click **Add Agency** and provide the abbreviation and full name.
- **Search:** Use the inline search bar to quickly find an agency by name or abbreviation.
- **Export:** Click "Export Options" to download the Agency list as a raw **CSV** or as a formatted **PDF Catalog**.

**2. Certification Mapping**
- Switch to the **Certifications** tab.
- Click **Add Certification**.
- Link the specific certification to its issuing Agency and set its standard validity period in months (e.g., 36 months for a 3-year cert). Enter `0` for permanent/lifetime certifications.
- **Search & Filter:** You can search certifications by name, or use the dropdown to filter the table to only show certifications from a specific Agency.
- **Export:** Click "Export Options" to download the Certification list as a **CSV** or export a full structured **PDF Catalog**.

![Screenshot of the Agency Master List](images/agency-master-list.png)
![Screenshot of the "Add Certification" form linking an agency with a validity period](images/add-certification-form.png)

---

## ⚙️ Phase 5: System Administration

The System Administration view manages global configuration options, employee records, and administrative corrections. New in **V1.1.1**, administrators can now reverse actions and configure dynamic dropdown lists directly from this view.

**1. Threshold Settings**
- Navigate to **Admin Management** and look under **Global System Configuration**.
- You can change the "Expiring Soon Threshold (Days)" (default is 30 days). Updating this instantly adjusts which certifications are flagged on the dashboard and email alerts.

**2. Employee Registry & Document Generation**
- Switch to the **Employees** tab in Admin Management.
- Manage the silent auto-generation of employees here. If duplicate or inactive employees appear from Active Directory imports, use the Deactivate (trash bin) tool to perform manual cleanup and hide them from selection menus.
- **Search:** Quickly search for a specific employee by their display name.
- **Print History (PDF):** Click the printer icon next to any employee to generate a beautiful, formatted **PDF Document** of their entire certification history.
- **Bulk Export:** Use the "Export Options" dropdown to export the entire employee registry as a **CSV** spreadsheet or a comprehensive **PDF Report**.

---

## 🔄 Phase 6: Administrative Action Reversal *(V1.1.1)*

Mistakes happen. The portal now supports full administrative correction of any certification request's status and expiration date without requiring a new submission.

**When to Use This Feature**
- A request was accidentally marked **Rejected** but should be **Pending** again.
- A cert was marked **Passed** with an incorrect expiration date that needs to be corrected.
- A **Passed** cert needs to be reverted to **Approved** for re-review.

**How to Correct a Request**
1. Navigate to the **Requests** page.
2. Locate the request you need to correct (use Search or Status Filter to find it quickly).
3. Click the **blue pencil icon** (Edit) in the Actions column for that record.
4. The **Edit Request** modal will open. In addition to the standard fields, you will now see:
   - **Status** — A dropdown containing all possible states: `Pending`, `Approved`, `Rejected`, `Passed`, `Failed`, `Revoked`, `Archived`. Select the correct state.
   - **Expiration Date** — A date picker pre-filled with the current expiration date (blank if the cert is Permanent). Clear or change this date as needed.
5. Click **Save Changes**. The record will immediately reflect the corrected status and date in the table.

> **Note:** Changing a status back to `Passed` does not automatically recalculate the expiration date. You must set the correct expiration date manually in the same edit operation.

![Screenshot of the Edit modal showing the Status dropdown and Expiration Date field](images/edit-modal-action-reversal.png)

---

## 🌐 Phase 7: Global System Configuration *(V1.1.1)*

The **Global System Configuration** panel, located below the Admin Management tabs, controls system-wide settings that apply to all users instantly.

**Expiring Soon Threshold**
Sets the number of days before a certification expiry that it is flagged as "Expiring Soon" on the dashboard and in analytics reports.

**Dynamic Combo-Box Lists**
The portal supports live-updating suggestion lists for the **Department** and **Job Role** fields when adding or editing employees. These lists are managed centrally and require no developer involvement to update.

- **Suggested Departments** — Enter department names separated by commas (e.g., `Engineering, Field Operations, Human Resources`). These appear as autocomplete suggestions in the Department field throughout the application.
- **Suggested Job Roles** — Enter role titles separated by commas (e.g., `Technician, Inspector, Manager`). These appear as autocomplete suggestions in the Role field.

**Saving an Empty List**
If you intentionally clear a combo-box list field and click **Save Global Settings**, the system will now correctly persist an empty list. The dropdown on the Employee portal (Port 8081) will render blank, showing no suggestions. This is the expected behavior and is not an error.

> **Previous Behavior (Fixed in V1.1):** In earlier versions, clearing a list would cause the page to revert to a hardcoded default on refresh. This has been resolved — the system now faithfully persists whatever you save, including an intentionally empty field.

**Disable Browser Autocomplete**
Toggle this switch to suppress native browser autofill across all form inputs in the portal — useful in shared workstation environments.

**Saving Settings**
Click **Save Global Settings** after making any changes. A green confirmation banner will appear at the top of the page confirming the save was successful.

![Screenshot of the Global System Configuration card with Departments and Roles fields](images/global-system-config.png)

---

## ❓ Phase 8: Integrated Help *(V1.1.1)*

A **Help** button is now available directly in the portal UI so you never need to leave the application to find guidance.

**Location**
- Navigate to **Admin Management** and scroll down to the **Global System Configuration** card.
- In the top-right corner of the card header, you will see a small **Help** button with a `help-circle` icon.

**How It Works**
- Clicking the **Help** button opens this User Guide (as a PDF) **in a new browser tab**.
- The guide is served directly from the application server — no internet connection is required.
- The PDF is always the latest version approved for the current release.

![Screenshot of the Help button in the Global System Configuration card header](images/admin-help-button.png)

---

![Screenshot of the System Settings/Admin page showing threshold configuration](images/system-settings-admin.png)
