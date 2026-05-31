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
    <div style={{ maxWidth: '600px', margin: '50px auto', padding: '20px' }}>
      <h2>{isEdit ? 'Edit Travel Plan' : 'Create Travel Plan'}</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '10px' }}>
          <label>Name:</label>
          <input type="text" value={name} onChange={e => setName(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Description:</label>
          <textarea value={description} onChange={e => setDescription(e.target.value)}
            style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Start Date:</label>
          <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>End Date:</label>
          <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Budget ($):</label>
          <input type="number" value={budget} onChange={e => setBudget(parseFloat(e.target.value))}
            min="0" required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Notes:</label>
          <textarea value={notes} onChange={e => setNotes(e.target.value)}
            style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button type="submit" disabled={loading} style={{ padding: '10px 20px' }}>
            {loading ? 'Saving...' : isEdit ? 'Update' : 'Create'}
          </button>
          <button type="button" onClick={() => navigate('/dashboard')} style={{ padding: '10px 20px' }}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default TravelPlanFormPage;