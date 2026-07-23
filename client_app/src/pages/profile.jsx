import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSession } from '../session/session_provider.jsx';
import { showSuccess, showError } from '../toast.js';
import API_URL from '../api_url.js';

export default function Profile() {
  const navigate = useNavigate();
  const { user, logout } = useSession();
  const [stellarPublicKey, setStellarPublicKey] = useState(user?.stellarPublicKey || '');
  const [loading, setLoading] = useState(false);

  const roleLabel = (role) => {
    const map = { Fazedor: 'Fazedor', Admin: 'Admin' };
    return map[role] || role;
  };

  const roleIcon = (role) => {
    const map = { Fazedor: '🌿', Admin: '⚙️' };
    return map[role] || '👤';
  };

  return (
    <div className="page-container">
      <div className="flex items-center gap-3 mb-6">
        <button onClick={() => navigate(-1)} className="text-green-700 text-2xl">&larr;</button>
        <h1 className="text-2xl font-bold text-green-900">Perfil</h1>
      </div>

      <div className="eco-card text-center mb-4">
        <span className="text-5xl block mb-3">{roleIcon(user?.role)}</span>
        <h2 className="text-xl font-bold text-green-900">{user?.name}</h2>
        <p className="text-gray-500">{user?.email}</p>
        {user?.cpf && <p className="text-gray-400 text-sm">CPF: {user.cpf}</p>}
        <span className="eco-badge eco-badge-green mt-2">{roleLabel(user?.role)}</span>
      </div>

      <div className="eco-card mb-4">
        <h2 className="text-lg font-semibold text-green-900 mb-3">Carteira Stellar</h2>
        {user?.stellarPublicKey ? (
          <div>
            <p className="text-xs text-gray-500 mb-1">Chave Pública (Testnet)</p>
            <p className="text-sm font-mono bg-gray-100 p-2 rounded break-all">{user.stellarPublicKey}</p>
          </div>
        ) : (
          <div className="text-center py-4">
            <p className="text-gray-500 mb-3">Carteira Stellar não configurada</p>
            <p className="text-xs text-gray-400">A carteira será criada automaticamente ao registrar sua primeira ação</p>
          </div>
        )}
      </div>

      <div className="eco-card mb-4">
        <h2 className="text-lg font-semibold text-green-900 mb-3">Ações Rápidas</h2>
        <div className="space-y-2">
          <button onClick={() => navigate('/ride-tracker')} className="eco-btn-outline w-full text-left">
            🌿 Registrar ação ambiental
          </button>
          <button onClick={() => navigate('/wallet')} className="eco-btn-outline w-full text-left">
            💰 Ver carteira ECOA
          </button>
        </div>
      </div>

      <button onClick={logout} className="w-full py-3 text-red-500 font-semibold border border-red-200 rounded-xl hover:bg-red-50 transition-colors">
        Sair da conta
      </button>
    </div>
  );
}
