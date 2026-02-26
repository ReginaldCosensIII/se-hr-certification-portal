using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Data;
using SeHrCertificationPortal.Models;

namespace SeHrCertificationPortal.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalRequests { get; set; }
    public int ActiveCertifications { get; set; }
    public int PendingCount { get; set; }
    public int ExpiringCount { get; set; }
    public IList<CertificationRequest> RecentRequests { get; set; } = default!;

    public async Task OnGetAsync()
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

        // 3. Fetch Recent Requests
        RecentRequests = await _context.CertificationRequests
            .Include(r => r.Employee)
            .Include(r => r.Agency)
            .Include(r => r.Certification)
            .OrderByDescending(r => r.RequestDate)
            .Take(5)
            .ToListAsync();
    }
}
