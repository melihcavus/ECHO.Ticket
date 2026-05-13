import { createContext, useState, useEffect, useContext } from 'react';
import { jwtDecode } from 'jwt-decode';

// 1. Context'i oluşturuyoruz
const AuthContext = createContext();

// 2. Uygulamayı sarmalayacak Provider (Sağlayıcı) bileşeni
export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);

    // Uygulama ilk açıldığında (veya sayfa yenilendiğinde) çalışır
    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                // Token'ı çöz ve içindeki bilgileri state'e kaydet
                const decoded = jwtDecode(token);
                setUser({
                    id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || decoded.id || decoded.sub, // .NET Core'un default ID claim'i
                    firstName: decoded.FirstName,
                    lastName: decoded.LastName,
                    email: decoded.email,
                    role: decoded.role
                });
            } catch (error) {
                console.error("Token çözülemedi:", error);
                localStorage.removeItem('token');
            }
        }
    }, []);

    // Giriş yapıldığında çağrılacak fonksiyon
    const login = (token) => {
        localStorage.setItem('token', token);
        const decoded = jwtDecode(token);
        setUser({
            firstName: decoded.FirstName,
            lastName: decoded.LastName,
            email: decoded.email,
            role: decoded.role
        });
    };

    // Çıkış yapıldığında çağrılacak fonksiyon
    const logout = () => {
        localStorage.removeItem('token');
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

// 3. Diğer sayfalardan bu verilere kolayca ulaşmak için özel Hook'umuz
export const useAuth = () => {
    return useContext(AuthContext);
};