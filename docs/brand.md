# Sany&Z — Brand Tokens

Documento consolidado a partir do `Manual de Marca - SANY&Z.pdf`. Use este arquivo como fonte de verdade ao implementar UI; o PDF não precisa ser aberto novamente.

> **Slogan:** *Leve com você*

## Sobre a marca

A Sany&Z é um ateliê de bolsas que transforma o comum em especial. Linguagem visual: tons cobre, serifa elegante, neutros aconchegantes. Valores: **qualidade**, **inovação**, **atendimento personalizado**, **responsabilidade social**.

## Paleta de cores

### Copper (primárias)

| Nome | Hex | RGB | CMYK |
|---|---|---|---|
| Copper Sany | `#c06647` | 192, 102, 71 | 20, 69, 78, 6 |
| Copper Z | `#df8252` | 223, 130, 82 | 10, 58, 75, 0 |
| Copper Brown | `#8a472a` | 138, 71, 42 | 31, 75, 90, 29 |

**Copper Degradê** — gradiente linear das três acima. Padrão: `linear-gradient(135deg, #df8252 0%, #c06647 50%, #8a472a 100%)`.

### Neutras

| Nome | Hex | RGB | CMYK |
|---|---|---|---|
| Alabaster | `#ede8e2` | 237, 232, 226 | 6, 6, 9, 0 |
| Quartz | `#dfd5ce` | 223, 213, 206 | 11, 13, 16, 0 |
| Antracito | `#332a27` | 51, 42, 39 | 63, 66, 66, 66 |
| Onyx | `#121212` | 18, 18, 18 | 73, 67, 66, 82 |

### Mapeamento por tema (uso no app)

#### Tema claro
- Background: Alabaster `#ede8e2` (ou variante mais clara `#f5efe8`)
- Surface (cards): `#ffffff`
- Texto primário: Antracito `#332a27`
- Texto secundário/muted: `#7a6a60` (derivado, não no manual)
- Bordas: `#e0d4c6` / Quartz `#dfd5ce`
- Primary (CTAs): Copper Sany `#c06647`
- Primary hover/pressed: Copper Brown `#8a472a`

#### Tema escuro
- Background: Onyx `#121212`
- Surface (cards): Antracito `#332a27`
- Texto primário: Alabaster `#ede8e2`
- Texto secundário: Quartz `#dfd5ce`
- Bordas: `#3d322e` (derivado de Antracito)
- Primary (CTAs): Copper Z `#df8252` (mais clara — melhor contraste no escuro)
- Primary hover/pressed: Copper Sany `#c06647`

### Status (derivados, não no manual)

- Sucesso: `#4a6638`
- Aviso: `#b08740`
- Erro: `#8a3a3a`

## Tipografia

| Família | Uso | Pesos disponíveis |
|---|---|---|
| **Lora** (serifa) | Headlines, hero, números financeiros, citações | 400, 500, 600, 700 + italics |
| **Lato** (sans) | Corpo, UI, labels, botões, navegação | 300, 400, 700, 900 + italics |

**Convenções:**
- H1/H2 em Lora 500 com leve `letter-spacing: -0.01em`. Italic 500 é a "voz" da marca (uso pontual em hero).
- Body em Lato 400.
- Labels e CAPS em Lato 700, `letter-spacing: 0.1em`, uppercase.
- Botões em Lato 700, `letter-spacing: 0.04em`.

Arquivos `.ttf` em `docs/FONTES/Lato/` e `docs/FONTES/Lora/static/`. OFL.

## Logo

Três variações em `docs/SVG/`:

1. **Logo principal** (vertical) — símbolo + "SANY&Z" + slogan abaixo.
2. **Logo secundário** (horizontal) — símbolo à esquerda + "SANY&Z" à direita.
3. **Símbolo** (ícone) — só a sacola estilizada com S/Z entrelaçados.

Variações de cor disponíveis: `alabaster`, `antracito`, `copper-brown`, `copper-sany`, `copper-z`, `onyx`, `preto`, `quartz` — escolher conforme contraste com o fundo.

### Uso correto

- Versão copper sobre fundo claro (alabaster/quartz/branco).
- Versão alabaster sobre fundo copper ou antracito/onyx.
- Manter proporções originais do SVG.

### Uso incorreto

- Aplicar logo copper sobre fundo copper-brown (sem contraste).
- Esticar/deformar.
- Trocar a cor para fora da paleta (ex: vermelho).
- Sobrepor a imagem sem garantir legibilidade.

## Spacing & shape (convenções de implementação)

Não vêm explicitadas no manual — derivadas do design HTML em `docs/design/`:

- **Radii:** `8px` (sm), `12px` (md), `18px` (lg), `24px` (xl).
- **Shadows (light):**
  - sm: `0 2px 8px rgba(51,42,39,0.06), 0 1px 2px rgba(51,42,39,0.04)`
  - md: `0 8px 24px rgba(51,42,39,0.08), 0 2px 4px rgba(51,42,39,0.04)`
  - lg: `0 24px 48px rgba(51,42,39,0.12), 0 4px 8px rgba(51,42,39,0.05)`

## Onde estão os assets

```
docs/
├─ Manual de Marca - SANY&Z.pdf   (referência original — 12 págs, 61MB)
├─ brand.md                        (este arquivo)
├─ FONTES/
│  ├─ Lato/                        (.ttf estáticos)
│  └─ Lora/                        (variable font + estáticos em static/)
├─ SVG/                            (logo em todas variações de cor)
└─ MOCKUPS/                        (PNGs de mockup p/ comunicação)
```
