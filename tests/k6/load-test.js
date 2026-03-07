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
    { duration: '1m', target: 200 },  // Ramp-up to 50 users over 1 minute
    { duration: '3m', target: 50 },  // Stay at 50 users for 3 minutes
    { duration: '1m', target: 0 },   // Ramp-down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500'], // 95% of requests must complete below 500ms
    'errors': ['rate<0.01'],            // Error rate must be less than 1%
  },
};

const HEADERS = {
  'Content-Type': 'application/json',
};

export default function () {
  let token;
  let catalogoId;

  group('Authentication', function () {
    const payload = JSON.stringify({
      email: USER_EMAIL,
      password: USER_PASSWORD,
    });

    const res = http.post(`${BASE_URL}/api/auth/login`, payload, { headers: HEADERS });

    const success = check(res, {
      'login status is 200': (r) => r.status === 200,
      'has token': (r) => r.json('token') !== undefined,
    });

    if (!success) {
      errorRate.add(1);
      console.error(`Login failed: ${res.status} ${res.body}`);
      return; // Stop iteration if login fails
    }

    token = res.json('token');
  });

  if (!token) return;

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

    // BaseApiController HandleApiResponse unwraps the data, so the response is the array itself.
    // However, let's be safe and check if it's an array or object with data property.
    const body = res.json();
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
