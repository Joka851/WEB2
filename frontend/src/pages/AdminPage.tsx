import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { User } from '../models/User';
import { userService } from '../services/userService';

const AdminPage: React.FC = () => {
  const { isAdmin, logout, user: currentUser } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isAdmin) {
      navigate('/dashboard');
      return;
    }
    const fetchUsers = async () => {
      try {
        const data = await userService.getAll();
        setUsers(data);
      } catch {
        setError('Failed to load users.');
      } finally {
        setLoading(false);
      }
    };
    fetchUsers();
  }, [isAdmin, navigate]);

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this user? This will delete all their travel plans.')) {
      try {
        await userService.delete(id);
        setUsers(users.filter(u => u.id !== id));
      } catch {
        setError('Failed to delete user.');
      }
    }
  };

  const handleRoleChange = async (id: number, role: string) => {
    try {
      const updated = await userService.updateRole(id, role);
      setUsers(users.map(u => u.id === id ? updated : u));
    } catch {
      setError('Failed to update role.');
    }
  };

  const handleToggleActive = async (id: number, currentStatus: boolean) => {
    try {
      await userService.updateUserStatus(id, { isActive: !currentStatus });
      const data = await userService.getAll();
      setUsers(data);
    } catch {
      setError('Failed to update user status.');
    }
  };

  return (
    <div className="page">
      <div className="topbar">
        <div>
          <span className="eyebrow"> Admin</span>
          <h2 style={{ margin: 0 }}>Admin Panel</h2>
        </div>
        <div className="topbar-actions">
          <button className="btn btn-outline" onClick={() => navigate('/dashboard')}>Dashboard</button>
          <button className="btn btn-outline" onClick={logout}>Logout</button>
        </div>
      </div>

      <div className="route-divider"><span>Users</span></div>
      {loading && <p style={{ color: 'var(--ink-soft)' }}>Loading...</p>}
      {error && <div className="alert alert-error">{error}</div>}

      <div style={{ overflowX: 'auto' }}>
        <table className="table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr key={user.id}>
                <td>{user.id}</td>
                <td>{user.firstName} {user.lastName}</td>
                <td>{user.email}</td>
                <td>
                  <select
                    className="select"
                    value={user.role}
                    onChange={e => handleRoleChange(user.id, e.target.value)}
                    disabled={user.id === currentUser?.id}
                    style={{ width: 'auto' }}
                  >
                    <option value="User">User</option>
                    <option value="Admin">Admin</option>
                  </select>
                </td>
                <td>
                  <span className={`badge ${user.isDeleted ? 'badge-danger' : (user.isActive ? 'badge-success' : 'badge-muted')}`}>
                    {user.isDeleted ? 'Deleted' : (user.isActive ? 'Active' : 'Inactive')}
                  </span>
                </td>
                <td>
                  <div className="btn-row">
                    {!user.isDeleted && (
                      <button
                        className={`btn btn-sm ${user.isActive ? 'btn-outline' : 'btn-success'}`}
                        onClick={() => handleToggleActive(user.id, user.isActive)}
                      >
                        {user.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                    )}
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => handleDelete(user.id)}
                      disabled={user.id === currentUser?.id}
                    >
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminPage;