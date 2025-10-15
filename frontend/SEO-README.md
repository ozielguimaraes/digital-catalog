# SEO Configuration - Sany&Z Catálogo Digital

## 📋 Configurações Implementadas

### 1. Meta Tags Básicas
- ✅ Title otimizado com palavras-chave
- ✅ Meta description com foco na marca e produtos
- ✅ Meta keywords relevantes
- ✅ Meta robots para indexação
- ✅ Language e locale configurados

### 2. Open Graph (Facebook/LinkedIn)
- ✅ og:title, og:description, og:image
- ✅ og:type, og:url, og:site_name
- ✅ og:locale para português brasileiro

### 3. Twitter Cards
- ✅ twitter:card, twitter:title, twitter:description
- ✅ twitter:image, twitter:url

### 4. Structured Data (Schema.org)
- ✅ Organization schema
- ✅ WebSite schema com SearchAction
- ✅ Brand information
- ✅ Contact information

### 5. Arquivos de SEO
- ✅ robots.txt
- ✅ sitemap.xml
- ✅ manifest.json (PWA)
- ✅ Google Analytics 4
- ✅ Google Tag Manager

### 6. Cores da Marca (Theme Colors)
- ✅ Primary: #c06647 (Copper Sany)
- ✅ Secondary: #ede8e2 (Alabaster)
- ✅ Accent: #df8252 (Copper Z)

## 🔧 Próximos Passos

### 1. Configurar IDs Reais
Substitua os placeholders pelos IDs reais:
- `GA_MEASUREMENT_ID` → Seu ID do Google Analytics
- `GTM-XXXXXXX` → Seu ID do Google Tag Manager
- Códigos de verificação do Google Search Console e Bing

### 2. Adicionar Imagens
Crie e adicione as seguintes imagens em `/public/assets/images/`:
- `og-image.jpg` (1200x630px) - Para redes sociais
- `twitter-image.jpg` (1200x600px) - Para Twitter
- `logo.png` - Logo da marca
- `icon-*.png` - Ícones para PWA (72x72 até 512x512)
- `screenshot-*.png` - Screenshots para PWA

### 3. Configurar URLs Reais
Atualize as URLs nos arquivos:
- `index.html` - URLs do Open Graph e Twitter
- `sitemap.xml` - URLs das páginas
- `robots.txt` - URL do sitemap

### 4. Implementar SEO Dinâmico
Use o `SeoService` para atualizar meta tags dinamicamente:
```typescript
// Exemplo de uso
this.seoService.updateMetaTags({
  title: 'Nome do Produto',
  description: 'Descrição do produto...',
  image: 'https://sanyz.com.br/assets/images/produto.jpg',
  keywords: 'produto, sanyz, exclusivo'
});
```

### 5. Configurar Analytics
- Configure eventos personalizados no Google Analytics
- Implemente tracking de produtos e conversões
- Configure goals e funnels

### 6. Otimizações Adicionais
- Implementar lazy loading de imagens
- Otimizar Core Web Vitals
- Configurar Service Worker para cache
- Implementar breadcrumbs
- Adicionar FAQ schema para páginas de produtos

## 📊 Monitoramento

### Ferramentas Recomendadas
1. **Google Search Console** - Monitorar indexação e performance
2. **Google Analytics 4** - Análise de tráfego e comportamento
3. **Google PageSpeed Insights** - Performance e Core Web Vitals
4. **Bing Webmaster Tools** - Monitoramento adicional
5. **Screaming Frog** - Auditoria técnica de SEO

### Métricas Importantes
- Posição nas SERPs para palavras-chave relevantes
- CTR (Click Through Rate) nos resultados de busca
- Core Web Vitals (LCP, FID, CLS)
- Taxa de rejeição e tempo na página
- Conversões e goals

## 🎯 Palavras-chave Principais

### Primárias
- Sany&Z
- catálogo digital
- produtos exclusivos
- produtos personalizados

### Secundárias
- alta qualidade
- design autêntico
- materiais selecionados
- atendimento personalizado
- sofisticação
- originalidade

### Long-tail
- catálogo digital de produtos exclusivos
- produtos personalizados de alta qualidade
- marca de produtos exclusivos Sany&Z
- catálogo online produtos personalizados

## 📱 PWA Features

O manifest.json está configurado para:
- Instalação como app nativo
- Tema personalizado com cores da marca
- Ícones em múltiplos tamanhos
- Screenshots para app stores
- Orientação portrait para mobile

## 🔍 Schema Markup

Implementado:
- Organization (marca, contato, redes sociais)
- WebSite (funcionalidade de busca)
- Brand (informações da marca)

Recomendado adicionar:
- Product (para cada produto)
- FAQ (para páginas de ajuda)
- BreadcrumbList (navegação)
- LocalBusiness (se tiver loja física)
