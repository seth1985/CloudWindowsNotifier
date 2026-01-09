import React from 'react';
import { Sidebar } from './Sidebar';

type Props = {
    children: React.ReactNode;
    activeTab: string;
    setActiveTab: (id: string) => void;
    darkMode: boolean;
    toggleTheme: () => void;
    onLogout: () => void;
    username: string;
    apiBase: string;
    authed: boolean;
};

export const Layout: React.FC<Props> = ({
    children,
    activeTab,
    setActiveTab,
    darkMode,
    toggleTheme,
    onLogout,
    username,
    apiBase,
    authed
}) => {
    if (!authed) {
        return (
            <div className="min-h-screen bg-background flex items-center justify-center p-4">
                <div className="w-full max-w-md">
                    {children}
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-background flex">
            <Sidebar
                activeTab={activeTab}
                setActiveTab={setActiveTab}
                darkMode={darkMode}
                toggleTheme={toggleTheme}
                onLogout={onLogout}
                username={username}
                apiBase={apiBase}
            />

            <main className="flex-1 ml-64 min-h-screen transition-all duration-300 p-8">
                <div className="max-w-7xl mx-auto space-y-6">
                    {children}
                </div>
            </main>
        </div>
    );
};
