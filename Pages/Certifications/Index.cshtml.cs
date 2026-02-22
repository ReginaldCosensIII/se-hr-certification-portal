using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SeHrCertificationPortal.Pages.Certifications
{
    public class IndexModel : PageModel
    {
    private readonly SeHrCertificationPortal.Data.ApplicationDbContext _context;

    public IndexModel(SeHrCertificationPortal.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<SeHrCertificationPortal.Models.CertificationRequest> ActiveCertifications { get;set; } = default!;

    public async Task OnGetAsync()
    {
        ActiveCertifications = await _context.CertificationRequests
            .Include(r => r.Employee)
            .Include(r => r.Certification)
            .ThenInclude(c => c.Agency)
            .Where(r => r.Status == SeHrCertificationPortal.Models.RequestStatus.Passed)
            .OrderBy(r => r.ExpirationDate)
            .Take(100)
            .AsNoTracking()
            .ToListAsync();
    }
    }
}
