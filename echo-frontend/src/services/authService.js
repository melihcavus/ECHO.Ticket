import axios from 'axios';

const API_URL = 'http://localhost:5216/api';

export const loginUser = async (email, password) => {
    return await axios.post(`${API_URL}/Users/login`, { email, password });
};

export const registerUser = async (userData) => {
    return await axios.post(`${API_URL}/Users`, userData);
};