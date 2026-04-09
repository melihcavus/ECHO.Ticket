import { useNavigate } from 'react-router-dom';
import api from '../../services/api'; // api.js'i içeri aktardık

function Dashboard() {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    // TEST FONKSİYONU
    const handleTestApi = async () => {
        try {
            // Sadece /Users yazıyoruz, baseURL ve Token'ı api.js halledecek
            const response = await api.get('/Users');
            console.log("Backend'den Gelen Cevap:", response.data);
            alert("İstek başarılı! Konsolu kontrol et.");
        } catch (error) {
            console.error("Test Hatası:", error);
        }
    };

    return (
        <div className="min-h-screen bg-gray-100 p-10">
            <div className="max-w-4xl mx-auto bg-white p-8 rounded-xl shadow-md">
                <h1 className="text-3xl font-bold text-slate-800 mb-4">ECHO Dashboard'a Hoş Geldin!</h1>
                <p className="text-slate-600 mb-8">Giriş işlemini başarıyla tamamladın ve korumalı alana girdin.</p>

                <div className="flex gap-4">
                    <button
                        onClick={handleLogout}
                        className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                    >
                        Çıkış Yap
                    </button>

                    {/* TEST BUTONU */}
                    <button
                        onClick={handleTestApi}
                        className="px-4 py-2 bg-cyan-500 text-white rounded-lg hover:bg-cyan-600 transition-colors"
                    >
                        API'ye Token Gönder
                    </button>
                </div>
            </div>
        </div>
    );
}

export default Dashboard;