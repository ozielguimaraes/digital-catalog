// screens-1.jsx — Login, Home, Catálogos (lista), Detalhe de catálogo
// Todas as telas recebem (go) para navegação interna

// ─────────────────────────────────────────────────────────
// LOGIN
// ─────────────────────────────────────────────────────────
function LoginScreen({ go }) {
  return (
    <Screen hasBottomNav={false} noPadding bg="var(--antracito)">
      {/* bg pattern */}
      <div style={{ position: 'absolute', inset: 0, opacity: 0.08,
        backgroundImage: `url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40'><g fill='none' stroke='%23ede8e2' stroke-width='1'><circle cx='20' cy='20' r='4'/><path d='M16 20v6M24 20v6'/></g></svg>")`,
      }}/>
      <div style={{ padding: '100px 28px 28px', position: 'relative', display: 'flex', flexDirection: 'column', height: '100%', color: '#fff' }}>
        <LogoLockup dark size="lg"/>
        <div style={{ fontFamily: 'var(--serif)', fontStyle: 'italic', fontSize: 13, opacity: 0.75, marginTop: 6, letterSpacing: '0.04em' }}>Leve com você</div>

        <div style={{ marginTop: 56, flex: 1 }}>
          <h2 style={{ color: '#fff', fontSize: 32, lineHeight: 1.15, fontStyle: 'italic', fontWeight: 400 }}>
            Gerencie seu<br/>ateliê <span style={{ color: 'var(--copper-z)' }}>com cuidado</span>.
          </h2>
          <p style={{ opacity: 0.7, fontSize: 14, marginTop: 14, lineHeight: 1.55 }}>
            Catálogos, pedidos, clientes e financeiro num só lugar.
          </p>

          <div style={{ marginTop: 40, display: 'flex', flexDirection: 'column', gap: 14 }}>
            <div>
              <label style={{ color: 'rgba(255,255,255,0.55)' }}>E-mail</label>
              <input type="email" defaultValue="sany@atelie.com" style={{ background: 'rgba(255,255,255,0.08)', color: '#fff', borderColor: 'rgba(255,255,255,0.15)' }}/>
            </div>
            <div>
              <label style={{ color: 'rgba(255,255,255,0.55)' }}>Senha</label>
              <input type="password" defaultValue="••••••••" style={{ background: 'rgba(255,255,255,0.08)', color: '#fff', borderColor: 'rgba(255,255,255,0.15)' }}/>
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 24 }}>
          <button className="btn btn-primary btn-lg" onClick={() => go('home')} style={{ width: '100%' }}>
            Entrar <I.arrowRight size={16}/>
          </button>
          <button className="btn" style={{ color: 'rgba(255,255,255,0.7)', background: 'transparent', width: '100%' }}>
            Criar conta
          </button>
        </div>
      </div>
    </Screen>
  );
}

// ─────────────────────────────────────────────────────────
// HOME / Dashboard
// ─────────────────────────────────────────────────────────
function HomeScreen({ go }) {
  return (
    <Screen>
      <AppHeader
        title="Bom dia, Sany"
        subtitle="Terça, 14 de abril"
        actions={<><button className="btn btn-icon"><I.search size={18}/></button><button className="btn btn-icon" style={{ position: 'relative' }}><I.bell size={18}/><span style={{ position: 'absolute', top: 8, right: 9, width: 7, height: 7, borderRadius: 99, background: 'var(--copper-sany)' }}/></button></>}
      />

      {/* Card principal — resumo do mês */}
      <div style={{ padding: '4px 20px 16px' }}>
        <div style={{
          borderRadius: 22, padding: 22, color: '#fff',
          background: 'var(--copper-grad)',
          position: 'relative', overflow: 'hidden',
          boxShadow: '0 14px 32px rgba(138,71,42,0.3)',
        }}>
          {/* decorative */}
          <svg viewBox="0 0 200 200" style={{ position: 'absolute', right: -30, top: -20, width: 180, opacity: 0.15 }}>
            <path d="M 65 60 C 65 30, 135 30, 135 60 L 155 170 L 45 170 Z" stroke="#fff" strokeWidth="3" fill="none"/>
          </svg>
          <div style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.12em', opacity: 0.85 }}>RECEITA — ABRIL</div>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 36, marginTop: 6, fontWeight: 500 }}>R$ 8.420,<span style={{ fontSize: 20 }}>00</span></div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, marginTop: 4, opacity: 0.9 }}>
            <I.trend size={14}/> +18% vs. mar
          </div>
          <div style={{ display: 'flex', gap: 18, marginTop: 18, paddingTop: 14, borderTop: '1px solid rgba(255,255,255,0.18)' }}>
            <Stat label="Pedidos" value="12"/>
            <Stat label="A receber" value="R$ 1.746"/>
            <Stat label="A pagar" value="R$ 3.782"/>
          </div>
        </div>
      </div>

      {/* Atalhos */}
      <div style={{ padding: '4px 20px 8px', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
        <QuickCard icon={I.plus} label="Novo produto" onClick={() => go('productForm')}/>
        <QuickCard icon={I.bag} label="Novo pedido" onClick={() => go('orders')}/>
        <QuickCard icon={I.users} label="Novo cliente" onClick={() => go('clients')}/>
        <QuickCard icon={I.share} label="Compartilhar catálogo" onClick={() => go('catalogs')}/>
      </div>

      {/* Pedidos em produção */}
      <SectionTitle title="Em produção" action="Ver todos" onAction={() => go('orders')}/>
      <div style={{ display: 'flex', gap: 12, overflowX: 'auto', padding: '0 20px 4px', scrollbarWidth: 'none' }}>
        {ORDERS.filter(o => o.status === 'production' || o.status === 'ready').map(o => (
          <div key={o.id} className="card" style={{ minWidth: 220, padding: 14, flexShrink: 0 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <div className="caps">{o.id}</div>
              <span className={'chip ' + STATUS_LABEL[o.status].chip}>{STATUS_LABEL[o.status].label}</span>
            </div>
            <div style={{ fontFamily: 'var(--serif)', fontSize: 17, marginTop: 10, fontStyle: 'italic' }}>{o.client}</div>
            <div className="muted" style={{ fontSize: 12, marginTop: 2 }}>{o.items} {o.items>1?'itens':'item'} • {o.date}</div>
            <div style={{ fontFamily: 'var(--serif)', fontSize: 20, marginTop: 10, color: 'var(--copper-brown)' }}>{brl(o.total)}</div>
          </div>
        ))}
      </div>

      {/* Catálogo em destaque */}
      <SectionTitle title="Catálogo favorito" action="Ver tudo" onAction={() => go('catalogs')}/>
      <div style={{ padding: '0 20px' }}>
        <div className="card" onClick={() => go('catalog')} style={{ padding: 0, overflow: 'hidden', cursor: 'pointer' }}>
          <div style={{ height: 130, position: 'relative' }}>
            <BagIllustration swatchIndex={0}/>
            <div style={{ position: 'absolute', top: 12, right: 12, width: 32, height: 32, borderRadius: 99, background: 'rgba(255,255,255,0.9)', backdropFilter: 'blur(10px)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--copper-sany)' }}>
              <I.starFill size={14}/>
            </div>
          </div>
          <div style={{ padding: 14 }}>
            <div style={{ fontFamily: 'var(--serif)', fontStyle: 'italic', fontSize: 18 }}>Coleção Outono 2026</div>
            <div className="muted" style={{ fontSize: 12, marginTop: 4, display: 'flex', gap: 14 }}>
              <span>24 produtos</span><span>•</span><span>318 visualizações</span>
            </div>
          </div>
        </div>
      </div>

      {/* Atividades recentes */}
      <SectionTitle title="Atividade recente"/>
      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 2 }}>
        {[
          { icon: I.bag, label: 'Pedido #1248 confirmado', sub: 'Marina Castello • R$ 489', time: '2h' },
          { icon: I.eye, label: 'Catálogo Outono — 12 visitas', sub: 'Pico hoje às 14h', time: '3h' },
          { icon: I.coin, label: 'Pagamento recebido', sub: 'Beatriz Moraes — R$ 928', time: 'ontem' },
        ].map((a, i) => (
          <div key={i} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderBottom: i < 2 ? '1px solid var(--line-soft)' : 'none' }}>
            <div style={{ width: 36, height: 36, borderRadius: 10, background: 'rgba(192,102,71,0.1)', color: 'var(--copper-sany)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <a.icon size={18}/>
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: 14, fontWeight: 700 }}>{a.label}</div>
              <div className="muted" style={{ fontSize: 12 }}>{a.sub}</div>
            </div>
            <div className="muted" style={{ fontSize: 11 }}>{a.time}</div>
          </div>
        ))}
      </div>
    </Screen>
  );
}

function Stat({ label, value }) {
  return (
    <div style={{ flex: 1 }}>
      <div style={{ fontSize: 10, opacity: 0.75, letterSpacing: '0.1em', fontWeight: 700 }}>{label.toUpperCase()}</div>
      <div style={{ fontFamily: 'var(--serif)', fontSize: 16, marginTop: 3, fontWeight: 500 }}>{value}</div>
    </div>
  );
}

function QuickCard({ icon: Ic, label, onClick }) {
  return (
    <button onClick={onClick} className="card" style={{
      border: '1px solid var(--line-soft)', background: '#fff',
      padding: 14, display: 'flex', alignItems: 'center', gap: 10,
      textAlign: 'left', cursor: 'pointer',
    }}>
      <div style={{ width: 36, height: 36, borderRadius: 10, background: 'var(--alabaster)', color: 'var(--copper-sany)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Ic size={18}/>
      </div>
      <div style={{ fontSize: 13, fontWeight: 700 }}>{label}</div>
    </button>
  );
}

function SectionTitle({ title, action, onAction }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', padding: '18px 20px 12px' }}>
      <h3 style={{ fontSize: 18, fontStyle: 'italic', fontWeight: 500 }}>{title}</h3>
      {action && <button onClick={onAction} style={{ background: 'none', border: 'none', color: 'var(--copper-sany)', fontSize: 12, fontWeight: 700, letterSpacing: '0.04em', cursor: 'pointer' }}>{action}</button>}
    </div>
  );
}

// ─────────────────────────────────────────────────────────
// CATÁLOGOS — lista
// ─────────────────────────────────────────────────────────
function CatalogsScreen({ go }) {
  return (
    <Screen>
      <AppHeader
        title="Catálogos"
        subtitle="3 catálogos • 51 produtos"
        actions={<button className="btn btn-icon" style={{ background: 'var(--copper-sany)', color: '#fff', borderColor: 'var(--copper-sany)' }}><I.plus size={18}/></button>}
      />

      {/* Tabs */}
      <div style={{ padding: '0 20px 12px', display: 'flex', gap: 8 }}>
        <Pill active>Todos</Pill>
        <Pill>Favoritos</Pill>
        <Pill>Arquivados</Pill>
      </div>

      {/* Lista */}
      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 14 }}>
        {CATALOGS.map(c => (
          <div key={c.id} className="card" onClick={() => go('catalog')} style={{ padding: 0, overflow: 'hidden', cursor: 'pointer' }}>
            <div style={{ display: 'flex', gap: 0 }}>
              <div style={{ width: 110, height: 110, flexShrink: 0 }}>
                <BagIllustration swatchIndex={c.cover}/>
              </div>
              <div style={{ flex: 1, minWidth: 0, padding: '14px 14px 14px 16px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between', gap: 8 }}>
                <div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', gap: 8 }}>
                    <div style={{ fontFamily: 'var(--serif)', fontSize: 16, fontStyle: 'italic', lineHeight: 1.25, flex: 1, minWidth: 0 }}>{c.name}</div>
                    <span style={{ color: c.favorite ? 'var(--copper-sany)' : 'var(--line)', flexShrink: 0 }}>
                      {c.favorite ? <I.starFill size={18}/> : <I.star size={18}/>}
                    </span>
                  </div>
                  <div className="muted" style={{ fontSize: 12, marginTop: 4 }}>Atualizado {c.updated}</div>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: 12 }}>
                  <div style={{ display: 'flex', gap: 12, color: 'var(--muted)' }}>
                    <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}><I.box size={12}/> {c.products}</span>
                    <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}><I.eye size={12}/> {c.views}</span>
                  </div>
                  <button className="btn btn-sm btn-ghost" style={{ padding: '5px 9px' }} onClick={(e) => { e.stopPropagation(); go('public'); }}>
                    <I.share size={12}/>
                  </button>
                </div>
              </div>
            </div>
          </div>
        ))}

        {/* Add */}
        <button onClick={() => go('catalogForm')} style={{
          border: '2px dashed var(--line)', background: 'transparent',
          borderRadius: 18, padding: 20, cursor: 'pointer',
          display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 10,
          color: 'var(--muted)', fontSize: 14, fontWeight: 700,
        }}>
          <I.plus size={18}/> Criar novo catálogo
        </button>
      </div>
    </Screen>
  );
}

function Pill({ children, active }) {
  return (
    <button style={{
      padding: '7px 14px', borderRadius: 99,
      border: '1px solid ' + (active ? 'var(--antracito)' : 'var(--line)'),
      background: active ? 'var(--antracito)' : 'transparent',
      color: active ? '#fff' : 'var(--ink)',
      fontSize: 12, fontWeight: 700, letterSpacing: '0.04em',
      cursor: 'pointer',
    }}>{children}</button>
  );
}

// ─────────────────────────────────────────────────────────
// CATÁLOGO — detalhe com produtos
// ─────────────────────────────────────────────────────────
function CatalogScreen({ go }) {
  return (
    <Screen>
      <AppHeader
        title="Coleção Outono"
        onBack={() => go('catalogs')}
        compact
        actions={<><button className="btn btn-icon"><I.share size={18}/></button><button className="btn btn-icon"><I.more size={18}/></button></>}
      />

      {/* Cover hero */}
      <div style={{ padding: '0 20px 14px' }}>
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <div style={{ height: 160, position: 'relative' }}>
            <BagIllustration swatchIndex={0}/>
            <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(180deg, transparent 40%, rgba(51,42,39,0.85))' }}/>
            <div style={{ position: 'absolute', bottom: 14, left: 16, right: 16, color: '#fff' }}>
              <div style={{ fontFamily: 'var(--serif)', fontStyle: 'italic', fontSize: 24, fontWeight: 500 }}>Coleção Outono 2026</div>
              <div style={{ fontSize: 12, opacity: 0.85, marginTop: 3 }}>Tons quentes, couros selecionados</div>
            </div>
            <button style={{ position: 'absolute', top: 12, right: 12, width: 36, height: 36, borderRadius: 99, border: 'none', background: 'rgba(255,255,255,0.92)', color: 'var(--copper-sany)', cursor: 'pointer' }}>
              <I.starFill size={16}/>
            </button>
          </div>
          <div style={{ padding: 14, display: 'flex', gap: 12, borderTop: '1px solid var(--line-soft)' }}>
            <MiniStat label="Produtos" value="24"/>
            <div style={{ width: 1, background: 'var(--line-soft)' }}/>
            <MiniStat label="Visitas" value="318"/>
            <div style={{ width: 1, background: 'var(--line-soft)' }}/>
            <MiniStat label="Favoritado" value="42×"/>
          </div>
        </div>
      </div>

      {/* Barra de compartilhamento */}
      <div style={{ padding: '0 20px 14px', display: 'flex', gap: 10 }}>
        <button className="btn btn-primary" style={{ flex: 1 }}>
          <I.share size={14}/> Compartilhar link
        </button>
        <button className="btn btn-ghost btn-icon" style={{ width: 44, height: 44 }}><I.qr size={18}/></button>
        <button className="btn btn-ghost btn-icon" style={{ width: 44, height: 44 }}><I.eye size={18}/></button>
      </div>

      {/* Filters */}
      <div style={{ padding: '0 20px 10px', display: 'flex', gap: 8, alignItems: 'center' }}>
        <div style={{ flex: 1 }}>
          <SearchInput placeholder="Buscar produto…" defaultValue="Aurora"/>
        </div>
        <button className="btn btn-ghost btn-icon" style={{ width: 44, height: 44, flexShrink: 0 }}><I.filter size={16}/></button>
      </div>

      {/* Grid produtos */}
      <div style={{ padding: '4px 20px', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
        {PRODUCTS.slice(0, 6).map(p => <ProductCard key={p.id} p={p} onClick={() => go('product')}/>)}
      </div>

      {/* FAB */}
      <button onClick={() => go('productForm')} style={{
        position: 'absolute', bottom: 94, right: 18, zIndex: 30,
        width: 52, height: 52, borderRadius: 99, border: 'none',
        background: 'var(--copper-sany)', color: '#fff', cursor: 'pointer',
        boxShadow: '0 10px 24px rgba(138,71,42,0.35)',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
      }}>
        <I.plus size={22}/>
      </button>
    </Screen>
  );
}

function MiniStat({ label, value }) {
  return (
    <div style={{ flex: 1 }}>
      <div className="caps" style={{ fontSize: 10 }}>{label}</div>
      <div style={{ fontFamily: 'var(--serif)', fontSize: 16, fontWeight: 500, marginTop: 2 }}>{value}</div>
    </div>
  );
}

function ProductCard({ p, onClick }) {
  const st = p.status === 'oos' ? { chip: 'chip-red', label: 'Esgotado' } :
             p.status === 'custom' ? { chip: 'chip-copper', label: 'Sob encomenda' } :
             { chip: 'chip-green', label: `${p.stock} un` };
  return (
    <div onClick={onClick} style={{ cursor: 'pointer' }}>
      <div className="card" style={{ padding: 0, overflow: 'hidden', border: '1px solid var(--line-soft)' }}>
        <div style={{ aspectRatio: '1 / 1', position: 'relative' }}>
          <BagIllustration swatchIndex={p.swatch}/>
          <span className={'chip ' + st.chip} style={{ position: 'absolute', top: 8, left: 8, fontSize: 9 }}>{st.label}</span>
          {p.favorite && <div style={{ position: 'absolute', top: 8, right: 8, color: 'var(--copper-sany)' }}><I.starFill size={16}/></div>}
        </div>
      </div>
      <div style={{ padding: '8px 2px 0' }}>
        <div style={{ fontFamily: 'var(--serif)', fontSize: 14, fontStyle: 'italic', lineHeight: 1.2 }}>{p.name}</div>
        <div style={{ fontSize: 13, fontWeight: 700, marginTop: 2, color: 'var(--copper-brown)' }}>{brl(p.price)}</div>
      </div>
    </div>
  );
}

Object.assign(window, { LoginScreen, HomeScreen, CatalogsScreen, CatalogScreen, ProductCard, Pill, SectionTitle });
