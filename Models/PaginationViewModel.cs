namespace SeHrCertificationPortal.Models
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string PageAction { get; set; } = "./Index";
        public Dictionary<string, string> RouteParams { get; set; } = new Dictionary<string, string>();
    }
}
