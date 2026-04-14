import { useState } from 'react'
import './App.css'

export default function App() {
  const [query, setQuery] = useState('')
  const [movies, setMovies] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  async function handleSearch() {
    if (!query.trim()) return
    setLoading(true)
    setError(null)
    setMovies([])

    try {
      const res = await fetch('http://localhost:5142/api/recommend', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ query }),
      })

      if (!res.ok) {
        const err = await res.json().catch(() => ({}))
        throw new Error(err.message || `Error ${res.status}`)
      }

      const data = await res.json()
      setMovies(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter') handleSearch()
  }

  return (
    <div className="app">
      <h1>🎬 Movie Recommender</h1>
      <p className="subtitle">Describe what you're in the mood for</p>

      <div className="search-row">
        <input
          type="text"
          placeholder="e.g. space adventure, crime drama..."
          value={query}
          onChange={e => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={loading}
        />
        <button onClick={handleSearch} disabled={loading || !query.trim()}>
          {loading ? 'Searching...' : 'Find Movies'}
        </button>
      </div>

      {loading && (
        <div className="spinner-wrapper">
          <div className="spinner" />
        </div>
      )}

      {error && <div className="error">⚠️ {error}</div>}

      {movies.length > 0 && (
        <div className="results">
          {movies.map((movie, i) => (
            <div className="card" key={i}>
              <div className="card-header">
                <span className="card-genre">{movie.genre}</span>
                <span className="card-score">
                  {(movie.similarity * 100).toFixed(0)}% match
                </span>
              </div>
              <h2 className="card-title">{movie.title}</h2>
              <p className="card-description">{movie.description}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
