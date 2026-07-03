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
  }, [id, planId, isEdit, shareToken]);

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
    <div className="page-narrow">
      <div className="card">
        <span className="eyebrow">📅 Activity</span>
        <h2>{isEdit ? 'Edit Activity' : 'Add Activity'}</h2>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Name</label>
            <input type="text" className="input" value={name} onChange={e => setName(e.target.value)} required />
          </div>
          <div className="field">
            <label>Date</label>
            <input type="date" className="input" value={date} onChange={e => setDate(e.target.value)} required />
          </div>
          <div className="field">
            <label>Time</label>
            <input type="time" className="input" value={time} onChange={e => setTime(e.target.value)} />
          </div>
          <div className="field">
            <label>Location</label>
            <input type="text" className="input" value={location_} onChange={e => setLocation(e.target.value)} />
          </div>
          <div className="field">
            <label>Description</label>
            <textarea className="textarea" value={description} onChange={e => setDescription(e.target.value)} />
          </div>
          <div className="field">
            <label>Estimated Cost ($)</label>
            <input type="number" className="input" value={estimatedCost}
              onChange={e => setEstimatedCost(parseFloat(e.target.value))} min="0" />
          </div>
          <div className="field">
            <label>Status</label>
            <select className="select" value={status} onChange={e => setStatus(e.target.value)}>
              <option value="Planned">Planned</option>
              <option value="Reserved">Reserved</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
          <div className="btn-row">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : isEdit ? 'Update' : 'Add'}
            </button>
            <button type="button" className="btn btn-outline" onClick={goBack}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ActivityFormPage;