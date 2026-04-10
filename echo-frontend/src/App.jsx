import Home from './pages/Home/Home';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Auth/Login';
import SignUp from './pages/Auth/SignUp';
import Dashboard from './pages/Dashboard/Dashboard';
import ProtectedRoute from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext'; // Context'i import ettik

function App() {
    return (
        // AuthProvider'ı en dışa koyuyoruz ki tüm Route'lar ona erişebilsin
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    {/* Ana sayfaya girene artık Home componentini gösteriyoruz */}
                    <Route path="/" element={<Home />} />

                    <Route path="/login" element={<Login />} />
                    <Route path="/signup" element={<SignUp />} />

                    <Route
                        path="/dashboard"
                        element={
                            <ProtectedRoute>
                                <Dashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route path="*" element={<Navigate to="/" />} /> {/* Yanlış adreste de Home'a atalım */}
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;