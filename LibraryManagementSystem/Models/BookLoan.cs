namespace LibraryManagementSystem.Models
{
    public enum LoanStatus
    {
        Requested,
        Borrowed,
        Returned
    }

    public class BookLoan
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public int BookId { get; set; }

        public Book? Book { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public LoanStatus Status { get; set; } = LoanStatus.Requested;
    }
}