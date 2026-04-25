// shell.jsx — Device frame mobile customizado + navegação + telas base

// ────────────────────────────────────────────────────────────
// Frame de dispositivo (iPhone-like, 390×844) — leve, copper-ready
// ────────────────────────────────────────────────────────────
function Phone({ children, width = 390, height = 844, dark = false, label, statusDark = false, noBottom = false }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 14 }}>
      <div style={{
        width, height, borderRadius: 46, overflow: 'hidden',
        position: 'relative',
        background: dark ? '#1a1210' : '#faf6f1',
        boxShadow: '0 40px 80px rgba(51,42,39,0.22), 0 0 0 10px #1a1210, 0 0 0 11px #2a1d16, 0 0 0 12px #0a0604',
        fontFamily: 'var(--sans)',
        flexShrink: 0,
      }}>
        {/* Dynamic island */}
        <div style={{
          position: 'absolute', top: 10, left: '50%', transform: 'translateX(-50%)',
          width: 118, height: 32, borderRadius: 20, background: '#000', zIndex: 50,
        }}/>
        {/* Status bar */}
        <div style={{
          position: 'absolute', top: 0, left: 0, right: 0, zIndex: 40,
          height: 50, padding: '14px 32px 0', display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          color: statusDark ? '#fff' : '#121212', fontSize: 15, fontWeight: 600,
        }}>
          <span>9:41</span>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <svg width="17" height="11" viewBox="0 0 17 11"><path d="M1 7.5v2a.5.5 0 0 0 .5.5h1a.5.5 0 0 0 .5-.5v-2a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5zm4-3v5a.5.5 0 0 0 .5.5h1a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5zm4-3v8a.5.5 0 0 0 .5.5h1a.5.5 0 0 0 .5-.5v-8a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5zm4-2v10a.5.5 0 0 0 .5.5h1a.5.5 0 0 0 .5-.5v-10a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5z" fill="currentColor"/></svg>
            <svg width="26" height="11" viewBox="0 0 26 11"><rect x="1" y="1" width="21" height="9" rx="2.5" fill="none" stroke="currentColor" strokeOpacity="0.4"/><rect x="2.5" y="2.5" width="18" height="6" rx="1.2" fill="currentColor"/><rect x="23" y="4" width="1.8" height="3" rx="0.5" fill="currentColor" opacity="0.5"/></svg>
          </div>
        </div>
        {/* Content area */}
        <div style={{ height: '100%', display: 'flex', flexDirection: 'column', paddingTop: 0 }}>
          {children}
        </div>
        {/* Home indicator */}
        {!noBottom && <div style={{
          position: 'absolute', bottom: 8, left: '50%', transform: 'translateX(-50%)',
          width: 135, height: 5, borderRadius: 99,
          background: dark ? 'rgba(255,255,255,0.7)' : 'rgba(0,0,0,0.28)',
          zIndex: 60,
        }}/>}
      </div>
      {label && <div style={{ fontFamily: 'var(--sans)', fontSize: 11, fontWeight: 700, letterSpacing: '0.12em', textTransform: 'uppercase', color: 'var(--muted)' }}>{label}</div>}
    </div>
  );
}

// App header: top bar com título + ações
function AppHeader({ title, subtitle, onBack, actions, compact }) {
  return (
    <div style={{
      paddingTop: 56,
      padding: compact ? '56px 20px 12px' : '56px 20px 16px',
      background: 'transparent',
      display: 'flex', flexDirection: 'column', gap: 8,
      flexShrink: 0,
    }}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minHeight: 36 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          {onBack && (
            <button className="btn btn-icon" style={{ width: 36, height: 36 }}>
              <I.arrowLeft size={18}/>
            </button>
          )}
          {!compact && <h1 style={{ fontSize: 28, fontWeight: 500, fontStyle: 'italic' }}>{title}</h1>}
          {compact && <div style={{ fontFamily: 'var(--serif)', fontSize: 17, fontWeight: 500 }}>{title}</div>}
        </div>
        <div style={{ display: 'flex', gap: 8 }}>{actions}</div>
      </div>
      {subtitle && <div className="muted" style={{ fontSize: 13 }}>{subtitle}</div>}
    </div>
  );
}

// Bottom nav tabs
function BottomNav({ current, onTab }) {
  const tabs = [
    { k: 'home', icon: I.home, label: 'Início' },
    { k: 'catalogs', icon: I.grid, label: 'Catálogos' },
    { k: 'orders', icon: I.bag, label: 'Pedidos' },
    { k: 'finance', icon: I.chart, label: 'Financeiro' },
    { k: 'more', icon: I.hamburger, label: 'Mais' },
  ];
  return (
    <div style={{
      position: 'absolute', bottom: 0, left: 0, right: 0, zIndex: 35,
      background: 'rgba(250, 246, 241, 0.92)',
      backdropFilter: 'blur(16px)', WebkitBackdropFilter: 'blur(16px)',
      borderTop: '1px solid var(--line-soft)',
      padding: '10px 8px 28px',
      display: 'flex', justifyContent: 'space-around', alignItems: 'center',
    }}>
      {tabs.map(t => {
        const active = current === t.k;
        return (
          <button key={t.k} onClick={() => onTab && onTab(t.k)} style={{
            background: 'none', border: 'none', cursor: 'pointer',
            display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3,
            color: active ? 'var(--copper-sany)' : 'var(--muted)',
            padding: '4px 10px', minWidth: 56,
          }}>
            <t.icon size={22} sw={active ? 2 : 1.6}/>
            <span style={{ fontSize: 10, fontWeight: 700, letterSpacing: '0.04em' }}>{t.label}</span>
          </button>
        );
      })}
    </div>
  );
}

// Container de conteúdo com scroll + padding p/ bottom nav
function Screen({ children, bg = 'var(--paper)', hasBottomNav = true, noPadding = false }) {
  return (
    <div style={{
      flex: 1, overflow: 'auto', background: bg,
      paddingBottom: hasBottomNav ? 100 : 20,
      padding: noPadding ? 0 : (hasBottomNav ? '0 0 100px' : '0 0 20px'),
    }}>
      {children}
    </div>
  );
}

// Helper: formatar moeda
function brl(v) { return 'R$ ' + v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
function brlShort(v) { return 'R$ ' + v.toLocaleString('pt-BR'); }

// Mini logo (símbolo) — círculo copper com glifo
function MiniMark({ size = 32 }) {
  return (
    <div style={{
      width: size, height: size, borderRadius: '50%',
      background: 'var(--copper-grad)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      boxShadow: 'inset 0 -4px 8px rgba(0,0,0,0.2), inset 0 2px 3px rgba(255,255,255,0.25), 0 2px 8px rgba(138,71,42,0.3)',
      color: '#fff',
    }}>
      <I.logo size={size * 0.58}/>
    </div>
  );
}

// Logo composto
function LogoLockup({ dark = false, size = 'md' }) {
  const sizes = { sm: { mk: 24, txt: 15 }, md: { mk: 32, txt: 20 }, lg: { mk: 48, txt: 30 } };
  const s = sizes[size];
  return (
    <div style={{ display: 'inline-flex', alignItems: 'center', gap: 10 }}>
      <MiniMark size={s.mk}/>
      <div style={{
        fontFamily: 'var(--serif)', fontStyle: 'italic', fontWeight: 500,
        fontSize: s.txt, letterSpacing: '-0.01em',
        color: dark ? '#fff' : 'var(--copper-sany)',
      }}>Sany<span style={{ color: dark ? '#fff' : 'var(--copper-brown)' }}>&</span>Z</div>
    </div>
  );
}

// SearchInput — estilo mobile nativo (pill, fundo alabaster, sem borda)
function SearchInput({ placeholder = 'Buscar…', defaultValue = '' }) {
  const [v, setV] = React.useState(defaultValue);
  const [focused, setFocused] = React.useState(false);
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 8,
      background: focused ? '#fff' : 'var(--alabaster)',
      borderRadius: 999,
      height: 40,
      paddingLeft: 14, paddingRight: 6,
      border: '1px solid ' + (focused ? 'var(--copper-sany)' : 'transparent'),
      boxShadow: focused ? '0 0 0 4px rgba(192,102,71,0.10)' : 'none',
      transition: 'background 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease',
    }}>
      <I.search size={16} stroke={focused ? 'var(--copper-sany)' : 'var(--muted)'} sw={2.2}/>
      <input
        type="text"
        value={v}
        onChange={(e) => setV(e.target.value)}
        onFocus={() => setFocused(true)}
        onBlur={() => setFocused(false)}
        placeholder={placeholder}
        style={{
          flex: 1, minWidth: 0,
          border: 'none', background: 'transparent',
          padding: '0 2px', fontSize: 15,
          fontFamily: 'var(--sans)',
          color: 'var(--ink)',
          outline: 'none', boxShadow: 'none',
          height: '100%',
        }}
      />
      {v && (
        <button
          onMouseDown={(e) => { e.preventDefault(); setV(''); }}
          aria-label="Limpar busca"
          style={{
            width: 22, height: 22, borderRadius: 999, border: 'none',
            background: 'rgba(43,43,43,0.32)', color: '#fff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            cursor: 'pointer', flexShrink: 0, padding: 0,
          }}
        >
          <I.x size={12} sw={3} stroke="#fff"/>
        </button>
      )}
    </div>
  );
}

Object.assign(window, { Phone, AppHeader, BottomNav, Screen, brl, brlShort, MiniMark, LogoLockup, SearchInput });
