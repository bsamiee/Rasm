---
name: secrets
description: "Use when a program needs a runtime secret or an agent needs a credential, covering Doppler scopes, doppler run, infra rows, 1Password sign-in, op reads and writes, SSH, and commit signing."
---

# [SECRETS]

1Password and Doppler each hold every secret at its current value. Code and repository files name Doppler alone. 1Password serves the owner and local agents through `op` and the desktop app. Use `README.md` for how a running program reads a Doppler secret.

## [01]-[DOPPLER_SCOPES]

Doppler reads each option from a flag, then a `DOPPLER_*` environment variable, then the directory scope in `~/.doppler/.doppler.yaml`:
- Repository directory scope holds a CLI token alone, every command names `--project` and `--config`
- Scope JSON names project and config `enclave.project` and `enclave.config`

| [INDEX] | [TASK]                   | [COMMAND]                                                                     |
| :-----: | :----------------------- | :---------------------------------------------------------------------------- |
|  [01]   | Signed-in identity       | `doppler me --json`                                                           |
|  [02]   | Options for working dir  | `doppler configure debug --json \| jq 'with_entries(.value \|= del(.token))'` |
|  [03]   | Every directory scope    | `doppler configure --all --json \| jq 'with_entries(.value \|= del(.token))'` |
|  [04]   | Projects                 | `doppler projects --json`                                                     |
|  [05]   | Configs of a project     | `doppler configs --project <p> --json`                                        |
|  [06]   | Key inventory            | `doppler secrets --only-names --json --project <p> --config <c> \| jq 'keys'` |
|  [07]   | Secrets in a process env | `doppler run --project <p> --config <c> -- <cmd>`                             |
|  [08]   | Shell operators          | `doppler run --project <p> --config <c> --command '<cmd> && <cmd>'`           |

## [02]-[INFRA_ROWS]

Doppler resources exist alone as typed `infra/cli.ts` rows of `Project`, `Environment`, `BranchConfig`, `Secret`, and `ServiceToken` from `@pulumiverse/doppler`:
- Workplace `Parametric_Arsenal` holds project `rasm`, environments `dev` and `prd` with locked root configs, and branch config `dev_repo` under `dev`
- `Secret` rows name `project`, `config`, `name`, and `value`, `@pulumiverse/doppler` stores `value` as a Pulumi secret
- `Secret` import ids take the form `<project>.<config>.<name>`
- Doppler adds `DOPPLER_PROJECT`, `DOPPLER_CONFIG`, and `DOPPLER_ENVIRONMENT` to every config
- Target `rasm:infra` runs `doppler run --project rasm --config dev_repo -- node infra/cli.ts`
- `infra/cli.ts` reads each `Secret` value from the environment `dev_repo` injects, a branch config inheriting every `dev` secret
- `dev_repo` supplies `DOPPLER_TOKEN` to `@pulumiverse/doppler`, `PULUMI_ACCESS_TOKEN` to Pulumi, and `GITHUB_TOKEN` to `@pulumi/github`, each its own `Secret` row
- `nx run rasm:infra:up` applies rows, `nx run rasm:infra:refresh` reads live state into the stack
- Use `manage-repo` for infra rows

## [03]-[OP_SIGNIN]

Desktop app integration authorizes `op` for account `my.1password.com`:
1. `open -a 1Password` starts the app, a closed app fails `op` with `couldn't connect to the 1Password desktop app`
2. `op vault list` raises the Touch ID prompt and lists vaults `Personal` and `Tokens`

- Authorization covers one terminal session and its subshells, expires after 10 idle minutes or 12 hours, and ends when the app locks
- Agent shells hold no tty, `op whoami` there prints `account is not signed in` while reads succeed

## [04]-[OP_READS]

Secret references take the form `op://<vault>/<item>/[<section>/]<field>[?<query>]`:

| [INDEX] | [TASK]                      | [COMMAND]                                                                     |
| :-----: | :-------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | Item titles in a vault      | `op item list --vault <vault> --format json \| jq -r '.[].title'`             |
|  [02]   | Field labels of an item     | `op item get <item> --vault <vault> --format json \| jq -r '.fields[].label'` |
|  [03]   | One value                   | `op read "op://<vault>/<item>/<field>"`                                       |
|  [04]   | One-time code               | `op read "op://<vault>/<item>/one-time password?attribute=otp"`               |
|  [05]   | Private key in OpenSSH form | `op read "op://<vault>/<item>/private key?ssh-format=openssh"`                |
|  [06]   | Value to a mode-600 file    | `op read --out-file <file> "op://<vault>/<item>/<field>"`                     |
|  [07]   | Env vars for one process    | `<VAR>="op://<vault>/<item>/<field>" op run -- <cmd>`                         |

- `Tokens` items hold their value in field `token` or `credential`
- `op run` masks values on stdout and stderr, `--no-masking` prints them
- Commands that expand a variable holding a reference run in a subshell (`sh -c '<cmd>'`), `op run` resolves the reference first

## [05]-[OP_WRITES]

Value writes pass through stdin JSON, `--dry-run` previews a create or an edit:
- `doppler secrets get --plain` and `op read` end the value with a newline, `rtrimstr("\n")` drops it
- `op item edit` writes an empty `DATE` field from stdin JSON as `0`, edits delete empty dates first

```bash
# New Tokens item
<producer> | jq -Rs '{title: "<NAME>", category: "API_CREDENTIAL", fields: [{id: "credential", label: "token", type: "CONCEALED", value: rtrimstr("\n")}]}' \
    | op item create --vault Tokens -

# New item from a live Doppler secret
doppler secrets get <NAME> --json --project <p> --config <c> \
    | jq 'to_entries[0] | {title: .key, category: "API_CREDENTIAL", fields: [{id: "credential", label: "token", type: "CONCEALED", value: .value.computed}]}' \
    | op item create --vault Tokens -

# New value for an existing Tokens item
op item get <NAME> --vault Tokens --format json \
    | jq --rawfile v <(<producer>) 'del(.fields[] | select(.type == "DATE" and .value == null)) | (.fields[] | select(.label == "token")).value = ($v | rtrimstr("\n"))' \
    | op item edit <NAME> --vault Tokens

# Vendor login with a generated password
op item create --category login --title <vendor> --vault Personal --url <url> username=<email> --generate-password

# Rename, every reader repoints to the new name
op item edit <NAME> --vault Tokens --title <NEW_NAME>
```

## [06]-[SSH_SIGNING]

Desktop app's SSH agent serves key `Forge SSH Key` (ED25519) to SSH hosts and Git commit and tag signing:
- `~/.ssh/config` sets `IdentityAgent "~/Library/Group Containers/2BUA8C4S2C.com.1password/t/agent.sock"` for every host
- Git sets `gpg.format=ssh`, `user.signingkey` to the public key, and `gpg.ssh.program=/Applications/1Password.app/Contents/MacOS/op-ssh-sign`
- Each application's first request raises an approval prompt, approval holds until the app locks

| [INDEX] | [TASK]                | [COMMAND]                                                                                         |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Keys the agent serves | `SSH_AUTH_SOCK="$HOME/Library/Group Containers/2BUA8C4S2C.com.1password/t/agent.sock" ssh-add -l` |
|  [02]   | GitHub authentication | `ssh -T git@github.com`                                                                           |
|  [03]   | Commit signature      | `git log -1 --show-signature`                                                                     |

## [07]-[RULES]

- Each secret keeps one name as `Tokens` item title, Doppler secret, and environment variable a consumer reads
- Configuration files hold no secret value
- `.mcp.json` headers read each value from the harness environment as `${<NAME>}`
- Values reach a consumer as injected environment, a command substitution, a mount, or a mode-600 file outside every repository tree
- Files holding values go when the consumer exits
- Agent output holds secret names alone

## [08]-[FILE_MATERIAL]

Processes reading secret material from a file take a rendered template or a Doppler mount:

| [INDEX] | [TASK]                     | [COMMAND]                                                                          |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | Doppler template render    | `doppler secrets substitute <template> --project <p> --config <c> --output <file>` |
|  [02]   | 1Password template render  | `op inject -i <template> -o <file>`                                                |
|  [03]   | Secrets as a file, not env | `doppler run --project <p> --config <c> --mount <path> -- <cmd>`                   |

- `doppler secrets substitute` renders Go `text/template` against the config, to stdout without `--output`
- `{{.KEY}}` interpolates a value, `{{if .OPTIONAL_KEY}}...{{end}}` renders a block when the config holds `OPTIONAL_KEY`
- `{{tojson .KEY}}` stringifies multiline material (private keys, certificates) into a JSON or YAML scalar
- `{{fromjson .KEY}}` expands a JSON secret value into template-addressable fields
- `--use-env` takes `true` to rank environment variables under Doppler values, `override` over them, or `only` to read them alone
- `op inject` renders `{{ op://<vault>/<item>/<field> }}` references, `$<VAR>` inside a reference expands to environment variable `<VAR>`
- `--mount` injects nothing into the environment, `DOPPLER_CLI_SECRETS_PATH` names the file inside `<cmd>`
- `--format` names `env`, `json`, `dotnet-json`, `docker`, `env-no-quotes`, or `template` when the mount name's extension names none
- `--mount-template <template>` renders the template before mounting, `--mount-max-reads <n>` caps reads of the file, `0` is unlimited
- Mounted file disappears when the Doppler process exits

```text
host: {{.API_HOST}}
key: {{tojson .PRIVATE_KEY}}
```
