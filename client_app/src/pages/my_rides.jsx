import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import API_URL from '../api_url.js';

const statusLabels = {
  Active: 'Ativa',
  PendingValidation: 'Pendente',
  Validated: 'Validada',
  Rejected: 'Rejeitada'
};

const statusBadge = {
  Active: 'eco-badge-blue',
  PendingValidation: 'eco-badge-yellow',
  Validated: 'eco-badge-green',
  Rejected: 'eco-badge-red'
};

export default function MyRides({ embedded, onNavigateToTracker, refreshKey, visible }) {
  const navigate = useNavigate();
  const [rides, setRides] = useState([]);
  const [loading, setLoading] = useState(true);
  const fetchIdRef = useRef(0);

  useEffect(() => {
    const fetchId = ++fetchIdRef.current;
    const token = localStorage.getItem('SessionJWT');
    setLoading(true);
    fetch(`${API_URL}/api/rides/my?_=${Date.now()}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(r => r.json())
      .then(data => {
        if (fetchId !== fetchIdRef.current) return;
        setRides(Array.isArray(data) ? data : []);
        setLoading(false);
      })
      .catch(() => {
        if (fetchId !== fetchIdRef.current) return;
        setLoading(false);
      });
  }, [refreshKey, visible]);

  const activeRide = rides.find(r => r.status === 'Active');

  return (
    <div className="page-container">
      {!embedded && (
        <div className="flex items-center gap-3 mb-6">
          <button onClick={() => navigate(-1)} className="text-green-700 text-2xl">&larr;</button>
          <h1 className="text-2xl font-bold text-green-900">Minhas Pedaladas</h1>
        </div>
      )}

      {activeRide ? (
        <button onClick={() => onNavigateToTracker ? onNavigateToTracker() : navigate('/ride-tracker')} className="eco-btn w-full mb-4" style={{ background: 'linear-gradient(135deg, #2d6a4f, #40916c)' }}>
          🚲 Continuar Pedalada em Andamento
        </button>
      ) : (
        <button onClick={() => onNavigateToTracker ? onNavigateToTracker() : navigate('/ride-tracker')} className="eco-btn w-full mb-4">
          🚲 Nova Pedalada
        </button>
      )}

      {loading ? (
        <p className="text-gray-400 text-center py-8">Carregando...</p>
      ) : rides.length === 0 ? (
        <div className="text-center py-8">
          <div className="text-5xl mb-3">🚲</div>
          <p className="text-gray-400">Nenhuma pedalada registrada</p>
          <p className="text-gray-400 text-sm mt-1">Use o GPS para registrar suas pedaladas</p>
        </div>
      ) : (
        <div className="space-y-3">
          {rides.map(ride => (
            <div key={ride.id} className="eco-card">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-2">
                  <span className="text-xl">🚲</span>
                  <div>
                    <p className="font-medium text-sm">
                      {new Date(ride.startedAt).toLocaleDateString('pt-BR')}
                    </p>
                    <p className="text-xs text-gray-500">
                      {new Date(ride.startedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                    </p>
                  </div>
                </div>
                <span className={`eco-badge ${statusBadge[ride.status] || 'eco-badge-yellow'}`}>
                  {statusLabels[ride.status] || ride.status}
                </span>
              </div>
              <div className="grid grid-cols-3 gap-2 text-center text-sm">
                <div>
                  <p className="font-bold text-green-800">{ride.totalDistanceKm ?? '--'} km</p>
                  <p className="text-xs text-gray-500">Distância</p>
                </div>
                <div>
                  <p className="font-bold text-green-800">{ride.avgSpeedKmh ?? '--'} km/h</p>
                  <p className="text-xs text-gray-500">Vel. Média</p>
                </div>
                <div>
                  <p className="font-bold text-green-800">{ride.cyclePathMatchPercent != null ? `${ride.cyclePathMatchPercent}%` : '--'}</p>
                  <p className="text-xs text-gray-500">Ciclovia</p>
                </div>
              </div>
              {ride.reason && (
                <div className={`mt-2 text-xs px-2 py-1 rounded ${
                  ride.status === 'Rejected' ? 'bg-red-50 text-red-700' :
                  ride.status === 'PendingValidation' ? 'bg-yellow-50 text-yellow-700' :
                  'bg-green-50 text-green-700'
                }`}>
                  {ride.reason}
                </div>
              )}
              {ride.flags && (
                <div className="mt-2 flex flex-wrap gap-1">
                  {ride.flags.split(',').map(flag => (
                    <span key={flag} className="text-xs bg-red-50 text-red-600 px-2 py-0.5 rounded-full">
                      {flag}
                    </span>
                  ))}
                </div>
              )}
              {ride.ecoaAmount != null && (
                <div className="mt-2 text-sm text-green-700 font-semibold">
                  +{ride.ecoaAmount} ECOA
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
