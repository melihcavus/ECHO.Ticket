import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import { BrainCircuit, AlertTriangle, CheckCircle2, ShieldAlert } from 'lucide-react';

// SİHİRLİ DOKUNUŞ: Dinamik API Base URL
// React projen Vite ile kurulduysa import.meta.env.VITE_API_URL okur.
// Create React App ile kurulduysa process.env.REACT_APP_API_URL okur.
// Hiçbiri yoksa (Fallback) otomatik olarak canlı Render sunucuna bağlanır.
const API_URL = import.meta.env?.VITE_API_URL || process.env?.REACT_APP_API_URL || 'https://echo-ticket.onrender.com';

function AiAnalytics() {
    const { user } = useAuth();
    const navigate = useNavigate();

    const [events, setEvents] = useState([]);
    const [selectedEvent, setSelectedEvent] = useState('');
    const [analyticsData, setAnalyticsData] = useState(null);
    const [isLoading, setIsLoading] = useState(false);

    // 1. GÜVENLİK KONTROLÜ: Sadece Admin girebilir
    useEffect(() => {
        if (user && user.role !== 'Admin') {
            navigate('/dashboard');
        }
    }, [user, navigate]);

    // 2. Etkinlikleri Listeye Çekme (Dropdown için)
    useEffect(() => {
        const fetchEvents = async () => {
            try {
                const token = localStorage.getItem('token');
                // Hardcoded localhost yerine dinamik API_URL kullanılıyor
                const response = await fetch(`${API_URL}/api/events`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                const result = await response.json();
                if (result.isSuccess && result.data.length > 0) {
                    setEvents(result.data);
                    setSelectedEvent(result.data[0].id);
                }
            } catch (error) {
                console.error("Etkinlikler çekilemedi:", error);
            }
        };
        fetchEvents();
    }, []);

    // 3. Seçilen Etkinliğin AI Analizini Çekme
    useEffect(() => {
        if (!selectedEvent) return;

        const fetchAnalytics = async () => {
            setIsLoading(true);
            try {
                const token = localStorage.getItem('token');
                // Hardcoded localhost yerine dinamik API_URL kullanılıyor
                const response = await fetch(`${API_URL}/api/EventReviews/analytics/${selectedEvent}`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                const result = await response.json();

                if (result.isSuccess) {
                    setAnalyticsData(result.data);
                } else {
                    setAnalyticsData(null);
                }
            } catch (error) {
                console.error("Analitik veri çekilemedi:", error);
            } finally {
                setIsLoading(false);
            }
        };

        fetchAnalytics();
    }, [selectedEvent]);

    // Güvenlik: Render edilmeden önce rol onayı
    if (!user || user.role !== 'Admin') return null;

    // Grafikler için veriyi harmanlama
    const gaugeData = analyticsData ? [
        { name: 'Memnuniyet', value: analyticsData.satisfactionScore },
        { name: 'Kalan', value: 100 - analyticsData.satisfactionScore }
    ] : [];

    const distributionData = analyticsData ? [
        { name: 'Pozitif', value: analyticsData.positiveCount, color: '#10B981' },
        { name: 'Negatif', value: analyticsData.negativeCount, color: '#F43F5E' },
        { name: 'Nötr', value: analyticsData.neutralCount, color: '#64748B' }
    ] : [];

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200">
            <Sidebar activeMenu="analytics" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-emerald-500/10 dark:bg-emerald-500/5 rounded-full blur-[100px] pointer-events-none"></div>

                    {/* Başlık ve Seçici */}
                    <div className="flex justify-between items-end mb-8 relative z-10">
                        <div>
                            <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 flex items-center gap-3">
                                <BrainCircuit className="text-emerald-500" size={32} />
                                AI Etkinlik Analitiği
                            </h1>
                            <p className="text-slate-600 dark:text-slate-400 text-sm">BERT-AI tarafından desteklenen kriz yönetim masası.</p>
                        </div>

                        <select
                            value={selectedEvent}
                            onChange={(e) => setSelectedEvent(e.target.value)}
                            className="bg-white dark:bg-[#111C3A] border border-slate-200 dark:border-white/10 rounded-xl py-3 px-4 focus:outline-none focus:border-emerald-500/50 font-bold shadow-sm max-w-xs"
                        >
                            {events.map(ev => (
                                <option key={ev.id} value={ev.id}>{ev.title}</option>
                            ))}
                        </select>
                    </div>

                    {isLoading ? (
                        <div className="flex justify-center py-20 text-emerald-500 font-bold animate-pulse">Analizler Hesaplanıyor...</div>
                    ) : !analyticsData || analyticsData.totalReviews === 0 ? (
                        <div className="bg-white dark:bg-[#111C3A] rounded-3xl p-10 text-center shadow-xl border border-slate-200 dark:border-white/5">
                            <ShieldAlert size={48} className="mx-auto text-slate-400 mb-4" />
                            <h3 className="text-xl font-bold text-slate-700 dark:text-slate-300">Yeterli Veri Yok</h3>
                            <p className="text-slate-500 mt-2">Bu etkinlik için henüz yapay zekanın analiz edebileceği bir yorum bulunmuyor.</p>
                        </div>
                    ) : (
                        <>
                            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8 relative z-10">
                                {/* Memnuniyet Grafiği */}
                                <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-8 shadow-xl flex flex-col items-center justify-center">
                                    <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4">Genel Memnuniyet Skoru</h3>
                                    <div className="w-48 h-48 relative">
                                        <ResponsiveContainer width="100%" height="100%">
                                            <PieChart>
                                                <Pie
                                                    data={gaugeData}
                                                    cx="50%" cy="50%" innerRadius={60} outerRadius={80} startAngle={180} endAngle={0} dataKey="value" stroke="none"
                                                >
                                                    <Cell fill="#10B981" />
                                                    <Cell fill="#1E293B" />
                                                </Pie>
                                            </PieChart>
                                        </ResponsiveContainer>
                                        <div className="absolute inset-0 flex flex-col items-center justify-center -mt-8">
                                            <span className="text-4xl font-extrabold text-emerald-500">%{analyticsData.satisfactionScore}</span>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2 mt-4 text-xs font-bold text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-500/10 px-3 py-1.5 rounded-full">
                                        <CheckCircle2 size={14} /> Toplam {analyticsData.totalReviews} Yorum Analiz Edildi
                                    </div>
                                </div>

                                {/* Dağılım Grafiği */}
                                <div className="lg:col-span-2 bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-8 shadow-xl">
                                    <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-6">Yorum Duygu Dağılımı</h3>
                                    <div className="h-48">
                                        <ResponsiveContainer width="100%" height="100%">
                                            <BarChart data={distributionData} layout="vertical" margin={{ top: 0, right: 30, left: 20, bottom: 0 }}>
                                                <XAxis type="number" hide />
                                                <YAxis dataKey="name" type="category" axisLine={false} tickLine={false} tick={{fill: '#64748B', fontWeight: 'bold'}} />
                                                <Tooltip cursor={{fill: 'transparent'}} contentStyle={{backgroundColor: '#0B1325', borderRadius: '12px', border: 'none', color: '#fff'}} />
                                                <Bar dataKey="value" radius={[0, 8, 8, 0]} barSize={24}>
                                                    {distributionData.map((entry, index) => (
                                                        <Cell key={`cell-${index}`} fill={entry.color} />
                                                    ))}
                                                </Bar>
                                            </BarChart>
                                        </ResponsiveContainer>
                                    </div>
                                </div>
                            </div>

                            {/* Kritik Yorumlar */}
                            {analyticsData.criticalReviews && analyticsData.criticalReviews.length > 0 && (
                                <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 shadow-xl overflow-hidden">
                                    <div className="p-6 border-b border-slate-200 dark:border-white/5 flex justify-between items-center bg-rose-50/50 dark:bg-rose-900/10">
                                        <h3 className="text-xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
                                            <AlertTriangle className="text-rose-500" size={24} />
                                            Acil Müdahale Gerektiren Yorumlar
                                        </h3>
                                        <span className="flex h-3 w-3 rounded-full bg-rose-500 animate-pulse"></span>
                                    </div>
                                    <div className="divide-y divide-slate-100 dark:divide-white/5">
                                        {analyticsData.criticalReviews.map((review) => (
                                            <div key={review.id} className="p-6 flex items-start gap-4 hover:bg-slate-50 dark:hover:bg-white/5 transition-colors">
                                                <div className="flex-1">
                                                    <div className="flex justify-between items-center mb-1">
                                                        <h4 className="font-bold text-slate-900 dark:text-white">{review.userFullName}</h4>
                                                    </div>
                                                    <p className="text-slate-600 dark:text-slate-300 mt-2">{review.content}</p>
                                                    <span className="text-xs text-slate-400 mt-2 block">
                                                        {new Date(review.createdAt).toLocaleDateString('tr-TR')}
                                                    </span>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </>
                    )}
                </div>
            </main>
        </div>
    );
}

export default AiAnalytics;