using Microsoft.AspNetCore.Mvc;
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
    public IList<SeHrCertificationPortal.Models.Agency> Agencies { get; set; } = default!;

    public async Task OnGetAsync()
    {
        CertificationRequest = await _context.CertificationRequests
            .Include(c => c.Agency)
            .Include(c => c.Certification)
            .Include(c => c.Employee)
            .OrderByDescending(c => c.RequestDate)
            .ToListAsync();

        Agencies = await _context.Agencies
            .Where(a => a.IsActive)
            .OrderBy(a => a.Abbreviation)
            .ToListAsync();
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
    }
}
