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

    public IList<SeHrCertificationPortal.Models.Certification> Certification { get;set; } = default!;

    public async Task OnGetAsync()
    {
        Certification = await _context.Certifications
            .Include(c => c.Agency)
            .OrderBy(c => c.Agency!.Abbreviation)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
    }
}
