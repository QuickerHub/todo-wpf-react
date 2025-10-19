import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import './App.css'

function App() {
  return (
    <BrowserRouter>
      <div className="app">
        <nav className="navbar">
          <Link to="/" className="nav-link">Home</Link>
          <Link to="/about" className="nav-link">About</Link>
        </nav>
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/about" element={<About />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}

function Home() {
  return (
    <div>
      <h1>Welcome to TodoApp!</h1>
      <p>This is a React app running inside a WPF WebView2 control.</p>
      <div className="demo-section">
        <h2>API Demo</h2>
        <button onClick={testApi} className="demo-button">
          Test API Connection
        </button>
        <div id="api-result"></div>
      </div>
    </div>
  )
}

function About() {
  return (
    <div>
      <h1>About TodoApp</h1>
      <p>This is a hybrid desktop application built with:</p>
      <ul>
        <li>WPF for the desktop container</li>
        <li>WebView2 for modern web rendering</li>
        <li>ASP.NET Core for the backend API</li>
        <li>React for the frontend UI</li>
      </ul>
    </div>
  )
}

async function testApi() {
  try {
    const response = await fetch('/api/demo')
    const data = await response.json()
    const resultDiv = document.getElementById('api-result')
    if (resultDiv) {
      resultDiv.innerHTML = `<p style="color: green;">✅ ${data.message}</p>`
    }
  } catch (error) {
    const resultDiv = document.getElementById('api-result')
    if (resultDiv) {
      resultDiv.innerHTML = `<p style="color: red;">❌ API connection failed: ${error}</p>`
    }
  }
}

export default App
