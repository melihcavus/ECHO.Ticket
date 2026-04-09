import api from './api'; // Ham axios yerine KENDİ api dosyamızı çağırıyoruz

// Dikkat edersen artık 'http://localhost:5216/api' yazmıyoruz, çünkü api.js içinde var!
export const loginUser = async (email, password) => {
    return await api.post('/Users/login', { email, password });
};

export const registerUser = async (userData) => {
    return await api.post('/Users', userData);
};