
import os
import time
import ftplib
import ssl
from urllib.parse import urlparse

# Configurações vindas das variáveis de ambiente
FTP_URL = os.environ.get('FTP_URL')
FTP_USER = os.environ.get('FTP_USER')
FTP_PASS = os.environ.get('FTP_PASS')
LOCAL_DIR = os.environ.get('LOCAL_DIR')
REMOTE_DIR = os.environ.get('REMOTE_DIR', '/9295_sanyz_api/wwwroot')

# Conteúdo do arquivo de manutenção
APP_OFFLINE_CONTENT = """
<!DOCTYPE html>
<html>
<head>
    <title>Atualizando Sistema</title>
    <style>
        body { font-family: sans-serif; text-align: center; padding-top: 50px; }
    </style>
</head>
<body>
    <h1>Estamos atualizando o sistema</h1>
    <p>O servidor esta sendo atualizado. Voltaremos em instantes.</p>
</body>
</html>
"""

def get_ftp_host(url):
    if "://" not in url:
        return url
    return urlparse(url).hostname

def connect_ftp():
    host = get_ftp_host(FTP_URL)
    print(f"🔌 Conectando ao FTP: {host}...")
    try:
        ftps = ftplib.FTP_TLS()
        ftps.connect(host, 21)
        ftps.login(FTP_USER, FTP_PASS)
        ftps.prot_p()
        print("✅ Conexão FTPS (Segura) estabelecida.")
        return ftps
    except Exception as e:
        print(f"⚠️ Falha no FTPS ({e}). Tentando FTP padrão...")
        ftp = ftplib.FTP()
        ftp.connect(host, 21)
        ftp.login(FTP_USER, FTP_PASS)
        print("✅ Conexão FTP padrão estabelecida.")
        return ftp

def try_delete_or_rename(ftp, filename):
    """Tenta deletar, se falhar (arquivo em uso), tenta renomear para .old"""
    try:
        ftp.delete(filename)
        return True
    except Exception as e:
        # Se erro for 550 (arquivo em uso), tenta renomear
        if "550" in str(e):
            print(f"⚠️ Arquivo {filename} em uso. Tentando renomear para .old...")
            try:
                timestamp = int(time.time())
                ftp.rename(filename, f"{filename}.old.{timestamp}")
                print(f"✅ Renomeado com sucesso. O novo arquivo poderá ser enviado.")
                return True
            except Exception as rename_error:
                print(f"❌ Falha ao renomear {filename}: {rename_error}")
        return False

def upload_files(ftp, local_path, remote_path):
    try:
        ftp.cwd(remote_path)
    except:
        print(f"Diretório {remote_path} não existe, tentando criar...")
        try:
            ftp.mkd(remote_path)
            ftp.cwd(remote_path)
        except Exception as e:
            print(f"❌ Erro ao acessar/criar diretório remoto: {e}")
            return

    for item in os.listdir(local_path):
        local_item_path = os.path.join(local_path, item)
        
        if os.path.isfile(local_item_path):
            # Se o arquivo já existe no remoto, tenta limpar o caminho antes
            files_in_remote = []
            try:
                files_in_remote = ftp.nlst()
            except:
                pass

            if item in files_in_remote:
                try_delete_or_rename(ftp, item)

            print(f"⬆️  Upload: {item}")
            try:
                with open(local_item_path, 'rb') as f:
                    ftp.storbinary(f'STOR {item}', f)
            except Exception as e:
                print(f"❌ Erro no upload de {item}: {e}")
                # Última tentativa: renomear o destino e tentar de novo
                if "550" in str(e):
                     try_delete_or_rename(ftp, item)
                     with open(local_item_path, 'rb') as f:
                        ftp.storbinary(f'STOR {item}', f)

        elif os.path.isdir(local_item_path):
            remote_subdir = f"{remote_path}/{item}"
            try:
                ftp.mkd(item)
            except:
                pass
            upload_files(ftp, local_item_path, remote_subdir)
            ftp.cwd(remote_path)

def main():
    if not all([FTP_URL, FTP_USER, FTP_PASS, LOCAL_DIR]):
        print("❌ Erro: Variáveis de ambiente FTP_URL, FTP_USER, FTP_PASS ou LOCAL_DIR não definidas.")
        exit(1)

    ftp = connect_ftp()
    
    try:
        # Navegar para raiz
        try:
            ftp.cwd(REMOTE_DIR)
        except:
            print(f"⚠️ Não foi possível entrar em {REMOTE_DIR}, tentando raiz...")
            ftp.cwd('/')

        print("\n🛑 1. Parando a aplicação (app_offline.htm)...")
        with open('app_offline.htm', 'w') as f:
            f.write(APP_OFFLINE_CONTENT)
        
        try:
            with open('app_offline.htm', 'rb') as f:
                ftp.storbinary('STOR app_offline.htm', f)
        except Exception as e:
            print(f"⚠️ Aviso: Não foi possível subir app_offline.htm: {e}")
        
        print("⏳ Aguardando 10 segundos para liberação dos arquivos...")
        time.sleep(10)

        print("\n🚀 2. Iniciando upload dos arquivos...")
        upload_files(ftp, LOCAL_DIR, REMOTE_DIR)

        print("\n✅ 3. Removendo app_offline.htm para reiniciar a aplicação...")
        ftp.cwd(REMOTE_DIR)
        try:
            ftp.delete('app_offline.htm')
            print("✅ Aplicação reiniciada com sucesso!")
        except Exception as e:
            print(f"⚠️ Aviso: Não foi possível deletar app_offline.htm: {e}")

    except Exception as e:
        print(f"❌ Erro crítico durante o deploy: {e}")
        exit(1)
    finally:
        try:
            ftp.quit()
        except:
            pass

if __name__ == "__main__":
    main()
