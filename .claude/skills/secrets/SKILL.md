---
name: secrets
description: "Use when a process needs a runtime secret or a terminal needs a credential, covering Doppler scopes, injection, templates, and 1Password reads."
---

# [SECRETS]

Doppler owns runtime secrets, `doppler run` around a command injects them under one project and config. 1Password owns a person's credentials and the tokens an agent reads in a local terminal through `op` under `sudo`, the desktop app's biometric unlock authorizes each read. `op` appears in no repository code, infra program, hook plugin, or agent profile.

[REFERENCES]:
- [01]-[PATTERNS](references/patterns.md): Templates and mounts for secret material a process reads from a file

## [01]-[RESOLUTION]

Doppler reads each option from the highest source present, a flag, then an environment variable, then the config file scope:
- Environment variables: `DOPPLER_TOKEN`, `DOPPLER_PROJECT`, `DOPPLER_CONFIG`, `DOPPLER_CONFIG_DIR`, `DOPPLER_PASSPHRASE`
- Config file sits under `~/.doppler`, scope entries key on a directory and subdirectories inherit the nearest one
- `doppler login` writes a CLI token at scope `/`, macOS and Windows hold it in the OS keychain
- `doppler setup` writes project and config at the working directory
- Service tokens grant one config read access, `--access read/write` adds writes and `--max-age` expires one
- Service token's project and config outrank flags, `DOPPLER_TOKEN` or `--token` carries one
- Commands name `--project` and `--config`
- `env -u DOPPLER_TOKEN` runs one command against the directory scope

## [02]-[DOPPLER_CLI]

| [INDEX] | [TASK]                       | [COMMAND]                                                                                      |
| :-----: | :--------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | Auth proof                   | `doppler me`                                                                                   |
|  [02]   | Effective options per scope  | `doppler configure debug --json \| jq 'with_entries(.value \|= del(.token))'`                  |
|  [03]   | Every scope entry            | `doppler configure --all --json \| jq 'with_entries(.value \|= del(.token))'`                  |
|  [04]   | One directory's scope        | `doppler configure get project config --scope <dir> --json`                                    |
|  [05]   | Set a directory scope        | `doppler configure set project=<p> config=<c> --scope <dir>`                                   |
|  [06]   | Unset a directory scope      | `doppler configure unset project config --scope <dir>`                                         |
|  [07]   | Key inventory                | `doppler secrets --only-names --json --project <p> --config <c> \| jq 'keys'`                  |
|  [08]   | One value                    | `doppler secrets get <NAME> --plain --project <p> --config <c>`                                |
|  [09]   | Write a value from stdin     | `<producer> \| doppler secrets set <NAME> --project <p> --config <c>`                          |
|  [10]   | Inject env into a process    | `doppler run --project <p> --config <c> -- <cmd>`                                              |
|  [11]   | Shell operators in a command | `doppler run --project <p> --config <c> --command '<cmd> && <cmd>'`                            |
|  [12]   | Ephemeral service token      | `doppler configs tokens create <name> --project <p> --config <c> --max-age <duration> --plain` |
|  [13]   | Revoke a service token       | `doppler configs tokens revoke <token> --project <p> --config <c>`                             |

- `configure` JSON prints `project` and `config` as `enclave.project` and `enclave.config`, `del(.token)` strips the token from a printed scope

## [03]-[OP_CLI]

Secret references take the form `op://<vault>/<item>/[<section>/]<field>`:

| [INDEX] | [TASK]                      | [COMMAND]                                                                     |
| :-----: | :-------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | Auth proof                  | `op whoami`                                                                   |
|  [02]   | Vault inventory             | `op vault list`                                                               |
|  [03]   | Item names in a vault       | `op item list --vault <vault> --format json \| jq -r '.[].title'`             |
|  [04]   | Field names of an item      | `op item get <item> --vault <vault> --format json \| jq -r '.fields[].label'` |
|  [05]   | One field                   | `op read "op://<vault>/<item>/<field>"`                                       |
|  [06]   | One-time code               | `op item get <item> --vault <vault> --otp`                                    |
|  [07]   | Private key in OpenSSH form | `op read "op://<vault>/<item>/private key?ssh-format=openssh"`                |
|  [08]   | Field to a mode-600 file    | `op read --out-file <file> "op://<vault>/<item>/<field>"`                     |
|  [09]   | Env vars for one process    | `<VAR>="op://<vault>/<item>/<field>" op run -- <cmd>`                         |
|  [10]   | Rendered template           | `op inject -i <template> -o <out>`                                            |
|  [11]   | Rename an item              | `op item edit "<title>" --title "<new-title>" --vault <vault>`                |

- `OP_ACCOUNT` or `--account` selects the account when the app holds more than one
- `op run` masks secret values on stdout and stderr
- Commands that expand a variable holding a reference run in a subshell (`sh -c '<cmd>'`), `op run` resolves the reference first

## [04]-[RULES]

- Items carry the credential's published name as title, a consumer needing another env-var name renames the item and repoints every reader
- Secret values reach a consumer as injected environment, a command substitution, a mount, or a mode-600 file
- Files holding secret values sit outside every repository tree and go when the consumer exits
- Transcripts and logs hold key names and counts
- Use `manage-repo` for Doppler projects, configs, and service tokens as infra rows
