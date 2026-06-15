import Home from './pages/Home/Home';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Auth/Login';
import SignUp from './pages/Auth/SignUp';
import Dashboard from './pages/Dashboard/Dashboard';
import ProtectedRoute from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext'; // Context'i import ettik
import Explore from './pages/Explore/Explore.jsx';
import EventDetail from './pages/Explore/EventDetail.jsx';
import MyTickets from './pages/Sidebar/MyTickets';
import Venues from './pages/Sidebar/Venues';
import Wallet from './pages/Sidebar/Wallet';
import AiAnalytics from './pages/Sidebar/AiAnalytics';
import Settings from './pages/Sidebar/Settings';

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
                        path="/explore"
                        element={
                            <ProtectedRoute>
                                <Explore />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/event/:id"
                        element={
                            <ProtectedRoute>
                                <EventDetail />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/dashboard"
                        element={
                            <ProtectedRoute>
                                <Dashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route path="/tickets" element={<MyTickets />} />
                    <Route path="/venues" element={<Venues />} />
                    <Route path="/wallet" element={<Wallet />} />
                    <Route path="/analytics" element={<AiAnalytics />} />
                    <Route path="/settings" element={<Settings />} />
                    <Route path="*" element={<Navigate to="/" />} /> {/* Yanlış adreste de Home'a atalım */}
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;