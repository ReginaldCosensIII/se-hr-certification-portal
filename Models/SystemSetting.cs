using System.ComponentModel.DataAnnotations;

namespace SeHrCertificationPortal.Models
{
    public class SystemSetting
    {
        [Key]
        public required string Key { get; set; }
        public string? Value { get; set; }
    }
}
