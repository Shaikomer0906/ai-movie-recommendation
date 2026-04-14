# 🎬 AI Movie Recommender

A full-stack AI-powered movie recommendation app that uses **OpenAI embeddings** and **pgvector** to find semantically similar movies based on a natural language query.

---

## 📌 Project Overview

Users describe what kind of movie they're in the mood for (e.g. *"space adventure with a twist ending"*), and the app returns the top 3 most semantically relevant movies from a PostgreSQL database — ranked by vector similarity using pgvector.

---

## 🛠️ Technologies Used

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 10 Web API |
| Frontend | React + Vite |
| Database | PostgreSQL (Neon serverless) |
| Vector Search | pgvector extension |
| AI Embeddings | OpenAI `text-embedding-3-small` |
| ORM / Query | Dapper + Npgsql |

---

## ⚙️ How It Works

1. **Seeding** — The `/api/seed` endpoint inserts 5 movies into the database. For each movie, it generates a 1536-dimension vector embedding using OpenAI and stores it in a `VECTOR(1536)` column.

2. **Recommending** — The `/api/recommend` endpoint accepts a natural language query, generates an embedding for it using the same OpenAI model, then queries PostgreSQL using pgvector's `<->` distance operator to find the 3 closest matching movies by cosine similarity.

```
Query text → OpenAI embedding → pgvector similarity search → Top 3 movies
```

---

## 🚀 Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- A [Neon](https://neon.tech) PostgreSQL database with the `pgvector` extension enabled
- An [OpenAI API key](https://platform.openai.com/api-keys)

### 1. Clone the repository

```bash
git clone https://github.com/your-username/MovieApi.git
cd MovieApi
```

### 2. Configure the backend

Edit `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "Default": "Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
  },
  "OpenAI": {
    "ApiKey": "sk-..."
  }
}
```

### 3. Run the backend

```bash
dotnet run
```

API will be available at `http://localhost:5142`  
Swagger UI at `http://localhost:5142/swagger`

### 4. Run the frontend

```bash
cd movie-ui
npm install
npm run dev
```

Frontend will be available at `http://localhost:5173`

---

## 📡 API Endpoints

### `POST /api/seed`

Seeds the database with 5 movies and their OpenAI embeddings. Creates the `movies` table if it doesn't exist.

**Response:**
```json
{ "seededCount": 5 }
```

---

### `POST /api/recommend`

Returns the top 3 most semantically similar movies for a given query.

**Request body:**
```json
{ "query": "space adventure with a twist ending" }
```

**Response:**
```json
[
  {
    "title": "Interstellar",
    "genre": "Sci-Fi",
    "description": "A team of explorers travel through a wormhole in space to save humanity.",
    "similarity": 0.87
  },
  ...
]
```

---

## 📁 Project Structure

```
MovieApi/
├── Controllers/
│   └── MovieController.cs   # Seed and Recommend endpoints
├── Program.cs                # Service registration, CORS, Swagger
├── appsettings.json          # DB connection string and OpenAI key
└── movie-ui/                 # React + Vite frontend
    └── src/
        ├── App.jsx
        └── App.css
```

---

## 🔒 Security Note

Do not commit `appsettings.json` with real credentials to source control. Use environment variables or [.NET user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in production.

Add to `.gitignore`:
```
appsettings.json
```
