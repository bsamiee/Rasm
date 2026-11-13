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
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- Pull requests, other branches, and cancelled CI runs start no deploy, and each deploy checks out `main`
- Target `deploy` runs target `check` of the same project first
- Deploys with unchanged inputs report `same` for every resource
- Stored commands hold no checkout path, so this Mac and a CI runner apply the same inputs
- GitHub environment `deploy` holds a read-only Doppler service token as `DOPPLER_TOKEN`, and `Deploy` reads config `deploy` through it
- Doppler `deploy` row `DOPPLER_IAC_TOKEN` authorizes the Doppler provider
- Doppler `deploy` row `GITHUB_TOKEN` authorizes the GitHub provider, `gh`, and the webhook's dispatch request
- `Deploy` uses no remote cache, so it never depends on the server it deploys
- Failed deploys fail their own `Deploy` run and leave the CI run as it ended
- `nx deploy @rasm/infra` on this Mac applies the same stacks, and no step needs it

## [03]-[MACHINES]

- Table `hosts` of `infra/platform-stack.ts` holds one row per machine, named by its Hostinger id, and Hostinger lists each machine's size and address
- Stacks dial the address Hostinger lists, and route `Address` of a row states a port alone
- Machine names are words drawn at random from the mnemonic word list, and state no role, provider, or location
- Roles sit in host row field `services`, in DNS aliases (`nx-cache.parametric-arsenal.cloud`), and in tailnet tags
- Machines hold one CNAME per Traefik `Host` rule of the services their row names, and a dependency Compose starts gets no public route
- Each machine holds DNS record `<name>.<zone>`, that hostname and reverse record at Hostinger, tailnet name `<name>`, and a stack of its DNS name
- Stacks set hostname and reverse record last, since Hostinger restarts a machine when its hostname is set, also on a changed hostname command
- Machine secrets are `<NAME>_SSH_PRIVATE_KEY`, `<NAME>_SSH_PUBLIC_KEY`, and `<NAME>_ROOT_PASSWORD`, in Doppler `deploy` and 1Password
- `HOSTINGER_API_TOKEN` covers the account and keeps its name
- Every machine gets Ubuntu 26.10, Docker, the files of table `files` in `infra/platform-stack.ts`, and a tailnet login
- Machines install Ubuntu and Docker package updates daily through `unattended-upgrades` and reboot when an update requires it, and Tailscale updates itself
- Field `services` names Compose services of `infra/compose.yaml`, and Compose starts the services they depend on
- Machines receive the definitions and variables of the services they run alone
- Machines with an empty `services` list run no container
- Apps on a machine get CNAME `<app>.<zone>`, and its row names `traefik`, `storage`, `authelia`, and each database service an app binds
- Zones hold each public service name once, so one machine per zone names `storage` or `authelia`
- Changed `services` lists stop the machine's Compose project, then start the new list
- Both machines are KVM guests without nested virtualization, so another need is a service name in a row and never a VM

New or reinstalled machines take these steps once:
1. `doppler run --config deploy -- hostinger vps virtual-machines recreate <id> --template-id <template> --password <root password>`
2. `ssh-keyscan -t ed25519 <address>` for `hostKey` of the machine's `hosts` row

## [04]-[CACHE]

- Nx hashes a task's inputs (source files, dependency files, config, lock file package versions) and replays the stored result of an equal hash
- CI setup action probes the cache server, pushes to `main` then read and write it, and pull requests read it
- This Mac uses its local cache alone: no remote cache variable is set here and Doppler `dev` holds none
- Nx 24 stops a whole run on any cache server answer it does not expect, or none, with no setting to continue without the server
- Cache variables set on this Mac would make every local `nx` command stop the same way while the cache machine is unreachable
- `lucky` runs the cache alone, so no work on `derby` reaches it
- Deploys recreate a cache container only on its changed definition, and `Deploy` starts after its CI run finishes every task
- Recreating a cache container takes about a second: a request in flight fails its run, and one sent meanwhile waits and answers
- Lock files are hash inputs: a changed lock reruns every task depending on its packages, and never needs a machine update
- Plugin source is an input of its tasks, so an edit under `apps/nx` invalidates every task of that plugin
