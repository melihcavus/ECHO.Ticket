import axios from 'axios';

// 1. SİHİRLİ DOKUNUŞ: Proje şu an nerede çalışıyor onu anlıyoruz
// Eğer tarayıcıda "localhost" yazıyorsa yerel backend'i, yoksa Render'daki canlı backend'i seç!
const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
const API_BASE_URL = isLocalhost
    ? 'http://localhost:5216/api'
    : 'https://echo-ticket.onrender.com/api'; // Render'daki CANLI backend adresin

// 2. Temel Axios kopyamızı dinamik URL ile oluşturuyoruz
const api = axios.create({
    baseURL: API_BASE_URL,
});

// 3. İSTEK (REQUEST) INTERCEPTOR'I
api.interceptors.request.use(
    (config) => {
        // Tarayıcı hafızasından token'ı al
        const token = localStorage.getItem('token');

        // Eğer token varsa, isteğin Header (Başlık) kısmına ekle
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }

        return config;
    },
    (error) => {
        // İstek gönderilmeden önce bir hata olursa buraya düşer
        return Promise.reject(error);
    }
);

export default api;