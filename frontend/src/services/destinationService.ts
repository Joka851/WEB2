import axios from 'axios';
import { Destination, CreateDestination } from '../models/Destination';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = (shareToken?: string) => ({
  headers: {
    Authorization: `Bearer ${authService.getToken()}`,
    ...(shareToken ? { 'X-Share-Token': shareToken } : {})
  }
});

export const destinationService = {
  getAll: async (travelPlanId: number, shareToken?: string): Promise<Destination[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/destinations`,
      getHeaders(shareToken)
    );
    return response.data;
  },

  create: async (travelPlanId: number, data: CreateDestination, shareToken?: string): Promise<Destination> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/destinations`,
      data,
      getHeaders(shareToken)
    );
    return response.data;
  },

  update: async (travelPlanId: number, id: number, data: CreateDestination, shareToken?: string): Promise<Destination> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/destinations/${id}`,
      data,
      getHeaders(shareToken)
    );
    return response.data;
  },

  delete: async (travelPlanId: number, id: number, shareToken?: string): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/destinations/${id}`,
      getHeaders(shareToken)
    );
  }
};