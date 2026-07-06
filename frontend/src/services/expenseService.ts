import axios from 'axios';
import { Expense, CreateExpense } from '../models/Expense';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = () => ({
  headers: { Authorization: `Bearer ${authService.getToken()}` }
});

export const expenseService = {
  getAll: async (travelPlanId: number): Promise<Expense[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/expenses`,
      getHeaders()
    );
    return response.data;
  },

  
  getById: async (travelPlanId: number, id: number): Promise<Expense> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/expenses/${id}`,
      getHeaders()
    );
    return response.data;
  },

  getSummary: async (travelPlanId: number): Promise<any> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/expenses/summary`,
      getHeaders()
    );
    return response.data;
  },

  create: async (travelPlanId: number, data: CreateExpense): Promise<Expense> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/expenses`,
      data,
      getHeaders()
    );
    return response.data;
  },

  update: async (travelPlanId: number, id: number, data: CreateExpense): Promise<Expense> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/expenses/${id}`,
      data,
      getHeaders()
    );
    return response.data;
  },

  delete: async (travelPlanId: number, id: number): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/expenses/${id}`,
      getHeaders()
    );
  }
};