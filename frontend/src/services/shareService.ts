import axios from 'axios';
 
const API_URL = process.env.REACT_APP_API_URL;
 
const getHeaders = () => ({
  headers: {
    Authorization: `Bearer ${localStorage.getItem('token')}`,
    'Content-Type': 'application/json',
  },
});
 
const shareService = {
  createToken: async (
    travelPlanId: number,
    expiresInDays: number
  ): Promise<{ token: string; expiresAt: string }> => {
    const expiresAt = new Date();
    expiresAt.setDate(expiresAt.getDate() + expiresInDays);
 
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/share`,
      { expiresAt: expiresAt.toISOString() },
      getHeaders()
    );
    return response.data;
  },
 
  getTokens: async (travelPlanId: number): Promise<any[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/share`,
      getHeaders()
    );
    return response.data;
  },
 
  revokeToken: async (travelPlanId: number, tokenId: number): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/share/${tokenId}`,
      getHeaders()
    );
  },
 
  // Pristup dijeljenom planu — koristi GET /api/share/access/{token}
  accessByToken: async (token: string): Promise<any> => {
    const response = await axios.get(
      `${API_URL}/api/share/access/${token}`
    );
    return response.data;
  },
};
 
export default shareService;
 