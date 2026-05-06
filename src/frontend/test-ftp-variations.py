#!/usr/bin/env python3
"""
Script para testar diferentes variações de credenciais FTP
"""

import ftplib
import sys

def test_ftp_variations():
    """Testa diferentes variações de credenciais"""
    
    # Variações de credenciais para testar
    variations = [
        {
            'host': '198.50.203.165',
            'user': '9295_sanyz',
            'pass': 'ugzxiohyenmjqpfrk4cw',
            'desc': 'Credenciais atuais (curtas)'
        },
        {
            'host': '198.50.203.165',
            'user': '9295_sanyz',
            'pass': 'ugzxiohyenmjqpfrkeeee4cw',
            'desc': 'Credenciais originais (longas)'
        },
        {
            'host': '198.50.203.165',
            'user': '9295_sanyz',
            'pass': 'ugzxiohyenmjqpfrk4cw',
            'port': 21,
            'desc': 'Com porta explícita 21'
        },
        {
            'host': '198.50.203.165',
            'user': '9295_sanyz',
            'pass': 'ugzxiohyenmjqpfrk4cw',
            'port': 22,
            'desc': 'Com porta 22 (SFTP)'
        }
    ]
    
    print("🔍 TESTANDO VARIAÇÕES DE CREDENCIAIS FTP")
    print("=" * 50)
    
    for i, config in enumerate(variations, 1):
        print(f"\n{i}️⃣ Testando: {config['desc']}")
        print(f"   Host: {config['host']}")
        print(f"   Usuário: {config['user']}")
        print(f"   Senha: {'*' * len(config['pass'])}")
        if 'port' in config:
            print(f"   Porta: {config['port']}")
        
        try:
            ftp = ftplib.FTP()
            ftp.set_debuglevel(0)  # Desativar debug para clareza
            
            # Conectar
            port = config.get('port', 21)
            ftp.connect(config['host'], port, timeout=10)
            
            # Login
            ftp.login(config['user'], config['pass'])
            
            print("   ✅ SUCESSO! Login realizado")
            
            # Testar navegação
            try:
                ftp.cwd('public_html')
                print("   ✅ Diretório public_html acessado")
            except:
                print("   ⚠️  Diretório public_html não encontrado")
            
            ftp.quit()
            print(f"   🎉 Configuração {i} funciona perfeitamente!")
            return config
            
        except ftplib.error_perm as e:
            print(f"   ❌ Erro de permissão: {e}")
        except Exception as e:
            print(f"   ❌ Erro: {e}")
    
    print("\n❌ Nenhuma configuração funcionou")
    return None

def test_alternative_methods():
    """Testa métodos alternativos de conexão"""
    print("\n🔧 TESTANDO MÉTODOS ALTERNATIVOS")
    print("=" * 40)
    
    # Teste com modo passivo
    print("\n1️⃣ Testando modo passivo...")
    try:
        ftp = ftplib.FTP()
        ftp.set_pasv(True)  # Modo passivo
        ftp.connect('198.50.203.165', 21, timeout=10)
        ftp.login('9295_sanyz', 'ugzxiohyenmjqpfrk4cw')
        print("   ✅ Modo passivo funcionou!")
        ftp.quit()
        return True
    except Exception as e:
        print(f"   ❌ Modo passivo falhou: {e}")
    
    # Teste com modo ativo
    print("\n2️⃣ Testando modo ativo...")
    try:
        ftp = ftplib.FTP()
        ftp.set_pasv(False)  # Modo ativo
        ftp.connect('198.50.203.165', 21, timeout=10)
        ftp.login('9295_sanyz', 'ugzxiohyenmjqpfrk4cw')
        print("   ✅ Modo ativo funcionou!")
        ftp.quit()
        return True
    except Exception as e:
        print(f"   ❌ Modo ativo falhou: {e}")
    
    return False

def suggest_manual_test():
    """Sugere teste manual"""
    print("\n📋 TESTE MANUAL RECOMENDADO")
    print("=" * 35)
    print("1. Acesse o painel de controle da hospedagem")
    print("2. Vá para 'Gerenciador de Arquivos' ou 'File Manager'")
    print("3. Verifique se consegue acessar a pasta public_html")
    print("4. Teste com um cliente FTP como FileZilla:")
    print("   - Host: 198.50.203.165")
    print("   - Usuário: 9295_sanyz")
    print("   - Senha: [sua senha real]")
    print("   - Porta: 21")
    print("5. Verifique se há restrições de IP no painel")

if __name__ == "__main__":
    print("🚀 Iniciando teste de variações FTP...\n")
    
    # Testar variações
    working_config = test_ftp_variations()
    
    if working_config:
        print(f"\n🎉 CONFIGURAÇÃO FUNCIONANDO ENCONTRADA!")
        print("=" * 45)
        print(f"Host: {working_config['host']}")
        print(f"Usuário: {working_config['user']}")
        print(f"Senha: {working_config['pass']}")
        if 'port' in working_config:
            print(f"Porta: {working_config['port']}")
    else:
        # Testar métodos alternativos
        if test_alternative_methods():
            print("\n✅ Pelo menos um método alternativo funcionou!")
        else:
            print("\n❌ Nenhum método funcionou")
            suggest_manual_test()
