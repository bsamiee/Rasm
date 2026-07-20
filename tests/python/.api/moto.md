# [PY_TESTS_API_MOTO]

`moto` intercepts AWS SDK traffic and answers it from process-local backend state, so a spec exercises S3 semantics with no network and no credentials. `moto[server]` adds `ThreadedMotoServer`, a Werkzeug server projecting those backends over a real HTTP endpoint — the exact surface the `_testkit` `ObjectStore` double binds so `s3fs` speaks to genuine S3-native egress (e-tags, presigned GET) that an in-memory `MemoryFileSystem` cannot forge. S3 is the one service the estate drives; the mock covers the wider AWS surface as latent capability, never a differentiated entry.

## [01]-[PACKAGE_SURFACE]

- package: `moto` · version `5.2.2` · license `Apache-2.0` · extra `moto[server]`
- namespace: `import moto` (decorator `moto.mock_aws`) · `from moto.server import ThreadedMotoServer`
- asset: pure-Python wheel; `server` extra pulls `flask`/`flask-cors`; SDK boundary rides `boto3` `1.43.0`
- rail: object-store double — the loopback S3 endpoint every `ObjectStore` provision terminates in

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]             | [KIND]                    | [CAPABILITY]                                                                          |
| :-----: | :------------------- | :------------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `ThreadedMotoServer` | class                     | Werkzeug server on a background thread projecting every backend over HTTP             |
|  [02]   | `MockAWS`            | context object            | the object `mock_aws()` returns; `start`/`stop` bracket the intercept window          |
|  [03]   | `DefaultConfig`      | config mapping            | per-service knobs (`core.reset_boto3_session`, passthrough) forwarded into `mock_aws` |
|  [04]   | `DEFAULT_ACCOUNT_ID` | constant `"123456789012"` | the `moto.core` account id backends key under for the static test credentials         |

```python signature
class ThreadedMotoServer:
    def __init__(self, ip_address: str = "0.0.0.0", port: int = 5000, verbose: bool = True) -> None: ...
    def start(self) -> None: ...
    def get_host_and_port(self) -> tuple[str, int]: ...      # port=0 yields the OS-assigned ephemeral port
    def stop(self) -> None: ...
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                          | [KIND]              | [CAPABILITY]                                                                 |
| :-----: | :--------------------------------- | :------------------ | :--------------------------------------------------------------------------- |
|  [01]   | `mock_aws(func=None, config=None)` | decorator + context | unified intercept over every AWS service; wraps a callable or a `with` block |
|  [02]   | `ThreadedMotoServer(...).start()`  | server boot         | binds a loopback HTTP endpoint an SDK or `s3fs` targets by `endpoint_url`    |
|  [03]   | `.get_host_and_port()`             | endpoint read       | resolves the ephemeral `(host, port)` after a `port=0` bind                  |
|  [04]   | `POST {endpoint}/moto-api/reset`   | backend reset       | drops all process-global backend state; the server-lane `mock_aws` teardown  |
|  [05]   | `moto.backends.get_backend(name)`  | backend handle      | in-process backend registry by service name; `.reset()` per account/region   |

```python signature
def mock_aws(func: Callable[P, T] | None = None, config: DefaultConfig | None = None) -> MockAWS | Callable[P, T]: ...
# ObjectStore double: ephemeral bind, resolve endpoint, project an s3fs view over it.
server = ThreadedMotoServer(ip_address="127.0.0.1", port=0, verbose=False); server.start()
host, port = server.get_host_and_port(); endpoint = f"http://{host}:{port}"
httpx.post(f"{endpoint}/moto-api/reset", timeout=5.0)   # idempotent teardown — process-global state cleared
```

## [04]-[IMPLEMENTATION_LAW]

[MOTO_TOPOLOGY]:
- Backend state is process-global, keyed by `(account_id, region, service)`; concurrent `ThreadedMotoServer` endpoints share one backend set, so isolation is per-key naming and reset, never per-server.
- `mock_aws` is the single intercept surface; the service discriminates by which SDK client the guarded code constructs, never by a per-service decorator.
- `moto[server]` is the only path to a real wire endpoint; the bare decorator patches the SDK in-process and exposes no HTTP surface an out-of-process client (`s3fs`) can reach.
- `port=0` binds an OS-ephemeral port the OS reissues to a later provision, so endpoint novelty never isolates state — the `s3fs` view fences itself against a dead endpoint (`.api/s3fs.md`) and isolation rides the reset.

[STACKING]:
- `s3fs`(`.api/s3fs.md`): `_provision_store` binds a `ThreadedMotoServer` loopback and projects an `S3FileSystem(endpoint_url=endpoint)` view; the real HTTP wire carries e-tags and presigned GET the `MemoryFileSystem` double refuses — the honest capability split `test_env.py` proves.
- `env.py`(`../_testkit/env.py`): `ObjectStore` teardown POSTs `/moto-api/reset` before `server.stop()`, so a later provision starts pristine over the shared process-global backend; a dead endpoint short-circuits to `stop()` since no state remains.
- `network` marker: every `ObjectStore` spec binds a real INET loopback socket, so it carries `network` and is excluded from the mutation lane where sockets stay disabled.

[LOCAL_ADMISSION]:
- Admitted at the dev-plane test tier only (`[dependency-groups] dev`, `moto[server]`); never a runtime `libs/python` dependency.
- `ObjectStore` is the sole moto consumer — specs reach S3 semantics through `provision(ObjectStore())`, never a bare `mock_aws` or a raw `ThreadedMotoServer`.

[RAIL_LAW]:
- Package: `moto[server]`
- Owns: the process-global AWS backend mock and its real HTTP projection — the S3-compatible loopback endpoint under `_testkit`.
- Accept: `ThreadedMotoServer(ip_address="127.0.0.1", port=0)` for the ephemeral loopback; `/moto-api/reset` for idempotent process-global teardown; `mock_aws` only where an in-process SDK patch suffices and no external client reads the wire.
- Reject: a hardcoded host port; a retired per-service `mock_*` decorator; a provision that skips reset (leaks residue keys across the shared backend); any moto import outside the `_testkit` `env.py` owner.
