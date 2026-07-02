import axios from 'axios';
import { ChecklistItem, CreateChecklistItem } from '../models/ChecklistItem';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = (shareToken?: string) => ({
  headers: {
    Authorization: `Bearer ${authService.getToken()}`,
    ...(shareToken ? { 'X-Share-Token': shareToken } : {})
  }
});

export const checklistService = {
  getAll: async (travelPlanId: number): Promise<ChecklistItem[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists`,
      getHeaders()
    );
    return response.data;
  },

  create: async (travelPlanId: number, data: CreateChecklistItem, shareToken?: string): Promise<ChecklistItem> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists`,
      data,
      getHeaders(shareToken)
    );
    return response.data;
  },

  toggle: async (travelPlanId: number, id: number, shareToken?: string): Promise<ChecklistItem> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists/${id}/toggle`,
      {},
      getHeaders(shareToken)
    );
    return response.data;
  },

  delete: async (travelPlanId: number, id: number, shareToken?: string): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists/${id}`,
      getHeaders(shareToken)
    );
  }
};