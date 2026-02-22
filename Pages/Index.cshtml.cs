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
        TotalRequests = await _context.CertificationRequests.CountAsync();
        ActiveCertifications = await _context.CertificationRequests.CountAsync(r => r.Status == RequestStatus.Passed);
        PendingCount = await _context.CertificationRequests.CountAsync(r => r.Status == RequestStatus.Pending);
        
        ExpiringCount = await _context.CertificationRequests
            .CountAsync(r => r.Status == RequestStatus.Passed && r.ExpirationDate != null && r.ExpirationDate <= DateTime.UtcNow.AddMonths(2));

        RecentRequests = await _context.CertificationRequests
            .Include(r => r.Employee)
            .Include(r => r.Agency)
            .Include(r => r.Certification)
            .OrderByDescending(r => r.RequestDate)
            .Take(5)
            .ToListAsync();
    }
}
