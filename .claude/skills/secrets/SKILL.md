---
name: secrets
description: >-
    Owns secret storage in 1Password and Doppler: op is the permanent local store and
    SSH-key holder, Doppler the runtime backend. Use when creating a secret, token, or env key
    or when one fails to resolve, creating and managing them, the op agent key, a tool or code logic
    needs a scoped token, or a config file needs secret material. Issuing tokens and creating projects,
    configs, or scopes is Pulumi topology in Parametric_Forge/services/topology.ts, the pulumi skill.
---

# [SECRETS]

`op` holds permanent local and session storage, and Doppler holds project configuration and explicit process delivery.

Topology (projects, environments, configs, service tokens, directory scopes) sits as IaC entries in `Parametric_Forge/services/topology.ts`, rendered by `repo.ts` and applied by `driver.ts` over the Pulumi Automation API. `doppler` reads and writes secret values against declared configs, `doppler run` and owner-specific downloads inject values at the consuming process, `~/.doppler` holds CLI scope and authentication state.

[REFERENCES]:
- [01]-[PATTERNS](references/patterns.md): Consumption patterns for secret material that is not process-env shaped, with the plan gates

## [01]-[RESOLUTION]

- `~/.doppler` is the CLI config dir, scopes sit in `~/.doppler/.doppler.yaml`, written by `doppler configure set` through the driver's `scopes apply`
- Repo-local `doppler.yaml` is vendor setup guidance, and the repo has none
- Precedence, highest first: a service token's embedded project/config, runtime flags, env vars, config-file scope
- Config-file scope resolves an exact directory match before the nearest ancestor
- Scope env vars: `DOPPLER_TOKEN`, `DOPPLER_PROJECT`, `DOPPLER_CONFIG`, `DOPPLER_CONFIG_DIR`, `DOPPLER_PASSPHRASE`
- Agents pass `--project`/`--config` explicitly, env holds only the token
- Ambient `DOPPLER_TOKEN` outranks flags and represents one config, strip it with `env -u DOPPLER_TOKEN` when fetching more than one source

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

- One directory's scope keys are written as `enclave.project`, `enclave.config`, the set is driver-owned and scope `/` stays untouched
- Secret downloads pipe to `jq 'keys'` or `jq 'length'`, configure reads strip the root token with `del(.token)`, an unfiltered `configure debug` or `configure --all` prints it
- Transcripts and logs hold key names and counts

## [03]-[OP_CLI]

`op` reads the local store directly, the field suffix is `token`, `credential`, or `password` per item. Exported `OP_SERVICE_ACCOUNT_TOKEN` pins `op` to the `Tokens` vault, `Personal` (the SSH key) resolves only under `env -u OP_SERVICE_ACCOUNT_TOKEN`.

| [INDEX] | [TASK]                          | [COMMAND]                                                                      |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------- |
|  [01]   | Auth proof                      | `op whoami`                                                                    |
|  [02]   | Vault inventory                 | `op vault list`                                                                |
|  [03]   | Tokens item names               | `op item list --vault Tokens --format json \| jq -r '.[].title'`               |
|  [04]   | Read one secret                 | `op read "op://Tokens/<ITEM>/<token\|credential\|password>"`                   |
|  [05]   | Resolve the rebuild template    | `op inject -i ~/.config/op/env.template -o <out>`                              |
|  [06]   | Personal-vault SSH item         | `env -u OP_SERVICE_ACCOUNT_TOKEN op item get "Forge SSH Key" --vault Personal` |
|  [07]   | Rename an item to its real name | `op item edit "<old-title>" title="<official-name>" --vault Tokens`            |

- `op` serves the SSH key to `ssh`, `git`, WezTerm, Yazi, and rclone through the 1Password agent socket, the item ref sits in `1Password/ssh/agent.toml`, with no private key on disk
- Read a secret only to verify presence or wire a one-off, standing local consumption uses the activation-generated session cache

## [04]-[SESSION_CACHE]

`op inject` resolves `~/.config/op/env.template` into the mode-600 `~/.config/hm-op-session.sh` cache on every `forge-redeploy --switch`. Interactive shells source that cache through `forge-session-secrets.sh`, `gui-op-secrets` projects the same names into the launchd GUI domain for newly spawned applications.

- `~/.config/op/env.template` owns the local session key set, activation keeps values outside the Nix store
- Doppler delivery stays at the process boundary through `doppler run` or an owner-specific `doppler secrets download`, with the owning project and config

## [05]-[STORAGE]

Local storage is `op`: every service, IaC, and MCP token and the SSH key sit in a `Tokens` or `Personal` vault item. Personal `doppler login` is the only credential Doppler keeps in the keychain, used for the operator's ad-hoc interactive work alone, no pipeline depends on it.

| [INDEX] | [CLASS]                          | [STORAGE]                                     | [USE]                           |
| :-----: | :------------------------------- | :-------------------------------------------- | :------------------------------ |
|  [01]   | Config-scoped service token      | Pulumi stack secret output                    | Explicit runtime reads          |
|  [02]   | IaC admin token                  | `op://Tokens/DOPPLER_IAC_TOKEN/token`         | Topology writes through Pulumi  |
|  [03]   | Pulumi stack passphrase          | `op://Tokens/PULUMI_FORGE_SERVICES/password`  | Stack state decryption          |
|  [04]   | MCP token                        | Ambient personal CLI token as `DOPPLER_TOKEN` | Read-only agent MCP             |
|  [05]   | Provider PATs (GitHub and peers) | `op://Tokens` items, copied into configs      | Activation or process injection |

- Config-scoped service token: issued by topology entries, static Developer-plan tokens are revoked and reissued
- IaC admin token and stack passphrase: brokered by `driver.ts`, an ambient `DOPPLER_TOKEN` or `PULUMI_CONFIG_PASSPHRASE` short-circuits the op read per run
- MCP token: the launcher prelude resolves the ambient personal CLI token, its grants are the enforcement, `--read-only` filters the toolset to GET endpoints

## [06]-[RULES]

- One item, one official name: an item has the credential's real published name, a consumer needing a different env-var name renames the item at the source and repoints every reader, and naming mistakes are fixed by renaming in `op` and Doppler
- New projects are added as project/config entries in `Parametric_Forge/services/topology.ts` and a directory scope entry, then `pulumi up`, retiring one deletes its entries
- Repos hold no Doppler files, their agents resolve through scope and hook automatically
- Rendered secret material is ephemeral: `--mount`/`--mount-template` over durable renders, plaintext binds only where the target owner requires it
