import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, Polyline, Marker, useMap } from 'react-leaflet';
import { Geolocation } from '@capacitor/geolocation';
import { showSuccess, showError } from '../toast.js';
import API_URL from '../api_url.js';

const INTERVAL_MS = 1000;
const BATCH_SIZE = 30;
const MIN_ACCURACY = 20;
const IMU_SEND_INTERVAL_MS = 10000;
const IMU_SUBSAMPLE_MOD = 2;

function MapUpdater({ center }) {
  const map = useMap();
  useEffect(() => {
    if (center) map.setView(center, map.getZoom());
  }, [center, map]);
  return null;
}

export default function RideTracker({ onNavigateToRides, onRideFinished }) {
  const navigate = useNavigate();
  const [status, setStatus] = useState('idle');
  const [rideId, setRideId] = useState(null);
  const [points, setPoints] = useState([]);
  const [center, setCenter] = useState([-23.96, -46.33]);
  const [metrics, setMetrics] = useState({ distance: 0, duration: 0, speed: 0, avgSpeed: 0 });
  const [gpsAccuracy, setGpsAccuracy] = useState(null);
  const [cyclePathPercent, setCyclePathPercent] = useState(null);
  const [rideResult, setRideResult] = useState(null);
  const [imuActive, setImuActive] = useState(false);

  const watchIdRef = useRef(null);
  const bufferRef = useRef([]);
  const startTimeRef = useRef(null);
  const totalDistanceRef = useRef(0);
  const lastPointRef = useRef(null);
  const speedsRef = useRef([]);
  const nearPathCountRef = useRef(0);
  const pointCountRef = useRef(0);
  const imuBufferRef = useRef([]);
  const imuSampleCounterRef = useRef(0);
  const imuTimerRef = useRef(null);
  const imuListenerActiveRef = useRef(false);
  const imuRideIdRef = useRef(null);

  const haversineDistance = (lat1, lon1, lat2, lon2) => {
    const R = 6371;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) ** 2 + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  };

  const handleDeviceMotion = useCallback((event) => {
    const accel = event.accelerationIncludingGravity;
    const rot = event.rotationRate;
    if (!accel || accel.x == null) return;

    imuSampleCounterRef.current++;
    if (imuSampleCounterRef.current % IMU_SUBSAMPLE_MOD !== 0) return;

    imuBufferRef.current.push({
      recordedAt: new Date().toISOString(),
      accelX: accel.x,
      accelY: accel.y,
      accelZ: accel.z,
      gyroX: rot?.alpha || 0,
      gyroY: rot?.beta || 0,
      gyroZ: rot?.gamma || 0
    });
  }, []);

  const sendImuBatch = useCallback(async () => {
    const currentRideId = imuRideIdRef.current;
    if (!currentRideId || imuBufferRef.current.length === 0) return;
    const token = localStorage.getItem('SessionJWT');
    const batch = [...imuBufferRef.current];
    imuBufferRef.current = [];
    try {
      await fetch(`${API_URL}/api/rides/${currentRideId}/imu`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ samples: batch })
      });
    } catch {
      imuBufferRef.current = [...batch, ...imuBufferRef.current];
    }
  }, []);

  const sendBatch = useCallback(async (batch) => {
    if (!rideId || batch.length === 0) return;
    const token = localStorage.getItem('SessionJWT');
    try {
      await fetch(`${API_URL}/api/rides/${rideId}/points`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({
          points: batch.map(p => ({
            latitude: p.lat,
            longitude: p.lng,
            accuracy: p.accuracy,
            speed: p.speed,
            recordedAt: p.timestamp
          }))
        })
      });
    } catch {
      bufferRef.current = [...bufferRef.current, ...batch];
    }
  }, [rideId]);

  const flushBuffer = useCallback(async () => {
    if (bufferRef.current.length === 0) return;
    const batch = [...bufferRef.current];
    bufferRef.current = [];
    await sendBatch(batch);
  }, [sendBatch]);

  const handlePosition = useCallback(async (position) => {
    if (!position || !position.coords) return;

    const { latitude, longitude, accuracy, speed } = position.coords;
    if (accuracy > MIN_ACCURACY) return;

    const now = new Date().toISOString();
    const point = { lat: latitude, lng: longitude, accuracy, speed: speed ? speed * 3.6 : null, timestamp: now };

    setPoints(prev => [...prev, [latitude, longitude]]);
    setCenter([latitude, longitude]);
    setGpsAccuracy(Math.round(accuracy));

    if (lastPointRef.current) {
      const dist = haversineDistance(lastPointRef.current.lat, lastPointRef.current.lng, latitude, longitude);
      totalDistanceRef.current += dist;
      const deltaTime = (new Date(now) - new Date(lastPointRef.current.timestamp)) / 1000;
      if (deltaTime > 0) {
        const speedKmh = (dist / deltaTime) * 3600;
        speedsRef.current.push(speedKmh);
      }
    }

    lastPointRef.current = point;
    pointCountRef.current++;

    const elapsed = startTimeRef.current ? (Date.now() - startTimeRef.current) / 1000 : 0;
    const avgSpeed = speedsRef.current.length > 0
      ? speedsRef.current.reduce((a, b) => a + b, 0) / speedsRef.current.length
      : 0;

    setMetrics({
      distance: Math.round(totalDistanceRef.current * 100) / 100,
      duration: Math.floor(elapsed),
      speed: speedsRef.current.length > 0 ? Math.round(speedsRef.current[speedsRef.current.length - 1] * 10) / 10 : 0,
      avgSpeed: Math.round(avgSpeed * 10) / 10
    });

    bufferRef.current.push(point);
    if (bufferRef.current.length >= BATCH_SIZE) {
      await flushBuffer();
    }
  }, [flushBuffer]);

  const startRide = async () => {
    const token = localStorage.getItem('SessionJWT');
    try {
      const res = await fetch(`${API_URL}/api/rides/start`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) {
        const data = await res.json();
        showError(data.error || 'Erro ao iniciar pedalada');
        return;
      }
      const ride = await res.json();
      setRideId(ride.id);
      imuRideIdRef.current = ride.id;
      setStatus('riding');
      startTimeRef.current = Date.now();
      totalDistanceRef.current = 0;
      lastPointRef.current = null;
      speedsRef.current = [];
      nearPathCountRef.current = 0;
      pointCountRef.current = 0;
      bufferRef.current = [];
      imuBufferRef.current = [];
      imuSampleCounterRef.current = 0;

      await startGpsWatch();
      await startImu();
    } catch {
      showError('Erro de conexão');
    }
  };

  const startGpsWatch = async () => {
    try {
      const perm = await Geolocation.requestPermissions();
      if (perm.location !== 'granted') {
        showError('Permissão de localização negada');
        return;
      }
    } catch {}

    const watchId = await Geolocation.watchPosition(
      { enableHighAccuracy: true, timeout: 2000, maximumAge: 0 },
      (pos, err) => {
        if (err) return;
        handlePosition(pos);
      }
    );
    watchIdRef.current = watchId;
  };

  const startImu = async () => {
    if (typeof DeviceMotionEvent !== 'undefined' &&
        typeof DeviceMotionEvent.requestPermission === 'function') {
      try {
        const perm = await DeviceMotionEvent.requestPermission();
        if (perm === 'granted') {
          imuListenerActiveRef.current = true;
          setImuActive(true);
        }
      } catch {}
    } else {
      imuListenerActiveRef.current = true;
      setImuActive(true);
    }

    if (imuListenerActiveRef.current) {
      window.addEventListener('devicemotion', handleDeviceMotion, true);
      imuTimerRef.current = setInterval(sendImuBatch, IMU_SEND_INTERVAL_MS);
    }
  };

  const resumeRide = async (ride) => {
    setRideId(ride.id);
    imuRideIdRef.current = ride.id;
    setStatus('riding');
    startTimeRef.current = new Date(ride.startedAt).getTime();
    totalDistanceRef.current = 0;
    lastPointRef.current = null;
    speedsRef.current = [];
    nearPathCountRef.current = 0;
    pointCountRef.current = 0;
    bufferRef.current = [];
    imuBufferRef.current = [];
    imuSampleCounterRef.current = 0;

    const token = localStorage.getItem('SessionJWT');
    try {
      const res = await fetch(`${API_URL}/api/rides/${ride.id}/points`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        const pts = await res.json();
        if (Array.isArray(pts) && pts.length > 0) {
          const positions = [];
          for (let i = 0; i < pts.length; i++) {
            const p = pts[i];
            positions.push([p.latitude, p.longitude]);
            const point = { lat: p.latitude, lng: p.longitude, accuracy: p.accuracy, speed: p.speed, timestamp: p.recordedAt };

            if (lastPointRef.current) {
              const dist = haversineDistance(lastPointRef.current.lat, lastPointRef.current.lng, p.latitude, p.longitude);
              totalDistanceRef.current += dist;
              const deltaTime = (new Date(p.recordedAt) - new Date(lastPointRef.current.timestamp)) / 1000;
              if (deltaTime > 0) {
                const speedKmh = (dist / deltaTime) * 3600;
                speedsRef.current.push(speedKmh);
              }
            }

            lastPointRef.current = point;
            pointCountRef.current++;
            if (p.nearCyclePath) nearPathCountRef.current++;
          }
          setPoints(positions);
          if (positions.length > 0) setCenter(positions[positions.length - 1]);

          const elapsed = startTimeRef.current ? (Date.now() - startTimeRef.current) / 1000 : 0;
          const avgSpeed = speedsRef.current.length > 0
            ? speedsRef.current.reduce((a, b) => a + b, 0) / speedsRef.current.length
            : 0;
          setMetrics({
            distance: Math.round(totalDistanceRef.current * 100) / 100,
            duration: Math.floor(elapsed),
            speed: speedsRef.current.length > 0 ? Math.round(speedsRef.current[speedsRef.current.length - 1] * 10) / 10 : 0,
            avgSpeed: Math.round(avgSpeed * 10) / 10
          });
        }
      }
    } catch {}

    await startGpsWatch();
    await startImu();
  };

  useEffect(() => {
    const token = localStorage.getItem('SessionJWT');
    fetch(`${API_URL}/api/rides/active`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(r => {
        if (r.ok) return r.json();
        return null;
      })
      .then(activeRide => {
        if (activeRide) resumeRide(activeRide);
      })
      .catch(() => {});

    return () => {
      if (watchIdRef.current) {
        Geolocation.clearWatch({ id: watchIdRef.current }).catch(() => {});
      }
      window.removeEventListener('devicemotion', handleDeviceMotion, true);
      if (imuTimerRef.current) {
        clearInterval(imuTimerRef.current);
      }
    };
  }, []);

  const stopRide = async () => {
    if (watchIdRef.current) {
      await Geolocation.clearWatch({ id: watchIdRef.current });
      watchIdRef.current = null;
    }

    if (imuListenerActiveRef.current) {
      await sendImuBatch();
      window.removeEventListener('devicemotion', handleDeviceMotion, true);
      if (imuTimerRef.current) {
        clearInterval(imuTimerRef.current);
        imuTimerRef.current = null;
      }
      imuListenerActiveRef.current = false;
      setImuActive(false);
    }

    await flushBuffer();

    const token = localStorage.getItem('SessionJWT');
    try {
      const res = await fetch(`${API_URL}/api/rides/${rideId}/stop`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) {
        const data = await res.json();
        showError(data.error || 'Erro ao finalizar pedalada');
        resetToIdle();
        return;
      }
      const ride = await res.json();
      setRideResult(ride);
      setStatus('summary');
      setCyclePathPercent(ride.cyclePathMatchPercent);
      if (ride.status === 'Validated' && ride.autoValidated) {
        showSuccess('Pedalada validada automaticamente!');
      } else if (ride.status === 'Rejected') {
        showError('Pedalada rejeitada');
      } else {
        showSuccess('Pedalada finalizada! Aguardando validação.');
      }
      onRideFinished?.();
    } catch {
      showError('Erro de conexão');
      resetToIdle();
    }
  };

  const resetToIdle = () => {
    setStatus('idle');
    setRideId(null);
    imuRideIdRef.current = null;
    setPoints([]);
    setCenter([-23.96, -46.33]);
    setMetrics({ distance: 0, duration: 0, speed: 0, avgSpeed: 0 });
    setGpsAccuracy(null);
    setCyclePathPercent(null);
    setRideResult(null);
    setImuActive(false);
    totalDistanceRef.current = 0;
    lastPointRef.current = null;
    speedsRef.current = [];
    nearPathCountRef.current = 0;
    pointCountRef.current = 0;
    bufferRef.current = [];
    imuBufferRef.current = [];
    imuSampleCounterRef.current = 0;
  };

  const formatDuration = (sec) => {
    const m = Math.floor(sec / 60);
    const s = sec % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  const polylinePositions = points;

  return (
    <div className="page-container" style={{ maxWidth: '100%', padding: 0 }}>
      <div style={{ height: '60vh', width: '100%', position: 'relative' }}>
        <MapContainer center={center} zoom={15} style={{ height: '100%', width: '100%' }} zoomControl={false}>
          <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
          <MapUpdater center={center} />
          {polylinePositions.length > 1 && (
            <Polyline positions={polylinePositions} pathOptions={{ color: '#2d6a4f', weight: 5 }} />
          )}
          {polylinePositions.length > 0 && (
            <Marker position={polylinePositions[polylinePositions.length - 1]} />
          )}
        </MapContainer>

        {status === 'riding' && (
          <div style={{
            position: 'absolute', top: 10, left: 10, right: 10,
            background: 'rgba(0,0,0,0.7)', color: 'white', borderRadius: 12,
            padding: '8px 12px', fontSize: 12, display: 'flex', justifyContent: 'space-between'
          }}>
            <span>GPS: {gpsAccuracy ? `${gpsAccuracy}m` : '--'}</span>
            <span>{imuActive ? 'IMU: ativo' : 'IMU: off'}</span>
            <span style={{ color: '#4ade80' }}>● Gravando</span>
          </div>
        )}
      </div>

      <div style={{ padding: '16px' }}>
        {status === 'idle' && (
          <div className="text-center">
            <div className="text-6xl mb-4">🚲</div>
            <h1 className="text-2xl font-bold text-green-900 mb-2">Iniciar Pedalada</h1>
            <p className="text-gray-500 mb-6">Pressione o botão para começar a registrar sua pedalada com GPS</p>
            <button onClick={startRide} className="eco-btn w-full text-lg py-4" style={{ fontSize: 18 }}>
              Começar a Pedalar
            </button>
          </div>
        )}

        {status === 'riding' && (
          <div>
            <div className="grid grid-cols-2 gap-3 mb-4">
              <div className="eco-card stat-card">
                <div className="stat-value">{metrics.distance}</div>
                <div className="stat-label">Distância (km)</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{formatDuration(metrics.duration)}</div>
                <div className="stat-label">Duração</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{metrics.speed}</div>
                <div className="stat-label">Velocidade (km/h)</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{metrics.avgSpeed}</div>
                <div className="stat-label">Média (km/h)</div>
              </div>
            </div>
            <button onClick={stopRide} className="eco-btn w-full text-lg py-4" style={{ background: 'linear-gradient(135deg, #dc2626, #ef4444)', fontSize: 18 }}>
              Finalizar Pedalada
            </button>
          </div>
        )}

        {status === 'summary' && (
          <div>
            <div className="text-center mb-4">
              <div className="text-5xl mb-2">
                {rideResult?.status === 'Validated' ? '✅' : rideResult?.status === 'Rejected' ? '❌' : '⏳'}
              </div>
              <h2 className="text-xl font-bold text-green-900">
                {rideResult?.status === 'Validated' ? (rideResult?.autoValidated ? 'Validada Automaticamente' : 'Pedalada Validada') : 
                 rideResult?.status === 'Rejected' ? 'Pedalada Rejeitada' : 'Pedalada Finalizada'}
              </h2>
              <p className="text-gray-500 text-sm">
                {rideResult?.status === 'Validated' ? `${rideResult?.ecoaAmount ?? 0} ECOA creditados` :
                 rideResult?.status === 'Rejected' ? (rideResult?.reason || 'Verifique os alertas') :
                 'Enviada para validação manual'}
              </p>
            </div>
            <div className="grid grid-cols-2 gap-3 mb-4">
              <div className="eco-card stat-card">
                <div className="stat-value">{metrics.distance}</div>
                <div className="stat-label">Distância (km)</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{formatDuration(metrics.duration)}</div>
                <div className="stat-label">Duração</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{metrics.avgSpeed}</div>
                <div className="stat-label">Vel. Média (km/h)</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{cyclePathPercent != null ? `${cyclePathPercent}%` : '--'}</div>
                <div className="stat-label">Em ciclovia</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{rideResult?.avgCadence != null ? `${rideResult.avgCadence}` : '--'}</div>
                <div className="stat-label">Cadência (RPM)</div>
              </div>
              <div className="eco-card stat-card">
                <div className="stat-value">{rideResult?.pedalingPercent != null ? `${rideResult.pedalingPercent}%` : '--'}</div>
                <div className="stat-label">Tempo pedalando</div>
              </div>
            </div>
            {rideResult?.status === 'Validated' && (
              <div className="bg-green-50 p-3 rounded-lg text-sm text-green-800 mb-4">
                Estimativa: <strong>{rideResult?.ecoaAmount ?? Math.round(metrics.distance * 5)} ECOA</strong>
              </div>
            )}
            {rideResult?.status === 'PendingValidation' && (
              <div className="bg-yellow-50 p-3 rounded-lg text-sm text-yellow-800 mb-4">
                Aguardando validação manual do administrador
              </div>
            )}
            {rideResult?.status === 'Rejected' && (
              <div className="bg-red-50 p-3 rounded-lg text-sm text-red-800 mb-4">
                {rideResult?.reason ? `Motivo: ${rideResult.reason}` : 'Pedalada não atende aos critérios'}
              </div>
            )}
            <button onClick={() => onNavigateToRides ? onNavigateToRides() : navigate('/my-rides')} className="eco-btn w-full">
              Ver Minhas Pedaladas
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
