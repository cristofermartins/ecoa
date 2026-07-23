import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, Polyline, Marker, useMap } from 'react-leaflet';
import { showSuccess, showError } from '../toast.js';
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

const flagLabels = {
  speed_hard_reject: 'Velocidade > 45 km/h',
  accel_hard_reject: 'Aceleração > 2.5 m/s²',
  high_speed_points: 'Pontos de alta velocidade',
  high_avg_speed: 'Velocidade média alta',
  constant_speed: 'Velocidade muito constante',
  off_path: 'Fora de ciclovia',
  gps_jump: 'Salto de GPS',
  low_accuracy: 'Precisão GPS baixa',
  bus_pattern: 'Padrão de ônibus',
  too_short: 'Distância muito curta',
  insufficient_points: 'Pontos insuficientes',
  no_pedaling: 'Sem pedalagem (IMU)',
  auto_validated: 'Validada automaticamente',
  auto_validated_gps_only: 'Validada por GPS (sem IMU)'
};

function FitBounds({ points }) {
  const map = useMap();
  useEffect(() => {
    if (points && points.length > 0) {
      const bounds = points.map(p => [p.latitude, p.longitude]);
      map.fitBounds(bounds, { padding: [30, 30] });
    }
  }, [points, map]);
  return null;
}

function SpeedColor(kmh) {
  if (kmh > 40) return '#dc2626';
  if (kmh > 30) return '#f59e0b';
  if (kmh > 20) return '#84cc16';
  return '#22c55e';
}

export default function RideValidation() {
  const navigate = useNavigate();
  const [rides, setRides] = useState([]);
  const [selectedRide, setSelectedRide] = useState(null);
  const [ridePoints, setRidePoints] = useState([]);
  const [loading, setLoading] = useState(true);
  const [notes, setNotes] = useState('');

  useEffect(() => {
    loadRides();
  }, []);

  const loadRides = async () => {
    const token = localStorage.getItem('SessionJWT');
    try {
      const res = await fetch(`${API_URL}/api/rides/pending`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      const data = await res.json();
      setRides(Array.isArray(data) ? data : []);
    } catch {}
    setLoading(false);
  };

  const selectRide = async (ride) => {
    setSelectedRide(ride);
    const token = localStorage.getItem('SessionJWT');
    try {
      const res = await fetch(`${API_URL}/api/rides/${ride.id}/points`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      const data = await res.json();
      setRidePoints(Array.isArray(data) ? data : []);
    } catch {
      setRidePoints([]);
    }
  };

  const handleValidate = async (approved) => {
    if (!selectedRide) return;
    const token = localStorage.getItem('SessionJWT');
    try {
      const res = await fetch(`${API_URL}/api/rides/${selectedRide.id}/validate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ approved, notes: notes || null })
      });
      if (!res.ok) {
        const data = await res.json();
        showError(data.error || 'Erro ao validar');
        return;
      }
      showSuccess(approved ? 'Pedalada aprovada!' : 'Pedalada rejeitada');
      setSelectedRide(null);
      setRidePoints([]);
      setNotes('');
      loadRides();
    } catch {
      showError('Erro de conexão');
    }
  };

  const formatDuration = (start, end) => {
    if (!start || !end) return '--';
    const sec = (new Date(end) - new Date(start)) / 1000;
    const m = Math.floor(sec / 60);
    const s = Math.floor(sec % 60);
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  const computeSpeeds = (points) => {
    const speeds = [];
    for (let i = 1; i < points.length; i++) {
      const prev = points[i - 1];
      const curr = points[i];
      const dt = (new Date(curr.recordedAt) - new Date(prev.recordedAt)) / 1000;
      if (dt <= 0) continue;
      const R = 6371;
      const dLat = (curr.latitude - prev.latitude) * Math.PI / 180;
      const dLon = (curr.longitude - prev.longitude) * Math.PI / 180;
      const a = Math.sin(dLat / 2) ** 2 + Math.cos(prev.latitude * Math.PI / 180) * Math.cos(curr.latitude * Math.PI / 180) * Math.sin(dLon / 2) ** 2;
      const dist = R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
      speeds.push({ index: i, speed: (dist / dt) * 3600 });
    }
    return speeds;
  };

  const speeds = computeSpeeds(ridePoints);
  const maxChartSpeed = Math.max(50, ...speeds.map(s => s.speed));

  return (
    <div className="page-container" style={{ maxWidth: '100%', padding: 0 }}>
      <div style={{ padding: '16px' }}>
        <div className="flex items-center gap-3 mb-4">
          <button onClick={() => navigate(-1)} className="text-green-700 text-2xl">&larr;</button>
          <h1 className="text-2xl font-bold text-green-900">Validar Pedaladas</h1>
        </div>
      </div>

      {selectedRide ? (
        <div>
          <div style={{ height: '40vh', width: '100%' }}>
            <MapContainer center={[-23.96, -46.33]} zoom={14} style={{ height: '100%', width: '100%' }} zoomControl={false}>
              <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
              <FitBounds points={ridePoints} />
              {ridePoints.length > 1 && (
                <Polyline
                  positions={ridePoints.map(p => [p.latitude, p.longitude])}
                  pathOptions={{ color: '#2d6a4f', weight: 4 }}
                />
              )}
              {ridePoints.length > 0 && (
                <>
                  <Marker position={[ridePoints[0].latitude, ridePoints[0].longitude]} />
                  <Marker position={[ridePoints[ridePoints.length - 1].latitude, ridePoints[ridePoints.length - 1].longitude]} />
                </>
              )}
            </MapContainer>
          </div>

          <div style={{ padding: '16px' }}>
            <div className="flex items-center justify-between mb-3">
              <div>
                <p className="font-semibold text-green-900">
                  {selectedRide.user?.name || 'Usuário'}
                  {selectedRide.autoValidated && (
                    <span className="eco-badge eco-badge-green" style={{ marginLeft: 8, fontSize: 10 }}>
                      Auto-validada
                    </span>
                  )}
                </p>
                <p className="text-xs text-gray-500">
                  {new Date(selectedRide.startedAt).toLocaleString('pt-BR')}
                </p>
              </div>
              <span className={`eco-badge ${statusBadge[selectedRide.status] || 'eco-badge-yellow'}`}>
                {statusLabels[selectedRide.status] || selectedRide.status}
              </span>
            </div>

            <div className="grid grid-cols-2 gap-3 mb-4">
              <div className="eco-card stat-card">
                <div className="stat-value">{selectedRide.totalDistanceKm ?? '--'} km</div>
                <div className="stat-label">Distância</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{formatDuration(selectedRide.startedAt, selectedRide.endedAt)}</div>
                <div className="stat-label">Duração</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{selectedRide.avgSpeedKmh ?? '--'} km/h</div>
                <div className="stat-label">Vel. Média</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{selectedRide.maxSpeedKmh ?? '--'} km/h</div>
                <div className="stat-label">Vel. Máxima</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{selectedRide.cyclePathMatchPercent != null ? `${selectedRide.cyclePathMatchPercent}%` : '--'}</div>
                <div className="stat-label">Em ciclovia</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{ridePoints.length}</div>
                <div className="stat-label">Pontos GPS</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{selectedRide.avgCadence != null ? `${selectedRide.avgCadence}` : '--'}</div>
                <div className="stat-label">Cadência (RPM)</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{selectedRide.pedalingPercent != null ? `${selectedRide.pedalingPercent}%` : '--'}</div>
                <div className="stat-label">Tempo pedalando</div>
              </div>
            </div>

            {selectedRide.reason && (
              <div className="mb-4">
                <p className="text-sm font-semibold text-gray-700 mb-1">Decisão automática:</p>
                <div className={`text-sm px-3 py-2 rounded-lg ${
                  selectedRide.status === 'Rejected' ? 'bg-red-50 text-red-800' :
                  selectedRide.status === 'PendingValidation' ? 'bg-yellow-50 text-yellow-800' :
                  'bg-green-50 text-green-800'
                }`}>
                  {selectedRide.reason}
                </div>
              </div>
            )}

            {selectedRide.flags && (
              <div className="mb-4">
                <p className="text-sm font-semibold text-gray-700 mb-2">Alertas:</p>
                <div className="flex flex-wrap gap-2">
                  {selectedRide.flags.split(',').map(flag => (
                    <span key={flag} className={`text-xs px-2 py-1 rounded-full ${
                      flag.includes('hard_reject') ? 'bg-red-100 text-red-700' : 'bg-yellow-50 text-yellow-700'
                    }`}>
                      {flagLabels[flag] || flag}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {speeds.length > 0 && (
              <div className="eco-card mb-4">
                <p className="text-sm font-semibold text-gray-700 mb-2">Velocidade ao longo do tempo</p>
                <div style={{ position: 'relative', height: 120 }}>
                  <svg width="100%" height="120" viewBox={`0 0 ${speeds.length} 120`} preserveAspectRatio="none">
                    <line x1="0" y1="80" x2={speeds.length} y2="80" stroke="#e5e7eb" strokeWidth="1" />
                    <line x1="0" y1="40" x2={speeds.length} y2="40" stroke="#fca5a5" strokeWidth="1" strokeDasharray="4" />
                    <text x="2" y="44" fontSize="8" fill="#ef4444">45 km/h</text>
                    <line x1="0" y1="53" x2={speeds.length} y2="53" stroke="#fcd34d" strokeWidth="1" strokeDasharray="4" />
                    <text x="2" y="57" fontSize="8" fill="#f59e0b">35 km/h</text>
                    <polyline
                      fill="none"
                      stroke="#2d6a4f"
                      strokeWidth="2"
                      points={speeds.map((s, i) => `${i},${120 - (s.speed / maxChartSpeed) * 110}`).join(' ')}
                    />
                  </svg>
                </div>
              </div>
            )}

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">Observações</label>
              <textarea
                className="eco-input"
                rows={2}
                placeholder="Motivo da rejeição (opcional)..."
                value={notes}
                onChange={e => setNotes(e.target.value)}
              />
            </div>

            <div className="flex gap-3">
              <button onClick={() => handleValidate(false)} className="eco-btn-outline flex-1" style={{ borderColor: '#dc2626', color: '#dc2626' }}>
                Rejeitar
              </button>
              <button onClick={() => handleValidate(true)} className="eco-btn flex-1">
                Aprovar
              </button>
            </div>

            <button onClick={() => { setSelectedRide(null); setRidePoints([]); setNotes(''); }} className="w-full text-center text-sm text-gray-500 mt-3 py-2">
              Voltar para lista
            </button>
          </div>
        </div>
      ) : (
        <div style={{ padding: '0 16px 16px' }}>
          {loading ? (
            <p className="text-gray-400 text-center py-8">Carregando...</p>
          ) : rides.length === 0 ? (
            <p className="text-gray-400 text-center py-8">Nenhuma pedalada pendente</p>
          ) : (
            <div className="space-y-3">
              {rides.map(ride => (
                <div key={ride.id} className="eco-card cursor-pointer" onClick={() => selectRide(ride)}>
                  <div className="flex items-center justify-between mb-2">
                    <div>
                      <p className="font-medium text-sm">{ride.user?.name || 'Usuário'}</p>
                      <p className="text-xs text-gray-500">
                        {new Date(ride.startedAt).toLocaleDateString('pt-BR')} {new Date(ride.startedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                      </p>
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
                  {ride.flags && (
                    <div className="mt-2 flex flex-wrap gap-1">
                      {ride.flags.split(',').map(flag => (
                        <span key={flag} className={`text-xs px-2 py-0.5 rounded-full ${
                          flag.includes('hard_reject') ? 'bg-red-100 text-red-700' : 'bg-yellow-50 text-yellow-700'
                        }`}>
                          {flagLabels[flag] || flag}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
