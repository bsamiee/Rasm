# [BOUNDARIES]

Code meeting an engine, a peer, a clock, or a log takes the crate form of each `code:test` boundary kind below.

## [01]-[REAL_SERVICES]

Behavior that is an engine's answer (a query, a migration, a protocol exchange) runs against that engine:
- `testcontainers` starts an image through `AsyncRunner::start`, maps a port through `get_host_port_ipv4`, and removes the container when `ContainerAsync` drops, unless `TESTCONTAINERS_COMMAND=keep`
- Feature `watchdog` removes every started container on SIGINT, SIGTERM, and SIGQUIT, where a killed test otherwise leaves its container running
- `testcontainers-modules` images default to old pinned tags or `latest`, and `ImageExt::with_tag` pins the release production runs
- Docker hosts resolve from `tc.host` in `~/.testcontainers.properties` (feature `properties-config`), then `DOCKER_HOST`, then fixed socket paths, never from a Docker context
- Code driving an engine itself (a Docker client, a supervisor) runs its own driver against that engine, as a second client checks itself alone
- Each test owns a database named from its `<row>` or `NEXTEST_TEST_NAME`, drops a leftover of that name, creates it, and drops it on a pass

Suites against real services form a package of their own, which the suites reference places and gates:
- Every object a case creates holds one prefix and its `<row>`, and a row of one table shares no container, bucket, or unit with another
- Cases reading every object on a daemon refuse to start while foreign objects exist, and run alone through `threads-required`
- Servers the crate builds run in process on port 0 (`TcpListener::bind(("127.0.0.1", 0))`), and the test reaches them through their real client

## [02]-[WIRE_DOUBLES]

Wire-level doubles run the whole client stack against scripted answers on a real socket or stream:

```rust
async fn quota(limit: &str) -> MockServer {
    let server = MockServer::start().await;
    Mock::given(method("GET"))
        .and(path("/v1/quota"))
        .respond_with(ResponseTemplate::new(200).set_body_json(json!({ "limit": limit })))
        .mount(&server)
        .await;
    server
}

#[tokio::test]
async fn advertised_quota_in_a_decimal_unit_is_refused_by_its_unit() -> Result<(), Failure> {
    let server = quota("16KB").await;
    let read = advertised(&reqwest::Client::new(), &server.uri().parse()?).await;
    assert_matches!(read, Err(FetchError::Size(SizeError::Unit { found })) if found == "KB", "limit `16KB`");
    Ok(())
}
```

`advertised` reads a quota from `GET /v1/quota` through `error_for_status` and parses its `limit`:
- Requests no mock matches answer 404, which reaches the test as a typed error through the client's `error_for_status`
- Vendor errors the crate classifies come from a mock answering that status (`ResponseTemplate::new(<status>)`), and the crate's mapping runs on it
- `expect(n)` takes a count or a range where the call count is the product (a retry budget, a cache serving a second read)
- Dropping the `MockServer` fails a test whose expectation went unmet, naming each mock by `named`
- `mount_as_scoped` returns a guard verifying its mock when it drops, for an expectation one phase of a test holds
- `received_requests` returns each request with its `url`, `headers`, and `body`, for a test whose product is the request sent
- Closures `Fn(&Request) -> ResponseTemplate` and `Respond` impls answer by rule from each request
- `set_delay` sleeps on the server's own runtime at wall time
- Clients with a deadline of their own take a delay one step past that deadline on the real clock, asserting the typed timeout variant

`tokio_test::io::Builder` scripts a byte peer for a protocol with no server crate:
- `read`, `write`, `read_error`, `write_error`, and `wait` run in order, an unexpected write panics naming the mock, and bytes left unread or unwritten panic at drop
- `wait` sleeps on tokio's clock, and a paused test moves past it at once
- Peers answering by rule from what they read take a double over `tokio::io::duplex`, as a `Builder` script runs one fixed sequence

## [03]-[FAKES]

Fakes are a second implementor of a capability trait the code under test takes, and generic functions taking its inputs as arguments hold the cases both implementors run:

```rust
struct Fixed(BTreeMap<&'static str, u64>);

impl Quotas for Fixed {
    async fn limit(&self, tenant: &str) -> Result<u64, FetchError> {
        self.0.get(tenant).copied().ok_or_else(|| FetchError::Tenant { tenant: tenant.to_owned() })
    }
}

async fn upload_at_its_tenants_limit_is_accepted_and_one_byte_past_is_not(quotas: &impl Quotas, tenant: &str, limit: u64) -> Result<(), FetchError> {
    let read = (accepted(quotas, tenant, limit).await?, accepted(quotas, tenant, limit + 1).await?);
    assert_eq!(read, (true, false), "{tenant} holds {limit} bytes");
    Ok(())
}

async fn unknown_tenant_is_refused_by_name(quotas: &impl Quotas, tenant: &str) {
    assert_matches!(accepted(quotas, tenant, 1).await, Err(FetchError::Tenant { tenant: refused }) if refused == tenant);
}

#[tokio::test]
async fn fixed_quotas_hold_the_quota_contract() -> Result<(), FetchError> {
    let quotas = Fixed(BTreeMap::from([("studio", 16_384)]));
    upload_at_its_tenants_limit_is_accepted_and_one_byte_past_is_not(&quotas, "studio", 16_384).await?;
    unknown_tenant_is_refused_by_name(&quotas, "absent").await;
    Ok(())
}
```

`Quotas` is the production trait `accepted` takes, and `Remote`, its HTTP implementor mapping a 404 to `FetchError::Tenant`, runs the same functions over a `MockServer` and once more in the suite against the real service:
- Cases one implementor alone produces (a 500 kept as `FetchError::Http`) stay rows of that implementor's own tests
- Fakes return crate errors alone, and generated mocks stay out, as a per-call expectation asserts a call whatever came of it
- Spies are fakes sending each call as one enum value into an `mpsc` channel the test drains once its source ends, where call order is the product
- Code taking a concrete client, with no trait in its signature, takes a wire double in place of a fake

## [04]-[TIME]

Tests of pure code pass literal instants with no clock, and async code over tokio's clock runs paused:

```rust
const DEADLINE: Duration = Duration::from_secs(5);

#[tokio::test(start_paused = true)]
async fn line_inside_the_deadline_is_read_and_silence_ends_at_it() -> Result<(), LineError> {
    let late = Builder::new().wait(DEADLINE - Duration::from_millis(1)).read(b"16KiB\n").build();
    assert_eq!(line(late, DEADLINE, 64).await?, "16KiB\n", "a line one millisecond inside the deadline");
    let (silent, _held) = tokio::io::duplex(64);
    let started = Instant::now();
    assert_matches!(line(silent, DEADLINE, 64).await, Err(LineError::Deadline { deadline: DEADLINE }));
    assert_eq!(started.elapsed(), DEADLINE, "a silent peer is given up on at the deadline");
    Ok(())
}
```

`line` reads one line under a deadline and a byte limit, and `Instant` is `tokio::time::Instant`:
- `start_paused = true` needs tokio's `test-util` feature and the current-thread flavor, and `multi_thread` with it fails to compile
- Paused time jumps to the next timer once every task idles, and `timeout` and `sleep` finish at once with `elapsed()` equal to the deadline
- Absence holds exactly: a `timeout` on a channel that never answers ends once every task idles, and the test asserts nothing arrived
- Peers under a paused clock are in memory (`duplex`, `Builder`), as an answer still on a real socket or from a child process loses to a timer the clock jumps to
- `std::time::Instant` and `SystemTime` keep real time, and a running `spawn_blocking` task holds the clock still
- Doubles holding a call open signal entry and wait for release through `tokio::sync::Notify`, `Semaphore`, or `oneshot`, and the test orders tasks with no sleep
- `futures::FutureExt::now_or_never` polls a future once, for a test that a step completes with no wait

## [05]-[EVENTS]

`tracing-test` captures events for code whose product is its log (an audit trail):
- `#[traced_test]` sits below `#[tokio::test]` or above `#[test]`, and `logs_contain` and `logs_assert` read the lines of the test's own span
- Its filter keeps the crate its test's first module path segment names, which is the library where the test target takes the library's name
- Negative `logs_contain` checks under any other target name pass with no event seen
- Feature `no-env-filter` keeps every crate's events
- Events from a `std::thread::spawn` thread fall outside the test's span, and a current-thread `tokio::spawn` task stays inside it
