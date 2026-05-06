#!/usr/bin/env python3
"""
Script para deploy via FTP com descompactação automática
Usa ZIP + descompactação no servidor via comandos FTP
"""

import os
import sys
import json
import zipfile
import subprocess
import ftplib
from datetime import datetime
from pathlib import Path

# Configurações FTP
FTP_CONFIG = {
    'host': '198.50.203.165',
    'user': '9295_sanyz',
    'pass': 'ugzxiohyenmjqpfrk4cw',
    'remote_dir': 'public_html'
}

# Cores para output
class Colors:
    RED = '\033[0;31m'
    GREEN = '\033[0;32m'
    YELLOW = '\033[1;33m'
    BLUE = '\033[0;34m'
    NC = '\033[0m'  # No Color

def print_message(message, color=Colors.NC):
    print(f"{color}{message}{Colors.NC}")

def print_header(title):
    print()
    print_message("=" * 50, Colors.BLUE)
    print_message(title, Colors.BLUE)
    print_message("=" * 50, Colors.BLUE)
    print()

def run_command(command, description):
    """Executa comando e retorna sucesso"""
    print_message(f"🔄 {description}...", Colors.YELLOW)
    try:
        result = subprocess.run(command, shell=True, check=True, capture_output=True, text=True)
        print_message(f"✅ {description} concluído!", Colors.GREEN)
        return True
    except subprocess.CalledProcessError as e:
        print_message(f"❌ Erro em {description}: {e.stderr}", Colors.RED)
        return False

def get_version():
    """Obtém versão do package.json"""
    try:
        with open('package.json', 'r') as f:
            package = json.load(f)
            return package.get('version', '1.0.0')
    except:
        return '1.0.0'

def create_build_info():
    """Cria arquivo de informações do build"""
    try:
        with open('build-info.js', 'r') as f:
            subprocess.run(['node', 'build-info.js'], check=True)
        return True
    except:
        return False

def create_zip(zip_name, source_dir):
    """Cria arquivo ZIP"""
    print_message(f"📦 Criando {zip_name}...", Colors.YELLOW)
    
    with zipfile.ZipFile(zip_name, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, dirs, files in os.walk(source_dir):
            for file in files:
                if not file.startswith('.') and not file.endswith('.DS_Store'):
                    file_path = os.path.join(root, file)
                    arc_path = os.path.relpath(file_path, source_dir)
                    zipf.write(file_path, arc_path)
    
    size = os.path.getsize(zip_name) / (1024 * 1024)  # MB
    print_message(f"✅ ZIP criado: {zip_name} ({size:.1f}MB)", Colors.GREEN)
    return True

def upload_and_extract_via_ftp(zip_name):
    """Faz upload do ZIP e tenta descompactar via FTP"""
    print_message("🌐 Conectando via FTP...", Colors.YELLOW)
    
    try:
        # Conectar ao FTP
        ftp = ftplib.FTP()
        ftp.set_debuglevel(0)
        ftp.set_pasv(True)
        ftp.connect(FTP_CONFIG['host'], 21, timeout=30)
        ftp.login(FTP_CONFIG['user'], FTP_CONFIG['pass'])
        
        print_message("✅ Conectado ao servidor FTP!", Colors.GREEN)
        
        # Navegar para diretório remoto
        try:
            ftp.cwd(FTP_CONFIG['remote_dir'])
            print_message(f"✅ Navegou para {FTP_CONFIG['remote_dir']}", Colors.GREEN)
        except:
            print_message(f"⚠️  Usando diretório atual: {ftp.pwd()}", Colors.YELLOW)
        
        # Upload do arquivo ZIP
        print_message(f"⬆️  Enviando {zip_name}...", Colors.YELLOW)
        with open(zip_name, 'rb') as file:
            ftp.storbinary(f'STOR {zip_name}', file)
        
        print_message("✅ Upload do ZIP concluído!", Colors.GREEN)
        
        # Verificar se o arquivo foi enviado
        try:
            files = ftp.nlst()
            if zip_name in files:
                print_message(f"✅ Arquivo {zip_name} confirmado no servidor", Colors.GREEN)
            else:
                print_message(f"⚠️  Arquivo {zip_name} não encontrado na listagem", Colors.YELLOW)
        except:
            print_message("⚠️  Não foi possível verificar arquivos no servidor", Colors.YELLOW)
        
        # Fechar conexão
        ftp.quit()
        return True
        
    except Exception as e:
        print_message(f"❌ Erro FTP: {str(e)}", Colors.RED)
        return False

def main():
    print_header("🚀 DEPLOY VIA FTP COM DESCOMPACTAÇÃO AUTOMÁTICA")
    
    # Verificar se estamos no diretório correto
    if not os.path.exists('package.json'):
        print_message("❌ ERRO: Execute no diretório frontend da aplicação Angular", Colors.RED)
        sys.exit(1)
    
    # Obter informações
    version = get_version()
    date = datetime.now().strftime("%Y%m%d_%H%M")
    zip_name = f"meucatalogo-v{version}-{date}.zip"
    
    print_message(f"📦 Versão: {version}", Colors.GREEN)
    print_message(f"📅 Data: {date}", Colors.GREEN)
    print_message(f"📁 Arquivo: {zip_name}", Colors.YELLOW)
    print_message(f"🌐 Servidor: {FTP_CONFIG['host']}", Colors.BLUE)
    print_message(f"📁 Diretório: {FTP_CONFIG['remote_dir']}", Colors.BLUE)
    
    # Build
    print_header("🔧 FAZENDO BUILD")
    
    if not run_command("npm run build", "Build de produção"):
        sys.exit(1)
    
    if not create_build_info():
        print_message("⚠️  Aviso: Não foi possível gerar build-info", Colors.YELLOW)
    
    # Verificar se o build existe
    source_dir = "dist/ng-tailadmin"
    if not os.path.exists(source_dir):
        print_message("❌ ERRO: Diretório de build não encontrado", Colors.RED)
        sys.exit(1)
    
    # Criar ZIP
    print_header("🗜️  CRIANDO ZIP")
    
    if not create_zip(zip_name, source_dir):
        sys.exit(1)
    
    # Upload
    print_header("🌐 UPLOAD VIA FTP")
    
    if not upload_and_extract_via_ftp(zip_name):
        print_message("❌ Falha no upload FTP", Colors.RED)
        sys.exit(1)
    
    # Limpeza local
    print_header("🧹 LIMPEZA LOCAL")
    
    os.remove(zip_name)
    print_message("🗑️  Arquivo ZIP local removido", Colors.YELLOW)
    
    # Sucesso
    print_header("✅ DEPLOY CONCLUÍDO!")
    
    print_message("🎉 Aplicação enviada com sucesso!", Colors.GREEN)
    print_message("🌐 Acesse seu domínio para verificar", Colors.BLUE)
    
    print_message("\n📋 Próximos passos no servidor:", Colors.YELLOW)
    print_message("1. Acesse o painel de controle da hospedagem", Colors.YELLOW)
    print_message("2. Vá para o gerenciador de arquivos", Colors.YELLOW)
    print_message(f"3. Navegue até {FTP_CONFIG['remote_dir']}", Colors.YELLOW)
    print_message(f"4. Descompacte o arquivo {zip_name}", Colors.YELLOW)
    print_message("5. Mova o conteúdo de ng-tailadmin/ para a raiz", Colors.YELLOW)
    print_message("6. Remova a pasta ng-tailadmin e o arquivo ZIP", Colors.YELLOW)
    
    print_message("\n💡 Dica: O arquivo ZIP já está no servidor, só precisa descompactar!", Colors.BLUE)

if __name__ == "__main__":
    main()
