import React, { useState } from 'react';
import { authService } from '../../services/api';

const Login = ({ onSwitch, onLoginSuccess }) => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            const data = await authService.login(email, password);
            if (data.success) {
                onLoginSuccess();
            } else {
                setError(data.message || 'Login failed');
            }
        } catch (err) {
            setError('Invalid credentials or server error');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="glass-card" style={{ maxWidth: '400px', width: '100%' }}>
            <h2 className="text-center">Welcome Back</h2>
            <p className="text-center text-muted mb-4">Please enter your details</p>

            {error && <p style={{ color: 'var(--error)', marginBottom: '1rem', textAlign: 'center' }}>{error}</p>}

            <form onSubmit={handleSubmit}>
                <div className="mb-4">
                    <label className="text-muted" style={{ display: 'block', marginBottom: '0.5rem' }}>Email</label>
                    <input
                        type="email"
                        placeholder="name@company.com"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>
                <div className="mb-4">
                    <label className="text-muted" style={{ display: 'block', marginBottom: '0.5rem' }}>Password</label>
                    <input
                        type="password"
                        placeholder="••••••••"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" className="btn-primary" disabled={loading}>
                    {loading ? 'Signing in...' : 'Sign In'}
                </button>
            </form>

            <p className="text-center mt-4">
                Don't have an account? {' '}
                <span className="text-primary cursor-pointer" onClick={onSwitch}>Sign up</span>
            </p>
        </div>
    );
};

export default Login;
