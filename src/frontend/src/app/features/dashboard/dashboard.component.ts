import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { User } from '../../core/models/user.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  user: User | null = null;
  stats = [
    {
      title: 'Total de Produtos',
      value: '1,234',
      icon: 'inventory',
      color: 'primary',
      change: '+12%',
      changeType: 'positive'
    },
    {
      title: 'Pedidos Hoje',
      value: '56',
      icon: 'shopping_cart',
      color: 'accent',
      change: '+8%',
      changeType: 'positive'
    },
    {
      title: 'Clientes Ativos',
      value: '892',
      icon: 'people',
      color: 'warn',
      change: '+5%',
      changeType: 'positive'
    },
    {
      title: 'Receita Mensal',
      value: 'R$ 45.678',
      icon: 'attach_money',
      color: 'primary',
      change: '+15%',
      changeType: 'positive'
    }
  ];

  recentActivities = [
    {
      action: 'Novo produto adicionado',
      description: 'iPhone 15 Pro Max foi adicionado ao catálogo',
      time: '2 minutos atrás',
      icon: 'add_circle',
      color: 'primary'
    },
    {
      action: 'Pedido processado',
      description: 'Pedido #12345 foi processado com sucesso',
      time: '15 minutos atrás',
      icon: 'check_circle',
      color: 'accent'
    },
    {
      action: 'Cliente registrado',
      description: 'João Silva se cadastrou na plataforma',
      time: '1 hora atrás',
      icon: 'person_add',
      color: 'warn'
    },
    {
      action: 'Relatório gerado',
      description: 'Relatório de vendas do mês foi gerado',
      time: '2 horas atrás',
      icon: 'assessment',
      color: 'primary'
    }
  ];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(authState => {
      this.user = authState.user;
    });
  }
}
