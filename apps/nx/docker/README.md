# [NX_DOCKER]

`@rasm/nx-docker` is an Nx 24 plugin for Compose projects. Projects, targets, and edges come from `docker compose config` over each Compose file's folder, and image builds run through Compose, which hands them to buildx bake.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-docker"] }
```

- The plugin takes no options: the Compose file states every fact it reads
- A machine without `docker compose` infers nothing, so the plugin loads on every host
- `COMPOSE_FILE` in the environment overrides Compose's own file discovery for every call, inference and targets alike

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                                       | [GIVES]                                                  |
| :-----: | :------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `compose.yaml`, `docker-compose.yaml`, their `.yml` and `.override` forms | One project per folder, named by Compose's project name |
|  [02]   | `docker compose --profile '*' config --format json --no-interpolate` | Services, profiles, build contexts, Dockerfiles      |
|  [03]   | `docker compose --profile '*' config --variables --format json` | Interpolated variable names, hashed as `env` inputs     |
|  [04]   | Build contexts inside another project's folder                 | Static edges to that project                             |

- A package manifest in the same folder names the project, since Nx merges its own `package.json` inference last
- Inference caches by every Compose file and by plugin source, and never caches a failure

## [03]-[TARGETS]

| [INDEX] | [TARGET]        | [COMMAND]                                          | [WHEN]                    | [CACHE] |
| :-----: | :-------------- | :------------------------------------------------- | :------------------------ | :-----: |
|  [01]   | `check--compose` | `docker compose --profile '*' config --quiet`     | Every project             |   Yes   |
|  [02]   | `check--buildx` | `docker buildx bake --call check`                  | A service with `build`    |   Yes   |
|  [03]   | `build--compose` | `docker compose build`                            | A service with `build`    |   No    |
|  [04]   | `check`, `build` | `nx:noop` over `<verb>--*`, inputs `...` and the union of theirs | A `<verb>--*` target exists | No |
|  [05]   | `install`       | `docker compose up --detach --wait --remove-orphans` | Every project           |   No    |

- `build--compose` and `install` take one configuration per profile the file declares, adding `--profile <name>`
- `nx install <project> -c <profile>` passes the profile through the `build` aggregate to `build--compose`
- `install` depends on `build` and `^install`, and `up --wait` returns once every healthcheck passes, so a dependent task starts against healthy services
- `--remove-orphans` removes containers of services the file no longer declares and keeps services of inactive profiles
- Check inputs are the Compose files, each in-tree Dockerfile, and every interpolated variable
- `bake --call check` reports warnings with exit 0 unless the Dockerfile holds `# check=error=true`, which states that policy in the checker's own form
- Images land in the engine's store, which Nx cannot restore, so `build--compose` stays uncached

## [04]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                                      |
| :-----: | :------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: Compose report decoder, graph cache, `createNodes`, `createDependencies` |
