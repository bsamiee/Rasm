# [NX_DOCKER]

`@rasm/nx-docker` is an Nx 24 plugin for Compose projects. Projects, targets, and edges come from `docker compose config` over each Compose file's folder, and image builds and their checks run through Compose, which hands them to buildx bake.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-docker"] }
```

- The plugin takes no options: the Compose file states every fact it reads
- A machine without `docker compose` infers nothing, so the plugin loads on every host
- `COMPOSE_FILE` in the environment overrides Compose's own file discovery for every call, inference and targets alike

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                                                  | [GIVES]                                                    |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | `compose.yaml`, `docker-compose.yaml`, their `.yml` and `.override` forms | One project per folder, named by Compose's project name   |
|  [02]   | `docker compose config --format json --no-interpolate`, every profile     | Services, profiles, env files, build contexts, Dockerfiles |
|  [03]   | `docker compose config --variables --format json`, every profile          | Interpolated variable names, hashed as `env` inputs        |
|  [04]   | Local build contexts inside another project's folder                      | Static edges to that project                               |

- A package manifest in the same folder names the project, since Nx merges its own `package.json` inference last
- Compose resolves every local path to an absolute one, so a context it leaves relative (a Git or HTTP URL, `docker-image://`, `service:`) draws no edge and adds no input
- Inference caches by every Compose file and by plugin source, and never caches a failure

## [03]-[TARGETS]

| [INDEX] | [TARGET]         | [COMMAND]                                                        | [WHEN]                      | [CACHE] |
| :-----: | :--------------- | :--------------------------------------------------------------- | :-------------------------- | :-----: |
|  [01]   | `check--compose` | `docker compose config --quiet`, every profile                   | Every project               |   Yes   |
|  [02]   | `check--buildx`  | `docker compose build --check`, every profile                    | A service with `build`      |   Yes   |
|  [03]   | `build--compose` | `docker compose build`                                           | A service with `build`      |   No    |
|  [04]   | `check`, `build` | `nx:noop` over `<verb>--*`, inputs `...` and the union of theirs | A `<verb>--*` target exists |   No    |
|  [05]   | `install`        | `docker compose up --detach --wait --remove-orphans`             | Every project               |   No    |

- Every profile is `COMPOSE_PROFILES=*` in the target's environment; `build--compose` and `install` take one configuration per declared profile, setting `COMPOSE_PROFILES` to it
- `nx install <project> -c <profile>` passes the profile through the `build` aggregate to `build--compose`
- `install` depends on `build` and `^install`, and `up --wait` returns once every healthcheck passes, so a dependent task starts against healthy services
- `--remove-orphans` removes containers of services the file no longer declares and keeps services of inactive profiles
- Check inputs are the Compose files, the project's `.env`, every env file and Dockerfile inside the workspace, and every interpolated variable
- `build --check` runs BuildKit's lint over exactly what `compose build` builds and fails on any warning, so no Dockerfile needs `# check=error=true`
- Images land in the engine's store, which Nx cannot restore, so `build--compose` stays uncached

## [04]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                                                 |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: Compose report decoder, graph cache, `createNodes`, `createDependencies` |
