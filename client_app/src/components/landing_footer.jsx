import { Link } from 'react-router-dom';

export default function LandingFooter() {
  return (
    <footer className="landing-footer">
      <div style={{ maxWidth: 1200, width: '100%', margin: '0 auto' }}>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-12">
          <div>
            <div className="flex flex-col items-center gap-1 mb-4">
              <span className="text-3xl leading-none">🌿</span>
              <span className="text-xl font-bold text-white">Ecoa Santos</span>
            </div>
            <p className="text-sm leading-relaxed" style={{ color: '#95d5b2' }}>
              Incentivos ambientais para uma cidade inteligente — verificados, tokenizados e auditáveis na rede Stellar. Toda ação ambiental ecoa.
            </p>
          </div>

          <div>
            <h4 className="text-white font-semibold mb-4 text-sm uppercase tracking-wide">Plataforma</h4>
            <div className="flex flex-col gap-2 text-sm">
              <Link to="/register" style={{ textDecoration: 'none' }}>Criar conta</Link>
              <Link to="/login" style={{ textDecoration: 'none' }}>Entrar</Link>
            </div>
          </div>

        </div>

        <div className="pt-8 flex flex-col md:flex-row items-center justify-between gap-4 !mt-4" style={{ borderTop: '1px solid rgba(183,228,199,0.2)' }}>
          <p className="text-sm" style={{ color: '#74c69d' }}>
            © {new Date().getFullYear()} Ecoa · Baixada Santista, SP · Brasil
          </p>
        </div>
      </div>
    </footer>
  );
}
