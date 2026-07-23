import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { showSuccess, showError } from '../toast.js';
import API_URL from '../api_url.js';

export default function TokenWallet({ embedded }) {
  const navigate = useNavigate();
  const [balance, setBalance] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [incentives, setIncentives] = useState([]);
  const [loading, setLoading] = useState(true);
  const [redeeming, setRedeeming] = useState(null);
  const [redeemedCode, setRedeemedCode] = useState(null);

  const token = localStorage.getItem('SessionJWT');

  const fetchData = () => {
    setLoading(true);
    Promise.all([
      fetch(`${API_URL}/api/wallet/balance`, { headers: { Authorization: `Bearer ${token}` } }).then(r => r.json()),
      fetch(`${API_URL}/api/wallet/transactions`, { headers: { Authorization: `Bearer ${token}` } }).then(r => r.json()),
      fetch(`${API_URL}/api/wallet/incentives`, { headers: { Authorization: `Bearer ${token}` } }).then(r => r.json())
    ])
      .then(([bal, tx, inc]) => {
        setBalance(bal);
        setTransactions(Array.isArray(tx) ? tx : []);
        setIncentives(Array.isArray(inc) ? inc : []);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchData(); }, []);

  const handleRedeem = async (incentive) => {
    setRedeeming(incentive.id);
    setRedeemedCode(null);
    try {
      const res = await fetch(`${API_URL}/api/wallet/redeem-incentive`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ incentiveId: incentive.id })
      });
      const data = await res.json();
      if (!res.ok) {
        showError(data.error || 'Erro ao resgatar');
        return;
      }
      showSuccess(`Incentivo "${incentive.name}" resgatado com sucesso!`);
      setRedeemedCode(data.incentive?.code || null);
      fetchData();
    } catch {
      showError('Erro de conexão');
    } finally {
      setRedeeming(null);
    }
  };

  const parseMetadata = (tx) => {
    if (!tx.metadata) return null;
    try {
      return typeof tx.metadata === 'string' ? JSON.parse(tx.metadata) : tx.metadata;
    } catch {
      return null;
    }
  };

  return (
    <div className="page-container">
      {!embedded && (
        <div className="flex items-center gap-3 mb-6">
          <button onClick={() => navigate(-1)} className="text-green-700 text-2xl">&larr;</button>
          <h1 className="text-2xl font-bold text-green-900">Carteira ECOA</h1>
        </div>
      )}

      {loading ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-700"></div>
        </div>
      ) : (
        <>
          <div className="eco-card text-center mb-4 eco-gradient text-green-900">
            <p className="text-sm opacity-80 mb-1">Saldo ECOA</p>
            <p className="text-4xl font-bold">{balance?.balance || '0'}</p>
            <p className="text-sm opacity-80 mt-1">ECOA</p>
          </div>

          {redeemedCode && (
            <div className="eco-card mb-4 bg-yellow-50 border-2 border-yellow-400">
              <h2 className="text-lg font-semibold text-yellow-800 mb-2">Cupom Resgatado!</h2>
              <p className="text-sm text-yellow-700 mb-2">Seu código de incentivo:</p>
              <div className="bg-white p-3 rounded-lg border border-yellow-300 text-center">
                <p className="text-2xl font-mono font-bold text-green-700 tracking-wider">{redeemedCode}</p>
              </div>
              <p className="text-xs text-yellow-600 mt-2">Apresente este código no estabelecimento para utilizar seu benefício.</p>
            </div>
          )}

          <div className="eco-card mb-4">
            <h2 className="text-lg font-semibold text-green-900 mb-3">Incentivos Disponíveis</h2>
            <p className="text-sm text-gray-500 mb-3">Resgate cupons de desconto usando seus tokens ECOA.</p>
            {incentives.length === 0 ? (
              <p className="text-gray-400 text-center py-4">Nenhum incentivo disponível no momento.</p>
            ) : (
              <div className="space-y-3">
                {incentives.map(inc => (
                  <div key={inc.id} className="p-4 bg-green-50 rounded-lg border border-green-100">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <h3 className="font-semibold text-green-900">{inc.name}</h3>
                        <p className="text-sm text-gray-600 mt-1">{inc.description}</p>
                        <div className="flex items-center gap-3 mt-2">
                          <span className="text-xs text-gray-500">Fornecido por: {inc.provider}</span>
                          <span className="text-sm font-bold text-green-700">{inc.price} ECOA</span>
                        </div>
                      </div>
                      <button
                        onClick={() => handleRedeem(inc)}
                        className="eco-btn text-sm ml-3"
                        disabled={redeeming === inc.id}
                        style={{ whiteSpace: 'nowrap' }}
                      >
                        {redeeming === inc.id ? '...' : 'Resgatar'}
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="eco-card">
            <h2 className="text-lg font-semibold text-green-900 mb-3">Extrato</h2>
            {transactions.length === 0 ? (
              <p className="text-gray-400 text-center py-4">Nenhuma transação</p>
            ) : (
              <div className="space-y-2">
                {transactions.map(tx => {
                  const meta = parseMetadata(tx);
                  return (
                    <div key={tx.id} className="p-3 bg-gray-50 rounded-lg">
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="font-medium text-sm">{tx.description || tx.type}</p>
                          <p className="text-xs text-gray-500">{new Date(tx.createdAt).toLocaleString('pt-BR')}</p>
                        </div>
                        <span className={`font-bold ${tx.type === 'Mint' ? 'text-green-600' : 'text-red-500'}`}>
                          {tx.type === 'Mint' ? '+' : '-'}{tx.amount} ECOA
                        </span>
                      </div>
                      {meta && (
                        <div className="mt-2 pt-2 border-t border-gray-200 text-xs text-gray-500">
                          <p>Atividade: {meta.action_type === 'BikeRide' ? 'Pedalada' : meta.action_type}</p>
                          <p>Dado qualitativo: {meta.qualitative_value} {meta.qualitative_unit}</p>
                          {meta.distance_km && <p>Distância: {meta.distance_km} km</p>}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
