import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DateUtils } from '../../../core/utils/date.utils';
import { DateFormatPipe, TimeAgoPipe, UtcToLocalPipe } from '../../../core/pipes/date-format.pipe';

@Component({
  selector: 'app-date-examples',
  standalone: true,
  imports: [CommonModule, FormsModule, DateFormatPipe, TimeAgoPipe, UtcToLocalPipe],
  template: `
    <div class="p-6 bg-white rounded-lg shadow-sm dark:bg-gray-800">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
        Exemplos de Tratamento de Datas UTC/Local
      </h3>
      
      <div class="space-y-6">
        <!-- Exemplo 1: Data atual -->
        <div class="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h4 class="font-medium text-gray-900 dark:text-white mb-2">1. Data Atual (UTC)</h4>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">
            UTC: {{ currentUtcDate }}
          </p>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">
            Local: {{ currentLocalDate }}
          </p>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Timezone: {{ timezoneName }} ({{ timezoneOffset }} min)
          </p>
        </div>

        <!-- Exemplo 2: Conversão de datas -->
        <div class="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h4 class="font-medium text-gray-900 dark:text-white mb-2">2. Conversão de Datas</h4>
          <div class="space-y-2">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Data Local (YYYY-MM-DD):
              </label>
              <input 
                type="date" 
                [(ngModel)]="localDateInput"
                (ngModelChange)="onLocalDateChange()"
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Data/Hora Local (YYYY-MM-DDTHH:mm):
              </label>
              <input 
                type="datetime-local" 
                [(ngModel)]="localDateTimeInput"
                (ngModelChange)="onLocalDateTimeChange()"
                class="w-full px-3 py-2 border border-gray-300 rounded-md dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              />
            </div>
            <div class="text-sm text-gray-600 dark:text-gray-400">
              <p><strong>UTC Resultado:</strong> {{ utcResult }}</p>
            </div>
          </div>
        </div>

        <!-- Exemplo 3: Formatação de datas -->
        <div class="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h4 class="font-medium text-gray-900 dark:text-white mb-2">3. Formatação de Datas</h4>
          <div class="space-y-2 text-sm">
            <p><strong>Formato Local:</strong> {{ sampleUtcDate | dateFormat:'local' }}</p>
            <p><strong>Formato Relativo:</strong> {{ sampleUtcDate | timeAgo }}</p>
            <p><strong>Formato Display:</strong> {{ sampleUtcDate | dateFormat:'display' }}</p>
            <p><strong>Formato Date Picker:</strong> {{ sampleUtcDate | dateFormat:'datePicker' }}</p>
            <p><strong>Formato DateTime Picker:</strong> {{ sampleUtcDate | dateFormat:'dateTimePicker' }}</p>
          </div>
        </div>

        <!-- Exemplo 4: Diferentes fusos horários -->
        <div class="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h4 class="font-medium text-gray-900 dark:text-white mb-2">4. Diferentes Fusos Horários</h4>
          <div class="space-y-2 text-sm">
            <p><strong>UTC:</strong> {{ sampleUtcDate | dateFormat:'local' }}</p>
            <p><strong>New York:</strong> {{ sampleUtcDate | dateFormat:'local' | date:'short':'America/New_York' }}</p>
            <p><strong>London:</strong> {{ sampleUtcDate | dateFormat:'local' | date:'short':'Europe/London' }}</p>
            <p><strong>Tokyo:</strong> {{ sampleUtcDate | dateFormat:'local' | date:'short':'Asia/Tokyo' }}</p>
          </div>
        </div>

        <!-- Exemplo 5: Validação de formato UTC -->
        <div class="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h4 class="font-medium text-gray-900 dark:text-white mb-2">5. Validação de Formato UTC</h4>
          <div class="space-y-2 text-sm">
            <p><strong>É formato UTC:</strong> {{ DateUtils.isUtcFormat(sampleUtcDate) ? 'Sim' : 'Não' }}</p>
            <p><strong>É formato UTC (local):</strong> {{ DateUtils.isUtcFormat(localDateInput) ? 'Sim' : 'Não' }}</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DateExamplesComponent {
  currentUtcDate: string;
  currentLocalDate: string;
  timezoneName: string;
  timezoneOffset: number;
  
  localDateInput: string = '';
  localDateTimeInput: string = '';
  utcResult: string = '';
  
  sampleUtcDate: string = '2024-01-15T14:30:00.000Z';

  constructor() {
    // Data atual em UTC
    this.currentUtcDate = new Date().toISOString();
    
    // Data atual local
    this.currentLocalDate = new Date().toLocaleString('pt-BR');
    
    // Informações do timezone
    this.timezoneName = DateUtils.getTimezoneName();
    this.timezoneOffset = DateUtils.getTimezoneOffset();
    
    // Inicializar com data de exemplo
    this.localDateInput = DateUtils.toDatePickerValue(this.sampleUtcDate);
    this.localDateTimeInput = DateUtils.toDateTimePickerValue(this.sampleUtcDate);
    this.utcResult = this.sampleUtcDate;
  }

  onLocalDateChange(): void {
    if (this.localDateInput) {
      this.utcResult = DateUtils.fromDatePickerValue(this.localDateInput);
    }
  }

  onLocalDateTimeChange(): void {
    if (this.localDateTimeInput) {
      this.utcResult = DateUtils.fromDateTimePickerValue(this.localDateTimeInput);
    }
  }
}
