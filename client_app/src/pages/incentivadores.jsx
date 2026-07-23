import { useState } from 'react';
import { Link } from 'react-router-dom';
import { showSuccess } from '../toast.js';
import LandingFooter from '../components/landing_footer.jsx';

const tiers = [
  {
    name: 'Bronze',
    icon: '🥉',
    price: 'R$ 500',
    period: '/mês',
    color: '#cd7f32',
    featured: false,
    benefits: [
      'Depósito mínimo no pool de lastro',
      'Selo Verde Bronze auditável on-chain',
      'Menção no site do Ecoa',
      'Acesso a indicadores básicos da cidade',
      '1 benefício para fazedores',
    ],
  },
  {
    name: 'Prata',
    icon: '🥈',
    price: 'R$ 2.000',
    period: '/mês',
    color: '#a8a8a8',
    featured: true,
    benefits: [
      'Tudo do plano Bronze',
      'Selo Verde Prata com destaque',
      'Logo no mural de incentivadores',
      'Até 5 benefícios para fazedores',
      'Relatório mensal de impacto ESG',
      'Conexão B2B com outros incentivadores',
    ],
  },
  {
    name: 'Ouro',
    icon: '🥇',
    price: 'R$ 5.000',
    period: '/mês',
    color: '#d4a373',
    featured: false,
    benefits: [
      'Tudo do plano Prata',
      'Selo Verde Ouro premium',
      'Logo em destaque na landing page',
      'Benefícios ilimitados para fazedores',
      'Relatório semanal de impacto + dashboard',
      'Direito a co-patrocinar metas globais da cidade',
      'Acesso prioritário a eventos e iniciativas',
    ],
  },
];

const whyReasons = [
  { icon: '🌿', title: 'Impacto ambiental real', desc: 'Seus recursos recompensam diretamente quem faz ações ambientais verificadas na sua cidade. Não é offset abstrato — é mudança local auditável.' },
  { icon: '🏆', title: 'Selos verdes auditáveis', desc: 'Cada incentivo depositado gera um registro público on-chain na Stellar. Seu compromisso ambiental é verificável por qualquer pessoa, em qualquer lugar.' },
  { icon: '📈', title: 'Relatórios ESG', desc: 'Métricas de impacto ambiental quantificáveis: CO₂ evitado, árvores plantadas, reciclagem incentivada. Dados prontos para seu relatório de sustentabilidade.' },
  { icon: '🤝', title: 'Visibilidade B2B e B2C', desc: 'Conecte-se com outras empresas incentivadoras e com milhares de fazedores que resgatam benefícios. Relação direta com a comunidade de Santos.' },
  { icon: '💎', title: 'Custo baixo, transparência alta', desc: 'Depósito via stablecoin no pool de lastro — sem intermediários. A Stellar garante rastreabilidade total, auditoria pública e queima automática de créditos.' },
  { icon: '🎯', title: 'Foco na Baixada Santista', desc: 'Incentive ações na sua região. Reciclagem em Santos, proteção de manguezais, limpeza de praia, ciclovia. Impacto onde mora o seu público.' },
];

const howSteps = [
  { icon: '📝', title: 'Cadastre sua empresa', desc: 'Crie a conta como incentivador e escolha seu plano (Bronze, Prata ou Ouro).' },
  { icon: '🏦', title: 'Deposite no pool', desc: 'Transfira stablecoin (USDC/BRZ) para o smart contract Soroban que lastrea os tokens ECOA.' },
  { icon: '🎁', title: 'Defina benefícios', desc: 'Cadastre prêmios e descontos (academia, restaurantes, serviços) que fazedores resgatam com ECOA.' },
  { icon: '📊', title: 'Acompanhe o impacto', desc: 'Veja em tempo real quantas ações foram incentivadas, CO₂ evitado e retorno para sua marca.' },
];

const seals = [
  { icon: '🥉', name: 'Selo Verde Bronze', desc: 'Compromisso inicial', color: '#cd7f32' },
  { icon: '🥈', name: 'Selo Verde Prata', desc: 'Incentivador ativo', color: '#a8a8a8' },
  { icon: '🥇', name: 'Selo Verde Ouro', desc: 'Parceiro premium', color: '#d4a373' },
  { icon: '🌍', name: 'Selo Impacto Local', desc: 'Foco regional', color: '#40916c' },
  { icon: '♻️', name: 'Selo Circular', desc: 'Economia circular', color: '#1b4965' },
  { icon: '⚡', name: 'Selo On-Chain', desc: 'Transparência total', color: '#52b788' },
];

export default function Incentivadores() {
  const [form, setForm] = useState({ company: '', contact: '', email: '', phone: '', tier: 'Prata', message: '' });
  const [loading, setLoading] = useState(false);

  const handleSubmit = (e) => {
    e.preventDefault();
    setLoading(true);
    setTimeout(() => {
      showSuccess('Recebemos seu interesse! Em breve entraremos em contato.');
      setForm({ company: '', contact: '', email: '', phone: '', tier: 'Prata', message: '' });
      setLoading(false);
    }, 1200);
  };

  return (
    <div className="landing-container">
      {/* Navbar simples */}
      <nav
        className="fixed top-0 left-0 right-0 z-50"
        style={{
          background: 'rgba(255,255,255,0.92)',
          backdropFilter: 'blur(12px)',
          WebkitBackdropFilter: 'blur(12px)',
          boxShadow: '0 2px 20px rgba(0,0,0,0.08)',
          borderBottom: '1px solid rgba(45,106,79,0.1)',
        }}
      >
        <div className="flex items-center justify-between px-6 py-4" style={{ maxWidth: 1200, margin: '0 auto' }}>
          <Link to="/" className="flex items-center gap-2" style={{ textDecoration: 'none' }}>
            <span className="text-2xl">🌿</span>
            <span className="text-xl font-bold" style={{ color: '#1b4332' }}>Ecoa</span>
          </Link>
          <div className="flex items-center gap-3">
            <Link to="/" className="font-medium text-slate-600 hover:text-green-700" style={{ textDecoration: 'none', fontSize: 14 }}>
              ← Voltar ao site
            </Link>
            <Link to="/register" className="eco-btn text-sm" style={{ textDecoration: 'none', padding: '8px 20px' }}>
              Cadastrar
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero Incentivadores */}
      <section className="landing-hero" style={{ paddingTop: 100, minHeight: '80vh' }}>
        <div className="landing-hero-content landing-fade-in">
          <div className="flex justify-center gap-3 mb-6 flex-wrap">
            <div className="landing-badge-stellar"><span>🏢</span> Para Empresas</div>
            <div className="landing-badge-stellar"><span>🏅</span> Selos Verdes</div>
            <div className="landing-badge-stellar"><span>📊</span> ESG Auditável</div>
          </div>
          <h1>Sua empresa como agente de mudança ambiental</h1>
          <p>
            Incentive ações ambientais verificadas na Baixada Santista, ganhe selos verdes auditáveis on-chain
            e conecte-se com a comunidade. Recompense quem faz — não pune quem não faz.
          </p>
          <div className="flex gap-4 justify-center flex-wrap">
            <button onClick={() => document.getElementById('contato')?.scrollIntoView({ behavior: 'smooth' })} className="eco-btn text-lg" style={{ padding: '14px 32px' }}>
              Quero ser incentivador
            </button>
            <button onClick={() => document.getElementById('planos')?.scrollIntoView({ behavior: 'smooth' })} className="eco-btn-outline text-lg" style={{ padding: '12px 30px', color: 'white', borderColor: 'white' }}>
              Ver planos
            </button>
          </div>
        </div>
      </section>

      {/* Por que ser incentivador */}
      <section className="landing-section landing-gradient-bg">
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Por que ser incentivador?</h2>
          <p className="landing-section-subtitle">
            Mais do que compensar emissões — sua empresa financia mudança real e verificável na cidade onde atua.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {whyReasons.map((r, i) => (
              <div key={i} className="landing-feature-card">
                <div className="landing-icon-circle">{r.icon}</div>
                <h3 className="text-lg font-bold mb-2" style={{ color: '#1b4332' }}>{r.title}</h3>
                <p className="text-slate-600 text-sm leading-relaxed">{r.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Como funciona */}
      <section className="landing-section" style={{ background: '#f8faf9' }}>
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Como funciona para empresas</h2>
          <p className="landing-section-subtitle">
            Quatro passos para sua empresa começar a incentivar ações ambientais com transparência total.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            {howSteps.map((step, i) => (
              <div key={i} className="text-center">
                <div className="landing-feature-card" style={{ height: '100%' }}>
                  <div className="text-4xl mb-3">{step.icon}</div>
                  <div className="text-xs font-bold mb-2" style={{ color: '#40916c' }}>PASSO {i + 1}</div>
                  <h3 className="text-base font-bold mb-2" style={{ color: '#1b4332' }}>{step.title}</h3>
                  <p className="text-slate-600 text-sm leading-relaxed">{step.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Selos e Reconhecimento */}
      <section className="landing-section landing-gradient-bg">
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Selos e reconhecimento</h2>
          <p className="landing-section-subtitle">
            Cada selo é um registro público na Stellar — não marketing verde. É compromisso auditável.
          </p>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            {seals.map((seal, i) => (
              <div key={i} className="landing-feature-card text-center" style={{ padding: 24 }}>
                <div className="text-4xl mb-3">{seal.icon}</div>
                <h4 className="font-bold text-sm mb-1" style={{ color: seal.color }}>{seal.name}</h4>
                <p className="text-xs text-slate-500">{seal.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Planos / Tiers */}
      <section className="landing-section" id="planos" style={{ background: '#f8faf9' }}>
        <div className="landing-section-narrow">
          <h2 className="landing-section-title">Planos para incentivadores</h2>
          <p className="landing-section-subtitle">
            Escolha o nível de compromisso da sua empresa. Todos os planos geram selos verdes auditáveis.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {tiers.map((tier, i) => (
              <div key={i} className={`landing-tier-card ${tier.featured ? 'featured' : ''}`}>
                {tier.featured && (
                  <div className="landing-tier-badge mb-4" style={{ background: '#d4a373', color: 'white' }}>
                    Mais popular
                  </div>
                )}
                <div className="text-4xl mb-2">{tier.icon}</div>
                <h3 className="text-xl font-bold mb-1" style={{ color: tier.color }}>{tier.name}</h3>
                <div className="landing-tier-price">
                  {tier.price}<span className="text-base font-normal text-slate-500">{tier.period}</span>
                </div>
                <div className="flex flex-col gap-3 text-left mt-6">
                  {tier.benefits.map((b, j) => (
                    <div key={j} className="flex items-start gap-2 text-sm text-slate-700">
                      <span style={{ color: tier.color, fontWeight: 'bold' }}>✓</span>
                      <span>{b}</span>
                    </div>
                  ))}
                </div>
                <button
                  onClick={() => document.getElementById('contato')?.scrollIntoView({ behavior: 'smooth' })}
                  className="w-full mt-8 py-3 rounded-xl font-semibold transition-all"
                  style={{
                    background: tier.featured ? `linear-gradient(135deg, ${tier.color}, #b8896c)` : 'transparent',
                    color: tier.featured ? 'white' : tier.color,
                    border: tier.featured ? 'none' : `2px solid ${tier.color}`,
                    cursor: 'pointer',
                  }}
                >
                  Escolher {tier.name}
                </button>
              </div>
            ))}
          </div>
          <p className="text-center text-sm text-slate-500 mt-8">
            Os valores depositados vão integralmente para o pool de lastro que recompensa os fazedores.
            A operação da plataforma é custeada separadamente.
          </p>
        </div>
      </section>

      {/* Empresas parceiras (placeholder) */}
      <section className="landing-section landing-gradient-bg">
        <div className="landing-section-narrow text-center">
          <h2 className="landing-section-title">Empresas que já incentivam</h2>
          <p className="landing-section-subtitle">
            Juntas, essas empresas estão financiando a transição ambiental de Santos.
          </p>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            {[1, 2, 3, 4, 5, 6, 7, 8].map((n) => (
              <div key={n} className="landing-glass-card flex items-center justify-center" style={{ height: 100, padding: 16 }}>
                <span className="text-slate-400 font-bold text-lg">Empresa {n}</span>
              </div>
            ))}
          </div>
          <p className="text-sm text-slate-400 mt-6">Seja a próxima empresa a aparecer aqui.</p>
        </div>
      </section>

      {/* Formulário de contato */}
      <section className="landing-section landing-gradient-bg-dark" id="contato">
        <div className="landing-section-narrow">
          <div className="text-center mb-10">
            <div className="text-5xl mb-4" style={{ textAlign: 'center' }}>🤝</div>
            <h2 className="text-3xl md:text-4xl font-bold mb-4" style={{ color: '#d4a373' }}>
              Quero ser incentivador
            </h2>
            <p className="text-lg opacity-90 leading-relaxed" style={{ maxWidth: 700, margin: '0 auto' }}>
              Preencha o formulário e nossa equipe entrará em contato para estruturar
              o incentivo da sua empresa no pool de lastro do Ecoa.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="landing-glass" style={{ background: 'rgba(255,255,255,0.1)', maxWidth: 700, width: '100%', margin: '0 auto' }}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium text-white mb-1.5">Nome da empresa *</label>
                <input
                  type="text"
                  className="landing-contact-input"
                  value={form.company}
                  onChange={e => setForm({ ...form, company: e.target.value })}
                  placeholder="Ex: Acme Ltda."
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-white mb-1.5">Pessoa de contato *</label>
                <input
                  type="text"
                  className="landing-contact-input"
                  value={form.contact}
                  onChange={e => setForm({ ...form, contact: e.target.value })}
                  placeholder="Nome completo"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-white mb-1.5">Email *</label>
                <input
                  type="email"
                  className="landing-contact-input"
                  value={form.email}
                  onChange={e => setForm({ ...form, email: e.target.value })}
                  placeholder="contato@empresa.com"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-white mb-1.5">Telefone</label>
                <input
                  type="tel"
                  className="landing-contact-input"
                  value={form.phone}
                  onChange={e => setForm({ ...form, phone: e.target.value })}
                  placeholder="(13) 99999-9999"
                />
              </div>
            </div>
            <div className="mb-4">
              <label className="block text-sm font-medium text-white mb-1.5">Plano de interesse</label>
              <select
                className="landing-contact-input"
                value={form.tier}
                onChange={e => setForm({ ...form, tier: e.target.value })}
              >
                <option value="Bronze">Bronze — R$ 500/mês</option>
                <option value="Prata">Prata — R$ 2.000/mês</option>
                <option value="Ouro">Ouro — R$ 5.000/mês</option>
                <option value="Custom">Personalizado / Quero conversar</option>
              </select>
            </div>
            <div className="mb-6">
              <label className="block text-sm font-medium text-white mb-1.5">Mensagem</label>
              <textarea
                className="landing-contact-input"
                rows={4}
                value={form.message}
                onChange={e => setForm({ ...form, message: e.target.value })}
                placeholder="Conte-nos sobre o interesse da sua empresa..."
              />
            </div>
            <button
              type="submit"
              className="eco-btn w-full text-lg"
              disabled={loading}
              style={{ background: 'linear-gradient(135deg, #d4a373, #b8896c)', padding: '14px' }}
            >
              {loading ? 'Enviando...' : 'Enviar interesse'}
            </button>
            <p className="text-center text-xs mt-4 opacity-60">
              Seus dados não serão compartilhados. Resposta em até 2 dias úteis.
            </p>
          </form>
        </div>
      </section>

      <LandingFooter />
    </div>
  );
}