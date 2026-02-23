using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Data;
using SeHrCertificationPortal.Models;

namespace SeHrCertificationPortal.Pages.Certifications
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
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

        public IList<CertificationRequest> PassedCertifications { get; set; } = default!;
        public SelectList AgencyOptions { get; set; } = default!;

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int ThresholdDays { get; set; } = 30;

        public int AnalyticsTotalActive { get; set; }
        public int AnalyticsExpiringSoon { get; set; }
        public int AnalyticsExpired { get; set; }
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

        public async Task OnGetAsync(int p = 1)
        {
            CurrentPage = p < 1 ? 1 : p;

            var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
            if (thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int explicitThreshold))
            {
                ThresholdDays = explicitThreshold;
            }

            var agencies = await _context.Agencies.Where(a => a.IsActive).ToListAsync();
            AgencyOptions = new SelectList(agencies, "Id", "Abbreviation");

            // Npgsql Date Standard: Declare local variables for the query
            DateTime today = DateTime.UtcNow;
            DateTime thresholdDate = today.AddDays(ThresholdDays);

            var query = _context.CertificationRequests
                .Include(c => c.Employee)
                .Include(c => c.Agency)
                .Include(c => c.Certification)
                .Where(c => c.Status == RequestStatus.Passed);

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
                    .Where(c => c.Status == RequestStatus.Passed);

            var analyticsRaw = await analyticsBaseQuery.Select(c => new {
                AgencyName = c.Agency != null ? c.Agency.Abbreviation : (c.CustomAgencyName ?? "Unknown"),
                CertName = c.Certification != null ? c.Certification.Name : (c.CustomCertificationName ?? "Unknown"),
                ExpDate = c.ExpirationDate
            }).ToListAsync();

            AnalyticsExpired = analyticsRaw.Count(c => c.ExpDate.HasValue && c.ExpDate.Value < today);
            AnalyticsExpiringSoon = analyticsRaw.Count(c => c.ExpDate.HasValue && c.ExpDate.Value >= today && c.ExpDate.Value <= thresholdDate);
            AnalyticsTotalActive = analyticsRaw.Count(c => !c.ExpDate.HasValue || c.ExpDate.Value > thresholdDate);

            var topAgencies = analyticsRaw.GroupBy(c => c.AgencyName).OrderByDescending(g => g.Count()).Take(5).ToList();
            AgencyChartLabels = System.Text.Json.JsonSerializer.Serialize(topAgencies.Select(g => g.Key));
            AgencyChartData = System.Text.Json.JsonSerializer.Serialize(topAgencies.Select(g => g.Count()));

            var topCerts = analyticsRaw.GroupBy(c => c.CertName).OrderByDescending(g => g.Count()).Take(5).ToList();
            CertChartLabels = System.Text.Json.JsonSerializer.Serialize(topCerts.Select(g => g.Key));
            CertChartData = System.Text.Json.JsonSerializer.Serialize(topCerts.Select(g => g.Count()));
            // ------------------------

            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            PassedCertifications = await query
                .OrderByDescending(c => c.ExpirationDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetEmployeeHistoryAsync(int employeeId)
        {
            var query = _context.CertificationRequests
                .Where(c => c.EmployeeId == employeeId && c.Status == RequestStatus.Passed)
                .Include(c => c.Agency)
                .Include(c => c.Certification)
                .AsNoTracking();

            var rawData = await query.ToListAsync(); // Materialize to memory first

            var history = rawData.Select(r => new { // Apply C# formatting safely
                agency = r.Agency != null ? r.Agency.Abbreviation : r.CustomAgencyName,
                certification = r.Certification != null ? r.Certification.Name : r.CustomCertificationName,
                datePassed = r.RequestDate.ToString("MMM dd, yyyy"),
                expirationDate = r.ExpirationDate.HasValue ? r.ExpirationDate.Value.ToString("MMM dd, yyyy") : "Permanent",
                status = GetComputedStatus(r.ExpirationDate).ToString()
            }).ToList();

            return new JsonResult(history);
        }
    }
}
