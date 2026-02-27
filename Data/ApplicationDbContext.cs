using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Models;

namespace SeHrCertificationPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<CertificationRequest> CertificationRequests { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
    }
}
