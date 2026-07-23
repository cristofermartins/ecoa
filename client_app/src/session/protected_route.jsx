import { Navigate } from 'react-router-dom';
import { useSession } from './session_provider.jsx';

export default function ProtectedRoute({ children }) {
  const { user, loading } = useSession();

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-700"></div>
      </div>
    );
  }

  if (!user) return <Navigate to="/login" replace />;
  return children;
}
