namespace LibraryManagementSystem.Models
{
    // Represents a book in the library
    public class Book
    {
        // Unique identifier for each book
        public int Id { get; set; }

        // Book title
        public string Title { get; set; } = string.Empty;

        // Book author
        public string Author { get; set; } = string.Empty;

        // Total number of copies
        public int TotalCopies { get; set; }

        // Number of available copies
        public int AvailableCopies { get; set; }

        // Indicates whether the book is available
        public bool IsAvailable { get; set; }
    }
}