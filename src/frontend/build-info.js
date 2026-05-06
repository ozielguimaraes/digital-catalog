// Script para gerar informações detalhadas do build
const fs = require('fs');
const path = require('path');

// Obter informações do sistema e build
const packageJson = JSON.parse(fs.readFileSync('package.json', 'utf8'));
const now = new Date();

const buildInfo = {
  application: {
    name: packageJson.name || 'Meu Catálogo',
    version: packageJson.version || '1.0.0',
    description: packageJson.description || 'Catálogo Digital'
  },
  build: {
    date: now.toISOString(),
    dateFormatted: now.toLocaleString('pt-BR'),
    timestamp: now.getTime(),
    environment: 'production'
  },
  system: {
    platform: process.platform,
    nodeVersion: process.version,
    arch: process.arch
  },
  files: {
    totalSize: 0,
    fileCount: 0
  }
};

// Calcular tamanho dos arquivos
function calculateSize(dirPath) {
  let totalSize = 0;
  let fileCount = 0;
  
  function scanDir(currentPath) {
    const items = fs.readdirSync(currentPath);
    
    items.forEach(item => {
      const itemPath = path.join(currentPath, item);
      const stat = fs.statSync(itemPath);
      
      if (stat.isDirectory()) {
        scanDir(itemPath);
      } else {
        totalSize += stat.size;
        fileCount++;
      }
    });
  }
  
  if (fs.existsSync(dirPath)) {
    scanDir(dirPath);
  }
  
  return { totalSize, fileCount };
}

const distRootPath = path.join(__dirname, 'dist', 'ng-tailadmin');
const distPath = fs.existsSync(path.join(distRootPath, 'browser'))
  ? path.join(distRootPath, 'browser')
  : distRootPath;
const sizeInfo = calculateSize(distPath);

buildInfo.files.totalSize = sizeInfo.totalSize;
buildInfo.files.fileCount = sizeInfo.fileCount;
buildInfo.files.totalSizeFormatted = formatBytes(sizeInfo.totalSize);

function formatBytes(bytes) {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Salvar informações do build
const buildInfoPath = path.join(distPath, 'build-info.json');
fs.writeFileSync(buildInfoPath, JSON.stringify(buildInfo, null, 2));

// Criar arquivo de versão legível
const versionPath = path.join(distPath, 'version.txt');
const versionContent = `
===============================================
    MEU CATÁLOGO DIGITAL - BUILD INFO
===============================================

Aplicação: ${buildInfo.application.name}
Versão: ${buildInfo.application.version}
Descrição: ${buildInfo.application.description}

Data do Build: ${buildInfo.build.dateFormatted}
Timestamp: ${buildInfo.build.timestamp}
Ambiente: ${buildInfo.build.environment}

Sistema:
- Plataforma: ${buildInfo.system.platform}
- Node.js: ${buildInfo.system.nodeVersion}
- Arquitetura: ${buildInfo.system.arch}

Arquivos:
- Total de arquivos: ${buildInfo.files.fileCount}
- Tamanho total: ${buildInfo.files.totalSizeFormatted}

===============================================
Build gerado automaticamente em ${buildInfo.build.dateFormatted}
===============================================
`;

fs.writeFileSync(versionPath, versionContent);

console.log('✅ Build info gerado com sucesso!');
console.log(`📊 Arquivos: ${buildInfo.files.fileCount}`);
console.log(`📏 Tamanho: ${buildInfo.files.totalSizeFormatted}`);
console.log(`📄 Versão: ${buildInfo.application.version}`);
