---
name: secrets
description: Estate secret custody and consumption over Doppler and 1Password — scoped CLI reads, directory-scope resolution, the SessionStart pull rail, service-token custody classes, secret-file templating and mounts, and the read-only Doppler MCP row. Use when reading or wiring secrets, minting or routing tokens, debugging missing session env keys, rendering secret-bearing config files, or touching doppler/op surfaces. Topology mutation — projects, configs, tokens, scopes — belongs to services/topology.ts and its driver, never to ad-hoc CLI calls or per-repo files.
---

# [SECRETS]

Doppler is the single secret backend; 1Password is transitional root custody and the IaC bootstrap broker. Sanctioned Doppler surfaces on this machine: `services/topology.ts` (the only topology owner — projects, environments, configs, service tokens, and directory scopes as IaC rows), the `doppler` CLI (secret-value reads and writes against declared configs), the SessionStart hook (the only consumption rail), and `~/.doppler` (CLI-owned config and fallback state).

## [01]-[ROUTING]

- [01]-[PATTERNS](references/patterns.md): pattern doctrine — template rendering, mounts, multi-command wrappers, MCP row shape, plan-gated features.

## [02]-[RESOLUTION]

- `~/.doppler` is the CLI config dir; scopes ride `~/.doppler/.doppler.yaml`, written by `doppler configure set` through the driver's `scopes apply`.
- A repo-local `doppler.yaml` is vendor setup guidance, not the estate scope owner; the estate carries none.
- Precedence, highest first: a service token's embedded project/config, runtime flags, env vars, config-file scope.
- Config-file scope resolves an exact directory match before the nearest ancestor.
- Scope env vars: `DOPPLER_TOKEN`, `DOPPLER_PROJECT`, `DOPPLER_CONFIG`, `DOPPLER_CONFIG_DIR`, `DOPPLER_PASSPHRASE`.
- Agents pass `--project`/`--config` explicitly; env carries only token custody and hook mode switches.
- An ambient `DOPPLER_TOKEN` outranks flags and represents one config; strip it with `env -u DOPPLER_TOKEN` when fetching more than one source.

## [03]-[CLI]

| [INDEX] | [TASK]                            | [COMMAND]                                                                                  |
| :-----: | :-------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | Binary and version proof          | `doppler --version`                                                                        |
|  [02]   | Effective options, token stripped | `doppler configure debug --json \| jq 'with_entries(.value \|= del(.token))'`              |
|  [03]   | Every scope row, token stripped   | `doppler configure --all --json \| jq 'with_entries(.value \|= del(.token))'`              |
|  [04]   | One directory's scope             | `doppler configure get project config --scope <dir> --json`                                |
|  [05]   | Set a scope row                   | `doppler configure set project=<p> config=<c> --scope <dir>`                               |
|  [06]   | Unset a scope row                 | `doppler configure unset project config --scope <dir>`                                     |
|  [07]   | Key inventory                     | `doppler secrets download --project <p> --config <c> --no-file --format json \| jq 'keys'` |
|  [08]   | Inject env into a process         | `doppler run --project <p> --config <c> --command '<cmd>'`                                 |
|  [09]   | Render a template                 | `doppler secrets substitute <template>`                                                    |

- One directory's scope: keys land as `enclave.project`, `enclave.config`.
- Set a scope row: driver-owned; scope `/` stays untouched.

Secret downloads pipe to `jq 'keys'` or `jq 'length'`; configure reads strip the root CLI token with `del(.token)` — a bare `configure debug` or `configure --all` prints it. Receipts, transcripts, and logs carry key names and counts, never values or tokens.

## [04]-[PULL_RAIL]

SessionStart hook `.claude/hooks/setup-env.sh` is the only consumption rail, canonical in Parametric_Forge and byte-identical in every repo copy.

- `DOPPLER_SOURCES` rows carry the shape `project:config:snapshot[:TOKEN_ENV_VAR]`; each row resolves independently and in parallel.
- A live fetch rewrites its encrypted snapshot under `~/.cache/doppler`; a failed fetch serves it; a dead row is loud and names the keys it owes.
- `TOKEN_ENV_VAR` names a config-scoped service token; an empty or unset segment means ambient CLI auth.
- A degraded token retries ambient once and blames the token in the receipt.
- `CLAUDE_DOPPLER_OFFLINE=1` forces fallback-only fetches. `CLAUDE_DOPPLER_STALE_DAYS` (default 14) marks aged snapshots `STALE` in the receipt.
- Under `CLAUDE_SECRET_BACKEND=transition` the 1Password cache fills only unset keys — Doppler wins per key. A `doppler` backend retires the fill.
- `jq` parses each JSON dump into a literal assignment, so secret bytes never reach a shell parser. Receipts land at `~/.cache/forge-secrets/receipt`.

## [05]-[CUSTODY]

| [INDEX] | [CLASS]                          | [CUSTODY]                             | [USE]                           |
| :-----: | :------------------------------- | :------------------------------------ | :------------------------------ |
|  [01]   | User CLI token                   | Keychain login, root `/` scope        | Operator local work             |
|  [02]   | Config-scoped service token      | Hook `TOKEN_ENV_VAR` rows             | Hook and runtime reads          |
|  [03]   | IaC admin token                  | `op://Tokens/DOPPLER_IAC_TOKEN/token` | Topology writes via Pulumi only |
|  [04]   | MCP token                        | Injected as `DOPPLER_TOKEN`           | Read-only agent MCP             |
|  [05]   | Provider PATs (GitHub and peers) | Secret values inside configs          | Consumed through the pull rail  |

- User CLI token: never enters op, files, or scope rows.
- Config-scoped service token: minted by topology rows; static Developer-plan tokens are revoked and reminted, never rotated in place.
- IaC admin token: brokered by `services/driver.ts`.
- MCP token: injected through the doppler-run indirection; grants bound access, while `--read-only --project --config` narrow only the tool surface.
- Provider PATs: never topology identity.

## [06]-[OP_BOUNDARY]

- `services/driver.ts` reads Pulumi passphrase `op://Tokens/PULUMI_FORGE_SERVICES/password` and IaC token `op://Tokens/DOPPLER_IAC_TOKEN/token`.
- Ambient `PULUMI_CONFIG_PASSPHRASE` or `DOPPLER_TOKEN` short-circuits op.
- `1password.nix` emits the mode-600 transition cache `~/.config/hm-op-session.sh`; `CLAUDE_SECRET_BACKEND` selects `transition` or `doppler`.
- An exported `OP_SERVICE_ACCOUNT_TOKEN` pins `op` to service-account vaults; personal-vault work runs `env -u OP_SERVICE_ACCOUNT_TOKEN op <verb>`.

## [07]-[LAW]

- A new project lands as project/config rows in `topology.ts` plus a directory scope row, then `pulumi up`; retiring it deletes its rows.
- A repo carries zero Doppler files; its agents resolve through scope plus hook automatically.
- GitHub Actions secret sync stays rejected until a workflow consumer exists; webhooks own future redeploy triggers.
- Each sync spends a Developer-plan slot and imports no existing GitHub values.
- Rendered secret material is ephemeral: `--mount`/`--mount-template` over durable renders; plaintext binds only where the target owner requires it.
