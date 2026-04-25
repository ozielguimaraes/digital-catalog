// screens-2.jsx — Catálogo form, Produto form (com editor de imagens), Detalhe produto, Pública

// ─────────────────────────────────────────────────────────
// CATÁLOGO — criar/editar
// ─────────────────────────────────────────────────────────
function CatalogFormScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="Novo catálogo" onBack={() => go('catalogs')} compact
        actions={<button className="btn btn-primary btn-sm">Salvar</button>}/>

      <div style={{ padding: '8px 20px 20px', display: 'flex', flexDirection: 'column', gap: 18 }}>
        {/* Capa */}
        <div>
          <label>Capa do catálogo</label>
          <div style={{ height: 160, borderRadius: 16, overflow: 'hidden', position: 'relative', border: '1px solid var(--line-soft)' }}>
            <BagIllustration swatchIndex={0}/>
            <button style={{ position: 'absolute', bottom: 12, right: 12, background: 'rgba(255,255,255,0.95)', border: 'none', borderRadius: 99, padding: '7px 13px', fontSize: 12, fontWeight: 700, color: 'var(--ink)', cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: 6 }}>
              <I.camera size={14}/> Alterar
            </button>
          </div>
        </div>

        <div>
          <label>Nome</label>
          <input defaultValue="Coleção Outono 2026"/>
        </div>
        <div>
          <label>Descrição pública</label>
          <textarea defaultValue="Peças em tons quentes, couros selecionados. Cada bolsa é feita sob medida pelo ateliê Sany&Z."/>
        </div>

        <div>
          <label>Tipo</label>
          <div style={{ display: 'flex', gap: 8 }}>
            <TypeOpt icon={I.grid} label="Coleção" active/>
            <TypeOpt icon={I.zap} label="Lançamento"/>
            <TypeOpt icon={I.tag} label="Promoção"/>
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <Toggle label="Marcar como favorito" subtitle="Aparece destacado no início" on icon={I.starFill}/>
          <Toggle label="Catálogo público" subtitle="Visível pelo link compartilhado" on icon={I.eye}/>
          <Toggle label="Pedir via WhatsApp" subtitle="Botão direto no catálogo" on icon={I.wa}/>
        </div>
      </div>
    </Screen>
  );
}

function TypeOpt({ icon: Ic, label, active }) {
  return (
    <div style={{
      flex: 1, padding: '12px 10px', borderRadius: 10,
      border: '1px solid ' + (active ? 'var(--copper-sany)' : 'var(--line)'),
      background: active ? 'rgba(192,102,71,0.08)' : '#fff',
      color: active ? 'var(--copper-brown)' : 'var(--ink)',
      display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 5,
      fontSize: 12, fontWeight: 700, cursor: 'pointer',
    }}>
      <Ic size={18}/> {label}
    </div>
  );
}

function Toggle({ label, subtitle, on, icon: Ic }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '12px 14px', background: '#fff', borderRadius: 12, border: '1px solid var(--line-soft)' }}>
      {Ic && <div style={{ width: 32, height: 32, borderRadius: 9, background: 'var(--alabaster)', color: 'var(--copper-sany)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><Ic size={16}/></div>}
      <div style={{ flex: 1 }}>
        <div style={{ fontSize: 13, fontWeight: 700 }}>{label}</div>
        {subtitle && <div className="muted" style={{ fontSize: 11 }}>{subtitle}</div>}
      </div>
      <div style={{ width: 42, height: 25, borderRadius: 99, background: on ? 'var(--copper-sany)' : 'var(--line)', position: 'relative', transition: 'background 0.2s' }}>
        <div style={{ position: 'absolute', top: 2, left: on ? 19 : 2, width: 21, height: 21, borderRadius: 99, background: '#fff', boxShadow: '0 2px 4px rgba(0,0,0,0.15)', transition: 'left 0.2s' }}/>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────
// PRODUTO — criar/editar (com editor de imagens)
// ─────────────────────────────────────────────────────────
function ProductFormScreen({ go }) {
  return (
    <Screen>
      <AppHeader title="Novo produto" onBack={() => go('catalog')} compact
        actions={<button className="btn btn-primary btn-sm">Salvar</button>}/>

      <div style={{ padding: '8px 20px 20px', display: 'flex', flexDirection: 'column', gap: 20 }}>
        {/* Upload + editor de imagens */}
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <label style={{ marginBottom: 0 }}>Fotos do produto</label>
            <span className="muted" style={{ fontSize: 11 }}>Estrela marca destaque</span>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 8 }}>
            {[0,1,2,3].map(i => (
              <div key={i} style={{ aspectRatio: '1/1', borderRadius: 12, overflow: 'hidden', position: 'relative', border: i === 0 ? '2px solid var(--copper-sany)' : '1px solid var(--line-soft)' }}>
                <BagIllustration swatchIndex={i}/>
                <div style={{ position: 'absolute', top: 6, right: 6, width: 26, height: 26, borderRadius: 99, background: 'rgba(255,255,255,0.95)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: i === 0 ? 'var(--copper-sany)' : 'var(--muted)' }}>
                  {i === 0 ? <I.starFill size={13}/> : <I.star size={13}/>}
                </div>
                {i === 0 && <div style={{ position: 'absolute', bottom: 0, left: 0, right: 0, background: 'var(--copper-sany)', color: '#fff', fontSize: 9, fontWeight: 700, letterSpacing: '0.1em', textAlign: 'center', padding: '3px 0' }}>DESTAQUE</div>}
              </div>
            ))}
            <button style={{
              aspectRatio: '1/1', borderRadius: 12,
              border: '2px dashed var(--line)', background: 'transparent',
              display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
              gap: 4, color: 'var(--muted)', cursor: 'pointer',
            }}>
              <I.plus size={20}/>
              <span style={{ fontSize: 11, fontWeight: 700 }}>Adicionar</span>
            </button>
          </div>
        </div>

        {/* Editor de imagem — mini */}
        <div className="card" style={{ padding: 14 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ fontSize: 13, fontWeight: 700 }}>Editar imagem em destaque</div>
            <button className="btn btn-sm btn-ghost" style={{ padding: '4px 10px' }}><I.check size={12}/> Aplicar</button>
          </div>
          <div style={{ marginTop: 10, height: 180, borderRadius: 12, overflow: 'hidden', position: 'relative' }}>
            <BagIllustration swatchIndex={0}/>
            {/* Crop overlay */}
            <div style={{ position: 'absolute', inset: 20, border: '1px solid rgba(255,255,255,0.9)', boxShadow: '0 0 0 9999px rgba(0,0,0,0.35)' }}>
              {[['tl',-5,-5],['tr',-5,-5],['bl',-5,-5],['br',-5,-5]].map(([k]) => (
                <div key={k} style={{ position: 'absolute', width: 14, height: 14, border: '2.5px solid #fff',
                  top: k[0] === 't' ? -7 : undefined, bottom: k[0] === 'b' ? -7 : undefined,
                  left: k[1] === 'l' ? -7 : undefined, right: k[1] === 'r' ? -7 : undefined,
                  borderTopWidth: k[0] === 'b' ? 0 : undefined, borderBottomWidth: k[0] === 't' ? 0 : undefined,
                  borderLeftWidth: k[1] === 'r' ? 0 : undefined, borderRightWidth: k[1] === 'l' ? 0 : undefined,
                }}/>
              ))}
            </div>
          </div>
          <div style={{ display: 'flex', gap: 6, marginTop: 10, justifyContent: 'space-between' }}>
            {[{i:I.crop, l:'Cortar', a:true}, {i:I.sliders, l:'Ajustes'}, {i:I.image, l:'Filtros'}, {i:I.zap, l:'Auto'}].map((t,i) => (
              <button key={i} style={{
                flex: 1, padding: '8px 4px', borderRadius: 9,
                background: t.a ? 'var(--antracito)' : 'transparent',
                color: t.a ? '#fff' : 'var(--ink)',
                border: t.a ? 'none' : '1px solid var(--line)',
                display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3,
                fontSize: 10, fontWeight: 700, cursor: 'pointer',
              }}>
                <t.i size={16}/> {t.l}
              </button>
            ))}
          </div>
          {/* Sliders */}
          <div style={{ marginTop: 14, display: 'flex', flexDirection: 'column', gap: 10 }}>
            <MiniSlider label="Brilho" value={60}/>
            <MiniSlider label="Contraste" value={45}/>
            <MiniSlider label="Saturação" value={72}/>
          </div>
        </div>

        {/* Dados */}
        <div><label>Nome do produto</label><input defaultValue="Bolsa Aurora"/></div>
        <div style={{ display: 'flex', gap: 10 }}>
          <div style={{ flex: 1 }}><label>Preço</label><input defaultValue="R$ 489,00"/></div>
          <div style={{ flex: 1 }}><label>Coleção</label>
            <select defaultValue="outono"><option value="outono">Coleção Outono</option></select>
          </div>
        </div>
        <div><label>Descrição</label>
          <textarea defaultValue="Bolsa estruturada em couro caramelo com detalhe em metal envelhecido. Feita sob medida em 12 dias." rows={3}/>
        </div>

        {/* Disponibilidade */}
        <div>
          <label>Disponibilidade</label>
          <div style={{ display: 'flex', gap: 8, marginBottom: 10 }}>
            <TypeOpt icon={I.check} label="Disponível" active/>
            <TypeOpt icon={I.x} label="Esgotado"/>
            <TypeOpt icon={I.tag} label="Sob encomenda"/>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, padding: '12px 14px', background: '#fff', borderRadius: 12, border: '1px solid var(--line-soft)' }}>
            <div style={{ flex: 1, fontSize: 13, fontWeight: 700 }}>Quantidade em estoque</div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <button style={{ width: 28, height: 28, borderRadius: 8, border: '1px solid var(--line)', background: '#fff', cursor: 'pointer' }}>−</button>
              <div style={{ width: 36, textAlign: 'center', fontFamily: 'var(--serif)', fontSize: 18, fontWeight: 500 }}>6</div>
              <button style={{ width: 28, height: 28, borderRadius: 8, border: '1px solid var(--line)', background: '#fff', cursor: 'pointer' }}>+</button>
            </div>
          </div>
        </div>

        {/* Variações */}
        <div>
          <label>Variações</label>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div style={{ padding: '12px 14px', background: '#fff', borderRadius: 12, border: '1px solid var(--line-soft)' }}>
              <div className="caps" style={{ fontSize: 10 }}>Cor</div>
              <div style={{ display: 'flex', gap: 7, marginTop: 6, flexWrap: 'wrap' }}>
                {BAG_SWATCHES.slice(0,5).map((s,i) => (
                  <div key={i} style={{ width: 26, height: 26, borderRadius: 99, background: s.bg, border: i === 0 ? '2px solid var(--copper-sany)' : '2px solid transparent', boxShadow: 'inset 0 -2px 3px rgba(0,0,0,0.15)' }}/>
                ))}
                <button style={{ width: 26, height: 26, borderRadius: 99, border: '1px dashed var(--line)', background: 'transparent', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--muted)', cursor: 'pointer' }}><I.plus size={12}/></button>
              </div>
            </div>
            <button className="btn btn-ghost btn-sm" style={{ justifyContent: 'flex-start' }}>
              <I.plus size={12}/> Adicionar variação (tamanho, material)
            </button>
          </div>
        </div>
      </div>
    </Screen>
  );
}

function MiniSlider({ label, value }) {
  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 11, marginBottom: 4 }}>
        <span style={{ fontWeight: 700, color: 'var(--muted)', letterSpacing: '0.05em', textTransform: 'uppercase' }}>{label}</span>
        <span style={{ fontWeight: 700, color: 'var(--copper-sany)' }}>+{value - 50}</span>
      </div>
      <div style={{ height: 4, background: 'var(--line-soft)', borderRadius: 99, position: 'relative' }}>
        <div style={{ position: 'absolute', left: 0, top: 0, bottom: 0, width: value + '%', background: 'var(--copper-sany)', borderRadius: 99 }}/>
        <div style={{ position: 'absolute', left: `calc(${value}% - 8px)`, top: -6, width: 16, height: 16, borderRadius: 99, background: '#fff', boxShadow: '0 2px 6px rgba(0,0,0,0.2)', border: '2px solid var(--copper-sany)' }}/>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────
// PRODUTO — detalhe
// ─────────────────────────────────────────────────────────
function ProductScreen({ go }) {
  const p = PRODUCTS[0];
  return (
    <Screen>
      {/* Hero image */}
      <div style={{ position: 'relative', height: 380, flexShrink: 0 }}>
        <BagIllustration swatchIndex={0}/>
        <div style={{ position: 'absolute', top: 54, left: 16, right: 16, display: 'flex', justifyContent: 'space-between' }}>
          <button onClick={() => go('catalog')} className="btn btn-icon"><I.arrowLeft size={18}/></button>
          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn btn-icon"><I.share size={18}/></button>
            <button className="btn btn-icon" style={{ color: 'var(--copper-sany)' }}><I.starFill size={18}/></button>
          </div>
        </div>
        {/* Thumb strip */}
        <div style={{ position: 'absolute', bottom: 16, left: 16, right: 16, display: 'flex', gap: 8 }}>
          {[0,1,2,3].map(i => (
            <div key={i} style={{ width: 50, height: 50, borderRadius: 10, overflow: 'hidden', border: i === 0 ? '2px solid #fff' : '2px solid transparent', opacity: i === 0 ? 1 : 0.75 }}>
              <BagIllustration swatchIndex={i}/>
            </div>
          ))}
        </div>
      </div>

      {/* Content sheet */}
      <div style={{ background: 'var(--paper)', borderRadius: '24px 24px 0 0', marginTop: -24, padding: '20px 20px 16px', position: 'relative', zIndex: 2 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', gap: 12 }}>
          <div>
            <span className="chip chip-copper">Coleção Outono</span>
            <h2 style={{ fontSize: 26, marginTop: 8, fontStyle: 'italic', fontWeight: 500 }}>{p.name}</h2>
          </div>
          <div style={{ fontFamily: 'var(--serif)', fontSize: 24, color: 'var(--copper-brown)', fontWeight: 500 }}>{brl(p.price)}</div>
        </div>

        <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
          <span className="chip chip-green"><I.check size={10}/> {p.stock} em estoque</span>
          <span className="chip">Pronto em 12 dias</span>
        </div>

        <p style={{ marginTop: 14, fontSize: 13, lineHeight: 1.55, color: 'var(--muted)' }}>{p.desc}</p>

        <div style={{ marginTop: 18 }}>
          <div className="caps" style={{ fontSize: 10, marginBottom: 8 }}>Cor</div>
          <div style={{ display: 'flex', gap: 8 }}>
            {BAG_SWATCHES.slice(0,5).map((s,i) => (
              <div key={i} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}>
                <div style={{ width: 36, height: 36, borderRadius: 99, background: s.bg, border: i === 0 ? '2px solid var(--copper-sany)' : '2px solid transparent', boxShadow: 'inset 0 -2px 3px rgba(0,0,0,0.15)' }}/>
                <span style={{ fontSize: 9, fontWeight: 700, color: i === 0 ? 'var(--copper-sany)' : 'var(--muted)' }}>{s.label}</span>
              </div>
            ))}
          </div>
        </div>

        {/* stats */}
        <div style={{ marginTop: 20, padding: 14, borderRadius: 12, background: '#fff', border: '1px solid var(--line-soft)', display: 'flex', justifyContent: 'space-around' }}>
          <MiniStat label="Visitas" value="86"/>
          <div style={{ width: 1, background: 'var(--line-soft)' }}/>
          <MiniStat label="Pedidos" value="4"/>
          <div style={{ width: 1, background: 'var(--line-soft)' }}/>
          <MiniStat label="Favoritos" value="12"/>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 18 }}>
          <button className="btn btn-ghost" style={{ flex: 1 }} onClick={() => go('productForm')}><I.edit size={14}/> Editar</button>
          <button className="btn btn-dark" style={{ flex: 1 }}><I.bag size={14}/> Criar pedido</button>
        </div>
      </div>
    </Screen>
  );
}

// ─────────────────────────────────────────────────────────
// CATÁLOGO PÚBLICO — o que o cliente final vê
// ─────────────────────────────────────────────────────────
function PublicScreen({ go }) {
  return (
    <Screen hasBottomNav={false} noPadding bg="var(--paper)">
      {/* Hero */}
      <div style={{ position: 'relative', height: 240, background: 'var(--copper-grad)', color: '#fff', flexShrink: 0 }}>
        <div style={{ position: 'absolute', inset: 0, opacity: 0.15, backgroundImage: `url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='40' height='40'><path d='M 15 12 C 15 7, 25 7, 25 12 L 27 30 L 13 30 Z' stroke='%23fff' fill='none' stroke-width='1'/></svg>")`, backgroundSize: '40px' }}/>
        <div style={{ padding: '60px 24px 20px', position: 'relative', display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
          <button onClick={() => go('catalog')} style={{ width: 36, height: 36, borderRadius: 99, border: 'none', background: 'rgba(255,255,255,0.2)', color: '#fff', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><I.arrowLeft size={18}/></button>
          <div style={{ display: 'flex', gap: 8 }}>
            <button style={{ width: 36, height: 36, borderRadius: 99, border: 'none', background: 'rgba(255,255,255,0.2)', color: '#fff', cursor: 'pointer' }}><I.search size={16}/></button>
            <button style={{ width: 36, height: 36, borderRadius: 99, border: 'none', background: 'rgba(255,255,255,0.2)', color: '#fff', cursor: 'pointer' }}><I.share size={16}/></button>
          </div>
        </div>
        <div style={{ position: 'absolute', bottom: 24, left: 24, right: 24 }}>
          <LogoLockup dark size="sm"/>
          <h1 style={{ color: '#fff', fontSize: 32, fontStyle: 'italic', fontWeight: 500, marginTop: 8, lineHeight: 1.1 }}>Coleção<br/>Outono 2026</h1>
        </div>
      </div>

      <div style={{ padding: '18px 20px 20px' }}>
        <p style={{ fontSize: 13, color: 'var(--muted)', lineHeight: 1.55, marginBottom: 14 }}>
          Tons quentes e couros selecionados. Cada peça é feita sob medida no nosso ateliê.
        </p>

        {/* Categorias */}
        <div style={{ display: 'flex', gap: 8, overflowX: 'auto', marginBottom: 18, scrollbarWidth: 'none', paddingBottom: 4 }}>
          {['Tudo', 'Tote', 'Clutch', 'Saddle', 'Hobo', 'Mini'].map((c, i) => (
            <button key={c} style={{
              padding: '7px 14px', borderRadius: 99, whiteSpace: 'nowrap',
              border: '1px solid ' + (i === 0 ? 'var(--antracito)' : 'var(--line)'),
              background: i === 0 ? 'var(--antracito)' : 'transparent',
              color: i === 0 ? '#fff' : 'var(--ink)', fontSize: 12, fontWeight: 700, cursor: 'pointer',
            }}>{c}</button>
          ))}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
          {PRODUCTS.slice(0, 6).map(p => <PublicProdCard key={p.id} p={p}/>)}
        </div>
      </div>

      {/* WhatsApp fab */}
      <button style={{
        position: 'absolute', bottom: 24, right: 20, zIndex: 30,
        padding: '14px 18px', borderRadius: 99, border: 'none',
        background: '#25d366', color: '#fff', cursor: 'pointer',
        display: 'flex', alignItems: 'center', gap: 8,
        fontSize: 13, fontWeight: 700, letterSpacing: '0.04em',
        boxShadow: '0 10px 24px rgba(37,211,102,0.4)',
      }}>
        <I.wa size={18}/> Pedir pelo WhatsApp
      </button>
    </Screen>
  );
}

function PublicProdCard({ p }) {
  const oos = p.status === 'oos';
  return (
    <div style={{ cursor: 'pointer' }}>
      <div style={{ aspectRatio: '1/1', borderRadius: 14, overflow: 'hidden', position: 'relative', background: '#fff', boxShadow: 'var(--sh-sm)' }}>
        <BagIllustration swatchIndex={p.swatch}/>
        {oos && <div style={{ position: 'absolute', inset: 0, background: 'rgba(51,42,39,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#fff', fontSize: 11, fontWeight: 700, letterSpacing: '0.15em' }}>ESGOTADO</div>}
        {!oos && <button style={{ position: 'absolute', top: 8, right: 8, width: 28, height: 28, borderRadius: 99, border: 'none', background: 'rgba(255,255,255,0.9)', color: 'var(--muted)', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><I.heart size={14}/></button>}
      </div>
      <div style={{ padding: '8px 2px 0' }}>
        <div style={{ fontFamily: 'var(--serif)', fontSize: 14, fontStyle: 'italic' }}>{p.name}</div>
        <div style={{ fontSize: 13, fontWeight: 700, marginTop: 2, color: oos ? 'var(--muted)' : 'var(--copper-brown)', textDecoration: oos ? 'line-through' : 'none' }}>{brl(p.price)}</div>
      </div>
    </div>
  );
}

Object.assign(window, { CatalogFormScreen, ProductFormScreen, ProductScreen, PublicScreen, Toggle, TypeOpt, MiniSlider });
