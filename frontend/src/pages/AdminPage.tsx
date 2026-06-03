import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { User } from '../models/User';
import { userService } from '../services/userService';

const AdminPage: React.FC = () => {
  const { isAdmin, logout } = useAuth();
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
    if (window.confirm('Are you sure you want to delete this user?')) {
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

  return (
    <div style={{ padding: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Admin Panel</h2>
        <div>
          <button onClick={() => navigate('/dashboard')} style={{ marginRight: '10px' }}>Dashboard</button>
          <button onClick={logout}>Logout</button>
        </div>
      </div>

      <h3>Users</h3>
      {loading && <p>Loading...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ backgroundColor: '#f0f0f0' }}>
            <th style={{ padding: '10px', border: '1px solid #ccc' }}>ID</th>
            <th style={{ padding: '10px', border: '1px solid #ccc' }}>Name</th>
            <th style={{ padding: '10px', border: '1px solid #ccc' }}>Email</th>
            <th style={{ padding: '10px', border: '1px solid #ccc' }}>Role</th>
            <th style={{ padding: '10px', border: '1px solid #ccc' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map(user => (
            <tr key={user.id}>
              <td style={{ padding: '10px', border: '1px solid #ccc' }}>{user.id}</td>
              <td style={{ padding: '10px', border: '1px solid #ccc' }}>{user.firstName} {user.lastName}</td>
              <td style={{ padding: '10px', border: '1px solid #ccc' }}>{user.email}</td>
              <td style={{ padding: '10px', border: '1px solid #ccc' }}>
                <select value={user.role} onChange={e => handleRoleChange(user.id, e.target.value)}>
                  <option value="User">User</option>
                  <option value="Admin">Admin</option>
                </select>
              </td>
              <td style={{ padding: '10px', border: '1px solid #ccc' }}>
                <button onClick={() => handleDelete(user.id)} style={{ color: 'red' }}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AdminPage;