using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SeHrCertificationPortal.Pages.Requests
{
    public class IndexModel : PageModel
    {
        private readonly SeHrCertificationPortal.Data.ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _env;

        public IndexModel(SeHrCertificationPortal.Data.ApplicationDbContext context, ILogger<IndexModel> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public IList<SeHrCertificationPortal.Models.CertificationRequest> CertificationRequest { get; set; } = default!;
        public IList<SeHrCertificationPortal.Models.Agency> Agencies { get; set; } = default!;

        [BindProperty]
        public SeHrCertificationPortal.Models.CertificationRequest NewRequest { get; set; } = new();

        [BindProperty]
        public string EmployeeNameInput { get; set; } = string.Empty;

        public IList<SeHrCertificationPortal.Models.Employee> Employees { get; set; } = default!;

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 25;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public SeHrCertificationPortal.Models.RequestStatus? StatusFilter { get; set; }

        public async Task<IActionResult> OnGetAsync(int p = 1, int? pageSize = null)
        {
            try {
                CurrentPage = p < 1 ? 1 : p;

                // Restrict PageSize to valid options, default to 25
                int[] validSizes = { 10, 15, 20, 25, 50 };
                PageSize = pageSize.HasValue && validSizes.Contains(pageSize.Value) ? pageSize.Value : 25;

                var query = _context.CertificationRequests
                    .Include(c => c.Agency)
                    .Include(c => c.Certification)
                    .Include(c => c.Employee)
                    .AsQueryable();

                if (StatusFilter.HasValue)
                {
                    query = query.Where(c => c.Status == StatusFilter.Value);
                }

                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    var searchLower = SearchString.ToLower().Trim();
                    // Clean the search string so users can type "REQ-764" or "764" interchangeably
                    var idSearch = searchLower.Replace("req-", "").Trim();

                    query = query.Where(c =>
                        (c.Employee != null && c.Employee.DisplayName.ToLower().Contains(searchLower)) ||
                        (c.ManagerName != null && c.ManagerName.ToLower().Contains(searchLower)) ||
                        c.Id.ToString() == idSearch);
                }

                TotalRecords = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

                CertificationRequest = await query
                    .OrderByDescending(c => c.RequestDate)
                    .ThenByDescending(c => c.Id)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                Agencies = await _context.Agencies
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Abbreviation)
                    .ToListAsync();

                Employees = await _context.Employees
                    .OrderBy(e => e.DisplayName)
                    .ToListAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error fetching data for page load.");
                TempData["ErrorMessage"] = "Unable to connect to the database to load records. The system may be experiencing an outage.";
                CertificationRequest = new List<SeHrCertificationPortal.Models.CertificationRequest>();
                Agencies = new List<SeHrCertificationPortal.Models.Agency>();
                Employees = new List<SeHrCertificationPortal.Models.Employee>();
            }
            return Page();
        }

        public IActionResult OnGetCertificationsByAgency(int agencyId)
        {
            var certs = _context.Certifications
                .Where(c => c.AgencyId == agencyId && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();
            return new JsonResult(certs);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, SeHrCertificationPortal.Models.RequestStatus newStatus, int p = 1, int pageSize = 25, string? searchString = null, SeHrCertificationPortal.Models.RequestStatus? statusFilter = null)
        {
            try
            {
                var request = await _context.CertificationRequests.FindAsync(id);
                if (request == null) return NotFound();

                request.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Status updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing status update.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }

            return RedirectToPage(new { p, pageSize, SearchString = searchString, StatusFilter = statusFilter });
        }

        public async Task<IActionResult> OnPostMarkPassedAsync(int id, DateTime? expirationDate, int p = 1, int pageSize = 25, string? searchString = null, SeHrCertificationPortal.Models.RequestStatus? statusFilter = null)
        {
            try
            {
                var request = await _context.CertificationRequests.FindAsync(id);
                if (request != null)
                {
                    request.Status = SeHrCertificationPortal.Models.RequestStatus.Passed;
                    if (expirationDate.HasValue)
                    {
                        request.ExpirationDate = DateTime.SpecifyKind(expirationDate.Value, DateTimeKind.Utc);
                    }
                    else
                    {
                        request.ExpirationDate = null;
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Certification marked as passed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing certification pass.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }
            return RedirectToPage(new { p, pageSize, SearchString = searchString, StatusFilter = statusFilter });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(EmployeeNameInput)) return RedirectToPage();

            try
            {
                // 1. Auto-Add Logic: Find existing or create new employee silently
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.DisplayName.ToLower() == EmployeeNameInput.ToLower());

                if (employee == null)
                {
                    employee = new SeHrCertificationPortal.Models.Employee { DisplayName = EmployeeNameInput };
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                }

                // 2. Map and Save Request
                NewRequest.EmployeeId = employee.Id;
                NewRequest.RequestDate = DateTime.SpecifyKind(NewRequest.RequestDate, DateTimeKind.Utc);
                NewRequest.Status = SeHrCertificationPortal.Models.RequestStatus.Pending;

                // Clean custom fields if standard agency is selected
                if (NewRequest.AgencyId != null)
                {
                    NewRequest.CustomAgencyName = null;
                    NewRequest.CustomCertificationName = null;
                }

                _context.CertificationRequests.Add(NewRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "New request created successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing new request.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditRequestAsync(int id, string managerName, SeHrCertificationPortal.Models.RequestType requestType, DateTime requestDate, int agencyId, int certificationId, int p = 1, int pageSize = 25, string? searchString = null, SeHrCertificationPortal.Models.RequestStatus? statusFilter = null)
        {
            try
            {
                var request = await _context.CertificationRequests.FindAsync(id);
                if (request != null)
                {
                    request.ManagerName = managerName;
                    request.RequestType = requestType;
                    request.RequestDate = DateTime.SpecifyKind(requestDate, DateTimeKind.Utc);
                    request.AgencyId = agencyId;
                    request.CertificationId = certificationId;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Request edited successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing request.");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again or contact IT.";
            }
            return RedirectToPage(new { p, pageSize, SearchString = searchString, StatusFilter = statusFilter });
        }

        public IActionResult OnPostDownloadBlankForm()
        {
            var logoPath = Path.Combine(_env.WebRootPath, "img", "branding-assets", "Specialized-Engineering-Logo-white.webp");
            byte[]? logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Background("#66615c").Padding(10).Row(row =>
                    {
                        if (logoBytes != null)
                        {
                            row.ConstantItem(150).Image(logoBytes);
                        }
                        row.RelativeItem().AlignRight().AlignMiddle().Text("Employee Certification Request Form").FontColor(Colors.White).FontSize(16).SemiBold();
                    });

                    page.Content().PaddingVertical(20).PaddingHorizontal(20).Column(col =>
                    {
                        col.Spacing(15);
                        col.Item().Text("Please complete all fields and obtain manager signature.").Italic().FontSize(12).FontColor(Colors.Grey.Medium);

                        col.Item().Text("Employee Name: _____________________________________________________    Request Date: ___________________________").FontSize(12);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Manager: ________________________________________________________________").FontSize(12);
                            r.AutoItem().AlignBottom().Row(r2 => 
                            { 
                                r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); 
                                r2.RelativeItem().PaddingLeft(5).AlignMiddle().Text("Recertification").FontSize(11); 
                            });
                        });

                        col.Item().Text("Certification Agency: ___________________________________    Certification Desired: ___________________________________").FontSize(12);

                        col.Item().PaddingTop(10).Column(c =>
                        {
                            c.Item().PaddingBottom(5).Text("Exam Type:").SemiBold();
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Review Session"); });
                                r.RelativeItem().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Written Exam"); });
                                r.RelativeItem().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Practical Exam"); });
                                r.RelativeItem().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Reciprocity"); });
                            });
                        });

                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.AutoItem().PaddingRight(15).Text("Need to purchase study material?").SemiBold();
                            r.AutoItem().PaddingRight(15).Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("YES"); });
                            r.AutoItem().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("NO"); });
                        });

                        col.Item().PaddingTop(10).Text("Date Needed: ___________________________________________    Date Offered: ___________________________________________").FontSize(12);
                        col.Item().Text("Location of Exam: ______________________________________________________________________________________________").FontSize(12);

                        col.Item().PaddingTop(20).Text("Approved By (Manager Signature): ___________________________________________    Date: ___________________________").FontSize(12);

                        col.Item().PaddingTop(30).Border(1).BorderColor(Colors.Black).Padding(10).Column(ac =>
                        {
                            ac.Item().PaddingBottom(10).Text("For Administrative Use Only").SemiBold().FontSize(12);
                            ac.Item().Row(r =>
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Spacing(8);
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Registration Submitted"); });
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Added to Tracking Spreadsheet"); });
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Cert / License Rec'd"); });
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Added to DB"); });
                                });
                                r.RelativeItem().Column(c =>
                                {
                                    c.Spacing(8);
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Confirmation Received"); });
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Confirmation Forwarded"); });
                                    c.Item().Row(r2 => { r2.AutoItem().Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.RelativeItem().PaddingLeft(5).Text("Added to Calendar"); });
                                    c.Item().Row(r2 => { r2.AutoItem().Text("Lodging Needed?"); r2.AutoItem().PaddingLeft(10).Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.AutoItem().PaddingLeft(5).Text("Yes"); r2.AutoItem().PaddingLeft(10).Width(12).Height(12).Border(1).BorderColor(Colors.Black); r2.AutoItem().PaddingLeft(5).Text("No"); });
                                });
                            });
                            ac.Item().PaddingTop(15).Text("Written Exam Date: ______________________________________________________________________________________________").FontSize(12);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("John Harrison | Corporate Director of Training & Facilities | Email: jharrison@specializedengineering.com | Mobile: 240-674-0250").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            Response.Headers.Append("Content-Disposition", "inline; filename=\"Certification_Request_Form.pdf\"");
            return File(pdfBytes, "application/pdf");
        }
    }
}
