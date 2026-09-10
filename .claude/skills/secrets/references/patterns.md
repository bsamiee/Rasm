# [PATTERNS]

Templates and mounts for secret material a process reads from a file.

## [01]-[TEMPLATES]

`doppler secrets substitute <template> --project <p> --config <c>` renders Go `text/template` against the config to stdout:
- `--output <file>` writes the render to a file where the target owner requires one
- `--use-env true` adds environment variables to the template values
- `{{.KEY}}` interpolates a value, `{{if .OPTIONAL_KEY}}...{{end}}` renders a block when the config holds the key
- `{{tojson .KEY}}` stringifies multiline material (private keys, certificates) into a JSON or YAML scalar
- `{{fromjson .KEY}}` expands a JSON secret value into template-addressable fields

```text
host: {{.API_HOST}}
key: {{tojson .PRIVATE_KEY}}
```

`op inject` renders `{{ op://<vault>/<item>/<field> }}` references in a template, `$<VAR>` inside a reference takes that environment variable's value.

## [02]-[MOUNTS]

`doppler run --project <p> --config <c> --mount <path> -- <cmd>` mounts secrets as a named pipe at `<path>` and injects none into the environment:
- `DOPPLER_CLI_SECRETS_PATH` names the pipe inside the command
- `--format` names `env`, `json`, `dotnet-json`, `docker`, `env-no-quotes`, or `template` when the mount name's extension names none
- `--mount-template <template>` renders the template before mounting
- `--mount-max-reads <n>` caps reads of the pipe, `0` is unlimited
- Pipe disappears when the Doppler process exits
- Mount under an iCloud Drive directory fails on macOS
