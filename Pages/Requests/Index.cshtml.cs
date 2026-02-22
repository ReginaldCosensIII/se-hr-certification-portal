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
    
    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 25;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public SeHrCertificationPortal.Models.RequestStatus? StatusFilter { get; set; }

    public async Task OnGetAsync(int p = 1, int? pageSize = null)
    {
        CurrentPage = p < 1 ? 1 : p;
        
        // Restrict PageSize to valid options, default to 25
        int[] validSizes = { 10, 15, 20, 25, 50 };
        PageSize = pageSize.HasValue && validSizes.Contains(pageSize.Value) ? pageSize.Value : 25;

        var query = _context.CertificationRequests
            .Include(c => c.Agency)
            .Include(c => c.Certification)
            .Include(c => c.Employee)
            .AsQueryable();

        if (StatusFilter.HasValue)
        {
            query = query.Where(c => c.Status == StatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            var searchLower = SearchString.ToLower();
            query = query.Where(c => 
                (c.Employee != null && c.Employee.DisplayName.ToLower().Contains(searchLower)) ||
                (c.ManagerName != null && c.ManagerName.ToLower().Contains(searchLower)) ||
                c.Id.ToString() == searchLower);
        }

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

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, SeHrCertificationPortal.Models.RequestStatus newStatus, int p = 1, int pageSize = 25)
    {
        var request = await _context.CertificationRequests.FindAsync(id);
        if (request == null) return NotFound();

        request.Status = newStatus;
        await _context.SaveChangesAsync();

        return RedirectToPage(new { p, pageSize });
    }

    public async Task<IActionResult> OnPostMarkPassedAsync(int id, DateTime? expirationDate, int p = 1, int pageSize = 25)
    {
        var request = await _context.CertificationRequests.FindAsync(id);
        if (request != null)
        {
            request.Status = SeHrCertificationPortal.Models.RequestStatus.Passed;
            if (expirationDate.HasValue)
            {
                request.ExpirationDate = DateTime.SpecifyKind(expirationDate.Value, DateTimeKind.Utc);
            }
            else
            {
                request.ExpirationDate = null;
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { p, pageSize });
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
        NewRequest.RequestDate = DateTime.SpecifyKind(NewRequest.RequestDate, DateTimeKind.Utc);
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

    public async Task<IActionResult> OnPostEditRequestAsync(int id, string managerName, SeHrCertificationPortal.Models.RequestType requestType, DateTime requestDate, int agencyId, int certificationId, int p = 1, int pageSize = 25)
    {
        var request = await _context.CertificationRequests.FindAsync(id);
        if (request != null)
        {
            request.ManagerName = managerName;
            request.RequestType = requestType;
            request.RequestDate = DateTime.SpecifyKind(requestDate, DateTimeKind.Utc);
            request.AgencyId = agencyId;
            request.CertificationId = certificationId;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { p, pageSize });
    }
    }
}
