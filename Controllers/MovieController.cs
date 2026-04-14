using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using OpenAI;
using OpenAI.Embeddings;

[ApiController]
[Route("api")]
public class MovieController : ControllerBase
{
    private readonly IConfiguration _config;

    public MovieController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        var movies = new[]
        {
            new { Title = "Inception", Genre = "Sci-Fi", Description = "A thief who steals corporate secrets through dream-sharing technology." },
            new { Title = "The Godfather", Genre = "Crime", Description = "The aging patriarch of an organized crime dynasty transfers control to his son." },
            new { Title = "Interstellar", Genre = "Sci-Fi", Description = "A team of explorers travel through a wormhole in space to save humanity." },
            new { Title = "The Dark Knight", Genre = "Action", Description = "Batman faces the Joker, a criminal mastermind who wreaks havoc on Gotham." },
            new { Title = "Forrest Gump", Genre = "Drama", Description = "The life journey of a man with a low IQ but pure heart across decades of American history." }
        };

        var connectionString = _config.GetConnectionString("Default");
        var apiKey = _config["OpenAI:ApiKey"];
        var client = new OpenAIClient(apiKey);
        var embeddingClient = client.GetEmbeddingClient("text-embedding-3-small");
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            CREATE EXTENSION IF NOT EXISTS vector;

            DROP TABLE IF EXISTS movies;

            CREATE TABLE movies (
                id SERIAL PRIMARY KEY,
                title TEXT NOT NULL,
                genre TEXT NOT NULL,
                description TEXT NOT NULL,
                embedding VECTOR(1536)
            );
            """);

        foreach (var movie in movies)
        {
            var text = $"{movie.Title} {movie.Genre} {movie.Description}";

            var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(text);
            var vector = embeddingResult.Value.ToFloats().ToArray();

            await connection.ExecuteAsync(
                "INSERT INTO movies (title, genre, description, embedding) VALUES (@Title, @Genre, @Description, @Embedding)",
                new
                {
                    movie.Title,
                    movie.Genre,
                    movie.Description,
                    Embedding = vector
                });
        }

        return Ok(new { seededCount = movies.Length });
    }

    [HttpPost("recommend")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var embeddingClient = new OpenAIClient(apiKey).GetEmbeddingClient("text-embedding-3-small");

        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(request.Query);
        var vector = embeddingResult.Value.ToFloats().ToArray();

        var connectionString = _config.GetConnectionString("Default");
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var movies = await connection.QueryAsync(
            """
            SELECT title, genre, description,
                   1 - (embedding <=> CAST(@Embedding AS vector)) AS similarity
            FROM movies
            ORDER BY embedding <-> CAST(@Embedding AS vector)
            LIMIT 3
            """,
            new { Embedding = vector });

        if (!movies.Any())
            return NotFound(new { message = "No movies found. Please seed the database first." });

        return Ok(movies);
    }
}

public record RecommendRequest(string Query);
