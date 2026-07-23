import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import API_URL from '../api_url.js';

const SessionContext = createContext(null);

export function SessionProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('SessionJWT'));
  const [loading, setLoading] = useState(true);

  const fetchUser = useCallback(async () => {
    if (!token) {
      setLoading(false);
      return;
    }
    try {
      const res = await fetch(`${API_URL}/api/auth/me`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        const data = await res.json();
        setUser(data);
      } else {
        localStorage.removeItem('SessionJWT');
        setToken(null);
        setUser(null);
      }
    } catch {
      setUser(null);
    }
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchUser();
  }, [fetchUser]);

  useEffect(() => {
    if (!token) return;
    const interval = setInterval(fetchUser, 5 * 60 * 1000);
    return () => clearInterval(interval);
  }, [token, fetchUser]);

  const login = (newToken) => {
    localStorage.setItem('SessionJWT', newToken);
    setToken(newToken);
  };

  const logout = () => {
    localStorage.removeItem('SessionJWT');
    setToken(null);
    setUser(null);
  };

  return (
    <SessionContext.Provider value={{ user, token, loading, login, logout }}>
      {children}
    </SessionContext.Provider>
  );
}

export function useSession() {
  const ctx = useContext(SessionContext);
  if (!ctx) throw new Error('useSession must be used within SessionProvider');
  return ctx;
}
