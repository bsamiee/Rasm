# [PLUMBING]

## [01]-[PLACES]

Nx Core runs alone: `nx.json` sets `neverConnectToCloud: true`, every plugin is ours, and the remote cache is a server on our own machine.

Places holding state, each updated by one action:

| [INDEX] | [PLACE]                  | [DECIDED_BY]                                | [UPDATED_BY]                                |
| :-----: | :----------------------- | :------------------------------------------ | :------------------------------------------ |
|  [01]   | This Mac                 | Tool versions, lock files, local Nx cache   | `mise install`, `pnpm install`, `mise deps` |
|  [02]   | CI runner                | Pushed commit                               | `git push`                                  |
|  [03]   | GitHub, Doppler, tailnet | `infra/repository-stack.ts`, Doppler's sync | Workflow `Deploy`                           |
|  [04]   | Machines                 | Table `hosts` of `infra/platform-stack.ts`  | Workflow `Deploy`                           |

## [02]-[DELIVERY]

- Workflow `Deploy` applies every stack through `nx deploy @rasm/infra`
- `Deploy` starts on `workflow_run` once a CI run on `main` completes, passed or failed, so a failing test never blocks an infrastructure fix
- `Deploy` applies only when that CI run tested the tip of `main`, so no deploy restarts a service under the CI run of a later push
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- Pull requests, other branches, and cancelled CI runs start no deploy, and each deploy checks out `main`
- Target `deploy` runs target `check` of the same project first
- Deploys with unchanged inputs report `same` for every resource
- Stored commands hold no checkout path, so this Mac and a CI runner apply the same inputs
- GitHub environment `deploy` holds a read-only Doppler service token as `DOPPLER_TOKEN`, and `Deploy` reads config `deploy` through it
- Doppler `deploy` rows hold the name of the variable their provider reads, and each stack's Pulumi process receives them as its environment
- Row `DOPPLER_IAC_TOKEN` reaches that process as `DOPPLER_TOKEN` and authorizes the Doppler provider
- Row `GITHUB_TOKEN` authorizes the GitHub provider and the webhook's dispatch request
- `Deploy` uses no remote cache, so it never depends on the server it deploys
- Failed deploys fail their own `Deploy` run and leave the CI run as it ended
- `nx deploy @rasm/infra` on this Mac applies the same stacks, and no step needs it
- Machine and app stacks dial a Docker engine by tailnet name, so they apply only from a computer on the tailnet
- Job `deploy` runs on a GitHub-hosted runner outside the tailnet

## [03]-[MACHINES]

- Table `hosts` of `infra/platform-stack.ts` holds one row per machine, keyed by its DNS name, stating `address`, `tags`, and `role`
- Machines are rented at Hostinger, and no code names it
- Machine names are words drawn at random from the mnemonic word list, and state no role, provider, or location
- Roles sit in host row field `role`, in DNS aliases (`nx-cache.parametric-arsenal.cloud`), and in tailnet tags
- Each machine holds DNS record `<name>.<zone>`, tailnet name `<name>`, and a stack of its DNS name
- Stacks reach a machine's Docker engine and shell as `root@<name>` over Tailscale SSH, with no key
- New machines get Docker and a tailnet login with Tailscale SSH under a tag of their row by hand, and no stack installs either
- Machine secrets are `<NAME>_SSH_PRIVATE_KEY`, `<NAME>_SSH_PUBLIC_KEY`, and `<NAME>_ROOT_PASSWORD`, in Doppler `deploy` and 1Password
- `HOSTINGER_API_TOKEN` covers the account and keeps its name
- Machines install Ubuntu and Docker package updates daily through `unattended-upgrades`, and Tailscale updates itself
- Machines reboot at 10:30 UTC on a day an update requires it, and a CI run reading the cache at that minute stops
- Role `Cache` runs the Nx cache server with its proxy and object storage, and `Runner` the GitHub Actions runner service and each Unreal image
- Role `Apps` runs the proxy, sign-in, and object storage of hosted apps
- Stacks declare containers, volumes, networks, and images through the Docker provider, and an engine builds its images from inline Dockerfiles
- Machines hold one CNAME `<service>.<zone>` per routed service, and a container with no route gets no public name
- Apps on a machine get CNAME `<app>.<zone>` behind the sign-in of that machine, which holds role `Apps`
- Zones hold each public service name once, so one machine per zone holds role `Apps`
- Both machines are KVM guests without nested virtualization, so another need is a container of a role and never a VM

## [04]-[CACHE]

- Nx hashes a task's inputs (source files, dependency files, config, lock file package versions) and replays the stored result of an equal hash
- Pushes to `main` read and write the cache server, and pull requests read it
- Plugin row `@rasm/nx-devkit` of `nx.json` sends the server one read before the tasks of a run that sets `NX_SELF_HOSTED_REMOTE_CACHE_SERVER`
- Runs getting an answer other than 200 or 404, or none within 5 seconds, print one warning (an annotation under GitHub Actions) and use the local cache alone
- Workflow `Release` uses no remote cache, so its run builds every file it attaches or publishes
- This Mac uses its local cache alone: no remote cache variable is set here and Doppler `dev` holds none
- Nx 24 stops a run on a failed cache read and on a failed write of a batched task (cargo, dotnet), which is the failure a server gives after its first answer
- `--skip-remote-cache` runs without the server, and no Nx setting uses the server and continues on its error
- Failed writes of any other task fail that task and each task depending on it, after 6 attempts over 1 to 20 seconds
- `lucky` runs the cache alone, so no work on `derby` reaches it
- Deploys recreate a cache container only on its changed definition, after the CI run of the newest push finishes every task
- Recreating a cache container takes about a second: a request in flight fails its run, and one sent meanwhile waits and answers
- Lock files are hash inputs: a changed lock reruns every task depending on its packages, and never needs a machine update
- Plugin source is no task input: an edit under `apps/nx` changes a task's hash only where it changes the target's inferred configuration
- Runs after an executor edit replay results the old executor produced, so its tasks run once with `--skip-nx-cache`
