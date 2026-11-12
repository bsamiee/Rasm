# [PLUMBING]

## [01]-[WHAT_WE_RUN]

We use Nx Core only. `nx.json` has `neverConnectToCloud: true`, all 15 plugins are our own, and the remote cache is a server we host on the VPS. So everything Nx Cloud would own, we own.

There are four places, and each one only changes when a specific command runs:

| [INDEX] | [PLACE]                            | [STATE_DECIDED_BY]                                 | [UPDATED_BY]                                            |
| :-----: | :--------------------------------- | :------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | This Mac                           | Tool versions, lock files, local Nx cache (7.5 GB) | `mise install`, `pnpm install`, `mise deps`             |
|  [02]   | CI runner                          | Pushed commit                                      | `git push`                                              |
|  [03]   | GitHub settings and secrets        | Repository stack, plus Doppler's sync              | `nx deploy @rasm/infra --stack=rasm`                    |
|  [04]   | VPS services (including the cache) | `infra/compose.yaml`                               | `nx deploy @rasm/infra --stack=srv2138272.hstgr.cloud`  |

"Drift" is the repo changing while one of those commands was not run, or was cut off halfway.

## [02]-[CACHE]

- What it is: before a task runs, Nx hashes its inputs (source files, dependency files, config, package versions from the lock files). Same hash means the stored result is replayed.
- Locally: only the local cache is used. No remote cache variable is set on this Mac and Doppler `dev` holds none.
- In CI: the setup action probes the VPS cache server. If it answers, pushes to `main` read and write it and pull requests read it.
- The weak point: with a self-hosted server, Nx fails the whole run if an upload gets an unexpected status. A VPS deploy recreates the cache container, so a deploy during a CI run breaks that run. That is what failed the last Linux run.
- Lock files: they are hash inputs. When Renovate or an install changes one, every task depending on those packages reruns. This never requires a VPS update.
- Why CI is slow right now: each plugin's own source is an input to its tasks, so every edit under `apps/nx` invalidates all tasks of that plugin. Run 43215992967 showed 10 of 848 cache hits. Once the plumbing stops changing, hits rise.
