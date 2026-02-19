import React from 'react';
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    AreaChart,
    Area
} from 'recharts';

const AnalyticsChart = ({ data, metric = 'totalAmount', color = '#6366f1' }) => {
    const formatValue = (value) => {
        if (metric === 'totalAmount') {
            return new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: 'USD',
                maximumFractionDigits: 0
            }).format(value);
        }
        return value;
    };

    if (!data || data.length === 0) {
        return (
            <div style={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <p className="text-muted">No data available for the selected period.</p>
            </div>
        );
    }

    return (
        <div style={{ width: '100%', height: 500, marginTop: '2rem' }}>
            <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={data}>
                    <defs>
                        <linearGradient id="colorMetric" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor={color} stopOpacity={0.3} />
                            <stop offset="95%" stopColor={color} stopOpacity={0} />
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" vertical={false} />
                    <XAxis
                        dataKey="label"
                        stroke="rgba(255,255,255,0.5)"
                        fontSize={12}
                        tickLine={false}
                        axisLine={false}
                    />
                    <YAxis
                        stroke="rgba(255,255,255,0.5)"
                        fontSize={12}
                        tickLine={false}
                        axisLine={false}
                        tickFormatter={formatValue}
                    />
                    <Tooltip
                        contentStyle={{
                            backgroundColor: '#1e293b',
                            border: 'none',
                            borderRadius: '8px',
                            boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)'
                        }}
                        itemStyle={{ color: '#fff' }}
                        formatter={(value) => [formatValue(value), 'Revenue']}
                    />
                    <Area
                        type="monotone"
                        dataKey={metric}
                        stroke={color}
                        strokeWidth={3}
                        fillOpacity={1}
                        fill="url(#colorMetric)"
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    );
};

export default AnalyticsChart;
