using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Data;
using SeHrCertificationPortal.Models;

namespace SeHrCertificationPortal.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public int TotalRequests { get; set; }
    public int ActiveCertifications { get; set; }
    public int PendingCount { get; set; }
    public int ExpiringCount { get; set; }
    public IList<CertificationRequest> RecentRequests { get; set; } = default!;
    public IList<CertificationRequest> CriticalActionItems { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? CurrentSort { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 5;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try 
        {
            // 1. Fetch Dynamic Expiration Threshold
            var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
            int thresholdDays = thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int explicitThreshold) ? explicitThreshold : 30;
            DateTime today = DateTime.UtcNow;
            DateTime thresholdDate = today.AddDays(thresholdDays);

            // 2. Execute KPI Queries
            TotalRequests = await _context.CertificationRequests.CountAsync();

            PendingCount = await _context.CertificationRequests.CountAsync(r => r.Status == RequestStatus.Pending);

            // Active: Passed AND (Permanent OR Expires strictly after today)
            ActiveCertifications = await _context.CertificationRequests
                .CountAsync(r => r.Status == RequestStatus.Passed && (r.ExpirationDate == null || r.ExpirationDate > today));

            // Expiring Soon: Passed AND Expires between today and threshold
            ExpiringCount = await _context.CertificationRequests
                .CountAsync(r => r.Status == RequestStatus.Passed && r.ExpirationDate >= today && r.ExpirationDate <= thresholdDate);

            // Fetch Critical Action Items (Already expired OR expiring within threshold)
            CriticalActionItems = await _context.CertificationRequests
                .Include(r => r.Employee)
                .Include(r => r.Certification)
                .Where(r => r.Status == RequestStatus.Passed && r.ExpirationDate != null && r.ExpirationDate <= thresholdDate)
                .OrderBy(r => r.ExpirationDate)
                .Take(15)
                .ToListAsync();

            // 3. Fetch Recent Requests
            var requestQuery = _context.CertificationRequests
                .Include(r => r.Employee)
                .Include(r => r.Agency)
                .Include(r => r.Certification)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                requestQuery = requestQuery.Where(r =>
                    (r.Employee != null && r.Employee.DisplayName.ToLower().Contains(SearchString.ToLower())) ||
                    r.Id.ToString() == SearchString);
            }

            requestQuery = CurrentSort switch
            {
                "id_asc" => requestQuery.OrderBy(c => c.Id),
                "id_desc" => requestQuery.OrderByDescending(c => c.Id),
                "emp_asc" => requestQuery.OrderBy(c => c.Employee!.DisplayName),
                "emp_desc" => requestQuery.OrderByDescending(c => c.Employee!.DisplayName),
                "manager_asc" => requestQuery.OrderBy(c => c.ManagerName),
                "manager_desc" => requestQuery.OrderByDescending(c => c.ManagerName),
                "agency_asc" => requestQuery.OrderBy(c => c.Agency!.Abbreviation ?? c.CustomAgencyName),
                "agency_desc" => requestQuery.OrderByDescending(c => c.Agency!.Abbreviation ?? c.CustomAgencyName),
                "cert_asc" => requestQuery.OrderBy(c => c.Certification!.Name ?? c.CustomCertificationName),
                "cert_desc" => requestQuery.OrderByDescending(c => c.Certification!.Name ?? c.CustomCertificationName),
                "type_asc" => requestQuery.OrderBy(c => c.RequestType),
                "type_desc" => requestQuery.OrderByDescending(c => c.RequestType),
                "status_asc" => requestQuery.OrderBy(c => c.Status),
                "status_desc" => requestQuery.OrderByDescending(c => c.Status),
                "date_asc" => requestQuery.OrderBy(c => c.RequestDate),
                "date_desc" => requestQuery.OrderByDescending(c => c.RequestDate),
                _ => requestQuery.OrderByDescending(c => c.RequestDate), // Default
            };

            RecentRequests = await requestQuery
                .Take(PageSize > 0 ? PageSize : 5)
                .ToListAsync();
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data for Dashboard.");
            TempData["ErrorMessage"] = "Unable to connect to the database to load records. The system may be experiencing an outage.";
            RecentRequests = new List<CertificationRequest>();
            CriticalActionItems = new List<CertificationRequest>();
        }
        return Page();
    }
}
