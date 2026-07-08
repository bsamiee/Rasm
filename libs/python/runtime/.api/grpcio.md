# [PY_RUNTIME_API_GRPCIO]

Full surface and stacking: `libs/python/.api/grpcio.md` (shared-tier canonical owner).

`grpcio` supplies the HTTP/2 RPC transport for the transport/serve rail. This overlay carries only the runtime-specific delta: the `grpc.aio` serve-leg method surface the branch tier declares abstract, the UDS locality credential pair, and the `AioRpcError` trailing-metadata lift.

## [01]-[LOCAL_ADMISSION]

[LOCAL_ADMISSION]:
- The transport/serve surface composes `grpcio` for HTTP/2 RPC; the runtime owns no second RPC transport and no parallel channel type per security mode.
- `transport/serve#SERVE`'s `CredentialPolicy` decode — the Python half of the C#-minted axis of the same name — is the single mint point for the TLS, local-loopback, and per-call credential families; TLS material comes from the caller-owned settings model, never inline literals.
- Tracing interceptors come settled from `.api/opentelemetry-instrumentation-grpc.md` (`aio_server_interceptor`), never hand-rolled here.

## [02]-[RUNTIME_DELTA]

[DELTA_SCOPE]: `grpc.aio.Server` serve lifecycle (`transport/serve#ServerHost`)
- The serve leg drives one `grpc.aio.server(interceptors=[...])` through `add_secure_port(addr, creds)`/`add_insecure_port(addr)`, `start()`, `stop(grace)`, and `wait_for_termination(timeout)` — the ordered receipted drain under the anyio lane, `insecure_port` admitted only for in-cluster/loopback targets.

[DELTA_SCOPE]: `grpc.aio.ServicerContext` per-RPC surface
- The branch tier declares `ServicerContext` abstract; the serve handlers compose its methods directly: `abort(code, details)`/`set_code`/`set_details` for status egress, `invocation_metadata()`/`send_initial_metadata`/`set_trailing_metadata` for the metadata seam, `read()`/`write(msg)` for the streaming pump, and `auth_context()`/`peer_identities()`/`peer()`/`time_remaining()` for verified peer identity and the inbound deadline budget — never a self-asserted metadata claim.

[DELTA_SCOPE]: UDS locality credential (`CredentialPolicy.loopback`)
- The in-host sidecar leg authenticates by socket locality through the `local_server_credentials(grpc.LocalConnectionType.UDS)`/`local_channel_credentials` pair, the kernel-reported peer credential standing in for a PEM bundle.

[INTEGRATION_STACK]:
- A failed client call raises `grpc.aio.AioRpcError`; its `code()` (a `grpc.StatusCode`), `details()`, and `trailing_metadata()` lift at the channel boundary into a typed `Result`, and transient members (`UNAVAILABLE`/`DEADLINE_EXCEEDED`/`RESOURCE_EXHAUSTED`) route through the `stamina` retry owner over the reused channel — never a bare `except` swallowing the status.
