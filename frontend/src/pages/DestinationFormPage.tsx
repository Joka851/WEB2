import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { destinationService } from '../services/destinationService';
import { CreateDestination } from '../models/Destination';

const DestinationFormPage: React.FC = () => {
  const { planId, id } = useParams<{ planId: string; id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [arrivalDate, setArrivalDate] = useState('');
  const [departureDate, setDepartureDate] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      const fetchDestination = async () => {
        try {
          const destinations = await destinationService.getAll(parseInt(planId!));
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
  }, [id, planId, isEdit]);

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
        name, location, arrivalDate, departureDate, description
      };

      if (isEdit) {
        await destinationService.update(parseInt(planId!), parseInt(id!), data);
      } else {
        await destinationService.create(parseInt(planId!), data);
      }
      navigate(`/travel-plans/${planId}`);
    } catch {
      setError('Failed to save destination.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '50px auto', padding: '20px' }}>
      <h2>{isEdit ? 'Edit Destination' : 'Add Destination'}</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '10px' }}>
          <label>Name:</label>
          <input type="text" value={name} onChange={e => setName(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Location:</label>
          <input type="text" value={location} onChange={e => setLocation(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Arrival Date:</label>
          <input type="date" value={arrivalDate} onChange={e => setArrivalDate(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Departure Date:</label>
          <input type="date" value={departureDate} onChange={e => setDepartureDate(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Description:</label>
          <textarea value={description} onChange={e => setDescription(e.target.value)}
            style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button type="submit" disabled={loading} style={{ padding: '10px 20px' }}>
            {loading ? 'Saving...' : isEdit ? 'Update' : 'Add'}
          </button>
          <button type="button" onClick={() => navigate(`/travel-plans/${planId}`)}
            style={{ padding: '10px 20px' }}>Cancel</button>
        </div>
      </form>
    </div>
  );
};

export default DestinationFormPage;