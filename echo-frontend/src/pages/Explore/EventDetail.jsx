import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar';
import { ArrowLeft, CalendarDays, MapPin, User, CheckCircle2, Plus, X } from 'lucide-react';

function EventDetail() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user, setUser } = useAuth();

    const [eventData, setEventData] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isPurchasing, setIsPurchasing] = useState(false);

    const [isTicketModalOpen, setIsTicketModalOpen] = useState(false);
    const [isTicketSubmitting, setIsTicketSubmitting] = useState(false);
    const [ticketFormData, setTicketFormData] = useState({
        name: '',
        description: '',
        price: '',
        capacity: ''
    });

    const fetchEventDetail = async () => {
        setIsLoading(true);
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5216/api/events/${id}`, {
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
            setError("Etkinlik yüklenirken bir sorun oluştu.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchEventDetail();
    }, [id]);

    const handleBuyTicket = async (ticketId) => {
        if (!user || !user.id) {
            alert("Satın alma işlemi için giriş yapmalısınız.");
            return;
        }

        const selectedTicket = eventData.tickets.find(t => t.ticketId === ticketId);
        if (selectedTicket && (user.balance || 0) < selectedTicket.price) {
            alert(`Bakiyeniz yetersiz!\nBilet Fiyatı: ₺${selectedTicket.price}\nMevcut Bakiyeniz: ₺${user.balance || 0}\n\nLütfen işleme devam etmek için cüzdanınıza bakiye yükleyin.`);
            navigate('/wallet');
            return;
        }

        setIsPurchasing(true);

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5216/api/Tickets/purchase', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    userId: user.id,
                    eventId: id,
                    ticketId: ticketId
                })
            });

            const data = await response.json();

            if (response.status === 202 || data.isSuccess) {
                if (setUser) {
                    setUser(prev => ({
                        ...prev,
                        balance: prev.balance - selectedTicket.price
                    }));
                }

                alert("Satın alma işleminiz sıraya alındı! Stok düştüğünde sayfaya yansıyacaktır.");

                setTimeout(() => {
                    fetchEventDetail();
                }, 2000);
            } else {
                alert(`İşlem başarısız: ${data.message || 'Bilinmeyen bir hata oluştu.'}`);
            }
        } catch (error) {
            alert("İşlem sırasında sunucu ile bağlantı kurulamadı.");
        } finally {
            setIsPurchasing(false);
        }
    };

    const handleCreateTicket = async (e) => {
        e.preventDefault();
        setIsTicketSubmitting(true);

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5216/api/Tickets', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    name: ticketFormData.name,
                    description: ticketFormData.description,
                    price: parseFloat(ticketFormData.price),
                    capacity: parseInt(ticketFormData.capacity),
                    eventId: id
                })
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                alert("Paket başarıyla eklendi!");
                setIsTicketModalOpen(false);
                setTicketFormData({ name: '', description: '', price: '', capacity: '' });
                fetchEventDetail();
            } else {
                alert(`Hata: ${result.message || 'Oluşturulamadı'}`);
            }
        } catch (err) {
            alert("Sunucuyla iletişim kurulurken bir hata oluştu.");
        } finally {
            setIsTicketSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">
            <Sidebar activeMenu="explore" />
            <main className="flex-1 flex flex-col h-screen overflow-hidden relative">
                <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none z-0"></div>

                <header className="h-24 bg-[#0B1325]/80 backdrop-blur-xl border-b border-white/5 flex items-center justify-between px-8 z-10 sticky top-0">
                    <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-slate-400 hover:text-white transition-colors">
                        <ArrowLeft size={20} />
                        <span className="font-medium">Geri Dön</span>
                    </button>
                    <div className="flex items-center gap-4 cursor-pointer hover:opacity-80 transition-opacity">
                        <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-lg shadow-cyan-900/50">
                            {user?.firstName?.charAt(0) || 'U'}{user?.lastName?.charAt(0) || ''}
                        </div>
                    </div>
                </header>

                <div className="flex-1 overflow-y-auto p-8 relative z-10">
                    {isLoading && !eventData ? (
                        <div className="text-center text-cyan-400 py-10 animate-pulse font-medium">Detaylar yükleniyor...</div>
                    ) : error ? (
                        <div className="text-center text-red-400 py-10 font-medium">{error}</div>
                    ) : eventData ? (
                        <div className="max-w-5xl mx-auto space-y-8">
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
                                <div className="lg:col-span-2 space-y-6">
                                    <div className="bg-[#111C3A] rounded-3xl border border-white/5 p-8 shadow-xl">
                                        <h2 className="text-xl font-bold text-white mb-4">Proje Hakkında</h2>
                                        <p className="text-slate-300 leading-relaxed whitespace-pre-wrap">
                                            {eventData.description || "Bu proje için henüz detaylı bir açıklama girilmemiştir."}
                                        </p>
                                    </div>
                                </div>

                                <div className="space-y-6">
                                    <div className="flex items-center justify-between px-2">
                                        <h2 className="text-xl font-bold text-white">Destek Paketleri</h2>
                                        {(user?.role === 'Admin' || user?.role === 'Organizer') && (
                                            <button
                                                onClick={() => setIsTicketModalOpen(true)}
                                                className="flex items-center gap-1 text-sm bg-cyan-600/20 text-cyan-400 hover:bg-cyan-600/40 px-3 py-1.5 rounded-lg transition-colors font-medium"
                                            >
                                                <Plus size={16} /> Paket Ekle
                                            </button>
                                        )}
                                    </div>

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
                                                    disabled={isPurchasing || ticket.remainingCapacity <= 0}
                                                    className={`w-full py-3 rounded-xl font-bold transition-colors shadow-lg shadow-cyan-900/50 
                                                        ${isPurchasing || ticket.remainingCapacity <= 0
                                                        ? 'bg-slate-600 cursor-not-allowed text-slate-300'
                                                        : 'bg-cyan-600 hover:bg-cyan-500 text-white'}`}
                                                >
                                                    {isPurchasing ? 'İşleniyor...' : (ticket.remainingCapacity > 0 ? 'Destek Ol / Bilet Al' : 'Tükendi')}
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

            {isTicketModalOpen && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex justify-center items-center z-50 p-4">
                    <div className="bg-[#111C3A] rounded-3xl border border-white/10 w-full max-w-lg shadow-2xl flex flex-col max-h-[90vh]">
                        <div className="flex justify-between items-center p-6 border-b border-white/10">
                            <h2 className="text-xl font-bold text-white">Yeni Paket Oluştur</h2>
                            <button onClick={() => setIsTicketModalOpen(false)} className="text-slate-400 hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form onSubmit={handleCreateTicket} className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-1">Paket Adı</label>
                                    <input
                                        type="text"
                                        required
                                        value={ticketFormData.name}
                                        onChange={(e) => setTicketFormData({...ticketFormData, name: e.target.value})}
                                        className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none"
                                        placeholder="Örn: VIP Katılım Paketi"
                                    />
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-slate-300 mb-1">Fiyat (₺)</label>
                                        <input
                                            type="number"
                                            required
                                            min="0"
                                            step="0.01"
                                            value={ticketFormData.price}
                                            onChange={(e) => setTicketFormData({...ticketFormData, price: e.target.value})}
                                            className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none"
                                            placeholder="1500"
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-slate-300 mb-1">Kapasite (Stok)</label>
                                        <input
                                            type="number"
                                            required
                                            min="1"
                                            value={ticketFormData.capacity}
                                            onChange={(e) => setTicketFormData({...ticketFormData, capacity: e.target.value})}
                                            className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none"
                                            placeholder="100"
                                        />
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-1">Açıklama</label>
                                    <textarea
                                        required
                                        rows="3"
                                        value={ticketFormData.description}
                                        onChange={(e) => setTicketFormData({...ticketFormData, description: e.target.value})}
                                        className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-2.5 px-4 text-white focus:border-cyan-500/50 focus:outline-none resize-none"
                                        placeholder="Paketin avantajlarını yazın..."
                                    ></textarea>
                                </div>

                                <button
                                    type="submit"
                                    disabled={isTicketSubmitting}
                                    className={`w-full py-3 rounded-xl font-bold transition-all mt-4
                                        ${isTicketSubmitting ? 'bg-slate-600 text-slate-400 cursor-not-allowed' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-lg shadow-cyan-900/50'}`}
                                >
                                    {isTicketSubmitting ? 'Oluşturuluyor...' : 'Paketi Kaydet'}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default EventDetail;