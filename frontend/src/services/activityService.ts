import axios from 'axios';
import { Activity, CreateActivity } from '../models/Activity';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = (shareToken?: string) => ({
  headers: {
    Authorization: `Bearer ${authService.getToken()}`,
    ...(shareToken ? { 'X-Share-Token': shareToken } : {})
  }
});

export const activityService = {
  getAll: async (travelPlanId: number, shareToken?: string): Promise<Activity[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities`,
      getHeaders(shareToken)
    );
    return response.data;
  },

  create: async (travelPlanId: number, data: CreateActivity, shareToken?: string): Promise<Activity> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities`,
      data,
      getHeaders(shareToken)
    );
    return response.data;
  },

  update: async (travelPlanId: number, id: number, data: CreateActivity, shareToken?: string): Promise<Activity> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities/${id}`,
      data,
      getHeaders(shareToken)
    );
    return response.data;
  },

  delete: async (travelPlanId: number, id: number, shareToken?: string): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities/${id}`,
      getHeaders(shareToken)
    );
  }
};