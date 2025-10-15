import { Component, OnInit } from '@angular/core';
import { InputFieldComponent } from './../../form/input/input-field.component';
import { ModalService } from '../../../services/modal.service';
import { CommonModule } from '@angular/common';
import { ModalComponent } from '../../ui/modal/modal.component';
import { ButtonComponent } from '../../ui/button/button.component';
import { AuthService } from '../../../../core/services/auth.service';
import { User, AuthState } from '../../../../core/models/user.model';

@Component({
  selector: 'app-user-meta-card',
  imports: [
    CommonModule,
    ModalComponent,
    InputFieldComponent,
    ButtonComponent,
  ],
  templateUrl: './user-meta-card.component.html',
  styles: ``
})
export class UserMetaCardComponent implements OnInit {
  isOpen = false;
  user: User | null = null;
  userProfile = {
    firstName: '',
    lastName: '',
    role: 'Usuário',
    location: 'Brasil',
    avatar: '/images/user/owner.jpg',
    social: {
      facebook: '',
      x: '',
      linkedin: '',
      instagram: '',
    },
    email: '',
    phone: '',
    bio: '',
  };

  constructor(
    public modal: ModalService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.authState$.subscribe((authState: AuthState) => {
      this.user = authState.user;
      if (this.user) {
        this.updateUserProfile();
      }
    });
  }

  private updateUserProfile() {
    if (this.user) {
      // Dividir o nome completo em primeiro e último nome
      const nameParts = this.user.nome?.split(' ') || ['Usuário'];
      this.userProfile.firstName = nameParts[0] || 'Usuário';
      this.userProfile.lastName = nameParts.slice(1).join(' ') || '';
      this.userProfile.email = this.user.email || '';
      this.userProfile.role = 'Usuário';
      this.userProfile.location = 'Brasil';
      this.userProfile.bio = 'Usuário do sistema';
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
