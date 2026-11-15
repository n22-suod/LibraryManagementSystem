using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display the book catalog with search and sorting
        public async Task<IActionResult> Catalog(
            string? searchString,
            string? sortOrder)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString));
            }

            books = sortOrder switch
            {
                "title" => books.OrderBy(b => b.Title),
                "author" => books.OrderBy(b => b.Author),
                _ => books.OrderBy(b => b.Id)
            };

            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;

            return View(await books.ToListAsync());
        }

        // Submit a book loan request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLoan(int bookId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Challenge();

            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
                return NotFound();

            if (!book.IsAvailable || book.AvailableCopies <= 0)
            {
                TempData["Message"] =
                    "This book is not available.";

                return RedirectToAction(nameof(Catalog));
            }

            // Prevent duplicate active requests by the same user
            var existingRequest = await _context.BookLoans.AnyAsync(l =>
                l.UserId == userId &&
                l.BookId == bookId &&
                (l.Status == LoanStatus.Requested ||
                 l.Status == LoanStatus.Borrowed));

            if (existingRequest)
            {
                TempData["Message"] =
                    "You already have an active request for this book.";

                return RedirectToAction(nameof(Catalog));
            }

            // Check how many requests are waiting for this book
            var pendingRequests = await _context.BookLoans.CountAsync(l =>
                l.BookId == bookId &&
                l.Status == LoanStatus.Requested);

            if (pendingRequests >= book.AvailableCopies)
            {
                TempData["Message"] =
                    "There are no available copies for new requests.";

                return RedirectToAction(nameof(Catalog));
            }

            var now = DateTime.Now;

            var loan = new BookLoan
            {
                UserId = userId,
                BookId = bookId,
                RequestDate = now,
                DueDate = now.AddDays(14),
                Status = LoanStatus.Requested
            };

            _context.BookLoans.Add(loan);
            await _context.SaveChangesAsync();

            TempData["Message"] =
                "Your loan request has been submitted.";

            return RedirectToAction(nameof(MyLoans));
        }

        // Display the current user's loan requests
        public async Task<IActionResult> MyLoans()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Challenge();

            var loans = await _context.BookLoans
                .Include(l => l.Book)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            return View(loans);
        }
    }
}