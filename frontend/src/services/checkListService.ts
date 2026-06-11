import axios from 'axios';
import { ChecklistItem } from '../models/ChecklistItem';
 
const API_URL = process.env.REACT_APP_API_URL;
 
const getHeaders = () => ({
  headers: {
    Authorization: `Bearer ${localStorage.getItem('token')}`,
    'Content-Type': 'application/json',
  },
});
 
const checkListService = {
  getAll: async (travelPlanId: number): Promise<ChecklistItem[]> => {
    const response = await axios.get(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists`,
      getHeaders()
    );
    return response.data;
  },
 
  create: async (travelPlanId: number, text: string): Promise<ChecklistItem> => {
    const response = await axios.post(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists`,
      { text },
      getHeaders()
    );
    return response.data;
  },
 
  toggle: async (travelPlanId: number, id: number): Promise<ChecklistItem> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists/${id}/toggle`,
      {},
      getHeaders()
    );
    return response.data;
  },
 
  update: async (travelPlanId: number, id: number, text: string): Promise<ChecklistItem> => {
    const response = await axios.put(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists/${id}`,
      { text },
      getHeaders()
    );
    return response.data;
  },
 
  delete: async (travelPlanId: number, id: number): Promise<void> => {
    await axios.delete(
      `${API_URL}/api/travel-plans/${travelPlanId}/checklists/${id}`,
      getHeaders()
    );
  },
};
 
export default checkListService;