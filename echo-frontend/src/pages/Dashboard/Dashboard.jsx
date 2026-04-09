import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import {
    LayoutDashboard,
    Ticket,
    FolderHeart,
    Wallet,
    Settings,
    LogOut,
    TrendingUp,
    Search,
    CalendarDays,
    Activity
} from 'lucide-react';

function Dashboard() {
    const navigate = useNavigate();
    const { user, logout } = useAuth();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">

            {/* SOL MENÜ (SIDEBAR) */}
            <aside className="w-64 bg-[#111C3A] border-r border-white/5 flex flex-col justify-between hidden md:flex z-20">
                <div>
                    {/* Logo Alanı */}
                    <div className="h-24 flex items-center px-8 border-b border-white/5">
                        <span className="text-white text-2xl font-bold flex items-center gap-2 tracking-wide">
                            <span className="text-cyan-400">|||</span> ECHO
                        </span>
                    </div>

                    {/* Menü Linkleri */}
                    <nav className="p-4 space-y-2 mt-4">
                        {/* Aktif Menü - Gradient ve Shadow */}
                        <a href="#" className="flex items-center gap-3 px-4 py-3 bg-gradient-to-r from-cyan-600 to-blue-600 text-white rounded-xl shadow-lg shadow-cyan-900/20 transition-all">
                            <LayoutDashboard size={20} />
                            <span className="font-medium">Genel Bakış</span>
                        </a>
                        <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:text-white hover:bg-[#1A2744] rounded-xl transition-all">
                            <FolderHeart size={20} />
                            <span className="font-medium">Desteklerim</span>
                        </a>
                        <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:text-white hover:bg-[#1A2744] rounded-xl transition-all">
                            <Ticket size={20} />
                            <span className="font-medium">Biletlerim</span>
                        </a>
                        <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:text-white hover:bg-[#1A2744] rounded-xl transition-all">
                            <Wallet size={20} />
                            <span className="font-medium">İşlemler</span>
                        </a>
                    </nav>
                </div>

                {/* Alt Menü & Çıkış */}
                <div className="p-4 border-t border-white/5 space-y-2 bg-[#0D162B]">
                    <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:text-white hover:bg-[#1A2744] rounded-xl transition-all">
                        <Settings size={20} />
                        <span className="font-medium">Ayarlar</span>
                    </a>
                    <button
                        onClick={handleLogout}
                        className="w-full flex items-center gap-3 px-4 py-3 text-red-400 hover:text-red-300 hover:bg-red-500/10 rounded-xl transition-all"
                    >
                        <LogOut size={20} />
                        <span className="font-medium">Çıkış Yap</span>
                    </button>
                </div>
            </aside>

            {/* ANA İÇERİK ALANI */}
            <main className="flex-1 flex flex-col h-screen overflow-hidden">

                {/* Üst Bar (Header) */}
                <header className="h-24 bg-[#0B1325]/80 backdrop-blur-xl border-b border-white/5 flex items-center justify-between px-8 z-10 sticky top-0">
                    <div className="relative w-96 group">
                        <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 w-5 h-5 group-focus-within:text-cyan-400 transition-colors" />
                        <input
                            type="text"
                            placeholder="Kampanya, bilet ara..."
                            className="w-full bg-[#111C3A] border border-white/5 rounded-2xl py-3 pl-12 pr-4 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500/50 focus:bg-[#16244A] transition-all shadow-inner"
                        />
                    </div>

                    <div className="flex items-center gap-4 cursor-pointer hover:opacity-80 transition-opacity">
                        <div className="text-right hidden sm:block">
                            <p className="text-sm font-bold text-white">{user?.firstName} {user?.lastName}</p>
                            <p className="text-xs text-cyan-400 font-medium">{user?.role || 'Kullanıcı'}</p>
                        </div>
                        <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-lg shadow-cyan-900/50 border-2 border-[#0B1325] ring-2 ring-cyan-500/30">
                            {user?.firstName?.charAt(0)}{user?.lastName?.charAt(0)}
                        </div>
                    </div>
                </header>

                {/* Kaydırılabilir İçerik */}
                <div className="flex-1 overflow-y-auto p-8 relative">

                    {/* Arka plan renk cümbüşü (Hafif Glow) */}
                    <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none"></div>
                    <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-blue-600/5 rounded-full blur-[100px] pointer-events-none"></div>

                    <div className="mb-10 relative z-10">
                        <h1 className="text-3xl font-extrabold text-white mb-2 tracking-tight">Tekrar hoş geldin, {user?.firstName}! ✨</h1>
                        <p className="text-slate-400 text-sm">İşte bugün projelerinde ve biletlerinde olan bitenler.</p>
                    </div>

                    {/* İstatistik Kartları */}
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10 relative z-10">

                        {/* Kart 1: Para (Yeşil Tema) */}
                        <div className="bg-[#111C3A] p-6 rounded-3xl border border-white/5 hover:border-emerald-500/30 transition-all group shadow-xl shadow-black/20">
                            <div className="flex justify-between items-start mb-4">
                                <div>
                                    <h3 className="text-slate-400 text-sm font-medium mb-1">Toplam Destek</h3>
                                    <div className="text-3xl font-bold text-white tracking-tight">₺1,250.00</div>
                                </div>
                                <div className="p-3 bg-emerald-500/10 rounded-2xl text-emerald-400 group-hover:scale-110 transition-transform">
                                    <Wallet size={24} />
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="px-2 py-1 bg-emerald-500/10 text-emerald-400 rounded-lg text-xs font-bold flex items-center gap-1">
                                    <TrendingUp size={12}/> +12%
                                </span>
                                <span className="text-xs text-slate-500">bu ay</span>
                            </div>
                        </div>

                        {/* Kart 2: Projeler (Mavi Tema) */}
                        <div className="bg-[#111C3A] p-6 rounded-3xl border border-white/5 hover:border-blue-500/30 transition-all group shadow-xl shadow-black/20">
                            <div className="flex justify-between items-start mb-4">
                                <div>
                                    <h3 className="text-slate-400 text-sm font-medium mb-1">Aktif Projeler</h3>
                                    <div className="text-3xl font-bold text-white tracking-tight">4</div>
                                </div>
                                <div className="p-3 bg-blue-500/10 rounded-2xl text-blue-400 group-hover:scale-110 transition-transform">
                                    <Activity size={24} />
                                </div>
                            </div>
                            <div className="flex items-center gap-2 mt-3">
                                <span className="text-xs text-slate-400 flex items-center gap-1">
                                    <span className="w-2 h-2 rounded-full bg-blue-500 animate-pulse"></span>
                                    2 kampanya yakında bitiyor
                                </span>
                            </div>
                        </div>

                        {/* Kart 3: Biletler (Mor/Turuncu Tema) */}
                        <div className="bg-[#111C3A] p-6 rounded-3xl border border-white/5 hover:border-purple-500/30 transition-all group shadow-xl shadow-black/20">
                            <div className="flex justify-between items-start mb-4">
                                <div>
                                    <h3 className="text-slate-400 text-sm font-medium mb-1">Yaklaşan Etkinlikler</h3>
                                    <div className="text-3xl font-bold text-white tracking-tight">2 Bilet</div>
                                </div>
                                <div className="p-3 bg-purple-500/10 rounded-2xl text-purple-400 group-hover:scale-110 transition-transform">
                                    <Ticket size={24} />
                                </div>
                            </div>
                            <div className="flex items-center gap-2 mt-3">
                                <span className="text-xs text-slate-400 flex items-center gap-1">
                                    <CalendarDays size={14} className="text-purple-400"/>
                                    Sıradaki etkinlik 5 gün içinde
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Boş Durum (Empty State) */}
                    <div className="border-2 border-dashed border-white/10 bg-[#111C3A]/30 rounded-3xl p-10 flex flex-col items-center justify-center text-center relative z-10">
                        <div className="w-20 h-20 bg-[#16244A] rounded-full flex items-center justify-center mb-5">
                            <FolderHeart className="w-10 h-10 text-cyan-500/50" />
                        </div>
                        <h3 className="text-xl font-bold text-white mb-2">Son Etkinlik Yok</h3>
                        <p className="text-slate-400 max-w-sm mb-6">Son zamanlarda herhangi bir kampanyayı desteklemedin veya bilet almadın. Başlamak için platformu keşfet!</p>
                        <button className="px-6 py-3 bg-white/5 hover:bg-white/10 text-white rounded-xl transition-colors font-medium border border-white/10">
                            Kampanyaları Keşfet
                        </button>
                    </div>

                </div>
            </main>
        </div>
    );
}

export default Dashboard;