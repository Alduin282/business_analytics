import React, { useState, useEffect } from 'react';
import Login from './components/Auth/Login';
import Register from './components/Auth/Register';
import { authService, ordersService } from './services/api';
import AnalyticsChart from './components/Dashboard/AnalyticsChart';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [showLogin, setShowLogin] = useState(true);
  const [checkingToken, setCheckingToken] = useState(true);
  const [analyticsData, setAnalyticsData] = useState([]);
  const [groupBy, setGroupBy] = useState('Month');
  const getDefaultDates = () => {
    const now = new Date();
    const end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    const start = new Date(now.getFullYear() - 1, now.getMonth(), 1);

    const formatDate = (date) => {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    };

    return {
      start: formatDate(start),
      end: formatDate(end)
    };
  };

  const initialDates = getDefaultDates();
  const [startDate, setStartDate] = useState(initialDates.start);
  const [endDate, setEndDate] = useState(initialDates.end);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const token = authService.getToken();
    if (token) {
      setIsAuthenticated(true);
    }
    setCheckingToken(false);
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      fetchAnalytics();
    }
  }, [isAuthenticated, groupBy, startDate, endDate]);

  const fetchAnalytics = async () => {
    setLoading(true);
    try {
      const data = await ordersService.getAnalytics({
        groupBy,
        startDate: startDate ? new Date(startDate).toISOString() : null,
        endDate: endDate ? new Date(endDate).toISOString() : null
      });
      setAnalyticsData(data);
    } catch (error) {
      console.error('Failed to fetch analytics:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    authService.logout();
    setIsAuthenticated(false);
  };

  const handleReset = () => {
    const defaults = getDefaultDates();
    setStartDate(defaults.start);
    setEndDate(defaults.end);
    setGroupBy('Month');
  };

  if (checkingToken) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        background: 'radial-gradient(circle at top left, #1e293b, #0f172a)',
        color: 'white'
      }}>
        <div className="spinner"></div>
        <p style={{ marginLeft: '1rem' }}>Checking session...</p>
      </div>
    );
  }

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
    <div className="container mt-4" style={{ width: '100%' }}>
      <nav style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '3rem' }}>
        <h1 style={{ color: 'var(--primary)', margin: 0 }}>BusinessAnalytics</h1>
        <button onClick={handleLogout} className="btn-primary" style={{ width: 'auto' }}>Logout</button>
      </nav>

      <div className="glass-card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: '2rem', flexWrap: 'wrap', gap: '1rem' }}>
          <div>
            <h2>Revenue Analytics</h2>
            <p className="text-muted">Track your performance over time.</p>
          </div>

          <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', flexWrap: 'wrap' }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
              <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginLeft: '0.5rem' }}>Start Date</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                style={{ marginBottom: 0, width: '160px', padding: '0.5rem' }}
              />
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
              <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginLeft: '0.5rem' }}>End Date</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                style={{ marginBottom: 0, width: '160px', padding: '0.5rem' }}
              />
            </div>

            <button
              onClick={handleReset}
              className="btn-ghost"
              style={{
                padding: '0.4rem 0.8rem',
                marginTop: 'auto',
                fontSize: '0.75rem',
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
                width: 'auto',
                borderRadius: '10px',
                border: '1px solid rgba(255,255,255,0.1)',
                height: '35px'
              }}
              title="Reset to default (1 year)"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"></path><path d="M3 3v5h5"></path></svg>
              Reset
            </button>

            <div className="btn-group" style={{ display: 'flex', gap: '0.25rem', background: 'rgba(255,255,255,0.05)', padding: '0.25rem', borderRadius: '8px', height: 'fit-content', marginTop: 'auto' }}>
              {['Day', 'Week', 'Month'].map((period) => (
                <button
                  key={period}
                  onClick={() => setGroupBy(period)}
                  className={groupBy === period ? 'btn-primary' : 'btn-ghost'}
                  style={{
                    padding: '0.4rem 0.8rem',
                    fontSize: '0.75rem',
                    width: 'auto',
                    background: groupBy === period ? 'var(--primary)' : 'transparent',
                    color: groupBy === period ? '#fff' : 'rgba(255,255,255,0.6)',
                    borderRadius: '6px'
                  }}
                >
                  {period}
                </button>
              ))}
            </div>
          </div>
        </div>

        {loading ? (
          <div style={{ height: 400, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <div className="spinner"></div> {/* Assume there's a spinner in CSS or just text */}
            <p>Loading analytics...</p>
          </div>
        ) : (
          <AnalyticsChart data={analyticsData} />
        )}

        <div className="mt-4" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
          <div className="glass-card" style={{ padding: '1.5rem', background: 'rgba(99, 102, 241, 0.1)' }}>
            <h3 style={{ fontSize: '1rem', color: 'var(--primary)' }}>Total Revenue</h3>
            <p style={{ fontSize: '1.5rem', fontWeight: 700 }}>
              ${analyticsData.reduce((sum, item) => sum + item.totalAmount, 0).toLocaleString()}
            </p>
          </div>
          <div className="glass-card" style={{ padding: '1.5rem', background: 'rgba(34, 211, 238, 0.1)' }}>
            <h3 style={{ fontSize: '1rem', color: 'var(--accent)' }}>Average/Period</h3>
            <p style={{ fontSize: '1.5rem', fontWeight: 700 }}>
              ${analyticsData.length > 0
                ? (analyticsData.reduce((sum, item) => sum + item.totalAmount, 0) / analyticsData.length).toLocaleString(undefined, { maximumFractionDigits: 0 })
                : 0}
            </p>
          </div>
          <div className="glass-card" style={{ padding: '1.5rem', background: 'rgba(239, 68, 68, 0.1)' }}>
            <h3 style={{ fontSize: '1rem', color: 'var(--error)' }}>Periods</h3>
            <p style={{ fontSize: '1.5rem', fontWeight: 700 }}>{analyticsData.length}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
