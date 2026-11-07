using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Book> Books { get; set; } = null!;

        public DbSet<BookLoan> BookLoans { get; set; } = null!;

        public DbSet<LoanNote> LoanNotes { get; set; } = null!;

        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    }
}
