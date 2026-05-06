#!/usr/bin/env python3
"""
Script para deploy direto via FTP - upload dos arquivos sem ZIP
Faz upload direto na pasta correta do servidor
"""

import os
import sys
import json
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

def upload_file(ftp, local_path, remote_path):
    """Upload de um arquivo individual"""
    try:
        with open(local_path, 'rb') as file:
            ftp.storbinary(f'STOR {remote_path}', file)
        return True
    except Exception as e:
        print_message(f"❌ Erro ao enviar {local_path}: {e}", Colors.RED)
        return False

def upload_directory(ftp, local_dir, remote_dir=""):
    """Upload recursivo de diretório"""
    uploaded_files = 0
    failed_files = 0
    
    for root, dirs, files in os.walk(local_dir):
        # Calcular caminho relativo
        rel_path = os.path.relpath(root, local_dir)
        if rel_path == ".":
            current_remote_dir = remote_dir
        else:
            current_remote_dir = f"{remote_dir}/{rel_path}".replace("\\", "/")
        
        # Criar diretório remoto se necessário
        if current_remote_dir and current_remote_dir != ".":
            try:
                ftp.mkd(current_remote_dir)
            except:
                pass  # Diretório já existe ou erro ignorável
        
        # Upload dos arquivos
        for file in files:
            if file.startswith('.') or file.endswith('.DS_Store'):
                continue
                
            local_file_path = os.path.join(root, file)
            remote_file_path = f"{current_remote_dir}/{file}".replace("\\", "/")
            
            print_message(f"📤 Enviando: {file}", Colors.YELLOW)
            
            if upload_file(ftp, local_file_path, remote_file_path):
                uploaded_files += 1
                print_message(f"✅ {file} enviado", Colors.GREEN)
            else:
                failed_files += 1
                print_message(f"❌ Falha ao enviar {file}", Colors.RED)
    
    return uploaded_files, failed_files

def upload_via_ftp_direct(source_dir):
    """Faz upload direto dos arquivos via FTP"""
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
        
        # Upload dos arquivos
        print_message("📤 Iniciando upload dos arquivos...", Colors.YELLOW)
        uploaded, failed = upload_directory(ftp, source_dir, "")
        
        print_message(f"📊 Upload concluído: {uploaded} arquivos enviados, {failed} falharam", Colors.BLUE)
        
        if failed == 0:
            print_message("🎉 Todos os arquivos foram enviados com sucesso!", Colors.GREEN)
        else:
            print_message(f"⚠️  {failed} arquivos falharam no upload", Colors.YELLOW)
        
        # Fechar conexão
        ftp.quit()
        return failed == 0
        
    except Exception as e:
        print_message(f"❌ Erro FTP: {str(e)}", Colors.RED)
        return False

def main():
    print_header("🚀 DEPLOY DIRETO VIA FTP (SEM ZIP)")
    
    # Verificar se estamos no diretório correto
    if not os.path.exists('package.json'):
        print_message("❌ ERRO: Execute no diretório frontend da aplicação Angular", Colors.RED)
        sys.exit(1)
    
    # Obter informações
    version = get_version()
    date = datetime.now().strftime("%Y%m%d_%H%M")
    
    print_message(f"📦 Versão: {version}", Colors.GREEN)
    print_message(f"📅 Data: {date}", Colors.GREEN)
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
    
    # Upload direto
    print_header("🌐 UPLOAD DIRETO VIA FTP")
    
    if not upload_via_ftp_direct(source_dir):
        print_message("❌ Falha no upload FTP", Colors.RED)
        sys.exit(1)
    
    # Sucesso
    print_header("✅ DEPLOY CONCLUÍDO!")
    
    print_message("🎉 Aplicação publicada com sucesso!", Colors.GREEN)
    print_message("🌐 Acesse seu domínio para verificar", Colors.BLUE)
    print_message("✨ Não é necessário descompactar nada no servidor!", Colors.GREEN)
    
    print_message("\n📋 Arquivos enviados diretamente para:", Colors.YELLOW)
    print_message(f"   📁 {FTP_CONFIG['remote_dir']}/", Colors.BLUE)
    print_message("   📄 index.html", Colors.BLUE)
    print_message("   📄 web.config", Colors.BLUE)
    print_message("   📄 *.js, *.css, assets/", Colors.BLUE)

if __name__ == "__main__":
    main()
