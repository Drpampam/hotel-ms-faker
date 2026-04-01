import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

// Initialize theme from storage before first render to avoid flash
const storedTheme = localStorage.getItem('hotel-ms-theme');
if (storedTheme) {
  try {
    const parsed = JSON.parse(storedTheme) as { state?: { theme?: string } };
    if (parsed?.state?.theme === 'dark') {
      document.documentElement.classList.add('dark');
    }
  } catch {
    // ignore
  }
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
