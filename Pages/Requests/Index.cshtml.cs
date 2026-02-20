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
    
    [BindProperty]
    public SeHrCertificationPortal.Models.CertificationRequest NewRequest { get; set; } = new();
    
    [BindProperty]
    public string EmployeeNameInput { get; set; } = string.Empty;

    public IList<SeHrCertificationPortal.Models.Employee> Employees { get; set; } = default!;

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public const int PageSize = 25;

    public async Task OnGetAsync(int p = 1)
    {
        CurrentPage = p < 1 ? 1 : p;

        var query = _context.CertificationRequests
            .Include(c => c.Agency)
            .Include(c => c.Certification)
            .Include(c => c.Employee);

        TotalRecords = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

        CertificationRequest = await query
            .OrderByDescending(c => c.RequestDate)
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(EmployeeNameInput)) return RedirectToPage();

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
        NewRequest.RequestDate = DateTime.UtcNow;
        NewRequest.Status = SeHrCertificationPortal.Models.RequestStatus.Pending;

        // Clean custom fields if standard agency is selected
        if (NewRequest.AgencyId != null) 
        {
            NewRequest.CustomAgencyName = null;
            NewRequest.CustomCertificationName = null;
        }

        _context.CertificationRequests.Add(NewRequest);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
    }
}
