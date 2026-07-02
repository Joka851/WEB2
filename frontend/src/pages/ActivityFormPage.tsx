import React, { useEffect, useState } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { activityService } from '../services/activityService';
import { CreateActivity } from '../models/Activity';

const ActivityFormPage: React.FC = () => {
  const { planId, id } = useParams<{ planId: string; id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const isEdit = !!id;

  const returnTo = (location.state as { returnTo?: string; shareToken?: string } | null)?.returnTo;
  const shareToken = (location.state as { returnTo?: string; shareToken?: string } | null)?.shareToken;
  const goBack = () => navigate(returnTo || `/travel-plans/${planId}`);

  const [name, setName] = useState('');
  const [date, setDate] = useState('');
  const [time, setTime] = useState('');
  const [location_, setLocation] = useState('');
  const [description, setDescription] = useState('');
  const [estimatedCost, setEstimatedCost] = useState(0);
  const [status, setStatus] = useState('Planned');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      const fetchActivity = async () => {
        try {
          const activities = await activityService.getAll(parseInt(planId!), shareToken);
          const act = activities.find(a => a.id === parseInt(id!));
          if (act) {
            setName(act.name);
            setDate(act.date.split('T')[0]);
            setTime(act.time);
            setLocation(act.location);
            setDescription(act.description);
            setEstimatedCost(act.estimatedCost);
            setStatus(act.status);
          }
        } catch {
          setError('Failed to load activity.');
        }
      };
      fetchActivity();
    }
  }, [id, planId, isEdit]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const data: CreateActivity = {
        name, date, time, location: location_, description, estimatedCost, status
      };

      if (isEdit) {
        await activityService.update(parseInt(planId!), parseInt(id!), data, shareToken);
      } else {
        await activityService.create(parseInt(planId!), data, shareToken);
      }
      goBack();
    } catch {
      setError('Failed to save activity.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '50px auto', padding: '20px' }}>
      <h2>{isEdit ? 'Edit Activity' : 'Add Activity'}</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '10px' }}>
          <label>Name:</label>
          <input type="text" value={name} onChange={e => setName(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Date:</label>
          <input type="date" value={date} onChange={e => setDate(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Time:</label>
          <input type="time" value={time} onChange={e => setTime(e.target.value)}
            style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Location:</label>
          <input type="text" value={location_} onChange={e => setLocation(e.target.value)}
            style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Description:</label>
          <textarea value={description} onChange={e => setDescription(e.target.value)}
            style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Estimated Cost ($):</label>
          <input type="number" value={estimatedCost}
            onChange={e => setEstimatedCost(parseFloat(e.target.value))}
            min="0" style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Status:</label>
          <select value={status} onChange={e => setStatus(e.target.value)}
            style={{ width: '100%', padding: '8px' }}>
            <option value="Planned">Planned</option>
            <option value="Reserved">Reserved</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button type="submit" disabled={loading} style={{ padding: '10px 20px' }}>
            {loading ? 'Saving...' : isEdit ? 'Update' : 'Add'}
          </button>
          <button type="button" onClick={goBack} style={{ padding: '10px 20px' }}>Cancel</button>
        </div>
      </form>
    </div>
  );
};

export default ActivityFormPage;