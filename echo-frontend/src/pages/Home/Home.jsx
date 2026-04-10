import { Link } from 'react-router-dom';
import { ArrowRight, Rocket, ShieldCheck, Users } from 'lucide-react';

function Home() {
    return (
        <div className="min-h-screen bg-[#0B1325] font-sans text-slate-200 selection:bg-cyan-500/30">

            {/* Navbar (Üst Menü) */}
            <nav className="absolute top-0 w-full z-50 border-b border-white/5 bg-[#0B1325]/50 backdrop-blur-md">
                <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
                    <div className="flex items-center gap-2 text-2xl font-bold text-white tracking-wide">
                        <span className="text-cyan-400">|||</span> ECHO
                    </div>
                    <div className="flex items-center gap-4">
                        <Link to="/login" className="px-5 py-2.5 text-sm font-medium text-slate-300 hover:text-white transition-colors">
                            Giriş Yap
                        </Link>
                        <Link to="/signup" className="px-5 py-2.5 text-sm font-bold bg-cyan-500 hover:bg-cyan-400 text-[#0B1325] rounded-xl transition-colors shadow-lg shadow-cyan-500/20">
                            Kayıt Ol
                        </Link>
                    </div>
                </div>
            </nav>

            {/* Hero Section (Karşılama Alanı) */}
            <main className="relative pt-32 pb-20 lg:pt-48 lg:pb-32 overflow-hidden flex flex-col items-center text-center">

                {/* Arka Plan Parlamaları */}
                <div className="absolute top-1/4 left-1/2 -translate-x-1/2 w-[800px] h-[600px] bg-cyan-600/20 rounded-full blur-[150px] pointer-events-none"></div>
                <div className="absolute top-1/2 left-1/4 w-[500px] h-[500px] bg-blue-600/10 rounded-full blur-[120px] pointer-events-none"></div>

                <div className="max-w-4xl mx-auto px-6 relative z-10">
                    <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/5 border border-white/10 text-cyan-400 text-sm font-medium mb-8">
                        <span className="flex h-2 w-2 rounded-full bg-cyan-400 animate-pulse"></span>
                        ECHO Beta Yayında!
                    </div>

                    <h1 className="text-5xl md:text-7xl font-extrabold text-white mb-8 tracking-tight leading-tight">
                        Fikirlerinizi <span className="text-transparent bg-clip-text bg-gradient-to-r from-cyan-400 to-blue-500">Gerçeğe</span> Dönüştürün.
                    </h1>

                    <p className="text-lg md:text-xl text-slate-400 mb-12 max-w-2xl mx-auto leading-relaxed">
                        ECHO, yenilikçi projeleri destekleyen bir kitle fonlama platformudur. İnandığınız projelere güç verin veya kendi hayalinizi kitlelerle buluşturun.
                    </p>

                    <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                        <Link to="/signup" className="w-full sm:w-auto px-8 py-4 bg-cyan-500 hover:bg-cyan-400 text-[#0B1325] font-bold rounded-2xl transition-all flex items-center justify-center gap-2 shadow-lg shadow-cyan-500/25 hover:shadow-cyan-500/40 hover:-translate-y-1">
                            Projeleri Keşfet <ArrowRight size={20} />
                        </Link>
                        <Link to="/login" className="w-full sm:w-auto px-8 py-4 bg-[#111C3A] hover:bg-[#16244A] text-white font-medium border border-white/10 rounded-2xl transition-all flex items-center justify-center gap-2">
                            Kampanya Başlat
                        </Link>
                    </div>
                </div>
            </main>

            {/* Özellikler (Features) Alanı */}
            <section className="py-20 bg-[#111C3A]/50 border-t border-white/5 relative z-10">
                <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 md:grid-cols-3 gap-8">

                    <div className="bg-[#0B1325] p-8 rounded-3xl border border-white/5">
                        <div className="w-14 h-14 bg-cyan-500/10 rounded-2xl flex items-center justify-center text-cyan-400 mb-6">
                            <Rocket size={28} />
                        </div>
                        <h3 className="text-xl font-bold text-white mb-3">Hızlı Başlangıç</h3>
                        <p className="text-slate-400 leading-relaxed">Saniyeler içinde projenizi oluşturun ve global bir kitleye anında ulaşarak fon toplamaya başlayın.</p>
                    </div>

                    <div className="bg-[#0B1325] p-8 rounded-3xl border border-white/5">
                        <div className="w-14 h-14 bg-blue-500/10 rounded-2xl flex items-center justify-center text-blue-400 mb-6">
                            <ShieldCheck size={28} />
                        </div>
                        <h3 className="text-xl font-bold text-white mb-3">Güvenli Ödeme</h3>
                        <p className="text-slate-400 leading-relaxed">Tüm işlemleriniz en yüksek güvenlik standartlarıyla korunur. Hem destekçiler hem üreticiler için %100 güvenli.</p>
                    </div>

                    <div className="bg-[#0B1325] p-8 rounded-3xl border border-white/5">
                        <div className="w-14 h-14 bg-purple-500/10 rounded-2xl flex items-center justify-center text-purple-400 mb-6">
                            <Users size={28} />
                        </div>
                        <h3 className="text-xl font-bold text-white mb-3">Topluluk Gücü</h3>
                        <p className="text-slate-400 leading-relaxed">Sadece finansal destek değil, projenizi büyütecek harika bir topluluk inşa etme fırsatı yakalayın.</p>
                    </div>

                </div>
            </section>
        </div>
    );
}

export default Home;