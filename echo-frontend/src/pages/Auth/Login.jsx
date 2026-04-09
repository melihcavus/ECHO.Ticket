import { Mail, Lock, EyeOff, Github, TrendingUp, Handshake, Users, Loader2 } from 'lucide-react';
import { useState } from 'react';
import { loginUser } from '../../services/authService';
import { useNavigate, Link } from 'react-router-dom';
function Login() {
    // 1. useNavigate'i BURADA, en üstte tanımlamalısın!
    const navigate = useNavigate();

    // 2. Verileri Takip Eden State'ler
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    // 3. Login Fonksiyonu (API İsteği)
    const handleLogin = async (e) => {
        e.preventDefault();

        if (!email || !password) {
            alert("Lütfen tüm alanları doldurun!");
            return;
        }

        setLoading(true);
        try {
            const response = await loginUser(email, password);

            const token = response.data.data.token || response.data.data;
            localStorage.setItem('token', token); // Token'ı tarayıcı hafızasına al

            alert("Giriş Başarılı! ECHO sistemine yönlendiriliyorsunuz.");
            console.log("Gelen Token:", token);

            // Yukarıda tanımladığımız navigate'i burada sadece ÇAĞIRIYORUZ
            navigate('/dashboard');

        } catch (error) {
            console.error("Giriş Hatası:", error);
            const errorMessage = error.response?.data?.message || "E-posta veya şifre hatalı!";
            alert("Hata: " + errorMessage);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex items-center justify-center p-4 relative overflow-hidden font-sans">

            {/* Arka plan süslemeleri */}
            <div className="absolute top-1/4 -left-1/4 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none"></div>
            <div className="absolute bottom-1/4 -right-1/4 w-[600px] h-[600px] bg-blue-600/10 rounded-full blur-[120px] pointer-events-none"></div>

            <div className="max-w-6xl w-full bg-[#111C3A] rounded-[2rem] shadow-2xl flex flex-col md:flex-row overflow-hidden border border-white/5 relative z-10">

                {/* SOL TARAF */}
                <div className="md:w-3/5 p-16 flex flex-col justify-between relative overflow-hidden">
                    <div className="text-white text-3xl font-bold flex items-center gap-2">
                        <span className="text-cyan-400">|||</span> ECHO
                    </div>

                    <div className="relative z-10 text-center md:text-left my-20">
                        <h1 className="text-6xl md:text-8xl font-extrabold text-white tracking-tight mb-3">
                            <span className="text-cyan-400">|||</span> ECHO
                        </h1>
                        <p className="text-2xl text-cyan-100/90 font-medium mb-1">Crowd-Pledging Platform</p>
                        <p className="text-cyan-200/60 text-lg mb-8">| Amplify Your Support</p>
                        <p className="text-sm text-slate-400 mt-10 max-w-md">Support the projects you love.<br/>Raise funds for your cause.<br/>Tickets for events.</p>
                    </div>

                    <div className="absolute -bottom-10 -right-10 flex items-center gap-6 text-cyan-500/20 pointer-events-none">
                        <Users size={120} />
                        <TrendingUp size={160} />
                        <Handshake size={120} />
                    </div>

                    <div className="absolute inset-0 bg-gradient-to-br from-cyan-600/15 to-blue-800/20 pointer-events-none"></div>
                    <div className="absolute bottom-0 left-0 w-full h-1/2 bg-gradient-to-t from-[#0B1325]/50 to-transparent"></div>
                </div>

                {/* SAĞ TARAF (FORM) */}
                <div className="md:w-2/5 p-5">
                    <div className="bg-white h-full rounded-[1.5rem] p-10 flex flex-col justify-center shadow-inner relative overflow-hidden">

                        <div className="text-center mb-10">
                            <h2 className="text-4xl font-bold text-slate-900">Welcome to ECHO</h2>
                            <p className="text-slate-600 mt-3 text-lg">Login to your account.</p>
                        </div>

                        {/* onSubmit eklendi */}
                        <form onSubmit={handleLogin} className="space-y-6">
                            <div className="relative">
                                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 w-6 h-6" />
                                <input
                                    type="email"
                                    required
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)} // Yazılanı state'e kaydeder
                                    placeholder="Email Address"
                                    className="w-full pl-12 pr-5 py-4 bg-slate-50 border border-slate-200 rounded-2xl text-lg focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white transition-all shadow-sm"
                                />
                            </div>

                            <div className="relative">
                                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 w-6 h-6" />
                                <input
                                    type="password"
                                    required
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)} // Yazılanı state'e kaydeder
                                    placeholder="Password"
                                    className="w-full pl-12 pr-12 py-4 bg-slate-50 border border-slate-200 rounded-2xl text-lg focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white transition-all shadow-sm"
                                />
                                <EyeOff className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 w-6 h-6 cursor-pointer hover:text-slate-700" />
                            </div>

                            <div className="flex justify-end pt-1">
                                <a href="#" className="text-sm text-cyan-600 hover:text-cyan-700 font-semibold transition-colors">Forgot password?</a>
                            </div>

                            <button
                                type="submit"
                                disabled={loading}
                                className="w-full py-4 bg-cyan-500 hover:bg-cyan-600 text-white rounded-2xl font-bold text-lg shadow-lg shadow-cyan-500/30 transition-all active:scale-[0.98] flex justify-center items-center gap-2"
                            >
                                {loading && <Loader2 className="animate-spin w-5 h-5" />}
                                {loading ? 'Checking...' : 'Login'}
                            </button>
                        </form>

                        <div className="mt-10">
                            <div className="relative flex items-center justify-center mb-6">
                                <div className="border-t border-slate-200 w-full absolute"></div>
                                <span className="bg-white px-4 text-sm text-slate-400 relative">Or login with</span>
                            </div>

                            <div className="flex justify-center gap-5">
                                <button className="p-4 border border-slate-200 rounded-2xl hover:bg-slate-100/70 transition-all hover:-translate-y-1">
                                    <img src="https://www.svgrepo.com/show/475656/google-color.svg" alt="Google" className="w-7 h-7" />
                                </button>
                                <button className="p-4 border border-slate-200 rounded-2xl hover:bg-slate-100/70 transition-all text-slate-800 hover:-translate-y-1">
                                    <Github className="w-7 h-7" />
                                </button>
                            </div>
                        </div>

                        <p className="text-center text-base text-slate-600 mt-10">
                            Don't have an account? <Link to="/signup" className="text-cyan-600 font-bold hover:underline">Sign up.</Link>
                        </p>

                    </div>
                </div>
            </div>
        </div>
    );
}

export default Login;