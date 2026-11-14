# [OPERATIONS]

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
- Deploys with unchanged inputs report `same` for every resource
- GitHub environment `deploy` holds a read-only Doppler service token as `DOPPLER_TOKEN`, and `Deploy` reads config `deploy` through it
- Doppler `deploy` rows hold the name of the variable their provider reads, and each stack's Pulumi process receives them as its environment
- Row `DOPPLER_IAC_TOKEN` reaches that process as `DOPPLER_TOKEN` and authorizes the Doppler provider
- Row `GITHUB_TOKEN` authorizes the GitHub provider and the webhook's dispatch request
- `Deploy` uses no remote cache, so it never depends on the server it deploys
- Failed deploys fail their own `Deploy` run and leave the CI run as it ended
- `nx deploy @rasm/infra` on this Mac applies the same stacks, and no step needs it
- Machine and app stacks dial a Docker engine by tailnet name, so they apply only from a computer on the tailnet
- Job `deploy` runs on a GitHub-hosted runner that joins the tailnet as `tag:deploy` through `tailscale/github-action`
- Stack `rasm` writes that tag's OAuth client and the machine names to GitHub environment `deploy`, and its tailnet policy grants the tag SSH

## [03]-[MACHINES]

- Table `hosts` of `infra/platform-stack.ts` holds one row per machine, keyed by its DNS name, stating `address`, `tags`, and `roles`
- Machines are rented at Hostinger, and no code names it
- Machine names are words drawn at random from the mnemonic word list, and state no role, provider, or location
- Roles sit in host row field `roles`, in DNS aliases (`nx-cache.parametric-arsenal.cloud`), and in tailnet tags
- Each machine holds DNS record `<name>.<zone>`, tailnet name `<name>`, and a stack of its DNS name
- Stacks reach a machine's Docker engine and shell as `root@<name>` over Tailscale SSH, with no key
- New machines get Docker and a tailnet login with Tailscale SSH under a tag of their row by hand, and no stack installs either
- Machine secrets are `<NAME>_SSH_PRIVATE_KEY`, `<NAME>_SSH_PUBLIC_KEY`, and `<NAME>_ROOT_PASSWORD`, in Doppler `deploy` and 1Password
- `HOSTINGER_API_TOKEN` covers the account and keeps its name
- Table `files` of `infra/platform-stack.ts` holds each settings file every machine gets, with the commands that make its tool read it
- Docker engines log through driver `local`, which rotates each container's log, and keep containers running across an engine restart
- Machines swap to zram sized a quarter of memory, and keep at most 1 GB of journal
- Machines install Ubuntu and Docker package updates daily through `unattended-upgrades`, and Tailscale updates itself
- Updates install between 05:00 and 06:00 `America/Chicago`, and a machine reboots at once when one requires it
- Reboots and Docker package updates stop a job running then: a runner service has no setting that finishes its job first
- Reboots of `lucky` stop a CI run reading the cache in that minute
- `ufw` drops incoming traffic on every interface except `tailscale0`, so `sshd` answers no public address
- Ports the proxy publishes (80, 443) stay public: Docker forwards a published port before `ufw` reads a packet
- Object storage port 7412 and checker port 8080 answer the tailnet alone: `/etc/ufw/after.rules` drops each in chain `DOCKER-USER` off `tailscale0`
- That rule matches a port Docker forwards to a container (`--ctstate DNAT`), so a container still dials the same port on another machine
- Stacks dial object storage by tailnet name, and `ufw` loads that rule at boot before Docker starts, which keeps `DOCKER-USER` rules
- Machines losing the tailnet are reached through the provider's browser console as `root` with `<NAME>_ROOT_PASSWORD`
- Role `Cache` runs the Nx cache server with its proxy and object storage, and `Runner` the GitHub Actions runner services and each Unreal image
- Role `Runner` field `instances` names each runner service by its label, with the systemd resource limits of its unit
- Runner `<machine>-<label>` holds folder `actions-runner-<label>`, its own work folder, and label `<label>` beside the default labels
- Instance `host` runs jobs on the machine and `container` runs jobs in a container, so each kind keeps ignored files at one path
- Host jobs take at most 20 GB (`MemoryMax`, slowed from 18 GB by `MemoryHigh`) and a job container 10 GB (`container.options` `--memory`)
- Both instances share every core by equal CPU weight, and a job running alone uses all of them
- `needrestart` leaves `actions.runner.*` services running after a library update, as GitHub documents
- Workflow `Maintenance` runs each Saturday at 18:30 UTC on both instances and by dispatch
- Job `container` checks out clean, removing every ignored file of that work folder, the container tools and package store included
- Job `host` checks out clean, installs tools and packages, then prunes the pnpm store, uv cache, mise tool versions, and Docker leftovers
- Docker pruning removes stopped containers made over an hour ago, unused anonymous volumes, build cache, and dangling images, never a tagged image
- GitHub can delay or drop a scheduled run, runs it from `main` alone, and disables it after 60 days without repository activity
- Role `Apps` runs the proxy, sign-in, and object storage of hosted apps
- Role `Notify` runs ntfy behind the proxy at `ntfy.<zone>`, public so a phone off the tailnet reads a notification, and every topic needs a login
- Requests to ntfy hold its own password or token, so its route takes no sign-in middleware, which answers 401 to a credential it does not hold
- Table `accounts` of `infra/platform-stack.ts` holds each ntfy user with its role and topic rules, and a stack generates its password and token
- `pulumi stack output notify --show-secrets` on stack `lucky.parametric-arsenal.cloud` prints them, the phone password and Shortcut token included
- Messages make ntfy send ntfy.sh one request each (`upstream-base-url`) with the message identifier and a topic address hash, so iOS wakes the app
- Requests to ntfy.sh without an account stop at 250 a day, and a paid account's token as `upstream-access-token` raises that
- Role `Watch` runs Gatus on port 8080 with field `memory` as its limit, and its page and API need no login on the tailnet
- Checkers probe their machine's services inside its Docker network, and each other machine's public names, certificates (10 days left), and `Watch`
- Checkers reach another machine by tailnet name through resolver `100.100.100.100`, which a container's own resolver never asks
- Checkers beside role `Notify` alert to its topic `infra`, any other checker to a generated ntfy.sh topic, so a stopped `lucky` still pages
- `pulumi stack output watch --show-secrets` prints a checker's address, heartbeat token, and alert topic, the ntfy.sh topic of `derby` included
- Table `jobs` holds each job reporting to the checker beside `Notify` as `<group>_<name>`, with the hours of silence that fail it
- Jobs POST `http://lucky:8080/api/v1/endpoints/<group>_<name>/external?success=<true|false>` with that heartbeat token as bearer
- Workflows `Maintenance` and `Deploy` report `jobs_maintenance` and `jobs_deploy` last, with CI secrets `GATUS_HEARTBEAT_*` the stack writes
- Each machine's `apt-daily-upgrade` service reports `jobs_apt` to its own checker when it stops, and 26 hours of silence fail it
- Stacks declare containers, volumes, networks, and images through the Docker provider, and an engine builds its images from inline Dockerfiles
- Machines hold one CNAME `<service>.<zone>` per routed service, and a container with no route gets no public name
- Apps on a machine get CNAME `<app>.<zone>` behind the sign-in of that machine, which holds role `Apps`
- Zones hold each public service name once, so one machine per zone holds role `Apps`
- Both machines are KVM guests without nested virtualization, so another need is a container of a role and never a VM

## [04]-[CACHE]

- Nx hashes a task's inputs (source files, dependency files, config, lock file package versions) and replays the stored result of an equal hash
- Pushes to `main` read and write the cache server, and pull requests read it
- Plugin row `@rasm/nx-distribution` of `nx.json` sends the server one read before the tasks of a run that sets `NX_SELF_HOSTED_REMOTE_CACHE_SERVER`
- Runs answered other than 200 or 404, or not within 5 seconds, print one warning (an Actions annotation) and use the local cache alone
- Workflow `Release` uses no remote cache, so its run builds every file it attaches or publishes
- This Mac uses its local cache alone: no remote cache variable is set here and Doppler `dev` holds none
- Nx 24 stops a run on a failed cache read or a failed write of a batched task (cargo, dotnet): a server failing after its first answer
- `--skip-remote-cache` runs without the server, and no Nx setting uses the server and continues on its error
- Failed writes of any other task fail that task and each task depending on it, after 6 attempts over 1 to 20 seconds
- Cache objects expire 7 days after their write (role `Cache` field `kept`): object storage expires by write date, and a read refreshes none
- Tasks unchanged for 7 days run once more on the next push to `main`, which writes their results again
- `lucky` runs the cache alone, so no work on `derby` reaches it
- Deploys recreate a cache container only on its changed definition, after the CI run of the newest push finishes every task
- Recreating a cache container takes about a second: a request in flight fails its run, and one sent meanwhile waits and answers
- Lock files are hash inputs: a changed lock reruns every task depending on its packages, and never needs a machine update
- Plugin source is no task input: an edit under `apps/nx` changes a task's hash only where it changes the target's inferred configuration
- Runs after an executor edit replay results the old executor produced, so its tasks run once with `--skip-nx-cache`
