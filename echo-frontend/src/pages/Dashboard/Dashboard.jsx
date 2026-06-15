import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import {
    Ticket,
    FolderHeart,
    Wallet,
    TrendingUp,
    CalendarDays,
    Activity,
    Sparkles
} from 'lucide-react';

// Kategori görsellerini Dashboard'a da dahil ediyoruz
import techImg from '../../assets/categories/technology.jpg';
import sportImg from '../../assets/categories/sports.jpg';
import artImg from '../../assets/categories/art.jpg';
import eduImg from '../../assets/categories/education.jpg';

const getCategoryImage = (categoryName) => {
    switch(categoryName) {
        case 'Spor': return sportImg;
        case 'Sanat ve Tasarım': return artImg;
        case 'Eğitim': return eduImg;
        case 'Bilişim ve Teknoloji':
        default: return techImg;
    }
};

function Dashboard() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const { t } = useLanguage();

    // Mevcut Dashboard State'leri
    const [summaryData, setSummaryData] = useState({
        totalPledgeAmount: 0,
        activeProjectCount: 0,
        upcomingEventCount: 0,
        recentActivities: []
    });
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    // Yapay Zeka Öneri State'leri
    const [aiRecommendations, setAiRecommendations] = useState([]);
    const [isAiLoading, setIsAiLoading] = useState(false);
    const [aiError, setAiError] = useState(null);

    // Dashboard özetini çeken efekt
    useEffect(() => {
        const fetchDashboardSummary = async () => {
            if (!user?.id) {
                setIsLoading(false);
                setError(t('authError', 'Kullanıcı kimliği bulunamadı. Lütfen tekrar giriş yapın.'));
                return;
            }

            setIsLoading(true);
            try {
                const token = localStorage.getItem('token');

                const response = await fetch(`http://localhost:5216/api/dashboard/summary/${user.id}`, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    }
                });

                if (!response.ok) {
                    throw new Error(t('fetchError', 'Veri çekilemedi'));
                }

                const result = await response.json();

                if (result.isSuccess) {
                    setSummaryData(result.data);
                } else {
                    setError(result.message);
                }
            } catch (err) {
                setError(t('statsLoadError', 'İstatistikler yüklenemedi.'));
            } finally {
                setIsLoading(false);
            }
        };

        fetchDashboardSummary();
    }, [user?.id, t]);

    // Yapay Zeka önerilerini çeken efekt
    useEffect(() => {
        const fetchAiRecommendations = async () => {
            if (!user?.id) return;

            setIsAiLoading(true);
            try {
                const token = localStorage.getItem('token');
                const response = await fetch(`http://localhost:5216/api/Recommendation/ForUser/${user.id}`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });

                if (!response.ok) throw new Error('AI servisine ulaşılamadı');

                const data = await response.json();
                setAiRecommendations(data);
            } catch (err) {
                console.error("Yapay Zeka Hatası:", err);
                setAiError(t('aiLoadError', 'Öneriler yüklenirken bir sorun oluştu.'));
            } finally {
                setIsAiLoading(false);
            }
        };

        fetchAiRecommendations();
    }, [user?.id, t]);

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="dashboard" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-500/20 dark:bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none transition-colors duration-300"></div>
                    <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-blue-500/10 dark:bg-blue-600/5 rounded-full blur-[100px] pointer-events-none transition-colors duration-300"></div>

                    <div className="mb-10 relative z-10">
                        <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 tracking-tight">
                            {t('welcomeBack', 'Tekrar hoş geldin')}, {user?.firstName}! ✨
                        </h1>
                        <p className="text-slate-600 dark:text-slate-400 text-sm">
                            {t('dashboardSubtitle', 'İşte bugün projelerinde ve biletlerinde olan bitenler.')}
                        </p>
                    </div>

                    {isLoading ? (
                        <div className="text-center text-cyan-600 dark:text-cyan-400 py-10 relative z-10 animate-pulse font-medium">
                            {t('loadingStats', 'İstatistikler yükleniyor...')}
                        </div>
                    ) : error ? (
                        <div className="text-center text-red-500 dark:text-red-400 py-10 relative z-10 font-medium">
                            {error}
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10 relative z-10">
                            {/* MEVCUT 3'LÜ KART YAPISI (Değişmedi) */}
                            <div className="bg-white dark:bg-[#111C3A] p-6 rounded-3xl border border-slate-200 dark:border-white/5 hover:border-emerald-400 dark:hover:border-emerald-500/30 transition-all group shadow-md dark:shadow-xl dark:shadow-black/20">
                                <div className="flex justify-between items-start mb-4">
                                    <div>
                                        <h3 className="text-slate-500 dark:text-slate-400 text-sm font-bold mb-1">{t('totalPledge', 'Toplam Destek')}</h3>
                                        <div className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">
                                            ₺{summaryData.totalPledgeAmount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
                                        </div>
                                    </div>
                                    <div className="p-3 bg-emerald-100 dark:bg-emerald-500/10 rounded-2xl text-emerald-600 dark:text-emerald-400 group-hover:scale-110 transition-transform">
                                        <Wallet size={24} />
                                    </div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <span className="px-2 py-1 bg-emerald-100 dark:bg-emerald-500/10 text-emerald-700 dark:text-emerald-400 rounded-lg text-xs font-bold flex items-center gap-1">
                                        <TrendingUp size={12}/> +12%
                                    </span>
                                    <span className="text-xs text-slate-500 dark:text-slate-500">{t('thisMonth', 'bu ay')}</span>
                                </div>
                            </div>

                            <div className="bg-white dark:bg-[#111C3A] p-6 rounded-3xl border border-slate-200 dark:border-white/5 hover:border-blue-400 dark:hover:border-blue-500/30 transition-all group shadow-md dark:shadow-xl dark:shadow-black/20">
                                <div className="flex justify-between items-start mb-4">
                                    <div>
                                        <h3 className="text-slate-500 dark:text-slate-400 text-sm font-bold mb-1">{t('activeProjects', 'Aktif Projeler')}</h3>
                                        <div className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">
                                            {summaryData.activeProjectCount}
                                        </div>
                                    </div>
                                    <div className="p-3 bg-blue-100 dark:bg-blue-500/10 rounded-2xl text-blue-600 dark:text-blue-400 group-hover:scale-110 transition-transform">
                                        <Activity size={24} />
                                    </div>
                                </div>
                                <div className="flex items-center gap-2 mt-3">
                                    <span className="text-xs text-slate-600 dark:text-slate-400 flex items-center gap-1 font-medium">
                                        <span className="w-2 h-2 rounded-full bg-blue-500 animate-pulse"></span>
                                        2 {t('campaignsEndingSoon', 'kampanya yakında bitiyor')}
                                    </span>
                                </div>
                            </div>

                            <div className="bg-white dark:bg-[#111C3A] p-6 rounded-3xl border border-slate-200 dark:border-white/5 hover:border-purple-400 dark:hover:border-purple-500/30 transition-all group shadow-md dark:shadow-xl dark:shadow-black/20">
                                <div className="flex justify-between items-start mb-4">
                                    <div>
                                        <h3 className="text-slate-500 dark:text-slate-400 text-sm font-bold mb-1">{t('upcomingEvents', 'Yaklaşan Etkinlikler')}</h3>
                                        <div className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">
                                            {summaryData.upcomingEventCount} {t('ticket', 'Bilet')}
                                        </div>
                                    </div>
                                    <div className="p-3 bg-purple-100 dark:bg-purple-500/10 rounded-2xl text-purple-600 dark:text-purple-400 group-hover:scale-110 transition-transform">
                                        <Ticket size={24} />
                                    </div>
                                </div>
                                <div className="flex items-center gap-2 mt-3">
                                    <span className="text-xs text-slate-600 dark:text-slate-400 flex items-center gap-1 font-medium">
                                        <CalendarDays size={14} className="text-purple-500 dark:text-purple-400"/>
                                        {t('nextEventIn', 'Sıradaki etkinlik 5 gün içinde')}
                                    </span>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* --- YENİ YERİNDE YAPAY ZEKA VİTRİNİ BAŞLANGICI --- */}
                    {user?.id && (
                        <div className="mb-10 relative z-10">
                            <h2 className="text-xl font-bold text-slate-900 dark:text-white mb-6 flex items-center gap-2">
                                <Sparkles className="text-cyan-500" size={24} />
                                {t('aiRecommendationsTitle', 'Senin İçin Seçildi')} 🎯
                            </h2>

                            {isAiLoading ? (
                                <div className="text-sm text-cyan-600 dark:text-cyan-400 animate-pulse bg-cyan-50 dark:bg-cyan-900/10 p-4 rounded-2xl border border-cyan-100 dark:border-cyan-900/30">
                                    {t('aiLoadingText', 'Yapay zeka profilini analiz ediyor ve en uygun etkinlikleri seçiyor...')}
                                </div>
                            ) : aiError ? (
                                <div className="text-sm text-red-500 p-4 bg-red-50 dark:bg-red-900/10 rounded-2xl border border-red-100 dark:border-red-900/30">
                                    {aiError}
                                </div>
                            ) : aiRecommendations.length > 0 ? (
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                                    {aiRecommendations.map((event) => {
                                        // İsim problemini çözmek için muhtemel property isimlerini yakalıyoruz
                                        const eventName = event.title || event.eventName || event.name || "Bilinmeyen Etkinlik";
                                        const eventId = event.id || event.eventId;

                                        return (
                                            <div key={eventId} className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 hover:border-cyan-400 dark:hover:border-cyan-500/30 transition-all group shadow-md dark:shadow-xl dark:shadow-black/20 overflow-hidden flex flex-col relative cursor-pointer" onClick={() => navigate(`/event/${eventId}`)}>
                                                <div className="absolute top-4 right-4 px-3 py-1 bg-cyan-500 text-white text-[11px] font-bold rounded-full z-10 shadow-lg flex items-center gap-1">
                                                    <Sparkles size={12} /> Yapay Zeka Önerisi
                                                </div>
                                                <div
                                                    className="h-32 relative bg-slate-100 dark:bg-[#16244A]"
                                                    style={{ backgroundImage: `url(${getCategoryImage(event.category)})`, backgroundSize: 'cover', backgroundPosition: 'center' }}
                                                >
                                                    <div className="absolute inset-0 bg-gradient-to-t from-white dark:from-[#111C3A] to-transparent"></div>
                                                </div>
                                                <div className="p-6 flex-1 flex flex-col">
                                                    <h3 className="font-bold text-slate-900 dark:text-white mb-2 group-hover:text-cyan-600 dark:group-hover:text-cyan-400 transition-colors line-clamp-2" title={eventName}>
                                                        {eventName}
                                                    </h3>
                                                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400 mb-6">{event.category}</p>
                                                    <button className="mt-auto w-full py-2.5 bg-slate-50 dark:bg-white/5 hover:bg-cyan-50 dark:hover:bg-cyan-600/20 text-cyan-700 dark:text-cyan-400 border border-slate-200 dark:border-white/5 hover:border-cyan-400 dark:hover:border-cyan-500/30 rounded-xl font-bold text-sm transition-all">
                                                        {t('viewProject', 'Projeyi İncele')}
                                                    </button>
                                                </div>
                                            </div>
                                        )})}
                                </div>
                            ) : null}
                        </div>
                    )}
                    {/* --- YAPAY ZEKA VİTRİNİ BİTİŞ --- */}

                    {summaryData.recentActivities && summaryData.recentActivities.length > 0 ? (
                        <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 overflow-hidden relative z-10 shadow-md dark:shadow-xl dark:shadow-black/20 p-8 transition-colors duration-300">
                            <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-6">{t('recentActivities', 'Son İşlemlerim')}</h3>
                            <div className="space-y-4">
                                {summaryData.recentActivities.map((activity) => (
                                    <div key={activity.activityId} className="flex items-center justify-between p-4 bg-slate-50 dark:bg-[#1A2744] rounded-2xl border border-slate-200 dark:border-white/5 hover:border-cyan-400 dark:hover:border-cyan-500/30 transition-all group">
                                        <div className="flex items-center gap-4">
                                            <div className="w-12 h-12 bg-cyan-100 dark:bg-cyan-500/10 rounded-full flex items-center justify-center text-cyan-600 dark:text-cyan-400 group-hover:scale-110 transition-transform">
                                                <Ticket size={20} />
                                            </div>
                                            <div>
                                                <h4 className="text-slate-900 dark:text-white font-bold">{activity.eventName}</h4>
                                                <p className="text-sm text-slate-500 dark:text-slate-400">
                                                    {new Date(activity.date).toLocaleDateString('tr-TR')} • {activity.type}
                                                </p>
                                            </div>
                                        </div>
                                        <div className="text-right">
                                            <div className="text-slate-900 dark:text-white font-extrabold tracking-tight">
                                                ₺{activity.amount.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    ) : (
                        <div className="border-2 border-dashed border-slate-300 dark:border-white/10 bg-white/50 dark:bg-[#111C3A]/30 rounded-3xl p-10 flex flex-col items-center justify-center text-center relative z-10 transition-colors duration-300">
                            <div className="w-20 h-20 bg-slate-100 dark:bg-[#16244A] rounded-full flex items-center justify-center mb-5 transition-colors">
                                <FolderHeart className="w-10 h-10 text-cyan-500/50" />
                            </div>
                            <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-2">{t('noRecentActivity', 'Son Etkinlik Yok')}</h3>
                            <p className="text-slate-500 dark:text-slate-400 max-w-sm mb-6">{t('noActivityDesc', 'Son zamanlarda herhangi bir kampanyayı desteklemedin veya bilet almadın. Başlamak için platformu keşfet!')}</p>

                            <button
                                onClick={() => navigate('/explore')}
                                className="px-6 py-3 bg-white dark:bg-white/5 hover:bg-slate-50 dark:hover:bg-white/10 text-slate-900 dark:text-white rounded-xl transition-colors font-bold border border-slate-200 dark:border-white/10 shadow-sm dark:shadow-none"
                            >
                                {t('exploreCampaignsBtn', 'Kampanyaları Keşfet')}
                            </button>
                        </div>
                    )}
                </div>
            </main>
        </div>
    );
}
export default Dashboard;