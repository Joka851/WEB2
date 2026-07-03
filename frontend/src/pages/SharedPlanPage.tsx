import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { shareService } from '../services/shareService';
import { checklistService } from '../services/checklistService';
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

  if (loading) return <div className="page"><p style={{ color: 'var(--ink-soft)' }}>Loading...</p></div>;
  if (error) return <div className="page"><div className="alert alert-error">{error}</div></div>;
  if (!plan) return <div className="page"><p>Plan not found.</p></div>;

  const canEdit = accessType === 'EDIT' && !!user;

  const handleToggleChecklist = async (itemId: number) => {
    if (!planId || !token) return;
    try {
      await checklistService.toggle(planId, itemId, token);
      // Osveži plan da prikaže novo stanje checklist stavke
      const data = await shareService.accessByToken(token);
      setPlan(data.travelPlan);
    } catch {
      setError('Failed to update checklist item.');
    }
  };

  return (
    <div className="page" style={{ maxWidth: '760px' }}>
      <span className="eyebrow"> Shared plan</span>
      <h2>{plan.name}</h2>
      <span className={`badge ${canEdit ? 'badge-accent' : 'badge-success'}`}>{accessType}</span>
      <p style={{ marginTop: '12px' }}>{plan.description}</p>
      <p>
        <strong>Period:</strong>{' '}
        {new Date(plan.startDate).toLocaleDateString()} -{' '}
        {new Date(plan.endDate).toLocaleDateString()}
      </p>
      <p><strong>Budget:</strong> ${plan.budget}</p>
      {plan.notes && <p>{plan.notes}</p>}

      {canEdit && (
        <div className="alert alert-info">
          <strong>Edit mode:</strong> You can edit this plan.
        </div>
      )}

      {/* Destinations */}
      {plan.destinations && plan.destinations.length > 0 && (
        <div>
          <div className="route-divider"><span>Destinations</span></div>
          {plan.destinations.map((d: any) => (
            <div key={d.id} className="card">
              <h4>{d.name} — {d.location}</h4>
              <p style={{ fontSize: '13px', color: 'var(--ink-soft)' }}>
                {new Date(d.arrivalDate).toLocaleDateString()} - {new Date(d.departureDate).toLocaleDateString()}
              </p>
              {d.description && <p>{d.description}</p>}
              {canEdit && (
                <button
                  className="btn btn-outline btn-sm"
                  onClick={() =>
                    navigate(`/travel-plans/${planId}/destinations/${d.id}/edit`, {
                      state: { returnTo: `/shared/${token}`, shareToken: token }
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
          <div className="route-divider"><span>Activities</span></div>
          {plan.activities
            .slice()
            .sort((a: any, b: any) => new Date(a.date).getTime() - new Date(b.date).getTime())
            .map((a: any) => (
              <div key={a.id} className="card">
                <h4>{a.name}</h4>
                <p style={{ fontSize: '13px', color: 'var(--ink-soft)' }}>
                  {new Date(a.date).toLocaleDateString()} {a.time && `at ${a.time}`} {a.location && `— ${a.location}`}
                </p>
                {a.description && <p>{a.description}</p>}
                <p style={{ fontSize: '13px' }}><strong>Cost:</strong> ${a.estimatedCost} | <strong>Status:</strong> {a.status}</p>
                {canEdit && (
                  <button
                    className="btn btn-outline btn-sm"
                    onClick={() =>
                      navigate(`/travel-plans/${planId}/activities/${a.id}/edit`, {
                        state: { returnTo: `/shared/${token}`, shareToken: token }
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
          <div className="route-divider"><span>Checklist</span></div>
          <div className="card">
            {plan.checklistItems.map((item: any) => (
              <div key={item.id} className="checkbox-row" style={{ marginBottom: '10px' }}>
                <input
                  type="checkbox"
                  checked={item.isCompleted}
                  readOnly={!canEdit}
                  onChange={canEdit ? () => handleToggleChecklist(item.id) : undefined}
                  style={{ cursor: canEdit ? 'pointer' : 'default' }}
                />
                <span style={{ textDecoration: item.isCompleted ? 'line-through' : 'none', color: item.isCompleted ? 'var(--ink-soft)' : 'var(--ink)' }}>
                  {item.name}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default SharedPlanPage;