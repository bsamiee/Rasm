# [NX_DOCKER]

`@rasm/nx-docker` is an Nx 24 plugin for Compose projects. Projects, targets, and edges come from `docker compose config` over each Compose file's folder, and image builds and their checks run through Compose, which hands them to buildx bake.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-docker"] }
```

- Options: none, since the Compose file states every fact the plugin reads
- Compose files `docker compose config` cannot read (no `docker` or `compose` on the host, a YAML or schema error) keep `check--compose` and `install`
- Such a file prints one warning, its runs fail with Compose's own message, and no Compose file fails the graph
- `COMPOSE_FILE` in the environment overrides Compose's own file discovery for every call, inference and targets alike

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                                                  | [GIVES]                                                    |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | `compose.yaml`, `docker-compose.yaml`, their `.yml` and `.override` forms | One project per folder, named by Compose's project name    |
|  [02]   | `docker compose config --format json --no-interpolate`, every profile     | Services, profiles, env files, build contexts, Dockerfiles |
|  [03]   | `docker compose config --variables --format json`, every profile          | Interpolated variable names, hashed as `env` inputs        |
|  [04]   | Local build contexts inside another project's declared outputs or folder  | Static edges to that project                               |

- Package manifests in the same folder name the project, since Nx merges its own `package.json` inference last
- Compose resolves every local path to an absolute one
- Contexts Compose leaves relative (a Git or HTTP URL, `docker-image://`, `service:`) draw no edge and add no input
- Contexts inside a target's declared `outputs` draw their edge to that target's project, never to the folder's owner
- Other local contexts draw their edge to the project owning their folder, since a root project holds every `.artifacts` path
- Inference runs both `config` calls (about 40 ms each, concurrently per folder) on every graph computation
- Compose names no file it reads through `include` or `extends`, and Nx recomputes the graph when any such file changes

## [03]-[TARGETS]

| [INDEX] | [TARGET]         | [COMMAND]                                                        | [WHEN]                 | [CACHE] |
| :-----: | :--------------- | :--------------------------------------------------------------- | :--------------------- | :-----: |
|  [01]   | `check--compose` | `docker compose config --quiet`, every profile                   | Every project          |   Yes   |
|  [02]   | `check--buildx`  | `docker compose build --check`, every profile                    | Services with `build`  |   Yes   |
|  [03]   | `build--compose` | `docker compose build`                                           | Services with `build`  |   No    |
|  [04]   | `check`, `build` | `nx:noop` over `<verb>--*`, inputs `...` and the union of theirs | Any `<verb>--*` target |   No    |
|  [05]   | `install`        | `docker compose up --detach --wait --remove-orphans`             | Every project          |   No    |

- Every profile is `COMPOSE_PROFILES=*` in the target's environment; `build--compose` and `install` take one configuration per declared profile, setting `COMPOSE_PROFILES` to it
- `nx install <project> -c <profile>` passes the profile through the `build` aggregate to `build--compose`
- `install` depends on `build` and `^install`, and `up --wait` returns once every healthcheck passes, so a dependent task starts against healthy services
- `--remove-orphans` removes containers of services the file no longer declares and keeps services of inactive profiles
- Check inputs are the Compose files, the project's `.env`, every env file and Dockerfile inside the workspace, and every interpolated variable
- Compose's resolved model is a `runtime` input (`config --format json --no-interpolate --no-path-resolution`), holding what `include` and `extends` files contribute
- `check--buildx` and `build--compose` depend on every other `build--*` target of the project and on `^build`
- Bake refuses a build context that does not exist yet, and an image copies what those builds write
- `build --check` runs BuildKit's lint over exactly what `compose build` builds and fails on any warning, so no Dockerfile needs `# check=error=true`
- Images land in the engine's store, which Nx cannot restore, so `build--compose` stays uncached

## [04]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                                    |
| :-----: | :------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: Compose report decoder, `createNodes`, `createDependencies` |
