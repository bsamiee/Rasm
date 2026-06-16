# [PY_RUNTIME]

`runtime` is the Python-local execution foundation every `libs/python` sibling consumes. It owns caller-owned context/settings admission, the single boundary-fault + Result/Option rail, the one resilience policy, the one content-identity owner, resource roots, bounded concurrency lanes, local receipts + the contributor port, the inbound companion gRPC server-runtime + credential axis, external-API + structural-parsing evidence, and the private daemon/CLI entrypoint grammar. This package currently contains planning and API evidence only; future source lands directly in this folder after the planning fences and `.api` pages admit it.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: the seven owner pages (context-settings, rails-resilience, content-identity, resources-lanes, observability, server-host, evidence) carrying transcription-complete signature fences.

[API]:
- Path: `.api/README.md`
- Owns: the verified `api-*.md` evidence for runtime dependencies; the companion server wire (`grpcio`/`grpcio-tools`/`protobuf`) rides the `python_version<'3.13'` floor.

[BOUNDARY]:
- Runtime receives host facts from app and tool owners and never discovers the host.
- `Rasm.AppHost` owns host lifecycle, global clock, health, support capture, product telemetry export, and product runtime composition.
- The `ServerHost` owner serves the companion's inbound gRPC only; the C# side mints the wire vocabulary and the runtime implements it.
