using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SeHrCertificationPortal.Utilities;

namespace SeHrCertificationPortal.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly SeHrCertificationPortal.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SettingsModel> _logger;

        public SettingsModel(SeHrCertificationPortal.Data.ApplicationDbContext context, IWebHostEnvironment env, ILogger<SettingsModel> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public IList<SeHrCertificationPortal.Models.Agency> Agency { get; set; } = default!;
        public IList<SeHrCertificationPortal.Models.Certification> Certifications { get; set; } = default!;

        public IList<SeHrCertificationPortal.Models.Employee> Employees { get; set; } = default!;

        [BindProperty]
        public SeHrCertificationPortal.Models.Employee NewEmployee { get; set; } = new() { DisplayName = "" };

        [BindProperty]
        public int EmployeeId { get; set; }

        [BindProperty]
        public string? EmployeeName { get; set; }

        [BindProperty]
        public string? EmployeeIdNumberInput { get; set; }

        [BindProperty]
        public string? EmployeeRoleInput { get; set; }

        [BindProperty]
        public string? EmployeeDepartmentInput { get; set; }

        [BindProperty]
        public SeHrCertificationPortal.Models.Agency NewAgency { get; set; } = new() { Abbreviation = "", FullName = "" };

        [BindProperty]
        public SeHrCertificationPortal.Models.Certification NewCertification { get; set; } = new() { Name = "" };

        [BindProperty]
        public int AgencyId { get; set; }

        [BindProperty]
        public string? AgencyName { get; set; }

        [BindProperty]
        public string? AgencyAbbreviation { get; set; }

        [BindProperty]
        public int CertId { get; set; }

        [BindProperty]
        public string? CertName { get; set; }

        [BindProperty]
        public int CertAgencyId { get; set; }

        [BindProperty]
        public int CertValidity { get; set; }

        [BindProperty]
        public int ExpiringSoonThresholdDays { get; set; }

        [BindProperty]
        public string GlobalDepartments { get; set; } = "";

        [BindProperty]
        public string GlobalRoles { get; set; } = "";

        [BindProperty]
        public bool DisableAutocomplete { get; set; }

        [BindProperty(Name = "sortOrder", SupportsGet = true)]
        public string? CurrentSort { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try {
                var today = DateTime.UtcNow;

                // 1. Sort Agencies
                var agencyQuery = _context.Agencies.Include(a => a.Certifications).AsQueryable();
                agencyQuery = CurrentSort switch {
                    "agId_asc" => agencyQuery.OrderBy(a => a.Id),
                    "agId_desc" => agencyQuery.OrderByDescending(a => a.Id),
                    "agName_asc" => agencyQuery.OrderBy(a => a.FullName),
                    "agName_desc" => agencyQuery.OrderByDescending(a => a.FullName),
                    "agCerts_asc" => agencyQuery.OrderBy(a => a.Certifications.Count),
                    "agCerts_desc" => agencyQuery.OrderByDescending(a => a.Certifications.Count),
                    "agStatus_asc" => agencyQuery.OrderByDescending(a => a.IsActive), // Descending puts Active (True) first
                    "agStatus_desc" => agencyQuery.OrderBy(a => a.IsActive),
                    _ => agencyQuery.OrderBy(a => a.Abbreviation)
                };
                Agency = await agencyQuery.ToListAsync();

                // 2. Sort Certifications
                var certQuery = _context.Certifications.Include(c => c.Agency).AsQueryable();
                certQuery = CurrentSort switch {
                    "ctName_asc" => certQuery.OrderBy(c => c.Name),
                    "ctName_desc" => certQuery.OrderByDescending(c => c.Name),
                    "ctAgency_asc" => certQuery.OrderBy(c => c.Agency!.Abbreviation),
                    "ctAgency_desc" => certQuery.OrderByDescending(c => c.Agency!.Abbreviation),
                    "ctValidity_asc" => certQuery.OrderBy(c => c.ValidityPeriodMonths),
                    "ctValidity_desc" => certQuery.OrderByDescending(c => c.ValidityPeriodMonths),
                    "ctStatus_asc" => certQuery.OrderByDescending(c => c.IsActive),
                    "ctStatus_desc" => certQuery.OrderBy(c => c.IsActive),
                    _ => certQuery.OrderBy(c => c.Agency!.Abbreviation).ThenBy(c => c.Name)
                };
                Certifications = await certQuery.ToListAsync();

                // 3. Sort Employees
                var empQuery = _context.Employees.Include(e => e.CertificationRequests).AsQueryable();
                empQuery = CurrentSort switch {
                    "emId_asc" => empQuery.OrderBy(e => e.Id),
                    "emId_desc" => empQuery.OrderByDescending(e => e.Id),
                    "emName_asc" => empQuery.OrderBy(e => e.DisplayName),
                    "emName_desc" => empQuery.OrderByDescending(e => e.DisplayName),
                    "emDept_asc" => empQuery.OrderBy(e => e.Department),
                    "emDept_desc" => empQuery.OrderByDescending(e => e.Department),
                    "emCerts_asc" => empQuery.OrderBy(e => e.CertificationRequests.Count(cr => cr.Status == SeHrCertificationPortal.Models.RequestStatus.Passed && (cr.ExpirationDate == null || cr.ExpirationDate > today))),
                    "emCerts_desc" => empQuery.OrderByDescending(e => e.CertificationRequests.Count(cr => cr.Status == SeHrCertificationPortal.Models.RequestStatus.Passed && (cr.ExpirationDate == null || cr.ExpirationDate > today))),
                    "emStatus_asc" => empQuery.OrderByDescending(e => e.IsActive),
                    "emStatus_desc" => empQuery.OrderBy(e => e.IsActive),
                    _ => empQuery.OrderBy(e => e.DisplayName)
                };
                Employees = await empQuery.ToListAsync();

                var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
                ExpiringSoonThresholdDays = thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int days) ? days : 30;

                GlobalDepartments = (await _context.SystemSettings.FindAsync("Global_Departments"))?.Value ?? "Engineering, Human Resources, Field Operations";
                GlobalRoles = (await _context.SystemSettings.FindAsync("Global_Roles"))?.Value ?? "Technician, Manager, Inspector";
                DisableAutocomplete = (await _context.SystemSettings.FindAsync("Disable_Autocomplete"))?.Value == "true";
            } catch (Exception ex) {
                _logger.LogError(ex, "Error fetching data for page load.");
                TempData["ErrorMessage"] = "Unable to connect to the database to load records. The system may be experiencing an outage.";
                Agency = new List<SeHrCertificationPortal.Models.Agency>();
                Certifications = new List<SeHrCertificationPortal.Models.Certification>();
                Employees = new List<SeHrCertificationPortal.Models.Employee>();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddAgencyAsync()
        {
            if (string.IsNullOrWhiteSpace(NewAgency.Abbreviation)) return RedirectToPage();
            try {
                NewAgency.IsActive = true;
                _context.Agencies.Add(NewAgency);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Agency added successfully.";
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding agency.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddCertificationAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCertification.Name) || NewCertification.AgencyId == 0) return RedirectToPage();
            try {
                NewCertification.IsActive = true;
                _context.Certifications.Add(NewCertification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Certification added successfully.";
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveSettingsAsync()
        {
            try {
                async Task SaveSetting(string key, string value) {
                    var setting = await _context.SystemSettings.FindAsync(key);
                    if (setting == null) _context.SystemSettings.Add(new SeHrCertificationPortal.Models.SystemSetting { Key = key, Value = value });
                    else setting.Value = value;
                }

                await SaveSetting("ExpiringSoonThresholdDays", ExpiringSoonThresholdDays.ToString());
                await SaveSetting("Global_Departments", GlobalDepartments);
                await SaveSetting("Global_Roles", GlobalRoles);
                await SaveSetting("Disable_Autocomplete", DisableAutocomplete ? "true" : "false");

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Global system settings saved successfully.";
            } catch (Exception ex) {
                _logger.LogError(ex, "Error saving settings.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAgencyAsync()
        {
            try {
                var agency = await _context.Agencies.FindAsync(AgencyId);
                if (agency != null && !string.IsNullOrWhiteSpace(AgencyName) && !string.IsNullOrWhiteSpace(AgencyAbbreviation))
                {
                    agency.FullName = AgencyName;
                    agency.Abbreviation = AgencyAbbreviation;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Agency edited successfully.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error editing agency.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage(); // PRG Pattern (Reloads active tab via our JS localstorage)
        }

        public async Task<IActionResult> OnPostDeactivateAgencyAsync()
        {
            try {
                var agency = await _context.Agencies.FindAsync(AgencyId);
                if (agency != null)
                {
                    agency.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Agency deactivated.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deactivating agency.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditCertificationAsync()
        {
            try {
                var cert = await _context.Certifications.FindAsync(CertId);
                if (cert != null && !string.IsNullOrWhiteSpace(CertName))
                {
                    cert.Name = CertName;
                    cert.AgencyId = CertAgencyId;
                    cert.ValidityPeriodMonths = CertValidity;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification edited successfully.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error editing certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivateCertificationAsync()
        {
            try {
                var cert = await _context.Certifications.FindAsync(CertId);
                if (cert != null)
                {
                    cert.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification deactivated.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deactivating certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReactivateAgencyAsync()
        {
            try {
                var agency = await _context.Agencies.FindAsync(AgencyId);
                if (agency != null)
                {
                    agency.IsActive = true;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Agency reactivated.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error reactivating agency.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReactivateCertificationAsync()
        {
            try {
                var cert = await _context.Certifications.FindAsync(CertId);
                if (cert != null)
                {
                    cert.IsActive = true;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification reactivated.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error reactivating certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDownloadAgenciesCsvAsync()
        {
            try
            {
                var agencies = await _context.Agencies.OrderBy(a => a.Abbreviation).ToListAsync();
                var headers = new[] { "ID", "Name", "Abbreviation" };
                var csv = CsvExportHelper.GenerateCsv(agencies, headers, a => new[] { a.Id.ToString(), a.FullName ?? "", a.Abbreviation ?? "" });
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "Agencies_Export.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Agencies CSV.");
                TempData["ErrorMessage"] = "Failed to export CSV. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDownloadCertsCsvAsync()
        {
            try 
            {
                var certs = await _context.Certifications.Include(c => c.Agency).OrderBy(c => c.Name).ToListAsync();
                var headers = new[] { "ID", "Name", "Agency", "ValidityPeriodMonths" };
                var csv = CsvExportHelper.GenerateCsv(certs, headers, c => new[] { c.Id.ToString(), c.Name ?? "", c.Agency?.Abbreviation ?? "", c.ValidityPeriodMonths.ToString() });
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "Certifications_Export.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Certifications CSV.");
                TempData["ErrorMessage"] = "Failed to export CSV. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDownloadCatalogAsync(string exportScope = "Full")
        {
            var agencies = await _context.Agencies
                .Include(a => a.Certifications)
                .OrderBy(a => a.Abbreviation)
                .ToListAsync();

            var logoPath = Path.Combine(_env.WebRootPath, "img", "branding-assets", "Specialized-Engineering-Logo-white.webp");
            byte[]? logoBytes = System.IO.File.Exists(logoPath) ? await System.IO.File.ReadAllBytesAsync(logoPath) : null;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // Brand Header
                    page.Header().Background("#66615c").Padding(10).Row(row =>
                    {
                        if (logoBytes != null)
                        {
                            row.ConstantItem(150).Image(logoBytes);
                        }
                        string title = exportScope == "Agencies" ? "Agency Roster" : (exportScope == "Certifications" ? "Certification Roster" : "Agency & Certification Roster");
                        row.RelativeItem().AlignRight().AlignMiddle().Text(title).FontColor(Colors.White).FontSize(16).SemiBold();
                    });

                    // Content
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        if (exportScope == "Certifications")
                        {
                            var allCerts = agencies.SelectMany(a => a.Certifications.Select(c => new { Cert = c, Agency = a })).OrderBy(x => x.Cert.Name).ToList();

                            col.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Name
                                    columns.RelativeColumn(2); // Agency
                                    columns.RelativeColumn(1); // Validity
                                    columns.RelativeColumn(1); // Status
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Certification Name").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Agency").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Validity Period").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Status").SemiBold();
                                });

                                foreach (var item in allCerts)
                                {
                                    var rowTextColor = item.Cert.IsActive ? Colors.Black : Colors.Grey.Medium;
                                    var statusColor = item.Cert.IsActive ? Colors.Green.Darken1 : Colors.Red.Medium;

                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(item.Cert.Name).FontColor(rowTextColor);
                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(item.Agency.Abbreviation).FontColor(rowTextColor);
                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(item.Cert.ValidityPeriodMonths == 0 ? "Permanent" : $"{item.Cert.ValidityPeriodMonths} Months").FontColor(rowTextColor);
                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(item.Cert.IsActive ? "Active" : "Inactive").FontColor(statusColor).SemiBold();
                                }
                            });
                        }
                        else if (exportScope == "Agencies")
                        {
                            col.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Name
                                    columns.RelativeColumn(1); // Abbreviation
                                    columns.RelativeColumn(1); // Active Certs
                                    columns.RelativeColumn(1); // Status
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Agency Full Name").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Abbreviation").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Certifications").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Status").SemiBold();
                                });

                                foreach (var agency in agencies)
                                {
                                    var rowTextColor = agency.IsActive ? Colors.Black : Colors.Grey.Medium;
                                    var statusColor = agency.IsActive ? Colors.Green.Darken1 : Colors.Red.Medium;

                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(agency.FullName).FontColor(rowTextColor);
                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(agency.Abbreviation).FontColor(rowTextColor);
                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(agency.Certifications.Count.ToString()).FontColor(rowTextColor);
                                    table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(agency.IsActive ? "Active" : "Inactive").FontColor(statusColor).SemiBold();
                                }
                            });
                        }
                        else
                        {
                            foreach (var agency in agencies)
                            {
                                // 1. Dynamic Agency Header
                                var agencyColor = agency.IsActive ? "#a19482" : (string)Colors.Red.Medium;
                                var agencyStatusText = agency.IsActive ? "" : " [INACTIVE]";

                                col.Item().PaddingTop(10).PaddingBottom(5).Text($"{agency.FullName} ({agency.Abbreviation}){agencyStatusText}")
                                    .FontSize(14).SemiBold().FontColor(agencyColor);

                                if (!agency.Certifications.Any())
                                {
                                    col.Item().PaddingBottom(15).Text("No certifications currently linked.").Italic().FontColor(Colors.Grey.Medium);
                                    continue;
                                }

                                // 2. Certifications Table
                                col.Item().PaddingBottom(15).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Name
                                        columns.RelativeColumn(1); // Validity
                                        columns.RelativeColumn(1); // Status
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Certification Name").SemiBold();
                                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Validity Period").SemiBold();
                                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Status").SemiBold();
                                    });

                                    foreach (var cert in agency.Certifications)
                                    {
                                        // Dynamic Row Colors
                                        var rowTextColor = cert.IsActive ? Colors.Black : Colors.Grey.Medium;
                                        var statusColor = cert.IsActive ? Colors.Green.Darken1 : Colors.Red.Medium;

                                        table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.Name).FontColor(rowTextColor);
                                        table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.ValidityPeriodMonths == 0 ? "Permanent" : $"{cert.ValidityPeriodMonths} Months").FontColor(rowTextColor);
                                        table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.IsActive ? "Active" : "Inactive").FontColor(statusColor).SemiBold();
                                    }
                                });
                            }
                        }
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"SPE_Catalog_{exportScope}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> OnGetExportEmployeesAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.CertificationRequests)
                .ThenInclude(cr => cr.Certification)
                .OrderBy(e => e.DisplayName)
                .ToListAsync();

            var logoPath = Path.Combine(_env.WebRootPath, "img", "branding-assets", "Specialized-Engineering-Logo-white.webp");
            byte[]? logoBytes = null;
            if (System.IO.File.Exists(logoPath)) logoBytes = await System.IO.File.ReadAllBytesAsync(logoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Background("#66615c").Padding(10).Row(row =>
                    {
                        if (logoBytes != null) row.ConstantItem(150).Image(logoBytes);
                        row.RelativeItem().AlignRight().AlignMiddle().Text("Employee Roster & Certifications").FontColor(Colors.White).FontSize(16).SemiBold();
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Name
                                columns.RelativeColumn(1); // ID
                                columns.RelativeColumn(1.5f); // Role
                                columns.RelativeColumn(1.5f); // Dept
                                columns.RelativeColumn(1); // Active Certs
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Employee Name").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Emp ID").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Role").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Department").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Active Certs").SemiBold();
                            });

                            foreach (var emp in employees)
                            {
                                var rowTextColor = emp.IsActive ? Colors.Black : Colors.Grey.Medium;
                                var activeCertsCount = emp.CertificationRequests.Count(c => c.Status == SeHrCertificationPortal.Models.RequestStatus.Passed && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow));

                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(emp.DisplayName + (emp.IsActive ? "" : " [INACTIVE]")).FontColor(rowTextColor);
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(emp.EmployeeIdNumber ?? "-").FontColor(rowTextColor);
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(emp.Role ?? "-").FontColor(rowTextColor);
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(emp.Department ?? "-").FontColor(rowTextColor);
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(activeCertsCount.ToString()).FontColor(rowTextColor);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on " + DateTime.Now.ToString("g") + " | Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"SPE_Employee_Roster_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> OnPostDownloadEmployeesCsvAsync()
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.CertificationRequests)
                    .OrderBy(e => e.DisplayName)
                    .ToListAsync();
                var headers = new[] { "Employee Name", "Emp ID", "Role", "Department", "Active Certs" };
                var csv = CsvExportHelper.GenerateCsv(employees, headers, e => {
                    var activeCertsCount = e.CertificationRequests.Count(c => c.Status == SeHrCertificationPortal.Models.RequestStatus.Passed && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow));
                    return new[] { e.DisplayName ?? "", e.EmployeeIdNumber ?? "", e.Role ?? "", e.Department ?? "", activeCertsCount.ToString() };
                });
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"Employees_Export_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Employees CSV.");
                TempData["ErrorMessage"] = "Failed to export CSV. Please try again.";
                return RedirectToPage();
            }
        }
        public async Task<IActionResult> OnPostAddEmployeeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewEmployee.DisplayName)) return RedirectToPage();
            try {
                NewEmployee.IsActive = true;
                // The frontend will bind EmployeeIdNumber, Role, and Department directly to NewEmployee
                _context.Employees.Add(NewEmployee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee added successfully.";
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding employee.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync()
        {
            try {
                var emp = await _context.Employees.FindAsync(EmployeeId);
                if (emp != null && !string.IsNullOrWhiteSpace(EmployeeName))
                {
                    emp.DisplayName = EmployeeName;
                    emp.EmployeeIdNumber = EmployeeIdNumberInput;
                    emp.Role = EmployeeRoleInput;
                    emp.Department = EmployeeDepartmentInput;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee edited successfully.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error editing employee.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivateEmployeeAsync()
        {
            try {
                var emp = await _context.Employees.FindAsync(EmployeeId);
                if (emp != null)
                {
                    emp.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee deactivated.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deactivating employee.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReactivateEmployeeAsync()
        {
            try {
                var emp = await _context.Employees.FindAsync(EmployeeId);
                if (emp != null)
                {
                    emp.IsActive = true;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee reactivated.";
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error reactivating employee.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again or contact IT.";
            }
            return RedirectToPage();
        }
        private SeHrCertificationPortal.Models.TrackerStatus GetComputedStatus(DateTime? expDate)
        {
            if (!expDate.HasValue) return SeHrCertificationPortal.Models.TrackerStatus.Permanent;
            if (expDate.Value < DateTime.UtcNow) return SeHrCertificationPortal.Models.TrackerStatus.Expired;
            var thresholdSetting = _context.SystemSettings.Find("ExpiringSoonThresholdDays");
            int thresholdDays = thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int explicitThreshold) ? explicitThreshold : 30;
            if (expDate.Value <= DateTime.UtcNow.AddDays(thresholdDays)) return SeHrCertificationPortal.Models.TrackerStatus.ExpiringSoon;
            return SeHrCertificationPortal.Models.TrackerStatus.Active;
        }

        public async Task<IActionResult> OnGetEmployeeHistoryAsync(int employeeId)
        {
            var query = _context.CertificationRequests
                .Where(c => c.EmployeeId == employeeId && (c.Status == SeHrCertificationPortal.Models.RequestStatus.Passed || c.Status == SeHrCertificationPortal.Models.RequestStatus.Revoked))
                .Include(c => c.Agency)
                .Include(c => c.Certification)
                .AsNoTracking();

            var rawData = await query.ToListAsync();

            var history = rawData.Select(r => new
            { 
                agency = r.Agency != null ? r.Agency.Abbreviation : r.CustomAgencyName,
                certification = r.Certification != null ? r.Certification.Name : r.CustomCertificationName,
                datePassed = r.RequestDate.ToString("MMM dd, yyyy"),
                expirationDate = r.ExpirationDate.HasValue ? r.ExpirationDate.Value.ToString("MMM dd, yyyy") : "Permanent",
                status = GetComputedStatus(r.ExpirationDate).ToString()
            }).ToList();

            return new JsonResult(history);
        }

        public async Task<IActionResult> OnPostDownloadEmployeeHistoryPdfAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null) return NotFound();

                var userCerts = await _context.CertificationRequests
                    .Include(c => c.Agency)
                    .Include(c => c.Certification)
                    .Where(c => c.EmployeeId == employeeId && (c.Status == SeHrCertificationPortal.Models.RequestStatus.Passed || c.Status == SeHrCertificationPortal.Models.RequestStatus.Revoked || c.Status == SeHrCertificationPortal.Models.RequestStatus.Archived))
                    .OrderByDescending(c => c.RequestDate)
                    .AsNoTracking()
                    .ToListAsync();

                var logoPath = Path.Combine(_env.WebRootPath, "img", "branding-assets", "Specialized-Engineering-Logo-white.webp");
                byte[]? logoBytes = System.IO.File.Exists(logoPath) ? await System.IO.File.ReadAllBytesAsync(logoPath) : null;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                        page.Header().Background("#66615c").Padding(10).Row(row =>
                        {
                            if (logoBytes != null) row.ConstantItem(150).Image(logoBytes);
                            row.RelativeItem().AlignRight().AlignMiddle().Text("Employee Certification History").FontColor(Colors.White).FontSize(16).SemiBold();
                        });

                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Spacing(20);

                            col.Item().Text($"Employee: {employee.DisplayName}").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken3);

                            if (!userCerts.Any())
                            {
                                col.Item().Text("No certification history found for this employee.").Italic().FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); columns.RelativeColumn(2); columns.RelativeColumn(2); columns.RelativeColumn(2); columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Certification").SemiBold();
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Agency").SemiBold();
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Date Passed").SemiBold();
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Expiration Date").SemiBold();
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold();
                                    });

                                    foreach (var cert in userCerts)
                                    {
                                        var certName = cert.Certification?.Name ?? cert.CustomCertificationName ?? "Custom";
                                        var agencyName = cert.Agency?.Abbreviation ?? cert.CustomAgencyName ?? "Custom";
                                        var passedDate = cert.RequestDate.ToString("MMM dd, yyyy");
                                        var expDate = cert.ExpirationDate.HasValue ? cert.ExpirationDate.Value.ToString("MMM dd, yyyy") : "None";
                                        var statusText = cert.Status == SeHrCertificationPortal.Models.RequestStatus.Passed && cert.ExpirationDate.HasValue && cert.ExpirationDate.Value < DateTime.UtcNow ? "Expired" : cert.Status.ToString();

                                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(certName);
                                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(agencyName);
                                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(passedDate);
                                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(expDate);
                                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(statusText);
                                    }
                                });
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span($"Generated {DateTime.UtcNow:g} | Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"History_{employee.DisplayName.Replace(" ", "_")}.pdf\"");
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee history PDF.");
                TempData["ErrorMessage"] = "Failed to generate history report.";
                return RedirectToPage();
            }
        }
    }
}
