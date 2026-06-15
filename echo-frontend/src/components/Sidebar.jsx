import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import {
    LayoutDashboard, Ticket, FolderHeart, Wallet, Settings, LogOut, Grid3X3, BrainCircuit
} from 'lucide-react';

function Sidebar({ activeMenu }) {
    const navigate = useNavigate();
    const { logout, user } = useAuth();
    const { t } = useLanguage();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const getMenuStyles = (menuName) => {
        const baseStyles = "flex items-center gap-3 px-4 py-3 rounded-xl transition-all cursor-pointer ";
        const activeStyles = "bg-gradient-to-r from-cyan-600 to-blue-600 text-white shadow-lg shadow-cyan-900/20 font-medium";
        const inactiveStyles = "text-slate-400 hover:text-white hover:bg-[#1A2744] font-medium";

        return baseStyles + (activeMenu === menuName ? activeStyles : inactiveStyles);
    };

    return (
        <aside className="w-64 bg-[#111C3A] border-r border-white/5 flex flex-col justify-between hidden md:flex z-20 h-screen sticky top-0">
            <div>
                <div className="h-24 flex items-center px-8 border-b border-white/5 cursor-pointer" onClick={() => navigate('/dashboard')}>
                    <span className="text-white text-2xl font-bold flex items-center gap-2 tracking-wide">
                        <span className="text-cyan-400">|||</span> ECHO
                    </span>
                </div>
                <nav className="p-4 space-y-2 mt-4">
                    <div onClick={() => navigate('/dashboard')} className={getMenuStyles('dashboard')}>
                        <LayoutDashboard size={20} />
                        <span>{t('dashboardMenu', 'Genel Bakış')}</span>
                    </div>
                    <div onClick={() => navigate('/explore')} className={getMenuStyles('explore')}>
                        <FolderHeart size={20} />
                        <span>{t('exploreMenu', 'Keşfet')}</span>
                    </div>
                    <div onClick={() => navigate('/tickets')} className={getMenuStyles('tickets')}>
                        <Ticket size={20} />
                        <span>{t('ticketsMenu', 'Biletlerim')}</span>
                    </div>
                    <div onClick={() => navigate('/wallet')} className={getMenuStyles('wallet')}>
                        <Wallet size={20} />
                        <span>{t('walletMenu', 'Cüzdanım')}</span>
                    </div>

                    {(user?.role === 'Admin' || user?.role === 'Organizer') && (
                        <div onClick={() => navigate('/venues')} className={getMenuStyles('venues')}>
                            <Grid3X3 size={20} />
                            <span>{t('venuesMenu', 'Sahneler')}</span>
                        </div>
                    )}

                    {/* SADECE ADMIN GÖREBİLİR */}
                    {user?.role === 'Admin' && (
                        <>
                            <div className="h-px bg-white/10 my-4 mx-4"></div>
                            <div onClick={() => navigate('/analytics')} className={getMenuStyles('analytics')}>
                                <BrainCircuit size={20} />
                                <span>AI Analitik</span>
                            </div>
                        </>
                    )}

                </nav>
            </div>
            <div className="p-4 border-t border-white/5 space-y-2 bg-[#0D162B]">
                <div onClick={() => navigate('/settings')} className={getMenuStyles('settings')}>
                    <Settings size={20} />
                    <span>{t('settingsMenu', 'Ayarlar')}</span>
                </div>
                <button onClick={handleLogout} className="w-full flex items-center gap-3 px-4 py-3 text-red-400 hover:text-red-300 hover:bg-red-500/10 rounded-xl transition-all">
                    <LogOut size={20} />
                    <span className="font-medium">{t('logoutMenu', 'Çıkış Yap')}</span>
                </button>
            </div>
        </aside>
    );
}

export default Sidebar;