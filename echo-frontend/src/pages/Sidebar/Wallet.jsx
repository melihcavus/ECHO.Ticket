import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { Wallet as WalletIcon, CreditCard, Plus } from 'lucide-react';

// API'mizi içeri aktarıyoruz
import api from '../../services/api';

function Wallet() {
    const navigate = useNavigate();
    const { user, setUser } = useAuth();
    const { t } = useLanguage();

    const [amount, setAmount] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [currentBalance, setCurrentBalance] = useState(0);

    useEffect(() => {
        if (user && user.balance !== undefined) {
            setCurrentBalance(user.balance);
        }
    }, [user]);

    const handleAddBalance = async (e) => {
        e.preventDefault();
        if (!amount || isNaN(amount) || parseFloat(amount) <= 0) {
            alert(t('enterValidAmount', "Lütfen geçerli bir tutar giriniz."));
            return;
        }

        setIsSubmitting(true);
        try {
            // Axios ile POST isteği
            const response = await api.post('/Users/add-balance', {
                amount: parseFloat(amount)
            });

            if (response.data.isSuccess) {
                alert(t('balanceLoadSuccess', "Bakiye başarıyla yüklendi!"));
                const addedAmount = parseFloat(amount);
                const updatedBalance = currentBalance + addedAmount;

                setCurrentBalance(updatedBalance);

                if (setUser) {
                    setUser(prev => ({
                        ...prev,
                        balance: updatedBalance
                    }));
                }

                setAmount('');
            } else {
                alert(`${t('error', 'Hata')}: ${response.data.message || t('balanceLoadFailed', 'Yükleme işlemi başarısız.')}`);
            }
        } catch (err) {
            alert(err.response?.data?.message || t('serverComError', "Sunucuyla iletişim kurulurken bir hata oluştu."));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="wallet" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-cyan-500/20 dark:bg-cyan-600/10 rounded-full blur-[100px] pointer-events-none transition-colors duration-300"></div>

                    <div className="mb-8 relative z-10">
                        <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 tracking-tight">{t('myWalletTitle', 'Cüzdanım')} 💳</h1>
                        <p className="text-slate-600 dark:text-slate-400 text-sm">{t('myWalletDesc', 'ECHO platformundaki bakiyeni yönet ve projelere kesintisiz destek ol.')}</p>
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 relative z-10">
                        <div className="bg-gradient-to-br from-cyan-600 to-blue-800 rounded-3xl p-8 shadow-xl dark:shadow-2xl dark:shadow-cyan-900/40 relative overflow-hidden h-64 flex flex-col justify-between">
                            <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl -mr-10 -mt-10 pointer-events-none"></div>

                            <div className="flex justify-between items-start relative z-10">
                                <div>
                                    <p className="text-cyan-100 text-sm font-bold mb-1">{t('currentBalance', 'Mevcut Bakiye')}</p>
                                    <h2 className="text-4xl font-extrabold text-white tracking-tight">₺{currentBalance.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</h2>
                                </div>
                                <WalletIcon size={32} className="text-white/90" />
                            </div>

                            <div className="relative z-10">
                                <p className="text-cyan-200 text-xs font-mono tracking-widest mb-1">{t('echoWallet', 'ECHO CÜZDAN')}</p>
                                <p className="text-white font-bold text-lg">{user?.firstName} {user?.lastName}</p>
                            </div>
                        </div>

                        <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 p-8 shadow-md dark:shadow-xl transition-colors duration-300">
                            <div className="flex items-center gap-3 mb-6">
                                <div className="w-10 h-10 rounded-xl bg-cyan-50 dark:bg-cyan-500/10 flex items-center justify-center text-cyan-600 dark:text-cyan-400 transition-colors">
                                    <CreditCard size={20} />
                                </div>
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">{t('loadBalanceTitle', 'Bakiye Yükle')}</h3>
                            </div>

                            <form onSubmit={handleAddBalance} className="space-y-6">
                                <div>
                                    <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('amountToLoad', 'Yüklenecek Tutar (₺)')}</label>
                                    <div className="relative">
                                        <input
                                            type="number"
                                            required
                                            min="1"
                                            value={amount}
                                            onChange={(e) => setAmount(e.target.value)}
                                            className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-3 pl-4 pr-12 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none text-lg font-bold transition-colors"
                                            placeholder="0.00"
                                        />
                                        <span className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 dark:text-slate-500 font-bold">₺</span>
                                    </div>
                                </div>

                                <div className="grid grid-cols-3 gap-3">
                                    {[100, 500, 1000].map((val) => (
                                        <button
                                            key={val}
                                            type="button"
                                            onClick={() => setAmount(val.toString())}
                                            className="py-2 rounded-xl border border-slate-200 dark:border-white/5 bg-slate-50 dark:bg-[#0B1325] hover:bg-cyan-50 dark:hover:bg-cyan-600/20 hover:border-cyan-200 dark:hover:border-cyan-500/30 text-slate-700 dark:text-slate-300 transition-all font-bold"
                                        >
                                            +{val} ₺
                                        </button>
                                    ))}
                                </div>

                                <button
                                    type="submit"
                                    disabled={isSubmitting}
                                    className={`w-full py-3.5 rounded-xl font-bold transition-all flex items-center justify-center gap-2 shadow-lg
                                        ${isSubmitting ? 'bg-slate-300 dark:bg-slate-600 text-slate-500 dark:text-slate-400 cursor-not-allowed shadow-none' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                >
                                    {isSubmitting ? t('processing', 'İşleniyor...') : (
                                        <>
                                            <Plus size={20} />
                                            {t('securePaymentBtn', 'Güvenli Ödeme Yap')}
                                        </>
                                    )}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}

export default Wallet;