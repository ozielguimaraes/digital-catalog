# Performance Readiness and Benchmark Runbook

## Goal

Produce a reliable throughput comparison and validate whether backend changes deliver at least **2x requests/second** with stable error rate and latency.

## Success Criteria

- Throughput (`http_reqs`) is at least **2.0x** the baseline median.
- `http_req_failed < 1%`.
- `errors < 1%`.
- `p(95) http_req_duration < 500ms`.

## Current Blocking Signal

The latest runs show infrastructure instability, not only application limits:

- SQL login failures under concurrency (`SqlException: Logon failed ... due to trigger execution`).
- Intermittent transport failures (`EOF`) during high pressure.

Until these are fixed, throughput comparisons are not trustworthy.

## Infra Readiness Checklist

Before running benchmark rounds, confirm all items:

1. **Database Authentication Stability**
   - Validate SQL login trigger behavior for the application user under burst connection load.
   - Confirm no login trigger side effects, policy lockouts, or throttling for service user.
   - Confirm SQL error log has no repeated login trigger failures.

2. **Connection Capacity**
   - Validate SQL max worker/session limits for expected concurrent API requests.
   - Confirm app-to-DB network path is stable (no firewall idle reset / TLS handshake bottleneck).
   - Keep API and DB in low-latency network path when possible.

3. **API Host Stability**
   - Ensure API process does not restart during load.
   - Ensure no aggressive reverse-proxy timeouts below k6 request timeout.
   - Keep tracing/profile sampling low in benchmark mode.

4. **Benchmark Hygiene**
   - Use same dataset, same user credentials strategy, same script version.
   - Run each scenario at least 3 times and use median values.
   - Discard first run as warm-up when cache is cold.

## SQL Diagnostic Queries (Run on SQL Server)

Use these to verify contention and login pressure during test windows:

```sql
SELECT TOP (50)
    login_name,
    session_id,
    status,
    host_name,
    program_name,
    login_time
FROM sys.dm_exec_sessions
WHERE is_user_process = 1
ORDER BY login_time DESC;
```

```sql
SELECT TOP (50)
    r.session_id,
    r.status,
    r.wait_type,
    r.wait_time,
    r.blocking_session_id,
    r.cpu_time,
    r.total_elapsed_time
FROM sys.dm_exec_requests r
ORDER BY r.total_elapsed_time DESC;
```

```sql
SELECT
    wait_type,
    waiting_tasks_count,
    wait_time_ms
FROM sys.dm_os_wait_stats
WHERE wait_type IN (
    'THREADPOOL',
    'RESOURCE_SEMAPHORE',
    'PAGEIOLATCH_SH',
    'LCK_M_X',
    'LCK_M_S',
    'ASYNC_NETWORK_IO',
    'WRITELOG'
)
ORDER BY wait_time_ms DESC;
```

## Benchmark Matrix

Run all scenarios with identical environment and capture full output.

### Scenario A: Baseline Reproduction

```bash
BASE_URL=http://127.0.0.1:5055 k6 run tests/k6/load-test.js
```

Repeat 3 times and record:

- `http_reqs`
- `http_req_failed`
- `http_req_duration p(95)`
- `errors`
- `iterations`

### Scenario B: Stability Gate

Run the same command after confirming DB/login-trigger stability.

Gate to proceed:

- No SQL login-trigger errors during the whole run.
- No transport-level `EOF` spike.

### Scenario C: Throughput Verification

Run 3 additional rounds and compare medians:

- `median(http_reqs_after) / median(http_reqs_baseline) >= 2.0`

If not reached, profile bottleneck layer:

- API CPU saturation
- SQL waits and login/session pressure
- network/reset behavior

## Output Template

Use this table per run:

| Run | http_reqs | req/s | http_req_failed | p95 | errors | Notes |
|-----|-----------|-------|-----------------|-----|--------|-------|
| A1  |           |       |                 |     |        |       |
| A2  |           |       |                 |     |        |       |
| A3  |           |       |                 |     |        |       |
| B1  |           |       |                 |     |        |       |
| C1  |           |       |                 |     |        |       |
| C2  |           |       |                 |     |        |       |
| C3  |           |       |                 |     |        |       |

## Decision Rules

- If SQL login-trigger errors appear: stop optimization work on API code and fix DB auth path first.
- If `http_req_failed` stays above 1% with stable DB: profile API and proxy timeouts.
- If p95 is high but fail rate low: tune query shape, payload size, and thread/connection pools.

