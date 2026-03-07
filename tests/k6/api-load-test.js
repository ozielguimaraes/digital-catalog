import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Configuration via environment variables
const BASE_URL = __ENV.BASE_URL || 'http://catalogo-api.sanyz.com.br'; // Default to local
const USER_EMAIL = __ENV.USER_EMAIL || 'microzapple@gmail.com';
const USER_PASSWORD = __ENV.USER_PASSWORD || 'Asdf@1234';

export const options = {
  stages: [
    { duration: '30s', target: 50 },  // Ramp-up to 50 users
    { duration: '1m', target: 50 },   // Stay at 50 users
    { duration: '30s', target: 0 },   // Ramp-down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500'], // 95% of requests must complete below 500ms
    'errors': ['rate<0.01'],            // Error rate must be less than 1%
  },
};

const HEADERS = {
  'Content-Type': 'application/json',
};

// Setup function: Runs ONCE at the beginning of the test to get the token
export function setup() {
  const payload = JSON.stringify({
    email: USER_EMAIL,
    password: USER_PASSWORD,
  });

  const res = http.post(`${BASE_URL}/api/auth/login`, payload, { headers: HEADERS });

  const success = check(res, {
    'setup login status is 200': (r) => r.status === 200,
    'setup has token': (r) => r.json('token') !== undefined,
  });

  if (!success) {
    console.error(`Setup login failed: ${res.status} ${res.body}`);
    throw new Error('Setup failed: Could not login');
  }

  // Return the token to be used by VUs
  return { token: res.json('token') };
}

export default function (data) {
  const token = data.token;
  let catalogoId;

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
      errorRate.add(1);
      return;
    }

    const body = res.json();
    const catalogsList = Array.isArray(body) ? body : (body.data || []);
    
    if (catalogsList && catalogsList.length > 0) {
      catalogoId = catalogsList[0].id;
    }
  });

  if (catalogoId) {
    group('Get Products', function () {
      const res = http.get(`${BASE_URL}/api/produtos/catalogo/${catalogoId}`, { headers: authHeaders });

      const success = check(res, {
        'get products status is 200': (r) => r.status === 200,
      });

      if (!success) {
        errorRate.add(1);
      }
    });

    group('Get Categories', function () {
        const res = http.get(`${BASE_URL}/api/categorias/catalogo/${catalogoId}`, { headers: authHeaders });
  
        const success = check(res, {
          'get categories status is 200': (r) => r.status === 200,
        });
  
        if (!success) {
          errorRate.add(1);
        }
      });
  }

  sleep(1);
}
