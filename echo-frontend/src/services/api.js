import axios from 'axios';

// 1. Dinamik URL Belirleme
const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

export const API_BASE_URL = isLocalhost
    ? 'http://localhost:5216/api'
    : 'https://echo-ticket.onrender.com/api';

// 2. Temel Axios Kopyasını Oluşturma
const api = axios.create({
    baseURL: API_BASE_URL
});

// 3. SİHİRLİ KISIM (Interceptor): Giden her isteğin arasına girip Token'ı ekler
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        // Eğer giriş yapılmışsa ve token varsa, bunu başlığa (Header) ekle
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export default api;