import React, { useState, useEffect } from 'react';
import Login from './components/Auth/Login';
import Register from './components/Auth/Register';
import { authService } from './services/api';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [showLogin, setShowLogin] = useState(true);

  useEffect(() => {
    const token = authService.getToken();
    if (token) {
      setIsAuthenticated(true);
    }
  }, []);

  const handleLogout = () => {
    authService.logout();
    setIsAuthenticated(false);
  };

  if (!isAuthenticated) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        background: 'radial-gradient(circle at top left, #1e293b, #0f172a)'
      }}>
        {showLogin ? (
          <Login onSwitch={() => setShowLogin(false)} onLoginSuccess={() => setIsAuthenticated(true)} />
        ) : (
          <Register onSwitch={() => setShowLogin(true)} />
        )}
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <nav style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '3rem' }}>
        <h1 style={{ color: 'var(--primary)', margin: 0 }}>OrderAnalytics</h1>
        <button onClick={handleLogout} className="btn-primary" style={{ width: 'auto' }}>Logout</button>
      </nav>

      <div className="glass-card">
        <h2>Dashboard</h2>
        <p className="text-muted">Welcome to your data analytics workspace. Your skeleton is ready, and you can now start adding your custom analytics charts and data tables.</p>

        <div className="mt-4" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
          <div className="glass-card" style={{ padding: '1.5rem', background: 'rgba(99, 102, 241, 0.1)' }}>
            <h3 style={{ fontSize: '1rem', color: 'var(--primary)' }}>Active Orders</h3>
            <p style={{ fontSize: '1.5rem', fontWeight: 700 }}>1,284</p>
          </div>
          <div className="glass-card" style={{ padding: '1.5rem', background: 'rgba(34, 211, 238, 0.1)' }}>
            <h3 style={{ fontSize: '1rem', color: 'var(--accent)' }}>Revenue</h3>
            <p style={{ fontSize: '1.5rem', fontWeight: 700 }}>$45,200</p>
          </div>
          <div className="glass-card" style={{ padding: '1.5rem', background: 'rgba(239, 68, 68, 0.1)' }}>
            <h3 style={{ fontSize: '1rem', color: 'var(--error)' }}>Pending</h3>
            <p style={{ fontSize: '1.5rem', fontWeight: 700 }}>42</p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
