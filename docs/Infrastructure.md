# [INFRASTRUCTURE]

## [01]-[PLACES]

Each place holding state changes through one action:

| [INDEX] | [PLACE]                              | [DECIDED_BY]                                                   | [UPDATED_BY]                |
| :-----: | :----------------------------------- | :------------------------------------------------------------- | :-------------------------- |
|  [01]   | This Mac                             | Tool versions, lock files, Nx cache folder                     | `mise install`, `mise deps` |
|  [02]   | CI runner                            | Pushed commit                                                  | `git push`                  |
|  [03]   | GitHub, Doppler, Cloudflare, tailnet | `infra/repository-stack.ts`, Doppler's sync                    | Workflow `Deploy`           |
|  [04]   | Machines                             | Table `hosts` of `infra/platform-stack.ts`, mise `[bootstrap]` | Workflow `Deploy`           |

## [02]-[MACHINES]

- Machine names state no role, provider, or location: roles sit in field `roles` and in the DNS alias of each service a role runs
- Each machine holds DNS record `<name>.<zone>`, tailnet name `<name>`, and a Pulumi stack named by its DNS name
- New machines get a Tailscale SSH login under a tag of their row by hand
- Machines losing the tailnet are reached through the provider's browser console as `root` with `<NAME>_ROOT_PASSWORD`
- Table `hosts` holds one row per machine, keyed by DNS name, stating public IPv4 `address`, `tags`, and `roles`
- `.config/mise.toml` `[bootstrap.remote.hosts]` holds one row per machine by tailnet name, and its `mise_env` loads a role file (`.config/mise.runner.toml`)
- `[bootstrap]` declares the Docker and Tailscale apt sources and packages, each settings file every machine gets, and the system service the file restarts
- `[bootstrap]` removes the hand-written Docker and Tailscale apt source files first, as apt refuses a second `Signed-By` key for one repository
- `mise bootstrap remote` sends folder `.config` and applies parts `accounts`, `packages`, `files`, and `services` as `root@<name>` over Tailscale SSH
- Role `Cache` runs the Nx cache server with its proxy and object storage
- Role `Runner` runs the GitHub Actions runner services and pulls each Unreal image
- Role `Apps` runs the proxy, sign-in, and object storage of hosted apps, on one machine per zone, as a zone holds each public name once
- Stacks declare containers, volumes, networks, and images through the Docker provider
- Docker engines build stack images from inline Dockerfiles
- Doppler `deploy` and 1Password hold `<NAME>_ROOT_PASSWORD`, `<NAME>_SSH_PRIVATE_KEY`, and `<NAME>_SSH_PUBLIC_KEY`, which no code reads

## [03]-[TAILNET]

- Job `deploy` joins as `tag:deploy` through the OAuth client stack `rasm` writes to GitHub environment `deploy`
- Machines carry `tag:server`
- Stacks reach each machine's Docker engine and shell as `root@<name>` over Tailscale SSH with no key
- Machine and app stacks apply only from a computer on the tailnet
- Tailnet policy grants every source every destination and port, SSH from members and `tag:deploy` to machine tags, and Funnel to members
- Tailnet policy is marked externally managed with a link to its stack file, and the admin console warns before an edit a deploy reverts
- Tailnet settings turn on device auto-updates, HTTPS, and MagicDNS, and turn off user approval

## [04]-[NETWORK]

- Traefik proxies publish ports 80 and 443 on every address, redirect 80 to 443, and issue ACME certificates by TLS challenge
- Routed services publish no port: the proxy reaches each on the Docker network, under one CNAME `<service>.<zone>` to its machine
- Apps on a machine get CNAME `<app>.<zone>` behind the sign-in of that machine
- Object storage publishes its port on the machine's tailnet IPv4 alone, `127.0.0.1` on a local install
- `net.ipv4.ip_nonlocal_bind` lets Docker bind the tailnet address before `tailscaled` sets it at boot
- Stacks dial object storage by tailnet name, and containers dial it inside their Docker network
- Stacks and `[bootstrap]` declare no firewall
- Containers a CI job starts publish on every address unless they name one

## [05]-[CACHE_SERVER]

- Nx cache server `nxcite/nx-cache-server` answers at HTTPS name `<name>.<zone>` of role field `name`, taking a read-only and a write token
- Runners reach the cache over public HTTPS through the proxy, and only object storage sits on the tailnet
- Cache stacks write the server URL and both tokens to Doppler `ci`, synced to repository secrets, and reference URL and read token from `dev`
- Cache objects expire 7 days after their write (role field `kept`), and a read refreshes none
- Deploys recreate a cache container only on its changed definition, and a CI run reading the cache in that moment stops

## [06]-[RUNNERS]

- Role `Runner` field `instances` names each runner service by its label
- `.config/mise.runner.toml` holds user `runner` (uid 1000, also in group `docker`), the build packages, its passwordless sudo, and the memory limits of unit `host` in a drop-in under `/usr/local/lib/systemd/system`, which `systemctl revert` keeps
- Memory drop-in changes notify `runner-prune.timer`, as mise reloads systemd only before a service change and a runner unit exists only after its stack installs it
- Runner `<machine>-<label>` holds folder `actions-runner-<label>`, its own work folder, and label `<label>` beside the default labels
- Instance `host` runs jobs on the machine and `container` runs jobs in a container, and a host job and a container job run at once
- Runner version, label, or repository changes install a runner again with a registration token `gh api` creates
- Runners keep tools, ignored files, and each Unreal image between runs
- `needrestart` leaves `actions.runner.*` services running after a library update, as GitHub documents

## [07]-[SECRETS]

- Doppler project `rasm` holds configs `dev`, `ci`, `release`, and `deploy`, and 1Password holds every value
- Config `ci` syncs to repository Actions secrets and `release` to GitHub environment `release`, each deleting names removed from Doppler
- GitHub environment `deploy` holds a read-only Doppler service token of `deploy` as `DOPPLER_TOKEN`, with the Tailscale OAuth client of job `deploy`
- Doppler `deploy` rows hold the name of the variable their provider reads, and each stack's Pulumi process receives them as its environment
- Row `DOPPLER_IAC_TOKEN` reaches that process as `DOPPLER_TOKEN`, and row `GITHUB_TOKEN` authorizes the GitHub provider and the deploy webhook
- Pulumi state sits in Pulumi Cloud for `deploy` and in local backend `file://~` for `install`

## [08]-[UPDATES]

- Machines install Ubuntu `*-updates` and Docker packages daily at 05:00 `America/Chicago`, after a random delay of up to an hour
- Machines reboot as soon as an update requires it, which stops a job running there and any CI run reading the cache it serves
- `runner-prune.timer` runs `runner-prune.service` each Sunday at 09:00 UTC to prune the pnpm store, uv cache, and unused mise tool versions
- `runner-prune.service` removes stopped Docker containers created over an hour before, unused anonymous volumes, and dangling images
- Docker's default build cache garbage collection bounds build cache, and no job prunes it
