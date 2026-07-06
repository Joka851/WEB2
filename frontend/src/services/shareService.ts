import axios from 'axios';
import { ShareToken, CreateShareToken } from '../models/ShareToken';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = () => ({
  headers: { Authorization: `Bearer ${authService.getToken()}` }
});

export const shareService = {
  getTokens: async (travelPlanId: number): Promise<ShareToken[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/share`,
      getHeaders()
    );
    return response.data;
  },

  createToken: async (travelPlanId: number, data: CreateShareToken): Promise<ShareToken> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/share/generate`,
      data,
      getHeaders()
    );
    return response.data;
  },

  
  accessByToken: async (token: string): Promise<any> => {
    const response = await axios.get(
      `${API_URL}/api/share/access/${token}`
    );
    return response.data;
  },

  deleteToken: async (travelPlanId: number, id: number): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/share/${id}`,
      getHeaders()
    );
  }
};