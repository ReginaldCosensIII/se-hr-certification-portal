using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Data;
using SeHrCertificationPortal.Models;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace SeHrCertificationPortal.Pages.Certifications
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment env, ILogger<IndexModel> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 25;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool FilterAnalytics { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public int? AgencyFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public TrackerStatus? StatusFilter { get; set; }

        [BindProperty]
        public int TargetCertId { get; set; }

        [BindProperty]
        public DateTime EditDatePassed { get; set; }

        [BindProperty]
        public DateTime? EditExpiration { get; set; }

        public IList<CertificationRequest> PassedCertifications { get; set; } = default!;
        public SelectList AgencyOptions { get; set; } = default!;
        public SelectList EmployeeOptions { get; set; } = default!;
        public IList<Certification> AllCertifications { get; set; } = default!;

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int ThresholdDays { get; set; } = 30;

        public int AnalyticsTotalActive { get; set; }
        public List<string> ExpiredDetails { get; set; } = new();
        public List<string> ExpiringSoonDetails { get; set; } = new();
        public int AnalyticsExpiringSoon { get; set; }
        public int AnalyticsExpired { get; set; }
        public string AgencyChartDataJson { get; set; } = "[]";
        public string AgencyChartLabelsJson { get; set; } = "[]";
        public Dictionary<string, int> TopAgenciesList { get; set; } = new Dictionary<string, int>();

        public string CertChartDataJson { get; set; } = "[]";
        public string CertChartLabelsJson { get; set; } = "[]";
        public Dictionary<string, int> TopCertsList { get; set; } = new Dictionary<string, int>();
        public string AgencyChartLabels { get; set; } = "[]";
        public string AgencyChartData { get; set; } = "[]";
        public string CertChartLabels { get; set; } = "[]";
        public string CertChartData { get; set; } = "[]";

        public TrackerStatus GetComputedStatus(DateTime? expDate)
        {
            if (expDate == null) return TrackerStatus.Permanent;
            if (expDate.Value < DateTime.UtcNow) return TrackerStatus.Expired;
            if (expDate.Value <= DateTime.UtcNow.AddDays(ThresholdDays)) return TrackerStatus.ExpiringSoon;
            return TrackerStatus.Active;
        }

        public async Task<IActionResult> OnGetAsync(int p = 1)
        {
            try {
                CurrentPage = p < 1 ? 1 : p;

                var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
                if (thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int explicitThreshold))
                {
                    ThresholdDays = explicitThreshold;
                }

                var agencies = await _context.Agencies.Where(a => a.IsActive).ToListAsync();
                AgencyOptions = new SelectList(agencies, "Id", "Abbreviation");

                var employees = await _context.Employees.OrderBy(e => e.DisplayName).ToListAsync();
                EmployeeOptions = new SelectList(employees, "Id", "DisplayName");

                AllCertifications = await _context.Certifications.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

                // Npgsql Date Standard: Declare local variables for the query
                DateTime today = DateTime.UtcNow;
                DateTime thresholdDate = today.AddDays(ThresholdDays);

                var query = _context.CertificationRequests
                    .Include(c => c.Employee)
                    .Include(c => c.Agency)
                    .Include(c => c.Certification)
                    .Where(c => c.Status == RequestStatus.Passed || c.Status == RequestStatus.Revoked);

                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    var searchLower = SearchString.ToLower();
                    query = query.Where(c => c.Employee != null && c.Employee.DisplayName.ToLower().Contains(searchLower));
                }

                if (AgencyFilter.HasValue && AgencyFilter.Value > 0)
                {
                    query = query.Where(c => c.AgencyId == AgencyFilter.Value);
                }

                if (StatusFilter.HasValue)
                {
                    switch (StatusFilter.Value)
                    {
                        case TrackerStatus.Expired:
                            query = query.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value < today);
                            break;
                        case TrackerStatus.ExpiringSoon:
                            query = query.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value >= today && c.ExpirationDate.Value <= thresholdDate);
                            break;
                        case TrackerStatus.Active:
                            query = query.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value > thresholdDate);
                            break;
                        case TrackerStatus.Permanent:
                            query = query.Where(c => c.ExpirationDate == null);
                            break;
                    }
                }

                // --- ANALYTICS ENGINE ---
                // If true, use 'query' (URL filters applied). If false, use global DB dataset.
                var analyticsBaseQuery = FilterAnalytics
                    ? query
                    : _context.CertificationRequests
                        .Include(c => c.Agency)
                        .Include(c => c.Certification)
                        .Where(c => c.Status == RequestStatus.Passed || c.Status == RequestStatus.Revoked);

                var analyticsRaw = await analyticsBaseQuery.Select(c => new
                {
                    EmployeeName = c.Employee != null ? c.Employee.DisplayName : "Unknown",
                    AgencyName = c.Agency != null ? c.Agency.Abbreviation : (c.CustomAgencyName ?? "Unknown"),
                    CertName = c.Certification != null ? c.Certification.Name : (c.CustomCertificationName ?? "Unknown"),
                    ExpDate = c.ExpirationDate
                }).ToListAsync();

                AnalyticsExpired = analyticsRaw.Count(c => c.ExpDate.HasValue && c.ExpDate.Value < today);
                AnalyticsExpiringSoon = analyticsRaw.Count(c => c.ExpDate.HasValue && c.ExpDate.Value >= today && c.ExpDate.Value <= thresholdDate);
                AnalyticsTotalActive = analyticsRaw.Count(c => !c.ExpDate.HasValue || c.ExpDate.Value > thresholdDate);

                ExpiredDetails = analyticsRaw.Where(c => c.ExpDate.HasValue && c.ExpDate.Value < today)
                    .Select(c => $"{c.EmployeeName} ({c.CertName})").ToList();
                ExpiringSoonDetails = analyticsRaw.Where(c => c.ExpDate.HasValue && c.ExpDate.Value >= today && c.ExpDate.Value <= thresholdDate)
                    .Select(c => $"{c.EmployeeName} ({c.CertName})").ToList();

                var topAgencies = analyticsRaw.GroupBy(c => c.AgencyName).OrderByDescending(g => g.Count()).Take(5).ToList();
                AgencyChartLabels = System.Text.Json.JsonSerializer.Serialize(topAgencies.Select(g => g.Key));
                AgencyChartData = System.Text.Json.JsonSerializer.Serialize(topAgencies.Select(g => g.Count()));

                var topCerts = analyticsRaw
                    .GroupBy(c => c.CertName)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count());

                AgencyChartDataJson = JsonSerializer.Serialize(topAgencies.Select(g => g.Count()));
                AgencyChartLabelsJson = JsonSerializer.Serialize(topAgencies.Select(g => g.Key));
                TopAgenciesList = topAgencies.ToDictionary(g => g.Key, g => g.Count());

                CertChartDataJson = JsonSerializer.Serialize(topCerts.Values);
                CertChartLabelsJson = JsonSerializer.Serialize(topCerts.Keys);
                TopCertsList = topCerts;

                CertChartData = CertChartDataJson;
                CertChartLabels = CertChartLabelsJson;
                // ------------------------

                TotalRecords = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

                PassedCertifications = await query
                    .OrderByDescending(c => c.ExpirationDate)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .AsNoTracking()
                    .ToListAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error fetching data for page load.");
                TempData["ErrorMessage"] = "Unable to connect to the database to load records. The system may be experiencing an outage.";
                
                PassedCertifications = new List<CertificationRequest>();
                AllCertifications = new List<Certification>();
                AgencyOptions = new SelectList(new List<Agency>(), "Id", "Abbreviation");
                EmployeeOptions = new SelectList(new List<Employee>(), "Id", "DisplayName");
            }
            return Page();
        }

        public async Task<IActionResult> OnGetEmployeeHistoryAsync(int employeeId)
        {
            var query = _context.CertificationRequests
                .Where(c => c.EmployeeId == employeeId && (c.Status == RequestStatus.Passed || c.Status == RequestStatus.Revoked))
                .Include(c => c.Agency)
                .Include(c => c.Certification)
                .AsNoTracking();

            var rawData = await query.ToListAsync(); // Materialize to memory first

            var history = rawData.Select(r => new
            { // Apply C# formatting safely
                agency = r.Agency != null ? r.Agency.Abbreviation : r.CustomAgencyName,
                certification = r.Certification != null ? r.Certification.Name : r.CustomCertificationName,
                datePassed = r.RequestDate.ToString("MMM dd, yyyy"),
                expirationDate = r.ExpirationDate.HasValue ? r.ExpirationDate.Value.ToString("MMM dd, yyyy") : "Permanent",
                status = GetComputedStatus(r.ExpirationDate).ToString()
            }).ToList();

            return new JsonResult(history);
        }

        public async Task<IActionResult> OnPostEditCertAsync(int p = 1, int pageSize = 25, string? searchString = null, int? agencyFilter = null, string? statusFilter = null, bool filterAnalytics = true)
        {
            try
            {
                var record = await _context.CertificationRequests.FindAsync(TargetCertId);
                if (record != null)
                {
                    record.RequestDate = EditDatePassed;
                    record.ExpirationDate = EditExpiration;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification edited successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }
            return RedirectToPage(new { p, pageSize, SearchString = searchString, AgencyFilter = agencyFilter, StatusFilter = statusFilter, FilterAnalytics = filterAnalytics });
        }

        public async Task<IActionResult> OnPostRevokeCertAsync(int p = 1, int pageSize = 25, string? searchString = null, int? agencyFilter = null, string? statusFilter = null, bool filterAnalytics = true)
        {
            try
            {
                var record = await _context.CertificationRequests.FindAsync(TargetCertId);
                if (record != null)
                {
                    record.Status = RequestStatus.Revoked;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification revoked.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }
            return RedirectToPage(new { p, pageSize, SearchString = searchString, AgencyFilter = agencyFilter, StatusFilter = statusFilter, FilterAnalytics = filterAnalytics });
        }

        public async Task<IActionResult> OnPostRestoreCertAsync(int p = 1, int pageSize = 25, string? searchString = null, int? agencyFilter = null, string? statusFilter = null, bool filterAnalytics = true)
        {
            try
            {
                var record = await _context.CertificationRequests.FindAsync(TargetCertId);
                if (record != null)
                {
                    record.Status = RequestStatus.Passed;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification restored successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }
            return RedirectToPage(new { p, pageSize, SearchString = searchString, AgencyFilter = agencyFilter, StatusFilter = statusFilter, FilterAnalytics = filterAnalytics });
        }

        public async Task<IActionResult> OnPostArchiveCertAsync(int p = 1, int pageSize = 25, string? searchString = null, int? agencyFilter = null, string? statusFilter = null, bool filterAnalytics = true)
        {
            try
            {
                var record = await _context.CertificationRequests.FindAsync(TargetCertId);
                if (record != null)
                {
                    record.Status = RequestStatus.Archived;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification archived.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }
            return RedirectToPage(new { p, pageSize, SearchString = searchString, AgencyFilter = agencyFilter, StatusFilter = statusFilter, FilterAnalytics = filterAnalytics });
        }

        public async Task<IActionResult> OnPostAddDirectCertAsync(int NewEmployeeId, string? NewEmployeeName, int NewAgencyId, int NewCertificationId, DateTime NewDatePassed, DateTime? NewExpirationDate, int p = 1, int pageSize = 25, string? searchString = null, int? agencyFilter = null, string? statusFilter = null, bool filterAnalytics = true)
        {
            try
            {
                // 1. Handle "Create New Employee" workflow on the fly
                if (NewEmployeeId == -1 && !string.IsNullOrWhiteSpace(NewEmployeeName))
                {
                    var newEmployee = new Employee
                    {
                        DisplayName = NewEmployeeName.Trim()
                    };
                    // Adding Employee without setting IsActive since it's not a property of Employee
                    _context.Employees.Add(newEmployee);
                    await _context.SaveChangesAsync(); // Save to generate the new ID

                    NewEmployeeId = newEmployee.Id; // Map the new ID for the certification record
                }

                // 2. Standard Certification Save Workflow
                if (NewEmployeeId > 0 && NewAgencyId > 0 && NewCertificationId > 0)
                {
                    DateTime utcDatePassed = DateTime.SpecifyKind(NewDatePassed, DateTimeKind.Utc);
                    DateTime? utcExpirationDate = NewExpirationDate.HasValue
                        ? DateTime.SpecifyKind(NewExpirationDate.Value, DateTimeKind.Utc)
                        : null;

                    var newCert = new CertificationRequest
                    {
                        EmployeeId = NewEmployeeId,
                        AgencyId = NewAgencyId,
                        CertificationId = NewCertificationId,
                        RequestDate = utcDatePassed,
                        ExpirationDate = utcExpirationDate,
                        Status = RequestStatus.Passed,
                        // Bypass approval lifecycle, no ActionDate included based on previous error
                    };

                    _context.CertificationRequests.Add(newCert);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "New certification added successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding direct certification.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }

            return RedirectToPage(new { p, pageSize, SearchString = searchString, AgencyFilter = agencyFilter, StatusFilter = statusFilter, FilterAnalytics = filterAnalytics });
        }

        public async Task<IActionResult> OnPostDownloadReportAsync(string agencyChartBase64, string certChartBase64, string? searchString, int? agencyFilter, string? statusFilter, bool filterAnalytics)
        {
            var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
            int thresholdDays = thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int explicitThreshold) ? explicitThreshold : 30;
            DateTime today = DateTime.UtcNow;
            DateTime thresholdDate = today.AddDays(thresholdDays);

            var baseQuery = _context.CertificationRequests
                .Include(c => c.Employee)
                .Include(c => c.Agency)
                .Include(c => c.Certification)
                .Where(c => c.Status == RequestStatus.Passed || c.Status == RequestStatus.Revoked);

            var tableQuery = baseQuery;

            if (!string.IsNullOrWhiteSpace(searchString))
                tableQuery = tableQuery.Where(c => c.Employee != null && c.Employee.DisplayName.ToLower().Contains(searchString.ToLower()));

            if (agencyFilter.HasValue && agencyFilter.Value > 0)
                tableQuery = tableQuery.Where(c => c.AgencyId == agencyFilter.Value);

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<TrackerStatus>(statusFilter, out var parsedStatus))
            {
                switch (parsedStatus)
                {
                    case TrackerStatus.Expired: tableQuery = tableQuery.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value < today); break;
                    case TrackerStatus.ExpiringSoon: tableQuery = tableQuery.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value >= today && c.ExpirationDate.Value <= thresholdDate); break;
                    case TrackerStatus.Active: tableQuery = tableQuery.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value > thresholdDate); break;
                    case TrackerStatus.Permanent: tableQuery = tableQuery.Where(c => c.ExpirationDate == null); break;
                }
            }

            var tableData = await tableQuery.AsNoTracking().ToListAsync();

            // Analytics Logic respecting the Toggle
            var analyticsQuery = filterAnalytics ? tableQuery : baseQuery;
            var analyticsRaw = await analyticsQuery.AsNoTracking().ToListAsync();

            int totalActive = analyticsRaw.Count(c => !c.ExpirationDate.HasValue || c.ExpirationDate.Value > thresholdDate);
            int expiringSoon = analyticsRaw.Count(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value >= today && c.ExpirationDate.Value <= thresholdDate);
            int criticalLapses = analyticsRaw.Count(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value < today);

            var groupedByCert = tableData
                .GroupBy(c => c.Certification?.Name ?? c.CustomCertificationName ?? "Custom Certification")
                .OrderBy(g => g.Key)
                .ToList();

            byte[]? agencyChartBytes = !string.IsNullOrEmpty(agencyChartBase64) ? Convert.FromBase64String(agencyChartBase64.Split(',')[1]) : null;
            byte[]? certChartBytes = !string.IsNullOrEmpty(certChartBase64) ? Convert.FromBase64String(certChartBase64.Split(',')[1]) : null;

            var logoPath = Path.Combine(_env.WebRootPath, "img", "branding-assets", "Specialized-Engineering-Logo-white.webp");
            byte[]? logoBytes = System.IO.File.Exists(logoPath) ? await System.IO.File.ReadAllBytesAsync(logoPath) : null;

            var topAgenciesList = analyticsRaw.GroupBy(c => c.Agency != null ? c.Agency.Abbreviation : (c.CustomAgencyName ?? "Unknown"))
                .OrderByDescending(g => g.Count()).Take(5).ToList();
            
            var topCertsList = analyticsRaw.GroupBy(c => c.Certification != null ? c.Certification.Name : (c.CustomCertificationName ?? "Unknown"))
                .OrderByDescending(g => g.Count()).Take(5).ToList();

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
                        row.RelativeItem().AlignRight().AlignMiddle().Text("Compliance & Coverage Analytics").FontColor(Colors.White).FontSize(16).SemiBold();
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Green.Darken1).Padding(10).Column(c =>
                            {
                                c.Item().Text("TOTAL ACTIVE").FontColor(Colors.White).FontSize(8).SemiBold();
                                c.Item().Text(totalActive.ToString()).FontColor(Colors.White).FontSize(18).Bold();
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Orange.Medium).Padding(10).Column(c =>
                            {
                                c.Item().Text("EXPIRING SOON").FontColor(Colors.Black).FontSize(8).SemiBold();
                                c.Item().Text(expiringSoon.ToString()).FontColor(Colors.Black).FontSize(18).Bold();
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Red.Medium).Padding(10).Column(c =>
                            {
                                c.Item().Text("CRITICAL LAPSES").FontColor(Colors.White).FontSize(8).SemiBold();
                                c.Item().Text(criticalLapses.ToString()).FontColor(Colors.White).FontSize(18).Bold();
                            });
                        });

                        var expiredList = analyticsRaw.Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value < today).ToList();
                        if (expiredList.Any())
                        {
                            col.Item().PaddingTop(15).PaddingBottom(5).Text("⚠️ Action Required: Critical Lapses").FontSize(12).SemiBold().FontColor(Colors.Red.Medium);
                            foreach(var lapse in expiredList) {
                                var empName = lapse.Employee != null ? lapse.Employee.DisplayName : "Unknown";
                                var certName = lapse.Certification != null ? lapse.Certification.Name : (lapse.CustomCertificationName ?? "Unknown");
                                col.Item().Text($"• {empName} - {certName} (Expired: {lapse.ExpirationDate!.Value:MMM dd, yyyy})").FontSize(10);
                            }
                        }

                        if (agencyChartBytes != null || certChartBytes != null)
                        {
                            col.Item().PaddingBottom(20).Row(row =>
                            {
                                if (agencyChartBytes != null) row.RelativeItem().Image(agencyChartBytes);
                                row.ConstantItem(20);
                                if (certChartBytes != null) row.RelativeItem().Image(certChartBytes);
                            });
                        }

                        // Inject Raw Data Tables below charts
                        col.Item().PaddingBottom(15).Row(dataRow =>
                        {
                            dataRow.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingBottom(5).Text("Top Agency Distribution").FontSize(11).SemiBold().FontColor(Colors.Grey.Darken3);
                                foreach(var item in topAgenciesList) {
                                    c.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{item.Key}: {item.Count()}").FontSize(9);
                                }
                            });
                            dataRow.ConstantItem(20);
                            dataRow.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingBottom(5).Text("Top 5 Certifications").FontSize(11).SemiBold().FontColor(Colors.Grey.Darken3);
                                foreach(var item in topCertsList) {
                                    c.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{item.Key}: {item.Count()}").FontSize(9);
                                }
                            });
                        });

                        col.Item().PaddingBottom(5).Text("Coverage Breakdown").FontSize(14).SemiBold().FontColor("#a19482");

                        if (!groupedByCert.Any())
                        {
                            col.Item().PaddingTop(10).Text("No records match the current filter criteria.").Italic().FontColor(Colors.Grey.Medium);
                        }

                        foreach (var group in groupedByCert)
                        {
                            col.Item().PaddingTop(10).PaddingBottom(5).Text(group.Key).FontSize(12).SemiBold().FontColor(Colors.Grey.Darken3);
                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); columns.RelativeColumn(2); columns.RelativeColumn(1.5f); columns.RelativeColumn(1.5f);
                                });
                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2).Text("Employee").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2).Text("Agency").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2).Text("Expires").SemiBold();
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2).Text("Status").SemiBold();
                                });
                                foreach (var cert in group.OrderBy(c => c.Employee?.DisplayName))
                                {
                                    var status = GetComputedStatus(cert.ExpirationDate);
                                    var statusText = status == TrackerStatus.Active ? "Active" : status == TrackerStatus.ExpiringSoon ? "Expiring Soon" : status == TrackerStatus.Expired ? "Expired" : "Permanent";
                                    var statusColor = status == TrackerStatus.Expired ? Colors.Red.Medium : status == TrackerStatus.ExpiringSoon ? Colors.Orange.Medium : Colors.Black;

                                    table.Cell().PaddingVertical(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.Employee?.DisplayName ?? "Unknown");
                                    table.Cell().PaddingVertical(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.Agency?.Abbreviation ?? cert.CustomAgencyName ?? "Custom");
                                    table.Cell().PaddingVertical(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.ExpirationDate.HasValue ? cert.ExpirationDate.Value.ToString("MMM dd, yyyy") : "None");
                                    table.Cell().PaddingVertical(2).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(statusText).FontColor(statusColor);
                                }
                            });
                        }
                    });
                    page.Footer().AlignCenter().Text(x => { x.Span($"Generated {DateTime.Now:g} | Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages(); });
                });
            });

            return File(document.GeneratePdf(), "application/pdf", $"SPE_Analytics_Report_{DateTime.Now:yyyyMMdd}.pdf");
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
                    .Where(c => c.EmployeeId == employeeId && (c.Status == RequestStatus.Passed || c.Status == RequestStatus.Revoked || c.Status == RequestStatus.Archived))
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
                                        var statusText = cert.Status == RequestStatus.Passed && cert.ExpirationDate.HasValue && cert.ExpirationDate.Value < DateTime.UtcNow ? "Expired" : cert.Status.ToString();

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
                            x.Span($"Generated {DateTime.Now:g} | Page ");
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
