import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { TravelPlan } from '../models/TravelPlan';
import { travelPlanService } from '../services/travelPlanService';

const DashboardPage: React.FC = () => {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();
  const [plans, setPlans] = useState<TravelPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchPlans = async () => {
      try {
        if (user) {
          let data: TravelPlan[];
          
          if (isAdmin) {
            // Admin vidi sve planove
            data = await travelPlanService.getAll();
          } else {
            // Običan korisnik vidi samo svoje planove (preko my-plans endpointa)
            data = await travelPlanService.getMyPlans();
          }
          
          setPlans(data);
        }
      } catch (err) {
        setError('Failed to load travel plans.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchPlans();
  }, [user, isAdmin]);

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this plan?')) {
      try {
        await travelPlanService.delete(id);
        setPlans(plans.filter(p => p.id !== id));
      } catch {
        setError('Failed to delete plan.');
      }
    }
  };

  return (
    <div className="page">
      <div className="topbar">
        <div>
          <span className="eyebrow"> TravelPlanner</span>
          <h2 style={{ margin: 0 }}>Welcome, {user?.firstName}!</h2>
        </div>
        <div className="topbar-actions">
          {isAdmin && (
            <button className="btn btn-outline" onClick={() => navigate('/admin')}>
              Admin Panel
            </button>
          )}
          <button className="btn btn-accent" onClick={() => navigate('/travel-plans/create')}>
            + Create New Plan
          </button>
          <button className="btn btn-outline" onClick={logout}>Logout</button>
        </div>
      </div>

      <div className="route-divider"><span>My Travel Plans</span></div>

      {loading && <p style={{ color: 'var(--ink-soft)' }}>Loading...</p>}
      {error && <div className="alert alert-error">{error}</div>}
      {!loading && plans.length === 0 && (
        <div className="empty-state">
          <p style={{ margin: 0 }}>No travel plans yet — start by creating your first one. 🗺️</p>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))', gap: '16px' }}>
        {plans.map(plan => (
          <div key={plan.id} className="card card-hover card-accent-left">
            <h4>{plan.name}</h4>
            <p style={{ color: 'var(--ink-soft)', fontSize: '13px' }}>{plan.description}</p>
            <p style={{ fontSize: '13px', margin: '4px 0' }}>
              <strong>{new Date(plan.startDate).toLocaleDateString()}</strong> — <strong>{new Date(plan.endDate).toLocaleDateString()}</strong>
            </p>
            <span className="badge badge-primary">Budget: ${plan.budget}</span>
            <div className="btn-row" style={{ marginTop: '14px' }}>
              <button className="btn btn-primary btn-sm" onClick={() => navigate(`/travel-plans/${plan.id}`)}>View</button>
              <button className="btn btn-outline btn-sm" onClick={() => navigate(`/travel-plans/${plan.id}/edit`)}>Edit</button>
              <button className="btn btn-danger btn-sm" onClick={() => handleDelete(plan.id)}>Delete</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DashboardPage;