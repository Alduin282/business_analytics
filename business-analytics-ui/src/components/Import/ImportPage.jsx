import React, { useState, useRef } from 'react';
import { ordersService } from '../../services/api';
import ImportHistory from './ImportHistory';

const ImportPage = ({ onBack, onImportSuccess }) => {
    const [file, setFile] = useState(null);
    const [uploading, setUploading] = useState(false);
    const [result, setResult] = useState(null);
    const [error, setError] = useState(null);
    const [dragActive, setDragActive] = useState(false);
    const fileInputRef = useRef(null);
    const historyRef = useRef(null);

    const handleDrag = (e) => {
        e.preventDefault();
        e.stopPropagation();
        if (e.type === 'dragenter' || e.type === 'dragover') {
            setDragActive(true);
        } else if (e.type === 'dragleave') {
            setDragActive(false);
        }
    };

    const handleDrop = (e) => {
        e.preventDefault();
        e.stopPropagation();
        setDragActive(false);
        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            setFile(e.dataTransfer.files[0]);
            setResult(null);
            setError(null);
        }
    };

    const handleChange = (e) => {
        e.preventDefault();
        if (e.target.files && e.target.files[0]) {
            setFile(e.target.files[0]);
            setResult(null);
            setError(null);
        }
    };

    const handleUpload = async () => {
        if (!file) return;

        setUploading(true);
        setResult(null);
        setError(null);

        try {
            const data = await ordersService.importOrders(file);
            setResult(data);
            if (onImportSuccess) onImportSuccess();
            if (historyRef.current) historyRef.current.refresh();
        } catch (err) {
            console.error('Import failed:', err);
            if (err.response && err.response.data) {
                setResult(err.response.data);
            } else {
                setError('Failed to upload file. Please try again.');
            }
        } finally {
            setUploading(false);
        }
    };

    const onButtonClick = () => {
        fileInputRef.current.click();
    };

    return (
        <div className="glass-card" style={{ maxWidth: '800px', margin: '0 auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <div>
                    <h2>Import Orders</h2>
                    <p className="text-muted">Upload a CSV file to import your business data.</p>
                </div>
                <button onClick={onBack} className="btn-ghost" style={{ width: 'auto' }}>
                    &larr; Back to Dashboard
                </button>
            </div>

            {!result || result.errors.length > 0 ? (
                <div
                    className={`drop-zone ${dragActive ? 'active' : ''}`}
                    onDragEnter={handleDrag}
                    onDragLeave={handleDrag}
                    onDragOver={handleDrag}
                    onDrop={handleDrop}
                    onClick={onButtonClick}
                >
                    <input
                        ref={fileInputRef}
                        type="file"
                        accept=".csv"
                        onChange={handleChange}
                        style={{ display: 'none' }}
                    />
                    <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ marginBottom: '1rem', color: 'var(--primary)' }}>
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                        <polyline points="17 8 12 3 7 8"></polyline>
                        <line x1="12" y1="3" x2="12" y2="15"></line>
                    </svg>
                    <p style={{ fontWeight: 600, marginBottom: '0.5rem' }}>
                        {file ? file.name : 'Click to upload or drag and drop'}
                    </p>
                    <p className="text-muted" style={{ fontSize: '0.875rem' }}>Only .csv files are supported</p>
                </div>
            ) : null}

            {file && !result && (
                <button
                    onClick={handleUpload}
                    className="btn-primary"
                    disabled={uploading}
                    style={{ marginBottom: '2rem' }}
                >
                    {uploading ? (
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.5rem' }}>
                            <div className="spinner" style={{ width: '16px', height: '16px', borderWidth: '2px' }}></div>
                            Uploading...
                        </div>
                    ) : 'Start Import'}
                </button>
            )}

            {error && (
                <div className="glass-card" style={{ background: 'rgba(239, 68, 68, 0.1)', border: '1px solid var(--error)', marginBottom: '2rem' }}>
                    <p className="text-error">{error}</p>
                </div>
            )}

            {result && (
                <div className="mt-4">
                    {result.success ? (
                        <div className="glass-card" style={{ background: 'rgba(16, 185, 129, 0.1)', border: '1px solid #10b981', marginBottom: '2rem' }}>
                            <h3 className="text-success">Import Successful!</h3>
                            <p>Successfully imported <strong>{result.ordersCount}</strong> orders with <strong>{result.itemsCount}</strong> items.</p>
                            <button onClick={() => { setResult(null); setFile(null); }} className="btn-primary" style={{ marginTop: '1rem', width: 'auto' }}>
                                Import Another File
                            </button>
                        </div>
                    ) : (
                        <div className="glass-card" style={{ background: 'rgba(239, 68, 68, 0.1)', border: '1px solid var(--error)', marginBottom: '2rem' }}>
                            <h3 className="text-error">Import Failed</h3>
                            <p>The file failed validation. No data was imported.</p>

                            <div className="table-container">
                                <table className="error-table">
                                    <thead>
                                        <tr>
                                            <th>Row</th>
                                            <th>Column</th>
                                            <th>Error</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {result.errors.map((err, idx) => (
                                            <tr key={idx}>
                                                <td><span className="badge badge-error">{err.rowNumber}</span></td>
                                                <td><strong>{err.column}</strong></td>
                                                <td className="text-error">{err.message}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>

                            <button onClick={() => { setResult(null); }} className="btn-primary" style={{ marginTop: '1.5rem', width: 'auto' }}>
                                Try Again
                            </button>
                        </div>
                    )}
                </div>
            )}

            <div className="mt-4" style={{ padding: '1rem', borderTop: '1px solid var(--glass-border)' }}>
                <h4>CSV Format Rules:</h4>
                <ul className="text-muted" style={{ fontSize: '0.875rem', marginTop: '0.5rem', marginLeft: '1.5rem' }}>
                    <li>Required columns: <strong>OrderDate, CustomerName, CustomerEmail, ProductName, CategoryName, Quantity, UnitPrice, Status</strong></li>
                    <li>Date format: <strong>YYYY-MM-DD HH:MM</strong></li>
                    <li>Group by: Orders are grouped by <strong>OrderDate + CustomerEmail</strong></li>
                    <li>Status: Must be one of <strong>Pending, Processing, Shipped, Delivered, Cancelled</strong></li>
                </ul>
            </div>

            <ImportHistory ref={historyRef} />
        </div>
    );
};

export default ImportPage;
