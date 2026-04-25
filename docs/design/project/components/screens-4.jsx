// screens-4.jsx — Financeiro (A pagar/receber, Fluxo de caixa), Mais/Perfil

// ─────────────────────────────────────────────────────────
// FINANCEIRO — dashboard + fluxo de caixa
// ─────────────────────────────────────────────────────────
function FinanceScreen({ go }) {
  const totalRec = FINANCE.receivable.filter(r => r.status !== 'paid').reduce((s, r) => s + r.amount, 0);
  const totalPay = FINANCE.payable.filter(r => r.status !== 'paid').reduce((s, r) => s + r.amount, 0);

  return (
    <Screen>
      <AppHeader title="Financeiro" subtitle="Abril 2026"
        actions={<button className="btn btn-icon"><I.calendar size={18}/></button>}/>

      {/* Saldo atual */}
      <div style={{ padding: '4px 20px 16px' }}>
        <div style={{
          borderRadius: 22, padding: 22, color: '#fff',
          background: 'var(--antracito)',
          position: 'relative', overflow: 'hidden',
        }}>
          <div style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.12em', opacity: 0.7 }}>SALDO DO MÊS</div>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 38, marginTop: 6, fontWeight: 500 }}>
            R$ 4.638,<span style={{ fontSize: 22 }}>00</span>
          </div>
          <div style={{ display: 'flex', gap: 16, marginTop: 18, paddingTop: 14, borderTop: '1px solid rgba(255,255,255,0.14)' }}>
            <div style={{ flex: 1 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 5, fontSize: 10, opacity: 0.7, fontWeight: 700, letterSpacing: '0.1em' }}>
                <I.trend size={12}/> ENTRADAS
              </div>
              <div style={{ fontFamily: 'var(--serif)', fontSize: 18, marginTop: 3, color: '#b8d4a0' }}>{brlShort(totalRec)}</div>
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 5, fontSize: 10, opacity: 0.7, fontWeight: 700, letterSpacing: '0.1em' }}>
                <I.trendDown size={12}/> SAÍDAS
              </div>
              <div style={{ fontFamily: 'var(--serif)', fontSize: 18, marginTop: 3, color: '#d4a898' }}>{brlShort(totalPay)}</div>
            </div>
          </div>
        </div>
      </div>

      {/* Gráfico fluxo de caixa simplificado */}
      <div style={{ padding: '0 20px 16px' }}>
        <div className="card" style={{ padding: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 12 }}>
            <h3 style={{ fontSize: 15, fontStyle: 'italic', fontWeight: 500 }}>Fluxo de caixa</h3>
            <span className="caps">Últimos 6 meses</span>
          </div>
          <CashflowChart/>
          <div style={{ display: 'flex', gap: 14, marginTop: 10, fontSize: 11 }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
              <span style={{ width: 10, height: 10, borderRadius: 2, background: 'var(--copper-sany)' }}/>
              Entradas
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
              <span style={{ width: 10, height: 10, borderRadius: 2, background: 'var(--antracito)' }}/>
              Saídas
            </span>
          </div>
        </div>
      </div>

      {/* Tabs A receber / A pagar */}
      <div style={{ padding: '0 20px 12px', display: 'flex', gap: 10 }}>
        <div onClick={() => go('receivable')} style={{ flex: 1, padding: 14, borderRadius: 14, background: 'rgba(74,102,56,0.08)', border: '1px solid rgba(74,102,56,0.2)', cursor: 'pointer' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, color: 'var(--success)' }}>
            <I.trend size={14}/>
            <span className="caps" style={{ color: 'var(--success)', fontSize: 10 }}>A receber</span>
          </div>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 20, marginTop: 4, color: 'var(--success)', fontWeight: 500 }}>{brlShort(totalRec)}</div>
          <div style={{ fontSize: 10, color: 'var(--muted)', marginTop: 2 }}>3 em aberto</div>
        </div>
        <div onClick={() => go('payable')} style={{ flex: 1, padding: 14, borderRadius: 14, background: 'rgba(138,58,58,0.06)', border: '1px solid rgba(138,58,58,0.2)', cursor: 'pointer' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, color: 'var(--danger)' }}>
            <I.trendDown size={14}/>
            <span className="caps" style={{ color: 'var(--danger)', fontSize: 10 }}>A pagar</span>
          </div>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 20, marginTop: 4, color: 'var(--danger)', fontWeight: 500 }}>{brlShort(totalPay)}</div>
          <div style={{ fontSize: 10, color: 'var(--muted)', marginTop: 2 }}>3 em aberto • 1 vencido</div>
        </div>
      </div>

      {/* Próximos vencimentos */}
      <SectionTitle title="Próximos vencimentos"/>
      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 8 }}>
        {[...FINANCE.receivable.filter(r => r.status !== 'paid').map(r => ({...r, kind: 'in'})),
          ...FINANCE.payable.filter(r => r.status !== 'paid').map(r => ({...r, kind: 'out'}))]
          .slice(0, 5).map(r => (
          <div key={r.id} className="card" style={{ padding: 12, display: 'flex', alignItems: 'center', gap: 12 }}>
            <div style={{
              width: 38, height: 38, borderRadius: 10,
              background: r.kind === 'in' ? 'rgba(74,102,56,0.12)' : 'rgba(138,58,58,0.08)',
              color: r.kind === 'in' ? 'var(--success)' : 'var(--danger)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
            }}>
              {r.kind === 'in' ? <I.trend size={16}/> : <I.trendDown size={16}/>}
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontSize: 13, fontWeight: 700, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{r.desc}</div>
              <div style={{ fontSize: 11, marginTop: 2, color: r.status === 'overdue' ? 'var(--danger)' : 'var(--muted)' }}>
                {r.status === 'overdue' ? '⚠ Vencido — ' : 'Vence em '}{r.due}
              </div>
            </div>
            <div style={{ fontFamily: 'var(--serif)', fontSize: 15, fontWeight: 500, color: r.kind === 'in' ? 'var(--success)' : 'var(--danger)' }}>
              {r.kind === 'in' ? '+' : '−'} {brlShort(r.amount)}
            </div>
          </div>
        ))}
      </div>
    </Screen>
  );
}

function CashflowChart() {
  const data = [
    { m: 'Nov', in: 45, out: 30 },
    { m: 'Dez', in: 62, out: 38 },
    { m: 'Jan', in: 38, out: 32 },
    { m: 'Fev', in: 55, out: 42 },
    { m: 'Mar', in: 71, out: 48 },
    { m: 'Abr', in: 84, out: 44 },
  ];
  const max = 90;
  return (
    <div style={{ display: 'flex', gap: 10, alignItems: 'flex-end', height: 120, padding: '8px 0' }}>
      {data.map((d, i) => (
        <div key={i} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}>
          <div style={{ display: 'flex', gap: 3, alignItems: 'flex-end', width: '100%', height: 96 }}>
            <div style={{ flex: 1, height: (d.in/max*100)+'%', background: 'var(--copper-sany)', borderRadius: '3px 3px 0 0', minHeight: 4 }}/>
            <div style={{ flex: 1, height: (d.out/max*100)+'%', background: 'var(--antracito)', borderRadius: '3px 3px 0 0', minHeight: 4 }}/>
          </div>
          <div style={{ fontSize: 10, color: 'var(--muted)', fontWeight: 700, letterSpacing: '0.05em' }}>{d.m}</div>
        </div>
      ))}
    </div>
  );
}

// ─────────────────────────────────────────────────────────
// A Receber (detalhe)
// ─────────────────────────────────────────────────────────
function ReceivableScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="A receber" onBack={() => go('finance')} compact
        actions={<button className="btn btn-icon" style={{ background: 'var(--copper-sany)', color: '#fff', borderColor: 'var(--copper-sany)' }}><I.plus size={18}/></button>}/>

      <div style={{ padding: '0 20px 14px' }}>
        <div className="card" style={{ padding: 16, background: 'rgba(74,102,56,0.06)', border: '1px solid rgba(74,102,56,0.2)' }}>
          <span className="caps" style={{ fontSize: 10, color: 'var(--success)' }}>Total em aberto</span>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 30, color: 'var(--success)', fontWeight: 500, marginTop: 4 }}>R$ 1.746,00</div>
          <div style={{ fontSize: 11, color: 'var(--muted)', marginTop: 2 }}>3 lançamentos • 1 vencido</div>
        </div>
      </div>

      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 8 }}>
        {FINANCE.receivable.map(r => <FinanceRow key={r.id} r={r} income/>)}
      </div>
    </Screen>
  );
}

function PayableScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="A pagar" onBack={() => go('finance')} compact
        actions={<button className="btn btn-icon" style={{ background: 'var(--copper-sany)', color: '#fff', borderColor: 'var(--copper-sany)' }}><I.plus size={18}/></button>}/>

      <div style={{ padding: '0 20px 14px' }}>
        <div className="card" style={{ padding: 16, background: 'rgba(138,58,58,0.05)', border: '1px solid rgba(138,58,58,0.2)' }}>
          <span className="caps" style={{ fontSize: 10, color: 'var(--danger)' }}>Total em aberto</span>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 30, color: 'var(--danger)', fontWeight: 500, marginTop: 4 }}>R$ 3.782,00</div>
          <div style={{ fontSize: 11, color: 'var(--muted)', marginTop: 2 }}>3 lançamentos • 1 vencido</div>
        </div>
      </div>

      <div style={{ padding: '0 20px', display: 'flex', flexDirection: 'column', gap: 8 }}>
        {FINANCE.payable.map(r => <FinanceRow key={r.id} r={r}/>)}
      </div>
    </Screen>
  );
}

function FinanceRow({ r, income }) {
  const paid = r.status === 'paid';
  const overdue = r.status === 'overdue';
  const color = income ? 'var(--success)' : 'var(--danger)';
  return (
    <div className="card" style={{ padding: 14, display: 'flex', alignItems: 'center', gap: 12, opacity: paid ? 0.55 : 1 }}>
      <div style={{
        width: 40, height: 40, borderRadius: 10,
        background: paid ? 'var(--alabaster)' : (income ? 'rgba(74,102,56,0.12)' : 'rgba(138,58,58,0.08)'),
        color: paid ? 'var(--muted)' : color,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
      }}>
        {paid ? <I.check size={16}/> : (income ? <I.trend size={16}/> : <I.trendDown size={16}/>)}
      </div>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontSize: 13, fontWeight: 700, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{r.desc}</div>
        <div style={{ display: 'flex', gap: 8, marginTop: 3, alignItems: 'center' }}>
          <span className="muted" style={{ fontSize: 11 }}>Venc. {r.due}</span>
          {paid && <span className="chip chip-green">Pago</span>}
          {overdue && <span className="chip chip-red">Vencido</span>}
        </div>
      </div>
      <div style={{ textAlign: 'right', flexShrink: 0 }}>
        <div style={{ fontFamily: 'var(--serif)', fontSize: 16, fontWeight: 500, color: paid ? 'var(--muted)' : color, textDecoration: paid ? 'line-through' : 'none' }}>
          {brlShort(r.amount)}
        </div>
        {!paid && <button className="btn btn-sm btn-ghost" style={{ padding: '3px 8px', fontSize: 10, marginTop: 4 }}>Marcar pago</button>}
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────
// MAIS / Perfil / Configurações
// ─────────────────────────────────────────────────────────
function MoreScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="Mais"/>

      {/* Profile card */}
      <div style={{ padding: '4px 20px 18px' }}>
        <div className="card" style={{ padding: 18, display: 'flex', alignItems: 'center', gap: 14 }}>
          <div style={{
            width: 56, height: 56, borderRadius: 99,
            background: 'var(--copper-grad)', color: '#fff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontFamily: 'var(--serif)', fontStyle: 'italic', fontSize: 22, fontWeight: 600,
          }}>SZ</div>
          <div style={{ flex: 1 }}>
            <div style={{ fontFamily: 'var(--serif)', fontStyle: 'italic', fontSize: 18, fontWeight: 500 }}>Ateliê Sany&Z</div>
            <div className="muted" style={{ fontSize: 12, marginTop: 2 }}>sany@atelie.com</div>
            <div style={{ marginTop: 6 }}>
              <span className="chip chip-copper" style={{ fontSize: 9 }}>Plano Pro</span>
            </div>
          </div>
          <button className="btn btn-icon" style={{ width: 32, height: 32 }}><I.chevRight size={14}/></button>
        </div>
      </div>

      {/* Menu sections */}
      <div style={{ padding: '0 20px 14px' }}>
        <div className="caps" style={{ padding: '0 4px 8px' }}>Cadastros</div>
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <MenuRow icon={I.users} label="Clientes" detail="5" onClick={() => go('clients')}/>
          <MenuRow icon={I.truck} label="Fornecedores" detail="4" onClick={() => go('suppliers')}/>
          <MenuRow icon={I.tag} label="Coleções & categorias" detail="6" isLast/>
        </div>
      </div>

      <div style={{ padding: '0 20px 14px' }}>
        <div className="caps" style={{ padding: '0 4px 8px' }}>Ateliê</div>
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <MenuRow icon={I.bag} label="Ordens de produção"/>
          <MenuRow icon={I.box} label="Matérias-primas"/>
          <MenuRow icon={I.chart} label="Relatórios" isLast/>
        </div>
      </div>

      <div style={{ padding: '0 20px 14px' }}>
        <div className="caps" style={{ padding: '0 4px 8px' }}>Configurações</div>
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <MenuRow icon={I.settings} label="Dados do ateliê"/>
          <MenuRow icon={I.bell} label="Notificações"/>
          <MenuRow icon={I.lock} label="Segurança e privacidade"/>
          <MenuRow icon={I.share} label="Integrações — WhatsApp, Pix" isLast/>
        </div>
      </div>

      <div style={{ padding: '0 20px 14px' }}>
        <button className="btn btn-ghost" style={{ width: '100%', color: 'var(--danger)' }}>Sair da conta</button>
      </div>
    </Screen>
  );
}

function MenuRow({ icon: Ic, label, detail, isLast, onClick }) {
  return (
    <button onClick={onClick} style={{
      display: 'flex', alignItems: 'center', gap: 12, width: '100%',
      padding: '14px 16px', background: 'transparent', border: 'none',
      borderBottom: isLast ? 'none' : '1px solid var(--line-soft)',
      cursor: 'pointer', textAlign: 'left',
    }}>
      <div style={{ width: 32, height: 32, borderRadius: 9, background: 'var(--alabaster)', color: 'var(--copper-sany)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Ic size={16}/>
      </div>
      <span style={{ flex: 1, fontSize: 14, fontWeight: 700 }}>{label}</span>
      {detail && <span className="muted" style={{ fontSize: 12 }}>{detail}</span>}
      <I.chevRight size={14} stroke="var(--muted)"/>
    </button>
  );
}

Object.assign(window, { FinanceScreen, ReceivableScreen, PayableScreen, MoreScreen, FinanceRow, MenuRow, CashflowChart });
