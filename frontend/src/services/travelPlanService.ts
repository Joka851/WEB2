import axios from 'axios';
import { TravelPlan, CreateTravelPlan } from '../models/TravelPlan';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = (shareToken?: string) => ({
  headers: {
    Authorization: `Bearer ${authService.getToken()}`,
    ...(shareToken ? { 'X-Share-Token': shareToken } : {})
  }
});

export const travelPlanService = {
  getAll: async (): Promise<TravelPlan[]> => {
    const response = await axios.get(`${API_URL}/api/travel-plans`, getHeaders());
    return response.data;
  },

  getById: async (id: number): Promise<TravelPlan> => {
    const response = await axios.get(`${API_URL}/api/travel-plans/${id}`, getHeaders());
    return response.data;
  },

  getMyPlans: async (): Promise<TravelPlan[]> => {
    const response = await axios.get(`${API_URL}/api/travel-plans/my-plans`, getHeaders());
    return response.data;
  },

  // Ovo ostaje SAMO ZA ADMINA (ne koristi se u DashboardPage za obične korisnike)
  getByUser: async (userId: number): Promise<TravelPlan[]> => {
    const response = await axios.get(`${API_URL}/api/travel-plans/user/${userId}`, getHeaders());
    return response.data;
  },

  create: async (data: CreateTravelPlan): Promise<TravelPlan> => {
    const response = await axios.post(`${API_URL}/api/travel-plans`, data, getHeaders());
    return response.data;
  },

  update: async (id: number, data: CreateTravelPlan, shareToken?: string): Promise<TravelPlan> => {
    const response = await axios.put(`${API_URL}/api/travel-plans/${id}`, data, getHeaders(shareToken));
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await axios.delete(`${API_URL}/api/travel-plans/${id}`, getHeaders());
  }
};