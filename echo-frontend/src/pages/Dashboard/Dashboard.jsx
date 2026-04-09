import { useNavigate } from 'react-router-dom';

function Dashboard() {
    const navigate = useNavigate();

    // Kullanıcı çıkış yapmak isterse
    const handleLogout = () => {
        localStorage.removeItem('token'); // Token'ı sil
        navigate('/login'); // Login sayfasına geri yolla
    };

    return (
        <div className="min-h-screen bg-gray-100 p-10">
            <div className="max-w-4xl mx-auto bg-white p-8 rounded-xl shadow-md">
                <h1 className="text-3xl font-bold text-slate-800 mb-4">ECHO Dashboard'a Hoş Geldin!</h1>
                <p className="text-slate-600 mb-8">Giriş işlemini başarıyla tamamladın ve korumalı alana girdin.</p>

                <button
                    onClick={handleLogout}
                    className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                >
                    Çıkış Yap
                </button>
            </div>
        </div>
    );
}

export default Dashboard;