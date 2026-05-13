import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar'; // YENİ SİDEBAR BİLEŞENİ
import { Search, CalendarDays } from 'lucide-react';

function Explore() {
    const navigate = useNavigate();
    const { user } = useAuth(); // logout fonksiyonunu sildik

    const [events, setEvents] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
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

        fetchEvents();
    }, []);

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">

            {/* YENİ OLUŞTURDUĞUMUZ MERKEZİ SIDEBAR */}
            <Sidebar activeMenu="explore" />

            {/* ANA İÇERİK */}
            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                {/* Üst Bar */}
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
                            {user?.firstName?.charAt(0)}{user?.lastName?.charAt(0)}
                        </div>
                    </div>
                </header>

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none"></div>

                    <div className="mb-8 relative z-10">
                        <h1 className="text-3xl font-extrabold text-white mb-2 tracking-tight">Kampanyaları Keşfet 🚀</h1>
                        <p className="text-slate-400 text-sm">Dünyayı değiştirecek projelere ve heyecan verici etkinliklere göz at.</p>
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
                                                <span className="font-bold text-white tracking-tight">₺{event.totalPledgeAmount.toLocaleString('tr-TR')}</span>
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
        </div>
    );
}

export default Explore;