import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { TravelPlan } from '../models/TravelPlan';
import { Destination } from '../models/Destination';
import { Activity } from '../models/Activity';
import { Expense } from '../models/Expense';
import { ChecklistItem } from '../models/ChecklistItem';
import { travelPlanService } from '../services/travelPlanService';
import { destinationService } from '../services/destinationService';
import { activityService } from '../services/activityService';
import { expenseService } from '../services/expenseService';
import { checklistService } from '../services/checklistService';

const TravelPlanDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const planId = parseInt(id!);

  const [plan, setPlan] = useState<TravelPlan | null>(null);
  const [destinations, setDestinations] = useState<Destination[]>([]);
  const [activities, setActivities] = useState<Activity[]>([]);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [checklist, setChecklist] = useState<ChecklistItem[]>([]);
  const [activeTab, setActiveTab] = useState('destinations');
  const [loading, setLoading] = useState(true);
  const [newChecklistItem, setNewChecklistItem] = useState('');

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [planData, destData, actData, expData, checkData] = await Promise.all([
          travelPlanService.getById(planId),
          destinationService.getAll(planId),
          activityService.getAll(planId),
          expenseService.getAll(planId),
          checklistService.getAll(planId)
        ]);
        setPlan(planData);
        setDestinations(destData);
        setActivities(actData);
        setExpenses(expData);
        setChecklist(checkData);
      } catch {
      } finally {
        setLoading(false);
      }
    };
    fetchAll();
  }, [planId]);

  const handleToggleChecklist = async (itemId: number) => {
    const updated = await checklistService.toggle(planId, itemId);
    setChecklist(checklist.map(c => c.id === itemId ? updated : c));
  };

  const handleAddChecklistItem = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newChecklistItem.trim()) return;
    const item = await checklistService.create(planId, { name: newChecklistItem });
    setChecklist([...checklist, item]);
    setNewChecklistItem('');
  };

  const handleDeleteChecklistItem = async (itemId: number) => {
    await checklistService.delete(planId, itemId);
    setChecklist(checklist.filter(c => c.id !== itemId));
  };

  const handleDeleteDestination = async (destId: number) => {
    await destinationService.delete(planId, destId);
    setDestinations(destinations.filter(d => d.id !== destId));
  };

  const handleDeleteActivity = async (actId: number) => {
    await activityService.delete(planId, actId);
    setActivities(activities.filter(a => a.id !== actId));
  };

  const handleDeleteExpense = async (expId: number) => {
    await expenseService.delete(planId, expId);
    setExpenses(expenses.filter(e => e.id !== expId));
  };

  if (loading) return <p>Loading...</p>;
  if (!plan) return <p>Plan not found.</p>;

  const totalExpenses = expenses.reduce((sum, e) => sum + e.amount, 0);
  const remainingBudget = plan.budget - totalExpenses;

  return (
    <div style={{ padding: '20px' }}>
      <button onClick={() => navigate('/dashboard')}>← Back</button>
      <h2>{plan.name}</h2>
      <p>{plan.description}</p>
      <p><strong>Period:</strong> {new Date(plan.startDate).toLocaleDateString()} - {new Date(plan.endDate).toLocaleDateString()}</p>
      <p><strong>Budget:</strong> ${plan.budget} | <strong>Spent:</strong> ${totalExpenses.toFixed(2)} | <strong>Remaining:</strong> ${remainingBudget.toFixed(2)}</p>
      <p>{plan.notes}</p>

      <div style={{ display: 'flex', gap: '10px', marginBottom: '20px', marginTop: '20px' }}>
        {['destinations', 'activities', 'expenses', 'checklist', 'share'].map(tab => (
          <button key={tab} onClick={() => setActiveTab(tab)}
            style={{ padding: '8px 16px', backgroundColor: activeTab === tab ? '#007bff' : '#f0f0f0', color: activeTab === tab ? 'white' : 'black' }}>
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {activeTab === 'destinations' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3>Destinations</h3>
            <button onClick={() => navigate(`/travel-plans/${planId}/destinations/create`)}>Add Destination</button>
          </div>
          {destinations.map(d => (
            <div key={d.id} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
              <h4>{d.name} — {d.location}</h4>
              <p>{new Date(d.arrivalDate).toLocaleDateString()} - {new Date(d.departureDate).toLocaleDateString()}</p>
              <p>{d.description}</p>
              <button onClick={() => navigate(`/travel-plans/${planId}/destinations/${d.id}/edit`)}>Edit</button>
              <button onClick={() => handleDeleteDestination(d.id)} style={{ marginLeft: '10px', color: 'red' }}>Delete</button>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'activities' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3>Activities</h3>
            <button onClick={() => navigate(`/travel-plans/${planId}/activities/create`)}>Add Activity</button>
          </div>
          {activities.map(a => (
            <div key={a.id} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
              <h4>{a.name}</h4>
              <p>{new Date(a.date).toLocaleDateString()} at {a.time} — {a.location}</p>
              <p>{a.description}</p>
              <p><strong>Cost:</strong> ${a.estimatedCost} | <strong>Status:</strong> {a.status}</p>
              <button onClick={() => navigate(`/travel-plans/${planId}/activities/${a.id}/edit`)}>Edit</button>
              <button onClick={() => handleDeleteActivity(a.id)} style={{ marginLeft: '10px', color: 'red' }}>Delete</button>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'expenses' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3>Expenses</h3>
            <button onClick={() => navigate(`/travel-plans/${planId}/expenses/create`)}>Add Expense</button>
          </div>
          <p><strong>Total:</strong> ${totalExpenses.toFixed(2)} / ${plan.budget} | <strong>Remaining:</strong> ${remainingBudget.toFixed(2)}</p>
          {expenses.map(e => (
            <div key={e.id} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
              <h4>{e.name} — {e.category}</h4>
              <p>${e.amount} on {new Date(e.date).toLocaleDateString()}</p>
              <p>{e.description}</p>
              <button onClick={() => navigate(`/travel-plans/${planId}/expenses/${e.id}/edit`)}>Edit</button>
              <button onClick={() => handleDeleteExpense(e.id)} style={{ marginLeft: '10px', color: 'red' }}>Delete</button>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'checklist' && (
        <div>
          <h3>Checklist</h3>
          <form onSubmit={handleAddChecklistItem} style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
            <input type="text" value={newChecklistItem} onChange={e => setNewChecklistItem(e.target.value)}
              placeholder="Add new item..." style={{ flex: 1, padding: '8px' }} />
            <button type="submit">Add</button>
          </form>
          {checklist.map(item => (
            <div key={item.id} style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '10px' }}>
              <input type="checkbox" checked={item.isCompleted} onChange={() => handleToggleChecklist(item.id)} />
              <span style={{ textDecoration: item.isCompleted ? 'line-through' : 'none' }}>{item.name}</span>
              <button onClick={() => handleDeleteChecklistItem(item.id)} style={{ color: 'red' }}>Delete</button>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'share' && (
        <div>
          <button onClick={() => navigate(`/travel-plans/${planId}/share`)}>Manage Share Tokens</button>
        </div>
      )}
    </div>
  );
};

export default TravelPlanDetailPage;