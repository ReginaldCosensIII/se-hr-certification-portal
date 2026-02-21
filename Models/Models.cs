namespace SeHrCertificationPortal.Models
{
    public enum RequestType { ReviewSession, WrittenExam, PracticalExam, Reciprocity, Recertification, Other }
    public enum RequestStatus { Pending, Approved, Rejected, Passed, Failed }

    public class Agency
    {
        public int Id { get; set; }
        public required string Abbreviation { get; set; }
        public required string FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    }

    public class Certification
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int ValidityPeriodMonths { get; set; }
        public bool IsActive { get; set; } = true;
        public int AgencyId { get; set; }
        public Agency? Agency { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
        public ICollection<CertificationRequest> CertificationRequests { get; set; } = new List<CertificationRequest>();
    }

    public class CertificationRequest
    {
        public int Id { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public string? ManagerName { get; set; }
        public RequestType? RequestType { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime? ExpirationDate { get; set; }
        public string? CustomAgencyName { get; set; }
        public string? CustomCertificationName { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }
        public int? CertificationId { get; set; }
        public Certification? Certification { get; set; }
    }
}
