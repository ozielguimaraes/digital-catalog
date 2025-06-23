import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-produto-form',
  templateUrl: './produto-form.component.html',
  styleUrls: ['./produto-form.component.scss']
})
export class ProdutoFormComponent implements OnInit {
  produtoForm!: FormGroup;
  categorias: string[] = [
    'Eletrônicos',
    'Roupas',
    'Acessórios',
    'Calçados',
    'Decoração',
    'Livros',
    'Alimentos',
    'Bebidas',
    'Beleza',
    'Saúde',
    'Esportes',
    'Brinquedos',
    'Outros'
  ];  
  formatterReal = (value: number): string => `R$ ${value}`;
  parserReal = (value: string): string => value.replace('R$ ', '');
  constructor(
    private fb: FormBuilder,
    private message: NzMessageService
  ) {}

  ngOnInit(): void {
    this.produtoForm = this.fb.group({
      nome: [null, [Validators.required]],
      categoria: [null, [Validators.required]],
      variacoes: this.fb.array([this.criarVariacao()]),
      preco: [null, [Validators.required, Validators.min(0)]],
      precoComDesconto: [null, [Validators.min(0)]],
      estoque: this.fb.group({
        quantidade: [null, [Validators.min(0)]],
        ilimitado: [false]
      }),
      informacoesAdicionais: [null]
    });
  }

  get variacoes(): FormArray {
    return this.produtoForm.get('variacoes') as FormArray;
  }

  criarVariacao(): FormGroup {
    return this.fb.group({
      nome: [null, Validators.required],
      valor: [null, Validators.required]
    });
  }

  adicionarVariacao(): void {
    this.variacoes.push(this.criarVariacao());
  }

  removerVariacao(index: number): void {
    if (this.variacoes.length > 1) {
      this.variacoes.removeAt(index);
    } else {
      this.message.warning('Pelo menos uma variau00e7ão u00e9 necessária');
    }
  }

  onEstoqueIlimitadoChange(checked: boolean): void {
    const quantidadeControl = this.produtoForm.get('estoque.quantidade');
    if (checked) {
      quantidadeControl?.disable();
      quantidadeControl?.setValue(null);
    } else {
      quantidadeControl?.enable();
    }
  }

  submitForm(): void {
    if (this.produtoForm.valid) {
      console.log('Produto enviado:', this.produtoForm.value);
      this.message.success('Produto cadastrado com sucesso!');
      this.produtoForm.reset();
      // Reiniciar o formulário com uma variação vazia
      this.variacoes.clear();
      this.variacoes.push(this.criarVariacao());
      // Resetar o checkbox de estoque ilimitado
      this.produtoForm.get('estoque.ilimitado')?.setValue(false);
      this.produtoForm.get('estoque.quantidade')?.enable();
    } else {
      Object.values(this.produtoForm.controls).forEach(control => {
        if (control.invalid) {
          control.markAsDirty();
          control.updateValueAndValidity({ onlySelf: true });
        }
      });
      this.message.error('Por favor, corrija os erros no formulário');
    }
  }
}