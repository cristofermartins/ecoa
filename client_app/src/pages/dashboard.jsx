import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSession } from '../session/session_provider.jsx';
import API_URL from '../api_url.js';
import RideTracker from './ride_tracker.jsx';
import MyRides from './my_rides.jsx';
import TokenWallet from './token_wallet.jsx';

const TABS = [
  { key: 'pedalar', label: 'Pedalar' },
  { key: 'pedaladas', label: 'Minhas Pedaladas' },
  { key: 'carteira', label: 'Carteira Ecoa' }
];

export default function Dashboard() {
  const { user, logout } = useSession();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('pedalar');
  const [actions, setActions] = useState([]);
  const [ridesRefreshKey, setRidesRefreshKey] = useState(0);

  useEffect(() => {
    fetch(`${API_URL}/api/actions`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('SessionJWT')}` }
    }).then(r => r.json()).then(setActions).catch(() => {});
  }, []);

  const recentActions = Array.isArray(actions) ? actions.slice(0, 5) : [];

  return (
    <div className="page-container">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-green-900">Ecoa Santos</h1>
          <p className="text-sm text-gray-500">Olá, {user?.name?.split(' ')[0]}</p>
        </div>
        <div className="flex gap-2">
          <Link to="/profile" className="eco-btn-outline text-sm px-3 py-2">Perfil</Link>
          <button onClick={logout} className="text-sm text-red-500 px-3 py-2">Sair</button>
        </div>
      </div>

      <div className="nav-tabs mb-6">
        {TABS.map(tab => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={`nav-tab ${activeTab === tab.key ? 'active' : ''}`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div style={{ display: activeTab === 'pedalar' ? 'block' : 'none' }}>
        <RideTracker
          onNavigateToRides={() => setActiveTab('pedaladas')}
          onRideFinished={() => setRidesRefreshKey(k => k + 1)}
        />
      </div>

      <div style={{ display: activeTab === 'pedaladas' ? 'block' : 'none' }}>
        <MyRides embedded onNavigateToTracker={() => setActiveTab('pedalar')} refreshKey={ridesRefreshKey} visible={activeTab === 'pedaladas'} />
      </div>

      <div style={{ display: activeTab === 'carteira' ? 'block' : 'none' }}>
        <TokenWallet embedded />
      </div>

    </div>
  );
}
