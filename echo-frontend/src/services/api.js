import axios from 'axios';

// 1. Temel Axios kopyamızı oluşturuyoruz (Her seferinde http://localhost... yazmamak için)
const api = axios.create({
    baseURL: 'http://localhost:5216/api',
});

// 2. İSTEK (REQUEST) INTERCEPTOR'I
// Frontend'den backend'e giden HER istekten hemen önce bu blok çalışır
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