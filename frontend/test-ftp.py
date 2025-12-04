#!/usr/bin/env python3
"""
Script para testar conexão FTP
"""

import ftplib
import sys

# Configurações FTP
FTP_CONFIG = {
    'host': '198.50.203.165',
    'user': '9295_sanyz',
    'pass': 'ugzxiohyenmjqpfrk4cw',
    'remote_dir': 'public_html'
}

def test_ftp_connection():
    """Testa conexão FTP"""
    print("🔍 Testando conexão FTP...")
    print(f"🌐 Host: {FTP_CONFIG['host']}")
    print(f"👤 Usuário: {FTP_CONFIG['user']}")
    print(f"📁 Diretório: {FTP_CONFIG['remote_dir']}")
    print()
    
    try:
        # Conectar ao FTP
        print("🔄 Conectando...")
        ftp = ftplib.FTP()
        ftp.connect(FTP_CONFIG['host'])
        print("✅ Conectado ao servidor!")
        
        # Login
        print("🔄 Fazendo login...")
        ftp.login(FTP_CONFIG['user'], FTP_CONFIG['pass'])
        print("✅ Login realizado com sucesso!")
        
        # Navegar para diretório
        print(f"🔄 Navegando para {FTP_CONFIG['remote_dir']}...")
        ftp.cwd(FTP_CONFIG['remote_dir'])
        print("✅ Diretório acessado com sucesso!")
        
        # Listar arquivos
        print("📋 Listando arquivos no diretório:")
        files = ftp.nlst()
        for file in files[:10]:  # Mostrar apenas os primeiros 10
            print(f"  📄 {file}")
        if len(files) > 10:
            print(f"  ... e mais {len(files) - 10} arquivos")
        
        # Fechar conexão
        ftp.quit()
        print()
        print("✅ Teste de conexão FTP bem-sucedido!")
        print("🚀 Pronto para fazer deploy!")
        return True
        
    except Exception as e:
        print(f"❌ Erro na conexão FTP: {str(e)}")
        print()
        print("🔧 Possíveis soluções:")
        print("1. Verifique se as credenciais estão corretas")
        print("2. Verifique se o servidor FTP está ativo")
        print("3. Verifique se não há firewall bloqueando")
        print("4. Tente usar um cliente FTP como FileZilla para testar")
        return False

if __name__ == "__main__":
    if test_ftp_connection():
        sys.exit(0)
    else:
        sys.exit(1)
