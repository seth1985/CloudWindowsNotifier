import React from 'react';
import {
    LayoutDashboard,
    Activity,
    LogOut,
    Moon,
    Sun,
    Server,
    Users,
    ChevronRight
} from 'lucide-react';
import { cn } from '../../lib/utils';

type NavItem = {
    icon: React.ElementType;
    label: string;
    id: string;
};

type Props = {
    activeTab: string;
    setActiveTab: (id: string) => void;
    darkMode: boolean;
    toggleTheme: () => void;
    onLogout: () => void;
    username: string;
    apiBase: string;
};

export const Sidebar: React.FC<Props> = ({
    activeTab,
    setActiveTab,
    darkMode,
    toggleTheme,
    onLogout,
    username,
    apiBase
}) => {

    const navItems: NavItem[] = [
        { icon: LayoutDashboard, label: 'Modules', id: 'modules' },
        { icon: Activity, label: 'Telemetry', id: 'telemetry' },
        { icon: Users, label: 'Users', id: 'users' },
    ];

    return (
        <aside className="fixed inset-y-0 left-0 z-50 w-64 bg-nav border-r border-white/10 text-nav-text flex flex-col transition-all duration-300 shadow-2xl">
            {/* Brand */}
            <div className="h-20 flex items-center px-6 mb-4">
                <div className="flex items-center gap-3 group cursor-default">
                    <div className="p-2 bg-primary-main/10 rounded-xl group-hover:bg-primary-main/20 transition-all border border-primary-main/30 group-hover:scale-110 duration-300">
                        <Server className="w-6 h-6 text-primary-main shadow-[0_0_15px_rgba(59,130,246,0.5)]" />
                    </div>
                    <div className="flex flex-col">
                        <span className="font-black text-white text-lg tracking-tight leading-none uppercase">NOTIFIER</span>
                        <span className="text-[10px] font-bold text-primary-main tracking-[0.2em] uppercase mt-1 opacity-90">Cloud System</span>
                    </div>
                </div>
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-4 space-y-2">
                <div className="text-[10px] font-black text-text-tertiary uppercase tracking-[0.2em] px-3 mb-4 opacity-70">Main Menu</div>
                {navItems.map((item) => (
                    <button
                        key={item.id}
                        onClick={() => setActiveTab(item.id)}
                        className={cn(
                            "w-full flex items-center justify-between px-4 py-3 rounded-xl text-sm font-bold transition-all relative group border border-transparent",
                            activeTab === item.id
                                ? "bg-white/10 text-white border-white/20 shadow-lg"
                                : "text-text-tertiary hover:bg-white/5 hover:text-white hover:border-white/10"
                        )}
                    >
                        <div className="flex items-center gap-3">
                            <item.icon className={cn("w-5 h-5 transition-transform duration-300", activeTab === item.id ? "text-primary-main" : "group-hover:scale-110")} />
                            {item.label}
                        </div>
                        {activeTab === item.id && (
                            <div className="absolute left-0 w-1.5 h-6 bg-primary-main rounded-r-full animate-in fade-in slide-in-from-left-2 duration-300" />
                        )}
                        <ChevronRight className={cn("w-3 h-3 opacity-0 -translate-x-2 transition-all group-hover:opacity-100 group-hover:translate-x-0", activeTab === item.id && "hidden")} />
                    </button>
                ))}
            </nav>

            {/* User & Actions */}
            <div className="p-6 border-t border-white/10 bg-black/20 space-y-6">
                <div className="flex items-center justify-between p-3 bg-white/5 rounded-2xl border border-white/10">
                    <div className="flex flex-col min-w-0">
                        <span className="text-[10px] font-bold text-primary-main uppercase tracking-wider mb-0.5 opacity-90">Session</span>
                        <div className="text-xs font-bold text-white truncate" title={username}>
                            {username}
                        </div>
                    </div>
                    <button
                        onClick={toggleTheme}
                        className="p-2 bg-white/10 rounded-xl border border-white/20 hover:bg-white/20 text-white transition-all shadow-md focus:ring-2 focus:ring-primary-main/50"
                        title="Toggle Theme"
                    >
                        {darkMode ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
                    </button>
                </div>

                <button
                    onClick={onLogout}
                    className="w-full flex items-center justify-center gap-2.5 px-4 py-3 rounded-xl text-xs font-black uppercase tracking-widest text-red-400 bg-red-500/10 hover:bg-red-500/20 border border-red-500/20 hover:border-red-500/40 transition-all group"
                >
                    <LogOut className="w-4 h-4 group-hover:-translate-x-1 transition-transform" />
                    Sign Out
                </button>

                <div className="text-center">
                    <span className="text-[9px] font-bold text-text-tertiary tracking-widest uppercase opacity-50">{apiBase.split('://')[1]?.split('/')[0] || apiBase}</span>
                </div>
            </div>
        </aside>
    );
};
