import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Globe, Moon, ArrowLeft } from 'lucide-react';

function Header({ showBack = false }) {
    const navigate = useNavigate();
    const { user } = useAuth();

    return (
        <header className="h-24 bg-[#0B1325]/80 backdrop-blur-xl border-b border-white/5 flex items-center justify-between px-8 z-10 sticky top-0">
            <div className="flex-1">
                {showBack && (
                    <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-slate-400 hover:text-white transition-colors">
                        <ArrowLeft size={20} />
                        <span className="font-medium">Geri Dön</span>
                    </button>
                )}
            </div>

            <div className="flex items-center gap-6">
                <div className="flex items-center gap-3 border-r border-white/10 pr-6">
                    <button className="text-slate-400 hover:text-cyan-400 transition-colors">
                        <Globe size={20} />
                    </button>
                    <button className="text-slate-400 hover:text-cyan-400 transition-colors">
                        <Moon size={20} />
                    </button>
                </div>

                <div className="flex items-center gap-4 cursor-pointer hover:opacity-80 transition-opacity">
                    <div className="text-right hidden sm:block">
                        <p className="text-sm font-bold text-white">{user?.firstName} {user?.lastName}</p>
                        <p className="text-xs text-cyan-400 font-medium">{user?.role || 'Kullanıcı'}</p>
                    </div>
                    <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-lg shadow-cyan-900/50 border-2 border-[#0B1325] ring-2 ring-cyan-500/30">
                        {user?.firstName?.charAt(0) || 'U'}{user?.lastName?.charAt(0) || ''}
                    </div>
                </div>
            </div>
        </header>
    );
}

export default Header;

