import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSession } from '../session/session_provider.jsx';
import { showError } from '../toast.js';
import API_URL from '../api_url.js';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useSession();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      const data = await res.json();
      if (!res.ok) {
        showError(data.error || 'Erro ao fazer login');
        return;
      }
      login(data.token);
      navigate('/dashboard');
    } catch {
      showError('Erro de conexão');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container flex flex-col justify-center">
      <div className="text-center mb-8">
        <div className="text-5xl mb-4">🌿</div>
        <h1 className="text-3xl font-bold text-green-900">Ecoa</h1>
        <p className="text-gray-500 mt-2">Transforme ações ambientais em recompensas</p>
      </div>

      <form onSubmit={handleSubmit} className="eco-card space-y-4">
        <h2 className="text-xl font-semibold text-green-900">Entrar</h2>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input type="email" className="eco-input" value={email} onChange={e => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
          <input type="password" className="eco-input" value={password} onChange={e => setPassword(e.target.value)} required />
        </div>
        <button type="submit" className="eco-btn w-full" disabled={loading}>
          {loading ? 'Entrando...' : 'Entrar'}
        </button>
        <p className="text-center text-sm text-gray-500">
          Não tem conta? <Link to="/register" className="text-green-700 font-semibold">Cadastre-se</Link>
        </p>
      </form>
    </div>
  );
}
