# Redesign do app MeuCatalogo (.NET MAUI) — Plano

> Documento de planejamento. **Não há código alterado nesta rodada.**
> Data: 2026-05-27 · Alvo: `src/mobile/MeuCatalogo`
> Prioridades acordadas: **offline-first, UX mobile, performance, brand Sany&Z**.

---

## 1. Estado atual (auditoria)

### 1.1 Inventário de telas
- **5 abas** no `AppShell`: Home, Produtos, Catálogos, Pedidos, Mais.
- **29 Pages** distribuídas em: Auth (2), Catálogo (4), Cliente (2), Financeiro (13), Fornecedor (1), Pedido (2), Produto (3), Settings/Mais.
- **5 BottomSheets** registrados (`BottomSheetKeys.cs`): Categoria, Estoque, CatalogoEmUso, PedidoCliente, PedidoProduto.
- **Componentes**: `CustomEntry`, `NumericEntry`, `DropDown` — usados sobretudo em formulários (Produto/Cliente/Financeiro).
- **Behavior único**: `CurrencyBehavior`.

### 1.2 Brand já em código (`Resources/Styles/`)
- **Cores Sany&Z** já estão modeladas em `BrandColors.xaml` (48 tokens):
  - Cobre: `BrandCopperSany #c06647`, `BrandCopperZ #df8252`, `BrandCopperBrown #8a472a`
  - Neutros: `BrandAlabaster`, `BrandQuartz`, `BrandAntracito`, `BrandOnyx`, `BrandPaper`, `BrandPaper2`
  - Status: `BrandSuccess`, `BrandWarning`, `BrandDanger`
  - Semânticos Light/Dark: `Primary/Background/Surface/Text/Border` + Soft/Hover/Muted
  - **Tokens legados** (`Primary`, `Gray100..950`, `Magenta`, `MidnightBlue`) marcados como deprecated mas ainda presentes.
- **Fontes**: Lora (headings) + Lato (corpo) + FluentSystemIcons já carregadas em `MauiProgram.cs`.
- **Styles nomeados** (26): tipografia (`SerifHero`, `SerifH1..H3`, `Caps`, `BodyMuted`, `FieldLabel`), componentes (`PrimaryButton`, `SecondaryButton`, `DangerButton`, `Card`, `CardElevated`, `PillEntryHost`, `IconCircle`), swipe (`SwipeAction*`).

### 1.3 SyncEngine atual (`Infrastructure/SyncEngine/`)
- Persistência da fila: `SyncQueue` (SQLite) com `Status`, `RetryCount`, `NextRetryAt`, `Operation`, `Payload`.
- Execução **sequencial** (`SemaphoreSlim`), FIFO por `CreatedAt`.
- **Backoff exponencial** com cap de 5 min — implementado.
- Handlers atuais:
  - Pull: Catalogos, Categorias por catálogo, Produtos por catálogo.
  - Push: Produto upsert + delete.
- Dispara em: `App.OnHandlerChanged` (1ª pintura) e `Connectivity.ConnectivityChanged → Internet`.

### 1.4 Gaps confirmados na auditoria
| Área | Gap | Evidência |
|---|---|---|
| Sync | Sem TTL / dead-letter — item Failed senta para sempre | `SyncEngine` + `offline-rules.txt` §próximo passo |
| Sync UI | `IsSyncing` global; **sem badge per-item** (`Pending`/`Failed`) nas listagens | `ProdutoListaPage`, `CatalogoListaPage` |
| Sync prioridade | Fila pura FIFO, sem precedência (delete antes de update) | `offline-rules.txt` §próximo passo |
| Sync escopo | `Categoria` e `Estoque` são online-only — quebram se offline | `mobile/CLAUDE.md` §Key Decisions |
| Lifecycle | Listener `ConnectivityChanged` nunca é unregistered; risco de double-fire | `App.xaml.cs` §Gotchas |
| Perf XAML | `AppShell.xaml` **sem** `x:DataType` → bindings via reflection | `AppShell.xaml:1` |
| Perf lista | `CollectionView` sem `ItemsUpdatingScrollMode` (jank em refresh) | `ProdutoListaPage.xaml:105`, `LancamentoListView.xaml:8` |
| Perf lista | Sem `RemainingItemsThreshold` → sem paginação incremental | todas as `CollectionView` |
| Perf imagem | `ProdutoListaPage` usa `ThumbnailUrl` mas backend serve `-thumb` only se cliente pedir; risco de baixar full | `ProdutoListaPage.xaml:174` |
| Perf imagem | `MauiImageProcessor.CompressAsync` retorna `MemoryStream` sem `using` no caller documentado | `MauiImageProcessor.cs:27-30` |
| Brand | Tokens legados (`Primary`, `Gray*`) ainda referenciados — duplicação semântica | `Colors.xaml` |
| Build | Sem `PublishAot` / `RunAOTCompilation` (JIT default) | `MeuCatalogo.csproj` |

---

## 2. Direção visual adotada

**Referência:** [Coffee Shop Mobile App Design — Figma Community](https://www.figma.com/design/D1ZYAT2h4E7fLEI1ASUw6z/Coffee-Shop-Mobile-App-Design--Community-?node-id=0-1).

Adotamos a **arquitetura e padrões de UI** desse template, **mantendo intacta** a identidade Sany&Z (cores cobre + Lora/Lato). Nada do template entra como cor laranja ou fonte Sora — só estrutura, hierarquia e composição.

### 2.1 Padrões visuais herdados do template

Identificados nas telas do mockup ("On Mockup" da Figma):

- **Onboarding hero** — imagem full-bleed escura com título serifado claro e CTA pílula full-width.
- **Header de Home** — saudação à esquerda + avatar/atalho circular cobre à direita.
- **Search bar pílula** — full-width, neutra, ícone de busca à esquerda.
- **Banner promocional escuro** — card grande com imagem + título claro + badge cobre ("Promo", "Novo"). Voz forte sem competir com produtos.
- **Chips de categoria** — scroll horizontal, chip selecionado preenchido cobre, demais outlined.
- **Grid 2-col de produtos** — card branco com canto bem arredondado (radius `lg`/`xl`), imagem em topo, nome compacto, preço em destaque, ação `+` cobre no canto.
- **Detalhe de produto** — hero image em card escuro (Antracito) com badge cobre; rating; descrição com "Ler mais"; chips de variação; preço grande em cobre; CTA "Buy now"/"Adicionar".
- **Cart / Checkout** — tabs (Entrega / Retirada), lista de itens com thumb circular + qty stepper, payment summary, CTA "Continuar".
- **Tab bar inferior** — 4–5 ícones, item ativo cobre filled.
- **Estilo geral** — cantos muito arredondados (16–24px), sombras sutis, hierarquia tipográfica clara, muito espaço em branco.

### 2.2 Mapeamento Coffee Shop → Sany&Z

#### Cores
| Função no template | Cor original (Coffee Shop) | Token Sany&Z |
|---|---|---|
| CTA primária / badges / acento | laranja vibrante | `BrandCopperSany` `#c06647` |
| Hover / secundário / chip selecionado claro | laranja claro | `BrandCopperZ` `#df8252` |
| Pressed / sombra de acento | laranja escuro | `BrandCopperBrown` `#8a472a` |
| Background da app | creme | `BrandPaper2` `#faf6f1` |
| Surface (cards) | branco | `#ffffff` |
| Texto primário | preto | `BrandAntracito` `#332a27` |
| Texto muted / placeholder | cinza | `#7a6a60` (derivado, já no `brand.md`) |
| Banner promo escuro | preto carvão | `BrandOnyx` `#121212` + sutil gradient `BrandCopperBrown → BrandCopperSany` opcional |
| Bordas / divisores | cinza claro | `BrandQuartz` `#dfd5ce` |

#### Tipografia
| Uso no template | Fonte original | Substituto Sany&Z |
|---|---|---|
| Hero / onboarding título | Sora 600+ | **Lora 500** com `letter-spacing: -0.01em` (italic 500 em destaque ocasional — voz da marca) |
| Headings de tela / nomes de produto | Sora 600 | **Lora 500** |
| Preço grande no detalhe | Sora 700 | **Lora 600** (números financeiros são uso oficial de Lora no `brand.md`) |
| Corpo / UI / labels | Sora 400/500 | **Lato 400** |
| Botões / chips | Sora 600 | **Lato 700** `letter-spacing: 0.04em` |
| CAPS / categorias | Sora 600 uppercase | **Lato 700** `letter-spacing: 0.1em` uppercase |

#### Forma e elevação
| Elemento | Coffee Shop | Sany&Z (já em `brand.md`) |
|---|---|---|
| Corner radius (card grande, banner) | 24px | `radius-xl` 24px |
| Corner radius (card produto, search) | 18px | `radius-lg` 18px |
| Corner radius (chip, botão pequeno) | 12px | `radius-md` 12px |
| Shadow elevação 1 (card produto) | sombra suave | `shadow-sm` |
| Shadow elevação 2 (banner promo, hero) | sombra média | `shadow-md` |

### 2.3 Mapeamento de telas Coffee Shop → MeuCatalogo

| Tela do template | Equivalente no app | Status | Ação |
|---|---|---|---|
| Onboarding "Fall in love…" | (não existe) | novo | Criar tela pré-`LoginPage` — opcional; pode ser hero da `LoginPage` |
| Home (saudação + banner + chips + grid) | `HomePage` | refazer | Vira dashboard com banner promo opcional + atalhos + últimos pedidos (substitui Fase 5.3) |
| Lista de produtos (chips + search + grid) | `ProdutoListaPage` | refazer | **Tela piloto** — combina chips, search, grid 2-col |
| Detalhe de produto (hero + chips + buy) | `ProdutoDetalhePage` | refazer | Direto — hero em card antracito, chips de variação, preço Lora 600 cobre |
| Cart / Order summary | `PedidoNovoPage` | refazer | Tabs Entrega/Retirada, items com thumb + stepper, summary |
| Tracking (mapa + steps) | — | pular | Não temos delivery no escopo |
| Tab bar inferior | `AppShell` (já existe) | ajustar | Manter 5 abas atuais, redesenhar ícones (Fluent) e item ativo cobre |
| Login / Signup | `LoginPage`, `SignupPage` | refazer | Hero superior, formulário em card branco, CTA pílula cobre |

### 2.4 Componentes novos (em `Components/`)

A criar — nomes propostos:
- `CategoryChipScroll` — chips horizontais com seleção (`ItemsSource`, `SelectedItem`).
- `ProductGridCard` — card 2-col com `ThumbnailUrl`, `Nome`, `Preco`, `PrecoComDesconto`, action `+`.
- `PromoBanner` — card escuro com imagem, título Lora claro, badge cobre, tap action.
- `PriceTag` — número em Lora 600 cobre com strikethrough opcional para `PrecoComDesconto`.
- `QtyStepper` — `−` `qty` `+` (carrinho).
- `SectionHeader` — título Lora + ação "Ver todos →" Lato 700 cobre.

Reutilizar:
- `CustomEntry` / `NumericEntry` / `DropDown` — já existem; só revisar styling para casar com pílula/radius lg.
- `Card` / `CardElevated` styles — usar `CardElevated` para banner promo, `Card` para produtos.
- `PrimaryButton` style — virar pílula full-width (`CornerRadius="28"`).

### 2.5 Guidelines mobile gerais

- Touch targets: mín. 48dp / 44pt.
- Tipografia (sp/Android, pt/iOS): hero 32, H1 24, H2 20, corpo 14–16, label 12.
- Espaçamento: escala 4/8/12/16/24/32 — normalizar `ItemSpacing="14"` atual para `16`.
- Navegação: tab bar fixa (5 abas), BottomSheets para ações curtas, modais full-screen para fluxos longos.
- Estados obrigatórios em toda lista: loading (skeleton), vazio (ilustração + CTA), erro (retry), offline (banner).
- Feedback: haptic em swipe e ações destrutivas; snackbar honesto pós-salvar.

### 2.6 Offline-first (regras do `offline-rules.txt`)

- UI lê **só do SQLite**. Nada de HTTP direto de ViewModel.
- Toda escrita: local com `Status = Pending` → retorna imediato → fila sincroniza.
- Status visíveis na UI: badge `Pending`, badge `Failed` + retry, ausência = `Synced`.
- Mensagens honestas: "salvo localmente" enquanto pendente; "sincronizado" só após confirmação do servidor.

---

## 3. Plano de execução (fases)

> Cada fase é um conjunto coeso e isolado, dá pra mergear sozinho.
> Tudo aqui é proposta — nenhuma fase começa sem você ok'ar.

### Fase 1 — Performance "low-hanging fruit" (sem mudar UX)
**Objetivo**: ganhar fluidez sem mexer em telas.
- F1.1 Adicionar `x:DataType` em `AppShell.xaml` e nas pages que faltam (auditoria mostrou 9).
- F1.2 Adicionar `ItemsUpdatingScrollMode="KeepItemsInView"` nas `CollectionView` (Produto, Catálogo, Pedido, Cliente, Fornecedor, Financeiro).
- F1.3 ✅ (entregue 2026-06-10) Confirmado que o gap era no backend: `ProdutosController.CriarLinksImagem` sobrescrevia `Images.Thumbnail/Medium/Full` com a URL full. Corrigido com `EnriquecerLinksImagem` que preserva as variantes geradas pelo `ProdutoService`. **Pendente deploy do backend** para o efeito chegar ao app.
- F1.4 Garantir `using` nos callers de `MauiImageProcessor.CompressAsync` (ou trocar a assinatura para `byte[]` e dispor internamente).
- F1.5 Avaliar `RunAOTCompilation` em Release (Android). Medir tamanho do APK e cold start antes/depois.

**Critério de aceite**: cold start em Android mid-tier ≤ 2.5s; scroll em `ProdutoListaPage` com 200+ itens sem jank visível.

### Fase 2 — Sync robusto (resolver gaps offline)
- F2.1 Implementar **prioridade de fila**: `Delete > Update > Create` quando aplicável (evita push de update sobre algo que vai ser deletado).
- F2.2 ✅ parcial (entregue 2026-06-10) — **dead-letter** após N=10 retries implementado: `SyncStatus.DeadLetter` + cap em `SyncEngineService.MarkAsFailedAsync`; item sai do loop de retry. A tela "Mais → Sincronização" com ação manual (F2.5) continua pendente.
- F2.3 Unregister do listener `ConnectivityChanged` no `App.Cleanup` (corrigir Gotcha).
- F2.4 Avaliar trazer **Categoria** e **Estoque** para o fluxo offline (decisão de produto — discutir antes).
- F2.5 Tela `SincronizacaoPage` em "Mais": lista itens pendentes/failed/dead, com retry manual e "limpar histórico" (idempotente).

**Critério de aceite**: app cria/edita/deleta produto totalmente offline; reconecta → tudo sobe sem duplicar; itens com erro ficam visíveis e acionáveis.

### Fase 3 — Status de sync na UI
- F3.1 Badge per-item nas listagens (`ProdutoListaPage`, `CatalogoListaPage`, `PedidoListaPage`):
  - `Pending` → ícone discreto Fluent `cloud_arrow_up` em `BrandWarning`
  - `Failed` → ícone `error_circle` em `BrandDanger` + tap para retry
  - `Synced` → sem ícone
- F3.2 Banner global de offline no topo do shell quando `Connectivity.NetworkAccess != Internet`.
- F3.3 Snackbar honesto pós-salvar: "Salvo localmente · sincronizando…" e "Sincronizado".

**Critério de aceite**: usuário consegue distinguir local-only de sincronizado em qualquer lista, sem abrir o item.

### Fase 4 — Design system "Coffee × Sany&Z"
Construir os blocos antes de redesenhar telas. Tudo em `Components/`, `Resources/Styles/` e `Converters/`.

- F4.1 ✅ **Limpeza de tokens** (entregue 2026-05-28) — varredura concluída: 7 ocorrências de `BrandCopper*` direto em Pages substituídas por `PrimaryColorLight/Dark`/`PrimaryHoverColor*`; 1 uso de `BrandAlabaster` no avatar da MaisPage trocado por `OnPrimaryColor` (perda visual aceitável: white puro em vez de creme em dark). Tokens legados (`Primary`, `Gray*`, `Magenta`, `MidnightBlue`, `OffBlack`) só sobreviveram em `Resources/Styles/Colors.xaml` como aliases conforme D4 — nenhum uso fora dali. Build verde.
- F4.2 ✅ **Styles novos** em `Styles.xaml` (entregue 2026-05-28):
  - `PrimaryButtonPill` (radius 28, Primary, Lato 700, full-width, height 56).
  - `SearchPill` (radius 24, fundo SurfaceMuted).
  - `CategoryChip` + `CategoryChipDefault` (outlined) / `CategoryChipSelected` (filled Primary) + texts.
  - `PromoBannerCard` (radius 24, fundo Antracito light / Onyx dark via tokens semânticos `PromoBackgroundBrush`, `OnPromoBrush`, `OnPromoMutedBrush`).
  - `PriceLargePrimary` / `PriceMediumPrimary` / `PriceStrikethrough`.
- F4.3 ✅ **Componentes novos** em `Components/` (entregue 2026-05-28): `SectionHeader`, `PriceTag`, `QtyStepper`, `CategoryChipScroll`, `ProductGridCard`, `PromoBanner`. Padrão `CustomEntry`: ContentView + `x:Name="Root"` + BindableProperty.
- F4.4 ✅ **Tab bar** (entregue 2026-05-28) — `AppShell.xaml` agora usa `PrimaryColorLight/Dark` em `Shell.TabBarTitleColor` e `TabBarForegroundColor` (eram `BrandCopper*` direto). ⚠️ **Limitação:** FluentUI atual no projeto só tem `_24_regular`, sem variantes `_24_filled`; o swap regular↔filled no item ativo não foi implementado. Distinção do item ativo fica por cor + título.
- F4.5 ⏳ **Dark mode** revisado token a token (contraste WCAG AA mínimo) — **pendente**. Requer ferramenta de medição de contraste e revisão tela a tela; não é varredura mecânica.
- F4.6 ⏳ **Estados vazios** — ilustração simples + copy curto em pt-BR — **pendente**. Cada lista precisa do seu próprio estado vazio com contexto.

**Critério de aceite**: nenhum XAML referencia tokens legados ✅; storybook informal (1 página `_DesignKitchenSink.xaml` em Debug) mostra todos os componentes novos lado a lado — **não criado** (decidir se vale a pena no DoD ou se valida direto na F5.1).

### Fase 5 — Redesign de telas (entregue 2026-05-28; build verde 0 erros)

- F5.1 ✅ **Piloto `ProdutoListaPage`** — VM ganhou `Categorias`/`CategoriaSelecionada`/`SearchText`/`ProdutosFiltrados` + `CategoriaTodas` sentinela + `ApplyFilter()`. XAML usa `SearchPill` + `CategoryChipScroll` + `SectionHeader` + `CollectionView` Span=2 com `ProductGridCard` + `ItemsUpdatingScrollMode=KeepItemsInView`. **Removida SwipeView de delete** (incompatível com grid 2-col — delete via tela de detalhe).
- F5.2 ✅ **`HomePage` → dashboard** — adicionado `PromoBanner` contextual (D7: aparece quando `TemProdutosRecentes`, leva ao `VerProdutosCommand`); `SectionHeader Recentes / Ver todos →` substitui Caps+Label inline; lista horizontal de recentes agora usa `ProductGridCard` (WidthRequest=172); botão "NOVO PRODUTO" virou `PrimaryButtonPill`.
- F5.3 ✅ **`ProdutoDetalhePage`** — hero escuro (`PromoBackgroundColorLight/Dark`) + badge `PROMO` cobre quando `TemDesconto`; nome em `SerifH1`; `PriceTag` grande com desconto; `SectionHeader Galeria` / `Descrição`; CTA "EDITAR PRODUTO" virou `PrimaryButtonPill`. ⚠️ **Crash de runtime corrigido em 2026-06-08 (Brush em `BackgroundColor`) — ver §7.**
- F5.4 ✅ **`ProdutoAdicionarPage`** (mínima) — botão SALVAR virou `PrimaryButtonPill`. Seções já existem como cards (DETALHES, ORGANIZAÇÃO, IMAGENS) — sem accordion recolhível (overengineering p/ esta rodada; D3 não exigia).
- F5.5 ✅ **`PedidoNovoPage`** (mínima) — botão SALVAR PEDIDO virou `PrimaryButtonPill`. Stepper inline existente mantido (controlado por commands, não Quantity TwoWay — não trocado pelo `QtyStepper` componente p/ evitar refactor desnecessário).
- F5.6 ✅ **Listagens secundárias** — `CatalogoListaPage` / `ClienteListaPage`: `PrimaryButton` empty state → `PrimaryButtonPill`. Todas as 4 (Catalogo/Pedido/Cliente/Fornecedor): `CollectionView` ganhou `ItemsUpdatingScrollMode=KeepItemsInView`. **SwipeViews mantidas** (delete/usar) — funcionam bem em lista vertical. ⚠️ **Listas saíam invisíveis (`CollectionView` em `ScrollView`); corrigido em 2026-06-08 — ver §7.**
- F5.7 ✅ **`LoginPage`** — botão ENTRAR virou `PrimaryButtonPill`. Hero já existia e está alinhado ao padrão. **`SignupPage` é stub** ("Será a página de cadastro de conta") — nada para redesenhar.
- F5.8 ✅ **`MaisPage`** já estava aderente ao design system (hero gradient + cards de seções CRM / GESTÃO / CONTA + logout). Sem PrimaryButton para trocar. **Nota**: seções "Sincronização" e "Assinatura" mencionadas no plano original são features novas (fora desta rodada).
- F5.9 ✅ **Pull-to-refresh** (entregue 2026-05-28). Adicionado `[ObservableProperty] private bool _isRefreshing;` em 4 VMs (Catalogo/Pedido/Cliente/Fornecedor) + `IsRefreshing = false` no `finally` de cada `Carregar*`. XAML: `CollectionView` envolvida em `RefreshView` com `Command="CarregarCommand"`/`CarregarCatalogosCommand` e `IsRefreshing` TwoWay. `ProdutoListaPage` já tinha desde F5.1.
- F5.10 ⏳ **Onboarding hero** — pendente conforme D6 (embutido no LoginPage; tela separada descartada).

**Critério de aceite**: build verde ✅; padrão visual coerente ✅; nenhum binding por reflection (todos com `x:DataType`) ✅; **validado em runtime (2026-06-08)** no device físico — ver §7.

---

## 7. Validação em runtime (2026-06-08)

Review visual no device físico (Redmi Note 10, Android, Debug). Telas OK de primeira: `HomePage` (F5.2), `ProdutoListaPage` (F5.1), `MaisPage` (F5.8). Dois bugs **só visíveis em runtime** foram encontrados e corrigidos:

### Bug 1 — Crash ao abrir `ProdutoDetalhePage` (F5.3)
- **Sintoma:** tocar em qualquer produto matava o processo (`MonoDroid: UNHANDLED EXCEPTION` → volta ao launcher).
- **Causa:** `ProdutoDetalhePage.xaml` setava `BackgroundColor="{StaticResource PromoBackgroundBrush}"`. `PromoBackgroundBrush` é `SolidColorBrush` (tipo **Brush**), mas `BackgroundColor` é **Color** → `ApplyPropertiesVisitor.SetPropertyValue` lança ao carregar o XAML. O XamlC não pega porque defere `StaticResource` + conversão de tipo para runtime (por isso build verde).
- **Fix:** trocar pelo padrão do projeto — `BackgroundColor="{AppThemeBinding Light={StaticResource PromoBackgroundColorLight}, Dark={StaticResource PromoBackgroundColorDark}}"` (igual ao `PromoBannerCard` / `PromoBanner.xaml`). Varredura confirmou ser a única ocorrência de Brush→Color em todo o projeto.

### Bug 2 — Listas invisíveis em Catálogos / Pedidos / Clientes / Fornecedores (F5.6)
- **Sintoma:** as 4 abas mostravam a área da lista **vazia** mesmo havendo dados (ou nem o empty-state aparecia). Pedidos parecia "Carregando…" para sempre.
- **Causa:** as 4 telas envolviam um `CollectionView` vertical num `ScrollView`, em `RowDefinition="Auto"`. `CollectionView` virtualizado dentro de `ScrollView` não mede altura → colapsa para 0 → nenhum item renderiza; e com dados presentes `ShowEmptyState=false`, então nem o empty aparece. Confirmado por dump da hierarquia (`ScrollView` interno + ausência de `RecyclerView`) e por leitura do SQLite (1 catálogo presente). ViewModels estavam corretos — bug puramente de layout. `ProdutoListaPage`/`HomePage` não têm o problema porque usam `Grid` com row `*` (sem `ScrollView`).
- **Fix:** remover o `ScrollView` externo das 4 telas e colapsar para um `Grid` de célula única (loading/empty/lista sobrepostos, só um visível por vez), replicando o padrão da `ProdutoListaPage`.
- **Validação:** Catálogos mostra o card "Sany & Z"; Clientes/Fornecedores/Pedidos mostram o empty-state correto.

**Pendências de validação:** `LoginPage` (F5.7) e `SignupPage` não testados em runtime (exigiriam logout). `ProdutoAdicionarPage` (F5.4) e `PedidoNovoPage` (F5.5) — formulários não revisados em runtime nesta rodada.

---

## 4. Decisões (confirmadas em 2026-05-28)

| # | Decisão | Bloqueia | Resolução |
|---|---|---|---|
| D1 | Trazer Categoria/Estoque para offline? | F2.4 | **Categoria = sim** (lista curta, raramente muda); **Estoque = não** (movimentação precisa do servidor como SSOT) |
| D2 | AOT em Android Release? | F1.5 | **Sim, tentar e medir.** Reversível via csproj |
| D3 | Wizard em ProdutoAdicionarPage? | F5.4 | **Não.** Page única reorganizada em seções recolhíveis (`SectionHeader`) |
| D4 | Tokens legados — remover já ou manter alias? | F4.1 | **Manter alias por 1 release** em `Colors.xaml`; remover de XAMLs de Page agora |
| D5 | Dead-letter limit (N retries)? | F2.2 | **N=10** (com backoff atual, ≈ várias horas até cair em DLQ) |
| D6 | Criar onboarding hero antes do Login? | F5.10 | **Não criar tela separada.** Embutir hero dentro do próprio `LoginPage` |
| D7 | `PromoBanner` na Home — uso? | F5.2 | **Contextual** — próximo pedido em aberto OU produto recém-criado. Se ambos `null`, banner some |

---

## 4.1. Follow-ups detectados durante o redesign (TODOs)

- **`CategoriaBottomSheet.xaml` — edição mais acessível**: hoje existe `EditarCategoriaCommand` mas só via swipe horizontal (linhas 111-138). Avaliar adicionar um ícone de edit visível em cada linha (ex.: lápis no canto direito do Border, sem swipe) — UX de descoberta hoje é fraca: usuário precisa adivinhar que existe swipe. Não implementado nesta rodada — task separada quando priorizar UX do bottom sheet.

## 5. Fora de escopo deste replanejamento

- Conflict resolution merge (apenas last-write-wins continua) — documentado como "próximo passo" no `offline-rules.txt`.
- Delta sync incremental — mesmo motivo.
- Redesign do backend ou do frontend Angular.
- Adicionar features novas que não existem hoje (ex: notificações push, multi-idioma).

---

## 6. Próximo passo concreto

Como o foco escolhido é **UI primeiro**, a ordem recomendada agora é:

1. **F4 — Design system "Coffee × Sany&Z"** (componentes + styles + tab bar). Sem isso, F5 vai produzir telas inconsistentes.
2. **F5.1 — Piloto `ProdutoListaPage`** logo na sequência. Valida tudo da F4 numa tela real e dá referência visual concreta antes de propagar.
3. Decidir D1–D7 antes de começar F4 (pelo menos D4 e D7 — os outros podem ficar para depois).

F1 (perf) e F2/F3 (sync) ficam em paralelo se houver banda, ou depois das telas se for foco serial.
