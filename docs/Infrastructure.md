# [INFRASTRUCTURE]

Machines, accounts, and services the `infra/` stacks declare.

## [01]-[PRINCIPLES]

Rules every stack and machine follows:
- Stacks apply from commits `main` holds, as each `Deploy` trigger checks out a commit of `main`, and a local apply serves a trial alone
- Secrets are made once and read by the stacks passing them to services, and an apply rotates no secret
- Stacks write no Doppler row, as a written row reaches `deploy` through its references and starts `Deploy` again
- Jobs and machines join services through identity federation or a credential with no expiry, and no credential waits on a renewal by hand

## [02]-[PLACES]

Each place holding state changes through one action:

| [INDEX] | [PLACE]                              | [DECIDED_BY]                                                   | [UPDATED_BY]      |
| :-----: | :----------------------------------- | :------------------------------------------------------------- | :---------------- |
|  [01]   | Owner's Mac                          | Tool versions, lock files                                      | `mise install`    |
|  [02]   | GitHub-hosted runner                 | Pushed commit                                                  | `git push`        |
|  [03]   | GitHub, Doppler, Cloudflare, tailnet | `infra/repository-stack.ts`, Doppler's sync                    | Workflow `Deploy` |
|  [04]   | Google Cloud project `rasm-release`  | `infra/repository-stack.ts`                                    | Workflow `Deploy` |
|  [05]   | Machines                             | Table `hosts` of `infra/platform-stack.ts`, mise `[bootstrap]` | Workflow `Deploy` |

## [03]-[SECRETS]

Doppler configs and their readers:
- Config `ci` syncs to repository Actions secrets and `release` to GitHub environment `release`, each deleting names removed from Doppler
- GitHub environment `deploy` holds read-only Doppler service token `DOPPLER_TOKEN` of `deploy`, set by hand and held in row `DOPPLER_DEPLOY_TOKEN`
- Doppler `deploy` rows take the variable name their provider reads, and each stack's Pulumi process receives them as its environment
- Deploys read Doppler `deploy`, and `nx install @rasm/infra` reads sign-in secrets and object storage keys from Doppler `dev`
- Rows one config holds and another reads are references there (`${ci.<NAME>}`), which change with their source
- Row `AUTHELIA_OWNER_PASSWORD_HASH` holds the bcrypt hash of row `AUTHELIA_OWNER_PASSWORD`, the form the sign-in service reads
- Doppler `deploy` and 1Password hold `<NAME>_ROOT_PASSWORD`, `<NAME>_SSH_PRIVATE_KEY`, and `<NAME>_SSH_PUBLIC_KEY`, and no code reads them
- Row `DOPPLER_IAC_TOKEN` reaches stacks as `DOPPLER_TOKEN`, and row `GITHUB_TOKEN` authorizes GitHub's provider, deploy webhook, and runners

## [04]-[STACKS]

Pulumi state and operations of each stack:
- Pulumi state sits in Pulumi Cloud for `deploy` and in local backend `file://~` for `install`
- Operation `up` alone creates a missing stack, as `preview` and `destroy` select an existing one
- Operation `destroy` walks stacks in reverse order over stack state alone, refreshing nothing and running no program, as a lost machine answers neither
- Stack `rasm` declares `Rasm` and each adopted repository with its Doppler project, environments, and branch configs
- Adopted repositories naming existing resources take option `import` for one deploy, as a kept import ID turns a rename into a replacement

## [05]-[MACHINES]

Machines, their roles, and their reinstall:
- Table `hosts` keys machines by DNS name, names state no role, provider, or location, and each service a role runs takes a DNS alias
- Each machine holds DNS record `<name>.<zone>`, tailnet name `<name>`, and a Pulumi stack named by its DNS name
- `[bootstrap.remote]` loads `machine`, and a host row with a role file names `machine` and that file, as `mise bootstrap remote` sends no miserc
- `mise bootstrap remote` sends folder `.config` and applies its parts as `root@<name>` over Tailscale SSH
- Remote bootstrap rows sit in `.config/mise.remote.toml` and Sparkle settings in `.config/sparkle.xcconfig`, files `deploy--pulumi` hashes whole
- Environment `remote` loads the remote rows, and job `deploy` adds it for its bootstrap step, as `mise -E remote bootstrap remote` does by hand
- Config files mise loads beside `mise.toml` outside an environment take the lock rows of tools a version file names, and locked installs of Rust then fail
- Deploy starts for those files and no other mise or Xcode setting, as named input `deployed` names what `deploy--pulumi` reads
- Role `Cache` runs the Nx cache server with its proxy and object storage
- Role `History` runs the Atuin sync server with its proxy
- Role `Runner` runs a runner service per instance and pulls and tags an Unreal image per engine release a tracked `.uproject` names
- Role `Runner` builds and pushes `ghcr.io/<owner>/<repository>/playwright:<version>`, locked Playwright Chromium over `node:slim`
- Role `Apps` runs the proxy, sign-in, and object storage of hosted apps, on one machine per zone, as a zone holds each public name once
- Hostinger VPS use under 60% of its vCPUs earns CPU credits and use above spends them
- Hostinger limits a machine with no credits to 30% of its vCPUs until a fifth of its balance returns
- Hostinger shows no credit balance, and steal in `/proc/stat` counts CPU time the hypervisor gives other machines, never a credit limit
- `[bootstrap]` lengthens the Hostinger image's `99lock-timeout`, as a reinstalled machine's runner install meets its first unattended upgrade
- `[bootstrap]` writes the Hostinger image's `/etc/fstab` whole, as a file entry edits no line of a file mise does not own
- Root filesystems mount with `nodiscard`, as online discard held the disk busy for hours
- `fstrim.timer` stays masked, as Hostinger's guest agent runs `fstrim` over every fstab filesystem daily through `guest-exec`, which `journalctl -t qemu-ga` records
- `[bootstrap]` removes the Hostinger image's `journald.conf`, as its guest agent writes 7-day retention and daily files there at install
- Machines keep line logs in the journal alone under `SystemMaxUse`, and `[bootstrap]` masks rsyslog and its dmesg copy, each a copy of journal entries
- Service containers log to the journal under their name across replacements, and containers a job starts keep the daemon's `local` driver, removed with them
- `[bootstrap]` keeps root's user manager running, as one started for each SSH session wrote half of each journal
- `systemd-coredump` holds crash dumps under its own size limits, as Docker's units allow core files of any size
- Stack `rasm` holds a Hostinger post-install script joining reinstalled machines to the tailnet with Tailscale SSH
- `set-hostname` restores a machine's hostname after its tailnet join and resets it with no shutdown, and post-install scripts flush writes first
- `hostinger vps virtual-machines recreate <id> --template-id <id> --post-install-script-id <id>` reinstalls a machine and erases its hostname
- Reinstalls follow `tscli delete device`, as a machine joining beside its old device takes a numbered name, and `mise bootstrap remote` follows
- Reinstalls of a storage machine follow `node stacks.ts deploy --operation=destroy --stack=<name>`, as its provider cannot refresh a lost store
- Storage volumes lost unplanned leave their MinIO service account in stack state, which `pulumi state delete <urn>` retires before a deploy
- Host rows leave table `hosts` after `node stacks.ts deploy --operation=destroy --stack=<name>`, as a removed row leaves its stack unvisited
- Reach machines off the tailnet through their provider's browser console as `root` with `<NAME>_ROOT_PASSWORD`

## [06]-[IMAGES]

Images stacks build, push, and pull:
- Docker engines build stack images from inline Dockerfiles with build checks as errors, load local images, and push registry images
- Builds run in a BuildKit container the build provider creates per client, and previews build no image
- Playwright images rebuild on a changed Playwright version or Dockerfile alone, and read and write layer cache at tag `cache`
- Playwright images add `curl`, a C compiler, and ICU, as action `tools` downloads mise, builds cargo tools, and runs `dotnet` inside them
- Playwright images give user `node` home `/github/home`, the `HOME` Actions sets in job containers, as rustup reports a home differing from its user's
- Pushed images hold their source label as a manifest annotation, as GHCR links a package by annotation alone and a linked package inherits access
- Pushed images hold their registry credentials, as a changed `registries` setting replaces the build provider and images ignore a changed password
- Locally built images lost from their machine stay in stack state until a deploy replaces their `docker-build:Image` resource
- Pulled images take name `<tag>@<digest>` from registry digests each deploy reads, and a moved tag replaces its image, then its containers
- Built images take each `FROM` base as `<tag>@<digest>` from the registry digest each deploy reads, so a moved base rebuilds its image
- Role `Runner` pulls the Playwright image it built and the test container images `@rasm/testing/containers` names, each tagged as tests name it
- Machine Docker providers sign in to Docker Hub with rows `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN`, as anonymous digest reads and pulls share one hourly allowance per address
- Docker removes no image name a container uses, its provider pulls no name a machine holds, and stacks pull no image by tag alone

## [07]-[TAILNET]

Tailnet members, tags, and policy:
- Job `deploy` joins as `tag:deploy` through a Tailscale federated identity trusting GitHub's token of environment `deploy`
- Stack `rasm` writes the federated identity's ID and audience to environment `deploy` as variables, and GitHub holds no Tailscale secret
- New machines join under their `hosts` tags through OAuth client secret row `TAILSCALE_ENROLMENT_OAUTH_SECRET`, which creates one key per machine
- Stacks reach each machine's Docker engine and shell as `root@<name>` over Tailscale SSH with no key
- Machine and app stacks apply only from a computer on the tailnet
- Tailnet policy grants every source every destination and port, SSH from members and `tag:deploy` to machine tags, and Funnel to members
- Tailnet policy is marked externally managed with a link to its stack file, and its admin console warns before an edit a deploy reverts
- Tailnet settings enable device auto-updates, HTTPS, and MagicDNS, and disable user approval

## [08]-[NETWORK]

Routes from public names to containers:
- Traefik proxies publish ports 80 and 443 on every address, redirect 80 to 443, and issue ACME certificates by TLS challenge
- Proxies mount folder `/var/run` to read the Docker socket, as Docker package updates replace its socket file under running containers
- Proxies reach routed services on their Docker network, each under CNAME `<service>.<zone>` to its machine's A record
- Apps on a machine get CNAME `<app>.<zone>`, and its sign-in service requires a session on each app's sign-in path alone
- Proxies strip `Remote-User` and `Remote-Email` from every app request and set both from its sign-in session, and apps read both through `ACCESS`
- Object storage publishes its port on its machine's tailnet IPv4 alone, `127.0.0.1` on a local install
- Object storage takes CNAME `storage.<zone>` on machines with role `Apps` alone, as apps hand its public origin to browsers
- `net.ipv4.ip_nonlocal_bind` lets Docker bind a tailnet address before `tailscaled` sets it at boot
- Docker maps published ports through firewall rules with no userland proxy, as `docker-proxy` accepts connections for a container with no listener
- Stacks dial object storage by tailnet name, and containers dial it inside their Docker network
- Stacks and `[bootstrap]` declare no firewall
- Containers a CI job starts without an address publish on every address

## [09]-[CACHE_SERVER]

Nx cache server of role `Cache`:
- Nx cache server `nxcite/nx-cache-server` answers at HTTPS name `<name>.<zone>` of role field `name` and takes a read-write and a read-only token
- Runners reach the server over public HTTPS through its proxy
- Doppler `ci` holds server URL and both tokens, `dev` references URL and read-write token, and `deploy` both tokens
- Cache objects expire role field `kept` after their write with no refresh on read, and an unchanged task runs again once per `kept`
- Incomplete cache uploads abort role field `abort` after they start, as no upload outlasts a job
- Cache buckets hold no quota, as RustFS refuses writes past one and a refused write fails its task
- Deploys recreate a cache container on a changed definition alone, runs starting meanwhile use their local cache, and runs past their first read stop

## [10]-[HISTORY_SERVER]

Atuin sync server of role `History`:
- Atuin server `atuinsh/atuin` answers at HTTPS name `<name>.<zone>` of role field `name`, as clients off the tailnet sync through it
- Servers hold records in SQLite on protected volume `atuin` apart from route field `name`, as one account with one writer needs no database server
- Servers sync the encrypted record store whole (history, threads, kv, scripts, dotfiles), and Atuin Hub alone serves packfiles
- Clients skip packfile records for a server advertising no packfile capability, as loose records cover them
- Registration stays closed, and each container start creates or resets the repository owner's account from Doppler row `ATUIN_PASSWORD`
- Doppler `dev` holds `ATUIN_SYNC_ADDRESS` and `ATUIN_PASSWORD`, the variables the Atuin client reads, and `deploy` references the password
- Clients log in with `doppler run -- atuin login -u <owner> -k "$(atuin key)"`, keeping their encryption key
- Clients hold every record of their account, and a rebuilt server refills from the next client to log in again and sync
- Clients signed in before a rebuild run `atuin logout` first, as `atuin login` with a saved session skips authentication

## [11]-[RUNNERS]

GitHub Actions runners of role `Runner`:
- Role `Runner` field `instances` names each runner service by label with systemd limits its unit's drop-in `instance` sets
- Systemd limits of instance `container` reach no job, as its jobs run in containers under Docker's cgroups
- Runner `<machine>-<label>` installs in folder `actions-runner-<label>`, works in folder `actions-work-<label>`, and holds label `<label>` beside default labels
- Instances `host` and `graph` run jobs on their machine and `container` in a container, one job of each at once
- Runner services stop by removing their label through GitHub's API and waiting until GitHub reports them idle, as a stop cancels a running job
- Label removal comes first, as a listener takes the next queued job within seconds of its last one
- Stops GitHub refuses wait for their instance's `Runner.Worker` to exit, and a job GitHub assigns meanwhile is canceled
- Runner services restore their label as they start, retry a failed start every 30 seconds, and stop before Docker
- Runner self-updates wait for a running job and stay on
- Drains wait up to `drained` of the `Runner` arm, the longest self-hosted job timeout, and a runner command's Pulumi timeout adds 15 minutes
- Runner installs keep `GITHUB_TOKEN` in root-only file `.token` of their folder, which the drain reads
- Changes to runner version, limits, repository, or tailnet node stop a runner, then `systemctl revert` deletes its drop-ins and it reinstalls
- Reinstalls replace folder `actions-runner-<label>` alone and keep the work folder's checkouts, tools, and caches, which only a label leaving `instances` deletes
- Runner installs register through `config.sh --pat`, which takes a registration token from `GITHUB_TOKEN` at each attempt
- Runner removals run `config.sh remove --pat` with the token of their last install, and a revoked token leaves an offline registration
- Runners keep tools, ignored files, and each Unreal image between runs, as jobs name an image by tag and `docker image prune` removes untagged ones
- `needrestart` leaves `actions.runner.*` services running after a library update

## [12]-[GOOGLE_CLOUD]

Google Cloud project `rasm-release`, its identities, and GitHub variables naming them:
- Stack `rasm` declares project APIs, workload identity pool `github`, and provider `github` trusting OIDC tokens of the repository ID alone
- Service accounts `chrome-web-store` and `deploy` take tokens from environments `release` and `deploy` through `roles/iam.workloadIdentityUser`
- Stack `rasm` writes `GOOGLE_WORKLOAD_IDENTITY_PROVIDER` and `GOOGLE_SERVICE_ACCOUNT` per environment, `CHROME_WEB_STORE_PUBLISHER` to `release`
- Role changes of service account `deploy` apply from the owner's Mac
- Local deploys read application default credentials `gcloud auth application-default login` writes
- Owners with token creator role on `chrome-web-store` add `--impersonate-service-account` and `--scopes` to `gcloud auth print-access-token`
- Chrome Web Store dashboard Settings list `chrome-web-store` under Service account, with access to every publisher item
- Service accounts hold no key, and Chrome Web Store API answers 403 alike for an item its publisher lacks and a refused identity

## [13]-[UPDATES]

Package updates and reboots of each machine:
- Machines install Ubuntu `*-updates` and Docker packages daily on a timer `.config/mise.machine.toml` sets
- Machines reboot once an update requires it, except a machine with role `Runner`, as a reboot stops its jobs and runs reading its cache
- Role `Runner` turns off `Automatic-Reboot` in `.config/mise.runner.toml`, and its timer `reboot-when-idle` checks hourly
- `reboot-when-idle` reboots while `/run/reboot-required` exists and the repository's 100 newest runs hold none without a conclusion
- `reboot-when-idle` reads the unfiltered run list, as a run filtered by status appears seconds after its creation
- Reboots stop runner services through their drain
- Unattended upgrades keep changed conffiles (`--force-confold`), as the Hostinger image changes `cloud.cfg` and a changed or deleted conffile holds its package
- Reinstalls restore `docker-compose-plugin` and `docker-ce-rootless-extras` as recommendations of Docker's packages, and no code runs either
- Docker Engine bounds its build cache by its disk-relative defaults, as deploy builds run on BuildKit builders each deploy removes
