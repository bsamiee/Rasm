# [INFRASTRUCTURE]

Machines, accounts, and services the `infra/` stacks declare.

## [01]-[PLACES]

Each place holding state changes through one action:

| [INDEX] | [PLACE]                              | [DECIDED_BY]                                                   | [UPDATED_BY]      |
| :-----: | :----------------------------------- | :------------------------------------------------------------- | :---------------- |
|  [01]   | Owner's Mac                          | Tool versions, lock files, Nx cache folder                     | `mise install`    |
|  [02]   | CI runner                            | Pushed commit                                                  | `git push`        |
|  [03]   | GitHub, Doppler, Cloudflare, tailnet | `infra/repository-stack.ts`, Doppler's sync                    | Workflow `Deploy` |
|  [04]   | Google Cloud project `rasm-release`  | `infra/repository-stack.ts`                                    | Workflow `Deploy` |
|  [05]   | Machines                             | Table `hosts` of `infra/platform-stack.ts`, mise `[bootstrap]` | Workflow `Deploy` |

## [02]-[MACHINES]

Machines, their reinstall, and images their stacks run:
- Table `hosts` keys machines by DNS name, names state no role, provider, or location, and each service a role runs takes a DNS alias
- Each machine holds DNS record `<name>.<zone>`, tailnet name `<name>`, and a Pulumi stack named by its DNS name
- Stack `rasm` holds a Hostinger post-install script joining reinstalled machines to the tailnet with Tailscale SSH under key `machines` and its tags
- `set-hostname` restores a machine's hostname after its tailnet join and resets it with no shutdown, and post-install scripts flush writes first
- `hostinger vps virtual-machines recreate <id> --template-id <id> --post-install-script-id <id>` reinstalls a machine and erases its hostname
- Reinstalls follow `tscli delete device`, as a machine joining beside its old device takes a numbered name, and `mise bootstrap remote` follows
- Reinstalls of a storage machine follow `node stacks.ts deploy --operation=destroy --stack=<name>`, as its provider cannot refresh a lost store
- Storage volumes lost unplanned leave their MinIO service account in stack state, which `pulumi state delete <urn>` retires before a deploy
- Locally built images lost from their machine stay in stack state until a deploy replaces their `docker-build:Image` resource
- Host rows leave table `hosts` after `node stacks.ts deploy --operation=destroy --stack=<name>`, as a removed row leaves its stack unvisited
- `[bootstrap]` lengthens the Hostinger image's `99lock-timeout`, as a reinstalled machine's runner install meets its first unattended upgrade
- `[bootstrap]` writes the Hostinger image's `/etc/fstab` whole, as a file entry edits no line of a file mise does not own
- Root filesystems mount with `nodiscard` and `fstrim.timer` trims weekly, as online discard after mass deletes held the disk busy for hours
- Reach machines off the tailnet through their provider's browser console as `root` with `<NAME>_ROOT_PASSWORD`
- `[bootstrap.remote]` loads `machine`, and a host row with a role names `machine` and its role file, as `mise bootstrap remote` sends no miserc
- `mise bootstrap remote` sends folder `.config` and applies its parts as `root@<name>` over Tailscale SSH
- Role `Cache` runs the Nx cache server with its proxy and object storage
- Role `History` runs the Atuin sync server with its proxy
- Role `Runner` runs a runner service per instance and pulls and tags an Unreal image per engine release a tracked `.uproject` names
- Role `Runner` builds and pushes `ghcr.io/<owner>/<repository>/playwright:<version>`, locked Playwright Chromium over `node:slim`
- Playwright images rebuild on a changed Playwright version or Dockerfile alone, and read and write layer cache at tag `cache`
- Playwright images add `curl`, a C compiler, and ICU, as action `tools` downloads mise, builds cargo tools, and runs `dotnet` inside them
- Pushed images hold their source label as a manifest annotation, as GHCR links a package by annotation alone and a linked package inherits access
- Role `Apps` runs the proxy, sign-in, and object storage of hosted apps, on one machine per zone, as a zone holds each public name once
- Docker engines build stack images from inline Dockerfiles with build checks as errors, load local images, and push registry images
- Pushed images hold their registry credentials, as a changed `registries` setting replaces the build provider and images ignore a changed password
- Previews build no image
- Builds run in a BuildKit container the build provider creates per client, which job `deploy` removes with its build cache
- Pulled images take name `<tag>@<digest>` from registry digests each deploy reads, and a moved tag replaces its image, then its containers
- Docker removes no image name a container uses, its provider pulls no name a machine holds, and stacks name no image by tag alone

## [03]-[TAILNET]

Tailnet members, tags, and policy:
- Job `deploy` joins as `tag:deploy` through the OAuth client stack `rasm` writes to GitHub environment `deploy`, and machines hold `tag:server`
- Stacks reach each machine's Docker engine and shell as `root@<name>` over Tailscale SSH with no key
- Machine and app stacks apply only from a computer on the tailnet
- Tailnet policy grants every source every destination and port, SSH from members and `tag:deploy` to machine tags, and Funnel to members
- Tailnet policy is marked externally managed with a link to its stack file, and its admin console warns before an edit a deploy reverts
- Tailnet settings enable device auto-updates, HTTPS, and MagicDNS, and disable user approval

## [04]-[NETWORK]

Routes from public names to containers:
- Traefik proxies publish ports 80 and 443 on every address, redirect 80 to 443, and issue ACME certificates by TLS challenge
- Proxies mount folder `/var/run` to read the Docker socket, as Docker package updates replace its socket file under running containers
- Routed services publish no port, and proxies reach each on its Docker network under one CNAME `<service>.<zone>` to its machine's A record
- Apps on a machine get CNAME `<app>.<zone>`, and its sign-in service requires a session on each app's sign-in path alone
- Proxies strip `Remote-User` and `Remote-Email` from every app request and set both from its sign-in session, and apps read both through `ACCESS`
- Object storage publishes its port on its machine's tailnet IPv4 alone, `127.0.0.1` on a local install
- Object storage takes CNAME `storage.<zone>` on machines with role `Apps` alone, as apps hand its public origin to browsers
- `net.ipv4.ip_nonlocal_bind` lets Docker bind a tailnet address before `tailscaled` sets it at boot
- Docker maps published ports through firewall rules with no userland proxy, as `docker-proxy` accepts connections for a container with no listener
- Stacks dial object storage by tailnet name, and containers dial it inside their Docker network
- Stacks and `[bootstrap]` declare no firewall
- Containers a CI job starts without an address publish on every address

## [05]-[CACHE_SERVER]

Nx cache server of role `Cache`:
- Nx cache server `nxcite/nx-cache-server` answers at HTTPS name `<name>.<zone>` of role field `name` and takes one token that reads and writes
- Runners reach the server over public HTTPS through its proxy
- Cache stacks write server URL and token to Doppler `ci` once the server runs healthy
- Cache stacks write object storage root keys and references to URL and token to Doppler `dev`
- Cache objects expire role field `kept` after their write, and a read refreshes none
- Cache buckets hold no quota, as RustFS refuses writes past a quota and Nx fails a run once a cache write fails six times
- Deploys recreate a cache container on a changed definition alone, runs starting meanwhile use their local cache, and runs past their first read stop

## [06]-[HISTORY_SERVER]

Atuin sync server of role `History`:
- Atuin server `atuinsh/atuin` answers at HTTPS name `<name>.<zone>` of role field `name`, as clients off the tailnet sync through it
- Servers hold records in SQLite on protected volume `atuin` apart from route field `name`, as one account with one writer needs no database server
- Servers sync the encrypted record store whole (history, threads, kv, scripts, dotfiles), and Atuin Hub alone serves packfiles
- Clients skip packfile records for a server advertising no packfile capability, as loose records cover them
- Registration stays closed, and each container start creates or resets the repository owner's account from the stack's password
- History stacks write `ATUIN_SYNC_ADDRESS` and `ATUIN_PASSWORD` to Doppler `dev` once the server runs healthy, the variables the Atuin client reads
- Clients log in with `doppler run -- atuin login -u <owner> -k "$(atuin key)"`, keeping their encryption key
- Clients hold every record of their account, and a rebuilt server refills from the next client to log in again and sync
- Clients signed in before a rebuild run `atuin logout` first, as `atuin login` with a saved session skips authentication

## [07]-[RUNNERS]

GitHub Actions runners of role `Runner`:
- Role `Runner` field `instances` names each runner service by label with systemd limits `systemctl set-property` sets once it starts
- Systemd limits of instance `container` reach no job, as its jobs run in containers under Docker's cgroups
- Runner `<machine>-<label>` installs in folder `actions-runner-<label>`, works in folder `actions-work-<label>`, and holds label `<label>` beside default labels
- Instance `host` runs jobs on its machine and `container` in a container, one job of each at once
- Changes to runner version, label, limits, repository, or tailnet node reinstall a runner after `systemctl revert`
- Reinstalls replace folder `actions-runner-<label>` alone, so checkouts, tools, and caches of its work folder stay, and a removed runner deletes both
- Runner installs register through `config.sh --pat`, which takes a registration token from `GITHUB_TOKEN` at each attempt
- Runner removals run `config.sh remove --pat` with the token of their last install, and a revoked token leaves an offline registration
- Runners keep tools, ignored files, and each Unreal image between runs, as jobs name an image by tag and `docker image prune` removes untagged ones
- `needrestart` leaves `actions.runner.*` services running after a library update

## [08]-[SECRETS]

Doppler configs and their readers:
- Config `ci` syncs to repository Actions secrets and `release` to GitHub environment `release`, each deleting names removed from Doppler
- GitHub environment `deploy` holds a read-only Doppler service token of `deploy` as `DOPPLER_TOKEN`
- Doppler `deploy` rows take the variable name their provider reads, and each stack's Pulumi process receives them as its environment
- Doppler `deploy` and 1Password hold `<NAME>_ROOT_PASSWORD`, `<NAME>_SSH_PRIVATE_KEY`, and `<NAME>_SSH_PUBLIC_KEY`, and no code reads them
- Stacks write no `deploy` row, as a change to `deploy` starts workflow `Deploy`
- Row `DOPPLER_IAC_TOKEN` reaches stacks as `DOPPLER_TOKEN`, and row `GITHUB_TOKEN` authorizes GitHub's provider, deploy webhook, and runners
- Pulumi state sits in Pulumi Cloud for `deploy` and in local backend `file://~` for `install`
- Operation `up` alone creates a missing stack, as `preview` and `destroy` select an existing one
- Operation `destroy` walks stacks in reverse order over stack state alone, refreshing nothing and running no program, as a lost machine answers neither
- Stack `rasm` declares `Rasm` and each adopted repository with its Doppler project, environments, and branch configs
- Adopted repositories naming existing resources take option `import` for one deploy, as a kept import ID turns a rename into a replacement

## [09]-[GOOGLE_CLOUD]

Google Cloud project `rasm-release`, its identities, and GitHub variables naming them:
- Stack `rasm` declares project APIs, workload identity pool `github`, and provider `github` trusting OIDC tokens of the repository ID alone
- Service accounts `chrome-web-store` and `deploy` take tokens from environments `release` and `deploy` through `roles/iam.workloadIdentityUser`
- Stack `rasm` writes `GOOGLE_WORKLOAD_IDENTITY_PROVIDER` and `GOOGLE_SERVICE_ACCOUNT` per environment, `CHROME_WEB_STORE_PUBLISHER` to `release`
- Role changes of service account `deploy` apply from the owner's Mac
- Local deploys read application default credentials `gcloud auth application-default login` writes
- Owners with token creator role on `chrome-web-store` add `--impersonate-service-account` and `--scopes` to `gcloud auth print-access-token`
- Chrome Web Store dashboard Settings list `chrome-web-store` under Service account, with access to every publisher item
- Service accounts hold no key, and Chrome Web Store API answers 403 alike for an item its publisher lacks and a refused identity

## [10]-[UPDATES]

Package updates and reboots of each machine:
- Machines install Ubuntu `*-updates` and Docker packages daily on a timer `.config/mise.machine.toml` sets
- Machines reboot when an update requires it, and each reboot stops jobs there and CI runs reading its cache
- Docker Engine's builder garbage collection keeps the build cache CI builds fill under `defaultMaxUsedSpace`
