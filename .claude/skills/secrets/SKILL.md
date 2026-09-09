---
name: secrets
description: "Use when a secret, token, or environment variable needs creating or fails to resolve, covering Doppler, 1Password, session cache, and storage."
---

# [SECRETS]

Doppler owns the runtime secrets a process reads under a project and config. 1Password keeps the credentials a person uses, IaC and Pulumi Cloud tokens among them.

Repository-owned resources (the Doppler project, environments, configs, and the GitHub repository settings) are rows in the repository's `infra/` program. The machine's projects and directory scopes are rows in `Parametric_Forge/services/topology.ts` applied by its `driver.ts`. `doppler` reads and writes secret values against declared configs, `doppler run` injects values at the consuming process, `~/.doppler` holds CLI scope and authentication state. A repository holds no `.env` and no `doppler.yaml`.

- [01]-[PATTERNS](references/patterns.md): Consumption patterns for secret material that is not process-env shaped

## [01]-[RESOLUTION]

- `~/.doppler/.doppler.yaml` holds the scopes, written by `doppler configure set` through the driver's `scopes apply`
- Precedence, highest first: a service token's embedded project and config, runtime flags, env vars, config-file scope
- Config-file scope resolves an exact directory match before the nearest ancestor
- Scope env vars: `DOPPLER_TOKEN`, `DOPPLER_PROJECT`, `DOPPLER_CONFIG`, `DOPPLER_CONFIG_DIR`, `DOPPLER_PASSPHRASE`
- Agents pass `--project` and `--config`, env holds the token alone
- Ambient `DOPPLER_TOKEN` outranks flags and represents one config, `env -u DOPPLER_TOKEN` strips it when fetching more than one source

## [02]-[DOPPLER_CLI]

| [INDEX] | [TASK]                            | [COMMAND]                                                                                  |
| :-----: | :-------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | Binary and version proof          | `doppler --version`                                                                        |
|  [02]   | Effective options, token stripped | `doppler configure debug --json \| jq 'with_entries(.value \|= del(.token))'`              |
|  [03]   | Every scope entry, token stripped | `doppler configure --all --json \| jq 'with_entries(.value \|= del(.token))'`              |
|  [04]   | One directory's scope             | `doppler configure get project config --scope <dir> --json`                                |
|  [05]   | Set a scope entry                 | `doppler configure set project=<p> config=<c> --scope <dir>`                               |
|  [06]   | Unset a scope entry               | `doppler configure unset project config --scope <dir>`                                     |
|  [07]   | Key inventory                     | `doppler secrets download --project <p> --config <c> --no-file --format json \| jq 'keys'` |
|  [08]   | Inject env into a process         | `doppler run --project <p> --config <c> --command '<cmd>'`                                 |
|  [09]   | Render a template                 | `doppler secrets substitute <template>`                                                    |

- One directory's scope keys are `enclave.project` and `enclave.config`, the set is driver-owned and scope `/` stays as found
- Secret downloads pipe to `jq 'keys'` or `jq 'length'`, configure reads strip the root token with `del(.token)`
- Transcripts and logs hold key names and counts

## [03]-[OP_CLI]

`op` reads the local store, each vault decides the account its reads run under:
- Exported `OP_SERVICE_ACCOUNT_TOKEN` pins `op` to the `Tokens` vault of service tokens, field suffix `token`, `credential`, or `password`
- Person credentials (Login items with `username`, `password`, and `one-time password` fields, the SSH key) sit in the `Personal` vault
- `Personal` rows run under `env -u OP_SERVICE_ACCOUNT_TOKEN`, the service account answers `isn't a vault` for it

| [INDEX] | [TASK]                          | [COMMAND]                                                                           |
| :-----: | :------------------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | Auth proof                      | `op whoami`                                                                         |
|  [02]   | Vault inventory                 | `op vault list`                                                                     |
|  [03]   | Tokens item names               | `op item list --vault Tokens --format json \| jq -r '.[].title'`                    |
|  [04]   | Read one secret                 | `op read "op://Tokens/<ITEM>/<token\|credential\|password>"`                        |
|  [05]   | Resolve the rebuild template    | `op inject -i ~/.config/op/env.template -o <out>`                                   |
|  [06]   | Fields of a Personal item       | `op item get <item> --vault Personal --format json \| jq 'del(.fields[].value)'`    |
|  [07]   | One Personal field              | `op read "op://Personal/<item>/<field>"`                                            |
|  [08]   | One-time code                   | `op item get <item> --vault Personal --otp`                                         |
|  [09]   | Dotenv a tool reads once        | `umask 077 && printf '<KEY>=%s\n' "$(<read>)" > <file>`                             |
|  [10]   | Rename an item to its real name | `op item edit "<old-title>" title="<official-name>" --vault Tokens`                 |

- `op` serves the SSH key to `ssh`, `git`, WezTerm, Yazi, and rclone through the 1Password agent socket, the item ref sits in `1Password/ssh/agent.toml`
- Field reads select `id`, `label`, `type`, and `purpose`, a transcript holds those and byte counts
- Read values go into a command substitution or a mode-600 file outside every repository tree, deleted when the consumer closes
- One-time codes are read in the command before the one that fills them
- A secret is read to verify presence or to run a one-off, standing local consumption uses the session cache

## [04]-[SESSION_CACHE]

`op inject` resolves `~/.config/op/env.template` into the mode-600 `~/.config/hm-op-session.sh` cache on every `forge-redeploy --switch`. Interactive shells source that cache through `forge-session-secrets.sh`, `gui-op-secrets` projects the same names into the launchd GUI domain.

- `~/.config/op/env.template` owns the local session key set, activation keeps values outside the Nix store
- Doppler delivery stays at the process boundary through `doppler run` with the owning project and config

## [05]-[STORAGE]

Local storage is `op`: every service, IaC, and MCP token and the SSH key sit in a `Tokens` or `Personal` vault item. Personal `doppler login` is the one credential Doppler keeps in the keychain, for the operator's interactive work alone.

| [INDEX] | [CLASS]                          | [STORAGE]                                     | [USE]                           |
| :-----: | :------------------------------- | :-------------------------------------------- | :------------------------------ |
|  [01]   | Config-scoped service token      | Pulumi stack secret output                    | Explicit runtime reads          |
|  [02]   | IaC admin token                  | `op://Tokens/DOPPLER_IAC_TOKEN/token`         | Topology writes through Pulumi  |
|  [03]   | Pulumi stack passphrase          | `op://Tokens/PULUMI_FORGE_SERVICES/password`  | Stack state decryption          |
|  [04]   | MCP token                        | Ambient personal CLI token as `DOPPLER_TOKEN` | Read-only agent MCP             |
|  [05]   | Provider PATs (GitHub and peers) | `op://Tokens` items, copied into configs      | Activation or process injection |

- Config-scoped service tokens are issued by topology entries
- IaC admin token and stack passphrase are brokered by `driver.ts`, an ambient `DOPPLER_TOKEN` or `PULUMI_CONFIG_PASSPHRASE` short-circuits the op read per run
- MCP provider keys `GH_PROJECTS_TOKEN`, `EXA_API_KEY`, `CONTEXT7_API_KEY`, and `GREPTILE_API_KEY` sit in `agent-runtime/dev`
- `.mcp.json` reads them as `${VAR}` headers, an agent session starts as `doppler run --project agent-runtime --config dev -- claude`
- Repository `infra/` programs read their Pulumi Cloud, Doppler, and GitHub tokens from the config their `doppler run` target names

## [06]-[RULES]

- One item, one official name: an item has the credential's real published name, a consumer needing a different env-var name renames the item at the source and repoints every reader
- Doppler projects, environments, configs, and service tokens are rows in the owning repository's `infra/` program, retiring one deletes its row
- A process resolves through the directory scope or `doppler run --project <p> --config <c>`
- Runtime secrets are written once with `doppler secrets set <NAME> --project <p> --config <c>` from stdin
- Rendered secret material is ephemeral, `--mount` and `--mount-template` over durable renders, plaintext binds where the target owner requires it
