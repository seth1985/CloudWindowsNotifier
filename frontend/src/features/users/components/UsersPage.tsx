import React from 'react';
import { useUsers } from '../useUsers';
import { UserCard } from './UserCard';
import { EditUserModal } from './EditUserModal';
import { Plus, Users, Search } from 'lucide-react';
import type { User, UserRole } from '../../../types';

type Props = {
    currentUserEmail?: string; // To highlight 'You'
    apiBase: string;
    token: string | null;
};

export const UsersPage: React.FC<Props> = ({ currentUserEmail, apiBase, token }) => {
    const { users, loading, updateUserRole, updateUser, removeUser, addUser } = useUsers(apiBase, token);
    const [search, setSearch] = React.useState('');
    const [editingUser, setEditingUser] = React.useState<User | null>(null);

    const filteredUsers = users.filter(u =>
        u.displayName.toLowerCase().includes(search.toLowerCase()) ||
        u.email.toLowerCase().includes(search.toLowerCase())
    );

    const handleCreateMockUser = () => {
        const names = ['Eve Example', 'Frank Finance', 'Grace Guest', 'Heidi Helpdesk'];
        const roles: UserRole[] = ['Standard', 'Advanced'];
        const randomName = names[Math.floor(Math.random() * names.length)];

        addUser({
            displayName: randomName,
            email: `${randomName.split(' ')[0].toLowerCase()}@contoso.com`,
            role: roles[Math.floor(Math.random() * roles.length)],
            avatarUrl: `https://api.dicebear.com/7.x/avataaars/svg?seed=${randomName.split(' ')[0]}`
        });
    };

    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-2 duration-500">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 pb-4 border-b border-border">
                <div>
                    <h3 className="text-2xl font-semibold text-text-primary flex items-center gap-2">
                        <Users className="w-6 h-6 text-primary" />
                        User Management
                    </h3>
                    <p className="text-text-secondary">Manage access and roles for your organization.</p>
                </div>
                <button
                    onClick={handleCreateMockUser}
                    disabled={loading}
                    className="btn btn-primary flex items-center gap-2"
                >
                    <Plus className="w-4 h-4" />
                    Add User
                </button>
            </div>

            <div className="flex items-center gap-4 bg-card border border-border p-4 rounded-lg shadow-sm">
                <Search className="w-5 h-5 text-text-tertiary" />
                <input
                    className="bg-transparent border-none focus:ring-0 text-sm w-full text-text-primary placeholder:text-text-tertiary"
                    placeholder="Search users by name or email..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {filteredUsers.map(user => (
                    <UserCard
                        key={user.id}
                        user={user}
                        loading={loading}
                        onRoleChange={(role) => updateUserRole(user.id, role)}
                        onDelete={() => removeUser(user.id)}
                        onEdit={() => setEditingUser(user)}
                        currentUserEmail={currentUserEmail}
                    />
                ))}
            </div>

            {editingUser && (
                <EditUserModal
                    user={editingUser}
                    isOpen={!!editingUser}
                    onClose={() => setEditingUser(null)}
                    onSave={(displayName, email) => updateUser(editingUser.id, { displayName, email })}
                    loading={loading}
                />
            )}
        </div>
    );
};
