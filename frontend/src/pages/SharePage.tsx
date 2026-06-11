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
      const token = await shareService.createToken(parseInt(planId!), { 
        accessType, 
        expiresInDays 
      });
      setTokens([...tokens, token]);
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
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <button onClick={() => navigate(`/travel-plans/${planId}`)}>← Back</button>
      <h2>Share Travel Plan</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}

      <div style={{ border: '1px solid #ccc', padding: '20px', borderRadius: '8px', marginBottom: '20px' }}>
        <h3>Generate New Share Link</h3>
        
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Access Type</label>
          <select
            value={accessType}
            onChange={(e) => setAccessType(e.target.value as 'VIEW' | 'EDIT')}
            style={{ padding: '8px', width: '200px' }}
          >
            <option value="VIEW">View Only</option>
            <option value="EDIT">Edit (requires login)</option>
          </select>
          <p style={{ fontSize: '12px', color: '#666', marginTop: '5px' }}>
            {accessType === 'VIEW' 
              ? 'Anyone with the link can view the plan (no login required).'
              : 'User must be logged in to edit the plan.'}
          </p>
        </div>

        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Expires In (days)</label>
          <input
            type="number"
            value={expiresInDays}
            onChange={(e) => setExpiresInDays(parseInt(e.target.value) || 7)}
            min={1}
            max={365}
            style={{ padding: '8px', width: '200px' }}
          />
        </div>

        <button onClick={handleCreate} style={{ padding: '10px 20px' }}>
          Generate Share Link
        </button>
      </div>

      {loading && <p>Loading...</p>}

      {tokens.map(token => (
        <div key={token.id} style={{ border: '1px solid #ccc', padding: '15px', marginBottom: '15px', borderRadius: '8px' }}>
          <h4>Access Type: <span style={{ color: token.accessType === 'EDIT' ? 'orange' : 'green' }}>{token.accessType}</span></h4>
          <p><strong>Expires:</strong> {new Date(token.expiresAt).toLocaleString()}</p>
          <p><strong>Link:</strong> <a href={getShareUrl(token.token)} target="_blank" rel="noreferrer">{getShareUrl(token.token)}</a></p>
          <div style={{ margin: '10px 0' }}>
            <QRCodeSVG value={getShareUrl(token.token)} size={128} />
          </div>
          <button onClick={() => handleDelete(token.id)} style={{ color: 'red' }}>Delete</button>
        </div>
      ))}

      {!loading && tokens.length === 0 && (
        <p>No share links created yet. Generate one above.</p>
      )}
    </div>
  );
};

export default SharePage;