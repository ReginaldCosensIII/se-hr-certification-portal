namespace SeHrCertificationPortal.Models
{
    public class CertificationRequest
    {
        public int Id { get; set; }
        public required string EmployeeName { get; set; }
        public required string CertificationName { get; set; }
        public required string Status { get; set; }
        public DateTime RequestDate { get; set; }
    }

    public class Certification
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Agency { get; set; }
        public int ValidityPeriodMonths { get; set; }
    }

    public class Agency
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
    }
}
