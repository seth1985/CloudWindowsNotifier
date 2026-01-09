import React from 'react';
import { LucideIcon } from 'lucide-react';
import { cn } from '../../../lib/utils';

type Props = {
    label: string;
    value: string | number;
    icon?: LucideIcon;
    color?: 'primary' | 'success' | 'warning' | 'info';
};

export const MetricTile: React.FC<Props> = ({ label, value, icon: Icon, color = 'primary' }) => {
    const colorClasses = {
        primary: 'text-primary-main bg-primary-main/5 border-primary-main/20',
        success: 'text-green-500 bg-green-500/5 border-green-500/20',
        warning: 'text-yellow-500 bg-yellow-500/5 border-yellow-500/20',
        info: 'text-blue-500 bg-blue-500/5 border-blue-500/20',
    };

    return (
        <div className="card p-5 group hover:border-text-tertiary/30 transition-all flex flex-col gap-4">
            <div className="flex items-center justify-between">
                <span className="text-[10px] font-black text-text-tertiary uppercase tracking-[0.2em]">
                    {label}
                </span>
                {Icon && (
                    <div className={cn("p-2 rounded-lg border", colorClasses[color])}>
                        <Icon className="w-4 h-4" />
                    </div>
                )}
            </div>
            <div className="flex items-baseline gap-2">
                <span className="text-2xl font-black text-text-primary tracking-tight">
                    {value}
                </span>
            </div>
        </div>
    );
};
