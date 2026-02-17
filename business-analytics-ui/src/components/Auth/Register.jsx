import React, { useState } from 'react';
import { authService } from '../../services/api';

const Register = ({ onSwitch }) => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [message, setMessage] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (password !== confirmPassword) {
            setError('Passwords do not match');
            return;
        }

        setLoading(true);
        setError('');
        setMessage('');
        try {
            const data = await authService.register(email, password);
            if (data.success) {
                setMessage('Registration successful! You can now login.');
                setTimeout(onSwitch, 2000);
            } else {
                setError(data.message || 'Registration failed');
            }
        } catch (err) {
            setError('Registration failed. Check password requirements.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="glass-card" style={{ maxWidth: '400px', width: '100%' }}>
            <h2 className="text-center">Create Account</h2>
            <p className="text-center text-muted mb-4">Start your analytics journey</p>

            {error && <p style={{ color: 'var(--error)', marginBottom: '1rem', textAlign: 'center' }}>{error}</p>}
            {message && <p style={{ color: 'var(--accent)', marginBottom: '1rem', textAlign: 'center' }}>{message}</p>}

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
                <div className="mb-4">
                    <label className="text-muted" style={{ display: 'block', marginBottom: '0.5rem' }}>Confirm Password</label>
                    <input
                        type="password"
                        placeholder="••••••••"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" className="btn-primary" disabled={loading}>
                    {loading ? 'Creating account...' : 'Sign Up'}
                </button>
            </form>

            <p className="text-center mt-4">
                Already have an account? {' '}
                <span className="text-primary cursor-pointer" onClick={onSwitch}>Log in</span>
            </p>
        </div>
    );
};

export default Register;
