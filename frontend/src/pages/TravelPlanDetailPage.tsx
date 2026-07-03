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

  if (loading) return <div className="page"><p style={{ color: 'var(--ink-soft)' }}>Loading...</p></div>;
  if (!plan) return <div className="page"><p>Plan not found.</p></div>;

  const totalExpenses = expenses.reduce((sum, e) => sum + Number(e.amount), 0);
  const remainingBudget = Number(plan.budget) - totalExpenses;

  const tabLabels: Record<string, string> = {
    destinations: ' Destinations',
    activities: ' Activities',
    calendar: ' Calendar',
    expenses: ' Expenses',
    checklist: ' Checklist',
    share: ' Share'
  };
  const tabs = ['destinations', 'activities', 'calendar', 'expenses', 'checklist', 'share'];

  return (
    <div className="page">
      <button className="btn btn-text" style={{ paddingLeft: 0 }} onClick={() => navigate('/dashboard')}>← Back</button>

      <div className="topbar" style={{ alignItems: 'flex-start' }}>
        <div>
          <span className="eyebrow"> Travel Plan</span>
          <h2 style={{ margin: 0 }}>{plan.name}</h2>
        </div>
        <button className="btn btn-outline" onClick={handleDownloadPdf} disabled={downloadingPdf}>
          {downloadingPdf ? 'Generating PDF...' : ' Download PDF Report'}
        </button>
      </div>

      <div className="card">
        <p style={{ marginBottom: '8px' }}>{plan.description}</p>
        <p style={{ marginBottom: '8px' }}>
          <strong>Period:</strong> {new Date(plan.startDate).toLocaleDateString()} - {new Date(plan.endDate).toLocaleDateString()}
        </p>
        <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
          <span className="badge badge-primary">Budget: ${Number(plan.budget).toFixed(2)}</span>
          <span className={`badge ${totalExpenses > Number(plan.budget) ? 'badge-danger' : 'badge-muted'}`}>
            Spent: ${totalExpenses.toFixed(2)}
          </span>
          <span className={`badge ${remainingBudget < 0 ? 'badge-danger' : 'badge-success'}`}>
            Remaining: ${remainingBudget.toFixed(2)}
          </span>
        </div>
        {plan.notes && <p style={{ marginTop: '10px', marginBottom: 0, color: 'var(--ink-soft)' }}>{plan.notes}</p>}
      </div>

      <div className="tabs">
        {tabs.map(tab => (
          <button
            key={tab}
            className={`tab ${activeTab === tab ? 'tab-active' : ''}`}
            onClick={() => setActiveTab(tab)}
          >
            {tabLabels[tab]}
          </button>
        ))}
      </div>

      {/* Destinations */}
      {activeTab === 'destinations' && (
        <div>
          <div className="topbar" style={{ marginBottom: '14px' }}>
            <h3 style={{ margin: 0 }}>Destinations</h3>
            <button className="btn btn-accent btn-sm" onClick={() => navigate(`/travel-plans/${planId}/destinations/create`)}>+ Add Destination</button>
          </div>
          {destinations.length === 0 && <div className="empty-state">No destinations yet.</div>}
          {destinations.map(d => (
            <div key={d.id} className="card">
              <h4>{d.name} — {d.location}</h4>
              <p style={{ fontSize: '13px', color: 'var(--ink-soft)' }}>
                {new Date(d.arrivalDate).toLocaleDateString()} - {new Date(d.departureDate).toLocaleDateString()}
              </p>
              <p>{d.description}</p>
              <div className="btn-row">
                <button className="btn btn-outline btn-sm" onClick={() => navigate(`/travel-plans/${planId}/destinations/${d.id}/edit`)}>Edit</button>
                <button className="btn btn-danger btn-sm" onClick={() => handleDeleteDestination(d.id)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Activities */}
      {activeTab === 'activities' && (
        <div>
          <div className="topbar" style={{ marginBottom: '14px' }}>
            <h3 style={{ margin: 0 }}>Activities</h3>
            <button className="btn btn-accent btn-sm" onClick={() => navigate(`/travel-plans/${planId}/activities/create`)}>+ Add Activity</button>
          </div>
          {activities.length === 0 && <div className="empty-state">No activities yet.</div>}
          {activities.map(a => (
            <div key={a.id} className="card">
              <h4>{a.name}</h4>
              <p style={{ fontSize: '13px', color: 'var(--ink-soft)' }}>
                {new Date(a.date).toLocaleDateString()} at {a.time} — {a.location}
              </p>
              <p>{a.description}</p>
              <p style={{ fontSize: '13px' }}><strong>Cost:</strong> ${a.estimatedCost} | <strong>Status:</strong> {a.status}</p>
              <div className="btn-row">
                <button className="btn btn-outline btn-sm" onClick={() => navigate(`/travel-plans/${planId}/activities/${a.id}/edit`)}>Edit</button>
                <button className="btn btn-danger btn-sm" onClick={() => handleDeleteActivity(a.id)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Calendar */}
      {activeTab === 'calendar' && (
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '14px' }}>
            <button className="btn btn-outline btn-icon-only" onClick={() => setCalendarMonth(new Date(calendarMonth.getFullYear(), calendarMonth.getMonth() - 1))}>‹</button>
            <strong style={{ fontFamily: "'Poppins', sans-serif" }}>{calendarMonth.toLocaleDateString('en-GB', { month: 'long', year: 'numeric' })}</strong>
            <button className="btn btn-outline btn-icon-only" onClick={() => setCalendarMonth(new Date(calendarMonth.getFullYear(), calendarMonth.getMonth() + 1))}>›</button>
          </div>
          <div className="calendar-grid" style={{ marginBottom: '6px' }}>
            {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map(d => (
              <div key={d} style={{ textAlign: 'center', fontWeight: 600, fontSize: '12px', color: 'var(--ink-soft)' }}>{d}</div>
            ))}
          </div>
          <div className="calendar-grid">
            {getCalendarDays().map((dateStr, idx) => {
              if (!dateStr) return <div key={`blank-${idx}`} />;
              const dayActivities = activitiesByDate[dateStr] || [];
              const inPlan = isPlanDate(dateStr);
              const isSelected = selectedDate === dateStr;
              const dayNum = parseInt(dateStr.split('-')[2]);
              return (
                <div
                  key={dateStr}
                  onClick={() => setSelectedDate(isSelected ? null : dateStr)}
                  className={`calendar-cell ${isSelected ? 'calendar-cell-selected' : inPlan ? 'calendar-cell-inplan' : ''}`}
                >
                  <div style={{ fontSize: '12px', fontWeight: 700 }}>{dayNum}</div>
                  {dayActivities.slice(0, 2).map(a => (
                    <div key={a.id} style={{
                      fontSize: '10px',
                      backgroundColor: isSelected ? 'rgba(255,255,255,0.25)' : 'var(--primary)',
                      color: '#fff', borderRadius: '4px', padding: '1px 4px', marginTop: '2px',
                      overflow: 'hidden', whiteSpace: 'nowrap', textOverflow: 'ellipsis'
                    }}>
                      {a.time ? a.time.slice(0, 5) + ' ' : ''}{a.name}
                    </div>
                  ))}
                  {dayActivities.length > 2 && (
                    <div style={{ fontSize: '10px', opacity: 0.8 }}>+{dayActivities.length - 2} more</div>
                  )}
                </div>
              );
            })}
          </div>
          {selectedDate && (
            <div className="card" style={{ marginTop: '18px' }}>
              <div className="topbar" style={{ marginBottom: '8px' }}>
                <h4 style={{ margin: 0 }}>
                  {new Date(selectedDate + 'T00:00:00').toLocaleDateString('en-GB', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' })}
                </h4>
                <button className="btn btn-accent btn-sm" onClick={() => navigate(`/travel-plans/${planId}/activities/create`)}>+ Add Activity</button>
              </div>
              {(activitiesByDate[selectedDate] || []).length === 0 ? (
                <p style={{ color: 'var(--ink-soft)' }}>No activities for this day.</p>
              ) : (
                (activitiesByDate[selectedDate] || [])
                  .sort((a, b) => (a.time || '').localeCompare(b.time || ''))
                  .map(a => (
                    <div key={a.id} style={{ border: '1px solid var(--border)', borderRadius: 'var(--radius-sm)', padding: '10px', marginTop: '10px' }}>
                      <strong>{a.name}</strong>
                      <p style={{ margin: '4px 0', fontSize: '13px', color: 'var(--ink-soft)' }}>
                        {a.time && ` ${a.time}`} {a.location && ` ${a.location}`}
                      </p>
                      {a.description && <p style={{ margin: '4px 0', fontSize: '13px' }}>{a.description}</p>}
                      <p style={{ margin: '4px 0', fontSize: '13px' }}>
                        <strong>Cost:</strong> ${a.estimatedCost} | <strong>Status:</strong> {a.status}
                      </p>
                      <div className="btn-row">
                        <button className="btn btn-outline btn-sm" onClick={() => navigate(`/travel-plans/${planId}/activities/${a.id}/edit`)}>Edit</button>
                        <button className="btn btn-danger btn-sm" onClick={() => handleDeleteActivity(a.id)}>Delete</button>
                      </div>
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
          <div className="topbar" style={{ marginBottom: '10px' }}>
            <h3 style={{ margin: 0 }}>Expenses</h3>
            <button className="btn btn-accent btn-sm" onClick={() => navigate(`/travel-plans/${planId}/expenses/create`)}>+ Add Expense</button>
          </div>
          <p style={{ marginBottom: '14px' }}>
            <strong>Total Spent:</strong> ${totalExpenses.toFixed(2)} / ${Number(plan.budget).toFixed(2)} &nbsp;
            <span className={`badge ${remainingBudget < 0 ? 'badge-danger' : 'badge-success'}`}>
              Remaining: ${remainingBudget.toFixed(2)}
            </span>
          </p>
          {expenses.length === 0 && <div className="empty-state">No expenses yet.</div>}
          {expenses.map(e => (
            <div key={e.id} className="card">
              <h4>{e.name} — {e.category}</h4>
              <p style={{ fontSize: '13px', color: 'var(--ink-soft)' }}>${Number(e.amount).toFixed(2)} on {new Date(e.date).toLocaleDateString()}</p>
              {e.description && <p>{e.description}</p>}
              <div className="btn-row">
                <button className="btn btn-outline btn-sm" onClick={() => navigate(`/travel-plans/${planId}/expenses/${e.id}/edit`)}>Edit</button>
                <button className="btn btn-danger btn-sm" onClick={() => handleDeleteExpense(e.id)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Checklist */}
      {activeTab === 'checklist' && (
        <div>
          <h3>Checklist</h3>
          <form onSubmit={handleAddChecklistItem} style={{ display: 'flex', gap: '10px', marginBottom: '18px' }}>
            <input
              type="text"
              className="input"
              value={newChecklistItem}
              onChange={e => setNewChecklistItem(e.target.value)}
              placeholder="Add new item..."
            />
            <button type="submit" className="btn btn-accent">Add</button>
          </form>
          {checklist.length === 0 && <div className="empty-state">No checklist items yet.</div>}
          {checklist.length > 0 && (
            <div className="card">
              {checklist.map(item => (
                <div key={item.id} className="checkbox-row" style={{ marginBottom: '10px', justifyContent: 'space-between' }}>
                  <div className="checkbox-row">
                    <input type="checkbox" checked={item.isCompleted} onChange={() => handleToggleChecklist(item.id)} />
                    <span style={{ textDecoration: item.isCompleted ? 'line-through' : 'none', color: item.isCompleted ? 'var(--ink-soft)' : 'var(--ink)' }}>
                      {item.name}
                    </span>
                  </div>
                  <button className="btn btn-text" style={{ color: 'var(--danger)' }} onClick={() => handleDeleteChecklistItem(item.id)}>Delete</button>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Share */}
      {activeTab === 'share' && (
        <div>
          <div className="empty-state">
            <p style={{ marginBottom: '14px' }}>Generate a link or QR code so others can view or edit this plan.</p>
            <button className="btn btn-primary" onClick={() => navigate(`/travel-plans/${planId}/share`)}>Manage Share Tokens</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default TravelPlanDetailPage;