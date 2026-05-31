import axios from 'axios';
import { Activity, CreateActivity } from '../models/Activity';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = () => ({
  headers: { Authorization: `Bearer ${authService.getToken()}` }
});

export const activityService = {
  getAll: async (travelPlanId: number): Promise<Activity[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities`,
      getHeaders()
    );
    return response.data;
  },

  create: async (travelPlanId: number, data: CreateActivity): Promise<Activity> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities`,
      data,
      getHeaders()
    );
    return response.data;
  },

  update: async (travelPlanId: number, id: number, data: CreateActivity): Promise<Activity> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities/${id}`,
      data,
      getHeaders()
    );
    return response.data;
  },

  delete: async (travelPlanId: number, id: number): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/activities/${id}`,
      getHeaders()
    );
  }
};