import { Link } from 'react-router-dom';
import LandingNavbar from '../components/landing_navbar.jsx';
import LandingFooter from '../components/landing_footer.jsx';

export default function Landing() {
  const contextCards = [
    { icon: '🚲', title: 'Ciclovias', desc: 'Pedale nas ciclovias de Santos. Cada pedalada registrada no app gera ECOA. Melhora sua saúde e a qualidade do ar da cidade.'},
  ];

  const actors = [
    {
      icon: '🌿',
      name: 'Fazedores',
      color: 'linear-gradient(135deg, #d8f3dc, #b7e4c7)',
      desc: 'Pedalam, usam VLT, ônibus, reciclam, protegem manguezal.',
      items: ['Ciclistas nas ciclovias', 'Usuários de VLT e ônibus', 'Reciclagem e coleta seletiva', 'Limpeza de praias', 'Proteção de manguezal'],
    },
    {
      icon: '🤝',
      name: 'Incentivadores',
      color: 'linear-gradient(135deg, #d4a373, #ccb087)',
      desc: 'Pessoas, empresas e órgãos públicos que financiam o pool de recompensas.',
      items: ['Pessoas físicas', 'Empresas com ESG', 'Órgãos do governo', 'Pool de recompensas', 'Descontos em parceiros'],
    },
    {
      icon: '🏪',
      name: 'Empresas parceiras',
      color: 'linear-gradient(135deg, #f4e1c1, #e8d0a3)',
      desc: 'Aceitam o token ECOA como desconto ou benefício.',
      items: ['Academias', 'Restaurantes', 'Comércio local', 'Serviços', 'Benefícios reais para fazedores'],
    },
    {
      icon: '⚙️',
      name: 'Ecoa Santos',
      color: 'linear-gradient(135deg, #a8dadc, #81d4dd)',
      desc: 'Valida a ação e conecta o mundo físico ao on-chain.',
      items: ['Validação automatica por Sistemas especialistas', 'Auditoria humana', 'Emissão na Stellar', 'Registro público auditável'],
    },
  ];

  const odsGoals = [
    { number: '13', label: 'Ação Climática' },
    { number: '14', label: 'Vida na Água' },
    { number: '11', label: 'Cidades Sustentáveis' },
    { number: '17', label: 'Parcerias' },
  ];

  return (
    <div className="landing-container">
      <LandingNavbar />

      {/* Hero */}
      <section className="landing-hero">
        <div className="landing-hero-content landing-fade-in">
          <img
            src="/logo_hero_3.png"
            alt="Ecoa Santos"
            className="!mt-8"
            style={{ width: '24rem', maxWidth: '100%', height: 'auto', margin: '0 auto 0rem', display: 'block' }}
          />
          <h1 className="landing-hero-title text-amber-600 !mt-4">Toda ação ambiental ecoa.</h1>
          <p className="landing-hero-desc">
          Transforme o que você faz pelo planeta em recompensa real, verificada e eternizada na rede Stellar.</p>
          <div className="flex gap-4 justify-center flex-wrap flex-col">
            <Link to="/register" className="eco-btn text-lg !py-[14px] !px-[32px]">
              Começar agora
            </Link>
      
            <Link to="/enterprise" className="bg-white text-black border-none rounded-xl font-semibold cursor-pointer transition-all duration-200 !py-[14px] !px-[32px] !flex !items-center !justify-center !gap-2">
              <img src="/esg_8541904.png" alt="ESG" style={{ width: '32px', height: '32px' }} />
              Para empresas
              <img src="/esg_8541904.png" alt="ESG" style={{ width: '32px', height: '32px' }} />
            </Link>

          </div>
          <div className="flex gap-8 justify-center !mt-4 flex-wrap">
            <div className="text-center">
              <div className="text-3xl font-bold" style={{ color: '#d4a373' }}>ECOA</div>
              <div className="text-sm opacity-70">Token ambiental</div>
            </div>
          </div>
        </div>
        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 text-white opacity-60 landing-float">
          <span className="text-2xl">⌄</span>
        </div>
      </section>

      {/* O Problema */}
      <section className="landing-section landing-gradient-bg" id="problema">
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Cidades inteligentes incentivam a proteção do meio ambiente e o bem estar social</h2>
          <p className="landing-section-subtitle">
            Todos os dias, milhares de ações ambientais acontecem na Baixada Santista.
            Mas essas ações raramente são incentivadas — e quase nunca chegam ao mercado de carbono.
            Não porque não tenham valor. Porque não conseguem provar esse valor.
          </p>
        </div>
        
        <div className="landing-section-narrow">
          <div className="landing-pilot-badge">MVP</div>
          <h2 className="landing-section-title">Piloto 1 — Carbono Fóssil</h2>
          <p className="landing-section-subtitle">
            Santos tem excesso de carros de passeio — principal fonte de poluição urbana e reclamação ambiental nas capitais. Uma cidade inteligente verifica seus niveis de poluição, seus picos de transito e incentiva transportes alternativos.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 ">
            {contextCards.map((card, i) => (
              <div key={i} className="landing-glass-card">
                <div className="flex items-start justify-between gap-4 mb-4">
                  <div className="flex items-center gap-3">
                    <div className="text-3xl">{card.icon}</div>
                    <h3 className="text-lg font-bold" style={{ color: '#1b4332' }}>{card.title}</h3>
                  </div>
                </div>
                <p className="text-slate-600 text-sm leading-relaxed">{card.desc}</p>
              </div>
            ))}
          </div>

          <div className="landing-glass-card text-center !mt-4" style={{ maxWidth: 800, margin: '0 auto', borderTop: '4px solid #8B4513' }}>
            <h3 className="text-xl font-bold mb-4" style={{ color: '#1b4332' }}>Impacto em múltiplas áreas</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <div className="text-3xl mb-2">🚦</div>
                <div className="font-bold text-sm mb-1" style={{ color: '#1b4332' }}>Mobilidade urbana</div>
                <p className="text-xs text-slate-500">Menos carros, trânsito mais fluido, cidade mais respirável</p>
              </div>
              <div>
                <div className="text-3xl mb-2">❤️</div>
                <div className="font-bold text-sm mb-1" style={{ color: '#1b4332' }}>Saúde pública</div>
                <p className="text-xs text-slate-500">Pedalar melhora a saúde. Ar mais limpo reduz doenças respiratórias</p>
              </div>
              <div>
                <div className="text-3xl mb-2">🌍</div>
                <div className="font-bold text-sm mb-1" style={{ color: '#1b4332' }}>Meio ambiente</div>
                <p className="text-xs text-slate-500">Menos CO₂ na atmosfera = menos carbono que os manguezais precisam sequestrar</p>
              </div>
            </div>
          </div>
        </div>

        <div className="landing-section-narrow !mt-4">
          <h2 className="landing-section-title">Piloto 2 — Carbono Azul</h2>
          <p className="landing-section-subtitle">
            Oceanos e praias limpas. Transformar resíduos em recompensa.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="landing-glass-card">
              <div className="flex items-start justify-between gap-4 mb-4">
                <div className="flex items-center gap-3">
                  <div className="text-3xl">🏖️</div>
                  <h3 className="text-lg font-bold" style={{ color: '#1b4332' }}>Limpeza de praias</h3>
                </div>
              </div>
              <p className="text-slate-600 text-sm leading-relaxed">Aproveita todos os projetos de limpeza de praia existentes e cria um incentivo para participantes validarem suas ações no Ecoa Santos.</p>
            </div>
            <div className="landing-glass-card">
              <div className="flex items-start justify-between gap-4 mb-4">
                <div className="flex items-center gap-3">
                  <div className="text-3xl">🚬</div>
                  <h3 className="text-lg font-bold" style={{ color: '#1b4332' }}>Totem de bitucas</h3>
                </div>
              </div>
              <p className="text-slate-600 text-sm leading-relaxed">Totem automático que recolhe bitucas de cigarro (Micro lixo) como MVP de totens de reciclagem, anexo à infraestrutura das praias (Santos tem condição de implementar).</p>
            </div>
          </div>

          <div className="landing-glass-card text-center !mt-4" style={{ maxWidth: 800, margin: '0 auto', borderTop: '4px solid #1b4332' }}>
            <h3 className="text-xl font-bold mb-4" style={{ color: '#1b4332' }}>Impacto em múltiplas áreas</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <div className="text-3xl mb-2">🌊</div>
                <div className="font-bold text-sm mb-1" style={{ color: '#1b4332' }}>Oceanos</div>
                <p className="text-xs text-slate-500">Menos plástico e bitucas chegam ao mar</p>
              </div>
              <div>
                <div className="text-3xl mb-2">🐢</div>
                <div className="font-bold text-sm mb-1" style={{ color: '#1b4332' }}>Vida marinha</div>
                <p className="text-xs text-slate-500">Redução de microplásticos e toxinas que afetam a fauna</p>
              </div>
              <div>
                <div className="text-3xl mb-2">🏝️</div>
                <div className="font-bold text-sm mb-1" style={{ color: '#1b4332' }}>Turismo</div>
                <p className="text-xs text-slate-500">Praias mais limpas valorizam a cidade e o turismo sustentável</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Como Funciona */}
      <section className="landing-section landing-gradient-bg" id="solucao">
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Como funciona</h2>
          <p className="landing-section-subtitle">
            Quatro atores, um fluxo transparente que conecta o mundo físico ao on-chain.
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-16">
            {actors.map((actor, i) => (
              <div key={i} className="landing-actor-card">
                <div className="landing-actor-header" style={{ background: actor.color }}>
                  <div className="text-4xl mb-3">{actor.icon}</div>
                  <h3 className="text-lg font-bold" style={{ color: '#1b4332' }}>{actor.name}</h3>
                </div>
                <div className="landing-actor-body">
                  <p className="text-slate-600 text-sm leading-relaxed mb-4">{actor.desc}</p>
                  <ul className="flex flex-col gap-2">
                    {actor.items.map((item, j) => (
                      <li key={j} className="flex items-start gap-2 text-xs text-slate-700">
                        <span style={{ color: '#40916c' }}>✓</span>
                        <span>{item}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            ))}
          </div>

        </div>
      </section>
      {/* Reconhecer, não só regular */}
      <section className="landing-section landing-gradient-bg-dark">
        <div className="landing-section-narrow text-center">
          <h2 className="text-3xl md:text-4xl font-bold mb-6" style={{ color: '#d4a373' }}>
            Reconhecer, não só regular
          </h2>
          <p className="text-lg leading-relaxed opacity-90" style={{ maxWidth: 800, margin: '0 auto' }}>
            A regulação ambiental — taxas, restrições, limites — tem seu papel. Mas tende a cobrar da mesma forma de quem já faz a sua parte e de quem ainda não faz. Nosso caminho é complementar: tornar o comportamento sustentável visível, mensurável e recompensado.
          </p>
        </div>
      </section>
      {/* Por Que Agora */}
      <section className="landing-section" id="agora" style={{ background: '#f8faf9' }}>
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Alinhado aos Objetivos de Desenvolvimento Sustentável da ONU</h2>

          <div className="text-center">
            <h3 className="text-xl font-bold mb-2" style={{ color: '#1b4332' }}>
              Alinhado aos Objetivos de Desenvolvimento Sustentável da ONU
            </h3>
            <p className="text-sm text-slate-500 mb-8">
              e aos compromissos ESG já assumidos pelas cidades da Baixada Santista
            </p>
            <div className="flex justify-center gap-4 flex-wrap">
              {odsGoals.map((g, i) => (
                <div key={i} className="landing-ods-card">
                  <div className="landing-ods-number">{g.number}</div>
                  <div className="landing-ods-label">{g.label}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* CTA Final */}
      <section className="landing-cta-section">
        <div className="landing-section-narrow" style={{ textAlign: 'center' }}>
          <div className="text-5xl mb-6" style={{ textAlign: 'center' }}>🌿</div>
          <h2 className="text-3xl md:text-4xl font-bold mb-6">Toda ação ambiental ecoa.</h2>
          <p className="text-lg opacity-90 mb-4 leading-relaxed" style={{ maxWidth: 700, margin: '0 auto' }}>
            Conectamos cidadãos, prefeituras, cooperativas e empresas numa infraestrutura transparente,
            construída sobre a Stellar.
          </p>
          <p className="text-xl font-semibold !mb-8 leading-relaxed" style={{ maxWidth: 700, margin: '0 auto', color: '#d4a373' }}>
            Estamos construindo a ponte entre quem
            protege o meio ambiente e quem quer investir num futuro sustentável.
          </p>
          <div className="flex gap-4 justify-center flex-wrap">
            <Link to="/register" className="eco-btn text-lg" style={{ textDecoration: 'none', padding: '14px 32px', background: 'linear-gradient(135deg, #d4a373, #b8896c)' }}>
              Criar conta gratuita
            </Link>
          </div>
        </div>
      </section>

      <LandingFooter />
    </div>
  );
}
