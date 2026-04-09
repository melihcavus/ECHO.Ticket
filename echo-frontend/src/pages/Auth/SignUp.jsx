import { Mail, Lock, EyeOff, User, Github, TrendingUp, Handshake, Users as UsersIcon, Loader2 } from 'lucide-react';
import { useState } from 'react';
import { registerUser } from '../../services/authService';

function SignUp() {
    // 1. Verileri Takip Eden State'ler
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    // 2. Kayıt Fonksiyonu (API İsteği)
    const handleSignUp = async (e) => {
        e.preventDefault(); // Sayfanın yenilenmesini engelle

        if (!firstName || !lastName || !email || !password) {
            alert("Lütfen tüm alanları doldurun!");
            return;
        }

        setLoading(true);
        try {
            // Servisi kullanarak API isteği atıyoruz
            const response = await registerUser({
                firstName: firstName,
                lastName: lastName,
                email: email,
                passwordHash: password // DTO'daki isme göre eşleştiriyoruz
            });

            alert("Kayıt Başarılı! ECHO sistemine hoş geldiniz. Şimdi giriş yapabilirsiniz.");

            // Başarılı kayıttan sonra login sayfasına yönlendir
            window.location.href = '/login';

        } catch (error) {
            console.error("Kayıt Hatası:", error);
            const errorMessage = error.response?.data?.message || "Kayıt olurken bir hata oluştu. Lütfen bilgileri kontrol edin.";
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
                <div className="md:w-3/5 p-16 flex flex-col justify-between relative overflow-hidden hidden md:flex">
                    <div className="text-white text-3xl font-bold flex items-center gap-2">
                        <span className="text-cyan-400">|||</span> ECHO
                    </div>

                    <div className="relative z-10 text-center md:text-left my-20">
                        <h1 className="text-6xl md:text-8xl font-extrabold text-white tracking-tight mb-3">
                            <span className="text-cyan-400">|||</span> ECHO
                        </h1>
                        <p className="text-2xl text-cyan-100/90 font-medium mb-1">Crowd-Pledging Platform</p>
                        <p className="text-cyan-200/60 text-lg mb-8">| Amplify Your Support</p>
                        <p className="text-sm text-slate-400 mt-10 max-w-md">Join us today.<br/>Support the projects you love.<br/>Raise funds for your cause.</p>
                    </div>

                    <div className="absolute -bottom-10 -right-10 flex items-center gap-6 text-cyan-500/20 pointer-events-none">
                        <UsersIcon size={120} />
                        <TrendingUp size={160} />
                        <Handshake size={120} />
                    </div>

                    <div className="absolute inset-0 bg-gradient-to-br from-cyan-600/15 to-blue-800/20 pointer-events-none"></div>
                    <div className="absolute bottom-0 left-0 w-full h-1/2 bg-gradient-to-t from-[#0B1325]/50 to-transparent"></div>
                </div>

                {/* SAĞ TARAF (FORM) */}
                <div className="w-full md:w-2/5 p-5">
                    <div className="bg-white h-full rounded-[1.5rem] p-8 lg:p-10 flex flex-col justify-center shadow-inner relative overflow-hidden">

                        <div className="text-center mb-8">
                            <h2 className="text-3xl lg:text-4xl font-bold text-slate-900">Create Account</h2>
                            <p className="text-slate-600 mt-2 text-md">Join ECHO to get started.</p>
                        </div>

                        <form onSubmit={handleSignUp} className="space-y-4">

                            {/* Ad ve Soyad (Yan yana) */}
                            <div className="flex gap-4">
                                <div className="relative w-1/2">
                                    <User className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 w-5 h-5" />
                                    <input
                                        type="text"
                                        required
                                        value={firstName}
                                        onChange={(e) => setFirstName(e.target.value)}
                                        placeholder="First Name"
                                        className="w-full pl-11 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-2xl text-md focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white transition-all shadow-sm"
                                    />
                                </div>
                                <div className="relative w-1/2">
                                    <User className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 w-5 h-5" />
                                    <input
                                        type="text"
                                        required
                                        value={lastName}
                                        onChange={(e) => setLastName(e.target.value)}
                                        placeholder="Last Name"
                                        className="w-full pl-11 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-2xl text-md focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white transition-all shadow-sm"
                                    />
                                </div>
                            </div>

                            <div className="relative">
                                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 w-5 h-5" />
                                <input
                                    type="email"
                                    required
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder="Email Address"
                                    className="w-full pl-11 pr-5 py-3 bg-slate-50 border border-slate-200 rounded-2xl text-md focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white transition-all shadow-sm"
                                />
                            </div>

                            <div className="relative">
                                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 w-5 h-5" />
                                <input
                                    type="password"
                                    required
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="Password"
                                    className="w-full pl-11 pr-11 py-3 bg-slate-50 border border-slate-200 rounded-2xl text-md focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white transition-all shadow-sm"
                                />
                                <EyeOff className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 w-5 h-5 cursor-pointer hover:text-slate-700" />
                            </div>

                            <button
                                type="submit"
                                disabled={loading}
                                className="w-full py-3.5 mt-2 bg-cyan-500 hover:bg-cyan-600 text-white rounded-2xl font-bold text-lg shadow-lg shadow-cyan-500/30 transition-all active:scale-[0.98] flex justify-center items-center gap-2"
                            >
                                {loading && <Loader2 className="animate-spin w-5 h-5" />}
                                {loading ? 'Creating...' : 'Sign Up'}
                            </button>
                        </form>

                        <div className="mt-8">
                            <div className="relative flex items-center justify-center mb-5">
                                <div className="border-t border-slate-200 w-full absolute"></div>
                                <span className="bg-white px-4 text-sm text-slate-400 relative">Or sign up with</span>
                            </div>

                            <div className="flex justify-center gap-4">
                                <button className="p-3 border border-slate-200 rounded-2xl hover:bg-slate-100/70 transition-all hover:-translate-y-1">
                                    <img src="https://www.svgrepo.com/show/475656/google-color.svg" alt="Google" className="w-6 h-6" />
                                </button>
                                <button className="p-3 border border-slate-200 rounded-2xl hover:bg-slate-100/70 transition-all text-slate-800 hover:-translate-y-1">
                                    <Github className="w-6 h-6" />
                                </button>
                            </div>
                        </div>

                        <p className="text-center text-sm lg:text-base text-slate-600 mt-8">
                            Already have an account? <a href="/login" className="text-cyan-600 font-bold hover:underline">Login.</a>
                        </p>

                    </div>
                </div>
            </div>
        </div>
    );
}

export default SignUp;