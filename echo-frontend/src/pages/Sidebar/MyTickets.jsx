import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { Ticket, CalendarDays, Receipt, X } from 'lucide-react';
import { QRCodeSVG } from 'qrcode.react';

function MyTickets() {
    const navigate = useNavigate();
    const { user } = useAuth();

    const { t } = useLanguage();

    const [tickets, setTickets] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    // QR Modal State'i
    const [selectedTicket, setSelectedTicket] = useState(null);

    useEffect(() => {
        if (user?.id) {
            fetchMyTickets();
        } else {
            setIsLoading(false);
        }
    }, [user]);

    const fetchMyTickets = async () => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5216/api/Pledges/user/${user.id}`, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                setTickets(result.data);
            } else {
                setError(result.message || t('ticketsLoadError', 'Biletler yüklenemedi.'));
            }
        } catch (err) {
            setError(t('serverConnError', 'Sunucu bağlantı hatası.'));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="tickets" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 left-0 w-[500px] h-[500px] bg-cyan-500/20 dark:bg-cyan-600/10 rounded-full blur-[100px] pointer-events-none transition-colors duration-300"></div>

                    <div className="mb-8 relative z-10">
                        <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 tracking-tight">{t('myTicketsTitle', 'Biletlerim ve Desteklerim')} 🎟️</h1>
                        <p className="text-slate-600 dark:text-slate-400 text-sm">{t('myTicketsDesc', 'Satın aldığınız tüm paketler ve destek olduğunuz projeler burada yer alır.')}</p>
                    </div>

                    {isLoading ? (
                        <div className="text-center text-cyan-600 dark:text-cyan-400 py-10 relative z-10 animate-pulse font-medium">{t('loadingTickets', 'Biletleriniz yükleniyor...')}</div>
                    ) : error && error !== "Henüz bir biletin bulunmuyor." ? (
                        <div className="text-center text-red-500 dark:text-red-400 py-10 relative z-10 font-medium">{error}</div>
                    ) : tickets.length === 0 || error === "Henüz bir biletin bulunmuyor." ? (
                        <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-12 text-center relative z-10 shadow-md dark:shadow-xl flex flex-col items-center transition-colors duration-300">
                            <div className="w-20 h-20 bg-cyan-50 dark:bg-cyan-500/10 rounded-full flex items-center justify-center mb-4 transition-colors">
                                <Ticket size={32} className="text-cyan-600 dark:text-cyan-400" />
                            </div>
                            <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-2">{t('noTicketsTitle', 'Henüz biletiniz yok')}</h3>
                            <p className="text-slate-500 dark:text-slate-400 mb-6 max-w-md">{t('noTicketsDesc', 'ECHO dünyasındaki projelere destek olarak ilk biletinizi alabilirsiniz.')}</p>
                            <button
                                onClick={() => navigate('/explore')}
                                className="px-6 py-3 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-bold transition-all shadow-lg shadow-cyan-900/50 dark:shadow-cyan-900/50"
                            >
                                {t('exploreProjectsBtn', 'Projeleri Keşfet')}
                            </button>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 relative z-10">
                            {tickets.map((ticket) => (
                                <div
                                    key={ticket.pledgeId}
                                    onClick={() => setSelectedTicket(ticket)}
                                    className="flex bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 shadow-md dark:shadow-xl overflow-hidden hover:border-cyan-400 dark:hover:border-cyan-500/30 transition-all group cursor-pointer hover:scale-[1.02]"
                                >
                                    <div className="flex-1 p-6 flex flex-col justify-between">
                                        <div>
                                            <div className="inline-block px-3 py-1 bg-cyan-100 dark:bg-cyan-500/10 text-cyan-700 dark:text-cyan-400 rounded-lg text-xs font-bold mb-3">
                                                {ticket.ticketName}
                                            </div>
                                            <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-4 line-clamp-2">
                                                {ticket.eventTitle}
                                            </h3>
                                        </div>
                                        <div className="space-y-2">
                                            <div className="flex items-center gap-2 text-sm text-slate-500 dark:text-slate-400">
                                                <CalendarDays size={16} className="text-cyan-600 dark:text-cyan-500" />
                                                <span>{new Date(ticket.pledgeDate).toLocaleString('tr-TR')}</span>
                                            </div>
                                            <div className="flex items-center gap-2 text-sm text-slate-500 dark:text-slate-400">
                                                <Receipt size={16} className="text-cyan-600 dark:text-cyan-500" />
                                                <span>{t('transactionId', 'İşlem ID')}: {ticket.pledgeId.substring(0, 8)}...</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="w-32 bg-gradient-to-b from-slate-50 to-slate-100 dark:from-[#16244A] dark:to-[#0B1325] border-l border-dashed border-slate-200 dark:border-white/20 flex flex-col items-center justify-center p-4 relative transition-colors duration-300">
                                        <div className="absolute -top-3 -left-3 w-6 h-6 bg-slate-50 dark:bg-[#0B1325] rounded-full border-b border-r border-slate-200 dark:border-white/20 transition-colors"></div>
                                        <div className="absolute -bottom-3 -left-3 w-6 h-6 bg-slate-50 dark:bg-[#0B1325] rounded-full border-t border-r border-slate-200 dark:border-white/20 transition-colors"></div>
                                        <span className="text-xs text-slate-500 dark:text-slate-400 mb-1">{t('paidAmount', 'Ödenen Tutar')}</span>
                                        <span className="text-2xl font-extrabold text-cyan-600 dark:text-cyan-400">₺{ticket.amountPaid.toLocaleString('tr-TR')}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </main>

            {/* QR KOD MODALI */}
            {selectedTicket && (
                <div className="fixed inset-0 bg-slate-900/60 dark:bg-black/80 backdrop-blur-sm flex justify-center items-center z-50 p-4 transition-colors duration-300">
                    <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/10 w-full max-w-sm shadow-2xl flex flex-col items-center p-8 relative transition-colors duration-300">

                        <button
                            onClick={() => setSelectedTicket(null)}
                            className="absolute top-5 right-5 text-slate-400 hover:text-slate-600 dark:hover:text-white transition-colors"
                        >
                            <X size={24} />
                        </button>

                        <h3 className="text-xl font-extrabold text-slate-900 dark:text-white mb-1 text-center">
                            {selectedTicket.eventTitle}
                        </h3>
                        <p className="text-sm font-bold text-cyan-600 dark:text-cyan-400 mb-8 text-center">
                            {selectedTicket.ticketName}
                            {selectedTicket.rowLabel && ` • Koltuk: ${selectedTicket.rowLabel}${selectedTicket.columnNumber}`}
                        </p>

                        <div className="bg-white p-4 rounded-2xl shadow-inner border border-slate-200">
                            <QRCodeSVG
                                value={selectedTicket.pledgeId}
                                size={200}
                                bgColor={"#ffffff"}
                                fgColor={"#0B1325"}
                                level={"H"}
                                includeMargin={false}
                            />
                        </div>

                        <p className="mt-6 text-xs font-mono text-slate-500 dark:text-slate-400 break-all text-center">
                            ECHO-{selectedTicket.pledgeId?.substring(0, 12).toUpperCase()}
                        </p>
                        <p className="mt-2 text-xs text-slate-600 dark:text-slate-500 font-medium">
                            Giriş yaparken bu kodu görevliye okutunuz.
                        </p>
                    </div>
                </div>
            )}
        </div>
    );
}

export default MyTickets;