// httpClient.js
// Provides a simple HTTP client for making API requests

import axios from 'axios';
import store from '@/store/store.js';
import { openAuthTab } from '@/services/authService.js'

const httpClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL, // Base API URL from environment variables
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor for adding auth token if available
httpClient.interceptors.request.use(
    (config) => {
        let token = store.getters.getToken;
        if (!token) {
            token = localStorage.getItem('authToken');
        }

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor for handling errors globally, 401 and refresh token
httpClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config

        // If 401 and no retry yet
        if (error.response && error.response.status === 401) {
            // If the request has already been retried, reject the promise
            if (originalRequest._retry) { return Promise.reject(error) }

            originalRequest._retry = true;

            // Check if it was background request
            const isSilent = originalRequest.isSilentCheck

            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) {
                // Clear auth data from store
                store.commit('clearAuthData')
                if (!isSilent) {
                    openAuthTab();
                }
                return Promise.reject(error);
            }

            try {
                // Try to refresh tokens
                const refreshResponse = await axios.post(`${import.meta.env.VITE_API_BASE_URL}/api/refresh-token`, {
                    jwtToken: localStorage.getItem('authToken'),
                    refreshToken: refreshToken
                })

                if (refreshResponse.data.isSuccess) {
                    const newToken = refreshResponse.data.jwtToken
                    const newRefreshToken = refreshResponse.data.refreshToken

                    // Update storage and store
                    store.commit('setToken', newToken)
                    store.commit('setRefreshToken', newRefreshToken)

                    // Update header and retry original request
                    originalRequest.headers.Authorization = `Bearer ${newToken}`
                    // Retry original request with new token
                    return httpClient(originalRequest)
                } else {
                    // Clear auth data from store
                    store.commit('clearAuthData')
                    
                    if (!isSilent) {
                        // Refresh token failed, redirect to login
                        openAuthTab();
                    }

                    return Promise.reject(error)
                }
            } catch (refreshError) {
                if(import.meta.env.DEV) {
                    console.error('Refresh token error:', refreshError);
                }

                // Clear auth data from store
                store.commit('clearAuthData')
                if (!isSilent) {
                    // Refresh token failed, redirect to login
                    openAuthTab();
                }
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
)

export default httpClient;