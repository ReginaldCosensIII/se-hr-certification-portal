using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SeHrCertificationPortal.Pages.Admin
{
    public class SettingsModel : PageModel
    {
    private readonly SeHrCertificationPortal.Data.ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public SettingsModel(SeHrCertificationPortal.Data.ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
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
    public string? AgencyAbbreviation { get; set; }

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
        if (agency != null && !string.IsNullOrWhiteSpace(AgencyName) && !string.IsNullOrWhiteSpace(AgencyAbbreviation))
        {
            agency.FullName = AgencyName;
            agency.Abbreviation = AgencyAbbreviation;
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
            cert.IsActive = false;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReactivateAgencyAsync()
    {
        var agency = await _context.Agencies.FindAsync(AgencyId);
        if (agency != null)
        {
            agency.IsActive = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReactivateCertificationAsync()
    {
        var cert = await _context.Certifications.FindAsync(CertId);
        if (cert != null)
        {
            cert.IsActive = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportListAsync()
    {
        var agencies = await _context.Agencies
            .Include(a => a.Certifications)
            .OrderBy(a => a.Abbreviation)
            .ToListAsync();

        var logoPath = Path.Combine(_env.WebRootPath, "img", "branding-assets", "Specialized-Engineering-Logo-white.webp");
        byte[]? logoBytes = System.IO.File.Exists(logoPath) ? await System.IO.File.ReadAllBytesAsync(logoPath) : null;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                // Brand Header
                page.Header().Background("#66615c").Padding(10).Row(row =>
                {
                    if (logoBytes != null)
                    {
                        row.ConstantItem(150).Image(logoBytes);
                    }
                    row.RelativeItem().AlignRight().AlignMiddle().Text("Agency & Certification Roster").FontColor(Colors.White).FontSize(16).SemiBold();
                });

                // Content
                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    foreach (var agency in agencies)
                    {
                        // Agency Group Header
                        col.Item().PaddingTop(10).PaddingBottom(5).Text($"{agency.FullName} ({agency.Abbreviation})")
                            .FontSize(14).SemiBold().FontColor("#a19482");

                        if (!agency.Certifications.Any())
                        {
                            col.Item().PaddingBottom(15).Text("No certifications currently linked.").Italic().FontColor(Colors.Grey.Medium);
                            continue;
                        }

                        // Certifications Table
                        col.Item().PaddingBottom(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Name
                                columns.RelativeColumn(1); // Validity
                                columns.RelativeColumn(1); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Certification Name").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Validity Period").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Status").SemiBold();
                            });

                            foreach (var cert in agency.Certifications)
                            {
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.Name);
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.ValidityPeriodMonths == 0 ? "Permanent" : $"{cert.ValidityPeriodMonths} Months");
                                table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(cert.IsActive ? "Active" : "Inactive");
                            }
                        });
                    }
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        byte[] pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"SPE_Admin_Export_{DateTime.Now:yyyyMMdd}.pdf");
    }
    }
}
