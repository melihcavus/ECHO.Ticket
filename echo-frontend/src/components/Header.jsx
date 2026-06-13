import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { useLanguage } from '../context/LanguageContext';
import { Globe, Moon, Sun, ArrowLeft } from 'lucide-react';

function Header({ showBack = false }) {
    const navigate = useNavigate();
    const { user } = useAuth();
    const { isDarkMode, toggleTheme } = useTheme();
    const { lang, toggleLanguage } = useLanguage();

    return (
        <header className="h-24 bg-white/80 dark:bg-[#0B1325]/80 backdrop-blur-xl border-b border-slate-200 dark:border-white/5 flex items-center justify-between px-8 z-10 sticky top-0 transition-colors duration-300">
            <div className="flex-1">
                {showBack && (
                    <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-slate-500 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white transition-colors">
                        <ArrowLeft size={20} />
                        <span className="font-bold">Geri Dön</span>
                    </button>
                )}
            </div>

            <div className="flex items-center gap-6">
                <div className="flex items-center gap-3 border-r border-slate-200 dark:border-white/10 pr-6">
                    <button
                        onClick={toggleLanguage}
                        className="flex items-center gap-1 text-slate-500 dark:text-slate-400 hover:text-cyan-600 dark:hover:text-cyan-400 transition-colors font-bold uppercase"
                    >
                        <Globe size={20} />
                        <span className="text-xs">{lang}</span>
                    </button>
                    <button
                        onClick={toggleTheme}
                        className="text-slate-500 dark:text-slate-400 hover:text-amber-500 dark:hover:text-cyan-400 transition-colors"
                    >
                        {isDarkMode ? <Sun size={20} /> : <Moon size={20} />}
                    </button>
                </div>

                <div className="flex items-center gap-4 cursor-pointer hover:opacity-80 transition-opacity">
                    <div className="text-right hidden sm:block">
                        <p className="text-sm font-bold text-slate-900 dark:text-white">{user?.firstName} {user?.lastName}</p>
                        <p className="text-xs text-cyan-600 dark:text-cyan-400 font-bold">{user?.role || 'Kullanıcı'}</p>
                    </div>
                    <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-lg shadow-cyan-900/50 border-2 border-slate-50 dark:border-[#0B1325] ring-2 ring-cyan-500/30">
                        {user?.firstName?.charAt(0) || 'U'}{user?.lastName?.charAt(0) || ''}
                    </div>
                </div>
            </div>
        </header>
    );
}

export default Header;