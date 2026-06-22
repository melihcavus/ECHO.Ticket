import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { Grid3X3, Plus, X } from 'lucide-react';

// Sihirli API kopyamızı içeri aktarıyoruz (Yolu kendi klasör yapına göre ayarlayabilirsin)
import api from '../../services/api';

function Venues() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const { t } = useLanguage();

    const [venues, setVenues] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState({
        name: '',
        rows: '',
        columns: ''
    });

    useEffect(() => {
        if (user?.role !== 'Admin' && user?.role !== 'Organizer') {
            navigate('/dashboard');
        } else {
            fetchVenues();
        }
    }, [user]);

    const fetchVenues = async () => {
        setIsLoading(true);
        try {
            // SADECE TEK SATIR: Axios bizim yerimize token'ı ekler ve doğru URL'e (Canlı/Local) gider
            const response = await api.get('/Venues');

            // Axios veriyi otomatik olarak json'a çevirip .data içine koyar
            if (response.data.isSuccess) {
                setVenues(response.data.data);
            } else {
                setError(response.data.message || t('venuesLoadError', 'Sahneler yüklenemedi.'));
            }
        } catch (err) {
            // Eğer backend'den özel bir hata mesajı gelirse onu yakalarız, yoksa standart mesaj
            setError(err.response?.data?.message || t('serverComError', 'Sunucuyla iletişim kurulurken bir hata oluştu.'));
        } finally {
            setIsLoading(false);
        }
    };

    const handleCreateVenue = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);

        try {
            // SADECE TEK SATIR: JSON.stringify veya Header ayarlarıyla uğraşmak yok!
            const response = await api.post('/Venues', {
                name: formData.name,
                rows: parseInt(formData.rows),
                columns: parseInt(formData.columns)
            });

            if (response.data.isSuccess) {
                alert(t('venueAddedSuccess', 'Sahne başarıyla eklendi!'));
                setIsModalOpen(false);
                setFormData({ name: '', rows: '', columns: '' });
                fetchVenues(); // Listeyi yenile
            } else {
                alert(`${t('error', 'Hata')}: ${response.data.message}`);
            }
        } catch (err) {
            alert(err.response?.data?.message || t('serverComError', 'Sunucuyla iletişim kurulurken bir hata oluştu.'));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="venues" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-purple-500/20 dark:bg-purple-600/10 rounded-full blur-[120px] pointer-events-none transition-colors duration-300"></div>

                    <div className="flex justify-between items-end mb-8 relative z-10">
                        <div>
                            <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 tracking-tight">{t('venuesTitle', 'Sahneler ve Oturma Düzenleri')} 🏟️</h1>
                            <p className="text-slate-600 dark:text-slate-400 text-sm">{t('venuesDesc', 'Platformdaki tüm etkinlik sahnelerini buradan yönetebilirsiniz.')}</p>
                        </div>

                        <button
                            onClick={() => setIsModalOpen(true)}
                            className="flex items-center gap-2 px-5 py-2.5 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-bold transition-all shadow-lg shadow-cyan-900/50"
                        >
                            <Plus size={20} />
                            {t('addNewVenue', 'Yeni Sahne Ekle')}
                        </button>
                    </div>

                    {isLoading ? (
                        <div className="text-center text-cyan-600 dark:text-cyan-400 py-10 relative z-10 animate-pulse font-medium">{t('loadingDetails', 'Yükleniyor...')}</div>
                    ) : error ? (
                        <div className="text-center text-red-500 dark:text-red-400 py-10 relative z-10 font-medium">{error}</div>
                    ) : venues.length === 0 ? (
                        <div className="text-center text-slate-500 dark:text-slate-400 py-10 relative z-10 bg-white dark:bg-[#111C3A]/50 rounded-3xl border border-slate-200 dark:border-white/5 shadow-sm">{t('noPackages', 'Henüz sahne eklenmemiş.')}</div>
                    ) : (
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 relative z-10">
                            {venues.map((venue) => (
                                <div key={venue.id} className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-6 hover:border-cyan-400 dark:hover:border-cyan-500/30 transition-all group shadow-md dark:shadow-xl flex flex-col">
                                    <div className="w-12 h-12 bg-purple-100 dark:bg-purple-500/10 rounded-xl flex items-center justify-center text-purple-600 dark:text-purple-400 mb-4 group-hover:scale-110 transition-transform">
                                        <Grid3X3 size={24} />
                                    </div>
                                    <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-2">{venue.name}</h3>

                                    <div className="grid grid-cols-2 gap-4 mt-4 bg-slate-50 dark:bg-[#0B1325] p-4 rounded-2xl border border-slate-200 dark:border-white/5">
                                        <div>
                                            <span className="text-xs text-slate-500 dark:text-slate-400 block mb-1">{t('rowCount', 'Satır')}</span>
                                            <span className="text-lg font-extrabold text-slate-900 dark:text-white">{venue.rows}</span>
                                        </div>
                                        <div>
                                            <span className="text-xs text-slate-500 dark:text-slate-400 block mb-1">{t('colCount', 'Sütun')}</span>
                                            <span className="text-lg font-extrabold text-slate-900 dark:text-white">{venue.columns}</span>
                                        </div>
                                    </div>
                                    <div className="mt-4 text-sm font-bold text-cyan-600 dark:text-cyan-400 text-center">
                                        {t('totalCapacity', 'Toplam Kapasite')}: {venue.rows * venue.columns} {t('seats', 'Koltuk')}
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
                            <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t('addNewVenue', 'Yeni Sahne Ekle')}</h2>
                            <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-slate-600 dark:hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form onSubmit={handleCreateVenue} className="space-y-4">
                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('venueName', 'Sahne Adı')}</label>
                                    <input
                                        type="text"
                                        required
                                        value={formData.name}
                                        onChange={(e) => setFormData({...formData, name: e.target.value})}
                                        className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                        placeholder={t('venueNamePlaceholder', 'Örn: Harbiye Açık Hava')}
                                    />
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('rowCount', 'Satır Sayısı (Harf)')}</label>
                                        <input
                                            type="number"
                                            required
                                            min="1"
                                            max="26"
                                            value={formData.rows}
                                            onChange={(e) => setFormData({...formData, rows: e.target.value})}
                                            className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                            placeholder="Örn: 10"
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-1">{t('colCount', 'Sütun Sayısı (Rakam)')}</label>
                                        <input
                                            type="number"
                                            required
                                            min="1"
                                            value={formData.columns}
                                            onChange={(e) => setFormData({...formData, columns: e.target.value})}
                                            className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-2.5 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                            placeholder="Örn: 20"
                                        />
                                    </div>
                                </div>
                                <p className="text-xs text-slate-500 dark:text-slate-400 mt-2">
                                    * {t('totalCapacity', 'Toplam Kapasite')}: {(parseInt(formData.rows) || 0) * (parseInt(formData.columns) || 0)} {t('seats', 'Koltuk')}
                                </p>

                                <button
                                    type="submit"
                                    disabled={isSubmitting}
                                    className={`w-full py-3 rounded-xl font-bold transition-all mt-4 shadow-lg
                                        ${isSubmitting ? 'bg-slate-300 dark:bg-slate-600 text-slate-500 dark:text-slate-400 cursor-not-allowed shadow-none' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                >
                                    {isSubmitting ? t('creating', 'Oluşturuluyor...') : t('saveVenue', 'Sahneyi Kaydet')}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Venues;