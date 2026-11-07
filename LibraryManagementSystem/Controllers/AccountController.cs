using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] =
                    "Please enter your username and password.";

                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                TempData["LoginError"] = "Invalid username or password.";
                return RedirectToAction("Index", "Home");
            }

            var passwordHasher = new PasswordHasher<User>();

            var result = passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                TempData["LoginError"] = "Invalid username or password.";
                return RedirectToAction("Index", "Home");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Dashboard", "Account");
        }

        // Dashboard
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        // Create Account - Display Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Create Account - Save User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please fill in all fields.";
                return View();
            }

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == username);

            if (usernameExists)
            {
                ViewBag.Error = "Username already exists.";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                Role = UserRole.RegularUser
            };

            var passwordHasher = new PasswordHasher<User>();

            newUser.PasswordHash =
                passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            TempData["RegisterSuccess"] =
                "Account created successfully. Please login.";

            return RedirectToAction("Index", "Home");
        }

        // Logout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}