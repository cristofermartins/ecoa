import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSession } from '../session/session_provider.jsx';
import { showError } from '../toast.js';
import API_URL from '../api_url.js';

export default function Register() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [cpf, setCpf] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useSession();
  const navigate = useNavigate();

  const formatCpf = (value) => {
    const digits = value.replace(/\D/g, '').slice(0, 11);
    if (digits.length <= 3) return digits;
    if (digits.length <= 6) return `${digits.slice(0, 3)}.${digits.slice(3)}`;
    if (digits.length <= 9) return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`;
    return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const rawCpf = cpf.replace(/\D/g, '');
    if (rawCpf.length !== 11) {
      showError('CPF deve ter 11 dígitos numéricos');
      return;
    }
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, cpf: rawCpf, password })
      });
      const data = await res.json();
      if (!res.ok) {
        showError(data.error || 'Erro ao cadastrar');
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
      <div className="text-center mb-6">
        <div className="text-4xl mb-3">🌱</div>
        <h1 className="text-2xl font-bold text-green-900">Criar Conta</h1>
      </div>

      <form onSubmit={handleSubmit} className="eco-card space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
          <input type="text" className="eco-input" value={name} onChange={e => setName(e.target.value)} required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input type="email" className="eco-input" value={email} onChange={e => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">CPF</label>
          <input
            type="text"
            className="eco-input"
            value={cpf}
            onChange={e => setCpf(formatCpf(e.target.value))}
            placeholder="000.000.000-00"
            maxLength={14}
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
          <input type="password" className="eco-input" value={password} onChange={e => setPassword(e.target.value)} required />
        </div>
        <button type="submit" className="eco-btn w-full" disabled={loading}>
          {loading ? 'Cadastrando...' : 'Cadastrar'}
        </button>
        <p className="text-center text-sm text-gray-500">
          Já tem conta? <Link to="/login" className="text-green-700 font-semibold">Entrar</Link>
        </p>
      </form>
    </div>
  );
}
