using Bogus;
using Microsoft.EntityFrameworkCore;
using MvcIdentity.Models;

namespace MvcIdentity.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Books (
                Id INTEGER NOT NULL CONSTRAINT PK_Books PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Author TEXT NOT NULL,
                Year INTEGER NOT NULL,
                Genre TEXT NOT NULL
            );
            """);

        if (await db.Books.AnyAsync())
        {
            return;
        }

        var faker = new Faker<Book>("en")
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Author, f => f.Person.FullName)
            .RuleFor(b => b.Year, f => f.Date.Past(50).Year)
            .RuleFor(b => b.Genre, f => f.PickRandom("Fantasy", "Drama", "Horror", "Sci-Fi", "History"));

        var books = faker.Generate(50);
        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }
}
