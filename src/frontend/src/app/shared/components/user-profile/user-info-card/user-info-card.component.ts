import { Component, OnInit } from '@angular/core';
import { ModalService } from '../../../services/modal.service';
import { CommonModule } from '@angular/common';
import { InputFieldComponent } from '../../form/input/input-field.component';
import { ButtonComponent } from '../../ui/button/button.component';
import { LabelComponent } from '../../form/label/label.component';
import { ModalComponent } from '../../ui/modal/modal.component';
import { AuthService } from '../../../../core/services/auth.service';
import { User, AuthState } from '../../../../core/models/user.model';

@Component({
  selector: 'app-user-info-card',
  imports: [
    CommonModule,
    InputFieldComponent,
    ButtonComponent,
    LabelComponent,
    ModalComponent,
  ],
  templateUrl: './user-info-card.component.html',
  styles: ``
})
export class UserInfoCardComponent implements OnInit {
  isOpen = false;
  user: User | null = null;
  userInfo = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    bio: '',
    social: {
      facebook: '',
      x: '',
      linkedin: '',
      instagram: '',
    },
  };

  constructor(
    public modal: ModalService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.authState$.subscribe((authState: AuthState) => {
      this.user = authState.user;
      if (this.user) {
        this.updateUserInfo();
      }
    });
  }

  private updateUserInfo() {
    if (this.user) {
      // Dividir o nome completo em primeiro e último nome
      const nameParts = this.user.nome?.split(' ') || ['Usuário'];
      this.userInfo.firstName = nameParts[0] || 'Usuário';
      this.userInfo.lastName = nameParts.slice(1).join(' ') || '';
      this.userInfo.email = this.user.email || '';
      this.userInfo.phone = ''; // Não temos telefone no modelo atual
      this.userInfo.bio = 'Usuário do sistema';
    }
  }

  openModal() { this.isOpen = true; }
  closeModal() { this.isOpen = false; }

  handleSave() {
    // Handle save logic here
    console.log('Saving changes...');
    this.modal.closeModal();
  }
}
