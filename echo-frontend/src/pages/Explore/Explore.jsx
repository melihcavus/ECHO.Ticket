import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { Search, CalendarDays, Plus, X } from 'lucide-react';

import techImg from '../../assets/categories/technology.jpg';
import sportImg from '../../assets/categories/sports.jpg';
import artImg from '../../assets/categories/art.jpg';
import eduImg from '../../assets/categories/education.jpg';

import api from '../../services/api';

const getCategoryImage = (categoryName) => {
    switch(categoryName) {
        case 'Spor': return sportImg;
        case 'Sanat ve Tasarım': return artImg;
        case 'Eğitim': return eduImg;
        case 'Bilişim ve Teknoloji':
        default: return techImg;
    }
};

function Explore() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const { t } = useLanguage();

    const [events, setEvents] = useState([]);
    const [venues, setVenues] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    const [searchTerm, setSearchTerm] = useState('');

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState({
        title: '',
        description: '',
        eventDate: '',
        location: '',
        category: 'Bilişim ve Teknoloji',
        venueId: ''
    });

    const fetchEvents = async () => {
        setIsLoading(true);
        try {
            const response = await api.get('/events/explore');

            if (response.data.isSuccess) {
                setEvents(response.data.data);
            } else {
                setError(response.data.message);
            }
        } catch (err) {
            console.error("Hata:", err);
            setError(err.response?.data?.message || t('campaignsLoadError', 'Kampanyalar yüklenemedi.'));
        } finally {
            setIsLoading(false);
        }
    };

    const fetchVenues = async () => {
        try {
            const response = await api.get('/Venues');

            if (response.data.isSuccess) {
                setVenues(response.data.data);
            }
        } catch (err) {
            console.error("Sahneler çekilemedi:", err);
        }
    };

    useEffect(() => {
        fetchEvents();
        fetchVenues();
    }, []);

    const handleCreateEvent = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);

        try {
            const response = await api.post('/events', {
                title: formData.title,
                description: formData.description,
                eventDate: new Date(formData.eventDate).toISOString(),
                location: formData.location,
                category: formData.category,
                organizerId: user?.id,
                venueId: formData.venueId ? formData.venueId : null
            });

            if (response.data.isSuccess) {
                alert(t('eventCreated', 'Etkinlik başarıyla oluşturuldu!'));
                setIsModalOpen(false);
                setFormData({ title: '', description: '', eventDate: '', location: '', category: 'Bilişim ve Teknoloji', venueId: '' });
                fetchEvents();
            } else {
                alert(`${t('error', 'Hata')}: ${response.data.message || t('createFailed', 'Oluşturulamadı')}`);
            }
        } catch (err) {
            console.error(err);
            alert(err.response?.data?.message || t('serverComError', 'Sunucuyla iletişim kurulurken bir hata oluştu.'));
        } finally {
            setIsSubmitting(false);
        }
    };

    const filteredEvents = events.filter((event) => {
        const searchLower = searchTerm.toLowerCase();
        return (
            event.eventName?.toLowerCase().includes(searchLower) ||
            event.category?.toLowerCase().includes(searchLower) ||
            event.location?.toLowerCase().includes(searchLower)
        );
    });

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="explore" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-500/20 dark:bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none transition-colors duration-300"></div>

                    <div className="flex justify-between items-end mb-8 relative z-10">
                        <div>
                            <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 tracking-tight">{t('exploreCampaigns', 'Kampanyaları Keşfet')} 🚀</h1>
                            <p className="text-slate-600 dark:text-slate-400 text-sm">{t('exploreDesc', 'Dünyayı değiştirecek projelere ve heyecan verici etkinliklere göz at.')}</p>
                        </div>

                        {(user?.role === 'Admin' || user?.role === 'Organizer') && (
                            <button
                                onClick={() => setIsModalOpen(true)}
                                className="flex items-center gap-2 px-5 py-2.5 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-bold transition-all shadow-lg shadow-cyan-900/50"
                            >
                                <Plus size={20} />
                                {t('newCampaign', 'Yeni Kampanya')}
                            </button>
                        )}
                    </div>

                    <div className="relative w-full max-w-xl group mb-8 z-10">
                        <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 dark:text-slate-500 w-5 h-5 group-focus-within:text-cyan-600 dark:group-focus-within:text-cyan-400 transition-colors" />
                        <input
                            type="text"
                            placeholder={t('searchPlaceholder', 'Kampanya, bilet, kategori veya konum ara...')}
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full bg-white dark:bg-[#111C3A] border border-slate-200 dark:border-white/5 rounded-2xl py-3 pl-12 pr-4 text-sm text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-slate-500 focus:outline-none focus:border-cyan-500/50 transition-all shadow-sm dark:shadow-inner"
                        />
                    </div>

                    {isLoading ? (
                        <div className="text-center text-cyan-600 dark:text-cyan-400 py-10 relative z-10 animate-pulse font-medium">{t('loadingProjects', 'Projeler yükleniyor...')}</div>
                    ) : error ? (
                        <div className="text-center text-red-500 dark:text-red-400 py-10 relative z-10 font-medium">{error}</div>
                    ) : events.length === 0 ? (
                        <div className="text-center text-slate-500 dark:text-slate-400 py-10 relative z-10 bg-white dark:bg-[#111C3A]/50 rounded-3xl border border-slate-200 dark:border-white/5 shadow-sm">{t('noActiveCampaigns', 'Şu an aktif bir kampanya bulunmuyor.')}</div>
                    ) : filteredEvents.length === 0 ? (
                        <div className="text-center text-slate-500 dark:text-slate-400 py-10 relative z-10 bg-white dark:bg-[#111C3A]/50 rounded-3xl border border-slate-200 dark:border-white/5 shadow-sm">
                            {t('noMatchingEvents', 'Arama kriterlerinize uygun etkinlik bulunamadı.')}
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 relative z-10">
                            {filteredEvents.map((event) => (
                                <div key={event.eventId} className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 hover:border-cyan-400 dark:hover:border-cyan-500/30 transition-all group shadow-md dark:shadow-xl dark:shadow-black/20 overflow-hidden flex flex-col">
                                    <div
                                        className="h-32 relative bg-slate-100 dark:bg-[#16244A]"
                                        style={{
                                            backgroundImage: `url(${getCategoryImage(event.category)})`,
                                            backgroundSize: 'cover',
                                            backgroundPosition: 'center'
                                        }}
                                    >
                                        <div className="absolute inset-0 bg-gradient-to-t from-white dark:from-[#111C3A] to-transparent transition-colors duration-300"></div>
                                        <div className="absolute top-4 right-4 px-3 py-1 bg-white/80 dark:bg-black/60 backdrop-blur-md rounded-full border border-slate-200 dark:border-white/10 text-xs font-bold text-cyan-700 dark:text-cyan-300 z-10">
                                            {t('active', 'Aktif')}
                                        </div>
                                    </div>

                                    <div className="p-6 flex-1 flex flex-col">
                                        <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-2 group-hover:text-cyan-600 dark:group-hover:text-cyan-400 transition-colors line-clamp-2">
                                            {event.eventName}
                                        </h3>

                                        <div className="flex items-center gap-2 text-sm text-slate-500 dark:text-slate-400 mb-6">
                                            <CalendarDays size={16} />
                                            <span>{new Date(event.eventDate).toLocaleDateString('tr-TR')}</span>
                                        </div>

                                        <div className="mt-auto">
                                            <div className="flex justify-between items-end mb-2">
                                                {/* ROL KONTROLÜ: Admin parayı görür, User/Organizer doluluk oranını görür */}
                                                {user?.role === 'Admin' ? (
                                                    <>
                                                        <span className="text-xs text-slate-500">{t('totalPledged', 'Toplanan Destek')}</span>
                                                        <span className="font-bold text-slate-900 dark:text-white tracking-tight">₺{event.totalPledgeAmount?.toLocaleString('tr-TR') || 0}</span>
                                                    </>
                                                ) : (
                                                    <>
                                                        <span className="text-xs text-slate-500 font-medium">Doluluk Oranı</span>
                                                        <span className="font-bold text-slate-900 dark:text-white tracking-tight">
                                                            %{event.totalCapacity > 0 ? Math.min(Math.round((event.soldTickets / event.totalCapacity) * 100), 100) : 0}
                                                        </span>
                                                    </>
                                                )}
                                            </div>

                                            {/* DİNAMİK DOLULUK BARI: Gerçek kapasiteye göre hesaplanıyor */}
                                            <div className="w-full bg-slate-100 dark:bg-[#0B1325] rounded-full h-2 overflow-hidden border border-slate-200 dark:border-white/5">
                                                <div
                                                    className="bg-gradient-to-r from-cyan-500 to-blue-500 h-2 rounded-full transition-all duration-1000 ease-out"
                                                    style={{
                                                        width: `${event.totalCapacity > 0 ? Math.min((event.soldTickets / event.totalCapacity) * 100, 100) : 0}%`
                                                    }}
                                                ></div>
                                            </div>

                                            <button
                                                onClick={() => navigate(`/event/${event.eventId}`)}
                                                className="w-full mt-6 py-3 bg-slate-50 dark:bg-white/5 hover:bg-cyan-50 dark:hover:bg-cyan-600/20 text-cyan-700 dark:text-cyan-400 border border-slate-200 dark:border-white/5 hover:border-cyan-400 dark:hover:border-cyan-500/30 rounded-xl font-bold transition-all"
                                            >
                                                {t('viewProject', 'Projeyi İncele')}
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </main>

            {isModalOpen && (
                <div className="fixed inset-0 bg-slate-900/40 dark:bg-black/60 backdrop-blur-sm flex justify-center items-center z-50 p-4 transition-colors duration-300">
                    <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/10 w-full max-w-lg shadow-2xl flex flex-col max-h-[90vh] transition-colors duration-300">
                        <div className="flex justify-between items-center p-6 border-b border-slate-200 dark:border-white/10">
                            <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t('startNewCampaign', 'Yeni Kampanya Başlat')}</h2>
                            <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-slate-600 dark:hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form onSubmit={handleCreateEvent} className="space-y-4">
                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('campaignTitle', 'Kampanya Başlığı')}</label>
                                    <input
                                        type="text"
                                        required
                                        value={formData.title}
                                        onChange={(e) => setFormData({...formData, title: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                        placeholder={t('campaignTitlePlaceholder', 'Örn: Yeni Nesil Robotik Kol')}
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('category', 'Kategori')}</label>
                                    <select
                                        required
                                        value={formData.category}
                                        onChange={(e) => setFormData({...formData, category: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-slate-300 focus:border-cyan-500/50 focus:outline-none appearance-none transition-colors"
                                    >
                                        <option value="Bilişim ve Teknoloji">Bilişim ve Teknoloji</option>
                                        <option value="Spor">Spor</option>
                                        <option value="Sanat ve Tasarım">Sanat ve Tasarım</option>
                                        <option value="Eğitim">Eğitim</option>
                                    </select>
                                </div>

                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('venueSelect', 'Sahne / Mekan (Opsiyonel)')}</label>
                                    <select
                                        value={formData.venueId}
                                        onChange={(e) => setFormData({...formData, venueId: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-slate-300 focus:border-cyan-500/50 focus:outline-none appearance-none transition-colors"
                                    >
                                        <option value="">{t('noVenue', 'Sahne Yok / Ayakta / Çevrimiçi')}</option>
                                        {venues.map(v => (
                                            <option key={v.id} value={v.id}>
                                                {v.name} (Kapasite: {v.rows * v.columns})
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('location', 'Konum')}</label>
                                    <input
                                        type="text"
                                        required
                                        value={formData.location}
                                        onChange={(e) => setFormData({...formData, location: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                        placeholder={t('locationPlaceholder', 'Örn: İstanbul / Çevrimiçi')}
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('eventEndDate', 'Etkinlik/Bitiş Tarihi')}</label>
                                    <input
                                        type="datetime-local"
                                        required
                                        value={formData.eventDate}
                                        onChange={(e) => setFormData({...formData, eventDate: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-slate-300 focus:border-cyan-500/50 focus:outline-none [color-scheme:light] dark:[color-scheme:dark] transition-colors"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('description', 'Açıklama')}</label>
                                    <textarea
                                        required
                                        rows="4"
                                        value={formData.description}
                                        onChange={(e) => setFormData({...formData, description: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none resize-none transition-colors"
                                        placeholder={t('campaignDescPlaceholder', 'Projenizi detaylıca anlatın...')}
                                    ></textarea>
                                </div>

                                <button
                                    type="submit"
                                    disabled={isSubmitting}
                                    className={`w-full py-3 rounded-xl font-bold transition-all mt-4 shadow-lg
                                        ${isSubmitting ? 'bg-slate-300 dark:bg-slate-600 text-slate-500 dark:text-slate-400 cursor-not-allowed shadow-none' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                >
                                    {isSubmitting ? t('creating', 'Oluşturuluyor...') : t('createCampaign', 'Kampanyayı Oluştur')}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Explore;