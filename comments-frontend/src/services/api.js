// File: src/services/api.js
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7002';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});
// Response interceptor
api.interceptors.response.use(
    (response) => {
      return response;
    },
    (error) => {
      if (error.code === 'ECONNREFUSED') {
        throw new Error('Cannot connect to server. Make sure the backend is running on port 7000.');
      }

      if (error.response?.status === 500) {
        throw new Error('Server error. Please try again later.');
      }

      if (error.response?.status === 400) {
        throw new Error('Validation error: ' + (error.response.data.message || 'Invalid data'));
      }

      throw error;
    }
);

export const commentService = {
  async getComments(page = 1, sortBy = 'CreatedAt', sortDescending = true) {
    const response = await api.get('/api/Comments', {
      params: { Page: page, PageSize: 25, SortBy: sortBy, SortDescending: sortDescending }
    });
    return response;
  },

  async createComment(commentData) {
    const response = await api.post('/api/Comments', commentData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  async getReplies(parentId) {
    const response = await api.get(`/api/Comments/${parentId}/replies`);
    return response.data;
  },

  async deleteComment(id) {
    const response = await api.delete(`/api/Comments/${id}`);
    return response.data;
  }
};

export const captchaService = {
  async getCaptcha() {
    const response = await api.get('/api/Captcha');
    return response.data;
  },

  async validateCaptcha(captchaId, code) {
    const response = await api.post('/api/Captcha/validate', { captchaId, code });
    return response.data;
  }
};