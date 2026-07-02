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
  const [downloadingPdf, setDownloadingPdf] = useState(false);

  const [calendarMonth, setCalendarMonth] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState<string | null>(null);

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
        if (planData.startDate) setCalendarMonth(new Date(planData.startDate));
      } catch (err) {
        console.error('Error loading plan data:', err);
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
    setExpenses(prev => prev.filter(e => e.id !== expId));
  };

  const handleDownloadPdf = async () => {
    if (!plan) return;
    setDownloadingPdf(true);
    try {
      await travelPlanService.downloadPdf(planId, plan.name);
    } catch (err) {
      console.error('Error downloading PDF:', err);
      alert('Failed to generate PDF report.');
    } finally {
      setDownloadingPdf(false);
    }
  };

  const activitiesByDate = activities.reduce<Record<string, Activity[]>>((acc, a) => {
    const d = a.date?.split('T')[0];
    if (d) {
      if (!acc[d]) acc[d] = [];
      acc[d].push(a);
    }
    return acc;
  }, {});

  const getCalendarDays = (): (string | null)[] => {
    const year = calendarMonth.getFullYear();
    const month = calendarMonth.getMonth();
    const firstDay = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const blanks: null[] = Array(firstDay === 0 ? 6 : firstDay - 1).fill(null);
    const days = Array.from({ length: daysInMonth }, (_, i) => {
      return new Date(year, month, i + 1).toISOString().split('T')[0];
    });
    return [...blanks, ...days];
  };

  const isPlanDate = (dateStr: string) => {
    if (!plan) return false;
    return dateStr >= plan.startDate.split('T')[0] && dateStr <= plan.endDate.split('T')[0];
  };

  if (loading) return <p>Loading...</p>;
  if (!plan) return <p>Plan not found.</p>;

  const totalExpenses = expenses.reduce((sum, e) => sum + Number(e.amount), 0);
  const remainingBudget = Number(plan.budget) - totalExpenses;

  const tabs = ['destinations', 'activities', 'calendar', 'expenses', 'checklist', 'share'];

  return (
    <div style={{ padding: '20px' }}>
      <button onClick={() => navigate('/dashboard')}>← Back</button>
      <h2>{plan.name}</h2>
      <button onClick={handleDownloadPdf} disabled={downloadingPdf} style={{ marginBottom: '10px' }}>
        {downloadingPdf ? 'Generating PDF...' : '📄 Download PDF Report'}
      </button>
      <p>{plan.description}</p>
      <p><strong>Period:</strong> {new Date(plan.startDate).toLocaleDateString()} - {new Date(plan.endDate).toLocaleDateString()}</p>
      <p>
        <strong>Budget:</strong> ${Number(plan.budget).toFixed(2)} |{' '}
        <strong>Spent:</strong> <span style={{ color: totalExpenses > Number(plan.budget) ? 'red' : 'inherit' }}>${totalExpenses.toFixed(2)}</span> |{' '}
        <strong>Remaining:</strong> <span style={{ color: remainingBudget < 0 ? 'red' : 'green' }}>${remainingBudget.toFixed(2)}</span>
      </p>
      {plan.notes && <p>{plan.notes}</p>}

      <div style={{ display: 'flex', gap: '10px', marginBottom: '20px', marginTop: '20px' }}>
        {tabs.map(tab => (
          <button key={tab} onClick={() => setActiveTab(tab)}
            style={{ padding: '8px 16px', backgroundColor: activeTab === tab ? '#007bff' : '#f0f0f0', color: activeTab === tab ? 'white' : 'black' }}>
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {/* Destinations */}
      {activeTab === 'destinations' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3>Destinations</h3>
            <button onClick={() => navigate(`/travel-plans/${planId}/destinations/create`)}>Add Destination</button>
          </div>
          {destinations.length === 0 && <p>No destinations yet.</p>}
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

      {/* Activities */}
      {activeTab === 'activities' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3>Activities</h3>
            <button onClick={() => navigate(`/travel-plans/${planId}/activities/create`)}>Add Activity</button>
          </div>
          {activities.length === 0 && <p>No activities yet.</p>}
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

      {/* Calendar */}
      {activeTab === 'calendar' && (
        <div>
          <h3>Calendar View</h3>
          <div style={{ display: 'flex', alignItems: 'center', gap: '20px', marginBottom: '10px' }}>
            <button onClick={() => setCalendarMonth(new Date(calendarMonth.getFullYear(), calendarMonth.getMonth() - 1))}>‹</button>
            <strong>{calendarMonth.toLocaleDateString('en-GB', { month: 'long', year: 'numeric' })}</strong>
            <button onClick={() => setCalendarMonth(new Date(calendarMonth.getFullYear(), calendarMonth.getMonth() + 1))}>›</button>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '4px', marginBottom: '4px' }}>
            {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map(d => (
              <div key={d} style={{ textAlign: 'center', fontWeight: 'bold', fontSize: '12px', color: '#666' }}>{d}</div>
            ))}
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '4px' }}>
            {getCalendarDays().map((dateStr, idx) => {
              if (!dateStr) return <div key={`blank-${idx}`} />;
              const dayActivities = activitiesByDate[dateStr] || [];
              const inPlan = isPlanDate(dateStr);
              const isSelected = selectedDate === dateStr;
              const dayNum = parseInt(dateStr.split('-')[2]);
              return (
                <div key={dateStr} onClick={() => setSelectedDate(isSelected ? null : dateStr)}
                  style={{ minHeight: '60px', padding: '4px', border: '1px solid #ddd', borderRadius: '6px',
                    backgroundColor: isSelected ? '#007bff' : inPlan ? '#e8f4fd' : '#fafafa',
                    cursor: 'pointer', color: isSelected ? 'white' : 'black' }}>
                  <div style={{ fontSize: '12px', fontWeight: 'bold' }}>{dayNum}</div>
                  {dayActivities.slice(0, 2).map(a => (
                    <div key={a.id} style={{ fontSize: '10px', backgroundColor: isSelected ? '#0056b3' : '#007bff',
                      color: 'white', borderRadius: '3px', padding: '1px 3px', marginTop: '2px',
                      overflow: 'hidden', whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>
                      {a.time ? a.time.slice(0, 5) + ' ' : ''}{a.name}
                    </div>
                  ))}
                  {dayActivities.length > 2 && (
                    <div style={{ fontSize: '10px', color: isSelected ? '#cce' : '#888' }}>+{dayActivities.length - 2} more</div>
                  )}
                </div>
              );
            })}
          </div>
          {selectedDate && (
            <div style={{ marginTop: '20px', border: '1px solid #ccc', borderRadius: '8px', padding: '15px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h4 style={{ margin: 0 }}>
                  {new Date(selectedDate + 'T00:00:00').toLocaleDateString('en-GB', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' })}
                </h4>
                <button onClick={() => navigate(`/travel-plans/${planId}/activities/create`)}>+ Add Activity</button>
              </div>
              {(activitiesByDate[selectedDate] || []).length === 0 ? (
                <p style={{ color: '#999' }}>No activities for this day.</p>
              ) : (
                (activitiesByDate[selectedDate] || [])
                  .sort((a, b) => (a.time || '').localeCompare(b.time || ''))
                  .map(a => (
                    <div key={a.id} style={{ border: '1px solid #eee', borderRadius: '6px', padding: '10px', marginTop: '10px' }}>
                      <strong>{a.name}</strong>
                      <p style={{ margin: '4px 0', fontSize: '13px', color: '#555' }}>
                        {a.time && `🕐 ${a.time}`} {a.location && `📍 ${a.location}`}
                      </p>
                      {a.description && <p style={{ margin: '4px 0', fontSize: '13px' }}>{a.description}</p>}
                      <p style={{ margin: '4px 0', fontSize: '13px' }}>
                        <strong>Cost:</strong> ${a.estimatedCost} | <strong>Status:</strong> {a.status}
                      </p>
                      <button onClick={() => navigate(`/travel-plans/${planId}/activities/${a.id}/edit`)}>Edit</button>
                      <button onClick={() => handleDeleteActivity(a.id)} style={{ marginLeft: '10px', color: 'red' }}>Delete</button>
                    </div>
                  ))
              )}
            </div>
          )}
        </div>
      )}

      {/* Expenses */}
      {activeTab === 'expenses' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3>Expenses</h3>
            <button onClick={() => navigate(`/travel-plans/${planId}/expenses/create`)}>Add Expense</button>
          </div>
          <p>
            <strong>Total Spent:</strong> ${totalExpenses.toFixed(2)} / ${Number(plan.budget).toFixed(2)} |{' '}
            <strong>Remaining:</strong>{' '}
            <span style={{ color: remainingBudget < 0 ? 'red' : 'green' }}>${remainingBudget.toFixed(2)}</span>
          </p>
          {expenses.length === 0 && <p>No expenses yet.</p>}
          {expenses.map(e => (
            <div key={e.id} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
              <h4>{e.name} — {e.category}</h4>
              <p>${Number(e.amount).toFixed(2)} on {new Date(e.date).toLocaleDateString()}</p>
              {e.description && <p>{e.description}</p>}
              <button onClick={() => navigate(`/travel-plans/${planId}/expenses/${e.id}/edit`)}>Edit</button>
              <button onClick={() => handleDeleteExpense(e.id)} style={{ marginLeft: '10px', color: 'red' }}>Delete</button>
            </div>
          ))}
        </div>
      )}

      {/* Checklist */}
      {activeTab === 'checklist' && (
        <div>
          <h3>Checklist</h3>
          <form onSubmit={handleAddChecklistItem} style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
            <input type="text" value={newChecklistItem} onChange={e => setNewChecklistItem(e.target.value)}
              placeholder="Add new item..." style={{ flex: 1, padding: '8px' }} />
            <button type="submit">Add</button>
          </form>
          {checklist.length === 0 && <p>No checklist items yet.</p>}
          {checklist.map(item => (
            <div key={item.id} style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '10px' }}>
              <input type="checkbox" checked={item.isCompleted} onChange={() => handleToggleChecklist(item.id)} />
              <span style={{ textDecoration: item.isCompleted ? 'line-through' : 'none' }}>{item.name}</span>
              <button onClick={() => handleDeleteChecklistItem(item.id)} style={{ color: 'red' }}>Delete</button>
            </div>
          ))}
        </div>
      )}

      {/* Share */}
      {activeTab === 'share' && (
        <div>
          <button onClick={() => navigate(`/travel-plans/${planId}/share`)}>Manage Share Tokens</button>
        </div>
      )}
    </div>
  );
};

export default TravelPlanDetailPage;