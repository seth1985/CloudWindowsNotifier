import React, { useState, useEffect } from 'react';
import { X, Save } from 'lucide-react';
import type { User } from '../../../types';

type Props = {
    user: User;
    isOpen: boolean;
    onClose: () => void;
    onSave: (displayName: string, email: string) => Promise<void>;
    loading?: boolean;
};

export const EditUserModal: React.FC<Props> = ({ user, isOpen, onClose, onSave, loading }) => {
    const [displayName, setDisplayName] = useState(user.displayName);
    const [email, setEmail] = useState(user.email);

    useEffect(() => {
        if (isOpen) {
            setDisplayName(user.displayName);
            setEmail(user.email);
        }
    }, [isOpen, user]);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        await onSave(displayName, email);
        onClose();
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-in fade-in duration-200">
            <div className="bg-card border border-border w-full max-w-md rounded-xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
                <div className="flex items-center justify-between p-6 border-b border-border bg-background/50">
                    <h3 className="text-xl font-semibold text-text-primary">Edit User</h3>
                    <button
                        onClick={onClose}
                        className="p-2 hover:bg-white/10 rounded-full transition-colors"
                    >
                        <X className="w-5 h-5 text-text-tertiary" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-text-secondary">Display Name</label>
                        <input
                            required
                            type="text"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            className="w-full bg-background border border-border rounded-lg px-4 py-2 text-text-primary focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all"
                            placeholder="John Doe"
                        />
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-text-secondary">Email Address</label>
                        <input
                            required
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            className="w-full bg-background border border-border rounded-lg px-4 py-2 text-text-primary focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all"
                            placeholder="john@example.com"
                        />
                    </div>

                    <div className="flex justify-end gap-3 pt-4">
                        <button
                            type="button"
                            onClick={onClose}
                            className="btn btn-secondary"
                            disabled={loading}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="btn btn-primary flex items-center gap-2"
                            disabled={loading}
                        >
                            <Save className="w-4 h-4" />
                            {loading ? 'Saving...' : 'Save Changes'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};
