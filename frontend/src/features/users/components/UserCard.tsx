import React from 'react';
import type { User, UserRole } from '../../../types';
import { Shield, ShieldAlert, ShieldCheck, Trash2, User as UserIcon, Edit2 } from 'lucide-react';
import { cn } from '../../../lib/utils'; // Assuming you have this

type Props = {
    user: User;
    onRoleChange: (role: UserRole) => void;
    onDelete: () => void;
    onEdit: () => void;
    loading?: boolean;
    currentUserEmail?: string;
};

const RoleBadge: React.FC<{ role: UserRole }> = ({ role }) => {
    switch (role) {
        case 'Admin':
            return (
                <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-semibold bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 border border-red-200 dark:border-red-800">
                    <ShieldAlert className="w-3 h-3" /> Admin
                </span>
            );
        case 'Advanced':
            return (
                <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-semibold bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400 border border-purple-200 dark:border-purple-800">
                    <ShieldCheck className="w-3 h-3" /> Advanced
                </span>
            );
        default:
            return (
                <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400 border border-blue-200 dark:border-blue-800">
                    <Shield className="w-3 h-3" /> Standard
                </span>
            );
    }
};

export const UserCard: React.FC<Props> = ({ user, onRoleChange, onDelete, onEdit, loading, currentUserEmail }) => {
    const isMe = user.email === currentUserEmail;

    return (
        <div className="bg-card border border-border rounded-xl shadow-sm hover:shadow-md transition-shadow p-5 flex flex-col gap-4 relative group">
            <div className="flex items-start justify-between">
                <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-full overflow-hidden bg-muted border border-border flex items-center justify-center">
                        {user.avatarUrl ? (
                            <img src={user.avatarUrl} alt={user.displayName} className="w-full h-full object-cover" />
                        ) : (
                            <UserIcon className="w-6 h-6 text-text-tertiary" />
                        )}
                    </div>
                    <div>
                        <h4 className="font-semibold text-text-primary flex items-center gap-2">
                            {user.displayName}
                            {isMe && <span className="text-[10px] uppercase font-bold text-text-tertiary bg-muted px-1.5 py-0.5 rounded">You</span>}
                        </h4>
                        <p className="text-xs text-text-tertiary">{user.email}</p>
                    </div>
                </div>
                <RoleBadge role={user.role} />
            </div>

            <div className="pt-2 border-t border-border mt-auto flex items-center justify-between">
                <div className="flex flex-col">
                    <span className="text-[10px] text-text-tertiary uppercase tracking-wider font-semibold">Role Access</span>
                    <select
                        className="mt-1 text-sm bg-transparent border-none p-0 font-medium text-text-primary focus:ring-0 cursor-pointer hover:text-primary transition-colors disabled:opacity-50"
                        value={user.role}
                        onChange={(e) => onRoleChange(e.target.value as UserRole)}
                        disabled={loading || isMe} // Prevent changing own role for safety in this demo
                    >
                        <option value="Standard">Standard</option>
                        <option value="Advanced">Advanced</option>
                        <option value="Admin">Admin</option>
                    </select>
                </div>

                <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button
                        onClick={onEdit}
                        disabled={loading}
                        className="p-2 text-text-tertiary hover:text-primary hover:bg-primary/10 rounded-full transition-colors"
                        title="Edit User"
                    >
                        <Edit2 className="w-4 h-4" />
                    </button>

                    {!isMe && (
                        <button
                            onClick={onDelete}
                            disabled={loading}
                            className="p-2 text-text-tertiary hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/10 rounded-full transition-colors"
                            title="Remove User"
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};
