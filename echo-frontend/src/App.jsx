import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Auth/Login';
import SignUp from './pages/Auth/SignUp';
import Dashboard from './pages/Dashboard/Dashboard';
import ProtectedRoute from './components/ProtectedRoute'; // Güvenlik duvarımızı import ettik

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Ana sayfaya gireni direkt logine at */}
                <Route path="/" element={<Navigate to="/login" />} />

                {/* Açık Rotalar (Herkes girebilir) */}
                <Route path="/login" element={<Login />} />
                <Route path="/signup" element={<SignUp />} />

                {/* Korumalı Rota (Sadece Token'ı olanlar girebilir) */}
                <Route
                    path="/dashboard"
                    element={
                        <ProtectedRoute>
                            <Dashboard />
                        </ProtectedRoute>
                    }
                />

                {/* 404 Catch-All: Eğer yukarıdaki adresler dışında saçma bir şey (/dashboar gibi) girilirse, logine at */}
                <Route path="*" element={<Navigate to="/login" />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;