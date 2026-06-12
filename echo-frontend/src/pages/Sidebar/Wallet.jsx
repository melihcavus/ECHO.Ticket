import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Sidebar from '../../components/Sidebar';
import { Wallet as WalletIcon, CreditCard, Plus } from 'lucide-react';

function Wallet() {
    const navigate = useNavigate();
    const { user, setUser } = useAuth();

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
            alert("Lütfen geçerli bir tutar giriniz.");
            return;
        }

        setIsSubmitting(true);
        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5216/api/Users/add-balance', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ amount: parseFloat(amount) })
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                alert("Bakiye başarıyla yüklendi!");
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
                alert(`Hata: ${result.message || 'Yükleme işlemi başarısız.'}`);
            }
        } catch (err) {
            alert("Sunucuyla iletişim kurulurken bir hata oluştu.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-[#0B1325] flex font-sans text-slate-200">
            <Sidebar activeMenu="wallet" />

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
                    <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-cyan-600/10 rounded-full blur-[100px] pointer-events-none"></div>

                    <div className="mb-8 relative z-10">
                        <h1 className="text-3xl font-extrabold text-white mb-2 tracking-tight">Cüzdanım 💳</h1>
                        <p className="text-slate-400 text-sm">ECHO platformundaki bakiyeni yönet ve projelere kesintisiz destek ol.</p>
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 relative z-10">
                        <div className="bg-gradient-to-br from-cyan-600 to-blue-800 rounded-3xl p-8 shadow-2xl shadow-cyan-900/40 relative overflow-hidden h-64 flex flex-col justify-between">
                            <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl -mr-10 -mt-10 pointer-events-none"></div>

                            <div className="flex justify-between items-start relative z-10">
                                <div>
                                    <p className="text-cyan-100 text-sm font-medium mb-1">Mevcut Bakiye</p>
                                    <h2 className="text-4xl font-extrabold text-white tracking-tight">₺{currentBalance.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}</h2>
                                </div>
                                <WalletIcon size={32} className="text-white/80" />
                            </div>

                            <div className="relative z-10">
                                <p className="text-cyan-200 text-xs font-mono tracking-widest mb-1">ECHO CÜZDAN</p>
                                <p className="text-white font-medium text-lg">{user?.firstName} {user?.lastName}</p>
                            </div>
                        </div>

                        <div className="bg-[#111C3A] rounded-3xl border border-white/5 p-8 shadow-xl">
                            <div className="flex items-center gap-3 mb-6">
                                <div className="w-10 h-10 rounded-xl bg-cyan-500/10 flex items-center justify-center text-cyan-400">
                                    <CreditCard size={20} />
                                </div>
                                <h3 className="text-xl font-bold text-white">Bakiye Yükle</h3>
                            </div>

                            <form onSubmit={handleAddBalance} className="space-y-6">
                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-2">Yüklenecek Tutar (₺)</label>
                                    <div className="relative">
                                        <input
                                            type="number"
                                            required
                                            min="1"
                                            value={amount}
                                            onChange={(e) => setAmount(e.target.value)}
                                            className="w-full bg-[#0B1325] border border-white/5 rounded-xl py-3 pl-4 pr-12 text-white focus:border-cyan-500/50 focus:outline-none text-lg font-medium"
                                            placeholder="0.00"
                                        />
                                        <span className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-500 font-bold">₺</span>
                                    </div>
                                </div>

                                <div className="grid grid-cols-3 gap-3">
                                    {[100, 500, 1000].map((val) => (
                                        <button
                                            key={val}
                                            type="button"
                                            onClick={() => setAmount(val.toString())}
                                            className="py-2 rounded-xl border border-white/5 bg-[#0B1325] hover:bg-cyan-600/20 hover:border-cyan-500/30 text-slate-300 transition-all font-medium"
                                        >
                                            +{val} ₺
                                        </button>
                                    ))}
                                </div>

                                <button
                                    type="submit"
                                    disabled={isSubmitting}
                                    className={`w-full py-3.5 rounded-xl font-bold transition-all flex items-center justify-center gap-2
                                        ${isSubmitting ? 'bg-slate-600 text-slate-400 cursor-not-allowed' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-lg shadow-cyan-900/50'}`}
                                >
                                    {isSubmitting ? 'İşleniyor...' : (
                                        <>
                                            <Plus size={20} />
                                            Güvenli Ödeme Yap
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