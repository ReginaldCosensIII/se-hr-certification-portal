namespace SeHrCertificationPortal.Models
{
    public class CertificationRequest
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string CertificationName { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
    }

    public class Certification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Agency { get; set; }
        public int ValidityPeriodMonths { get; set; }
    }

    public class Agency
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }
}
