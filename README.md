<p align="center">
  <img src="mvp/client_app/public/logo_hero_3.png" alt="Ecoa Santos" width="320" />
</p>

<p align="center">
  <strong>Toda ação ambiental ecoa.</strong>
</p>

<p align="center">
  Transforme o que você faz pelo planeta em recompensa real, verificada e eternizada na rede Stellar.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/status-MVP-green" alt="MVP" />
  <img src="https://img.shields.io/badge/frontend-React_19-61DAFB?logo=react" alt="React" />
  <img src="https://img.shields.io/badge/backend-.NET_9-512BD4?logo=dotnet" alt=".NET" />
  <img src="https://img.shields.io/badge/blockchain-Stellar-7D00FF?logo=stellar" alt="Stellar" />
  <img src="https://img.shields.io/badge/contracts-Soroban_(Rust)-000?logo=rust" alt="Soroban" />
  <img src="https://img.shields.io/badge/mobile-Capacitor-119EFF?logo=capacitor" alt="Capacitor" />
</p>

---

## O problema

Todos os dias, milhares de ações ambientais acontecem na Baixada Santista — pessoas pedalando nas ciclovias, usando VLT e ônibus, reciclando, limpando praias, protegendo manguezais.

Mas essas ações raramente são incentivadas. Elas quase nunca chegam ao mercado de carbono. **Não porque não tenham valor. Porque não conseguem provar esse valor.**

Nosso ângulo é **incentivar**, não punir. A sociedade pode direcionar a si mesma, recompensando aquilo que acredita ser melhor para todos e para o meio ambiente.

---

## A solução

Quatro atores, um fluxo transparente que conecta o mundo físico ao on-chain.

| Ator | Papel |
|---|---|
| 🌿 **Fazedores** | Executam ações ambientais: pedalam, usam VLT/ônibus, reciclam, protegem manguezais |
| 🤝 **Incentivadores** | Pessoas, empresas e órgãos públicos que financiam o pool de recompensas |
| 🏪 **Empresas parceiras** | Aceitam o token ECOA como desconto ou benefício (academias, restaurantes, comércio local) |
| ⚙️ **Ecoa Santos (Oracle)** | Valida a ação e conecta o mundo físico ao on-chain |

### Fluxo

```
Fazedor executa ação ambiental
        ↓
App/Plataforma registra e comprova (GPS, foto, check-in)
        ↓
Ecoa Santos (Oracle) valida — sistemas especialistas + auditoria humana
        ↓
Soroban emite ECOA automaticamente na Stellar
        ↓
Fazedor resgata por stablecoin (Pix) ou benefício em parceiro
```

---

## Pilotos (MVP)

### Piloto 1 — Carbono Fóssil

Santos tem excesso de carros de passeio — principal fonte de poluição urbana. Uma cidade inteligente verifica seus níveis de poluição, seus picos de trânsito e incentiva transportes alternativos.

| 🚲 Ciclovias | 🚦 Mobilidade urbana | ❤️ Saúde pública | 🌍 Meio ambiente |
|---|---|---|---|
| Pedale nas ciclovias de Santos. Cada pedalada registrada gera ECOA | Menos carros, trânsito mais fluido, cidade mais respirável | Pedalar melhora a saúde. Ar mais limpo reduz doenças respiratórias | Menos CO₂ na atmosfera = menos carbono que os manguezais precisam sequestrar |

### Piloto 2 — Carbono Azul

Oceanos e praias limpas. Transformar resíduos em recompensa.

| 🏖️ Limpeza de praias | 🚬 Totem de bitucas |
|---|---|
| Aproveita projetos de limpeza existentes e cria incentivo para participantes validarem suas ações | Totem automático que recolhe bitucas como MVP de totens de reciclagem |

| 🌊 Oceanos | 🐢 Vida marinha | 🏝️ Turismo |
|---|---|---|
| Menos plástico e bitucas chegam ao mar | Redução de microplásticos e toxinas | Praias mais limpas valorizam a cidade e o turismo sustentável |

---

## Tokenomics

| Parâmetro | Valor |
|---|---|
| **Token** | EcoaSantos (ECOA) — custom asset na Stellar |
| **Lastro** | Incentivadores depositam stablecoin (USDC/BRZ) ou benefícios em smart contract Soroban |
| **Proporção** | 1 ECOA ≈ R$ 1,00 (a definir) |
| **Resgate** | Queimar ECOA → receber stablecoin (Pix) |
| | Transferir ECOA para empresa parceira → benefício/desconto |

> **Pix resolve pagamento.** Grátis, instantâneo, nacional.  
> **Stellar resolve confiança.** Emissão rápida e barata de ativos ambientais, com registro público que um comprador internacional audita sozinho.  
> Um cuida do dinheiro. O outro cuida da prova.

---

## Alinhamento ODS

<p align="center">
  <img src="https://img.shields.io/badge/ODS_13-Ação_Climática-3F7E44?style=for-the-badge" alt="ODS 13" />
  <img src="https://img.shields.io/badge/ODS_14-Vida_na_Água-0A97D9?style=for-the-badge" alt="ODS 14" />
  <img src="https://img.shields.io/badge/ODS_11-Cidades_Sustentáveis-F99D26?style=for-the-badge" alt="ODS 11" />
  <img src="https://img.shields.io/badge/ODS_17-Parcerias-19486A?style=for-the-badge" alt="ODS 17" />
</p>

Alinhado aos Objetivos de Desenvolvimento Sustentável da ONU e aos compromissos ESG já assumidos pelas cidades da Baixada Santista.

---

## Tech Stack

| Camada | Tecnologia |
|---|---|
| **Frontend web** | React 19, Vite, Tailwind CSS 4 |
| **Mobile** | Capacitor (Android/iOS), Ionic |
| **Mapas** | Leaflet + React-Leaflet |
| **Backend** | C# .NET 9, Entity Framework Core |
| **Banco de dados** | MySQL (Pomelo) |
| **Autenticação** | JWT |
| **Blockchain** | Stellar + Soroban (Rust SDK → WASM) |
| **Notificações** | React Hot Toast |

---

## Estrutura do projeto

```
ecoa/
├── mvp/
│   ├── client_app/          # Frontend React + Capacitor
│   │   ├── src/
│   │   │   ├── pages/       # landing, register, login, dashboard...
│   │   │   ├── components/  # Navbar, Footer, cards...
│   │   │   └── session/     # Auth context
│   │   └── public/
│   ├── Controllers/         # API REST (.NET)
│   ├── Core/                # Domínio, serviços, portas
│   ├── Infrastructure/      # EF Core, Stellar SDK, repositórios
│   ├── Contracts/           # Smart contracts Soroban (Rust)
│   └── Program.cs           # Entry point
├── ecoa_spec/               # Documentação e especificação
└── README.md
```

---

## Rodando localmente

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [MySQL](https://dev.mysql.com/downloads/)

### Backend

```bash
cd mvp

# Configure a connection string em appsettings.Development.json
# "MySQLConnectionString": "server=localhost;database=ecoa;user=root;password=..."

dotnet run
```

### Frontend

```bash
cd mvp/client_app

npm install
npm run dev
```

O app estará disponível em `http://localhost:5170` (dev) ou via backend em `http://localhost:5270`.

---

## Modelo de negócio

A **Ecoa Santos** atua como camada de confiança e validação — o elo entre o mundo físico (ações ambientais) e o mundo on-chain (emissão de tokens).

- **O que oferece:** plataforma de verificação que transforma ações climáticas de pequena escala em tokens ambientais carbono emitidos como ativo na Stellar, fracionável e com histórico público auditável.
- **Quem paga:** pool de incentivadores.

---

<p align="center">
  <em>Conectamos cidadãos, prefeituras, cooperativas e empresas numa infraestrutura transparente, construída sobre a Stellar.</em>
</p>

<p align="center">
  <strong>Estamos construindo a ponte entre quem protege o meio ambiente e quem quer investir num futuro sustentável.</strong>
</p>
