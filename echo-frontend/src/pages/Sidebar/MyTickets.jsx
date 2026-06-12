import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar';
import { Ticket, CalendarDays, Receipt } from 'lucide-react';

function MyTickets() {
    const navigate = useNavigate();
    const { user } = useAuth();

    const [tickets, setTickets] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

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
                setError(result.message || 'Biletler yüklenemedi.');
            }
        } catch (err) {
            setError('Sunucu bağlantı hatası.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">
            <Sidebar activeMenu="tickets" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <header className="h-24 bg-[#0B1325]/80 backdrop-blur-xl border-b border-white/5 flex items-center justify-end px-8 z-10 sticky top-0">
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
                    <div className="absolute top-0 left-0 w-[500px] h-[500px] bg-cyan-600/10 rounded-full blur-[100px] pointer-events-none"></div>

                    <div className="mb-8 relative z-10">
                        <h1 className="text-3xl font-extrabold text-white mb-2 tracking-tight">Biletlerim ve Desteklerim 🎟️</h1>
                        <p className="text-slate-400 text-sm">Satın aldığınız tüm paketler ve destek olduğunuz projeler burada yer alır.</p>
                    </div>

                    {isLoading ? (
                        <div className="text-center text-cyan-400 py-10 relative z-10 animate-pulse font-medium">Biletleriniz yükleniyor...</div>
                    ) : error && error !== "Henüz bir biletin bulunmuyor." ? (
                        <div className="text-center text-red-400 py-10 relative z-10 font-medium">{error}</div>
                    ) : tickets.length === 0 || error === "Henüz bir biletin bulunmuyor." ? (
                        <div className="bg-[#111C3A] rounded-3xl border border-white/5 p-12 text-center relative z-10 shadow-xl flex flex-col items-center">
                            <div className="w-20 h-20 bg-cyan-500/10 rounded-full flex items-center justify-center mb-4">
                                <Ticket size={32} className="text-cyan-400" />
                            </div>
                            <h3 className="text-xl font-bold text-white mb-2">Henüz biletiniz yok</h3>
                            <p className="text-slate-400 mb-6 max-w-md">ECHO dünyasındaki projelere destek olarak ilk biletinizi alabilirsiniz.</p>
                            <button
                                onClick={() => navigate('/explore')}
                                className="px-6 py-3 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-bold transition-all shadow-lg shadow-cyan-900/50"
                            >
                                Projeleri Keşfet
                            </button>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 relative z-10">
                            {tickets.map((ticket) => (
                                <div key={ticket.pledgeId} className="flex bg-[#111C3A] rounded-3xl border border-white/5 shadow-xl overflow-hidden hover:border-cyan-500/30 transition-all group">
                                    <div className="flex-1 p-6 flex flex-col justify-between">
                                        <div>
                                            <div className="inline-block px-3 py-1 bg-cyan-500/10 text-cyan-400 rounded-lg text-xs font-bold mb-3">
                                                {ticket.ticketName}
                                            </div>
                                            <h3 className="text-xl font-bold text-white mb-4 line-clamp-2">
                                                {ticket.eventTitle}
                                            </h3>
                                        </div>
                                        <div className="space-y-2">
                                            <div className="flex items-center gap-2 text-sm text-slate-400">
                                                <CalendarDays size={16} className="text-cyan-500" />
                                                <span>{new Date(ticket.pledgeDate).toLocaleString('tr-TR')}</span>
                                            </div>
                                            <div className="flex items-center gap-2 text-sm text-slate-400">
                                                <Receipt size={16} className="text-cyan-500" />
                                                <span>İşlem ID: {ticket.pledgeId.substring(0, 8)}...</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="w-32 bg-gradient-to-b from-[#16244A] to-[#0B1325] border-l border-dashed border-white/20 flex flex-col items-center justify-center p-4 relative">
                                        <div className="absolute -top-3 -left-3 w-6 h-6 bg-[#0B1325] rounded-full border-b border-r border-white/20"></div>
                                        <div className="absolute -bottom-3 -left-3 w-6 h-6 bg-[#0B1325] rounded-full border-t border-r border-white/20"></div>
                                        <span className="text-xs text-slate-400 mb-1">Ödenen Tutar</span>
                                        <span className="text-2xl font-extrabold text-cyan-400">₺{ticket.amountPaid.toLocaleString('tr-TR')}</span>
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

export default MyTickets;