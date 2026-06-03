import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { expenseService } from '../services/expenseService';
import { CreateExpense } from '../models/Expense';

const CATEGORIES = ['Transport', 'Accommodation', 'Food', 'Tickets', 'Shopping', 'Other'];

const ExpenseFormPage: React.FC = () => {
  const { planId, id } = useParams<{ planId: string; id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [name, setName] = useState('');
  const [category, setCategory] = useState('Transport');
  const [amount, setAmount] = useState(0);
  const [date, setDate] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit) {
      const fetchExpense = async () => {
        try {
          const expenses = await expenseService.getAll(parseInt(planId!));
          const exp = expenses.find(e => e.id === parseInt(id!));
          if (exp) {
            setName(exp.name);
            setCategory(exp.category);
            setAmount(exp.amount);
            setDate(exp.date.split('T')[0]);
            setDescription(exp.description);
          }
        } catch {
          setError('Failed to load expense.');
        }
      };
      fetchExpense();
    }
  }, [id, planId, isEdit]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (amount < 0) {
      setError('Amount cannot be negative.');
      return;
    }

    setLoading(true);
    try {
      const data: CreateExpense = { name, category, amount, date, description };
      if (isEdit) {
        await expenseService.update(parseInt(planId!), parseInt(id!), data);
      } else {
        await expenseService.create(parseInt(planId!), data);
      }
      navigate(`/travel-plans/${planId}`);
    } catch {
      setError('Failed to save expense.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '50px auto', padding: '20px' }}>
      <h2>{isEdit ? 'Edit Expense' : 'Add Expense'}</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '10px' }}>
          <label>Name:</label>
          <input type="text" value={name} onChange={e => setName(e.target.value)}
            required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Category:</label>
          <select value={category} onChange={e => setCategory(e.target.value)}
            style={{ width: '100%', padding: '8px' }}>
            {CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Amount ($):</label>
          <input type="number" value={amount} onChange={e => setAmount(parseFloat(e.target.value))}
            min="0" required style={{ width: '100%', padding: '8px' }} />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Date:</label>
          <input type="date" value={date} onChange={e => setDate(e.target.value)}
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

export default ExpenseFormPage;