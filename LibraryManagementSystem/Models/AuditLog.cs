namespace LibraryManagementSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int BookLoanId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}