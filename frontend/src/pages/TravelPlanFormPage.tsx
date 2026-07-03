import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { travelPlanService } from '../services/travelPlanService';
import { CreateTravelPlan } from '../models/TravelPlan';

const TravelPlanFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [budget, setBudget] = useState(0);
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      const fetchPlan = async () => {
        try {
          const plan = await travelPlanService.getById(parseInt(id!));
          setName(plan.name);
          setDescription(plan.description);
          setStartDate(plan.startDate.split('T')[0]);
          setEndDate(plan.endDate.split('T')[0]);
          setBudget(plan.budget);
          setNotes(plan.notes);
        } catch {
          setError('Failed to load plan.');
        }
      };
      fetchPlan();
    }
  }, [id, isEdit]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (new Date(endDate) < new Date(startDate)) {
      setError('End date cannot be before start date.');
      return;
    }

    if (budget < 0) {
      setError('Budget cannot be negative.');
      return;
    }

    setLoading(true);
    try {
      const data: CreateTravelPlan = {
        userId: user!.id,
        name,
        description,
        startDate,
        endDate,
        budget,
        notes
      };

      if (isEdit) {
        await travelPlanService.update(parseInt(id!), data);
      } else {
        await travelPlanService.create(data);
      }
      navigate('/dashboard');
    } catch {
      setError('Failed to save plan.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-narrow">
      <div className="card">
        <span className="eyebrow"> {isEdit ? 'Editing' : 'New adventure'}</span>
        <h2>{isEdit ? 'Edit Travel Plan' : 'Create Travel Plan'}</h2>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Name</label>
            <input type="text" className="input" value={name} onChange={e => setName(e.target.value)} required />
          </div>
          <div className="field">
            <label>Description</label>
            <textarea className="textarea" value={description} onChange={e => setDescription(e.target.value)} />
          </div>
          <div className="field">
            <label>Start Date</label>
            <input type="date" className="input" value={startDate} onChange={e => setStartDate(e.target.value)} required />
          </div>
          <div className="field">
            <label>End Date</label>
            <input type="date" className="input" value={endDate} onChange={e => setEndDate(e.target.value)} required />
          </div>
          <div className="field">
            <label>Budget ($)</label>
            <input type="number" className="input" value={budget}
              onChange={e => setBudget(parseFloat(e.target.value))} min="0" required />
          </div>
          <div className="field">
            <label>Notes</label>
            <textarea className="textarea" value={notes} onChange={e => setNotes(e.target.value)} />
          </div>
          <div className="btn-row">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : isEdit ? 'Update' : 'Create'}
            </button>
            <button type="button" className="btn btn-outline" onClick={() => navigate('/dashboard')}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TravelPlanFormPage;