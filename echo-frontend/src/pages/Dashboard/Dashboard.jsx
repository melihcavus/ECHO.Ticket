import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext'; // Context'i çağırdık

function Dashboard() {
    const navigate = useNavigate();
    const { user, logout } = useAuth(); // Kullanıcı bilgilerini ve logout fonksiyonunu çektik

    const handleLogout = () => {
        logout(); // Artık Context'teki temiz ve güvenli logout'u kullanıyoruz
        navigate('/login');
    };

    return (
        <div className="min-h-screen bg-[#0B1325] p-10 font-sans">
            <div className="max-w-4xl mx-auto bg-[#111C3A] p-8 rounded-[2rem] shadow-2xl border border-white/5">

                {/* Kullanıcıya özel karşılama */}
                <h1 className="text-4xl font-bold text-white mb-2">
                    Hoş geldin, <span className="text-cyan-400">{user?.firstName} {user?.lastName}</span>! 👋
                </h1>

                <p className="text-cyan-200/60 mb-8">
                    Giriş işlemini başarıyla tamamladın. Rolün: <strong className="text-white">{user?.role}</strong>
                </p>

                <button
                    onClick={handleLogout}
                    className="px-6 py-3 bg-red-500/10 text-red-400 border border-red-500/20 rounded-xl hover:bg-red-500/20 transition-all font-semibold"
                >
                    Güvenli Çıkış Yap
                </button>
            </div>
        </div>
    );
}

export default Dashboard;