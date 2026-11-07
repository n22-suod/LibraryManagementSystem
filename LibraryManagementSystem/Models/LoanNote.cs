namespace LibraryManagementSystem.Models
{
    public class LoanNote
    {
        public int Id { get; set; }

        public int BookLoanId { get; set; }

        public BookLoan? BookLoan { get; set; }

        public string NoteText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Author { get; set; } = string.Empty;
    }
}