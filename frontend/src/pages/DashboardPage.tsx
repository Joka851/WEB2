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
    <div style={{ padding: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Welcome, {user?.firstName}!</h2>
        <div>
          {isAdmin && (
            <button onClick={() => navigate('/admin')} style={{ marginRight: '10px' }}>
              Admin Panel
            </button>
          )}
          <button onClick={() => navigate('/travel-plans/create')} style={{ marginRight: '10px' }}>
            Create New Plan
          </button>
          <button onClick={logout}>Logout</button>
        </div>
      </div>

      <h3>My Travel Plans</h3>
      {loading && <p>Loading...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {!loading && plans.length === 0 && <p>No travel plans found.</p>}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '20px' }}>
        {plans.map(plan => (
          <div key={plan.id} style={{ border: '1px solid #ccc', padding: '15px', borderRadius: '8px' }}>
            <h4>{plan.name}</h4>
            <p>{plan.description}</p>
            <p><strong>From:</strong> {new Date(plan.startDate).toLocaleDateString()}</p>
            <p><strong>To:</strong> {new Date(plan.endDate).toLocaleDateString()}</p>
            <p><strong>Budget:</strong> ${plan.budget}</p>
            <div style={{ display: 'flex', gap: '10px' }}>
              <button onClick={() => navigate(`/travel-plans/${plan.id}`)}>View</button>
              <button onClick={() => navigate(`/travel-plans/${plan.id}/edit`)}>Edit</button>
              <button onClick={() => handleDelete(plan.id)} style={{ color: 'red' }}>Delete</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DashboardPage;