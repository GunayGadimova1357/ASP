using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcIdentity.Data;

namespace MvcIdentity.Controllers;

[Authorize]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;

    public BooksController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? searchString, string? sortOrder)
    {
        var booksQuery = _db.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            booksQuery = booksQuery.Where(b => EF.Functions.Like(b.Title, $"%{searchString}%"));
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["YearSort"] = sortOrder == "year_desc" ? "year_asc" : "year_desc";
        ViewData["YearSortLabel"] = sortOrder == "year_desc" ? "Year ↓" : "Year ↑";

        booksQuery = sortOrder switch
        {
            "year_desc" => booksQuery.OrderByDescending(b => b.Year).ThenBy(b => b.Title),
            "year_asc" => booksQuery.OrderBy(b => b.Year).ThenBy(b => b.Title),
            _ => booksQuery.OrderBy(b => b.Title)
        };

        var books = await booksQuery.ToListAsync();
        return View(books);
    }
}
