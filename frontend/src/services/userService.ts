import axios from 'axios';
import { User } from '../models/User';
import { authService } from './authService';

const API_URL = process.env.REACT_APP_API_URL;

const getHeaders = () => ({
  headers: { Authorization: `Bearer ${authService.getToken()}` }
});

export const userService = {
  getAll: async (): Promise<User[]> => {
    const response = await axios.get(`${API_URL}/api/users`, getHeaders());
    return response.data;
  },

  getById: async (id: number): Promise<User> => {
    const response = await axios.get(`${API_URL}/api/users/${id}`, getHeaders());
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await axios.delete(`${API_URL}/api/users/${id}`, getHeaders());
  },

  updateRole: async (id: number, role: string): Promise<User> => {
    const response = await axios.put(
      `${API_URL}/api/users/${id}/role`,
      { role: role },
      getHeaders()
    );
    return response.data;
  },

  updateUserStatus: async (id: number, data: { isActive: boolean }): Promise<User> => {
    const response = await axios.patch(
      `${API_URL}/api/users/${id}/status`,
      data,
      getHeaders()
    );
    return response.data;
  }
};