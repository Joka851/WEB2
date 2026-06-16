import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { shareService } from '../services/shareService';
import { useAuth } from '../context/AuthContext';
import { TravelPlan } from '../models/TravelPlan';

const SharedPlanPage: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [plan, setPlan] = useState<TravelPlan | null>(null);
  const [planId, setPlanId] = useState<number | null>(null);
  const [accessType, setAccessType] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPlan = async () => {
      try {
        const data = await shareService.accessByToken(token!);
        setPlan(data.travelPlan);
        setPlanId(data.travelPlan.id);
        setAccessType(data.accessType);

        // Ako je EDIT a nije ulogovan, redirect na login
        if (data.accessType === 'EDIT' && !user) {
          navigate(`/login?redirect=/shared/${token}`);
          return;
        }
      } catch {
        setError('Invalid or expired link.');
      } finally {
        setLoading(false);
      }
    };
    fetchPlan();
  }, [token, user, navigate]);

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;
  if (!plan) return <p>Plan not found.</p>;

  const canEdit = accessType === 'EDIT' && !!user;

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <h2>{plan.name}</h2>
      <p><strong>Access Type:</strong> <span style={{ color: canEdit ? 'orange' : 'green' }}>{accessType}</span></p>
      <p>{plan.description}</p>
      <p>
        <strong>Period:</strong>{' '}
        {new Date(plan.startDate).toLocaleDateString()} -{' '}
        {new Date(plan.endDate).toLocaleDateString()}
      </p>
      <p><strong>Budget:</strong> ${plan.budget}</p>
      {plan.notes && <p>{plan.notes}</p>}

      {canEdit && (
        <div style={{ marginBottom: '20px', padding: '10px', backgroundColor: '#fff3cd', borderRadius: '8px' }}>
          <strong>Edit mode:</strong> You can edit this plan.
        </div>
      )}

      {/* Destinations */}
      {plan.destinations && plan.destinations.length > 0 && (
        <div>
          <h3>Destinations</h3>
          {plan.destinations.map((d: any) => (
            <div key={d.id} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
              <h4>{d.name} — {d.location}</h4>
              <p>{new Date(d.arrivalDate).toLocaleDateString()} - {new Date(d.departureDate).toLocaleDateString()}</p>
              {d.description && <p>{d.description}</p>}
              {canEdit && (
                <button
                  onClick={() =>
                    navigate(`/travel-plans/${planId}/destinations/${d.id}/edit`, {
                      state: { returnTo: `/shared/${token}` }
                    })
                  }
                >
                  Edit
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Activities */}
      {plan.activities && plan.activities.length > 0 && (
        <div>
          <h3>Activities</h3>
          {plan.activities
            .slice()
            .sort((a: any, b: any) => new Date(a.date).getTime() - new Date(b.date).getTime())
            .map((a: any) => (
              <div key={a.id} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
                <h4>{a.name}</h4>
                <p>{new Date(a.date).toLocaleDateString()} {a.time && `at ${a.time}`} {a.location && `— ${a.location}`}</p>
                {a.description && <p>{a.description}</p>}
                <p><strong>Cost:</strong> ${a.estimatedCost} | <strong>Status:</strong> {a.status}</p>
                {canEdit && (
                  <button
                    onClick={() =>
                      navigate(`/travel-plans/${planId}/activities/${a.id}/edit`, {
                        state: { returnTo: `/shared/${token}` }
                      })
                    }
                  >
                    Edit
                  </button>
                )}
              </div>
            ))}
        </div>
      )}

      {/* Checklist */}
      {plan.checklistItems && plan.checklistItems.length > 0 && (
        <div>
          <h3>Checklist</h3>
          {plan.checklistItems.map((item: any) => (
            <div key={item.id} style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '8px' }}>
              <input type="checkbox" checked={item.isCompleted} readOnly />
              <span style={{ textDecoration: item.isCompleted ? 'line-through' : 'none' }}>
                {item.name}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SharedPlanPage;