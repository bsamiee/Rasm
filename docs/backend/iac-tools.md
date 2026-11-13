# [IAC_TOOLS]

Evaluation of tools that state and run infrastructure as code, judged for a repository that manages a GitHub repository, rented machines, containers, and hosted services. Candidates are judged for macOS developer machines and Linux hosts alone. Facts were read on 2026-11-12 from each tool's documentation and source at its newest release and from public repositories read file by file. `[Snn]` names a row of [13]-[SOURCES].

## [01]-[QUESTION]

Owner requirements, restated as tests each candidate takes:

| [INDEX] | [TEST]   | [MEANING]                                                                             |
| :-----: | :------- | :------------------------------------------------------------------------------------ |
|  [01]   | Code     | Written in a general-purpose language, no hand-written YAML or Dockerfile             |
|  [02]   | Reach    | Covers GitHub settings, DNS, machine purchase, machine contents, containers, services |
|  [03]   | Start    | First machine, first state store, and first credential have a stated path             |
|  [04]   | Drift    | Hand change on a managed thing is detected and corrected                              |
|  [05]   | Hosts    | Ten hosts come from one definition                                                    |
|  [06]   | Choice   | One declaration binds to a self-hosted or a hosted implementation                     |
|  [07]   | Rows     | App lists what it needs, defaults supply ports, sockets, credentials, and extensions  |
|  [08]   | Runtime  | Works for one machine and for a cluster                                               |
|  [09]   | Library  | Serves as base of a package other code fills in                                       |
|  [10]   | Delivery | Merge applies changes, no command typed by hand                                       |
|  [11]   | Health   | License, release cadence, owner, and real users                                       |

## [02]-[LAYERS]

No tool read passes every test. Tools divide by what they can read back, and each layer has its own candidates:

| [INDEX] | [LAYER]         | [HOLDS]                                                       | [READ_THROUGH]        |
| :-----: | :-------------- | :------------------------------------------------------------ | :-------------------- |
|  [01]   | Resource engine | Objects behind a vendor API: settings, DNS, machines, buckets | Vendor API            |
|  [02]   | Machine         | Packages, files, and services inside one Linux host           | SSH or agent on host  |
|  [03]   | Runtime         | Containers on one engine, pods on a cluster                   | Engine or cluster API |
|  [04]   | Image           | Filesystem a container starts from                            | Registry              |
|  [05]   | App model       | Rows an app states, bound to layers 1 and 3                   | Own code              |
|  [06]   | Delivery        | Event that starts a run                                       | CI or agent on host   |

- Resource engines (Pulumi, OpenTofu, Alchemy) manage layer 3 through providers, since a Docker engine and a Kubernetes cluster each expose an API
- Resource engines hand layer 2 to another tool: Terraform documents provisioners as a last resort, Pulumi's command provider has no read [S04] [S26]
- Public repositories pair an engine with a machine tool: OpenTofu with NixOS, Pulumi with Ansible, Terraform with pyinfra [S21] [S39] [S40]
- No public repository was found that applies GitHub settings, machines, and containers from one engine on merge [S61]
- `radiosilence/jaritanet` comes closest: one `pulumi up` on merge for machine, k3s, and workloads, with GitHub settings unmanaged [S23]

## [03]-[FIELD]

Candidates alive on 2026-11-12, release and license read from each repository:

| [INDEX] | [TOOL]           | [LAYER] | [WRITTEN_IN]                    | [RELEASE]                    | [LICENSE]    |
| :-----: | :--------------- | :-----: | :------------------------------ | :--------------------------- | :----------- |
|  [01]   | Pulumi           |    1    | TS, Python, Go, C#, Java        | `v3.276.0`, 2026-11-12       | Apache-2.0   |
|  [02]   | OpenTofu         |    1    | HCL, JSON                       | `v1.13.9`, 2026-11-05        | MPL-2.0      |
|  [03]   | Terraform        |    1    | HCL, JSON                       | `v1.17.2`, 2026-11-11        | BUSL         |
|  [04]   | CDK Terrain      |    1    | TS, Python, Java, C#, Go        | `v0.25.0`, 2026-10-27        | MPL-2.0      |
|  [05]   | Alchemy          |  1, 3   | TypeScript with Effect          | `v2.0.0-beta.86`, 2026-11-12 | Apache-2.0   |
|  [06]   | SST              |    1    | TypeScript                      | `v4.17.2`, 2026-10-08        | MIT          |
|  [07]   | Crossplane       |    1    | YAML, Go, Python, KCL           | `v2.4.3`, 2026-10-22         | Apache-2.0   |
|  [08]   | AWS CDK          |    1    | TS, Python, Java, C#, Go        | `v2.276.0`, 2026-11-10       | Apache-2.0   |
|  [09]   | Formae           |    1    | Pkl                             | `0.90.5`, 2026-10-21         | FSL-1.1-ALv2 |
|  [10]   | NixOS            |    2    | Nix                             | `26.05`, 2026-05-30          | MIT          |
|  [11]   | `nixos-anywhere` |    2    | Nix                             | `1.13.0`, 2025-11-13         | MIT          |
|  [12]   | `pyinfra`        |    2    | Python                          | `v3.11.0`, 2026-11-09        | MIT          |
|  [13]   | Talos Linux      |  2, 3   | YAML                            | `v1.14.6`, 2026-11-11        | MPL-2.0      |
|  [14]   | Incus            |  2, 3   | CLI, REST, HCL through provider | `v7.6.0`, 2026-10-30         | Apache-2.0   |
|  [15]   | `k3s`            |    3    | Kubernetes objects              | `v1.37.2+k3s1`, 2026-11-03   | Apache-2.0   |
|  [16]   | Kamal            |    3    | YAML                            | `v2.12.0`, 2026-06-18        | MIT          |
|  [17]   | Uncloud          |    3    | Compose YAML                    | `v0.22.0`, 2026-11-06        | Apache-2.0   |
|  [18]   | Komodo           |    3    | TOML                            | `v2.3.4`, 2026-10-22         | GPL-3.0      |
|  [19]   | Coolify          |    3    | Web UI, API                     | `v4.7.5`, 2026-11-10         | Apache-2.0   |
|  [20]   | Dagger           |    4    | Go, TypeScript, Python          | `v0.21.11`, 2026-10-21       | Apache-2.0   |
|  [21]   | `nix2container`  |    4    | Nix                             | `v1.1.0`, 2026-09-24         | Apache-2.0   |
|  [22]   | `crane`          |    4    | CLI                             | `v0.22.1`, 2026-09-04        | Apache-2.0   |
|  [23]   | Railpack         |    4    | Nothing, or JSON                | `v0.43.0`, 2026-10-19        | MIT          |
|  [24]   | `apko`           |    4    | YAML                            | `v1.4.18`, 2026-11-09        | Apache-2.0   |
|  [25]   | Aspire           |    5    | C#, TypeScript                  | `v13.6.5`, 2026-11-10        | MIT          |
|  [26]   | Encore           |    5    | TypeScript, Go                  | `v1.58.8`, 2026-10-20        | MPL-2.0      |
|  [27]   | `comin`          |    6    | Nix                             | `v0.14.0`, 2026-07-19        | MIT          |
|  [28]   | Flux             |    6    | YAML                            | `v2.10.0`, 2026-10-26        | Apache-2.0   |

Candidates that ended, with date and stated successor:
- CDK for Terraform: archived 2025-12-10, HashiCorp names no successor and recommends HCL, CDK Terrain is its community fork [S27] [S28]
- System Initiative: archived 2026-02-06, successor swamp is an agent workflow tool and no desired-state engine [S34]
- Wing: company closed 2025-04-09, last release 2025-02-03 [S34]
- Nitric's successor Monk: archived 2026-03-17 [S34]
- Earthly: maintenance ended, cloud shut down 2025-07-16, fork EarthBuild continues Earthfile syntax [S48]
- Nixpacks: maintenance mode, Railpack replaces it [S51]
- Archived: `kaniko`, `firecracker-containerd`, Watchtower, and Daytona's public repository [S44] [S51] [S54]

## [04]-[ENGINES]

Four engines pass enough tests to compare. SST runs Pulumi's engine, AWS CDK reaches AWS alone, and Formae is source-available with no GitHub, Cloudflare, or Hetzner plugin [S30] [S32] [S33].

| [INDEX] | [TEST]            | [PULUMI]                       | [OPENTOFU_CDK_TERRAIN]     | [ALCHEMY_V2]                | [CROSSPLANE]        |
| :-----: | :---------------- | :----------------------------- | :------------------------- | :-------------------------- | :------------------ |
|  [01]   | Before first run  | Account and token, or a bucket | Nothing, state file local  | Nothing, state folder local | Running cluster     |
|  [02]   | State stores      | Cloud, S3, Blob, GCS, Postgres | Any backend, encrypted     | Local, Cloudflare, AWS, PG  | `etcd`              |
|  [03]   | Drift detection   | `refresh` on request           | `plan` exit code 2         | `alchemy drift` exit code 2 | Poll each minute    |
|  [04]   | Drift on schedule | Own cron, or Pro at $400       | Own cron                   | Own cron                    | Built in            |
|  [05]   | Providers         | Native, bridged, any Terraform | Every Terraform provider   | Own packages alone          | Thin outside clouds |
|  [06]   | GitHub settings   | Yes                            | Yes                        | Repository, Ruleset, Secret | Repository, Ruleset |
|  [07]   | Tailscale         | Yes                            | Yes                        | No package                  | No release          |
|  [08]   | Remote Docker     | `host: ssh://`                 | `host = "ssh://"`          | `Docker.Context` over SSH   | No                  |
|  [09]   | Kubernetes        | Provider `v4.34.3`             | Provider                   | Package                     | Native              |
|  [10]   | Machine contents  | Commands, no read              | Provisioners, last resort  | Service on systemd over SSH | Ansible provider    |
|  [11]   | Library form      | Component, package             | Module, construct          | Effect Layer                | Composition         |
|  [12]   | Release state     | Stable, about weekly           | Stable, fork has 284 stars | Beta since 2026-04-13       | CNCF graduated      |

### [04.1]-[PULUMI]

- Start: `pulumi login` to Pulumi Cloud, or a bucket, folder, or PostgreSQL database that exists before any stack [S02] [S07]
- Start: provider tokens (machine vendor, GitHub, Cloudflare) and each vendor account are made by hand once [S24]
- Start: Automation API `PulumiCommand.install` downloads its CLI through `install.sh`, so a package needs no tool manager for it [S62]
- State: Free edition gives 1 user, unlimited stacks and updates, 500 workflow minutes, and no resource limit [S03]
- State: self-managed stores lock by file, lack transactional recovery, and lose GitHub App, ESC, and OIDC login [S02] [S07] [S09]
- State: `pulumi stack migrate` moves a stack between stores from CLI v3.254.0 [S02]
- Drift: no operation refreshes unless asked, `pulumi up --refresh` refreshes then applies [S02]
- Drift: `pulumi refresh --preview-only --expect-no-changes` exits non-zero on drift, for a cron job [S08]
- Drift: scheduled detection inside Pulumi Cloud needs Pro at $400 per month [S03]
- Machines: `command.remote.Command` runs over SSH with host key checking off unless `hostKey` is set [S04]
- Machines: `CopyToRemote` implements no delete, so destroy leaves copied files [S04]
- Containers: `@pulumi/docker` v5.3.0 bridges `kreuzwerker/docker` v4.7.0 and accepts `host: ssh://` [S12]
- Containers: refresh drops a container, volume, or network removed by hand from state, and next `up` recreates it [S12]
- Containers: open defects show a change on every run for `networkMode` and registry credentials [S12]
- Images: `@pulumi/docker-build` v0.0.23 is pre-1.0 and requires Dockerfile text, as file or inline string [S13]
- Reach: `pulumi package add terraform-provider <name>` generates a typed SDK for any Terraform provider [S11]
- Reach: Hostinger has no Pulumi package, its Terraform provider v0.1.23 creates a machine, keys, scripts, and DNS records [S19]
- Limits: `Pulumi.yaml` is YAML, inline programs through Automation API need none on disk [S15]
- Limits: Rust is no supported language, request open with 690 reactions [S01]
- Users: BMW, NVIDIA, Supabase, Snowflake, Starburst, Unity, and Lemonade are named on Pulumi's own pages [S01]
- Users: `schmitthub/openclaw-deploy` runs machines and containers from TypeScript components with no committed Compose file [S20]
- Users: `codigo/basic-infra-setup` creates machines with Pulumi and hands contents to Ansible [S21]
- Users: `madhank93/homelab` creates machines with Pulumi and hands contents to a shell script and Compose [S22]

### [04.2]-[OPENTOFU_AND_CDK_TERRAIN]

- OpenTofu encrypts state and plan files on its client for every backend, which Pulumi and Terraform leave to a store [S25] [S26]
- OpenTofu provider configuration accepts no value known only after apply, a new machine's address included [S25]
- HCL is no general-purpose language, and CDK Terrain is its one maintained route from general-purpose code to Terraform JSON [S28]
- CDK Terrain needs Node.js and an OpenTofu or Terraform binary, and `init` configures HCP Terraform unless `--local` is passed [S28]
- `vincentbernat/cdktf-take1` uses it for machines and DNS, applies by hand, and passes host configuration to Colmena [S28]
- Terraform's license is BUSL with IBM as licensor [S26]

### [04.3]-[ALCHEMY]

- Version 2 states resources as Effect programs and requires `effect` `^4.1.4` [S29]
- Packages cover GitHub, Cloudflare, Docker, Kubernetes, Hetzner, Doppler, Neon, AWS, and GCP [S29]
- `Hetzner.Service` bundles an Effect program, copies it over SSH, and runs it under systemd [S29]
- `alchemy drift` re-reads live resources, exits 2 on drift, and `--repair` restores recorded state [S29]
- State stores: local folder, Cloudflare Durable Object, DynamoDB, Cloud Storage, and PostgreSQL with an advisory lock [S29]
- Bindings are Effect Layers, so one service swaps a hosted for a self-hosted implementation by providing another Layer [S29]
- Limits: README states beta and breaking changes, 86 betas since 2026-04-13, 510 open issues, one author holds most commits [S29]
- Limits: no Tailscale package, no PostgreSQL role resource, no Hostinger package, and no reuse of Terraform or Pulumi providers [S29]
- Limits: `Docker.Image` builds from a Dockerfile [S29]
- Limits: state encoding writes each `Redacted` value as plain text inside a marker object [S29]
- Users: one public repository on beta.85 manages GitHub and Cloudflare, none found manages machines or containers [S29]

### [04.4]-[KUBERNETES_ENGINES]

- Crossplane, `kro`, `cdk8s`, and Yoke each need a running cluster before any resource exists [S31]
- Crossplane corrects drift continuously, its providers for Cloudflare, Hetzner, Tailscale, and Docker have no usable release [S31]
- Upbound limits users without a subscription to 12 months of official provider releases [S31]
- Manifests from `cdk8s` are applied by another tool [S31]

## [05]-[MACHINES]

### [05.1]-[CREATING_A_MACHINE]

| [INDEX] | [VENDOR]     | [TERRAFORM_PROVIDER]      | [PULUMI_PACKAGE]    | [FIRST_BOOT_INPUT]            |
| :-----: | :----------- | :------------------------ | :------------------ | :---------------------------- |
|  [01]   | Hetzner      | `hcloud` `v1.70.2`        | `v1.43.4`           | Cloud-init, ISO, rescue mode  |
|  [02]   | DigitalOcean | `v2.107.0`                | `v4.85.0`           | Cloud-init, custom image      |
|  [03]   | Vultr        | `v2.33.2`                 | Community, 2025     | Cloud-init, ISO, snapshot     |
|  [04]   | Scaleway     | `v2.92.1`                 | Community `v1.62.0` | Cloud-init, custom image      |
|  [05]   | Hostinger    | `v0.1.23`, community tier | None                | Shell script of at most 48 KB |

- Hostinger's API purchases a machine, reinstalls it, attaches keys, sets firewall rules, and holds one snapshot per machine [S19]
- Hostinger's provider has no firewall or snapshot resource, and destroying `hostinger_vps` cancels its subscription [S19]
- Hostinger accepts no cloud-init data and no custom image, so image-based systems install through SSH after purchase [S19]
- Vendor rows [01] to [04] rest on each provider's documentation at its listed release [S44]

### [05.2]-[CONFIGURING_A_MACHINE]

| [INDEX] | [TOOL]         | [TRUTH]                  | [HAND_CHANGE]                           | [REACH]          | [FIRST_INSTALL]              |
| :-----: | :------------- | :----------------------- | :-------------------------------------- | :--------------- | :--------------------------- |
|  [01]   | NixOS          | Git commit               | Declared files rewritten on each switch | SSH push or pull | `nixos-anywhere`, wipes disk |
|  [02]   | `pyinfra`      | Machine, read each run   | Seen on next run, removal hand-written  | SSH push         | Working SSH                  |
|  [03]   | `cloud-init`   | First-boot document      | Never detected                          | Vendor metadata  | At creation                  |
|  [04]   | Flatcar        | First-boot Ignition file | Never detected, reprovision to change   | SSH              | Custom image or snapshot     |
|  [05]   | Talos Linux    | Machine config on node   | No shell exists                         | `talosctl` API   | ISO, Kubernetes only         |
|  [06]   | `bootc`        | Image tag in a registry  | `/etc` edits kept by merge              | Registry pull    | Converts a stock machine     |
|  [07]   | Pulumi command | Command text in state    | Never detected                          | SSH from engine  | After machine exists         |

Rows [03] to [06] rest on each project's documentation [S41] [S42] [S43].

NixOS facts:
- Modules exist on release 26.05 for PostgreSQL, PgBouncer, pgBackRest, ClickHouse, Redis, Grafana, Traefik, and Authelia [S35]
- Modules exist on release 26.05 for Garage, MinIO, Tailscale, k3s, and Incus, and none for RustFS [S35]
- `services.postgresql.extensions = ps: with ps; [ postgis pg_repack ];` states extensions in one line [S35]
- `virtualisation.oci-containers.containers.<name>` runs an image as a systemd service, with Docker as one backend [S35]
- Stable branch packages PostgreSQL 14 to 18, so a release candidate comes from an image and no module [S35]
- Store mounts read-only, each declared `/etc` file is a link rewritten on activation, and no report lists hand changes [S35]
- `ensureDatabases`, `ensureUsers`, and Incus preseed never delete [S35]
- `nixos-anywhere` installs over SSH through kexec, needs 1.5 GB of memory, and erases its target [S36]
- `comin` runs on each host, polls a Git branch each 60 seconds, builds there, and needs no inbound access [S37]
- NixOS modules configure a NixOS system, macOS developer services come from separate module sets (devenv, services-flake) [S46]
- `NixOS/nixos-wiki-infra` pairs NixOS with OpenTofu for vendor APIs, deploys by hand, and keeps 10 shell scripts [S39]
- `nix-community/infra` hosts pull a system built by CI each 5 minutes, and an admin applies GitHub settings by hand [S39]

Facts for `pyinfra`:
- Python deploy files become shell commands over SSH, with no state file and no agent [S40]
- Docker operations exist, PostgreSQL has no extension operation [S40]
- `zephyros-dev/media-server` deploys on merge through WireGuard and keeps 44 Python files beside 20 templates [S40]

### [05.3]-[AD_HOC_CONTAINERS]

- Every tool read keeps a container made by hand outside declared configuration [S35] [S44]
- Incus records each instance in its own database, with `incus launch`, `list`, `snapshot create`, `copy`, and `delete` [S44]
- Incus runs system containers, OCI images, and virtual machines through one REST API, with projects and profiles as isolation [S44]
- `--ephemeral` instances delete themselves on stop, `snapshots.expiry` bounds snapshot age [S44]
- Terraform provider `lxc/incus` v1.2.1 declares projects, profiles, networks, and instances, and imports a hand-made instance [S44]
- Sandboxes on microVMs (E2B, microsandbox, Docker Sandboxes) need `/dev/kvm`, so a rented machine needs nested virtualization [S44]
- Declared configuration states where ad-hoc instances run and their limits (project, quota, expiry), while each instance stays imperative

### [05.4]-[REACHING_A_MACHINE]

- Tailscale SSH needs `tailscale set --ssh` once per host, then tailnet policy decides who connects, with no key files [S45]
- Policy is code through `tailscale.Acl` [S45]
- `docker context create --docker host=ssh://<user>@<host>` gives one named entry to a remote engine [S45]
- Incus accepts expiring trust tokens and OIDC on its HTTPS API [S44]
- Hostinger publishes an MCP server v3.40.0 with `search`, `execute`, and `multi-execute` over its API [S19]
- Teleport's community binaries carry a license limited to organizations under 100 employees [S45]

## [06]-[IMAGES]

| [INDEX] | [TOOL]               | [INPUT]                  | [RUNS_COMMANDS] | [NODE_SERVER]         | [POSTGRESQL_WITH_EXTENSIONS] |
| :-----: | :------------------- | :----------------------- | :-------------: | :-------------------- | :--------------------------- |
|  [01]   | Dockerfile, BuildKit | Dockerfile text          |       Yes       | Yes                   | Yes                          |
|  [02]   | Dagger               | Go, TypeScript, Python   |       Yes       | Yes                   | Yes, no doc example          |
|  [03]   | Nix `dockerTools`    | Nix                      | Yes, needs KVM  | `buildNpmPackage`     | `postgresql.withPackages`    |
|  [04]   | `crane append`       | Tar of files, base image |       No        | Bundle on a Node base | No                           |
|  [05]   | Buildpacks, Railpack | Nothing, or JSON         |    Buildpack    | Detected              | No                           |
|  [06]   | `apko`               | YAML                     |       No        | App packaged as apk   | Wolfi packages, no doc       |
|  [07]   | Aspire builder       | C#, TypeScript           |       Yes       | Generated Dockerfile  | Generated Dockerfile         |

- Images that install operating system packages need a build step running commands, which rows [04] to [06] lack [S50] [S51] [S52]
- Dagger's cache is lost after each job on a hosted runner unless Dagger Cloud or a persistent engine holds it [S48]
- Dagger 1.0 is in beta, replaces BuildKit's solver, and its company sells checks run without GitHub Actions [S48]
- Nix images are reproducible by default and load through `docker image load` or push through Skopeo [S35] [S49]
- `docker pussh` (unregistry v0.4.3) sends an image to a remote engine over SSH with no registry [S50]
- Typed code that emits Dockerfile text exists in Aspire (experimental) and in `openclaw-deploy/templates/dockerfile.ts` [S20] [S46]

## [07]-[APP_MODELS]

- Aspire's AppHost states services, references, and parameters in C# or TypeScript [S46]
- Aspire integrations supply ports, credentials, health checks, and connection variables [S46]
- Aspire's TypeScript AppHost is generally available from 13.4 and drives a .NET host process over local RPC [S46]
- Aspire's `addPostgres` has no member for extensions, a custom image needs a Dockerfile [S46]
- Aspire deploys by writing Compose YAML or a Helm chart and running it on a local engine or current cluster context [S46]
- Aspire has no first-party remote host target, request #11209 is open, a prerelease community package adds SSH [S46]
- Aspire switches container for hosted service on Azure alone (`runAsContainer`) [S46]
- Aspire stores deploy state as a plain-text JSON file under a user folder [S46]
- Encore provisions through its paid cloud, and its self-hosted image runs no migrations [S34]
- Kamal, Uncloud, Coolify, and Dokploy take YAML or a web UI and document no drift detection [S54]
- Komodo takes TOML and alerts on pending differences [S54]
- No maintained package was found that gives PostgreSQL with extensions, pooling, and backups as one typed row on a plain Docker engine
- Maintained defaults exist as NixOS modules, Kubernetes operators (CloudNativePG v1.30.1), and Aspire integrations [S35] [S46] [S58]

Lifetimes practitioners separate, with a tool built for each:

| [INDEX] | [LIFETIME]               | [ENDS]                      | [TOOL]                                  |
| :-----: | :----------------------- | :-------------------------- | :-------------------------------------- |
|  [01]   | Container for one test   | Test process exit           | Testcontainers                          |
|  [02]   | Services for development | Developer stops them        | Aspire run mode, Compose, devenv        |
|  [03]   | Service on a host        | Declaration removal         | Engine provider, NixOS module, operator |
|  [04]   | Sandbox made by hand     | Owner deletes it, or expiry | Incus, microVM sandbox tools            |

- Testcontainers states its purpose as test dependencies, removes containers after a run through its reaper, and maps random ports [S47]
- Testcontainers reuse is experimental in Java and .NET and disables cleanup, its docs name no deploy use [S47]
- `libs/typescript/network/tests/ssh` uses Testcontainers for an SSH server inside tests, which is lifetime [01]

## [08]-[RUNTIMES]

- Runtime is a property of a host group: one Docker engine per machine, or one Kubernetes API across machines
- Pulumi, OpenTofu, and Alchemy each hold a Docker and a Kubernetes provider, so one engine serves both [S12] [S18] [S29]
- Each service needs one implementation per runtime: containers and volumes on an engine, an operator or chart on a cluster
- Hosted services are one more implementation with identical outputs (address, credentials)
- Docker Swarm is built into Docker Engine, supported by Mirantis through at least 2030, with `docker.Service` in its provider [S12] [S54]
- Single-binary `k3s` needs 2 cores and 2 GB, CloudNativePG mounts extensions as image volumes on Kubernetes 1.35 [S58]
- Talos Linux runs Kubernetes alone and lists no Hostinger support [S41]
- Nomad's license is BUSL [S54]
- Colima is a developer-machine runtime for macOS and Linux, with Docker, containerd, or Incus and optional Kubernetes [S59]
- Colima runs in foreground through `colima start --foreground`, and its documented autostart is `brew services start colima` [S59]
- Colima makes itself default Docker context on start, with a socket at `$HOME/.colima/default/docker.sock` [S59]
- Docker Engine on a Linux machine runs as a system service, so Linux adds no runtime daemon [S29]
- Local runtime daemons are machine-layer objects (launchd agent on macOS, systemd unit on Linux), which no resource engine reads
- Rust crate `service-manager` 0.11.0 states adapters for operating system service managers [S63]
- `pitchfork` v2.34.0 is a daemon supervisor CLI under MIT, and Lima v2.3.0 source holds a `start-at-login` command [S63]
- Infrastructure code addresses an engine through a Docker context or socket path, whichever daemon started it

## [09]-[DELIVERY]

- GitHub runs a workflow from a YAML file under `.github/workflows` alone, Agentic Workflows compile Markdown to that YAML [S55]
- Generators exist for Kotlin (typed action inputs), TypeScript, Rust, and Pkl, each with a check that generated YAML matches [S56]
- Pkl package `com.github.actions@1.9.2` types core actions and pins each to a commit [S56]
- Documented Pulumi layout: preview on pull request, `up` on push to `main`, through `pulumi/actions` v7.1.0 [S09] [S17]
- Pulumi Deployments applies on push from Pulumi Cloud, with 500 free minutes and no native production gate [S03] [S16]
- `pulumi/auth-actions` exchanges GitHub's OIDC token for a personal Pulumi token on every edition [S10]
- Tailscale accepts GitHub OIDC, Cloudflare documents a stored token alone, Doppler OIDC needs Team plan [S45] [S60]
- Pull agents on a host (`comin`, Komodo Periphery, doco-cd) remove inbound access from CI [S37] [S54]
- `lingrino/infra` applies on push and on a weekly cron, `busybeaver/homelab-iac` runs a weekly no-change preview [S61]
- GitHub provider v6.13.0 lacks code scanning default setup, private vulnerability reporting, and immutable releases [S57]
- Personal accounts have no organization rulesets, and safe-settings needs an organization [S57]

## [10]-[LIBRARY_BASE]

| [INDEX] | [BASE]      | [UNIT]              | [CONSUMED_FROM]                  | [IMPLEMENTATION_SWAP]    |
| :-----: | :---------- | :------------------ | :------------------------------- | :----------------------- |
|  [01]   | Pulumi      | Component resource  | Same language, or any as package | Class per implementation |
|  [02]   | Alchemy     | Effect Layer        | TypeScript                       | Provide another Layer    |
|  [03]   | CDK Terrain | Construct           | Five languages through jsii      | Class per implementation |
|  [04]   | OpenTofu    | Module              | HCL                              | Module `source` variable |
|  [05]   | NixOS       | Module with options | Nix                              | Option value             |
|  [06]   | Aspire      | Hosting integration | C#, TypeScript                   | Azure alone              |
|  [07]   | Crossplane  | Composition         | YAML claim                       | `compositionSelector`    |

- Pulumi components group resources as one resource, and a source-based package serves every Pulumi language [S14]
- Publishing to Pulumi's private registry needs Pulumi Cloud, a Git reference works without it [S14]
- Pulumi's guidance for growth is layered stacks joined by stack references [S14]
- `openclaw-deploy` shows a working shape: components `Server`, `HostBootstrap`, and `Gateway` mapped over a configuration list [S20]
- Aspire integrations are .NET code, a TypeScript AppHost declares resources and writes no integration [S46]
- Alchemy's Layer is closest to tests [06] and [09], and its beta state decides against it today [S29]

### [10.1]-[TAKEN_AND_OWNED]

Parts of an engine, and who holds each when a product builds on Pulumi:

| [INDEX] | [PART]                                        | [HOLDER]                   | [EVIDENCE]                          |
| :-----: | :-------------------------------------------- | :------------------------- | :---------------------------------- |
|  [01]   | Plan, ordering, replacement, failure recovery | Pulumi                     | `pkg/engine`, `pkg/resource/deploy` |
|  [02]   | State, checkpoints, locks, secrets, stores    | Pulumi                     | `pkg/backend`                       |
|  [03]   | Diff per resource                             | Provider, through `Diff`   | `provider.proto`                    |
|  [04]   | Provider protocol                             | Pulumi                     | 21 calls in `provider.proto`        |
|  [05]   | Vendor packages that exist                    | Pulumi and vendors         | Registry                            |
|  [06]   | Vendor packages that are missing              | Product                    | Hostinger has none                  |
|  [07]   | Kinds (host, container, service), defaults    | Product                    | Components                          |
|  [08]   | CLI, configuration file, delivery entry       | Product                    | Automation API                      |
|  [09]   | Daemons and workers                           | Product, separate packages | Outside any engine                  |

- Pulumi's provider protocol holds 21 calls, with `Check`, `Diff`, `Create`, `Read`, `Update`, `Delete`, `Construct`, and `GetSchema` among them [S64]
- Terraform's protocol 6.9 holds 22 calls, with `PlanResourceChange`, `ApplyResourceChange`, and `ReadResource` among them [S65]
- SST is a product of this kind: own configuration file, CLI, and components, with Pulumi as provider source and deployment engine [S30]
- Automation API runs stack programs from a product's own process and installs Pulumi's CLI where absent [S15] [S62]

### [10.2]-[VENDOR_PACKAGES]

| [INDEX] | [OPTION]                   | [PRODUCT_WRITES]                       | [PRODUCT_GETS]              | [LIMIT]                     |
| :-----: | :------------------------- | :------------------------------------- | :-------------------------- | :-------------------------- |
|  [01]   | Pulumi component           | Class composing existing resources     | Kind with defaults          | Needs providers beneath     |
|  [02]   | Dynamic provider           | `create`, other calls optional         | Resource inside one program | No plugin, no schema        |
|  [03]   | Native Pulumi provider     | Schema and each call per resource      | Typed SDK, real read        | Plugin to build and publish |
|  [04]   | Bridged Terraform provider | Version pins, or a Go wrapper module   | Every upstream resource     | Upstream quality            |
|  [05]   | Product-owned model        | Interface, each vendor by hand, engine | Full control                | No reuse of providers       |

- Node.js `Provider` interface holds `getSchema`, `check`, `diff`, `create`, `read`, `update`, `delete`, `construct`, and `invoke` [S64]
- Dynamic provider interface requires `create` alone, with `check`, `diff`, `read`, `update`, and `delete` optional [S64]
- `pulumi-go-provider` v1.8.0 derives a provider from Go types, and a resource with no read returns stored state on refresh [S05]
- `pulumi-terraform-bridge` v3.143.0 adapts a provider built on Terraform's plugin SDK through a Go module [S66]
- `pulumi package add terraform-provider` needs no Go, and its Automation API support is an open request, issue #33 [S11] [S66]
- Hostinger's Terraform provider is 8 Go files of 62,911 bytes for 4 resources, a measure of one thin vendor package [S19]
- Alchemy's provider holds `reconcile` and `delete`, with `read`, `diff`, `precreate`, and `list` beside them [S29]
- Alchemy's vendor packages measure 13,503 lines for Hetzner, 9,665 for GitHub, and 9,945 for Docker, tests excluded [S29]

## [11]-[VERDICT]

| [INDEX] | [SUBJECT]         | [DECISION]                                                                          |
| :-----: | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Engine            | Pulumi, for every resource a provider reads through an API                          |
|  [02]   | Machine contents  | No package holds shell commands in state or `docker compose up` over SSH            |
|  [03]   | Containers        | Typed `@pulumi/docker` resources per engine, reached as `ssh://` over a tailnet     |
|  [04]   | Host              | Machine with a container engine and a tailnet agent, created by its vendor provider |
|  [05]   | Vendor            | First-party provider and cloud-init decide which vendors a package supports first   |
|  [06]   | Host list         | Rows in consumer code, read from no secret store                                    |
|  [07]   | Library           | Packages of Pulumi components in a new repository, each consumer states rows alone  |
|  [08]   | Cluster           | `@pulumi/kubernetes` with operators, once machines need shared scheduling           |
|  [09]   | NixOS             | Not chosen as service layer, correct for a host of native services alone            |
|  [10]   | Images            | Layer append for file-copy images, generated build text for PostgreSQL              |
|  [11]   | Aspire            | Not adopted                                                                         |
|  [12]   | Testcontainers    | Stays inside tests                                                                  |
|  [13]   | Ad-hoc containers | Incus on a host that wants them, declared as project, quota, and expiry             |
|  [14]   | Delivery          | Preview on pull request, apply on push to `main`, cron drift check, OIDC login      |
|  [15]   | State             | Consumer sets its state store, Pulumi Cloud Free fits one user                      |
|  [16]   | Pins              | Stable releases of `@pulumi/pulumi` and each provider in package manifests          |
|  [17]   | Workflows         | YAML stays, generation optional                                                     |
|  [18]   | Alchemy           | Reviewed again at stable 2.0                                                        |
|  [19]   | Local daemon      | Separate package owns it, infrastructure packages accept an engine address          |

Rows are recommendations of this review, worded for a new standalone repository with packages other projects import. Each stays open until planning of that project settles it.

Pulumi is a correct choice of engine. No candidate beats it on providers, languages, license, and release record. OpenTofu's gain is client-side state encryption, and its cost is HCL or a fork with 284 stars. Pulumi is no way to manage a machine's contents: refresh reads neither a command nor a Compose project, so decisions [02] and [03] state containers as resources a provider reads.

Decision [03] keeps one definition for a local and a remote engine, which a Docker engine API gives and a machine tool does not. Decision [04] leaves one first-boot script per vendor inside a host component, which every design short of an image-based system keeps. Decision [05] follows from provider quality: Hostinger's is community tier with four resources, no firewall resource, and no first-boot data beyond a script, so it comes after vendors with cloud-init.

Decision [07] answers tests [06], [07], and [09]: host, engine, and each service are components, and each service holds a container implementation and a hosted implementation behind one output type. Defaults for each service are those packages' own code on a Docker engine, since no maintained package supplies them there. Components published to npm serve TypeScript consumers, and a source-based Pulumi package serves every Pulumi language where a consumer holds a Node.js runtime [S14]. Decision [08] places runtime on a host group, costs one more implementation per service, and takes operator defaults in return.

Decision [09]: NixOS is the strongest machine tool read. Its modules run on no developer Mac, so one definition cannot serve a macOS developer machine and a Linux host, which a container engine API does. Release candidates absent from its stable branch are a fact of present pins and decide nothing. Decision [10]: PostgreSQL with extensions keeps a build step, typed code emits it as inline text for `@pulumi/docker` or `@pulumi/docker-build`, and Dagger waits until builds need logic. Decision [11]: Aspire's row model matches test [07], and its deploy writes Compose YAML to a local engine, tracks no drift, and states no extensions.

Decision [14] needs no new tool and belongs to each consumer's CI: apply on push, a preview posted on each pull request, a cron job running refresh with a no-change expectation per stack, and `pulumi/auth-actions` in place of a stored Pulumi token. Packages expose stack programs, and a consumer runs them through Automation API or Pulumi's CLI, so no package depends on a consumer's build tool. Decision [15]: Pulumi accepts Pulumi Cloud and self-managed stores, a state store on a managed host depends on that host, and `pulumi stack migrate` moves between stores. Decision [16]: those packages write state. Decision [17]: GitHub runs nothing but YAML, and generation gains little for a handful of files. Decision [18]: Alchemy's Layer matches test [06] better than a Pulumi component, and Alchemy installs from npm with no engine binary, a gap Pulumi's CLI install from Automation API narrows. Alchemy today lacks Tailscale and PostgreSQL role packages and a stable interface, and each consumer of an Alchemy package must write Effect. Decision [19]: daemons and workers are separate packages of that repository, uncoupled from infrastructure packages, which accept a Docker context or socket path as one field.

Work order: packages for decisions [07], [04], [03], [06], and [10] come first, and each consumer then applies decisions [14] and [16] to its own files.

Vendor integrations are packages of that repository. A vendor with a sound provider is composed by its package, and a vendor with none gets a native provider written from its API or a published bridge of its Terraform provider. SST shows that product form: its own configuration file, CLI, and components over an existing engine [S30].

## [12]-[UNVERIFIED]

- Unverified: `pulumi package add terraform-provider hostinger/hostinger 0.1.23` generates an SDK that Automation API programs load, settled by running it in a scratch folder
- Unverified: Pulumi Cloud Free behavior when stacks update at once, settled by its concurrency documentation or one deploy log
- Unverified: `nixos-anywhere` installs on a Hostinger machine, settled by one install on a disposable machine
- Unverified: Hostinger templates include an image with Docker installed, settled by `GET /api/vps/v1/templates`
- Unverified: `comin` re-applies an unchanged commit after a hand change, settled by its deployer source at v0.14.0
- Unverified: Aspire detects drift on Compose or Kubernetes targets, settled by `src/Aspire.Hosting.Docker` at v13.6.5
- Unverified: `@pulumi/docker` over a Tailscale SSH session with no key file, settled by one `pulumi preview` against a tailnet host
- Unverified: Alchemy's Cloudflare and DynamoDB state stores lock, settled by their store sources at v2.0.0-beta.86
- Unverified: date Pulumi renamed its paid editions, settled by a dated post on Pulumi's blog
- Unverified: which machine vendors expose nested virtualization, settled by each vendor's documentation
- Unverified: a maintained TypeScript package registers launchd agents and systemd units, settled by a registry search
- Unverified: `@pulumi/docker` reaches Colima through its Docker context setting, settled by one `pulumi preview`
- Unverified: limits of a TypeScript dynamic provider shared across programs, settled by Pulumi's dynamic provider documentation
- Unverified: memory and core needs of `k3s` at its current release, settled by its requirements page

## [13]-[SOURCES]

Rows marked `*` were read by a research agent and returned with quotes, unmarked rows were read directly. Each was fetched on 2026-11-12. Repository rows name a commit, documentation rows name a release or that date.

- [S01]: Pulumi v3.276.0 release, license, issue #3622, customer pages `*` — <https://github.com/pulumi/pulumi/releases/tag/v3.276.0>
- [S02]: Pulumi state and backends — <https://www.pulumi.com/docs/iac/concepts/state-and-backends/>
- [S03]: Pulumi pricing — <https://www.pulumi.com/pricing/>
- [S04]: `pulumi-command` v1.3.0 at `e31e379a`, local and remote controllers, README — <https://github.com/pulumi/pulumi-command/tree/v1.3.0>
- [S05]: `pulumi-go-provider` v1.8.0 `infer/resource.go` `*` — <https://github.com/pulumi/pulumi-go-provider/blob/v1.8.0/infer/resource.go>
- [S06]: `pulumi-command` issues #432 and #433 `*` — <https://github.com/pulumi/pulumi-command/issues/432>
- [S07]: Pulumi self-managed backends `*` — <https://www.pulumi.com/docs/iac/operations/stack-management/using-a-diy-backend/>
- [S08]: Pulumi drift `*` — <https://www.pulumi.com/docs/iac/operations/stack-management/drift/>
- [S09]: Pulumi with GitHub Actions `*` — <https://www.pulumi.com/docs/iac/operations/continuous-delivery/github-actions/>
- [S10]: Pulumi GitHub OIDC issuer `*` — <https://www.pulumi.com/docs/administration/guides/oidc-issuers/github/>
- [S11]: Pulumi any Terraform provider `*` — <https://www.pulumi.com/docs/iac/concepts/providers/any-terraform-provider/>
- [S12]: `pulumi-docker` v5.3.0 `*`, bridged provider v4.7.0 docs — <https://github.com/kreuzwerker/terraform-provider-docker/tree/v4.7.0>
- [S13]: `pulumi-docker-build` v0.0.23 schema `*` — <https://github.com/pulumi/pulumi-docker-build/tree/v0.0.23>
- [S14]: Pulumi components and packaging `*` — <https://www.pulumi.com/docs/iac/concepts/components/>
- [S15]: Pulumi Automation API `*` — <https://www.pulumi.com/docs/iac/concepts/automation-api/>
- [S16]: Pulumi Deployments triggers and GitHub App `*` — <https://www.pulumi.com/docs/deployments/concepts/triggers/>
- [S17]: `pulumi/actions` v7.1.0 `*` — <https://github.com/pulumi/actions/tree/v7.1.0>
- [S18]: `pulumi-kubernetes` v4.34.3 README — <https://github.com/pulumi/pulumi-kubernetes/tree/v4.34.3>
- [S19]: Hostinger provider v0.1.23, OpenAPI 2.14.1 `*`, MCP `*` — <https://github.com/hostinger/terraform-provider-hostinger/tree/v0.1.23>
- [S20]: `schmitthub/openclaw-deploy` at `4de1b3a5` `*`, file list read directly — <https://github.com/schmitthub/openclaw-deploy>
- [S21]: `codigo/basic-infra-setup` at `71794e4f` `*` — <https://github.com/codigo/basic-infra-setup>
- [S22]: `madhank93/homelab` at `a98c2b35` `*` — <https://github.com/madhank93/homelab>
- [S23]: `radiosilence/jaritanet` at `59b165ac` `*` — <https://github.com/radiosilence/jaritanet>
- [S24]: `muhlba91/github-infrastructure` at `eec8a3c7`, `BOOTSTRAP.md` `*` — <https://github.com/muhlba91/github-infrastructure>
- [S25]: OpenTofu v1.13.9 state encryption, provider configuration, plan `*` — <https://opentofu.org/docs/language/state/encryption/>
- [S26]: Terraform v1.17.2 license and provisioners `*` — <https://developer.hashicorp.com/terraform/language/provisioners>
- [S27]: CDK for Terraform archive notice of 2025-12-10 `*` — <https://github.com/hashicorp/terraform-cdk>
- [S28]: CDK Terrain v0.25.0, `vincentbernat/cdktf-take1` at `18cbdd63` `*` — <https://github.com/open-constructs/cdk-terrain/tree/v0.25.0>
- [S29]: Alchemy v2.0.0-beta.86 at `dc25b096`, docs, packages, `StateEncoding.ts` — <https://github.com/alchemy-run/alchemy/tree/v2.0.0-beta.86>
- [S30]: SST v4.17.2 docs `*` — <https://github.com/anomalyco/sst/tree/v4.17.2>
- [S31]: Crossplane v2.4.3, Upbound policy 1.0.0, `cdk8s`, `kro`, Yoke `*` — <https://github.com/crossplane/crossplane/tree/v2.4.3>
- [S32]: AWS CDK guide `*` — <https://docs.aws.amazon.com/cdk/v2/guide/home.html>
- [S33]: Formae 0.90.5 README `*` — <https://github.com/platform-engineering-labs/formae/tree/0.90.5>
- [S34]: System Initiative, Wing, Nitric, Monk, Encore v1.58.8 `*` — <https://github.com/systeminit/si>
- [S35]: `nixpkgs` `release-26.05` at `a1557424`, module paths read directly, manual `*` — <https://github.com/NixOS/nixpkgs/tree/release-26.05>
- [S36]: `nixos-anywhere` 1.13.0 `*` — <https://github.com/nix-community/nixos-anywhere/tree/1.13.0>
- [S37]: `comin` v0.14.0 `*` — <https://github.com/nlewo/comin/tree/v0.14.0>
- [S39]: `NixOS/nixos-wiki-infra` at `10c53171`, `nix-community/infra` at `d74010f4` `*` — <https://github.com/NixOS/nixos-wiki-infra>
- [S40]: `pyinfra` v3.11.0, `zephyros-dev/media-server` at `60a54eae` `*` — <https://github.com/pyinfra-dev/pyinfra/tree/v3.11.0>
- [S41]: Talos Linux v1.14 docs `*` — <https://docs.siderolabs.com/talos/v1.14/>
- [S42]: Flatcar 4757.2.3 docs, `bootc` v1.17.4 docs `*` — <https://www.flatcar.org/docs/latest/>
- [S43]: `cloud-init` 26.2 docs `*` — <https://github.com/canonical/cloud-init/tree/26.2>
- [S44]: Incus v7.6.0, provider v1.2.1, vendor providers, sandbox tools `*` — <https://github.com/lxc/incus/tree/v7.6.0>
- [S45]: Tailscale SSH and federation, Docker contexts, Teleport license `*` — <https://tailscale.com/docs/features/tailscale-ssh>
- [S46]: Aspire v13.6.5 full docs read directly, issue #11209, devenv v2.4.0 `*` — <https://aspire.dev/llms-full.txt>
- [S47]: Testcontainers getting started and reuse pages `*` — <https://testcontainers.com/getting-started/>
- [S48]: Dagger v0.21.11, 1.0 beta post of 2026-10-23, pricing, Earthly README `*` — <https://github.com/dagger/dagger/tree/v0.21.11>
- [S49]: `nix2container` v1.1.0, `TraceMachina/nativelink` at `90459eb3` `*` — <https://github.com/nlewo/nix2container/tree/v1.1.0>
- [S50]: `go-containerregistry` v0.22.1, unregistry v0.4.3 `*` — <https://github.com/google/go-containerregistry/tree/v0.22.1>
- [S51]: `pack` v0.40.10, Railpack v0.43.0, Nixpacks, `kaniko` `*` — <https://github.com/railwayapp/railpack/tree/v0.43.0>
- [S52]: `apko` v1.4.18 and Wolfi packages `*` — <https://github.com/chainguard-dev/apko/tree/v1.4.18>
- [S54]: Kamal, Uncloud, Komodo, Coolify, Dokploy, Swarm support, Nomad v2.1.1 `*` — <https://github.com/basecamp/kamal/tree/v2.12.0>
- [S55]: GitHub workflow syntax, changelog of 2026-10-28 `*` — <https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax>
- [S56]: `github-workflows-kt` v4.0.0, `gagen`, `gh-workflow`, `pkl-pantry` `*` — <https://github.com/typesafegithub/github-workflows-kt/tree/v4.0.0>
- [S57]: Terraform GitHub provider v6.13.0, `pulumi-github` v6.15.1 `*` — <https://github.com/integrations/terraform-provider-github/tree/v6.13.0>
- [S58]: `k3s` v1.37.2+k3s1, CloudNativePG v1.30.1, Flux v2.10.0 `*` — <https://github.com/cloudnative-pg/cloudnative-pg/tree/v1.30.1>
- [S59]: Colima v0.10.3 README and `docs/FAQ.md` — <https://github.com/abiosoft/colima/tree/v0.10.3>
- [S60]: Doppler service account identities, Cloudflare CI docs `*` — <https://docs.doppler.com/docs/service-account-identities>
- [S61]: `lingrino/infra` at `df97ccd5`, `busybeaver/homelab-iac` at `60a9bb6e` `*` — <https://github.com/lingrino/infra>
- [S62]: Pulumi v3.276.0 `sdk/nodejs/automation/cmd.ts` — <https://github.com/pulumi/pulumi/blob/v3.276.0/sdk/nodejs/automation/cmd.ts>
- [S63]: `service-manager` 0.11.0, `pitchfork` v2.34.0, Lima v2.3.0 file list — <https://crates.io/crates/service-manager>
- [S64]: Pulumi v3.276.0 `provider.proto`, Node.js provider and dynamic interfaces, `pkg` sizes — <https://github.com/pulumi/pulumi/tree/v3.276.0>
- [S65]: OpenTofu v1.13.9 `tfplugin6.9.proto` — <https://github.com/opentofu/opentofu/tree/v1.13.9/docs/plugin-protocol>
- [S66]: `pulumi-terraform-bridge` v3.143.0 README, `pulumi-terraform-provider` issue #33 — <https://github.com/pulumi/pulumi-terraform-bridge>
