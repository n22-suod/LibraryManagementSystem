using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // Display the create form
        public IActionResult Create()
        {
            return View();
        }

        // Add a new book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Author,TotalCopies")] Book book,
             IFormFile? coverImage)
        {
            if (book.TotalCopies < 0)
            {
                ModelState.AddModelError(
                    "TotalCopies",
                    "Total copies cannot be negative.");
            }

            if (ModelState.IsValid)
            {
                if (coverImage != null && coverImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension) ||
                        coverImage.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("coverImage",
                            "Please upload a JPG or PNG image under 2 MB.");
                        return View(book);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "covers");

                    Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.CreateNew))
                    {
                        await coverImage.CopyToAsync(stream);
                    }

                    book.CoverImagePath = "/covers/" + fileName;
                }
                book.AvailableCopies = book.TotalCopies;
                book.IsAvailable = book.AvailableCopies > 0;

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // Display the edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // Update book details and stock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
           [Bind("Id,Title,Author,TotalCopies,AvailableCopies")]
           Book book,
           IFormFile? coverImage)

        {
            if (id != book.Id)
                return NotFound();

            if (book.TotalCopies < 0 ||
                book.AvailableCopies < 0 ||
                book.AvailableCopies > book.TotalCopies)
            {
                ModelState.AddModelError(
                    "",
                    "Please enter valid stock quantities.");
            }

            if (!ModelState.IsValid)
                return View(book);

            var existingBook =
                await _context.Books.FindAsync(id);

            if (existingBook == null)
                return NotFound();
            if (coverImage != null && coverImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension) ||
                    coverImage.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("coverImage",
                        "Please upload a JPG or PNG image under 2 MB.");
                    return View(book);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "covers");

                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.CreateNew))
                {
                    await coverImage.CopyToAsync(stream);
                }

                existingBook.CoverImagePath = "/covers/" + fileName;
            }
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.TotalCopies = book.TotalCopies;
            existingBook.AvailableCopies =
                book.AvailableCopies;
            existingBook.IsAvailable =
                existingBook.AvailableCopies > 0;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Display delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // Delete a book
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            bool hasLoans = await _context.BookLoans
                .AnyAsync(loan => loan.BookId == id);

            if (hasLoans)
            {
                ModelState.AddModelError(
                    "",
                    "Cannot delete a book with loan records.");

                return View("Delete", book);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(book.CoverImagePath))
            {
                var fileName = Path.GetFileName(book.CoverImagePath);
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "covers",
                    fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}