import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar';
import { Search, CalendarDays, Plus, X } from 'lucide-react';

function Explore() {
    const navigate = useNavigate();
    const { user } = useAuth();

    const [events, setEvents] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    // Modal State'leri
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState({
        title: '',
        description: '',
        eventDate: '',
        location: ''
    });

    // Veri çekme fonksiyonunu dışarı aldık ki yeni etkinlik eklenince tekrar çağırabilelim
    const fetchEvents = async () => {
        setIsLoading(true);
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5216/api/events/explore`, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Etkinlikler çekilemedi');

            const result = await response.json();

            if (result.isSuccess) {
                setEvents(result.data);
            } else {
                setError(result.message);
            }
        } catch (err) {
            console.error("Hata:", err);
            setError("Kampanyalar yüklenemedi.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchEvents();
    }, []);

    // Form gönderme işlemi (UTC dönüşümü eklendi)
    const handleCreateEvent = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5216/api/events', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    title: formData.title,
                    description: formData.description,
                    // BURASI GÜNCELLENDİ: Yerel tarihi PostgreSQL'in istediği UTC formatına çeviriyoruz
                    eventDate: new Date(formData.eventDate).toISOString(),
                    location: formData.location,
                    organizerId: user?.id
                })
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                alert("Etkinlik başarıyla oluşturuldu!");
                setIsModalOpen(false);
                setFormData({ title: '', description: '', eventDate: '', location: '' }); // Formu sıfırla
                fetchEvents(); // Listeyi yenile
            } else {
                alert(`Hata: ${result.message || 'Oluşturulamadı'}`);
            }
        } catch (err) {
            console.error(err);
            alert("Sunucuyla iletişim kurulurken bir hata oluştu.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">
            <Sidebar activeMenu="explore" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <header className="h-24 bg-[#0B1325]/80 backdrop-blur-xl border-b border-white/5 flex items-center justify-between px-8 z-10 sticky top-0">
                    <div className="relative w-96 group">
                        <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 w-5 h-5 group-focus-within:text-cyan-400 transition-colors" />
                        <input type="text" placeholder="Kampanya, bilet ara..." className="w-full bg-[#111C3A] border border-white/5 rounded-2xl py-3 pl-12 pr-4 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-cyan-500/50 focus:bg-[#16244A] transition-all shadow-inner" />
                    </div>
                    <div className="flex items-center gap-4 cursor-pointer hover:opacity-80 transition-opacity">
                        <div className="text-right hidden sm:block">
                            <p className="text-sm font-bold text-white">{user?.firstName} {user?.lastName}</p>
                            <p className="text-xs text-cyan-400 font-medium">{user?.role || 'Kullanıcı'}</p>
                        </div>
                        <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-lg shadow-cyan-900/50 border-2 border-[#0B1325] ring-2 ring-cyan-500/30">
                            {user?.firstName?.charAt(0) || 'U'}{user?.lastName?.charAt(0) || ''}
                        </div>
                    </div>
                </header>

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none"></div>

                    <div className="flex justify-between items-end mb-8 relative z-10">
                        <div>
                            <h1 className="text-3xl font-extrabold text-white mb-2 tracking-tight">Kampanyaları Keşfet 🚀</h1>
                            <p className="text-slate-400 text-sm">Dünyayı değiştirecek projelere ve heyecan verici etkinliklere göz at.</p>
                        </div>

                        {/* SADECE ADMIN/ORGANIZER GÖREBİLİR */}
                        {(user?.role === 'Admin' || user?.role === 'Organizer') && (
                            <button
                                onClick={() => setIsModalOpen(true)}
                                className="flex items-center gap-2 px-5 py-2.5 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-bold transition-all shadow-lg shadow-cyan-900/50"
                            >
                                <Plus size={20} />
                                Yeni Kampanya
                            </button>
                        )}
                    </div>

                    {isLoading ? (
                        <div className="text-center text-cyan-400 py-10 relative z-10 animate-pulse font-medium">Projeler yükleniyor...</div>
                    ) : error ? (
                        <div className="text-center text-red-400 py-10 relative z-10 font-medium">{error}</div>
                    ) : events.length === 0 ? (
                        <div className="text-center text-slate-400 py-10 relative z-10 bg-[#111C3A]/50 rounded-3xl border border-white/5">Şu an aktif bir kampanya bulunmuyor.</div>
                    ) : (
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 relative z-10">
                            {events.map((event) => (
                                <div key={event.eventId} className="bg-[#111C3A] rounded-3xl border border-white/5 hover:border-cyan-500/30 transition-all group shadow-xl shadow-black/20 overflow-hidden flex flex-col">
                                    <div className="h-32 bg-gradient-to-br from-[#16244A] to-[#1A2744] relative">
                                        <div className="absolute top-4 right-4 px-3 py-1 bg-black/40 backdrop-blur-md rounded-full border border-white/10 text-xs font-medium text-cyan-300">
                                            Aktif
                                        </div>
                                    </div>

                                    <div className="p-6 flex-1 flex flex-col">
                                        <h3 className="text-lg font-bold text-white mb-2 group-hover:text-cyan-400 transition-colors line-clamp-2">
                                            {event.eventName}
                                        </h3>

                                        <div className="flex items-center gap-2 text-sm text-slate-400 mb-6">
                                            <CalendarDays size={16} />
                                            <span>{new Date(event.eventDate).toLocaleDateString('tr-TR')}</span>
                                        </div>

                                        <div className="mt-auto">
                                            <div className="flex justify-between items-end mb-2">
                                                <span className="text-xs text-slate-500">Toplanan Destek</span>
                                                <span className="font-bold text-white tracking-tight">₺{event.totalPledgeAmount?.toLocaleString('tr-TR') || 0}</span>
                                            </div>
                                            <div className="w-full bg-[#0B1325] rounded-full h-2 overflow-hidden border border-white/5">
                                                <div className="bg-gradient-to-r from-cyan-500 to-blue-500 h-2 rounded-full w-[15%]"></div>
                                            </div>

                                            <button
                                                onClick={() => navigate(`/event/${event.eventId}`)}
                                                className="w-full mt-6 py-3 bg-white/5 hover:bg-cyan-600/20 text-cyan-400 border border-white/5 hover:border-cyan-500/30 rounded-xl font-medium transition-all"
                                            >
                                                Projeyi İncele
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </main>

            {/* ETKİNLİK OLUŞTURMA MODALI */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex justify-center items-center z-50 p-4">
                    <div className="bg-[#111C3A] rounded-3xl border border-white/10 w-full max-w-lg shadow-2xl flex flex-col max-h-[90vh]">
                        <div className="flex justify-between items-center p-6 border-b border-white/10">
                            <h2 className="text-xl font-bold text-white">Yeni Kampanya Başlat</h2>
                            <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form onSubmit={handleCreateEvent} className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-1">Kampanya Başlığı</label>
                                    <input
                                        type="text"
                                        required
                                        value={formData.title}
                                        onChange={(e) => setFormData({...formData, title: e.target.value})}
                                        className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none"
                                        placeholder="Örn: Yeni Nesil Robotik Kol"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-1">Konum</label>
                                    <input
                                        type="text"
                                        required
                                        value={formData.location}
                                        onChange={(e) => setFormData({...formData, location: e.target.value})}
                                        className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none"
                                        placeholder="Örn: İstanbul / Çevrimiçi"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-1">Etkinlik/Bitiş Tarihi</label>
                                    <input
                                        type="datetime-local"
                                        required
                                        value={formData.eventDate}
                                        onChange={(e) => setFormData({...formData, eventDate: e.target.value})}
                                        className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-slate-300 focus:border-cyan-500/50 focus:outline-none [color-scheme:dark]"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-1">Açıklama</label>
                                    <textarea
                                        required
                                        rows="4"
                                        value={formData.description}
                                        onChange={(e) => setFormData({...formData, description: e.target.value})}
                                        className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none resize-none"
                                        placeholder="Projenizi detaylıca anlatın..."
                                    ></textarea>
                                </div>

                                <button
                                    type="submit"
                                    disabled={isSubmitting}
                                    className={`w-full py-3 rounded-xl font-bold transition-all mt-4
                                        ${isSubmitting ? 'bg-slate-600 text-slate-400 cursor-not-allowed' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-lg shadow-cyan-900/50'}`}
                                >
                                    {isSubmitting ? 'Oluşturuluyor...' : 'Kampanyayı Oluştur'}
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