using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Utilities;

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

        public string? CurrentSort { get; set; }

        public async Task<IActionResult> OnGetAsync(int p = 1, int? pageSize = null, string? sortOrder = null)
        {
            try {
                CurrentSort = sortOrder;
                CurrentPage = p < 1 ? 1 : p;

                // Restrict PageSize to valid options, default to 25 (or User Preference Cookie)
                int[] validSizes = { 10, 25, 50, 100 };
                int defaultSize = 25;
                if (Request.Cookies.TryGetValue("userDefaultRows", out string? cookieVal) && int.TryParse(cookieVal, out int parsedCookie) && validSizes.Contains(parsedCookie)) {
                    defaultSize = parsedCookie;
                }
                
                PageSize = pageSize.HasValue && validSizes.Contains(pageSize.Value) ? pageSize.Value : defaultSize;

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

        // Server-Side Sorting Logic
        var orderedQuery = sortOrder switch
        {
            "id_asc" => query.OrderBy(c => c.Id),
            "id_desc" => query.OrderByDescending(c => c.Id),
            "emp_asc" => query.OrderBy(c => c.Employee!.DisplayName),
            "emp_desc" => query.OrderByDescending(c => c.Employee!.DisplayName),
            "manager_asc" => query.OrderBy(c => c.ManagerName),
            "manager_desc" => query.OrderByDescending(c => c.ManagerName),
            "agency_asc" => query.OrderBy(c => c.Agency!.Abbreviation ?? c.CustomAgencyName),
            "agency_desc" => query.OrderByDescending(c => c.Agency!.Abbreviation ?? c.CustomAgencyName),
            "cert_asc" => query.OrderBy(c => c.Certification!.Name ?? c.CustomCertificationName),
            "cert_desc" => query.OrderByDescending(c => c.Certification!.Name ?? c.CustomCertificationName),
            "type_asc" => query.OrderBy(c => c.RequestType),
            "type_desc" => query.OrderByDescending(c => c.RequestType),
            "status_asc" => query.OrderBy(c => c.Status),
            "status_desc" => query.OrderByDescending(c => c.Status),
            "date_asc" => query.OrderBy(c => c.RequestDate),
            "date_desc" => query.OrderByDescending(c => c.RequestDate),
            _ => query.OrderByDescending(c => c.RequestDate), // Default Reset State
        };

                CertificationRequest = await orderedQuery
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
            try
            {
                if (string.IsNullOrWhiteSpace(EmployeeNameInput) ||
                    string.IsNullOrWhiteSpace(NewRequest.ManagerName) ||
                    NewRequest.AgencyId == null ||
                    NewRequest.CertificationId == null ||
                    NewRequest.RequestType == null)
                {
                    TempData["ErrorMessage"] = "Please complete all required fields to submit a new request.";
                    return RedirectToPage(new { openNew = "true" }); // Keeps the accordion open so the user sees the error
                }
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
                if (string.IsNullOrWhiteSpace(managerName) || agencyId <= 0 || certificationId <= 0)
                {
                    TempData["ErrorMessage"] = "Please complete all required fields to save changes.";
                    return RedirectToPage(new { p, pageSize, SearchString = searchString, StatusFilter = statusFilter });
                }

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

        public async Task<IActionResult> OnPostDownloadRequestsCsvAsync()
        {
            try
            {
                var requests = await _context.CertificationRequests
                    .Include(c => c.Agency)
                    .Include(c => c.Certification)
                    .Include(c => c.Employee)
                    .OrderByDescending(c => c.RequestDate)
                    .ToListAsync();
                var headers = new[] { "Request ID", "Employee Name", "Agency", "Certification", "Request Date", "Expiration Date", "Status" };
                var csv = CsvExportHelper.GenerateCsv(requests, headers, r => new string[] { 
                    r.Id.ToString(), 
                    r.Employee?.DisplayName ?? "Unknown", 
                    r.Agency?.Abbreviation ?? r.CustomAgencyName ?? "Unknown", 
                    r.Certification?.Name ?? r.CustomCertificationName ?? "Unknown", 
                    r.RequestDate.ToString("yyyy-MM-dd"), 
                    r.ExpirationDate?.ToString("yyyy-MM-dd") ?? "Permanent", 
                    r.Status.ToString() 
                });
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"Requests_Export_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Requests CSV.");
                TempData["ErrorMessage"] = "Failed to export CSV. Please try again.";
                return RedirectToPage();
            }
        }
    }
}
