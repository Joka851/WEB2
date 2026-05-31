import axios from 'axios';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/User';

const API_URL = process.env.REACT_APP_API_URL;

export const authService = {
  register: async (data: RegisterRequest): Promise<User> => {
    const response = await axios.post(`${API_URL}/api/auth/register`, data);
    return response.data;
  },

  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await axios.post(`${API_URL}/api/auth/login`, data);
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getToken: (): string | null => {
    return localStorage.getItem('token');
  },

  saveToken: (token: string) => {
    localStorage.setItem('token', token);
  }
};