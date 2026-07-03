import React, { useEffect, useState } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { destinationService } from '../services/destinationService';
import { CreateDestination } from '../models/Destination';

const DestinationFormPage: React.FC = () => {
  const { planId, id } = useParams<{ planId: string; id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const isEdit = !!id;

  const returnTo = (location.state as { returnTo?: string; shareToken?: string } | null)?.returnTo;
  const shareToken = (location.state as { returnTo?: string; shareToken?: string } | null)?.shareToken;
  const goBack = () => navigate(returnTo || `/travel-plans/${planId}`);

  const [name, setName] = useState('');
  const [location_, setLocation] = useState('');
  const [arrivalDate, setArrivalDate] = useState('');
  const [departureDate, setDepartureDate] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      const fetchDestination = async () => {
        try {
          const destinations = await destinationService.getAll(parseInt(planId!), shareToken);
          const dest = destinations.find(d => d.id === parseInt(id!));
          if (dest) {
            setName(dest.name);
            setLocation(dest.location);
            setArrivalDate(dest.arrivalDate.split('T')[0]);
            setDepartureDate(dest.departureDate.split('T')[0]);
            setDescription(dest.description);
          }
        } catch {
          setError('Failed to load destination.');
        }
      };
      fetchDestination();
    }
  }, [id, planId, isEdit, shareToken]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (new Date(departureDate) < new Date(arrivalDate)) {
      setError('Departure date cannot be before arrival date.');
      return;
    }

    setLoading(true);
    try {
      const data: CreateDestination = {
        name, location: location_, arrivalDate, departureDate, description
      };

      if (isEdit) {
        await destinationService.update(parseInt(planId!), parseInt(id!), data, shareToken);
      } else {
        await destinationService.create(parseInt(planId!), data, shareToken);
      }
      goBack();
    } catch {
      setError('Failed to save destination.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-narrow">
      <div className="card">
        <span className="eyebrow">📍 Destination</span>
        <h2>{isEdit ? 'Edit Destination' : 'Add Destination'}</h2>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Name</label>
            <input type="text" className="input" value={name} onChange={e => setName(e.target.value)} required />
          </div>
          <div className="field">
            <label>Location</label>
            <input type="text" className="input" value={location_} onChange={e => setLocation(e.target.value)} required />
          </div>
          <div className="field">
            <label>Arrival Date</label>
            <input type="date" className="input" value={arrivalDate} onChange={e => setArrivalDate(e.target.value)} required />
          </div>
          <div className="field">
            <label>Departure Date</label>
            <input type="date" className="input" value={departureDate} onChange={e => setDepartureDate(e.target.value)} required />
          </div>
          <div className="field">
            <label>Description</label>
            <textarea className="textarea" value={description} onChange={e => setDescription(e.target.value)} />
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

export default DestinationFormPage;