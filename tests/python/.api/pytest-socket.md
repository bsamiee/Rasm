# [pytest-socket] — the default-deny INET guard the whole Python suite runs under

`pytest-socket` swaps `socket.socket`, `socket.getaddrinfo`, and `socket.gethostbyname` for guarded stand-ins for the duration of a test, turning accidental egress into an immediate `SocketBlockedError` instead of a hang. The Rasm suite arms it globally through `addopts` so every test starts INET-blind; a test opts back in through the `socket_enabled` fixture, which the testkit couples to the auto-applied `network` marker. `--allow-unix-socket` keeps loopback UDS capsules alive while INET stays sealed.

## [01]-[PACKAGE_SURFACE]

- package: `pytest-socket` · version `0.8.0` · license `MIT`
- namespace: `import pytest_socket`; `pytest11` entry point `socket = pytest_socket`
- asset: `pytest_socket/__init__.py` (single-module plugin, `py.typed`)
- rail: the INET-block lane — the default session disables `socket.socket`, and only `socket_enabled`/`network` capsules lift it

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                    | [KIND]    | [CAPABILITY]                                                                                                                        |
| :-----: | :-------------------------- | :-------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SocketBlockedError`        | exception | raised when `socket.socket`/`getaddrinfo`/`gethostbyname` runs under the block; subclasses `RuntimeError` and warns on construction |
|  [02]   | `SocketConnectBlockedError` | exception | raised when `connect()` targets a host outside the `--allow-hosts` allow-list                                                       |
|  [03]   | `disable_socket`            | function  | installs `GuardedSocket`; `allow_unix_socket=True` lets `AF_UNIX` families through                                                  |
|  [04]   | `enable_socket`             | function  | restores the real `socket.socket`/`getaddrinfo`/`gethostbyname`                                                                     |
|  [05]   | `socket_allow_hosts`        | function  | installs a `connect` guard admitting plain hosts and CIDR networks; resolves hostnames through a cache                              |
|  [06]   | `normalize_allowed_hosts`   | function  | maps an allow-list to resolved IP sets via `resolve_hostnames`; `host_from_connect_args` extracts the target host                   |

```python contract
class SocketBlockedError(RuntimeError): ...
class SocketConnectBlockedError(RuntimeError): ...  # __init__(allowed: list[str], host: str | None)
def disable_socket(allow_unix_socket: bool = False) -> None: ...
def enable_socket() -> None: ...
def socket_allow_hosts(allowed: str | list[str] | None = None, allow_unix_socket: bool = False, resolution_cache: dict[str, set[str]] | None = None) -> None: ...
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                      | [KIND]     | [CAPABILITY]                                                                         |
| :-----: | :--------------------------------------------- | :--------- | :----------------------------------------------------------------------------------- |
|  [01]   | `--disable-socket`                             | CLI flag   | blocks `socket.socket` for the whole session; the suite pins it in `addopts`         |
|  [02]   | `--allow-unix-socket`                          | CLI flag   | admits `AF_UNIX` sockets while INET stays blocked; keeps loopback UDS capsules alive |
|  [03]   | `--allow-hosts`                                | CLI option | CSV of hosts or CIDR networks whose `connect()` targets pass the guard               |
|  [04]   | `--force-enable-socket`                        | CLI flag   | session override that lifts the block ahead of every per-test rule                   |
|  [05]   | `socket_enabled`                               | fixture    | lifts the block for one test; its presence drives the testkit `network` marker       |
|  [06]   | `socket_disabled`                              | fixture    | forces the block for one test, honoring the session `allow_unix_socket`              |
|  [07]   | `pytest.mark.enable_socket` / `disable_socket` | marker     | per-test override of the session default, resolved in `pytest_runtest_setup`         |
|  [08]   | `pytest.mark.allow_hosts([hosts])`             | marker     | per-test host allow-list, taking precedence over `--allow-hosts`                     |

```python contract
@pytest.fixture
def socket_enabled(pytestconfig: pytest.Config) -> Iterator[None]: ...  # calls enable_socket()
@pytest.fixture
def socket_disabled(pytestconfig: pytest.Config) -> Iterator[None]: ...  # disable_socket(allow_unix_socket=...)
# pytest_runtest_setup precedence: enable (fixture|marker|--force-enable-socket) > disable (fixture|marker) > allow_hosts > session --disable-socket
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_SOCKET_TOPOLOGY]:
- One `_PytestSocketConfig` stash row holds the session decision; `pytest_runtest_setup` resolves per test in a fixed precedence and `pytest_runtest_teardown` restores the real socket surface every time.
- The block replaces `socket.socket` with a `GuardedSocket` whose `__new__` raises unless the family is `AF_UNIX` under `allow_unix_socket`; `getaddrinfo`/`gethostbyname` raise directly, so DNS fails as fast as `connect`.
- `--allow-hosts` installs a narrower `connect`-only guard through `socket_allow_hosts`, admitting exact hosts, resolved hostnames, and CIDR networks parsed by `_partition_allowed`.

[STACKING]:
- `runtime.py`(`../_testkit/runtime.py`): `pytest_collection_modifyitems` auto-applies `pytest.mark.network` to any item whose `fixturenames` contains `socket_enabled`, so lifting the block always tags the test for the mutation-lane deselection.
- `seams.py`(`../_testkit/seams.py`): `loopback_server`/`grpc_loopback` bind `127.0.0.1` capsules that ride the `network` marker; `grpc_loopback` needs the asyncio backend and skips otherwise.
- `pyproject.toml`(`../../../pyproject.toml`): `addopts` pins `--disable-socket` and `--allow-unix-socket`; the `network` marker is declared and excluded from mutation lanes via `-m "not network"`.

[LOCAL_ADMISSION]:
- Admitted at the shared test tier through the `pytest11` entry point; no suite imports `pytest_socket` directly — the guard is session-installed and fixture-lifted.
- `required_plugins` lists `pytest-socket`, so a missing plugin fails collection instead of silently allowing egress.

[RAIL_LAW]:
- Package: `pytest-socket`
- Owns: the process-wide INET block, the UDS exemption, the host allow-list guard, and the per-test lift surface.
- Accept: `socket_enabled` (with its coupled `network` marker) for loopback and egress capsules; `--allow-hosts` for a scoped allow-list; `--allow-unix-socket` for UDS transports.
- Reject: raw `enable_socket()` calls inside a test body over the `socket_enabled` fixture; unmarked network access; `--force-enable-socket` in the default lane, which erases the guard for the whole session.
