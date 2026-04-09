import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Auth/Login';
import SignUp from './pages/Auth/SignUp';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Kullanıcı siteye ilk girdiğinde direkt login sayfasına yönlendirilsin */}
                <Route path="/" element={<Navigate to="/login" />} />

                {/* URL'de /login yazarsa Login sayfasını (bileşenini) aç */}
                <Route path="/login" element={<Login />} />

                {/* URL'de /signup yazarsa SignUp sayfasını aç */}
                <Route path="/signup" element={<SignUp />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;