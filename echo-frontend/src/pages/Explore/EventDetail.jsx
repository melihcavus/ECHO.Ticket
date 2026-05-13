import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar'; // YENİ SİDEBAR BİLEŞENİ
import { ArrowLeft, CalendarDays, MapPin, User, CheckCircle2 } from 'lucide-react';

function EventDetail() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth(); // logout fonksiyonunu sildik

    const [eventData, setEventData] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchEventDetail = async () => {
            setIsLoading(true);
            try {
                const token = localStorage.getItem('token');
                const response = await fetch(`http://localhost:5216/api/events/explore/${id}`, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    }
                });

                if (!response.ok) throw new Error('Etkinlik detayları çekilemedi');

                const result = await response.json();

                if (result.isSuccess) {
                    setEventData(result.data);
                } else {
                    setError(result.message);
                }
            } catch (err) {
                console.error("Hata:", err);
                setError("Etkinlik yüklenirken bir sorun oluştu.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchEventDetail();
    }, [id]);

    const handleBuyTicket = (ticketId) => {
        // TODO: RabbitMQ entegrasyonu burada yapılacak!
        console.log(`Satın alınacak bilet ID: ${ticketId}`);
        alert("Satın alma altyapısı (RabbitMQ) sıradaki adımda eklenecek!");
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">

            {/* YENİ OLUŞTURDUĞUMUZ MERKEZİ SIDEBAR */}
            <Sidebar activeMenu="explore" />

            {/* ANA İÇERİK */}
            <main className="flex-1 flex flex-col h-screen overflow-hidden relative">
                <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none z-0"></div>

                {/* Üst Bar */}
                <header className="h-24 bg-[#0B1325]/80 backdrop-blur-xl border-b border-white/5 flex items-center justify-between px-8 z-10 sticky top-0">
                    <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-slate-400 hover:text-white transition-colors">
                        <ArrowLeft size={20} />
                        <span className="font-medium">Geri Dön</span>
                    </button>
                    <div className="flex items-center gap-4 cursor-pointer hover:opacity-80 transition-opacity">
                        <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-lg shadow-cyan-900/50">
                            {user?.firstName?.charAt(0)}{user?.lastName?.charAt(0)}
                        </div>
                    </div>
                </header>

                <div className="flex-1 overflow-y-auto p-8 relative z-10">
                    {isLoading ? (
                        <div className="text-center text-cyan-400 py-10 animate-pulse font-medium">Detaylar yükleniyor...</div>
                    ) : error ? (
                        <div className="text-center text-red-400 py-10 font-medium">{error}</div>
                    ) : eventData ? (
                        <div className="max-w-5xl mx-auto space-y-8">
                            {/* Başlık ve Temel Bilgiler */}
                            <div className="bg-[#111C3A] rounded-3xl border border-white/5 p-8 shadow-xl">
                                <div className="inline-block px-3 py-1 bg-cyan-500/10 text-cyan-400 rounded-full text-sm font-medium mb-4">
                                    Aktif Kampanya
                                </div>
                                <h1 className="text-4xl font-extrabold text-white mb-6 tracking-tight">{eventData.title}</h1>

                                <div className="flex flex-wrap gap-6 text-slate-400">
                                    <div className="flex items-center gap-2">
                                        <CalendarDays size={18} className="text-cyan-500" />
                                        <span>{new Date(eventData.eventDate).toLocaleDateString('tr-TR')}</span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <MapPin size={18} className="text-cyan-500" />
                                        <span>{eventData.location}</span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <User size={18} className="text-cyan-500" />
                                        <span>{eventData.organizerName}</span>
                                    </div>
                                </div>
                            </div>

                            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                                {/* Açıklama Alanı */}
                                <div className="lg:col-span-2 space-y-6">
                                    <div className="bg-[#111C3A] rounded-3xl border border-white/5 p-8 shadow-xl">
                                        <h2 className="text-xl font-bold text-white mb-4">Proje Hakkında</h2>
                                        <p className="text-slate-300 leading-relaxed whitespace-pre-wrap">
                                            {eventData.description || "Bu proje için henüz detaylı bir açıklama girilmemiştir."}
                                        </p>
                                    </div>
                                </div>

                                {/* Biletler / Paketler Alanı */}
                                <div className="space-y-6">
                                    <h2 className="text-xl font-bold text-white px-2">Destek Paketleri</h2>
                                    {eventData.tickets && eventData.tickets.length > 0 ? (
                                        eventData.tickets.map(ticket => (
                                            <div key={ticket.ticketId} className="bg-gradient-to-b from-[#16244A] to-[#111C3A] rounded-3xl border border-white/5 p-6 shadow-xl hover:border-cyan-500/30 transition-all group">
                                                <h3 className="text-lg font-bold text-white mb-2">{ticket.name}</h3>
                                                <div className="text-2xl font-extrabold text-cyan-400 mb-4">
                                                    ₺{ticket.price.toLocaleString('tr-TR')}
                                                </div>
                                                <p className="text-sm text-slate-400 mb-6 min-h-[40px]">
                                                    {ticket.description}
                                                </p>

                                                <div className="flex items-center justify-between text-xs text-slate-500 mb-4">
                                                    <span className="flex items-center gap-1"><CheckCircle2 size={14} /> Stok: {ticket.remainingCapacity}</span>
                                                </div>

                                                <button
                                                    onClick={() => handleBuyTicket(ticket.ticketId)}
                                                    className="w-full py-3 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-bold transition-colors shadow-lg shadow-cyan-900/50"
                                                >
                                                    Destek Ol / Bilet Al
                                                </button>
                                            </div>
                                        ))
                                    ) : (
                                        <div className="bg-[#111C3A] rounded-3xl border border-white/5 p-6 text-center text-slate-400">
                                            Bu etkinlik için henüz bir paket eklenmemiş.
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    ) : null}
                </div>
            </main>
        </div>
    );
}

export default EventDetail;