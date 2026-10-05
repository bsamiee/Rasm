# [PATTERNS]

## [01]-[TEMPLATES]

`doppler secrets substitute <template> --project <p> --config <c>` renders Go `text/template` against the config to stdout or to `--output <file>`:
- `--use-env` takes `true` to rank environment variables under Doppler values, `override` over them, or `only` to read them alone
- `{{.KEY}}` interpolates a value, `{{if .OPTIONAL_KEY}}...{{end}}` renders a block when the config holds `OPTIONAL_KEY`
- `{{tojson .KEY}}` stringifies multiline material (private keys, certificates) into a JSON or YAML scalar
- `{{fromjson .KEY}}` expands a JSON secret value into template-addressable fields

```text
host: {{.API_HOST}}
key: {{tojson .PRIVATE_KEY}}
```

`op inject -i <template> -o <out>` renders `{{ op://<vault>/<item>/<field> }}` references, `$<VAR>` inside a reference expands to environment variable `<VAR>`.

## [02]-[MOUNTS]

`doppler run --project <p> --config <c> --mount <path> -- <cmd>` writes secrets to a file at `<path>` and injects none into the environment:
- `DOPPLER_CLI_SECRETS_PATH` names the file inside `<cmd>`
- `--format` names `env`, `json`, `dotnet-json`, `docker`, `env-no-quotes`, or `template` when the mount name's extension names none
- `--mount-template <template>` renders the template before mounting
- `--mount-max-reads <n>` caps reads of the file, `0` is unlimited
- File disappears when the Doppler process exits
