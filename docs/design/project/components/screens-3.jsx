// screens-3.jsx — Pedidos, Clientes, Fornecedores

// ─────────────────────────────────────────────────────────
// PEDIDOS — lista
// ─────────────────────────────────────────────────────────
function OrdersScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="Pedidos" subtitle="12 pedidos em abril"
        actions={<button className="btn btn-icon" style={{ background: 'var(--copper-sany)', color: '#fff', borderColor: 'var(--copper-sany)' }}><I.plus size={18}/></button>}
      />

      <div style={{ padding: '0 20px 12px', display: 'flex', gap: 8, overflowX: 'auto', scrollbarWidth: 'none' }}>
        <Pill active>Todos (12)</Pill>
        <Pill>Rascunho</Pill>
        <Pill>Em produção (3)</Pill>
        <Pill>Pronto (2)</Pill>
        <Pill>Entregue</Pill>
      </div>

      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 10 }}>
        {ORDERS.map(o => {
          const st = STATUS_LABEL[o.status];
          return (
            <div key={o.id} className="card" style={{ padding: 14, cursor: 'pointer' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', gap: 10 }}>
                <div style={{ minWidth: 0, flex: 1 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <span className="caps">{o.id}</span>
                    <span className="muted" style={{ fontSize: 11 }}>• {o.date}</span>
                  </div>
                  <div style={{ fontFamily: 'var(--serif)', fontSize: 17, fontStyle: 'italic', marginTop: 4, lineHeight: 1.3, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{o.client}</div>
                  <div className="muted" style={{ fontSize: 12, marginTop: 2 }}>{o.items} {o.items>1?'itens':'item'}</div>
                </div>
                <div style={{ textAlign: 'right', flexShrink: 0 }}>
                  <span className={'chip ' + st.chip}>{st.label}</span>
                  <div style={{ fontFamily: 'var(--serif)', fontSize: 18, marginTop: 8, color: 'var(--copper-brown)', lineHeight: 1.2 }}>{brl(o.total)}</div>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </Screen>
  );
}

// ─────────────────────────────────────────────────────────
// CLIENTES
// ─────────────────────────────────────────────────────────
function ClientsScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="Clientes" subtitle={`${CLIENTS.length} cadastrados`} onBack={() => go('more')} compact
        actions={<button className="btn btn-icon" style={{ background: 'var(--copper-sany)', color: '#fff', borderColor: 'var(--copper-sany)' }}><I.plus size={18}/></button>}
      />

      <div style={{ padding: '0 20px 12px' }}>
        <SearchInput placeholder="Buscar cliente…"/>
      </div>

      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 8 }}>
        {CLIENTS.map(c => (
          <div key={c.id} className="card" style={{ padding: 14, display: 'flex', alignItems: 'center', gap: 12, cursor: 'pointer' }}>
            <div style={{
              width: 44, height: 44, borderRadius: 99,
              background: 'var(--copper-grad)', color: '#fff',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontFamily: 'var(--serif)', fontStyle: 'italic', fontSize: 16, fontWeight: 600,
              flexShrink: 0,
            }}>{c.name.split(' ').map(n => n[0]).slice(0,2).join('')}</div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontFamily: 'var(--serif)', fontSize: 15, fontStyle: 'italic' }}>{c.name}</div>
              <div className="muted" style={{ fontSize: 11, marginTop: 2, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{c.phone}</div>
            </div>
            <div style={{ textAlign: 'right', flexShrink: 0 }}>
              <div style={{ fontSize: 11, fontWeight: 700, color: 'var(--copper-brown)' }}>{brl(c.total)}</div>
              <div className="muted" style={{ fontSize: 10, marginTop: 1 }}>{c.orders} pedido{c.orders>1?'s':''}</div>
            </div>
          </div>
        ))}
      </div>
    </Screen>
  );
}

// ─────────────────────────────────────────────────────────
// FORNECEDORES
// ─────────────────────────────────────────────────────────
function SuppliersScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="Fornecedores" subtitle={`${SUPPLIERS.length} ativos`} onBack={() => go('more')} compact
        actions={<button className="btn btn-icon" style={{ background: 'var(--copper-sany)', color: '#fff', borderColor: 'var(--copper-sany)' }}><I.plus size={18}/></button>}
      />

      <div style={{ padding: '0 20px 12px', display: 'flex', gap: 8, overflowX: 'auto', scrollbarWidth: 'none' }}>
        <Pill active>Todos</Pill>
        <Pill>Couros</Pill>
        <Pill>Metais</Pill>
        <Pill>Forros</Pill>
      </div>

      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 8 }}>
        {SUPPLIERS.map(s => (
          <div key={s.id} className="card" style={{ padding: 14, cursor: 'pointer' }}>
            <div style={{ display: 'flex', alignItems: 'start', gap: 12 }}>
              <div style={{
                width: 44, height: 44, borderRadius: 12,
                background: 'var(--alabaster)', color: 'var(--copper-sany)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                flexShrink: 0,
              }}><I.truck size={20}/></div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontFamily: 'var(--serif)', fontSize: 15, fontStyle: 'italic', lineHeight: 1.2 }}>{s.name}</div>
                <div style={{ display: 'flex', gap: 8, marginTop: 4, alignItems: 'center' }}>
                  <span className="chip" style={{ fontSize: 9 }}>{s.cat}</span>
                  <span className="muted" style={{ fontSize: 11 }}>• {s.contact}</span>
                </div>
                {s.payable > 0 && (
                  <div style={{ marginTop: 8, display: 'flex', alignItems: 'center', gap: 6, padding: '5px 10px', background: 'rgba(138,58,58,0.08)', borderRadius: 8, width: 'fit-content' }}>
                    <I.coin size={12} stroke="var(--danger)"/>
                    <span style={{ fontSize: 11, fontWeight: 700, color: 'var(--danger)' }}>A pagar {brl(s.payable)}</span>
                  </div>
                )}
              </div>
              <button className="btn btn-icon" style={{ width: 34, height: 34 }}><I.phone size={14}/></button>
            </div>
          </div>
        ))}
      </div>
    </Screen>
  );
}

Object.assign(window, { OrdersScreen, ClientsScreen, SuppliersScreen });
