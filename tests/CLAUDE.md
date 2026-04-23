# Tests

k6 load tests for the backend API. No unit or integration test project — these are black-box performance tests against a deployed API.

## Files (`k6/`)

- **`load-test.js`** — heavy production stress profile. Stages: ramp to 1200 VUs (1m) → drop to 500 VUs (3m) → wind down to 30 VUs (1m). Thresholds: `http_req_duration p(95)<500ms`, error rate <1%, failure rate <1%. Flow: login → list catalogs → list products/categories (all authenticated).
- **`api-load-test.js`** — lightweight baseline. Stages: ramp to 50 VUs (30s) → hold (1m) → ramp down (30s). Same endpoint flow as `load-test.js`.
- **`README.md`** — how to run.
- **`PERFORMANCE-RUNBOOK.md`** — DB stability checklist (SQL login trigger, connection limits, API host) and success criteria for throughput comparison.

## How to Run

```bash
k6 run k6/load-test.js
# With env overrides:
k6 run -e BASE_URL=https://api.example.com -e USER_EMAIL=... -e USER_PASSWORD=... k6/load-test.js
```

Defaults in the scripts hit `http://catalogo-api.sanyz.com.br` with a seeded test user.

## Gotchas

- **Default credentials are embedded** in the scripts (`microzapple@gmail.com` / `Asdf@1234`). Override via env vars; do not commit new credentials.
- **Infrastructure-sensitive**: the runbook documents SQL login trigger failures and intermittent EOF transport errors under high concurrency. Don't chase phantom regressions — run the DB stability checklist first.
- **Benchmark protocol**: 3 runs per scenario, discard the first as warm-up, compare the median. Without this you will mis-read variance as a regression.
- **Success gate**: throughput ≥ 2× baseline, failure/error rate < 1%, `p(95) < 500ms`. Treat any threshold breach as a hard fail.

## When Adding Code

- New scenario → add a `.js` in `k6/`, follow the existing `stages`/`thresholds`/`http.post` pattern.
- Don't put unit tests here — this folder is load tests only. If unit tests are introduced, add a sibling folder with its own `CLAUDE.md`.
