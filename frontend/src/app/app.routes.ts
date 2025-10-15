import { Routes } from '@angular/router';
import { EcommerceComponent } from './pages/dashboard/ecommerce/ecommerce.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { FormElementsComponent } from './pages/forms/form-elements/form-elements.component';
import { BasicTablesComponent } from './pages/tables/basic-tables/basic-tables.component';
import { BlankComponent } from './pages/blank/blank.component';
import { NotFoundComponent } from './pages/other-page/not-found/not-found.component';
import { AppLayoutComponent } from './shared/layout/app-layout/app-layout.component';
import { InvoicesComponent } from './pages/invoices/invoices.component';
import { LineChartComponent } from './pages/charts/line-chart/line-chart.component';
import { BarChartComponent } from './pages/charts/bar-chart/bar-chart.component';
import { AlertsComponent } from './pages/ui-elements/alerts/alerts.component';
import { AvatarElementComponent } from './pages/ui-elements/avatar-element/avatar-element.component';
import { BadgesComponent } from './pages/ui-elements/badges/badges.component';
import { ButtonsComponent } from './pages/ui-elements/buttons/buttons.component';
import { ImagesComponent } from './pages/ui-elements/images/images.component';
import { VideosComponent } from './pages/ui-elements/videos/videos.component';
import { SignInComponent } from './pages/auth-pages/sign-in/sign-in.component';
import { SignUpComponent } from './pages/auth-pages/sign-up/sign-up.component';
import { CalenderComponent } from './pages/calender/calender.component';
import { ProductsComponent } from './pages/products/products.component';
import { CatalogsComponent } from './pages/catalogs/catalogs.component';
import { HomeComponent } from './pages/home/home.component';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  // Home page - Public e-commerce catalog
  {
    path: '',
    component: HomeComponent,
    pathMatch: 'full',
    title: 'Catálogo Digital - Sany & Z | Produtos Exclusivos e Personalizados'
  },
  // auth pages - SEM autenticação necessária, mas redireciona se já autenticado
  {
    path:'signin',
    component:SignInComponent,
    canActivate: [GuestGuard],
    title:'Login | Catálogo Digital - Sany & Z'
  },
  {
    path:'signup',
    component:SignUpComponent,
    canActivate: [GuestGuard],
    title:'Cadastro | Catálogo Digital - Sany & Z'
  },
  // páginas protegidas - COM autenticação necessária
  {
    path:'dashboard',
    component:AppLayoutComponent,
    canActivate: [AuthGuard],
    children:[
      {
        path: '',
        component: EcommerceComponent,
        pathMatch: 'full',
        title: 'Dashboard | Catálogo Digital - Sany & Z',
      },
      {
        path:'calendar',
        component:CalenderComponent,
        title:'Calendário | Catálogo Digital - Sany & Z'
      },
      {
        path:'profile',
        component:ProfileComponent,
        title:'Perfil | Catálogo Digital - Sany & Z'
      },
      {
        path:'form-elements',
        component:FormElementsComponent,
        title:'Formulários | Catálogo Digital - Sany & Z'
      },
      {
        path:'basic-tables',
        component:BasicTablesComponent,
        title:'Tabelas | Catálogo Digital - Sany & Z'
      },
      {
        path:'blank',
        component:BlankComponent,
        title:'Vazio | Catálogo Digital - Sany & Z'
      },
      // support tickets
      {
        path:'invoice',
        component:InvoicesComponent,
        title:'Faturas | Catálogo Digital - Sany & Z'
      },
      {
        path:'line-chart',
        component:LineChartComponent,
        title:'Gráfico de Linha | Catálogo Digital - Sany & Z'
      },
      {
        path:'bar-chart',
        component:BarChartComponent,
        title:'Gráfico de Barras | Catálogo Digital - Sany & Z'
      },
      {
        path:'alerts',
        component:AlertsComponent,
        title:'Alertas | Catálogo Digital - Sany & Z'
      },
      {
        path:'avatars',
        component:AvatarElementComponent,
        title:'Avatares | Catálogo Digital - Sany & Z'
      },
      {
        path:'badge',
        component:BadgesComponent,
        title:'Badges | Catálogo Digital - Sany & Z'
      },
      {
        path:'buttons',
        component:ButtonsComponent,
        title:'Botões | Catálogo Digital - Sany & Z'
      },
      {
        path:'images',
        component:ImagesComponent,
        title:'Imagens | Catálogo Digital - Sany & Z'
      },
      {
        path:'videos',
        component:VideosComponent,
        title:'Vídeos | Catálogo Digital - Sany & Z'
      },
      {
        path:'products',
        component:ProductsComponent,
        title:'Produtos | Catálogo Digital - Sany & Z'
      },
      {
        path:'catalogs',
        component:CatalogsComponent,
        title:'Catálogos | Catálogo Digital - Sany & Z'
      },
    ]
  },
  // error pages
  {
    path:'**',
    component:NotFoundComponent,
    title:'Página não encontrada | Catálogo Digital - Sany & Z'
  },
];
