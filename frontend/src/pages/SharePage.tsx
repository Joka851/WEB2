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

  const handleCreate = async (accessType: string) => {
    try {
      const token = await shareService.createToken(parseInt(planId!), { accessType });
      setTokens([...tokens, token]);
    } catch {
      setError('Failed to create token.');
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

      <div style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
        <button onClick={() => handleCreate('View')} style={{ padding: '10px 20px' }}>
          Create View Link
        </button>
        <button onClick={() => handleCreate('Edit')} style={{ padding: '10px 20px' }}>
          Create Edit Link
        </button>
      </div>

      {loading && <p>Loading...</p>}

      {tokens.map(token => (
        <div key={token.id} style={{ border: '1px solid #ccc', padding: '15px', marginBottom: '15px', borderRadius: '8px' }}>
          <h4>Access Type: {token.accessType}</h4>
          <p><strong>Expires:</strong> {new Date(token.expiresAt).toLocaleDateString()}</p>
          <p><strong>Link:</strong> <a href={getShareUrl(token.token)} target="_blank" rel="noreferrer">{getShareUrl(token.token)}</a></p>
          <div style={{ margin: '10px 0' }}>
            <QRCodeSVG value={getShareUrl(token.token)} size={128} />
          </div>
          <button onClick={() => handleDelete(token.id)} style={{ color: 'red' }}>Delete</button>
        </div>
      ))}
    </div>
  );
};

export default SharePage;