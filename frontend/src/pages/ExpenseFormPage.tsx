import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { expenseService } from '../services/expenseService';
import { CreateExpense } from '../models/Expense';

const CATEGORIES = ['Transport', 'Accommodation', 'Food', 'Activities', 'Shopping', 'Insurance', 'Other'];

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
          const expense = await expenseService.getById(parseInt(planId!), parseInt(id!));
          setName(expense.name);
          setCategory(expense.category);
          setAmount(expense.amount);
          setDate(expense.date.split('T')[0]);
          setDescription(expense.description || '');
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
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save expense.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-narrow">
      <div className="card">
        <span className="eyebrow"> Expense</span>
        <h2>{isEdit ? 'Edit Expense' : 'Add Expense'}</h2>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Name</label>
            <input type="text" className="input" value={name} onChange={e => setName(e.target.value)} required />
          </div>
          <div className="field">
            <label>Category</label>
            <select className="select" value={category} onChange={e => setCategory(e.target.value)}>
              {CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
            </select>
          </div>
          <div className="field">
            <label>Amount (€)</label>
            <input type="number" className="input" value={amount} onChange={e => setAmount(parseFloat(e.target.value))}
              min="0" step="0.01" required />
          </div>
          <div className="field">
            <label>Date</label>
            <input type="date" className="input" value={date} onChange={e => setDate(e.target.value)} required />
          </div>
          <div className="field">
            <label>Description</label>
            <textarea className="textarea" value={description} onChange={e => setDescription(e.target.value)} rows={3} />
          </div>
          <div className="btn-row">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : isEdit ? 'Update' : 'Add'}
            </button>
            <button type="button" className="btn btn-outline" onClick={() => navigate(`/travel-plans/${planId}`)}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ExpenseFormPage;