# [INFRASTRUCTURE]

Machines, accounts, and services the `infra/` stacks declare.

## [01]-[PLACES]

Each place holding state changes through one action:

| [INDEX] | [PLACE]                              | [DECIDED_BY]                                                   | [UPDATED_BY]                |
| :-----: | :----------------------------------- | :------------------------------------------------------------- | :-------------------------- |
|  [01]   | Owner's Mac                          | Tool versions, lock files, Nx cache folder                     | `mise install`, `mise deps` |
|  [02]   | CI runner                            | Pushed commit                                                  | `git push`                  |
|  [03]   | GitHub, Doppler, Cloudflare, tailnet | `infra/repository-stack.ts`, Doppler's sync                    | Workflow `Deploy`           |
|  [04]   | Google Cloud project `rasm-release`  | `infra/repository-stack.ts`                                    | Workflow `Deploy`           |
|  [05]   | Machines                             | Table `hosts` of `infra/platform-stack.ts`, mise `[bootstrap]` | Workflow `Deploy`           |

## [02]-[MACHINES]

Machines, their reinstall, and images their stacks run:
- Table `hosts` keys machines by DNS name, names state no role, provider, or location, and each service a role runs takes a DNS alias
- Each machine holds DNS record `<name>.<zone>`, tailnet name `<name>`, and a Pulumi stack named by its DNS name
- Stack `rasm` holds a Hostinger post-install script joining reinstalled machines to the tailnet with Tailscale SSH under key `machines` and its tags
- `set-hostname` restores a machine's hostname after its tailnet join and resets it with no shutdown, and post-install scripts flush writes first
- `hostinger vps virtual-machines recreate <id> --template-id <id> --post-install-script-id <id>` reinstalls a machine and erases its hostname
- Reinstalls follow `tscli delete device`, as a machine joining beside its old device takes a numbered name, and `mise bootstrap remote` follows
- Reinstalls of a storage machine follow `node stacks.ts deploy --operation=destroy --stack=<name>`, as its provider cannot refresh a lost store
- `[bootstrap]` lengthens the Hostinger image's `99lock-timeout`, as a reinstalled machine's runner install meets its first unattended upgrade
- Reach machines off the tailnet through their provider's browser console as `root` with `<NAME>_ROOT_PASSWORD`
- `[bootstrap.remote]` loads `linux`, and a host row with a role names `linux` and its role file, as `mise bootstrap remote` sends no `.miserc.toml`
- `mise bootstrap remote` sends folder `.config` and applies its parts as `root@<name>` over Tailscale SSH
- Role `Cache` runs the Nx cache server with its proxy and object storage
- Role `Runner` runs a runner service per instance and pulls an Unreal image per engine release a tracked `.uproject` names
- Role `Runner` builds and pushes `ghcr.io/<owner>/<repository>/playwright:<version>`, locked Playwright Chromium over the newest `node:slim`
- Playwright images add `curl`, a C compiler, and ICU, as action `tools` downloads mise, builds cargo tools, and runs `dotnet` inside them
- Pushed images hold their source label as a manifest annotation, as GHCR links a package by annotation alone and a linked package inherits access
- Role `Apps` runs the proxy, sign-in, and object storage of hosted apps, on one machine per zone, as a zone holds each public name once
- Docker engines build stack images from inline Dockerfiles with build checks as errors, load local images, and push registry images
- Pushed images hold their registry credentials, as a changed `registries` setting replaces the build provider and images ignore a changed password
- Builds run in a BuildKit container the build provider creates per client
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
- Routed services publish no port, and proxies reach each on its Docker network under one CNAME `<service>.<zone>` to its machine
- Apps on a machine get CNAME `<app>.<zone>`, and its sign-in service requires a session on each app's sign-in path alone
- Proxies strip `Remote-User` and `Remote-Email` from every app request and set both from its sign-in session, and apps read both through `ACCESS`
- Object storage publishes its port on its machine's tailnet IPv4 alone, `127.0.0.1` on a local install
- `net.ipv4.ip_nonlocal_bind` lets Docker bind a tailnet address before `tailscaled` sets it at boot
- Stacks dial object storage by tailnet name, and containers dial it inside their Docker network
- Stacks and `[bootstrap]` declare no firewall
- Containers a CI job starts without an address publish on every address

## [05]-[CACHE_SERVER]

Nx cache server of role `Cache`:
- Nx cache server `nxcite/nx-cache-server` answers at HTTPS name `<name>.<zone>` of role field `name` and takes a read-only and a write token
- Runners reach the server over public HTTPS through its proxy
- Cache stacks write server URL and both tokens to Doppler `ci`
- Cache stacks write object storage root keys and references to URL and read token to Doppler `dev`
- Cache objects expire role field `kept` after their write, and a read refreshes none
- Deploys recreate a cache container on a changed definition alone, runs starting meanwhile use their local cache, and runs past their first read stop

## [06]-[RUNNERS]

GitHub Actions runners of role `Runner`:
- Role `Runner` field `instances` names each runner service by label with systemd limits `systemctl set-property` sets once it starts
- Runner `<machine>-<label>` holds folder `actions-runner-<label>`, its own work folder, and label `<label>` beside default labels
- Instance `host` runs jobs on its machine and `container` in a container, one job of each at once
- Changes to runner version, label, limits, repository, or tailnet node reinstall a runner with a `gh api` token after `systemctl revert`
- Runners keep tools, ignored files, and each Unreal image between runs
- `needrestart` leaves `actions.runner.*` services running after a library update

## [07]-[SECRETS]

Doppler configs and their readers:
- Config `ci` syncs to repository Actions secrets and `release` to GitHub environment `release`, each deleting names removed from Doppler
- GitHub environment `deploy` holds a read-only Doppler service token of `deploy` as `DOPPLER_TOKEN`
- Doppler `deploy` rows take the variable name their provider reads, and each stack's Pulumi process receives them as its environment
- Doppler `deploy` and 1Password hold `<NAME>_ROOT_PASSWORD`, `<NAME>_SSH_PRIVATE_KEY`, and `<NAME>_SSH_PUBLIC_KEY`, and no code reads them
- Stacks write no `deploy` row, as a change to `deploy` starts workflow `Deploy`
- Row `DOPPLER_IAC_TOKEN` reaches stacks as `DOPPLER_TOKEN`, and row `GITHUB_TOKEN` authorizes GitHub's provider and deploy webhook
- Pulumi state sits in Pulumi Cloud for `deploy` and in local backend `file://~` for `install`

## [08]-[GOOGLE_CLOUD]

Google Cloud project `rasm-release`, its identities, and GitHub variables naming them:
- Stack `rasm` declares project APIs, workload identity pool `github`, and provider `github` trusting OIDC tokens of the repository ID alone
- Service accounts `chrome-web-store` and `deploy` take tokens from environments `release` and `deploy` through `roles/iam.workloadIdentityUser`
- Stack `rasm` writes `GOOGLE_WORKLOAD_IDENTITY_PROVIDER` and `GOOGLE_SERVICE_ACCOUNT` per environment, `CHROME_WEB_STORE_PUBLISHER` to `release`
- Role changes of service account `deploy` apply from the owner's Mac
- Local deploys read application default credentials `gcloud auth application-default login` writes
- Owners with token creator role on `chrome-web-store` add `--impersonate-service-account` and `--scopes` to `gcloud auth print-access-token`
- Chrome Web Store dashboard Settings list `chrome-web-store` under Service account, with access to every publisher item
- Service accounts hold no key, and Chrome Web Store API answers 403 alike for an item its publisher lacks and a refused identity

## [09]-[UPDATES]

Package updates and reboots of each machine:
- Machines install Ubuntu `*-updates` and Docker packages daily on a timer `.config/mise.linux.toml` sets
- Machines reboot when an update requires it, and each reboot stops jobs there and CI runs reading its cache
- Docker Engine's default garbage collection bounds the build cache CI builds fill
