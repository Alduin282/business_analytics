import axios from 'axios';

const API_URL = 'http://localhost:5014/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authService = {
  register: async (email, password) => {
    const timeZoneId = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const response = await api.post('/auth/register', { email, password, timeZoneId });
    return response.data;
  },
  login: async (email, password) => {
    const response = await api.post('/auth/login', { email, password });
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
    }
    return response.data;
  },
  logout: () => {
    localStorage.removeItem('token');
  },
  getToken: () => localStorage.getItem('token'),
};

export const ordersService = {
  getAnalytics: async (params) => {
    const response = await api.get('/orders/analytics', { params });
    return response.data;
  },
  importOrders: async (file) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post('/import/orders', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
  getImportHistory: async () => {
    const response = await api.get('/import/history');
    return response.data;
  },
  rollbackImport: async (id) => {
    const response = await api.post(`/import/rollback/${id}`);
    return response.data;
  },
};

export default api;
