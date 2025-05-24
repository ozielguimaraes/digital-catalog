import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProdutoFormComponent } from './produto-form/produto-form.component';

const routes: Routes = [
  { path: '', redirectTo: '/produtos/novo', pathMatch: 'full' },
  { path: 'produtos/novo', component: ProdutoFormComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }