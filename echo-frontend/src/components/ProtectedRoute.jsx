import { Navigate } from 'react-router-dom';

function ProtectedRoute({ children }) {
    // Tarayıcı hafızasında token var mı diye bakıyoruz
    const token = localStorage.getItem('token');

    // Eğer token YOKSA, kullanıcıyı zorla login sayfasına geri gönderiyoruz
    if (!token) {
        return <Navigate to="/login" replace />;
    }

    // Eğer token VARSA, gitmek istediği sayfayı (children) gösteriyoruz
    return children;
}

export default ProtectedRoute;