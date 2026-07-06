import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { shareService } from '../services/shareService';
import { ShareToken } from '../models/ShareToken';
import { QRCodeSVG } from 'qrcode.react';

const SharePage: React.FC = () => {
  const { planId } = useParams<{ planId: string }>();
  const navigate = useNavigate();
  const [tokens, setTokens] = useState<ShareToken[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [accessType, setAccessType] = useState<'VIEW' | 'EDIT'>('VIEW');
  const [expiresInDays, setExpiresInDays] = useState(7);

  useEffect(() => {
    const fetchTokens = async () => {
      try {
        const data = await shareService.getTokens(parseInt(planId!));
        setTokens(data);
      } catch {
        setError('Failed to load share tokens.');
      } finally {
        setLoading(false);
      }
    };
    fetchTokens();
  }, [planId]);

  const handleCreate = async () => {
    try {
      const expiresAt = new Date();
      expiresAt.setDate(expiresAt.getDate() + expiresInDays);

      await shareService.createToken(parseInt(planId!), {
        accessType,
        expiresAt: expiresAt.toISOString()
      });

      
      const data = await shareService.getTokens(parseInt(planId!));
      setTokens(data);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create token.');
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await shareService.deleteToken(parseInt(planId!), id);
      setTokens(tokens.filter(t => t.id !== id));
    } catch {
      setError('Failed to delete token.');
    }
  };

  const getShareUrl = (token: string) => `${window.location.origin}/shared/${token}`;

  return (
    <div className="page" style={{ maxWidth: '760px' }}>
      <button className="btn btn-text" style={{ paddingLeft: 0 }} onClick={() => navigate(`/travel-plans/${planId}`)}>← Back</button>
      <span className="eyebrow"> Sharing</span>
      <h2>Share Travel Plan</h2>
      {error && <div className="alert alert-error">{error}</div>}

      <div className="card">
        <h3>Generate New Share Link</h3>

        <div className="field">
          <label>Access Type</label>
          <select
            className="select"
            value={accessType}
            onChange={(e) => setAccessType(e.target.value as 'VIEW' | 'EDIT')}
            style={{ maxWidth: '240px' }}
          >
            <option value="VIEW">View Only</option>
            <option value="EDIT">Edit (requires login)</option>
          </select>
          <p style={{ fontSize: '12px', color: 'var(--ink-soft)', marginTop: '6px', marginBottom: 0 }}>
            {accessType === 'VIEW'
              ? 'Anyone with the link can view the plan (no login required).'
              : 'User must be logged in to edit the plan.'}
          </p>
        </div>

        <div className="field">
          <label>Expires In (days)</label>
          <input
            type="number"
            className="input"
            value={expiresInDays}
            onChange={(e) => setExpiresInDays(parseInt(e.target.value) || 7)}
            min={1}
            max={365}
            style={{ maxWidth: '240px' }}
          />
        </div>

        <button className="btn btn-accent" onClick={handleCreate}>
          Generate Share Link
        </button>
      </div>

      {loading && <p style={{ color: 'var(--ink-soft)' }}>Loading...</p>}

      {tokens.map(token => (
        <div key={token.id} className="card">
          <span className={`badge ${token.accessType === 'EDIT' ? 'badge-accent' : 'badge-success'}`}>
            {token.accessType}
          </span>
          <p style={{ marginTop: '10px' }}><strong>Expires:</strong> {new Date(token.expiresAt).toLocaleString()}</p>
          <p style={{ wordBreak: 'break-all' }}>
            <strong>Link:</strong>{' '}
            <a href={getShareUrl(token.token)} target="_blank" rel="noreferrer">{getShareUrl(token.token)}</a>
          </p>
          <div className="qr-box" style={{ margin: '10px 0' }}>
            <QRCodeSVG value={getShareUrl(token.token)} size={128} />
          </div>
          <div>
            <button className="btn btn-danger btn-sm" onClick={() => handleDelete(token.id)}>Delete</button>
          </div>
        </div>
      ))}

      {!loading && tokens.length === 0 && (
        <div className="empty-state">No share links created yet. Generate one above.</div>
      )}
    </div>
  );
};

export default SharePage;