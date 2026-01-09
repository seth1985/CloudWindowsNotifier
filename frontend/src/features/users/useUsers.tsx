import { useState, useEffect } from 'react';
import type { User, UserRole } from '../../types';

export const useUsers = (apiBase: string, token: string | null) => {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(false);

    const fetchUsers = async () => {
        if (!token) return;

        setLoading(true);
        try {
            const res = await fetch(`${apiBase}/api/users`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (res.ok) {
                const data = await res.json();
                setUsers(data);
            }
        } catch (err) {
            console.error('Failed to fetch users:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, [apiBase, token]);

    const updateUserRole = async (userId: string, newRole: UserRole) => {
        if (!token) return;

        setLoading(true);
        try {
            const res = await fetch(`${apiBase}/api/users/${userId}/role`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ role: newRole })
            });

            if (res.ok) {
                // Update local state
                setUsers(prev => prev.map(u =>
                    u.id === userId ? { ...u, role: newRole } : u
                ));
            }
        } catch (err) {
            console.error('Failed to update user role:', err);
        } finally {
            setLoading(false);
        }
    };

    const addUser = async (user: Omit<User, 'id'>) => {
        if (!token) return;

        setLoading(true);
        try {
            const res = await fetch(`${apiBase}/api/users`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    displayName: user.displayName,
                    email: user.email,
                    role: user.role,
                    avatarUrl: user.avatarUrl
                })
            });

            if (res.ok) {
                const newUser = await res.json();
                setUsers(prev => [...prev, newUser]);
            }
        } catch (err) {
            console.error('Failed to add user:', err);
        } finally {
            setLoading(false);
        }
    };

    const updateUser = async (userId: string, data: { displayName: string; email: string }) => {
        if (!token) return;

        setLoading(true);
        try {
            const res = await fetch(`${apiBase}/api/users/${userId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });

            if (res.ok) {
                setUsers(prev => prev.map(u =>
                    u.id === userId ? { ...u, ...data } : u
                ));
            }
        } catch (err) {
            console.error('Failed to update user:', err);
        } finally {
            setLoading(false);
        }
    };

    const removeUser = async (userId: string) => {
        if (!token) return;

        setLoading(true);
        try {
            const res = await fetch(`${apiBase}/api/users/${userId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (res.ok) {
                setUsers(prev => prev.filter(u => u.id !== userId));
            }
        } catch (err) {
            console.error('Failed to remove user:', err);
        } finally {
            setLoading(false);
        }
    };

    return {
        users,
        loading,
        updateUserRole,
        updateUser,
        addUser,
        removeUser,
        refresh: fetchUsers
    };
};
