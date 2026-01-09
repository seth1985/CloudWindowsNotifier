import React from 'react';
import {
    AreaChart,
    Area,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer
} from 'recharts';

type DataPoint = {
    date: string;
    shown: number;
    clicked: number;
};

// Mock data generator since we don't have real time-series API yet
const generateMockData = (): DataPoint[] => {
    const data: DataPoint[] = [];
    const now = new Date();
    for (let i = 6; i >= 0; i--) {
        const d = new Date(now);
        d.setDate(d.getDate() - i);
        data.push({
            date: d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
            shown: Math.floor(Math.random() * 50) + 10,
            clicked: Math.floor(Math.random() * 20) + 5,
        });
    }
    return data;
};

export const TelemetryChart: React.FC = () => {
    const data = React.useMemo(() => generateMockData(), []);

    return (
        <div className="w-full h-[300px] mt-4">
            <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={data} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                    <defs>
                        <linearGradient id="colorShown" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="#3b7cff" stopOpacity={0.3} />
                            <stop offset="95%" stopColor="#3b7cff" stopOpacity={0} />
                        </linearGradient>
                        <linearGradient id="colorClicked" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="#10b981" stopOpacity={0.3} />
                            <stop offset="95%" stopColor="#10b981" stopOpacity={0} />
                        </linearGradient>
                    </defs>
                    <XAxis
                        dataKey="date"
                        stroke="var(--text-tertiary)"
                        fontSize={12}
                        tickLine={false}
                        axisLine={false}
                    />
                    <YAxis
                        stroke="var(--text-tertiary)"
                        fontSize={12}
                        tickLine={false}
                        axisLine={false}
                        tickFormatter={(value) => `${value}`}
                    />
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--border-default)" opacity={0.4} />
                    <Tooltip
                        contentStyle={{
                            backgroundColor: 'var(--bg-card)',
                            borderColor: 'var(--border-default)',
                            borderRadius: '8px',
                            boxShadow: 'var(--shadow-card)'
                        }}
                        labelStyle={{ color: 'var(--text-secondary)' }}
                    />
                    <Area
                        type="monotone"
                        dataKey="shown"
                        stroke="#3b7cff"
                        fillOpacity={1}
                        fill="url(#colorShown)"
                        name="Toasts Shown"
                    />
                    <Area
                        type="monotone"
                        dataKey="clicked"
                        stroke="#10b981"
                        fillOpacity={1}
                        fill="url(#colorClicked)"
                        name="Interactions"
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    );
};
