using Microsoft.AspNetCore.Mvc;
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

    [BindProperty]
    public SeHrCertificationPortal.Models.Agency NewAgency { get; set; } = new() { Abbreviation = "", FullName = "" };

    [BindProperty]
    public SeHrCertificationPortal.Models.Certification NewCertification { get; set; } = new() { Name = "" };

    [BindProperty]
    public int AgencyId { get; set; }

    [BindProperty]
    public string? AgencyName { get; set; }

    [BindProperty]
    public int CertId { get; set; }

    [BindProperty]
    public string? CertName { get; set; }

    [BindProperty]
    public int CertAgencyId { get; set; }

    [BindProperty]
    public int CertValidity { get; set; }

    [BindProperty]
    public int ExpiringSoonThresholdDays { get; set; }

    [BindProperty]
    public string AdminEmail { get; set; } = string.Empty;

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

        var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
        ExpiringSoonThresholdDays = thresholdSetting != null && int.TryParse(thresholdSetting.Value, out int days) ? days : 30;

        var emailSetting = await _context.SystemSettings.FindAsync("AdminEmail");
        AdminEmail = emailSetting?.Value ?? "";
    }

    public async Task<IActionResult> OnPostAddAgencyAsync()
    {
        if (string.IsNullOrWhiteSpace(NewAgency.Abbreviation)) return RedirectToPage();
        NewAgency.IsActive = true;
        _context.Agencies.Add(NewAgency);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddCertificationAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCertification.Name) || NewCertification.AgencyId == 0) return RedirectToPage();
        NewCertification.IsActive = true;
        _context.Certifications.Add(NewCertification);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveSettingsAsync()
    {
        var thresholdSetting = await _context.SystemSettings.FindAsync("ExpiringSoonThresholdDays");
        if (thresholdSetting == null)
        {
            _context.SystemSettings.Add(new SeHrCertificationPortal.Models.SystemSetting { Key = "ExpiringSoonThresholdDays", Value = ExpiringSoonThresholdDays.ToString() });
        }
        else
        {
            thresholdSetting.Value = ExpiringSoonThresholdDays.ToString();
        }

        var emailSetting = await _context.SystemSettings.FindAsync("AdminEmail");
        if (emailSetting == null)
        {
            _context.SystemSettings.Add(new SeHrCertificationPortal.Models.SystemSetting { Key = "AdminEmail", Value = AdminEmail });
        }
        else
        {
            emailSetting.Value = AdminEmail;
        }

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAgencyAsync()
    {
        var agency = await _context.Agencies.FindAsync(AgencyId);
        if (agency != null && !string.IsNullOrWhiteSpace(AgencyName))
        {
            agency.FullName = AgencyName;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(); // PRG Pattern (Reloads active tab via our JS localstorage)
    }

    public async Task<IActionResult> OnPostDeactivateAgencyAsync()
    {
        var agency = await _context.Agencies.FindAsync(AgencyId);
        if (agency != null)
        {
            agency.IsActive = false;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditCertificationAsync()
    {
        var cert = await _context.Certifications.FindAsync(CertId);
        if (cert != null && !string.IsNullOrWhiteSpace(CertName))
        {
            cert.Name = CertName;
            cert.AgencyId = CertAgencyId;
            cert.ValidityPeriodMonths = CertValidity;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateCertificationAsync()
    {
        var cert = await _context.Certifications.FindAsync(CertId);
        if (cert != null)
        {
            _context.Certifications.Remove(cert);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
    }
}
