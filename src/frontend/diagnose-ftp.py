#!/usr/bin/env python3
"""
Script de diagnóstico FTP para identificar problemas de conexão
"""

import ftplib
import sys
import socket

def test_connection():
    """Testa conexão FTP com diagnóstico detalhado"""
    
    # Configurações FTP
    FTP_CONFIG = {
        'host': '198.50.203.165',
        'user': '9295_sanyz',
        'pass': 'ugzxiohyenmjqpfrk4cw',
        'port': 21
    }
    
    print("🔍 DIAGNÓSTICO FTP DETALHADO")
    print("=" * 50)
    print(f"🌐 Host: {FTP_CONFIG['host']}")
    print(f"🔌 Porta: {FTP_CONFIG['port']}")
    print(f"👤 Usuário: {FTP_CONFIG['user']}")
    print(f"🔑 Senha: {'*' * len(FTP_CONFIG['pass'])}")
    print()
    
    # Teste 1: Conectividade de rede
    print("1️⃣ Testando conectividade de rede...")
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(10)
        result = sock.connect_ex((FTP_CONFIG['host'], FTP_CONFIG['port']))
        sock.close()
        
        if result == 0:
            print("✅ Servidor FTP está acessível")
        else:
            print("❌ Servidor FTP não está acessível")
            print("🔧 Verifique se o host e porta estão corretos")
            return False
    except Exception as e:
        print(f"❌ Erro de rede: {e}")
        return False
    
    # Teste 2: Conexão FTP
    print("\n2️⃣ Testando conexão FTP...")
    try:
        ftp = ftplib.FTP()
        ftp.set_debuglevel(2)  # Ativar debug
        ftp.connect(FTP_CONFIG['host'], FTP_CONFIG['port'], timeout=30)
        print("✅ Conexão FTP estabelecida")
    except Exception as e:
        print(f"❌ Erro na conexão FTP: {e}")
        return False
    
    # Teste 3: Login
    print("\n3️⃣ Testando login...")
    try:
        ftp.login(FTP_CONFIG['user'], FTP_CONFIG['pass'])
        print("✅ Login realizado com sucesso!")
    except ftplib.error_perm as e:
        print(f"❌ Erro de permissão: {e}")
        print("\n🔧 Possíveis causas:")
        print("   - Usuário ou senha incorretos")
        print("   - Conta desabilitada")
        print("   - Restrições de IP")
        print("   - Formato de credenciais incorreto")
        return False
    except Exception as e:
        print(f"❌ Erro no login: {e}")
        return False
    
    # Teste 4: Navegação
    print("\n4️⃣ Testando navegação...")
    try:
        # Listar diretório atual
        print("📋 Diretório atual:")
        ftp.retrlines('PWD')
        
        # Listar arquivos
        print("📄 Arquivos no diretório:")
        files = ftp.nlst()
        for file in files[:5]:  # Mostrar apenas os primeiros 5
            print(f"   📄 {file}")
        if len(files) > 5:
            print(f"   ... e mais {len(files) - 5} arquivos")
        
        # Testar mudança de diretório
        print("\n📁 Testando mudança para public_html...")
        try:
            ftp.cwd('public_html')
            print("✅ Diretório public_html acessado")
        except:
            print("⚠️  Diretório public_html não encontrado, testando outros...")
            # Listar diretórios disponíveis
            ftp.retrlines('LIST')
        
    except Exception as e:
        print(f"❌ Erro na navegação: {e}")
        return False
    
    # Fechar conexão
    try:
        ftp.quit()
        print("\n✅ Conexão fechada com sucesso")
    except:
        pass
    
    return True

def suggest_solutions():
    """Sugere soluções para problemas comuns"""
    print("\n🔧 SOLUÇÕES SUGERIDAS:")
    print("=" * 30)
    print("1. Verifique as credenciais no painel de controle da hospedagem")
    print("2. Confirme se a conta FTP está ativa")
    print("3. Verifique se há restrições de IP")
    print("4. Teste com um cliente FTP como FileZilla")
    print("5. Verifique se o servidor suporta FTP passivo/ativo")
    print("6. Confirme o diretório correto (public_html, www, htdocs, etc.)")

if __name__ == "__main__":
    print("🚀 Iniciando diagnóstico FTP...\n")
    
    if test_connection():
        print("\n🎉 Diagnóstico concluído com sucesso!")
        print("✅ FTP está funcionando corretamente")
    else:
        print("\n❌ Problemas encontrados no diagnóstico")
        suggest_solutions()
        sys.exit(1)
