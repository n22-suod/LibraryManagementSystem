using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/Index";
    });
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate();

    var passwordHasher = new PasswordHasher<User>();

    // Add default users
    if (!context.Users.Any())
    {
        var admin = new User
        {
            Username = "admin",
            Role = UserRole.Admin
        };

        admin.PasswordHash =
            passwordHasher.HashPassword(admin, "Admin123!");

        var student = new User
        {
            Username = "Rawabi",
            Role = UserRole.RegularUser
        };

        student.PasswordHash =
            passwordHasher.HashPassword(student, "Student123!");

        context.Users.AddRange(admin, student);
    }

    // Add default books
    if (!context.Books.Any())
    {
        context.Books.AddRange(
            new Book
            {
                Title = "C# Programming",
                Author = "John Doe",
                TotalCopies = 5,
                AvailableCopies = 5,
                IsAvailable = true
            },
            new Book
            {
                Title = "Database Systems",
                Author = "Jane Smith",
                TotalCopies = 3,
                AvailableCopies = 3,
                IsAvailable = true
            }
        );
    }

    context.SaveChanges();
}

app.Run();