import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { shareService } from '../services/shareService';
import { TravelPlan } from '../models/TravelPlan';

const SharedPlanPage: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const [plan, setPlan] = useState<TravelPlan | null>(null);
  const [accessType, setAccessType] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPlan = async () => {
      try {
        const data = await shareService.accessByToken(token!);
        setPlan(data.travelPlan);
        setAccessType(data.accessType);
      } catch {
        setError('Invalid or expired link.');
      } finally {
        setLoading(false);
      }
    };
    fetchPlan();
  }, [token]);

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;
  if (!plan) return <p>Plan not found.</p>;

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <h2>{plan.name}</h2>
      <p><strong>Access Type:</strong> {accessType}</p>
      <p>{plan.description}</p>
      <p><strong>Period:</strong> {new Date(plan.startDate).toLocaleDateString()} - {new Date(plan.endDate).toLocaleDateString()}</p>
      <p><strong>Budget:</strong> ${plan.budget}</p>
      <p>{plan.notes}</p>
    </div>
  );
};

export default SharedPlanPage;