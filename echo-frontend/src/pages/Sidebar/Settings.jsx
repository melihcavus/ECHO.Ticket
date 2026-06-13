import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import Sidebar from '../../components/Sidebar';
import Header from '../../components/Header';
import { User, Shield, Save, KeyRound } from 'lucide-react';

function Settings() {
    const navigate = useNavigate();
    const { user, setUser } = useAuth();
    const { t } = useLanguage();

    const [activeTab, setActiveTab] = useState('profile');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [profileData, setProfileData] = useState({
        firstName: '',
        lastName: ''
    });

    const [securityData, setSecurityData] = useState({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
    });

    useEffect(() => {
        if (user) {
            setProfileData({
                firstName: user.firstName || '',
                lastName: user.lastName || ''
            });
        }
    }, [user]);

    const handleProfileUpdate = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5216/api/Users/update-profile', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    firstName: profileData.firstName,
                    lastName: profileData.lastName
                })
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                alert(t('profileUpdateSuccess', "Profil bilgileriniz başarıyla güncellendi!"));
                if (setUser) {
                    setUser(prev => ({
                        ...prev,
                        firstName: profileData.firstName,
                        lastName: profileData.lastName
                    }));
                }
            } else {
                alert(`${t('error', 'Hata')}: ${result.message || t('profileUpdateError', 'Profil güncellenemedi.')}`);
            }
        } catch (err) {
            alert(t('serverComError', "Sunucuyla iletişim kurulurken bir hata oluştu."));
        } finally {
            setIsSubmitting(false);
        }
    };

    const handlePasswordChange = async (e) => {
        e.preventDefault();
        if (securityData.newPassword !== securityData.confirmPassword) {
            alert(t('passwordsDoNotMatch', "Yeni şifreler birbiriyle eşleşmiyor!"));
            return;
        }

        setIsSubmitting(true);

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5216/api/Users/change-password', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    currentPassword: securityData.currentPassword,
                    newPassword: securityData.newPassword,
                    confirmPassword: securityData.confirmPassword
                })
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                alert(t('passwordUpdateSuccess', "Şifreniz başarıyla güncellendi!"));
                setSecurityData({ currentPassword: '', newPassword: '', confirmPassword: '' });
            } else {
                alert(`${t('error', 'Hata')}: ${result.message || t('passwordUpdateError', 'Şifre güncellenemedi.')}`);
            }
        } catch (err) {
            alert(t('serverComError', "Sunucuyla iletişim kurulurken bir hata oluştu."));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-[#0B1325] flex font-sans text-slate-900 dark:text-slate-200 transition-colors duration-300">
            <Sidebar activeMenu="settings" />

            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <Header />

                <div className="flex-1 overflow-y-auto p-8 relative">
                    <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-cyan-500/20 dark:bg-cyan-600/10 rounded-full blur-[100px] pointer-events-none transition-colors duration-300"></div>

                    <div className="max-w-4xl mx-auto relative z-10">
                        <div className="mb-8">
                            <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white mb-2 tracking-tight">{t('accountSettingsTitle', 'Hesap Ayarları')}</h1>
                            <p className="text-slate-600 dark:text-slate-400 text-sm">{t('accountSettingsDesc', 'Profil bilgilerinizi ve güvenlik tercihlerinizi buradan yönetebilirsiniz.')}</p>
                        </div>

                        <div className="bg-white dark:bg-[#111C3A] rounded-3xl border border-slate-200 dark:border-white/5 shadow-md dark:shadow-xl overflow-hidden transition-colors duration-300">
                            <div className="flex border-b border-slate-200 dark:border-white/5 bg-slate-50 dark:bg-[#0B1325]/50 transition-colors">
                                <button
                                    onClick={() => setActiveTab('profile')}
                                    className={`flex items-center gap-2 px-8 py-5 text-sm font-bold transition-all border-b-2 ${
                                        activeTab === 'profile'
                                            ? 'border-cyan-600 dark:border-cyan-500 text-cyan-700 dark:text-cyan-400 bg-cyan-50 dark:bg-cyan-500/5'
                                            : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-white/5'
                                    }`}
                                >
                                    <User size={18} />
                                    {t('profileInfo', 'Profil Bilgileri')}
                                </button>
                                <button
                                    onClick={() => setActiveTab('security')}
                                    className={`flex items-center gap-2 px-8 py-5 text-sm font-bold transition-all border-b-2 ${
                                        activeTab === 'security'
                                            ? 'border-cyan-600 dark:border-cyan-500 text-cyan-700 dark:text-cyan-400 bg-cyan-50 dark:bg-cyan-500/5'
                                            : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-white/5'
                                    }`}
                                >
                                    <Shield size={18} />
                                    {t('securityAndPass', 'Güvenlik ve Şifre')}
                                </button>
                            </div>

                            <div className="p-8">
                                {activeTab === 'profile' && (
                                    <form onSubmit={handleProfileUpdate} className="space-y-6 max-w-xl">
                                        <div className="flex items-center gap-6 mb-8">
                                            <div className="w-24 h-24 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-3xl shadow-lg shadow-cyan-900/50">
                                                {user?.firstName?.charAt(0) || 'U'}{user?.lastName?.charAt(0) || ''}
                                            </div>
                                            <div>
                                                <h3 className="text-slate-900 dark:text-white font-bold mb-1">{t('profilePhoto', 'Profil Fotoğrafı')}</h3>
                                                <p className="text-xs text-slate-500 dark:text-slate-400 mb-3">{t('profilePhotoDesc', 'Sisteme kayıtlı ad ve soyadınızın baş harfleri otomatik olarak atanır.')}</p>
                                            </div>
                                        </div>

                                        <div className="grid grid-cols-2 gap-6">
                                            <div>
                                                <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('firstName', 'Adınız')}</label>
                                                <input
                                                    type="text"
                                                    required
                                                    value={profileData.firstName}
                                                    onChange={(e) => setProfileData({...profileData, firstName: e.target.value})}
                                                    className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-3 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none font-medium transition-colors"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('lastName', 'Soyadınız')}</label>
                                                <input
                                                    type="text"
                                                    required
                                                    value={profileData.lastName}
                                                    onChange={(e) => setProfileData({...profileData, lastName: e.target.value})}
                                                    className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-3 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none font-medium transition-colors"
                                                />
                                            </div>
                                        </div>

                                        <div>
                                            <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('emailAddr', 'E-posta Adresi')}</label>
                                            <input
                                                type="email"
                                                disabled
                                                value={user?.email || ''}
                                                className="w-full bg-slate-100 dark:bg-[#0B1325]/50 border border-slate-200 dark:border-white/5 rounded-xl py-3 px-4 text-slate-400 dark:text-slate-500 cursor-not-allowed font-medium transition-colors"
                                            />
                                            <p className="text-xs text-slate-500 mt-2">{t('emailNotice', 'E-posta adresi güvenlik nedeniyle değiştirilemez.')}</p>
                                        </div>

                                        <button
                                            type="submit"
                                            disabled={isSubmitting}
                                            className={`py-3 px-6 rounded-xl font-bold transition-all flex items-center justify-center gap-2 mt-4 shadow-lg
                                                ${isSubmitting ? 'bg-slate-300 dark:bg-slate-600 text-slate-500 dark:text-slate-400 cursor-not-allowed shadow-none' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                        >
                                            <Save size={18} />
                                            {t('saveChanges', 'Değişiklikleri Kaydet')}
                                        </button>
                                    </form>
                                )}

                                {activeTab === 'security' && (
                                    <form onSubmit={handlePasswordChange} className="space-y-6 max-w-xl">
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="w-10 h-10 rounded-xl bg-cyan-50 dark:bg-cyan-500/10 flex items-center justify-center text-cyan-600 dark:text-cyan-400 transition-colors">
                                                <KeyRound size={20} />
                                            </div>
                                            <h3 className="text-lg font-bold text-slate-900 dark:text-white">{t('changePasswordTitle', 'Şifre Değiştirme')}</h3>
                                        </div>

                                        <div>
                                            <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('currentPassword', 'Mevcut Şifreniz')}</label>
                                            <input
                                                type="password"
                                                required
                                                value={securityData.currentPassword}
                                                onChange={(e) => setSecurityData({...securityData, currentPassword: e.target.value})}
                                                className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-3 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                            />
                                        </div>

                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                            <div>
                                                <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('newPassword', 'Yeni Şifre')}</label>
                                                <input
                                                    type="password"
                                                    required
                                                    minLength="6"
                                                    value={securityData.newPassword}
                                                    onChange={(e) => setSecurityData({...securityData, newPassword: e.target.value})}
                                                    className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-3 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-sm font-bold text-slate-700 dark:text-slate-300 mb-2">{t('newPasswordConfirm', 'Yeni Şifre (Tekrar)')}</label>
                                                <input
                                                    type="password"
                                                    required
                                                    minLength="6"
                                                    value={securityData.confirmPassword}
                                                    onChange={(e) => setSecurityData({...securityData, confirmPassword: e.target.value})}
                                                    className="w-full bg-slate-50 dark:bg-[#0B1325] border border-slate-200 dark:border-white/5 rounded-xl py-3 px-4 text-slate-900 dark:text-white focus:border-cyan-500/50 focus:outline-none transition-colors"
                                                />
                                            </div>
                                        </div>

                                        <button
                                            type="submit"
                                            disabled={isSubmitting}
                                            className={`py-3 px-6 rounded-xl font-bold transition-all flex items-center justify-center gap-2 mt-4 shadow-lg
                                                ${isSubmitting ? 'bg-slate-300 dark:bg-slate-600 text-slate-500 dark:text-slate-400 cursor-not-allowed shadow-none' : 'bg-cyan-600 hover:bg-cyan-500 text-white shadow-cyan-900/50 dark:shadow-cyan-900/50'}`}
                                        >
                                            <Shield size={18} />
                                            {t('updatePasswordBtn', 'Şifreyi Güncelle')}
                                        </button>
                                    </form>
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}

export default Settings;