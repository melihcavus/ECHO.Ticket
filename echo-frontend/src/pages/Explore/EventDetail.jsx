import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { CalendarDays, MapPin, User, CheckCircle2, Plus, X } from 'lucide-react';
import { HubConnectionBuilder } from '@microsoft/signalr';

function EventDetail() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user, setUser } = useAuth();
    const { t } = useLanguage();

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

    const [takenSeats, setTakenSeats] = useState([]);
    const [selectedSeat, setSelectedSeat] = useState(null);

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

            if (!response.ok) throw new Error(t('fetchError', 'Etkinlik detayları çekilemedi'));

            const result = await response.json();

            if (result.isSuccess) {
                setEventData(result.data);
            } else {
                setError(result.message);
            }
        } catch (err) {
            setError(t('loadError', 'Etkinlik yüklenirken bir sorun oluştu.'));
        } finally {
            setIsLoading(false);
        }
    };

    const fetchTakenSeats = async () => {
        try {
            const response = await fetch(`http://localhost:5216/api/events/${id}/taken-seats`);
            const result = await response.json();
            if (response.ok && result.isSuccess) {
                setTakenSeats(result.data);
            }
        } catch (err) {
            console.error("Dolu koltuklar çekilemedi:", err);
        }
    };

    useEffect(() => {
        fetchEventDetail();
        fetchTakenSeats();

        const connection = new HubConnectionBuilder()
            .withUrl("http://localhost:5216/ticketHub")
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(() => console.log("SignalR Bağlandı!"))
            .catch(err => console.error("SignalR Bağlantı Hatası:", err));

        connection.on("SeatSold", (eventId, seatLabel) => {
            if (eventId === id) {
                setTakenSeats(prev => [...new Set([...prev, seatLabel])]);
            }
        });

        return () => {
            connection.stop();
        };
    }, [id]);

    const handleBuyTicket = async (ticketId) => {
        if (!user || !user.id) {
            alert(t('loginRequired', 'Satın alma işlemi için giriş yapmalısınız.'));
            return;
        }

        if (eventData.venueId && !selectedSeat) {
            alert(t('selectSeatAlert', 'Lütfen önce haritadan bir koltuk seçin!'));
            return;
        }

        const selectedTicket = eventData.tickets.find(t => t.ticketId === ticketId);
        if (selectedTicket && (user.balance || 0) < selectedTicket.price) {
            alert(`${t('insufficientBalance', 'Bakiyeniz yetersiz!')}\n${t('ticketPrice', 'Bilet Fiyatı')}: ₺${selectedTicket.price}\n${t('currentBalance', 'Mevcut Bakiyeniz')}: ₺${user.balance || 0}\n\n${t('balancePrompt', 'Lütfen işleme devam etmek için cüzdanınıza bakiye yükleyin.')}`);
            navigate('/wallet');
            return;
        }

        setIsPurchasing(true);

        let rowLabel = null;
        let colNumber = null;
        if (selectedSeat) {
            const parts = selectedSeat.split('-');
            rowLabel = parts[0];
            colNumber = parseInt(parts[1], 10);
        }

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
                    ticketId: ticketId,
                    rowLabel: rowLabel,
                    columnNumber: colNumber
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

                alert(t('purchaseSuccess', 'Satın alma işleminiz sıraya alındı! Stok düştüğünde sayfaya yansıyacaktır.'));
                setSelectedSeat(null);

                setTimeout(() => {
                    fetchEventDetail();
                }, 2000);
            } else {
                alert(`${t('operationFailed', 'İşlem başarısız')}: ${data.message || t('unknownError', 'Bilinmeyen bir hata oluştu.')}`);
            }
        } catch (error) {
            alert(t('serverError', 'İşlem sırasında sunucu ile bağlantı kurulamadı.'));
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
                alert(t('packageAdded', 'Paket başarıyla eklendi!'));
                setIsTicketModalOpen(false);
                setTicketFormData({ name: '', description: '', price: '', capacity: '' });
                fetchEventDetail();
            } else {
                alert(`${t('error', 'Hata')}: ${result.message || t('createFailed', 'Oluşturulamadı')}`);
            }
        } catch (err) {
            alert(t('serverComError', 'Sunucuyla iletişim kurulurken bir hata oluştu.'));
        } finally {
            setIsTicketSubmitting(false);
        }
    };

    const renderSeatMap = () => {
        if (!eventData?.venueRows || !eventData?.venueColumns) return null;

        const rows = Array.from({ length: eventData.venueRows }, (_, i) => String.fromCharCode(65 + i));
        const cols = Array.from({ length: eventData.venueColumns }, (_, i) => i + 1);

        const totalCapacity = eventData.venueRows * eventData.venueColumns;
        const fillPercentage = totalCapacity === 0 ? 0 : (takenSeats.length / totalCapacity) * 100;

        return (
            <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-8 shadow-xl mt-8">
                <div className="flex justify-between items-end mb-6">
                    <div>
                        <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t('seatSelection', 'Koltuk Seçimi')}</h2>
                        <p className="text-sm text-slate-500 dark:text-slate-400">{eventData.venueName}</p>
                    </div>
                    <div className="text-right">
                        <span className="text-sm font-bold text-slate-900 dark:text-white">{takenSeats.length} / {totalCapacity} {t('seatsSold', 'Koltuk Dolu')}</span>
                    </div>
                </div>

                <div className="w-full bg-slate-100 dark:bg-[#0B1325] rounded-full h-3 overflow-hidden border border-slate-200 dark:border-white/5 mb-8">
                    <div
                        className="bg-gradient-to-r from-cyan-500 to-blue-500 h-full rounded-full transition-all duration-1000 ease-out"
                        style={{ width: `${fillPercentage}%` }}
                    ></div>
                </div>

                <div className="w-full max-w-4xl mx-auto overflow-x-auto pb-4">
                    <div className="min-w-max flex flex-col gap-2 items-center">
                        <div className="w-full max-w-2xl h-8 bg-slate-200 dark:bg-[#16244A] rounded-t-[100px] mb-8 flex items-center justify-center border-b-4 border-slate-300 dark:border-white/10">
                            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 tracking-[0.5em]">{t('stage', 'SAHNE')}</span>
                        </div>

                        {rows.map(rowLabel => (
                            <div key={rowLabel} className="flex gap-2 items-center">
                                <div className="w-6 text-center text-xs font-bold text-slate-500 dark:text-slate-400">{rowLabel}</div>
                                {cols.map(colNumber => {
                                    const seatId = `${rowLabel}-${colNumber}`;
                                    const isTaken = takenSeats.includes(seatId);
                                    const isSelected = selectedSeat === seatId;

                                    return (
                                        <button
                                            key={seatId}
                                            disabled={isTaken}
                                            onClick={() => setSelectedSeat(seatId)}
                                            className={`w-8 h-8 rounded-t-lg rounded-b-sm text-[10px] font-bold transition-all
                                                ${isTaken ? 'bg-red-500 dark:bg-red-500/80 text-white cursor-not-allowed border-b-4 border-red-700' :
                                                isSelected ? 'bg-cyan-500 text-white border-b-4 border-cyan-700 -translate-y-1' :
                                                    'bg-slate-200 dark:bg-[#16244A] text-slate-600 dark:text-slate-400 hover:bg-cyan-200 dark:hover:bg-cyan-900/50 border-b-4 border-slate-300 dark:border-[#0B1325]'}`}
                                            title={isTaken ? t('seatTaken', 'Dolu') : seatId}
                                        >
                                            {colNumber}
                                        </button>
                                    );
                                })}
                                <div className="w-6 text-center text-xs font-bold text-slate-500 dark:text-slate-400">{rowLabel}</div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="flex justify-center gap-6 mt-8 pt-6 border-t border-slate-200 dark:border-white/5">
                    <div className="flex items-center gap-2">
                        <div className="w-4 h-4 rounded-t-sm bg-slate-200 dark:bg-[#16244A]"></div>
                        <span className="text-sm text-slate-600 dark:text-slate-400">{t('availableSeat', 'Boş')}</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="w-4 h-4 rounded-t-sm bg-red-500 dark:bg-red-500/80"></div>
                        <span className="text-sm text-slate-600 dark:text-slate-400">{t('takenSeat', 'Dolu')}</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="w-4 h-4 rounded-t-sm bg-cyan-500"></div>
                        <span className="text-sm text-slate-600 dark:text-slate-400">{t('selectedSeat', 'Seçili')}</span>
                    </div>
                </div>
            </div>
        );
    };

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="explore" />
            <main className="flex-1 flex flex-col h-screen overflow-hidden relative">
                <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-cyan-500/20 dark:bg-cyan-600/10 rounded-full blur-[120px] pointer-events-none z-0 transition-colors duration-300"></div>

                <Header showBack={true} />

                <div className="flex-1 overflow-y-auto p-8 relative z-10">
                    {isLoading && !eventData ? (
                        <div className="text-center text-cyan-600 dark:text-cyan-400 py-10 animate-pulse font-medium">{t('loadingDetails', 'Detaylar yükleniyor...')}</div>
                    ) : error ? (
                        <div className="text-center text-red-500 dark:text-red-400 py-10 font-medium">{error}</div>
                    ) : eventData ? (
                        <div className="max-w-5xl mx-auto space-y-8">
                            <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-8 shadow-xl transition-colors duration-300">
                                <div className="inline-block px-3 py-1 bg-cyan-100 dark:bg-cyan-500/10 text-cyan-700 dark:text-cyan-400 rounded-full text-sm font-bold mb-4">
                                    {t('activeCampaign', 'Aktif Kampanya')}
                                </div>
                                <h1 className="text-4xl font-extrabold text-slate-900 dark:text-white mb-6 tracking-tight">{eventData.title}</h1>

                                <div className="flex flex-wrap gap-6 text-slate-600 dark:text-slate-400">
                                    <div className="flex items-center gap-2">
                                        <CalendarDays size={18} className="text-cyan-600 dark:text-cyan-500" />
                                        <span>{new Date(eventData.eventDate).toLocaleDateString('tr-TR')}</span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <MapPin size={18} className="text-cyan-600 dark:text-cyan-500" />
                                        <span>{eventData.location}</span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <User size={18} className="text-cyan-600 dark:text-cyan-500" />
                                        <span>{eventData.organizerName}</span>
                                    </div>
                                </div>
                            </div>

                            {renderSeatMap()}

                            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                                <div className="lg:col-span-2 space-y-6">
                                    <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-8 shadow-xl transition-colors duration-300">
                                        <h2 className="text-xl font-bold text-slate-900 dark:text-white mb-4">{t('aboutProject', 'Proje Hakkında')}</h2>
                                        <p className="text-slate-600 dark:text-slate-300 leading-relaxed whitespace-pre-wrap">
                                            {eventData.description || t('noDescription', 'Bu proje için henüz detaylı bir açıklama girilmemiştir.')}
                                        </p>
                                    </div>
                                </div>

                                <div className="space-y-6">
                                    <div className="flex items-center justify-between px-2">
                                        <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t('supportPackages', 'Destek Paketleri')}</h2>
                                        {(user?.role === 'Admin' || user?.role === 'Organizer') && (
                                            <button
                                                onClick={() => setIsTicketModalOpen(true)}
                                                className="flex items-center gap-1 text-sm bg-cyan-100 dark:bg-cyan-600/20 text-cyan-700 dark:text-cyan-400 hover:bg-cyan-200 dark:hover:bg-cyan-600/40 px-3 py-1.5 rounded-lg transition-colors font-bold"
                                            >
                                                <Plus size={16} /> {t('addPackage', 'Paket Ekle')}
                                            </button>
                                        )}
                                    </div>

                                    {eventData.tickets && eventData.tickets.length > 0 ? (
                                        eventData.tickets.map(ticket => (
                                            <div key={ticket.ticketId} className="bg-gradient-to-b from-white to-slate-50 dark:from-[#16244A] dark:to-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-6 shadow-xl hover:border-cyan-400 dark:hover:border-cyan-500/30 transition-all group">
                                                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-2">{ticket.name}</h3>
                                                <div className="text-2xl font-extrabold text-cyan-600 dark:text-cyan-400 mb-4">
                                                    ₺{ticket.price.toLocaleString('tr-TR')}
                                                </div>
                                                <p className="text-sm text-slate-600 dark:text-slate-400 mb-6 min-h-[40px]">
                                                    {ticket.description}
                                                </p>

                                                <div className="flex items-center justify-between text-xs text-slate-500 mb-4">
                                                    <span className="flex items-center gap-1"><CheckCircle2 size={14} /> {t('stock', 'Stok')}: {ticket.remainingCapacity}</span>
                                                </div>

                                                <button
                                                    onClick={() => handleBuyTicket(ticket.ticketId)}
                                                    disabled={isPurchasing || ticket.remainingCapacity <= 0}
                                                    className={`w-full py-3 rounded-xl font-bold transition-colors shadow-lg 
                                                        ${isPurchasing || ticket.remainingCapacity <= 0
                                                        ? 'bg-slate-300 dark:bg-slate-600 cursor-not-allowed text-slate-500 dark:text-slate-300 shadow-none'
                                                        : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                                >
                                                    {isPurchasing ? t('processing', 'İşleniyor...') : (ticket.remainingCapacity > 0 ? t('buyTicket', 'Destek Ol / Bilet Al') : t('soldOut', 'Tükendi'))}
                                                </button>
                                            </div>
                                        ))
                                    ) : (
                                        <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-6 text-center text-slate-500 dark:text-slate-400 transition-colors duration-300">
                                            {t('noPackages', 'Bu etkinlik için henüz bir paket eklenmemiş.')}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    ) : null}
                </div>
            </main>

            {isTicketModalOpen && (
                <div className="fixed inset-0 bg-slate-900/40 dark:bg-black/60 backdrop-blur-sm flex justify-center items-center z-50 p-4 transition-colors duration-300">
                    <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/10 w-full max-w-lg shadow-2xl flex flex-col max-h-[90vh] transition-colors duration-300">
                        <div className="flex justify-between items-center p-6 border-b border-slate-200 dark:border-white/10">
                            <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t('createPackage', 'Yeni Paket Oluştur')}</h2>
                            <button onClick={() => setIsTicketModalOpen(false)} className="text-slate-400 hover:text-slate-600 dark:hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form onSubmit={handleCreateTicket} className="space-y-4">
                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('packageName', 'Paket Adı')}</label>
                                    <input
                                        type="text"
                                        required
                                        value={ticketFormData.name}
                                        onChange={(e) => setTicketFormData({...ticketFormData, name: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                        placeholder={t('packagePlaceholder', 'Örn: VIP Katılım Paketi')}
                                    />
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('price', 'Fiyat (₺)')}</label>
                                        <input
                                            type="number"
                                            required
                                            min="0"
                                            step="0.01"
                                            value={ticketFormData.price}
                                            onChange={(e) => setTicketFormData({...ticketFormData, price: e.target.value})}
                                            className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                            placeholder="1500"
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('capacity', 'Kapasite (Stok)')}</label>
                                        <input
                                            type="number"
                                            required
                                            min="1"
                                            value={ticketFormData.capacity}
                                            onChange={(e) => setTicketFormData({...ticketFormData, capacity: e.target.value})}
                                            className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                            placeholder="100"
                                        />
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('description', 'Açıklama')}</label>
                                    <textarea
                                        required
                                        rows="3"
                                        value={ticketFormData.description}
                                        onChange={(e) => setTicketFormData({...ticketFormData, description: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none resize-none transition-colors"
                                        placeholder={t('descPlaceholder', 'Paketin avantajlarını yazın...')}
                                    ></textarea>
                                </div>

                                <button
                                    type="submit"
                                    disabled={isTicketSubmitting}
                                    className={`w-full py-3 rounded-xl font-bold transition-all mt-4 shadow-lg
                                        ${isTicketSubmitting ? 'bg-slate-300 dark:bg-slate-600 text-slate-500 dark:text-slate-400 cursor-not-allowed shadow-none' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                >
                                    {isTicketSubmitting ? t('creating', 'Oluşturuluyor...') : t('savePackage', 'Paketi Kaydet')}
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