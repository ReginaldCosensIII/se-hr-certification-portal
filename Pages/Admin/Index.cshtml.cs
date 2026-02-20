using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SeHrCertificationPortal.Pages.Admin
{
    public class SettingsModel : PageModel
    {
    private readonly SeHrCertificationPortal.Data.ApplicationDbContext _context;

    public SettingsModel(SeHrCertificationPortal.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<SeHrCertificationPortal.Models.Agency> Agency { get;set; } = default!;
    public IList<SeHrCertificationPortal.Models.Certification> Certifications { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Agency = await _context.Agencies
            .Include(a => a.Certifications)
            .OrderBy(a => a.Abbreviation)
            .ToListAsync();

        Certifications = await _context.Certifications
            .Include(c => c.Agency)
            .OrderBy(c => c.Agency!.Abbreviation)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
    }
}
