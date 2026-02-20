using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SeHrCertificationPortal.Pages.Requests
{
    public class IndexModel : PageModel
    {
    private readonly SeHrCertificationPortal.Data.ApplicationDbContext _context;

    public IndexModel(SeHrCertificationPortal.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<SeHrCertificationPortal.Models.CertificationRequest> CertificationRequest { get;set; } = default!;

    public async Task OnGetAsync()
    {
        CertificationRequest = await _context.CertificationRequests
            .Include(c => c.Agency)
            .Include(c => c.Certification)
            .Include(c => c.Employee)
            .OrderByDescending(c => c.RequestDate)
            .ToListAsync();
    }
    }
}
