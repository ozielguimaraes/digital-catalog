// shared.jsx — Ícones, dados mock, componentes comuns

// ────────────────────────────────────────────────────────────
// Ícones (stroke 1.8, tamanho padrão 20)
// ────────────────────────────────────────────────────────────
const Icon = ({ d, size = 20, fill = 'none', stroke = 'currentColor', sw = 1.8, children, viewBox='0 0 24 24' }) => (
  <svg width={size} height={size} viewBox={viewBox} fill={fill} stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round">
    {d && <path d={d}/>}
    {children}
  </svg>
);

const I = {
  home: (p) => <Icon {...p}><path d="M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6h-6v6H4a1 1 0 0 1-1-1z"/></Icon>,
  grid: (p) => <Icon {...p}><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></Icon>,
  bag: (p) => <Icon {...p}><path d="M5 8h14l-1 12H6zM8 8V6a4 4 0 0 1 8 0v2"/></Icon>,
  users: (p) => <Icon {...p}><path d="M16 20v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 20v-2a4 4 0 0 0-3-3.87M15 3.13a4 4 0 0 1 0 7.75"/></Icon>,
  truck: (p) => <Icon {...p}><path d="M1 3h15v13H1zM16 8h4l3 3v5h-7zM5.5 21a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5zM18.5 21a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z"/></Icon>,
  coin: (p) => <Icon {...p}><circle cx="12" cy="12" r="9"/><path d="M15 9.5a3 3 0 0 0-3-1.5c-1.5 0-3 1-3 2.5 0 3 6 1.5 6 4.5 0 1.5-1.5 2.5-3 2.5a3 3 0 0 1-3-1.5M12 6v2M12 16v2"/></Icon>,
  chart: (p) => <Icon {...p}><path d="M3 3v18h18M7 15l4-4 3 3 5-7"/></Icon>,
  settings: (p) => <Icon {...p}><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></Icon>,
  plus: (p) => <Icon {...p}><path d="M12 5v14M5 12h14"/></Icon>,
  search: (p) => <Icon {...p}><circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/></Icon>,
  filter: (p) => <Icon {...p}><path d="M3 5h18M6 12h12M10 19h4"/></Icon>,
  heart: (p) => <Icon {...p}><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></Icon>,
  heartFill: (p) => <Icon {...p} fill="currentColor" sw={0}><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></Icon>,
  star: (p) => <Icon {...p}><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></Icon>,
  starFill: (p) => <Icon {...p} fill="currentColor" sw={0}><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></Icon>,
  arrowLeft: (p) => <Icon {...p}><path d="M19 12H5M12 19l-7-7 7-7"/></Icon>,
  arrowRight: (p) => <Icon {...p}><path d="M5 12h14M12 5l7 7-7 7"/></Icon>,
  chevRight: (p) => <Icon {...p}><path d="m9 18 6-6-6-6"/></Icon>,
  chevDown: (p) => <Icon {...p}><path d="m6 9 6 6 6-6"/></Icon>,
  more: (p) => <Icon {...p}><circle cx="12" cy="5" r="1" fill="currentColor"/><circle cx="12" cy="12" r="1" fill="currentColor"/><circle cx="12" cy="19" r="1" fill="currentColor"/></Icon>,
  moreH: (p) => <Icon {...p}><circle cx="5" cy="12" r="1" fill="currentColor"/><circle cx="12" cy="12" r="1" fill="currentColor"/><circle cx="19" cy="12" r="1" fill="currentColor"/></Icon>,
  edit: (p) => <Icon {...p}><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></Icon>,
  trash: (p) => <Icon {...p}><path d="M3 6h18M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2m3 0v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"/></Icon>,
  share: (p) => <Icon {...p}><circle cx="18" cy="5" r="3"/><circle cx="6" cy="12" r="3"/><circle cx="18" cy="19" r="3"/><path d="m8.59 13.51 6.83 3.98M15.41 6.51l-6.82 3.98"/></Icon>,
  qr: (p) => <Icon {...p}><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><path d="M14 14h3v3h-3zM20 14v3M14 20h3v1h-3zM20 17v4"/></Icon>,
  camera: (p) => <Icon {...p}><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></Icon>,
  image: (p) => <Icon {...p}><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="m21 15-5-5L5 21"/></Icon>,
  crop: (p) => <Icon {...p}><path d="M6 2v14a2 2 0 0 0 2 2h14M18 22V8a2 2 0 0 0-2-2H2"/></Icon>,
  sliders: (p) => <Icon {...p}><path d="M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M1 14h6M9 8h6M17 16h6"/></Icon>,
  upload: (p) => <Icon {...p}><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12"/></Icon>,
  check: (p) => <Icon {...p}><path d="M20 6 9 17l-5-5"/></Icon>,
  x: (p) => <Icon {...p}><path d="M18 6 6 18M6 6l12 12"/></Icon>,
  phone: (p) => <Icon {...p}><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></Icon>,
  mail: (p) => <Icon {...p}><rect x="2" y="4" width="20" height="16" rx="2"/><path d="m22 7-10 6L2 7"/></Icon>,
  map: (p) => <Icon {...p}><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></Icon>,
  tag: (p) => <Icon {...p}><path d="M20.59 13.41 13.42 20.58a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/><circle cx="7" cy="7" r="1" fill="currentColor"/></Icon>,
  trend: (p) => <Icon {...p}><path d="M23 6 13.5 15.5l-5-5L1 18"/><path d="M17 6h6v6"/></Icon>,
  trendDown: (p) => <Icon {...p}><path d="M23 18 13.5 8.5l-5 5L1 6"/><path d="M17 18h6v-6"/></Icon>,
  calendar: (p) => <Icon {...p}><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></Icon>,
  box: (p) => <Icon {...p}><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><path d="m3.3 7 8.7 5 8.7-5M12 22V12"/></Icon>,
  eye: (p) => <Icon {...p}><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></Icon>,
  bell: (p) => <Icon {...p}><path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9M13.73 21a2 2 0 0 1-3.46 0"/></Icon>,
  wa: (p) => <Icon {...p} fill="currentColor" sw={0}><path d="M20.52 3.48A12 12 0 0 0 3.47 20.52l-1.34 4.9 5.02-1.32A12 12 0 1 0 20.52 3.48zm-8.52 18.1a9.57 9.57 0 0 1-4.88-1.34l-.35-.2-3.1.81.83-3-.23-.36a9.58 9.58 0 1 1 7.73 4.1zm5.27-7.17c-.29-.15-1.71-.84-1.97-.94-.27-.1-.46-.15-.66.15-.19.29-.75.94-.92 1.13-.17.19-.34.22-.63.07-.29-.15-1.22-.45-2.33-1.44-.86-.77-1.44-1.71-1.61-2-.17-.29-.02-.45.13-.6.13-.13.29-.34.43-.51.15-.17.19-.29.29-.48.1-.19.05-.36-.02-.51-.07-.15-.66-1.58-.9-2.17-.23-.57-.47-.49-.66-.5h-.57c-.19 0-.5.07-.76.36-.26.29-1 .97-1 2.37 0 1.4 1.02 2.76 1.17 2.95.15.19 2.02 3.07 4.89 4.31.68.29 1.22.47 1.63.6.68.22 1.31.19 1.8.12.55-.08 1.71-.7 1.95-1.37.24-.67.24-1.25.17-1.37-.07-.12-.26-.19-.55-.34z"/></Icon>,
  zap: (p) => <Icon {...p}><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></Icon>,
  hamburger: (p) => <Icon {...p}><path d="M3 6h18M3 12h18M3 18h18"/></Icon>,
  lock: (p) => <Icon {...p}><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></Icon>,
  logo: (p) => <Icon {...p} viewBox="0 0 2000 2000" stroke="currentColor" fill="none" sw={60}><path d="M791.51,2000h1173.42L1768.09,38.72l-3.78-38.72H233.04L35.08,2000h84.85L309.52,86.02h1378.27l183.32,1826.63h-984.03l82.24-187.7c60.15-4.85,111.73-21.73,153.52-50.36,49.9-34.23,87.81-80.71,112.55-138.11,24.34-56.17,36.68-120.15,36.68-190.25,0-62.24-8.67-115.97-25.71-159.74-6.17-15.71-13.32-30.92-21.28-45.36l186.99-426.89v-66.12h-185.46c-4.44-61.99-17.55-114.85-39.08-157.4-27.6-54.54-65.26-95.36-111.84-121.17-45.92-25.56-97.96-38.52-154.8-38.52-66.02,0-122.5,15.51-167.86,46.12-45.2,30.41-79.34,70.87-101.38,120.26-21.63,48.21-32.6,102.04-32.6,159.95,0,82.19,19.13,148.62,56.79,197.5,36.38,47.3,84.03,86.58,141.43,116.53,40.1,21.38,83.62,41.43,129.44,59.69,43.27,17.19,84.23,38.27,121.68,62.65,27.81,18.16,52.14,41.63,72.35,69.95,5.87,8.16,10.87,15.87,15.2,23.32.46.77.92,1.58,1.33,2.45l1.02,1.84c21.07,38.21,31.73,90.36,31.73,154.95,0,54.54-9.34,105.05-27.7,150.05-17.76,43.37-45.66,78.21-83.11,103.67-29.64,20.2-67.55,32.55-112.5,36.68-9.9.92-20.41,1.43-30.82,1.58l-5.1.05c-67.19,0-121.43-15.71-161.28-46.63-40.51-31.48-70.41-73.83-88.88-125.82-19.13-53.83-28.37-113.32-27.45-176.79l.36-25.87h-82.04l-.36,25.15c-.97,78.42,10.97,151.22,35.51,216.33,25.1,66.89,65,121.07,118.57,160.97,42.7,31.89,95.26,51.33,156.48,58.06l-90.2,206.07v70.26ZM1169.26,1067.35c-26.33-26.12-55.66-48.93-87.45-68.06-42.96-25.15-88.37-47.6-135.1-66.63-44.18-17.96-86.12-38.11-124.64-59.85-35.77-20.2-65.36-47.5-87.91-81.12-21.94-32.7-33.06-77.91-33.06-134.34,0-42.35,7.45-82.24,22.14-118.57,14.23-35.31,36.89-63.52,69.23-86.17,31.63-22.19,74.85-33.42,128.42-33.42,42.76,0,81.84,8.78,116.07,26.07,33.32,16.79,59.13,44.29,78.88,84.08,15.2,30.77,24.8,70.61,28.62,118.78h-247.35v86.28h418.01l-145.87,332.96Z" fill="currentColor" sw={0}/></Icon>,
};

// ────────────────────────────────────────────────────────────
// Dados mock — ateliê Sany&Z
// ────────────────────────────────────────────────────────────
const BAG_SWATCHES = [
  { bg: 'linear-gradient(135deg, #d4a574 0%, #8a472a 100%)', label: 'Caramelo' },
  { bg: 'linear-gradient(135deg, #c06647 0%, #6b2818 100%)', label: 'Terracota' },
  { bg: 'linear-gradient(135deg, #3a2a22 0%, #121212 100%)', label: 'Onix' },
  { bg: 'linear-gradient(135deg, #ede8e2 0%, #c4b5a3 100%)', label: 'Alabaster' },
  { bg: 'linear-gradient(135deg, #8a6f5c 0%, #4a3829 100%)', label: 'Café' },
  { bg: 'linear-gradient(135deg, #df8252 0%, #b04d22 100%)', label: 'Copper' },
  { bg: 'linear-gradient(135deg, #b09488 0%, #6d4e41 100%)', label: 'Taupe' },
  { bg: 'linear-gradient(135deg, #dfd5ce 0%, #9c8978 100%)', label: 'Quartz' },
];

const PRODUCTS = [
  { id: 'p1', name: 'Bolsa Aurora', price: 489, stock: 6, status: 'available', swatch: 0, collection: 'Coleção Outono', desc: 'Bolsa estruturada em couro caramelo com detalhe em metal envelhecido. Feita sob medida em 12 dias.' },
  { id: 'p2', name: 'Clutch Âmbar', price: 289, stock: 0, status: 'oos', swatch: 1, collection: 'Coleção Outono' },
  { id: 'p3', name: 'Tote Maré', price: 629, stock: 3, status: 'available', swatch: 2, collection: 'Essenciais' },
  { id: 'p4', name: 'Saddle Lume', price: 539, stock: 2, status: 'available', swatch: 3, collection: 'Essenciais', favorite: true },
  { id: 'p5', name: 'Bolsa Ember', price: 459, stock: 4, status: 'custom', swatch: 5, collection: 'Coleção Outono' },
  { id: 'p6', name: 'Hobo Cais', price: 389, stock: 1, status: 'available', swatch: 4, collection: 'Essenciais' },
  { id: 'p7', name: 'Mini Sienna', price: 329, stock: 8, status: 'available', swatch: 6, collection: 'Coleção Outono' },
  { id: 'p8', name: 'Bolsa Oásis', price: 579, stock: 0, status: 'oos', swatch: 7, collection: 'Essenciais' },
];

const CATALOGS = [
  { id: 'c1', name: 'Coleção Outono 2026', products: 24, views: 318, favorite: true, updated: 'há 2 dias', cover: 0 },
  { id: 'c2', name: 'Essenciais Sany&Z', products: 18, views: 612, favorite: false, updated: 'há 1 semana', cover: 2 },
  { id: 'c3', name: 'Sob Encomenda', products: 9, views: 142, favorite: false, updated: 'há 3 dias', cover: 5 },
];

const CLIENTS = [
  { id: 'cl1', name: 'Marina Castello', email: 'marina.c@email.com', phone: '(11) 98234-1120', orders: 4, total: 1890, last: 'há 5 dias' },
  { id: 'cl2', name: 'Helena Duarte', email: 'helena.duarte@email.com', phone: '(11) 99541-2087', orders: 2, total: 980, last: 'há 2 semanas' },
  { id: 'cl3', name: 'Beatriz Moraes', email: 'bia.moraes@email.com', phone: '(21) 98812-4409', orders: 7, total: 3420, last: 'ontem' },
  { id: 'cl4', name: 'Juliana Pires', email: 'ju.pires@email.com', phone: '(11) 97705-5591', orders: 1, total: 489 },
  { id: 'cl5', name: 'Renata Almeida', email: 'renata.a@email.com', phone: '(31) 99214-3385', orders: 3, total: 1560, last: 'há 9 dias' },
];

const SUPPLIERS = [
  { id: 's1', name: 'Couro Marmota Ltda.', cat: 'Couros', contact: 'Roberto Lima', phone: '(11) 3421-0098', payable: 1280 },
  { id: 's2', name: 'Ferragens Vênus', cat: 'Metais & Fivelas', contact: 'Ana Souza', phone: '(11) 3881-2210', payable: 0 },
  { id: 's3', name: 'Atelier Forro Lino', cat: 'Forros & Têxtil', contact: 'Tania Costa', phone: '(11) 97002-3321', payable: 640 },
  { id: 's4', name: 'Metalúrgica Orla', cat: 'Metais & Fivelas', contact: 'João Pires', phone: '(11) 3222-4400', payable: 2190 },
];

const ORDERS = [
  { id: '#1248', client: 'Marina Castello', items: 1, total: 489, status: 'production', date: '12 abr' },
  { id: '#1247', client: 'Beatriz Moraes', items: 2, total: 928, status: 'ready', date: '10 abr' },
  { id: '#1246', client: 'Helena Duarte', items: 1, total: 329, status: 'delivered', date: '08 abr' },
  { id: '#1245', client: 'Juliana Pires', items: 1, total: 489, status: 'draft', date: '08 abr' },
  { id: '#1244', client: 'Renata Almeida', items: 3, total: 1560, status: 'delivered', date: '02 abr' },
];

const STATUS_LABEL = {
  draft: { label: 'Rascunho', chip: 'chip' },
  production: { label: 'Em produção', chip: 'chip-copper' },
  ready: { label: 'Pronto p/ entrega', chip: 'chip-yellow' },
  delivered: { label: 'Entregue', chip: 'chip-green' },
  cancelled: { label: 'Cancelado', chip: 'chip-red' },
};

const FINANCE = {
  receivable: [
    { id: 'r1', desc: 'Pedido #1248 — Marina Castello', amount: 489, due: '18 abr', status: 'pending' },
    { id: 'r2', desc: 'Pedido #1247 — Beatriz Moraes', amount: 928, due: '15 abr', status: 'pending' },
    { id: 'r3', desc: 'Pedido #1244 — Renata Almeida', amount: 1560, due: '02 abr', status: 'paid' },
    { id: 'r4', desc: 'Pedido #1246 — Helena Duarte', amount: 329, due: '22 abr', status: 'overdue' },
  ],
  payable: [
    { id: 'p1', desc: 'Couro Marmota — Nota 881', amount: 1280, due: '20 abr', status: 'pending' },
    { id: 'p2', desc: 'Aluguel ateliê — Abril', amount: 2400, due: '10 abr', status: 'paid' },
    { id: 'p3', desc: 'Metalúrgica Orla — Nota 442', amount: 2190, due: '25 abr', status: 'pending' },
    { id: 'p4', desc: 'Energia — Abril', amount: 312, due: '15 abr', status: 'overdue' },
  ],
};

// ────────────────────────────────────────────────────────────
// Bolsa ilustrada (SVG) — placeholder visual para produtos
// ────────────────────────────────────────────────────────────
function BagIllustration({ swatchIndex = 0, size = 'md', detail = 'handle' }) {
  const sw = BAG_SWATCHES[swatchIndex] || BAG_SWATCHES[0];
  return (
    <div style={{
      width: '100%', height: '100%',
      background: sw.bg,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      position: 'relative', overflow: 'hidden',
    }}>
      {/* texture/grain */}
      <div style={{
        position: 'absolute', inset: 0,
        backgroundImage: 'radial-gradient(circle at 30% 20%, rgba(255,255,255,0.15), transparent 50%), radial-gradient(circle at 70% 80%, rgba(0,0,0,0.18), transparent 60%)',
      }}/>
      {/* SVG bolsa */}
      <svg viewBox="0 0 200 200" width="62%" height="62%" style={{ filter: 'drop-shadow(0 12px 20px rgba(0,0,0,0.25))' }}>
        {/* alça */}
        <path d="M 65 60 C 65 30, 135 30, 135 60"
              stroke="rgba(0,0,0,0.45)" strokeWidth="5" fill="none" strokeLinecap="round"/>
        {/* corpo da bolsa */}
        <path d="M 40 65 L 160 65 L 155 170 Q 155 178 147 178 L 53 178 Q 45 178 45 170 Z"
              fill="rgba(0,0,0,0.18)" />
        <path d="M 42 63 L 158 63 L 153 168 Q 153 175 146 175 L 54 175 Q 47 175 47 168 Z"
              fill="url(#leather)" />
        <defs>
          <linearGradient id="leather" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0" stopColor="rgba(255,255,255,0.18)"/>
            <stop offset="0.4" stopColor="rgba(255,255,255,0.02)"/>
            <stop offset="1" stopColor="rgba(0,0,0,0.25)"/>
          </linearGradient>
        </defs>
        {/* costura */}
        <path d="M 50 72 L 150 72" stroke="rgba(255,255,255,0.25)" strokeWidth="0.8" strokeDasharray="2 2"/>
        <path d="M 50 168 L 150 168" stroke="rgba(255,255,255,0.25)" strokeWidth="0.8" strokeDasharray="2 2"/>
        {/* fivela */}
        <rect x="92" y="105" width="16" height="22" rx="2" fill="rgba(230,210,180,0.85)"/>
        <rect x="94" y="107" width="12" height="18" rx="1" fill="rgba(180,150,110,0.9)"/>
      </svg>
    </div>
  );
}

Object.assign(window, { Icon, I, BAG_SWATCHES, PRODUCTS, CATALOGS, CLIENTS, SUPPLIERS, ORDERS, STATUS_LABEL, FINANCE, BagIllustration });
