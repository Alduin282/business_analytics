import React, { useState, useEffect, useCallback, useImperativeHandle, forwardRef } from 'react';
import { ordersService } from '../../services/api';

const ImportHistory = forwardRef((props, ref) => {
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const fetchHistory = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await ordersService.getImportHistory();
            setHistory(data);
        } catch (err) {
            console.error('Failed to fetch import history:', err);
            setError('Failed to load import history.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchHistory();
    }, [fetchHistory]);

    useImperativeHandle(ref, () => ({
        refresh: fetchHistory
    }));

    const formatDate = (dateString) => {
        const options = {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleDateString(undefined, options);
    };

    if (loading && history.length === 0) {
        return (
            <div style={{ padding: '2rem', textAlign: 'center' }}>
                <div className="spinner" style={{ margin: '0 auto' }}></div>
                <p className="text-muted mt-2">Loading history...</p>
            </div>
        );
    }

    return (
        <div className="mt-5">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                <h3>Import History</h3>
                <button
                    onClick={fetchHistory}
                    className="btn-ghost"
                    style={{ width: 'auto', padding: '0.5rem' }}
                    title="Refresh history"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"></path><path d="M3 3v5h5"></path></svg>
                </button>
            </div>

            {error && (
                <div className="glass-card" style={{ background: 'rgba(239, 68, 68, 0.1)', border: '1px solid var(--error)', marginBottom: '1rem', padding: '1rem' }}>
                    <p className="text-error" style={{ margin: 0 }}>{error}</p>
                </div>
            )}

            {history.length === 0 ? (
                <div className="glass-card" style={{ textAlign: 'center', padding: '3rem', background: 'rgba(255,255,255,0.02)' }}>
                    <p className="text-muted">No import sessions found. Start by uploading a CSV file.</p>
                </div>
            ) : (
                <div className="table-container">
                    <table className="error-table">
                        <thead>
                            <tr>
                                <th style={{ width: '40%' }}>File Name</th>
                                <th style={{ width: '30%' }}>Import Date</th>
                                <th style={{ textAlign: 'right', paddingRight: '1.5rem' }}>Orders</th>
                                <th style={{ textAlign: 'right' }}>Items</th>
                            </tr>
                        </thead>
                        <tbody>
                            {history.map((session) => (
                                <tr key={session.id}>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--primary)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path><polyline points="14 2 14 8 20 8"></polyline></svg>
                                            <span style={{ fontWeight: 500 }}>{session.fileName}</span>
                                        </div>
                                    </td>
                                    <td>{formatDate(session.importedAt)}</td>
                                    <td style={{ textAlign: 'right', paddingRight: '1.5rem' }}>
                                        <span className="badge badge-primary" style={{ minWidth: '32px', display: 'inline-block', textAlign: 'center' }}>
                                            {session.ordersCount}
                                        </span>
                                    </td>
                                    <td style={{ textAlign: 'right' }}>
                                        <span className="badge" style={{
                                            background: 'rgba(34, 211, 238, 0.2)',
                                            color: 'var(--accent)',
                                            minWidth: '32px',
                                            display: 'inline-block',
                                            textAlign: 'center'
                                        }}>
                                            {session.itemsCount}
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
});

export default ImportHistory;
