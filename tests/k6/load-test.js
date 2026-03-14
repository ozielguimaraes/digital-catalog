import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');
const tokenMissingRate = new Rate('token_missing');

const BASE_URL = __ENV.BASE_URL || 'http://catalogo-api.sanyz.com.br';
const USER_EMAIL = __ENV.USER_EMAIL || 'microzapple@gmail.com';
const USER_PASSWORD = __ENV.USER_PASSWORD || 'Asdf@1234';

export const options = {
  stages: [
    { duration: '1m', target: 1200 },
    { duration: '3m', target: 500 },
    { duration: '1m', target: 30 },
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500'],
    'http_req_failed': ['rate<0.01'],
    'errors': ['rate<0.01'],
    'token_missing': ['rate<0.01'],
  },
};

const HEADERS = {
  'Content-Type': 'application/json',
};

function safeJson(res) {
  try {
    return res.json();
  } catch (_) {
    return null;
  }
}

function extractToken(body) {
  if (!body || typeof body !== 'object') return undefined;
  return body.token || body.access_token || body?.data?.token || body?.data?.access_token;
}

function sampleLog(prefix, res) {
  if (__ITER % 100 !== 0) return;
  console.error(`${prefix}: status=${res.status} body=${String(res.body).slice(0, 400)}`);
}

export default function () {
  let token;
  let catalogoId;
  let iterationFailed = false;

  group('Authentication', function () {
    const payload = JSON.stringify({
      email: USER_EMAIL,
      password: USER_PASSWORD,
    });

    const res = http.post(`${BASE_URL}/api/auth/login`, payload, { headers: HEADERS });
    const body = safeJson(res);
    token = extractToken(body);

    const loginStatusOk = check(res, {
      'login status is 200': (r) => r.status === 200,
    });
    const hasToken = check({ token }, {
      'has token': (t) => t.token !== undefined && t.token !== null && t.token !== '',
    });
    const success = loginStatusOk && hasToken;
    tokenMissingRate.add(loginStatusOk && !hasToken ? 1 : 0);

    if (!success) {
      iterationFailed = true;
      sampleLog('Login failed', res);
    }
  });

  if (!token) {
    errorRate.add(1);
    sleep(1);
    return;
  }

  const authHeaders = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`,
  };

  group('Get Catalogs', function () {
    const res = http.get(`${BASE_URL}/api/catalogos/meus`, { headers: authHeaders });

    const success = check(res, {
      'get catalogs status is 200': (r) => r.status === 200,
    });

    if (!success) {
      iterationFailed = true;
      sampleLog('Get catalogs failed', res);
      return;
    }

    const body = safeJson(res);
    const catalogs = Array.isArray(body) ? body : (body.data || []);
    
    if (catalogs && catalogs.length > 0) {
      catalogoId = catalogs[0].id;
    }
  });

  if (catalogoId) {
    group('Get Products', function () {
      const res = http.get(`${BASE_URL}/api/produtos/catalogo/${catalogoId}`, { headers: authHeaders });

      const success = check(res, {
        'get products status is 200': (r) => r.status === 200,
      });

      if (!success) {
        iterationFailed = true;
        sampleLog('Get products failed', res);
      }
    });

    group('Get Categories', function () {
        const res = http.get(`${BASE_URL}/api/categorias/catalogo/${catalogoId}`, { headers: authHeaders });
  
        const success = check(res, {
          'get categories status is 200': (r) => r.status === 200,
        });
  
        if (!success) {
          iterationFailed = true;
          sampleLog('Get categories failed', res);
        }
      });
  }

  errorRate.add(iterationFailed ? 1 : 0);
  sleep(1);
}
