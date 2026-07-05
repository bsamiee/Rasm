# Admissions plan — registry-verified 2026-07-04, applied to pnpm-workspace.yaml at build-ts completion

Catalog files carry NO versions (central-manifest law); this file is the one version source for the application step.

## Pins to add (package | version | pnpm label block | catalog tier/path)

- `ssh2` | 1.17.0 | # runtime | substrate: `libs/typescript/.api/ssh2.md`
- `webdav` | 5.10.0 | # data | `libs/typescript/data/.api/webdav.md`
- `basic-ftp` | 6.0.1 | # data | `libs/typescript/data/.api/basic-ftp.md`
- `chokidar` | 5.0.0 | # data | `libs/typescript/data/.api/chokidar.md`
- `@nats-io/kv` | 3.4.0 | # runtime | `libs/typescript/runtime/.api/nats-io-kv.md`
- `@nats-io/obj` | 3.4.0 | # runtime | `libs/typescript/runtime/.api/nats-io-obj.md`
- `@opentelemetry/api-logs` | 0.220.0 | # runtime | `libs/typescript/runtime/.api/opentelemetry-api-logs.md`
- `@opentelemetry/exporter-logs-otlp-http` | 0.220.0 | # runtime | `libs/typescript/runtime/.api/opentelemetry-exporter-logs-otlp-http.md`
- `motion` | 12.42.2 | # ui | `libs/typescript/ui/.api/motion.md`
- `@visx/*` | resolve exact subpackages from prefetch-ui-iac.md (umbrella `visx` is NOT on npm) | # ui | one catalog per admitted subpackage
- `@observablehq/plot` | 0.6.17 | # ui | `libs/typescript/ui/.api/observablehq-plot.md`
- `uplot` | 1.6.32 | # ui | `libs/typescript/ui/.api/uplot.md`
- `d3` | 7.9.0 | # ui | `libs/typescript/ui/.api/d3.md`
- `typegpu` | 0.11.9 | # ui | `libs/typescript/ui/.api/typegpu.md`
- `@perspective-dev/client` | 4.5.1 | # ui | `libs/typescript/ui/.api/perspective-dev-client.md`
- `@perspective-dev/viewer` | 4.5.1 (+ viewer plugins per dossier) | # ui | `libs/typescript/ui/.api/perspective-dev-viewer.md`
- `@pulumi/eks` | 4.2.0 | # iac | `libs/typescript/iac/.api/pulumi-eks.md`
- `@pulumi/cloudinit` | 1.6.0 | # iac | `libs/typescript/iac/.api/pulumi-cloudinit.md`
- `@pulumiverse/acme` | 0.16.1 | # iac | `libs/typescript/iac/.api/pulumiverse-acme.md`
- `@pulumi/github` | 6.14.0 | # iac | `libs/typescript/iac/.api/pulumi-github.md`
- `@pulumi/synced-folder` | 0.12.4 | # iac | `libs/typescript/iac/.api/pulumi-synced-folder.md`
- `@pulumi/esc-sdk` | 0.14.0 | # iac | `libs/typescript/iac/.api/pulumi-esc-sdk.md`
- `@pulumi/pulumiservice` | 1.3.0 | # iac | `libs/typescript/iac/.api/pulumi-pulumiservice.md`

## Pin changes (existing rows)

- `react` + `react-dom` -> `19.3.0-canary-e71a6393-20260702` (operator canary override; `@types/react`/`@types/react-dom` stay latest stable — canary types gap noted in react.md catalog)
- `@opentelemetry/*` SDK/core family -> 2.9.0; exporter family -> 0.220.0; `@opentelemetry/semantic-conventions` -> 1.41.1 (detector API changed in resources 2.9.0: `DetectedResource`, `resourceFromAttributes`)

## Rejected (do not add)

- `eslint-plugin-react-hooks` — Biome is the sole lint rail; compiler checks ride `babel-plugin-react-compiler` already admitted.
- `xstate` — mine-design-only; machines are BUILT on effect primitives.
- `crd2pulumi` — Go CLI, not an npm dependency; tooling note only.
- `node-ssh`, `ssh2-promise`, `node-scp`, `ssh2-sftp-client` — thin wrappers over `ssh2`; design-mined only.

## Application step (post-build-ts, before rebuild-ts dispatch)

1. Edit pnpm-workspace.yaml: add pins at labels above; apply pin changes; keep catalogMode strict.
2. `pnpm install` (full — node_modules must carry the new surfaces for lane member-verification).
3. Update folder README domain-package registries + the branch substrate registry (ssh2) for every admission.
4. Commit checkpoint; dispatch rebuild-ts.
