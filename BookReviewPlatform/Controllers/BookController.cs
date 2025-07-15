using BookReviewPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BookReviewPlatform.Data;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace BookReviewPlatform.Controllers
{
    [Authorize] // Only authenticated users can see books
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.Include(b => b.Reviews).ToListAsync();

            var userId = _userManager.GetUserId(User);
            var userReactions = await _context.BookReactions
                .Where(r => r.UserId == userId)
                .ToListAsync();

            ViewBag.UserReactions = userReactions;

            return View(books);
        }


        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create(with file upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book, IFormFile image)
        {
            // Log book details for debugging
            Console.WriteLine($"Book Title: {book.Title}, Author: {book.Author}");

            if (image != null)
            {
                try
                {
                    // Generate a unique filename for the image
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    // Define the image file path in the "wwwroot/images" folder
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    // Combine the images path with the unique filename
                    var filePath = Path.Combine(imagesPath, uniqueFileName);

                    // Save the image file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Save the relative image path in the book object
                    book.ImagePath = "/images/" + uniqueFileName; // Relative URL
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log error if any
                    Console.WriteLine($"Error saving book: {ex.Message}");
                }
            }

            // Return to the view with validation errors if the model is invalid
            return View(book);
        }


        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(b => b.Id == book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }


        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // POST: Books/AddReview
        [HttpPost]
        public async Task<IActionResult> AddReview(int bookId, string comment)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var review = new Review
            {
                Comment = comment,
                UserId = user.Id,
                BookId = bookId
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Books/DeleteReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId, int bookId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        // POST: Books/Like/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var book = await _context.Books.Include(b => b.Reactions).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var reaction = await _context.BookReactions.FirstOrDefaultAsync(r => r.BookId == id && r.UserId == userId);

            if (reaction == null)
            {
                // New like
                book.Likes++;
                _context.BookReactions.Add(new BookReaction { BookId = id, UserId = userId, IsLike = true });
            }
            else if (!reaction.IsLike)
            {
                // Switch from dislike to like
                book.Dislikes--;
                book.Likes++;
                reaction.IsLike = true;
                _context.BookReactions.Update(reaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        // POST: Books/Dislike/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dislike(int id)
        {
            var book = await _context.Books.Include(b => b.Reactions).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var reaction = await _context.BookReactions.FirstOrDefaultAsync(r => r.BookId == id && r.UserId == userId);

            if (reaction == null)
            {
                // New dislike
                book.Dislikes++;
                _context.BookReactions.Add(new BookReaction { BookId = id, UserId = userId, IsLike = false });
            }
            else if (reaction.IsLike)
            {
                // Switch from like to dislike
                book.Likes--;
                book.Dislikes++;
                reaction.IsLike = false;
                _context.BookReactions.Update(reaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}



