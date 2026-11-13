# [BACKEND_MAP]

Map of what a production backend is made of in 2026: its areas, categories inside each area, what belongs in a category and what only looks like it, where one product is the clear choice and where a real menu exists, and what running a part yourself means beside buying it hosted. Product facts were read on 2026-11-12, and each bracketed key (`[rds]`) names a row of the last section.

## [01]-[AREAS]

Backends hold every program and service between a user's request and stored data, with systems that build, change, watch, and protect them. Sections 03 to 14 follow table order:

| [INDEX] | [AREA]            | [HOLDS]                                                           | [UNIT]                   |
| :-----: | :---------------- | :---------------------------------------------------------------- | :----------------------- |
|  [01]   | Machines          | Providers, servers, operating systems                             | Host                     |
|  [02]   | Workloads         | Images, containers, schedulers, platforms                         | App, job                 |
|  [03]   | Network           | Names, certificates, proxies, edge networks, private links        | Route                    |
|  [04]   | State             | Databases, object storage, volumes                                | Database, bucket, volume |
|  [05]   | Messaging         | Queues, event logs, background jobs, workflows                    | Queue, topic, workflow   |
|  [06]   | Identity          | Sign-in, permissions, machine identity, secrets                   | User, role, secret       |
|  [07]   | Telemetry         | Metrics, logs, traces, profiles, errors, product events           | Signal                   |
|  [08]   | Delivery          | Source hosting, CI, artifacts, deployment, infrastructure as code | Release, stack           |
|  [09]   | Protection        | Backup, recovery, hardening                                       | Backup, policy           |
|  [10]   | Bought services   | Email, payments, feature flags, model APIs                        | Account                  |
|  [11]   | Environments      | Development, test, preview, sandbox                               | Environment              |
|  [12]   | Application model | Declared needs of an app and whatever supplies them               | Resource type            |

Areas relate in a fixed way:
- Hosts run workloads, and a workload is either your own app or a server someone else wrote (PostgreSQL, a proxy, a collector)
- Apps consume backing services over a network: databases, queues, caches, object stores, email senders `[12f-backing]`
- Backing services are roles, and product, version, and place (a container on your host, a vendor's cloud service) are attributes of one role
- Areas 01 to 07 serve requests, areas 08 and 09 change and protect them, area 10 is rented outright, and areas 11 and 12 cut across the rest
- Twelve-factor apps hold no state in their processes, every stored byte belongs to a backing service `[12f-processes]`

Published taxonomies split the same ground:

| [INDEX] | [AREA]            | [CNCF_LANDSCAPE]                                                   | [CNCF_PLATFORM_CAPABILITY]               |
| :-----: | :---------------- | :----------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Machines          | Automation & Configuration                                         | Infrastructure services                  |
|  [02]   | Workloads         | Container Runtime, Scheduling & Orchestration                      | Infrastructure services                  |
|  [03]   | Network           | Cloud Native Network, Service Proxy, API Gateway, Service Mesh     | Infrastructure services                  |
|  [04]   | State             | Database, Cloud Native Storage                                     | Data services                            |
|  [05]   | Messaging         | Streaming & Messaging                                              | Messaging and event services             |
|  [06]   | Identity          | Key Management, Security & Compliance                              | Identity and secret management services  |
|  [07]   | Telemetry         | Observability                                                      | Observability                            |
|  [08]   | Delivery          | Continuous Integration & Delivery, Container Registry, Image Build | Build, test, delivery, artifact storage  |
|  [09]   | Protection        | Security & Compliance                                              | Security services                        |
|  [10]   | Bought services   | Feature Flagging                                                   | None                                     |
|  [11]   | Environments      | None                                                               | Development environments                 |
|  [12]   | Application model | Application Definition, PaaS/Container Service                     | Provisioning APIs, golden path templates |

CNCF landscape names come from `landscape.yml` `[cncf-landscape]`, capability names from the CNCF Platforms White Paper `[cncf-platforms]`.

## [02]-[TERMS]

One word names one thing through this map:

| [INDEX] | [TERM]                | [MEANING]                                                                                     |
| :-----: | :-------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | Provider              | Company renting machines or services by hour, month, or request                               |
|  [02]   | Bare-metal server     | Physical machine rented or owned whole                                                        |
|  [03]   | Virtual machine       | Machine emulated through hardware virtualization, with its own kernel `[incus-containers]`    |
|  [04]   | VPS                   | Virtual machine rented from a provider                                                        |
|  [05]   | MicroVM               | Virtual machine with a reduced device set, started through KVM `[firecracker]`                |
|  [06]   | Host                  | Machine with an operating system that runs workloads, physical or virtual                     |
|  [07]   | Image                 | Read-only package of one program and every file it needs `[docker-container]`                 |
|  [08]   | Application container | One isolated process started from an image, sharing its host's kernel `[docker-container]`    |
|  [09]   | System container      | Whole Linux user space sharing its host's kernel, used like a small host `[incus-containers]` |
|  [10]   | Service               | Process that runs until stopped and answers requests                                          |
|  [11]   | Worker                | Process that runs until stopped and takes work from a queue `[12f-disposability]`             |
|  [12]   | Job                   | Process that runs to completion, then stops `[k8s-workloads]`                                 |
|  [13]   | Scheduled job         | Job started on a schedule `[k8s-workloads]`                                                   |
|  [14]   | Function              | Code a platform starts per request and stops when idle                                        |
|  [15]   | Scheduler             | Program that places containers on hosts and keeps a stated count running `[k8s-workloads]`    |
|  [16]   | Platform              | Layer that accepts an app and runs it without showing hosts or scheduler `[cncf-platforms]`   |
|  [17]   | App                   | Your own code: one or more images, config, and a list of backing services it needs            |
|  [18]   | Backing service       | Any service an app consumes over a network as part of normal operation `[12f-backing]`        |
|  [19]   | Config                | Values that differ between deploys: resource handles, credentials, hostnames `[12f-config]`   |
|  [20]   | Environment           | One deployed copy of an app with its own config and backing services `[gh-environments]`      |
|  [21]   | Stack                 | One independently configured instance of an infrastructure program `[pulumi-stacks]`          |
|  [22]   | Volume                | Storage that outlives containers using it `[docker-volumes]`                                  |

Words that carry more than one meaning in backend writing:
- Host: a machine in this map, and a name in an HTTP request, where one web server holds many virtual servers `[nginx-request]`
- Worker: a process of your app that handles background work `[12f-concurrency]`
- Worker, second meaning: a Cloudflare Worker, a function unit of one hosted platform `[cf-workers]`
- Server: a machine, and a program that listens for connections (a PostgreSQL server runs on a server)
- Daemon: any long-running background process, which this map calls a service or a worker `[systemd-daemon]`
- Service: a long-running process, a Compose or Kubernetes object describing one, and a product a vendor runs for you

Nesting runs from provider to process, and each level is replaced on its own schedule:

```text
Provider account
└── Machine (bare-metal server or virtual machine)       # rented by the hour or month
    └── Host operating system                            # patched, hardened, monitored
        ├── System container or nested virtual machine   # a smaller host inside this host
        └── Container runtime
            ├── App container                            # your image, replaced on every release
            ├── Backing-service container                # someone else's image, state on a volume
            └── Short-lived container                    # job, test dependency, debug shell
```

Host, container, and app differ in kind:
- Hosts are capacity: CPU, memory, disk, a network address, and a kernel. Hosts know nothing about any app
- Containers are running processes. Docker docs define one as "an isolated process with all of the files it needs to run" `[docker-container]`
- Apps are descriptions: which images to run, with which config, attached to which backing services
- Apps exist before any of their containers run and after every one stops
- Virtual machines carry their own kernel and can run another operating system, containers share one kernel `[incus-containers]`
- Application containers hold one program, system containers hold a full user space with many programs `[incus-containers]`
- Docker runs inside an Incus system container, Incus does not run inside a Docker container `[incus-containers]`

Container lifetime follows purpose, and whatever starts a container owns its removal:

| [INDEX] | [PURPOSE]             | [STARTED_BY]           | [ENDS_WHEN]                                                                      |
| :-----: | :-------------------- | :--------------------- | :------------------------------------------------------------------------------- |
|  [01]   | App service or worker | Deployment             | Next release replaces it                                                         |
|  [02]   | Backing service       | Deployment             | Version update replaces it, its volume stays                                     |
|  [03]   | Job                   | Deployment or schedule | Process exits `[k8s-workloads]`                                                  |
|  [04]   | Admin process         | Operator               | Command exits (migration, console, one-time script) `[12f-admin]`                |
|  [05]   | CI job                | CI system              | Job ends, a GitHub-hosted runner is a new virtual machine per job `[gh-runners]` |
|  [06]   | CI service container  | CI job                 | Job completes `[gh-services]`                                                    |
|  [07]   | Test dependency       | Test code              | Test run ends, a reaper container removes leftovers `[tc-start]`                 |
|  [08]   | Debug container       | Operator               | Process exits, never restarted `[k8s-ephemeral]`                                 |
|  [09]   | Preview environment   | Pull request           | Pull request closes                                                              |
|  [10]   | Sandbox               | Person or agent        | Owner deletes it, or a hosted platform's idle limit stops it                     |

Rows 01 to 03 exist for as long as an app or host does and are declared beside it. Rows 04 to 08 begin and end inside one command, CI run, or test run, and no deployment description or state file lists them. Rows 09 and 10 are whole environments, covered in section 13.

## [03]-[MACHINES]

Machines are rented or owned capacity. Four decisions stack here: who supplies a machine, what kind it is, which operating system it boots, and how it reaches its configured state.

Machine kinds:

| [INDEX] | [KIND]           | [IS]                                                    | [SOLD_AS]                                           |
| :-----: | :--------------- | :------------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | Dedicated server | Whole physical machine, billed monthly                  | Hetzner Robot, OVHcloud, Scaleway, Latitude.sh      |
|  [02]   | Virtual server   | Virtual machine on shared hardware, billed hourly       | Hetzner Cloud, DigitalOcean, Vultr, Linode, AWS EC2 |
|  [03]   | Own hypervisor   | Virtual machines and system containers on your hardware | Proxmox VE (AGPL-3.0), Incus (Apache-2.0)           |
|  [04]   | MicroVM          | Virtual machine a platform starts per workload          | Firecracker under AWS Lambda                        |

Rows rest on `[providers]`, `[proxmox]`, `[incus]`, and `[firecracker]`. Incus is a community fork of Canonical's LXD, which Canonical moved to AGPL-3.0 with a contributor agreement in December 2023 `[incus]` `[lxd-relicense]`.

Providers differ in what they run above a machine:

| [INDEX] | [PROVIDER]         | [MANAGED_KUBERNETES] | [MANAGED_DATABASES] | [OBJECT_STORAGE] | [PROVISIONING_PROVIDER]              |
| :-----: | :----------------- | :------------------- | :------------------ | :--------------- | :----------------------------------- |
|  [01]   | Hetzner            | No                   | None listed         | Yes              | `hetznercloud/hcloud`                |
|  [02]   | OVHcloud           | Yes                  | Yes                 | Yes              | `ovh/ovh`                            |
|  [03]   | Scaleway           | Yes                  | Yes                 | Yes              | `scaleway/scaleway`                  |
|  [04]   | DigitalOcean       | Yes                  | Yes                 | Yes              | `digitalocean/digitalocean`          |
|  [05]   | Vultr              | Yes                  | Yes                 | Yes              | `vultr/vultr`                        |
|  [06]   | Akamai (Linode)    | Yes                  | Yes                 | Not read         | `linode/linode`                      |
|  [07]   | UpCloud            | Yes                  | Yes                 | Not read         | `UpCloudLtd/upcloud`                 |
|  [08]   | Latitude.sh        | Yes                  | Yes                 | Yes              | `latitudesh/latitudesh`              |
|  [09]   | AWS, Google, Azure | Yes                  | Yes                 | Yes              | `hashicorp/aws`, `google`, `azurerm` |
|  [10]   | Netcup, Contabo    | None listed          | None listed         | Not read         | Contabo community tier only          |

Rows rest on `[providers]` and `[tf-registry]`. Equinix Metal reached end of life on 2026-06-30 `[equinix-metal]`.

Operating systems for hosts:

| [INDEX] | [KIND]          | [CHANGES_BY]                                         | [PRODUCTS]                                           |
| :-----: | :-------------- | :--------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | General purpose | Packages installed and updated in place              | Debian 13, Ubuntu 26.04 LTS and 24.04 LTS            |
|  [02]   | Container host  | Whole-image updates, containers hold everything else | Flatcar Container Linux, Fedora CoreOS, Bottlerocket |
|  [03]   | Kubernetes host | Whole-image updates, runs Kubernetes                 | Talos Linux                                          |
|  [04]   | Declarative     | Whole system rebuilt from one configuration          | NixOS 26.05                                          |
|  [05]   | Image based     | Boots and updates from a container image             | bootc (CNCF Sandbox)                                 |

Rows rest on `[os-releases]` and `[machines-github]`. Flatcar is CNCF incubating since 2024-08-02 `[flatcar-cncf]`.

Machines reach a configured state by one of these routes:
- First boot: cloud-init on general-purpose images, Ignition on Flatcar and Fedora CoreOS `[machines-github]`
- Butane merged into Ignition's repository after 0.29.0, its last standalone release `[butane]`
- In place over SSH or an agent: Ansible (GPL-3.0, Red Hat), pyinfra (MIT), Salt (Apache-2.0, Broadcom) `[machines-github]`
- Chef's binaries are under a EULA. Perforce moved Puppet's binaries private, and OpenVox is its open fork `[chef-license]` `[openvox]`
- Replacement: build an image and boot a new machine from it
- Replacement tools: Packer (BUSL-1.1), mkosi, bootc, and NixOS with nixos-anywhere, Colmena, deploy-rs, or comin `[machines-github]`

Every machine is rented from someone unless you own hardware, and "hosted" here measures how far up a provider operates. AWS states its side as hardware, facilities, and virtualization, and leaves guest operating system, patches, and firewall rules with its customer `[aws-shared]`.

Published cost accounts point both ways. 37signals left AWS for owned Dell servers and reports its cloud bill falling from 3.2 million US dollars a year in 2022 to 1.3 million in 2024, all of that remainder for S3 `[37signals-exit]` `[37signals-savings]`. Netflix finished moving to AWS in 2016 after seven years and reports eightfold member growth over that time `[netflix-aws]`.

## [04]-[WORKLOADS]

Workloads turn code into running processes. Each layer uses the one below:

| [INDEX] | [LAYER]              | [DOES]                                            | [PRODUCTS]                                                   |
| :-----: | :------------------- | :------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | Image and registry   | Packages a program, moves it between machines     | OCI image 1.1.1, runtime 1.3.0, and distribution 1.1.1 specs |
|  [02]   | Runtime              | Starts one container from an image                | runc, crun, gVisor, Kata Containers                          |
|  [03]   | Engine               | Manages images, containers, networks on one host  | containerd, Docker Engine                                    |
|  [04]   | One-host scheduler   | Keeps a described set of containers running       | Docker Compose                                               |
|  [05]   | Cluster scheduler    | Places containers across hosts, replaces failures | Kubernetes, k3s, k0s, Nomad, Docker Swarm mode               |
|  [06]   | Self-hosted platform | Runs an app on your hosts through layers 01 to 05 | Coolify, Dokploy, Kamal, Dokku, CapRover                     |
|  [07]   | Hosted platform      | Accepts an app and hides every host               | Fly.io, Railway, Render, Cloud Run, Fargate, Lambda, Vercel  |

Layer facts:
- OCI specifications are layer 01's single standard. Every runtime, engine, and registry here reads and writes them `[workloads-github]`
- runc is the reference runtime `[workloads-github]`
- gVisor puts "an application kernel" between container and host `[workloads-github]`
- Kata Containers gives each container a small virtual machine `[workloads-github]`
- Docker Engine is Apache-2.0 `[workloads-github]`
- Docker Desktop is proprietary and needs a paid subscription in larger organizations `[docker-desktop-license]`
- Docker Desktop's free use ends at 250 employees or 10 million US dollars in annual revenue `[docker-desktop-license]`
- Colima (MIT) supplies Docker Engine on macOS inside a Lima virtual machine `[devmachine-github]`
- Docker Compose is "a tool for defining and running multi-container applications" on one host, from a YAML file `[workloads-github]`
- Kubernetes v1.37 is its newest minor, k3s and k0s are distributions in CNCF Sandbox `[workloads-github]`
- Talos Linux is an operating system built to run Kubernetes `[workloads-github]`
- Nomad is BUSL-1.1 with IBM as licensor since IBM completed its HashiCorp acquisition on 2025-02-27 `[nomad-license]` `[ibm-hashicorp]`
- Docker Swarm mode is maintained in `moby/swarmkit`, and Mirantis commits to support it through 2030 inside its Kubernetes Engine product `[swarm]`
- Komodo (GPL-3.0) builds and deploys across many servers from one interface `[workloads-github]`
- Portainer (Zlib) is a management interface for Docker, Swarm, and Kubernetes `[workloads-github]`

Kubernetes names every workload shape, and other schedulers express each one some way `[k8s-workloads]`:

| [INDEX] | [KIND]      | [RUNS]                                                |
| :-----: | :---------- | :---------------------------------------------------- |
|  [01]   | Deployment  | Interchangeable copies of a stateless service         |
|  [02]   | StatefulSet | Pods with a fixed identity and their own volume each  |
|  [03]   | DaemonSet   | One copy on every host (log collector, network agent) |
|  [04]   | Job         | Pods retried until a stated count finishes            |
|  [05]   | CronJob     | Jobs on a repeating schedule                          |

Stateful servers need more than a kind. Kubernetes operators add code that knows one product: an operator creates volumes, takes backups, upgrades versions, and fails over for a database declared as one custom resource `[k8s-operator]`.

Daemons are long-running background processes: systemd's manual defines one as "a service process that runs in the background and supervises the system or provides functionality to other processes" `[systemd-daemon]`. Services and workers of section 02 are daemons, and so are helpers beside them (a log collector, a metrics agent). A daemon is a workload shape and no product category. Each level has its own supervisor, which starts a daemon, restarts it after a crash, and collects its output:

| [INDEX] | [LEVEL]            | [SUPERVISOR]                        | [DAEMON_DECLARED_AS]                                          |
| :-----: | :----------------- | :---------------------------------- | :------------------------------------------------------------ |
|  [01]   | Linux host         | systemd                             | Unit file                                                     |
|  [02]   | macOS machine      | launchd                             | Launch daemon for a system, launch agent for a signed-in user |
|  [03]   | Developer session  | process-compose                     | Entry in that tool's file of processes                        |
|  [04]   | Docker Engine      | Docker daemon with a restart policy | Container or Compose service                                  |
|  [05]   | Inside a container | Container's main process            | Child process, with `--init` for a small init                 |
|  [06]   | Kubernetes         | Controllers                         | Deployment, DaemonSet per host, sidecar container per pod     |
|  [07]   | Hosted platform    | Platform                            | Service or worker process type                                |

Supervisor facts:
- Twelve-factor processes "should never daemonize or write PID files" and rely on a process manager for output, crashes, and restarts `[12f-concurrency]`
- launchd agents are "essentially identical to a daemon" and run only while their user is signed in `[launchd]`
- Docker restart policies are `no`, `on-failure`, `always`, and `unless-stopped` `[docker-restart]`
- Docker recommends restart policies over process managers for starting containers `[docker-restart]`
- Docker's practice is one service per container, with `--init` adding a small init that reaps child processes `[docker-multi]`
- Docker calls that small init superior to running `sysvinit` or `systemd` inside a container `[docker-multi]`
- Kubernetes DaemonSets run "a logs collection daemon on every node" or a monitoring daemon `[k8s-daemonset]`
- Kubernetes sidecar containers run beside one app container for logging, monitoring, or data synchronization `[k8s-sidecar]`
- process-compose (Apache-2.0) supervises non-containerized processes on a developer machine `[supervisors-github]`
- Descriptions name a daemon once (image or command, restart rule, config), and a level's native supervisor does everything else

Hosted platforms differ in what unit they run and whether idle units cost nothing:

| [INDEX] | [PLATFORM]            | [UNIT]                                       | [IDLE]                                                      |
| :-----: | :-------------------- | :------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Fly.io Machines       | Fast-launching virtual machine from an image | Proxy stops machines with no traffic, starts one on request |
|  [02]   | Railway               | Container from an image                      | Opt-in serverless setting                                   |
|  [03]   | Render                | Web service or Docker image                  | Free tier stops after 15 minutes without traffic            |
|  [04]   | Google Cloud Run      | Container, function, or source               | Last instance removed with no requests                      |
|  [05]   | AWS ECS on Fargate    | Container without managed servers            | Unverified, see section 18                                  |
|  [06]   | AWS Lambda            | Function                                     | Scales to zero                                              |
|  [07]   | Cloudflare Workers    | V8 isolate, no container or virtual machine  | Unverified, see section 18                                  |
|  [08]   | Cloudflare Containers | Container attached to a Worker               | Charges stop when an instance sleeps                        |
|  [09]   | Vercel Functions      | Function                                     | Scales to zero                                              |

Rows rest on `[fly-machines]`, `[railway]`, `[render]`, `[cloud-run]`, `[fargate]`, `[lambda]`, `[cf-workers]`, `[cf-containers]`, and `[vercel-functions]`.

Self-hosted platforms of layer 06:

| [INDEX] | [PLATFORM] | [LICENSE]                                   | [FORM]                                                                  |
| :-----: | :--------- | :------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | Coolify    | Apache-2.0                                  | Web interface, needs SSH to each server, hosted control plane sold too  |
|  [02]   | Dokploy    | Apache-2.0, `/proprietary` source available | Web interface, hosted control plane sold too                            |
|  [03]   | Kamal      | MIT                                         | Command-line tool from 37signals, deploys over SSH behind `kamal-proxy` |
|  [04]   | Dokku      | MIT                                         | Docker-powered mini-Heroku                                              |
|  [05]   | CapRover   | Apache-2.0                                  | Web interface for apps and databases                                    |

Rows rest on `[workloads-github]` and `[kamal]`.

Docker Engine runs on a Linux kernel. Linux machines install it from packages, and a Mac supplies that kernel through a virtual machine. That machine-local piece is a development tool and no part of a server:

| [INDEX] | [TOOL]          | [LICENSE]   | [RUNS_ON]    | [SUPPLIES]                                                          |
| :-----: | :-------------- | :---------- | :----------- | :------------------------------------------------------------------ |
|  [01]   | Colima          | MIT         | macOS, Linux | Lima virtual machine with Docker or containerd, optional Kubernetes |
|  [02]   | Rancher Desktop | Apache-2.0  | macOS, Linux | Container management and Kubernetes on a desktop                    |
|  [03]   | Docker Desktop  | Proprietary | macOS, Linux | One-click application that includes Docker Engine                   |

Rows rest on `[devmachine-github]` and `[docker-desktop-license]`. Colima (containers on Lima) is macOS's open-source route. Linux servers and Linux laptops skip this table. kind and minikube run a throwaway Kubernetes cluster on a laptop for testing `[devmachine-github]`.

One host or a cluster is a property of where workloads run. An app's parts and its backing-service needs stay as they are, and what fulfils each concern changes with that choice:

| [INDEX] | [CONCERN]             | [ONE_HOST]                                      | [KUBERNETES]                                      |
| :-----: | :-------------------- | :---------------------------------------------- | :------------------------------------------------ |
|  [01]   | Description of a unit | Compose service                                 | Deployment, StatefulSet, Job                      |
|  [02]   | Routing               | Reverse proxy reading files or container labels | Gateway API implementation: Envoy Gateway, Cilium |
|  [03]   | Certificates          | ACME client inside that proxy                   | cert-manager                                      |
|  [04]   | DNS records           | DNSControl, OctoDNS, or a provisioning tool     | external-dns                                      |
|  [05]   | PostgreSQL            | Container or package with a volume              | CloudNativePG or another operator                 |
|  [06]   | Volumes               | Docker volumes on local or provider disks       | Longhorn, Rook-Ceph, provider volume drivers      |
|  [07]   | Secrets               | Files, SOPS, or a secrets product's agent       | External Secrets Operator, sealed-secrets         |
|  [08]   | Telemetry collection  | Collector container                             | OpenTelemetry Operator                            |
|  [09]   | Deployment            | Kamal, Komodo, Doco-CD                          | Argo CD, Flux, Helm                               |
|  [10]   | Backup                | Restic, offen/docker-volume-backup              | Velero                                            |
|  [11]   | Loss of a host        | Its workloads stop until it returns             | Pods are created again on other hosts             |

Each product named here has its facts in this map's section for that concern, and row 11 rests on `[k8s-workloads]`. Consequences:
- Scheduler choice sits between hosts and apps. It is made once per group of hosts, and every concern above then takes that column's form
- Kubernetes does not require many machines: k3s documents a single-server setup with an embedded SQLite datastore `[k3s-arch]`
- Highly available k3s control planes take 3 or more servers `[k3s-arch]`
- Kubernetes adds categories with no one-host form: admission policy (Kyverno, OPA Gatekeeper) and cost allocation (OpenCost) `[extras-github]`
- KEDA (Apache-2.0) adds event-driven autoscaling on Kubernetes `[extras-github]`
- Knative (Apache-2.0) adds request-driven serving on Kubernetes `[extras-github]`
- Application models keep app descriptions out of this choice
- Score translates one file to Compose or Kubernetes, and Radius picks a recipe per environment (section 14)

Choice in this area: layer 01 has one standard. Layer 05 has one option under a foundation with certified distributions and hosted forms in CNCF's landscape, Kubernetes, beside Nomad under a vendor license and Docker Swarm mode `[cncf-landscape]`. Layers 06 and 07 are menus, and an app that fits one host needs layer 04 or 06 and no cluster.

Self-hosting a scheduler means updating it, rotating its certificates, and backing up its state. Hosted Kubernetes runs that control plane for you and leaves hosts, workloads, and data with you. Hosted platforms take hosts as well.

## [05]-[NETWORK]

Network gets a request from a user's device to a process, and connects your own machines and people privately. Public requests pass these parts in order:

| [INDEX] | [PART]            | [DOES]                                                | [PRODUCTS]                                                  |
| :-----: | :---------------- | :---------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Registrar         | Rents a domain name from its registry                 | Any ICANN-accredited registrar                              |
|  [02]   | Authoritative DNS | Answers which address a name has                      | Cloudflare DNS, Route 53, Google Cloud DNS, deSEC, PowerDNS |
|  [03]   | Edge network      | Caches, filters attacks, absorbs floods near users    | Cloudflare, Fastly, Bunny.net, Amazon CloudFront, Akamai    |
|  [04]   | Load balancer     | Spreads connections across hosts                      | AWS ALB and NLB, Hetzner Load Balancer, HAProxy             |
|  [05]   | Reverse proxy     | Ends TLS, picks a site by hostname, forwards requests | nginx, Caddy, Traefik, HAProxy, Envoy                       |
|  [06]   | Certificate       | Proves a name to browsers                             | Let's Encrypt, ZeroSSL, Google Trust Services over ACME     |

Registrar and DNS host are separate roles: a registrar reserves a name, and its only link to DNS is a list of authoritative nameservers, which can belong to another company `[icann-registrar]` `[cf-dns-faq]`.

Reverse proxies are where nginx belongs:
- nginx describes itself as "an HTTP web server, reverse proxy, content cache, load balancer, TCP/UDP proxy server, and mail proxy server" `[nginx]`
- nginx is a workload, a server program running on a host or in a container. It defines no machine
- nginx's unit is a site: one `server` block per set of hostnames, selected by each request's `Host` header `[nginx-request]`
- `server` blocks hold where files are or which app receives requests `[nginx-request]`
- Caddy calls that unit a site block, which starts with a site address and holds directives (`reverse_proxy`, `file_server`) `[caddy-concepts]`
- Traefik sets entrypoints at start and reads routing from providers: container labels, an orchestrator, files `[traefik-concepts]`
- Traefik reloads routing without restart `[traefik-concepts]`
- HTTP requests name their site in a `Host` header, and nginx calls each site a virtual server `[nginx-request]`
- This map uses host for a machine
- One machine serves many sites, and one site can span many machines
- Web server, reverse proxy, load balancer, ingress controller, and API gateway name roles of this one category
- Kong and Apache APISIX (both Apache-2.0) are sold as API gateways, a subcategory of its own in CNCF's landscape `[extras-github]` `[cncf-landscape]`
- Row 04 is its role across machines, row 03 its role at a vendor's locations worldwide
- Site rows hold hostnames, a certificate source, an upstream address or a file root, and access rules
- Hosts, apps, and DNS records are other rows that a site refers to

Peers in row 05:

| [INDEX] | [PRODUCT]     | [LICENSE]    | [DISTINCT_TRAIT]                                     |
| :-----: | :------------ | :----------- | :--------------------------------------------------- |
|  [01]   | nginx         | BSD-2-Clause | F5 owns it. Forks: freenginx (2024), Angie           |
|  [02]   | Caddy         | Apache-2.0   | Automatic HTTPS                                      |
|  [03]   | Traefik Proxy | MIT          | Reads routes from container labels and orchestrators |
|  [04]   | HAProxy       | GPL-2.0      | Describes itself as a load balancer                  |
|  [05]   | Envoy         | Apache-2.0   | CNCF graduated, Envoy Gateway manages it             |
|  [06]   | Pingora       | Apache-2.0   | Rust library from Cloudflare for building a proxy    |

Rows rest on `[network-github]`, `[freenginx]`, and `[caddy-concepts]`. Inside Kubernetes this role is an implementation of Gateway API (v1.7.0). ingress-nginx, long its common form, was retired in March 2026 and its repository archived `[gateway-api]` `[ingress-nginx-retirement]`.

Certificates run on one protocol and shrinking lifetimes:
- ACME is RFC 8555. A client proves control of a name and receives a certificate without a person involved `[acme]`
- Let's Encrypt issues 90-day certificates by default, moving to 64 days on 2027-02-10 and 45 days on 2028-02-16 `[le-45]`
- Let's Encrypt has issued 160-hour certificates and certificates for IP addresses since 2026-01-15, both opt-in `[le-shortlived]`
- CA/Browser Forum ballot SC-081v3 caps public certificates at 200 days from 2026-03-15 and 100 days from 2027-03-15 `[cabf-sc081]`
- SC-081v3's cap falls to 47 days on 2029-03-15 `[cabf-sc081]`
- Caddy and Traefik hold an ACME client `[network-github]`
- Certbot, acme.sh, and lego are standalone clients, cert-manager is one for Kubernetes `[network-github]`
- step-ca is an ACME server for private names `[network-github]`

Private connectivity has its own categories:

| [INDEX] | [CATEGORY]               | [DOES]                                               | [PRODUCTS]                                            |
| :-----: | :----------------------- | :--------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Provider private network | Links servers inside one provider                    | Hetzner Cloud Networks, AWS VPC                       |
|  [02]   | Overlay network          | Encrypted links between machines and people anywhere | Tailscale, Headscale, NetBird, Nebula, ZeroTier       |
|  [03]   | Tunnel                   | Publishes a private service over an outbound link    | Cloudflare Tunnel, ngrok, Pangolin, frp               |
|  [04]   | Remote access            | Shell on a machine tied to an identity               | Tailscale SSH, Teleport, OpenSSH, AWS Session Manager |
|  [05]   | Service mesh             | Proxies between services for encryption and routing  | Istio, Linkerd, Cilium, Consul                        |

Facts that decide among them:
- Tailscale's client daemon is BSD-3-Clause and its coordination server is closed `[tailscale-opensource]`
- Headscale is an open implementation of that server, scoped by its own FAQ to one network for personal use or a small organization `[headscale-faq]`
- Tailscale SSH takes over port 22 and lets network policy decide who may log in `[tailscale-ssh]`
- Workload identity federation lets a CI job join with its provider's OIDC token and no stored key `[tailscale-wif]`
- NetBird's client is BSD-3-Clause and its server parts are AGPL-3.0 since 2025-08-05 `[netbird-license]`
- ZeroTier's controller is commercial source-available since 2025-08-21 `[zerotier-116]`
- Teleport's source is AGPL-3.0, and its Community Edition binaries carry a commercial license since version 16 `[teleport-license]`
- HashiCorp Boundary Community Edition was archived on 2026-10-08. Consul is BUSL-1.1 `[boundary-archive]` `[network-github]`
- Linkerd's open-source project has published weekly edge releases only since 2.15, stable builds come from vendors `[linkerd-215]`

Rows 01 and 03 of the first table are bought: registrars need ICANN accreditation and edge networks need locations worldwide. Row 02 is bought or self-hosted on PowerDNS, Knot DNS, NSD, BIND 9, or CoreDNS. Rows 05 and 06 run on your hosts or come inside a platform (section 04). DNS records, sites, and certificates are all small declarative lists, and OctoDNS, DNSControl, and external-dns manage records from files or from Kubernetes resources `[network-github]`.

## [06]-[STATE]

State is every stored byte. Shape, workload, and form place a data store, and products are peers only when every property matches:
- Data shape: relational rows, documents, key-value pairs, wide columns, graphs, time series, text indexes, vectors, or opaque objects
- Workload: transactional work reads and writes single rows in milliseconds, analytical work computes over large parts of a dataset `[clickhouse-olap]`
- Form: a library inside your process, a server, a cluster, or a cloud service with no downloadable form

SQL engines by workload and form:

| [INDEX] | [FORM]             | [TRANSACTIONAL]                        | [ANALYTICAL]                                              |
| :-----: | :----------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Library in process | SQLite, Turso Database, PGlite         | DuckDB, chDB                                              |
|  [02]   | Server             | PostgreSQL, MySQL, MariaDB, SQL Server | ClickHouse, DuckDB behind Quack                           |
|  [03]   | Cluster            | YugabyteDB, TiDB, Vitess, CockroachDB  | ClickHouse, Apache Doris, StarRocks, Druid, Pinot         |
|  [04]   | Cloud service only | Aurora DSQL, Spanner                   | Snowflake, BigQuery, Redshift, Databricks SQL, MotherDuck |

PostgreSQL and DuckDB share SQL and sit in opposite cells:
- PostgreSQL is a transactional server: many clients connect over a socket and change single rows under ACID guarantees `[pg-about]`
- DuckDB targets analytical queries "that process significant portions of the stored dataset" and stores columns `[duckdb-why]`
- By default DuckDB "does not run as a separate process but is completely embedded within a host process" `[duckdb-why]`
- DuckDB names SQLite as its model for form, and ClickHouse states DuckDB's workload, OLAP, for a server `[duckdb-why]` `[clickhouse-intro]`
- One process at a time writes a DuckDB file `[duckdb-concurrency]`
- Writes from other processes go through Quack, a client-server protocol released with DuckDB 2.0, or through DuckLake `[duckdb-quack]`
- Apps use both without conflict: PostgreSQL holds records, DuckDB computes over exports, Parquet files, or a lake
- DuckLake can keep its catalog in that same PostgreSQL `[ducklake]`
- chDB is ClickHouse's engine as an in-process library `[analytics-github]`
- Apache DataFusion and Polars are query engine libraries with no database file of their own `[analytics-github]`

ClickHouse fills analytical rows 02 and 03 and is not a system of record:
- ClickHouse is "a column-oriented SQL database management system (DBMS) for online analytical processing (OLAP)" `[clickhouse-intro]`
- Column storage makes single-row retrieval slower and filters and aggregations over few columns faster `[clickhouse-intro]`
- Updates and deletes exist as patch parts, rewrites of data parts, or row marks applied at a later merge `[clickhouse-updates]`
- Transactions over more than one statement are experimental and exclude replicated tables `[clickhouse-transactions]`
- ClickHouse Cloud runs a storage engine (SharedMergeTree) that open-source releases lack `[clickhouse-cloud]`

Categories of data store:

| [INDEX] | [CATEGORY]             | [HOLDS]                                        | [PRODUCTS]                                                 |
| :-----: | :--------------------- | :--------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Relational             | Records an app must never lose or contradict   | PostgreSQL, MySQL, MariaDB, SQLite                         |
|  [02]   | Document               | JSON-like documents with no fixed columns      | MongoDB, DocumentDB, FerretDB, CouchDB                     |
|  [03]   | Key-value, wide column | Values under a key, spread over many machines  | DynamoDB, Cassandra, ScyllaDB, FoundationDB, etcd          |
|  [04]   | In-memory              | Cache entries, sessions, counters, rate limits | Valkey, Redis, Dragonfly, Memcached, Garnet                |
|  [05]   | Analytical             | Events and facts queried in bulk               | ClickHouse, DuckDB, Doris, StarRocks, Snowflake, BigQuery  |
|  [06]   | Lake                   | Open-format files on object storage            | Iceberg, Delta Lake, DuckLake with a catalog and an engine |
|  [07]   | Time series            | Timestamped business or device measurements    | TimescaleDB, InfluxDB 3, QuestDB, TDengine                 |
|  [08]   | Search                 | Inverted indexes over text                     | Elasticsearch, OpenSearch, Meilisearch, Typesense, Solr    |
|  [09]   | Vector                 | Embeddings for nearest-neighbor lookup         | pgvector, Qdrant, Milvus, Weaviate, Chroma, LanceDB        |
|  [10]   | Graph                  | Nodes and relationships                        | Neo4j, Memgraph, FalkorDB, Dgraph, Apache AGE              |
|  [11]   | Object storage         | Files of any size under a key                  | Amazon S3 and every service that speaks its API            |
|  [12]   | Block and file         | Disks and shared folders for servers           | Provider volumes, local NVMe, NFS, Ceph                    |

"NoSQL" names no category. It lumps rows 02, 03, 04, and 10, which solve different problems.

PostgreSQL is row 01's clear choice and reaches into other rows:
- Stack Overflow's 2026 survey has PostgreSQL used by 57.9 percent of respondents, ahead of SQLite at 42.1 and MySQL at 35.1 `[so-2026]`
- PostgreSQL 18 is its released major, each major is supported for 5 years, and PostgreSQL 19 is due on 2026-11-19 `[pg-versioning]` `[pg-19rc2]`
- OpenAI serves ChatGPT from one primary PostgreSQL with nearly 50 read replicas, and moved shardable write-heavy workloads elsewhere `[openai-pg]`
- Built-in types cover documents (`jsonb`) and full-text search `[pg-json]` `[pg-textsearch]`
- Extensions cover more rows inside one server:

| [INDEX] | [ROW]                | [EXTENSION]             | [LICENSE]                        | [PRELOAD] |
| :-----: | :------------------- | :---------------------- | :------------------------------- | :-------- |
|  [01]   | Vector               | pgvector, pgvectorscale | PostgreSQL                       | No        |
|  [02]   | Time series          | TimescaleDB             | Apache-2.0 and Timescale License | Yes       |
|  [03]   | Search               | `pg_search` (ParadeDB)  | AGPL-3.0                         | Yes       |
|  [04]   | Graph                | Apache AGE              | Apache-2.0                       | No        |
|  [05]   | Document             | DocumentDB              | MIT                              | Not read  |
|  [06]   | Analytical           | `pg_duckdb`             | MIT                              | Yes       |
|  [07]   | Lake                 | `pg_lake`               | Apache-2.0                       | Yes       |
|  [08]   | Geospatial           | PostGIS, pgRouting      | GPL-2.0                          | Not read  |
|  [09]   | Queue                | pgmq                    | PostgreSQL                       | No        |
|  [10]   | Schedule             | `pg_cron`               | PostgreSQL                       | Yes       |
|  [11]   | Partition management | `pg_partman`            | PostgreSQL                       | Optional  |
|  [12]   | Audit                | pgaudit                 | PostgreSQL                       | Yes       |
|  [13]   | Distribution         | Citus                   | AGPL-3.0                         | Yes       |

Rows rest on `[pg-extensions-github]`. Preload means `shared_preload_libraries`, which loads a library at server start and needs a restart to change `[pg-preload]`. A dedicated product of rows 02 to 10 earns its place when a workload outgrows what an extension gives.

Getting PostgreSQL extensions without compiling them:
- PGDG apt and yum repositories package extensions per PostgreSQL major, 114 `postgresql-18-*` packages besides debug symbols on 2026-11-12 `[pgdg-apt]`
- Docker's official image installs those packages on its Debian variant, Alpine variants need a compiled image `[docker-postgres]`
- Pigsty and its `pig` tool package 457 non-contrib extension families `[pig]`
- CloudNativePG mounts an extension as its own image on PostgreSQL 18 and Kubernetes 1.35, with no rebuilt database image `[cnpg-imagevolume]`
- Tembo Trunk shut down on 2025-07-07 and pgxman's last release was in 2024 `[trunk-shutdown]` `[pg-extensions-github]`
- Hosted PostgreSQL offers a fixed list per vendor, Amazon RDS names it in `rds.extensions` `[rds-extensions]`

Parts around a PostgreSQL server:

| [INDEX] | [PART]            | [DOES]                                               | [PRODUCTS]                                              |
| :-----: | :---------------- | :--------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | Pooler            | Shares few server connections among many clients     | PgBouncer, PgDog, Supavisor, Odyssey, pgagroal          |
|  [02]   | High availability | Elects a primary, fails over to a replica            | Patroni, repmgr, `pg_auto_failover`                     |
|  [03]   | Operator          | Runs clusters on Kubernetes                          | CloudNativePG, Crunchy PGO, Zalando operator, StackGres |
|  [04]   | Backup            | Base backups and WAL archive, point-in-time recovery | pgBackRest, Barman, WAL-G                               |
|  [05]   | Schema change     | Applies and checks table changes                     | Atlas, Flyway, Liquibase, Sqitch, pgroll, Squawk        |
|  [06]   | Local transport   | Unix-domain socket locally, TCP with TLS remotely    | `unix_socket_directories`, `listen_addresses`           |

Rows rest on `[pg-ha-github]` and `[pg-connection]`. pgBackRest's author announced on 2026-04-27 that it was no longer maintained and on 2026-05-18 that sponsors fund it again `[pgbackrest-news]`. CloudNativePG is CNCF incubating since 2026-11-02 `[cnpg-cncf]`. Liquibase Community is FSL-1.1 from version 5.0, and Atlas's standard binary holds proprietary features `[liquibase-fsl]` `[atlas-license]`.

PostgREST (MIT) and Hasura (Apache-2.0) publish tables as an HTTP or GraphQL API without app code `[extras-github]`.

Hosted PostgreSQL is a real menu of one engine. Buyers changed in 2025: Databricks acquired Neon, Snowflake acquired Crunchy Data, and PlanetScale added PostgreSQL `[neon-databricks]` `[crunchy-snowflake]` `[planetscale-pg]`. Amazon RDS and Aurora, Google Cloud SQL and AlloyDB, Azure Database for PostgreSQL, Supabase, Tiger Cloud, Xata, Aiven, and DigitalOcean sell it as well, and Neon, Xata, and PlanetScale lead with branching. Hetzner lists none (section 18) `[pg-hosted]`.

Engines that speak PostgreSQL's protocol and are other software: CockroachDB (its own license, source closed to new releases since 2026-09-15), YugabyteDB, Aurora DSQL, and Spanner's PostgreSQL interface `[cockroach-source]` `[pg-compatible]`.

Other rows of the category table:
- MySQL (GPL-2.0, Oracle) and MariaDB (GPL-2.0) are PostgreSQL's server peers `[opsdb-github]`
- SQLite is public domain, and Turso Database is its Rust rewrite `[opsdb-github]`
- MongoDB is SSPL-1.0 `[opsdb-github]`
- DocumentDB is an MIT PostgreSQL extension under Linux Foundation that speaks MongoDB's protocol, and FerretDB builds on it `[documentdb-lf]` `[opsdb-github]`
- ScyllaDB went source available in December 2024, Cassandra stays Apache-2.0, DynamoDB exists only at AWS `[scylla-license]` `[opsdb-github]`
- Redis 8 is offered under RSALv2, SSPLv1, or AGPLv3, versions through 7.2 stay BSD `[redis-license]`
- Valkey is Linux Foundation's BSD-3-Clause fork of Redis 7.2.4, and AWS and Google sell hosted Valkey `[valkey]`
- Dragonfly's license caps production use at 100 vCPUs, KeyDB has had no release since 2023 `[dragonfly-license]` `[opsdb-github]`
- Valkey persists by snapshot (RDB), by write log (AOF), by both, or not at all `[valkey-persistence]`
- Caches restart empty without harm, a primary store cannot `[redis-worst]`
- Redis-protocol stores are backing services at one level, beside a database
- Self-hosted Valkey and hosted ElastiCache, Memorystore, or Upstash answer one protocol, and an app's handle is all that differs `[valkey]` `[opsdb-github]`
- Jobs such a store does: GitLab keeps session data, temporary cache entries, and background job queues in Redis `[gitlab-arch]`
- Row 04 is optional: Plausible and SigNoz run without it `[plausible]` `[signoz-arch]`
- GlitchTip lists Valkey as optional, and Postgres-backed job queues remove its most common use `[glitchtip]` `[messaging-github]`
- TimescaleDB is a PostgreSQL extension from Tiger Data `[timescale-rename]`
- InfluxDB 3 stores Parquet on object storage, QuestDB is Apache-2.0 `[influxdb3]` `[analytics-github]`
- Elasticsearch is offered under AGPL-3.0, SSPL-1.0, or Elastic License 2.0 `[elastic-license]`
- OpenSearch is Apache-2.0 under Linux Foundation `[opensearch-lf]`
- Meilisearch is MIT with sharding under BUSL-1.1, Typesense is GPL-3.0 `[meilisearch-ee]` `[analytics-github]`
- Qdrant, Milvus, Chroma, and LanceDB are Apache-2.0 `[analytics-github]`
- Weaviate added enterprise-licensed code in September 2026 `[weaviate-license]`
- Pinecone and turbopuffer are cloud only `[vector-hosted]`
- Neo4j Community is GPL-3.0, Memgraph is BUSL-1.1, FalkorDB is SSPL-1.0. Kuzu was archived on 2025-10-10 `[opsdb-github]`

Lakes are a layout of parts, each replaceable:

| [INDEX] | [PART]       | [DOES]                                                 | [PRODUCTS]                                              |
| :-----: | :----------- | :----------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | Storage      | Holds files                                            | Any object storage                                      |
|  [02]   | File format  | Encodes columns in a file                              | Apache Parquet, ORC, Lance, Vortex                      |
|  [03]   | Table format | Says which files form a table at one snapshot          | Apache Iceberg, Delta Lake, Hudi, Paimon, DuckLake      |
|  [04]   | Catalog      | Names tables and points at each one's current snapshot | Polaris, Lakekeeper, Unity Catalog, AWS Glue, S3 Tables |
|  [05]   | Engine       | Runs queries                                           | Trino, Spark, DuckDB, ClickHouse, StarRocks, Snowflake  |

Lake terms:
- Data lake: "a central location that holds a large amount of data in its native, raw format" `[databricks-lake]`
- Data warehouse: a system storing current and historical data of many sources for reporting `[databricks-warehouse]`
- Warehouses sold as services keep storage and engine with one vendor `[warehouses]`
- Lakehouse: warehouse management and ACID transactions over lake storage in open file formats `[lakehouse-paper]`
- Iceberg's specification is at version 3, and its REST catalog specification is row 04's interface `[iceberg-spec]` `[iceberg-rest]`
- DuckLake keeps all table metadata in a SQL database (PostgreSQL, MySQL, SQLite, or DuckDB) and data in Parquet `[ducklake]`
- PostgreSQL, a bucket, and DuckDB make a complete lake `[ducklake-manifesto]`
- DuckDB reads and writes Iceberg through a catalog. ClickHouse reads Iceberg and has beta inserts `[duckdb-iceberg]` `[clickhouse-iceberg]`
- Snowflake, BigQuery, Redshift, and Databricks SQL are warehouses sold as services, MotherDuck is one built on DuckDB `[warehouses]`
- Movement and transformation tools feed warehouses and lakes: Airbyte (Elastic-2.0), dlt and dbt Core (Apache-2.0) `[extras-github]`

Lakes fit data that many engines read, that outgrows one database's disks, or that must stay in open files `[ducklake-manifesto]`. They are a second home for data, behind a transactional database that takes writes first.

Storage under every row is object, block, or file: object storage keeps whole objects in a flat namespace, block storage presents fixed-size blocks as a disk, and file storage shares a file system between servers `[aws-storage-types]`.

Object storage has one interface and many sellers:
- Amazon S3's API is what "S3-compatible" refers to `[s3-compat]`
- No standards body defines it, and each vendor publishes a table of supported operations `[s3-compat]`
- S3 gives strong read-after-write consistency `[s3-consistency]`
- S3 accepts conditional writes (`If-None-Match`), which lake table formats and backup tools build on `[s3-conditional]`
- Hosted sellers speaking it: Cloudflare R2, Backblaze B2, Wasabi, Tigris, Hetzner, OVHcloud, Scaleway, DigitalOcean Spaces `[object-hosted]`
- R2 charges no egress fee, B2 gives free egress up to 3 times stored volume `[object-hosted]`
- Google Cloud Storage speaks a subset through its XML API `[gcs-interop]`
- Azure Blob Storage has its own API and needs a gateway for S3 clients `[azure-s3]`
- Documented gaps differ: R2 omits ACL and Object Lock headers and has its own bucket locks `[r2-s3]`
- DigitalOcean Spaces omits tag-based lifecycle rules `[spaces-s3]`
- Object Lock makes objects undeletable for a retention period `[object-hosted]`
- S3, B2, Wasabi, Hetzner, OVHcloud, Scaleway, and IDrive e2 document it `[object-hosted]`

Self-hosted object storage changed in 2025 and 2026:

| [INDEX] | [PRODUCT]          | [LICENSE]        | [STATUS]                                                                                    |
| :-----: | :----------------- | :--------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | MinIO              | AGPL-3.0         | Repository archived, README says no longer maintained, successor AIStor needs a license key |
|  [02]   | Garage             | AGPL-3.0         | Deuxfleurs non-profit, built for small geo-distributed clusters, no erasure coding          |
|  [03]   | SeaweedFS          | Apache-2.0       | Objects, files, Iceberg table buckets                                                       |
|  [04]   | RustFS             | Apache-2.0       | Release 1.0.5, README states no maturity level                                              |
|  [05]   | Ceph RGW with Rook | LGPL, Apache-2.0 | Object, block, and file from one cluster, Rook is CNCF graduated                            |
|  [06]   | Versity S3 Gateway | Apache-2.0       | S3 API over an existing file system                                                         |
|  [07]   | `pgsty/silo`       | AGPL-3.0         | Community fork of MinIO                                                                     |

Rows rest on `[minio-readme]`, `[garage-goals]`, and `[storage-github]`.

Block and file storage follow machines:
- Provider volumes (AWS EBS, Hetzner Volumes) are network disks that outlive a server and attach to one server at a time `[ebs]` `[hetzner-volumes]`
- Local disks of a rented server are fastest and die with it: EC2 instance store data is erased when an instance stops `[ec2-instance-store]`
- Docker volumes are "persistent data stores for containers", bind mounts map a host directory into a container `[docker-volumes]`
- Kubernetes clusters add a storage layer that replicates volumes between hosts: Longhorn (CNCF incubating), OpenEBS, Rook-Ceph `[storage-github]`
- ZFS and Btrfs snapshot a file system in place, and Btrfs docs warn "a snapshot is not a backup" `[btrfs]`

## [07]-[MESSAGING]

Messaging covers work an app does outside a request and data programs pass to each other. Its categories share words (queue, job, event, workflow) and differ in what they store and who reads it:

| [INDEX] | [CATEGORY]          | [HOLDS]                                              | [PRODUCTS]                                                |
| :-----: | :------------------ | :--------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Message queue       | Messages each handed to one consumer, then removed   | RabbitMQ, NATS JetStream, pgmq, Amazon SQS                |
|  [02]   | Event log           | Append-only records, each reader at its own position | Apache Kafka, Redpanda, AutoMQ, WarpStream, Apache Pulsar |
|  [03]   | Job queue           | Function calls with arguments, retries, run times    | River, pg-boss, Oban, Solid Queue, BullMQ, Sidekiq        |
|  [04]   | Durable execution   | Multi-step functions with each finished step saved   | Temporal, Restate, Inngest, DBOS Transact, Hatchet        |
|  [05]   | Data orchestration  | Scheduled graphs of data tasks                       | Apache Airflow, Dagster, Prefect, Kestra                  |
|  [06]   | Schedule            | Times at which a job starts                          | cron, systemd timers, Kubernetes CronJob, `pg_cron`       |
|  [07]   | Change data capture | Database commits turned into an event stream         | Debezium, PeerDB                                          |
|  [08]   | Webhooks            | HTTP calls between companies when an event happens   | Svix, Hookdeck Outpost, Convoy                            |
|  [09]   | Client delivery     | Pushes to browsers and devices, synced local data    | Centrifugo, Mercure, Supabase Realtime, ElectricSQL, Zero |
|  [10]   | Device messaging    | MQTT topics for sensors and vehicles                 | Mosquitto, EMQX                                           |

Queue and log are different stores:
- Kafka docs: in a queue "each record goes to one" consumer and "once one process reads the data it's gone" `[kafka-intro]`
- Publish-subscribe broadcasts each record to all consumers, and Kafka's consumer groups cover both `[kafka-intro]`
- RabbitMQ docs: streams "model an append-only log of messages that can be repeatedly read until they expire" `[rabbitmq-streams]`
- Products cross over: RabbitMQ has streams, NATS JetStream adds persistence and replay to NATS `[rabbitmq-streams]` `[nats-jetstream]`
- Kafka 4.2 made share groups, queue consumption of a topic, production-ready `[kafka-42]`
- Kafka's protocol is the interface of row 02 `[messaging-github]`
- Redpanda, AutoMQ, and WarpStream implement it over other storage, AutoMQ and WarpStream directly on object storage `[messaging-github]` `[warpstream]`

Job queues and durable execution are libraries or services for your own code, where rows 01 and 02 are transport between programs:
- Job queues run inside an app's language and keep jobs in a store that app has `[messaging-github]`
- pgmq is a message queue inside PostgreSQL, "like AWS SQS and RSMQ but on Postgres" `[messaging-github]`
- Solid Queue describes itself as a "database-backed Active Job backend" `[messaging-github]`
- Postgres-backed job libraries (River, graphile-worker, pg-boss, Oban, GoodJob, Solid Queue) add no backing service `[messaging-github]`
- Redis-backed queues (Sidekiq, BullMQ) add an in-memory store `[messaging-github]`
- Durable execution saves each step so a crashed function resumes after its last finished step `[temporal-durable]`
- Temporal: it "guarantees that code runs to completion, regardless of hardware reliability" or network stability `[temporal-durable]`
- Each engine needs a store underneath: Temporal takes Cassandra, MySQL, or PostgreSQL `[temporal-persistence]`
- DBOS Transact and Hatchet take PostgreSQL, Restate keeps its own replicated log `[dbos-arch]` `[hatchet-arch]` `[restate-server]`
- Trigger.dev takes PostgreSQL, Redis, ClickHouse, and object storage `[trigger-selfhost]`
- No shared protocol exists among durable execution engines, moving between them rewrites workflow code

Workers are processes, and no backing service carries that name:
- Twelve-factor assigns each kind of work to a process type: web for HTTP requests, worker for background tasks `[12f-concurrency]`
- Workers are made of your job code, a job library for its language (row 03), and a store that library keeps jobs in
- Sidekiq "pulls jobs from the Redis queue", Postgres-backed libraries reuse an app's database `[gitlab-arch]` `[messaging-github]`
- Deployments describe a worker as one more process of an app's image with its own command and count
- Worker config holds handles that its app's web process holds too
- Hosted function platforms own both queue and invocation `[cf-queues]`
- Cloudflare Queues hand messages to a consumer Worker bound to a queue, and producers write through a binding `[cf-queues]`
- Self-hosted background work is a library with a store and a process, hosted is a platform's queue and function
- No interface is shared between them, job code targets one or a wrapper over both

Status and license facts:

| [INDEX] | [PRODUCT]                        | [LICENSE]        | [FACT]                                                              |
| :-----: | :------------------------------- | :--------------- | :------------------------------------------------------------------ |
|  [01]   | Apache Kafka                     | Apache-2.0       | 4.0 removed ZooKeeper mode on 2025-03-18, newest tag 4.4.0          |
|  [02]   | Redpanda                         | Source available | Source repository archived, README and docs name different licenses |
|  [03]   | WarpStream                       | Proprietary      | Confluent acquired it on 2024-09-09, runs in your cloud account     |
|  [04]   | NATS                             | Apache-2.0       | CNCF graduated it on 2026-11-02, after a 2025 dispute with Synadia  |
|  [05]   | RabbitMQ                         | MPL-2.0          | Broadcom owns its trademarks                                        |
|  [06]   | Temporal, DBOS Transact, Hatchet | MIT              | Temporal is a server, DBOS Transact a library, Hatchet a server     |
|  [07]   | Restate                          | BUSL-1.1         | Grant bars offering it as a public service                          |
|  [08]   | Inngest                          | SSPL-1.0         | Each release converts to Apache-2.0 later                           |
|  [09]   | Trigger.dev                      | Apache-2.0       | Self-hosted form lacks checkpoints                                  |
|  [10]   | Debezium                         | Apache-2.0       | Moved to Commonhaus Foundation in 2024                              |
|  [11]   | PeerDB                           | AGPL-3.0         | ClickHouse owns it, license changed from ELv2 on 2026-01-13         |
|  [12]   | Sequin                           | MIT              | Notion acquired its team in 2025, last commit 2026-02-23            |
|  [13]   | ElectricSQL                      | Apache-2.0       | Databricks acquired it on 2026-08-11                                |
|  [14]   | EMQX                             | BUSL-1.1         | Changed with 5.9 in May 2025, grant covers one production node      |
|  [15]   | Convoy                           | Elastic-2.0      | Changed on 2024-08-23                                               |

Rows rest on `[messaging-github]`, `[kafka-40]`, `[redpanda-readme]`, `[redpanda-licensing]`, `[warpstream-confluent]`, `[nats-cncf]`, `[debezium-commonhaus]`, `[electric-databricks]`, `[sequin-notion]`, `[cncf-landscape]`, and `[emqx-busl]`.

Formats have standards where transports do not: CloudEvents 1.0.2 describes an event's envelope and graduated at CNCF on 2024-01-25, Standard Webhooks v1.0.2 fixes webhook signing and headers, and AsyncAPI 3.1.0 documents message-driven interfaces `[cloudevents]` `[std-webhooks]` `[asyncapi]`.

Stream processing computes over an event log as records arrive, with Apache Flink and RisingWave (both Apache-2.0) as open-source engines `[extras-github]`. Strimzi (Apache-2.0) is Kafka's operator on Kubernetes `[extras-github]`.

Amazon SQS, SNS, Kinesis, Google Pub/Sub, Azure Service Bus, and Cloudflare Queues exist only as cloud services with their own APIs. Self-hosted work for a broker is disk for retention, replication across hosts, and version updates. Confluent Cloud and Redpanda Cloud sell hosted Kafka-protocol service `[warpstream-confluent]` `[redpanda-licensing]`.

## [08]-[IDENTITY]

Identity answers who or what is calling and what it may do. Its categories answer different questions and are not substitutes:

| [INDEX] | [CATEGORY]        | [ANSWERS]                                       | [STANDARD]                                      |
| :-----: | :---------------- | :---------------------------------------------- | :---------------------------------------------- |
|  [01]   | Identity provider | Which person signed in to your product          | OpenID Connect, OAuth 2, SAML, WebAuthn, SCIM   |
|  [02]   | Access proxy      | May this person open an internal tool           | Forward auth in a reverse proxy                 |
|  [03]   | Authorization     | May this user do this action on this object     | None, Zanzibar is a common model                |
|  [04]   | Machine identity  | Which workload is calling                       | OIDC token exchange, SPIFFE                     |
|  [05]   | Secrets           | Where credentials sit and how programs get them | None                                            |
|  [06]   | Keys              | What encrypts data and other keys               | None, envelope encryption is common `[aws-kms]` |

Row 01 comes as a server, a library, or a service:

| [INDEX] | [FORM]             | [USERS_STORED_IN]   | [PRODUCTS]                                                                           |
| :-----: | :----------------- | :------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | Self-hosted server | Its own database    | Keycloak, authentik, ZITADEL, Ory Kratos and Hydra, Logto, Kanidm, Rauthy, Pocket ID |
|  [02]   | Library in an app  | Your app's database | Better Auth, ASP.NET Core Identity, OpenIddict                                       |
|  [03]   | Hosted service     | Vendor              | Auth0, Clerk, WorkOS, Stytch, Amazon Cognito, Entra External ID, Supabase Auth       |

Facts about row 01:
- Keycloak is Apache-2.0 and CNCF incubating since 2023-04-10 `[identity-github]` `[keycloak-cncf]`
- ZITADEL changed from Apache-2.0 to AGPL-3.0 with v3 on 2025-05-02. authentik is MIT outside its enterprise folder `[zitadel-v3]` `[identity-github]`
- Auth.js has been maintained by Better Auth's team since 2025-09-22, and its README sends new projects to Better Auth `[authjs-betterauth]`
- Lucia was deprecated in March 2025 `[identity-github]`
- Duende IdentityServer is proprietary with a free edition under revenue limits `[duende]`
- Okta owns Auth0, Twilio owns Stytch since 2025-11-14 `[auth0]` `[stytch-twilio]`
- OAuth 2.1 remains an IETF draft. WebAuthn Level 3 is a W3C Recommendation and is what passkeys run on `[oauth21]` `[webauthn3]`

Row 02 is a different product from row 01 even where one name does both. Traefik's ForwardAuth "delegates authentication to an external service" and grants access on a 2XX answer, and Caddy's `forward_auth` documents Authelia and authentik as that service `[traefik-forwardauth]` `[caddy-forwardauth]`. Authelia is "the Single Sign-On Multi-Factor portal for web apps", oauth2-proxy and Pomerium sit in front of an app and ask an identity provider, and Cloudflare Access, Google Identity-Aware Proxy, and Tailscale Serve are hosted forms `[identity-github]` `[cf-access]` `[google-iap]` `[tailscale-serve]`.

Row 03 engines keep permission logic out of app code: Open Policy Agent (CNCF graduated, creators joined Apple on 2025-08-20), Cedar (CNCF Sandbox), OpenFGA (CNCF incubating), SpiceDB, Permify (FusionAuth owns it since 2025-11-20), Casbin, and Cerbos. OpenFGA, SpiceDB, and Permify follow Google's Zanzibar paper `[identity-github]` `[opa-apple]` `[permify-fusionauth]` `[zanzibar]`.

Row 04 removes stored credentials between systems. A GitHub Actions job receives a signed OIDC token that a cloud exchanges for "a short-lived cloud access token". Google and Tailscale call the receiving side workload identity federation, PyPI calls it trusted publishing `[gh-oidc]` `[google-wif]` `[pypi-trusted]` `[tailscale-wif]`. SPIFFE and SPIRE (CNCF graduated) issue identities to workloads inside your own systems `[spiffe]`.

Row 05 products:

| [INDEX] | [PRODUCT]                 | [LICENSE]                   | [FORM]                                                      |
| :-----: | :------------------------ | :-------------------------- | :---------------------------------------------------------- |
|  [01]   | HashiCorp Vault           | BUSL-1.1                    | Self-hosted server, IBM is licensor                         |
|  [02]   | OpenBao                   | MPL-2.0                     | Fork of Vault in OpenSSF Sandbox                            |
|  [03]   | Infisical                 | MIT outside `ee/`           | Self-hosted or vendor-hosted                                |
|  [04]   | Doppler                   | Proprietary, CLI Apache-2.0 | Vendor-hosted, on-premises form for enterprises             |
|  [05]   | 1Password                 | Proprietary                 | Service accounts, or a Connect server you run               |
|  [06]   | SOPS with age             | MPL-2.0, BSD-3-Clause       | Encrypted files in a repository, no server                  |
|  [07]   | External Secrets Operator | Apache-2.0                  | Copies secrets from a store into Kubernetes                 |
|  [08]   | Cloud secret stores       | Proprietary                 | AWS Secrets Manager, Google Secret Manager, Azure Key Vault |

Rows rest on `[identity-github]`, `[vault-license]`, `[openbao]`, `[doppler-onprem]`, and `[1password-automation]`. External Secrets Operator paused releases on 2025-08-13 for lack of maintainers and resumed in 2026 `[eso-pause]`.

Standards make row 01 swappable at its protocol: an app that signs users in through OpenID Connect can change provider by changing an issuer URL and client credentials, while user records and password hashes need a migration. Rows 03 and 05 have no shared protocol, each product has its own API.

## [09]-[TELEMETRY]

Telemetry is data a running system emits about itself. Operational telemetry tells engineers whether a system works, product telemetry tells a business what users do, and each has its own category of tools even where one database engine stores both.

Operational telemetry moves through stages:

| [INDEX] | [STAGE]         | [DOES]                                          | [PRODUCTS]                                                          |
| :-----: | :-------------- | :---------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Instrumentation | Code or kernel probes emit signals              | OpenTelemetry SDKs, OBI (eBPF, no code change)                      |
|  [02]   | Collection      | Receives, filters, batches, forwards            | OpenTelemetry Collector, Grafana Alloy, Vector, Fluent Bit, vmagent |
|  [03]   | Storage         | Keeps and indexes each signal                   | Store per signal, or one columnar database, or a vendor             |
|  [04]   | Reading         | Queries and dashboards                          | Grafana, Perses, each store's own interface                         |
|  [05]   | Alerting        | Evaluates rules, routes notifications           | Alertmanager, vmalert, Grafana alerting, PagerDuty, incident.io     |
|  [06]   | Outside checks  | Probes a system from outside, expects check-ins | Uptime Kuma, Gatus, Blackbox exporter, Checkly, Healthchecks.io     |

Stages 01 and 02 have one standard, OpenTelemetry:
- CNCF graduated OpenTelemetry in May 2026 `[otel-graduation]`
- OpenTelemetry generates, collects, and exports telemetry and "is not an observability backend itself" `[otel-what]`
- OTLP, its protocol, is stable for traces, metrics, and logs, and in development for profiles `[otel-status]`
- Profiles reached public alpha on 2026-03-26 `[otel-status]`
- Collector is "a vendor-agnostic proxy that can receive, process, and export telemetry data" and stores nothing `[otel-components]`
- Contrib distribution of Collector adds components written by vendors and community members `[otel-distributions]`
- Grafana Agent reached end of life on 2025-11-01 and Promtail on 2026-03-02 `[grafana-agent-eol]` `[promtail-eol]`
- Grafana Alloy, an OpenTelemetry Collector distribution, replaces both `[grafana-agent-eol]`

Each signal has purpose-built stores:

| [INDEX] | [SIGNAL]       | [HOLDS]                                           | [STORES]                                                        |
| :-----: | :------------- | :------------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | Metrics        | Numeric samples over time, identified by labels   | Prometheus, VictoriaMetrics, Grafana Mimir, Thanos, Cortex      |
|  [02]   | Logs           | Timestamped records of text or fields             | Grafana Loki, VictoriaLogs, OpenSearch, Elasticsearch, Quickwit |
|  [03]   | Traces         | Timed spans of one request across services        | Grafana Tempo, Jaeger, VictoriaTraces                           |
|  [04]   | Profiles       | Sampled call stacks with CPU or memory cost       | Grafana Pyroscope, Parca                                        |
|  [05]   | Errors         | Exceptions grouped with stack and request context | Sentry, GlitchTip, Bugsink                                      |
|  [06]   | Product events | Actions users take, with properties               | PostHog, Plausible, Umami, Matomo, OpenPanel                    |

Storage follows one of these designs:
- Store per signal: Prometheus with Loki, Tempo, and Pyroscope, or VictoriaMetrics with VictoriaLogs and VictoriaTraces. Grafana reads all of them
- One columnar database for every signal: SigNoz, ClickStack, and Uptrace keep signals in ClickHouse `[signoz-arch]` `[clickstack]` `[uptrace]`
- Coroot and qryn keep signals in ClickHouse as well `[coroot-arch]` `[telemetry-github]`
- OpenObserve keeps Parquet on object storage, GreptimeDB uses its own columnar engine `[openobserve-arch]` `[greptime]`
- Vendor: Datadog, Grafana Cloud, Honeycomb, New Relic, Dynatrace, Axiom, Dash0, and Better Stack accept telemetry and run storage themselves

Metric stores are peers of each other and of nothing else:
- Prometheus scrapes targets, stores samples on local disk of one server, and serves PromQL `[prom-overview]`
- Prometheus docs call each server standalone, "not depending on network storage or other remote services" `[prom-overview]`
- Prometheus docs rule it out where "100% accuracy" is needed, per-request billing being their example `[prom-overview]`
- VictoriaMetrics is a metric store with single-node and cluster versions under Apache-2.0 `[vm-docs]`
- VictoriaMetrics accepts Prometheus remote write, OTLP, InfluxDB, and Graphite input `[vm-docs]`
- VictoriaMetrics serves MetricsQL, which is backward-compatible with PromQL `[vm-faq]`
- Grafana Mimir, Thanos, and Cortex keep metrics in object storage for long retention `[mimir-arch]` `[thanos]` `[cortex]`
- Mimir 3.0 added Kafka between its write and read paths `[mimir-arch]`
- Prometheus 3 accepts OTLP at `/api/v1/otlp/v1/metrics` behind `--web.enable-otlp-receiver` `[prom-otlp]`

Names that look like telemetry stores and are something else:
- ClickHouse is a general analytical database (section 06)
- In telemetry it is an engine under a product, or a store an engineering team builds a log platform on
- Data lakes are a storage layout for analytics (section 06)
- Tempo, InfluxDB 3, and OpenObserve write Parquet to object storage and reuse that layout `[tempo-blocks]` `[influxdb3]` `[openobserve-arch]`
- Dynatrace describes Grail as a data lakehouse `[dynatrace-grail]`
- Grafana is a dashboard and graph editor over other stores `[gitlab-arch]`
- Sentry groups errors `[telemetry-github]`
- Sentry's self-hosted release 26.10.1 starts 53 services, among them PostgreSQL, ClickHouse, Kafka, Redis, and SeaweedFS `[sentry-compose]`
- PostHog and Plausible are product analytics. Both store events in ClickHouse and account data in PostgreSQL `[posthog-arch]` `[plausible]`
- Uptime checks run outside a system and report reachability
- Healthchecks.io inverts this: a job must check in, and a missed check-in raises an alert `[healthchecks]`

Licenses and owners that changed what a name means:

| [INDEX] | [PRODUCT]                                     | [LICENSE]             | [FACT]                                                        |
| :-----: | :-------------------------------------------- | :-------------------- | :------------------------------------------------------------ |
|  [01]   | Grafana, Loki, Tempo, Mimir, Pyroscope        | AGPL-3.0              | Grafana Labs, Grafana Cloud hosts each                        |
|  [02]   | Prometheus, Alertmanager, Thanos, Cortex      | Apache-2.0            | CNCF projects                                                 |
|  [03]   | VictoriaMetrics, VictoriaLogs, VictoriaTraces | Apache-2.0            | Enterprise binaries are separate and need a license           |
|  [04]   | Jaeger                                        | Apache-2.0            | v2 is built from OpenTelemetry Collector components           |
|  [05]   | SigNoz                                        | MIT outside `ee/`     | ClickHouse for signals, SQLite or PostgreSQL for metadata     |
|  [06]   | ClickStack (was HyperDX)                      | MIT                   | ClickHouse acquired HyperDX on 2025-03-13                     |
|  [07]   | OpenObserve, Uptrace, qryn (Gigapipe)         | AGPL-3.0              | qryn was renamed Gigapipe in 2025                             |
|  [08]   | Coroot, GreptimeDB, Parca                     | Apache-2.0            | Dash0 acquired Parca's maker Polar Signals on 2026-08-17      |
|  [09]   | Quickwit                                      | Apache-2.0            | Datadog acquired it on 2025-01-09                             |
|  [10]   | Vector                                        | MPL-2.0               | Maintained by Datadog                                         |
|  [11]   | Sentry                                        | FSL-1.1-Apache-2.0    | Source available, each release becomes Apache-2.0 later       |
|  [12]   | GlitchTip, Umami                              | MIT                   | GlitchTip needs PostgreSQL only                               |
|  [13]   | Bugsink                                       | PolyForm-Shield-1.0.0 | Source available                                              |
|  [14]   | PostHog                                       | MIT outside `ee/`     | ClickHouse, Kafka, PostgreSQL, Redis, object storage          |
|  [15]   | Plausible, OpenPanel                          | AGPL-3.0              | ClickHouse and PostgreSQL                                     |
|  [16]   | Grafana OnCall OSS                            | AGPL-3.0              | Archived 2026-03-24, Grafana IRM is cloud only                |
|  [17]   | Opsgenie                                      | Proprietary           | Sales ended 2025-06-04, support ends 2027-04-05               |
|  [18]   | Highlight.io                                  | Apache-2.0            | LaunchDarkly acquired it, hosted accounts moved by 2026-02-28 |

Rows rest on `[telemetry-github]` for licenses and on `[clickhouse-hyperdx]`, `[polarsignals-dash0]`, `[quickwit-datadog]`, `[sentry-fsl]`, `[oncall-archive]`, `[opsgenie-eol]`, and `[highlight]` for ownership and end dates.

Self-hosted telemetry means sizing disks for retention, bounding label cardinality, updating each store, and keeping it reachable while everything it watches is down. Hosted telemetry means sending OTLP to a vendor endpoint. Changing vendor or moving in-house changes an exporter in Collector configuration when instrumentation is OpenTelemetry `[otel-what]`.

## [10]-[DELIVERY]

Delivery is everything that changes a running backend. App code becomes a release, and descriptions of infrastructure become resources:

| [INDEX] | [STAGE]            | [DOES]                                                | [PRODUCTS]                                                |
| :-----: | :----------------- | :---------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Source hosting     | Holds repositories, reviews, and merge rules          | GitHub, GitLab, Forgejo, Gitea                            |
|  [02]   | CI                 | Runs checks and builds for each change                | GitHub Actions, GitLab CI, Woodpecker, Jenkins, Dagger    |
|  [03]   | Image build        | Turns source into an OCI image                        | BuildKit with a Dockerfile, Buildpacks, Railpack, ko, Nix |
|  [04]   | Registry           | Stores images and other artifacts                     | GHCR, Docker Hub, Amazon ECR, Harbor, Zot                 |
|  [05]   | Supply chain       | Signs, attests, and scans what was built              | cosign, SLSA, GitHub attestations, Syft, Grype, Trivy     |
|  [06]   | Provisioning       | Creates resources through provider APIs, records them | Terraform, OpenTofu, Pulumi, AWS CDK, Crossplane, SST     |
|  [07]   | Deployment         | Makes a new release run in place of an old one        | Kamal, Argo CD, Flux, Komodo, Doco-CD                     |
|  [08]   | Dependency updates | Opens changes for newer versions                      | Renovate, Dependabot, Updatecli                           |
|  [09]   | Release            | Versions, tags, and publishes                         | release-please, semantic-release, GoReleaser              |

Provisioning tools share one model and differ in language, license, and when they act:
- State is their record of what exists `[tf-state]`
- Terraform uses it "to map real world resources to your configuration" `[tf-state]`
- OpenTofu calls it "some sort of database to map OpenTofu config to the real world" `[tofu-state]`
- Pulumi programs are written in Python, TypeScript, Go, .NET, Java, YAML, or HCL `[pulumi-hcl]`
- Programs register resources, and Pulumi's engine computes operations that bring current state to desired state `[pulumi-how]`
- Pulumi keeps state in Pulumi Cloud, S3-compatible storage, Azure Blob Storage, Google Cloud Storage, PostgreSQL, or a file `[pulumi-state]`
- Pulumi runs any Terraform or OpenTofu provider `[pulumi-any-tf]`
- Pulumi's Automation API runs programs from other code and still needs its CLI binary installed `[pulumi-automation]`
- Terraform is BUSL-1.1 with IBM as licensor `[tf-license]`
- OpenTofu is its MPL-2.0 fork in CNCF Sandbox and adds state encryption `[tofu-cncf]` `[tofu-encryption]`
- Pulumi's CLI and SDKs are Apache-2.0 `[delivery-github]`
- Terraform, OpenTofu, and Pulumi act when a person or CI job runs them
- Crossplane is a control plane in Kubernetes that watches resources and "automatically corrects the drift" `[crossplane]`
- SST builds on Pulumi's providers. Alchemy is TypeScript with its own providers and is in beta `[sst-v3]` `[alchemy]`
- Gone or frozen: CDK for Terraform was deprecated on 2025-12-10, System Initiative's repository is archived `[cdktf]` `[delivery-github]`
- Wing Cloud shut down in April 2025 `[wing]`
- Anything with an API can be a resource: machines, DNS records, buckets, repository rulesets and environments, tunnels, secrets `[gh-rulesets]`
- Probot Settings and safe-settings manage GitHub settings from a file without a provisioning tool `[delivery-github]`

Deployment is push or pull:
- Push: a CI job or a person connects to a target and changes it
- Kamal pushes over SSH and "uses kamal-proxy to provide gapless deployments" `[kamal-proxy]`
- Pull: an agent on a target fetches desired state `[opengitops]`
- OpenGitOps 1.0.0 requires desired state to be declarative, versioned and immutable, pulled automatically, and continuously reconciled `[opengitops]`
- Argo CD and Flux are CNCF graduated pull agents for Kubernetes `[delivery-github]`
- Flux lost its sponsor Weaveworks in 2024 and continues with new backers `[flux-support]`
- Docker hosts have Komodo (GPL-3.0) and Doco-CD `[delivery-github]`
- Watchtower was archived on 2025-12-17 `[watchtower]`
- Helm 4 was released on 2025-11-12 and Kustomize is at v5, both package Kubernetes manifests `[helm4]` `[delivery-github]`

Images need no Dockerfile:

| [INDEX] | [TOOL]                  | [READS]                                         | [NOTE]                                                       |
| :-----: | :---------------------- | :---------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Cloud Native Buildpacks | App source                                      | CNCF graduated 2026-07-17, Spring Boot `build-image` uses it |
|  [02]   | Railpack                | App source                                      | MIT, replaces Nixpacks, which is in maintenance              |
|  [03]   | ko                      | Go module                                       | Apache-2.0                                                   |
|  [04]   | Jib                     | Java project                                    | Apache-2.0                                                   |
|  [05]   | .NET SDK                | .NET project through `dotnet publish`           | Part of SDK                                                  |
|  [06]   | apko with melange       | List of APK packages                            | Apache-2.0, from Chainguard                                  |
|  [07]   | Nix                     | Nix expression                                  | `dockerTools`, nix2container                                 |
|  [08]   | Bazel `rules_oci`       | Bazel targets                                   | Apache-2.0                                                   |
|  [09]   | Dagger                  | Code in Go, TypeScript, Python, and 5 more SDKs | Apache-2.0, drives BuildKit                                  |

Rows rest on `[delivery-github]`, `[buildpacks-cncf]`, `[railpack]`, and `[dotnet-containers]`. kaniko was archived by Google in June 2025, and Earthly ended maintenance and its cloud on 2025-07-16 `[delivery-github]` `[earthly]`.

CI and supply-chain facts:
- GitHub-hosted runners became up to 39 percent cheaper on 2026-01-01 `[gh-pricing]`
- GitHub announced a per-minute charge for self-hosted runners for 2026-03-01 and postponed it `[gh-pricing]`
- Docker Hub limits pulls to 100 per 6 hours without sign-in and 200 for personal accounts `[dockerhub-limits]`
- Mutable action tags were repointed to malicious code in tj-actions/changed-files in March 2025 `[tj-actions]`
- Attackers published a malicious Trivy v0.69.4 on 2026-03-19 and repointed 76 of 77 `trivy-action` version tags `[trivy-incident]`
- zizmor and pinact check and pin workflow references, and SLSA 1.2 defines provenance levels for builds and source `[delivery-github]` `[slsa]`
- Registries hold more than images: ORAS stores any artifact through OCI distribution `[delivery-github]`
- Renovate is AGPL-3.0 from Mend, which sells closed self-hosted editions and runs a hosted app `[renovate]`
- Dependabot's core is MIT `[delivery-github]`
- Forgejo is GPL-3.0-or-later from v9, Codeberg e.V. holds its domains, and Gitea stays MIT `[forgejo]` `[delivery-github]`

## [11]-[PROTECTION]

Protection keeps data and service through failure, mistake, and attack:

| [INDEX] | [CATEGORY]      | [GUARDS_AGAINST]                                 | [PRODUCTS]                                               |
| :-----: | :-------------- | :----------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Database backup | Lost or wrongly changed data                     | pgBackRest, Barman, WAL-G, clickhouse-backup, Litestream |
|  [02]   | File backup     | Lost disks and deleted files                     | Restic, Kopia, BorgBackup, rustic, Plakar, rclone        |
|  [03]   | Volume backup   | Lost container and cluster volumes               | offen/docker-volume-backup, Velero, zrepl, sanoid        |
|  [04]   | Backup target   | Loss of a primary site and of backups themselves | Object storage with Object Lock, rsync.net, BorgBase     |
|  [05]   | Replication     | Downtime when one machine fails                  | Patroni, CloudNativePG, database replicas                |
|  [06]   | Hardening       | Intrusion                                        | Provider firewalls, patching, gVisor, Kata Containers    |

Rules practitioners cite:
- 3-2-1: 3 copies of data, on 2 types of media, with 1 copy off-site `[backblaze-321]`
- 3-2-1-1-0 adds one copy offline, air-gapped, or immutable, and zero errors on verification `[veeam-32110]`
- Recovery point objective is how much recent data may be lost, recovery time objective is how long restoration may take `[aws-rto]`
- Point-in-time recovery in PostgreSQL restores a file-level backup and replays archived WAL to a chosen moment `[pg-pitr]`
- Replication is not backup: an accidental or malicious `DROP DATABASE` replicates to every replica `[mariadb-replication]`
- Backups count once restored. GitLab's 2017 outage found its `pg_dump` bucket empty because dumps had failed without notice `[gitlab-2017]`

Tool facts: Restic (BSD-2-Clause) and its rest-server offer an append-only mode. BorgBackup 1.4 is stable and 2.0 is beta. Velero entered CNCF Sandbox on 2026-03-11. Proxmox Backup Server 4.2 backs up to S3 storage `[protection-github]` `[velero-cncf]` `[pbs]`.

Hardening and failure testing have open-source products of their own: OWASP Core Rule Set for web application firewalls, CrowdSec (MIT), Falco (Apache-2.0), Chaos Mesh, and LitmusChaos `[extras-github]`.

Managed database services run backups as part of their service `[rds]`. Object storage with Object Lock is an off-site, immutable target for backups of self-hosted systems.

## [12]-[BOUGHT_SERVICES]

Bought services depend on parties outside any backend: mailbox providers, card networks, phone carriers, model vendors.

| [INDEX] | [CATEGORY]          | [HOSTED]                               | [SELF_HOSTED]                                | [STANDARD]             |
| :-----: | :------------------ | :------------------------------------- | :------------------------------------------- | :--------------------- |
|  [01]   | Transactional email | Amazon SES, Postmark, Resend, Mailgun  | Postal, Stalwart, Mailpit for development    | SMTP, SPF, DKIM, DMARC |
|  [02]   | SMS and push        | Twilio, Firebase Cloud Messaging, APNs | ntfy, Novu                                   | Web Push               |
|  [03]   | Payments            | Stripe, Paddle, Polar                  | None                                         | None                   |
|  [04]   | Billing logic       | Stripe Billing                         | Lago, Kill Bill                              | None                   |
|  [05]   | Feature flags       | LaunchDarkly, Statsig                  | Flagsmith, Unleash, GrowthBook, Flipt, flagd | OpenFeature            |
|  [06]   | Media processing    | Cloudflare Images, Mux                 | imgproxy, Thumbor, FFmpeg                    | None                   |
|  [07]   | Model access        | OpenRouter, Cloudflare AI Gateway      | LiteLLM, Portkey gateway                     | None                   |

Facts:
- SMTP is RFC 5321. DMARC's RFC 7489 was replaced by RFCs 9989, 9990, and 9991 in May 2026 `[email-rfcs]`
- Paddle and Polar are merchants of record, which sell to your customer on your behalf `[paddle]` `[polar]`
- Stripe acquired Lemon Squeezy in 2024 `[lemonsqueezy]`
- OpenFeature is CNCF incubating and standardizes how app code asks for a flag `[openfeature]`
- Unleash moved from Apache-2.0 to AGPL-3.0 on 2026-05-26 `[bought-github]`
- Flipt v2 is under Fair Core License `[bought-github]`
- Amplitude acquired Statsig on 2026-05-05 `[statsig]`
- Palo Alto Networks acquired Portkey on 2026-05-29 `[portkey]`
- Inference servers vLLM (Apache-2.0) and Ollama (MIT) run open-weight models on your machines, gateways route between vendors `[extras-github]`

Backend-as-a-service products bundle areas of this map behind one account:

| [INDEX] | [PRODUCT]  | [LICENSE]          | [BUNDLES]                                                                                 |
| :-----: | :--------- | :----------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | Supabase   | Apache-2.0         | PostgreSQL, pooler, sign-in, REST over tables, realtime, file storage, functions, gateway |
|  [02]   | Firebase   | Proprietary        | Document and SQL databases, sign-in, functions, file storage, hosting                     |
|  [03]   | Appwrite   | BSD-3-Clause       | Sign-in, databases, storage, functions, messaging, realtime, hosting                      |
|  [04]   | PocketBase | MIT                | Embedded SQLite, realtime, files, users, in one binary                                    |
|  [05]   | Convex     | FSL-1.1-Apache-2.0 | Database, server functions, client libraries                                              |
|  [06]   | Nhost      | MIT                | PostgreSQL, GraphQL, sign-in, storage, functions                                          |

Rows rest on `[supabase-compose]`, `[firebase]`, and `[bought-github]`. Supabase's self-hosted Compose file at v1.26.08 runs 11 services. PocketBase's GitHub repository is archived and its source moved to its author's own site `[supabase-compose]` `[bought-github]`.

## [13]-[ENVIRONMENTS]

Environments are copies of an app with their own config and backing services. Twelve-factor asks every copy to use "the same type and version of each of the backing services" `[12f-parity]`.

| [INDEX] | [KIND]            | [LIVES]                              | [PRODUCTS]                                                          |
| :-----: | :---------------- | :----------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Production        | As long as a product                 | Everything in sections 03 to 12                                     |
|  [02]   | Staging           | As long as a product                 | Second stack of one description `[pulumi-stacks]`                   |
|  [03]   | Preview           | While a pull request is open         | Vercel Preview, database branches at Neon, Xata, and PlanetScale    |
|  [04]   | Local development | While a developer works              | Docker Compose, Aspire, Tilt, devenv, process-compose, Skaffold     |
|  [05]   | Test dependencies | One test run                         | Testcontainers, CI service containers, PGlite                       |
|  [06]   | Dev container     | Until its owner removes it           | Development Containers specification, GitHub Codespaces             |
|  [07]   | Toolbox           | Until its owner removes it           | distrobox, Toolbx                                                   |
|  [08]   | Local sandbox     | Until its owner removes it           | Incus system container or virtual machine, a named Docker container |
|  [09]   | Hosted sandbox    | Seconds to months, stopped when idle | E2B, Modal Sandboxes, Cloudflare Sandbox SDK, Fly.io Sprites        |

Rows rest on `[vercel-env]`, `[pg-hosted]`, `[devcontainers]`, `[codespaces]`, `[environments-github]`, and `[sandboxes]`.

Test dependencies belong to tests:
- Testcontainers is a library that starts test dependencies as real services in Docker containers `[tc-start]`
- Test code starts containers before tests and Testcontainers destroys them afterward `[tc-start]`
- In JUnit a static field shares one container across a class and an instance field starts one per test method `[tc-junit]`
- Ryuk, a helper container, removes labeled containers, volumes, and networks even when a test process is killed `[tc-start]`
- Reusable containers are experimental, "won't stop after all tests are finished", and are "not suited for CI usage" `[tc-reuse]`
- Docker Engine on Linux, Docker Desktop, and Testcontainers Cloud are supported runtimes `[tc-start]`
- Colima and Rancher Desktop are documented, and alternative runtimes are "not actively tested" `[tc-runtimes]`
- Docker acquired Testcontainers' maker AtomicJar on 2023-12-11 `[tc-docker]`
- Java, Node, and .NET libraries are MIT, Python and Rust are Apache-2.0 `[environments-github]`
- GitHub Actions service containers do this job in CI: created per job, destroyed when it completes `[gh-services]`

Local development tools start an app's whole set of parts on one machine:
- Aspire models services, databases, queues, and caches in a C# or TypeScript AppHost and starts them with one command `[aspire]`
- Aspire publishes that model to Docker Compose, Kubernetes, or Azure `[aspire-deploy]`
- Docker acquired Tilt in 2022. Skaffold, DevSpace, Telepresence, and mirrord serve Kubernetes development `[tilt]` `[environments-github]`
- LocalStack's repository is archived and its image needs an account and token since 2026-03-23 `[localstack]`
- Moto and Floci emulate AWS APIs without one `[environments-github]`

Sandboxes are for work with no place in a product: trying a tool, running untrusted code, an agent's scratch machine.
- Application containers started by hand (`docker run`) live until removed and appear in no deployment `[docker-run]`
- System containers and virtual machines from Incus behave like small hosts with many services inside `[incus-containers]`
- Dev containers use "a container as a full-featured development environment" `[devcontainers]`
- Codespaces are dev containers on GitHub-hosted virtual machines `[codespaces]`
- Hosted sandboxes are created through an API, and E2B's is Apache-2.0 `[sandboxes]`
- Fly.io Sprites are persistent virtual machines "free when idle", Cloudflare's container sandboxes are microVMs `[sandboxes]`
- Daytona moved development to a private codebase in June 2026 and archived its repository `[daytona]`

## [14]-[APPLICATION_MODEL]

Application models describe an app by its parts and by type of each backing service it needs, and leave product, place, and credentials to whatever supplies that type in one environment. Twelve-factor states the rule they build on: code "makes no distinction between local and third party services", each is an attached resource reached through a handle in config, and replacing a local database with a managed one changes that handle alone `[12f-backing]`.

Published models:

| [INDEX] | [MODEL]               | [APP_STATES]                                         | [ENVIRONMENT_SUPPLIES]                                |
| :-----: | :-------------------- | :--------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Twelve-factor config  | Handle names read from environment variables         | Handle values per deploy                              |
|  [02]   | Score 0.4.1           | `containers` and `resources` by type in `score.yaml` | `score-compose`, `score-k8s`, other implementations   |
|  [03]   | Radius v0.62.0        | Application with Resource Types                      | Recipe Pack: one Terraform or Bicep recipe per type   |
|  [04]   | Aspire v13.6.5        | AppHost program in C# or TypeScript                  | Local containers on `aspire run`, a target on publish |
|  [05]   | Dapr v1.19.1          | Calls to Dapr's state, pub/sub, and secret APIs      | Component file naming a product behind each API       |
|  [06]   | Crossplane v2.4.3     | Custom resource of a platform team's API             | Composition: functions that create real resources     |
|  [07]   | Service Binding 1.1.0 | Credentials expected as mounted files                | Binding from a provisioned service's secret           |
|  [08]   | Platform add-ons      | Attach command or dashboard choice                   | Vendor's own managed service                          |

Rows rest on `[12f-config]`, `[score]`, `[radius]`, `[aspire]`, `[dapr-components]`, `[crossplane]`, and `[servicebinding]`.

Radius states the split most directly:
- Resource Types are "the interface, or contract, between developers and the platform" `[radius]`
- One type exists per kind of resource (`Radius.Data/postgreSqlDatabases`) `[radius]`
- Recipe Packs are implementations: one recipe per Resource Type, concrete and provider-specific `[radius-recipes]`
- Changing a pack "changes how resources are deployed without changing the applications that use them" `[radius-recipes]`
- Environments select packs `[radius]`
- Radius docs give one PostgreSQL type a recipe per environment: PostgreSQL in Kubernetes for local work, Amazon RDS on AWS `[radius]`
- Community types cover containers, routes, volumes, PostgreSQL, MySQL, MongoDB, SQL Server, Neo4j, and Redis `[radius-types]`
- Kafka, RabbitMQ, object storage, secrets, and model endpoints complete that list `[radius-types]`

Score shows what an app-side row holds. Its example declares `db: type: postgres` and reads host, port, name, username, and password from that resource, and its docs state what a developer writes as "I want a database of type X and an event queue of type Y" `[score]`.

Aspire puts one model in a general-purpose language: `addPostgres("db")` declares a database, `withReference(db)` attaches it to a service, and one command starts every part locally. Aspire docs say it "isn't a cloud provider or production runtime" `[aspire]`.

Interchange works at one of these levels:
- Same engine, different place: PostgreSQL in a container and PostgreSQL from a vendor speak one wire protocol
- App code stays unchanged, and version, extensions, and limits can differ (section 15)
- Different engines behind one API: Dapr's state API in front of Redis or DynamoDB `[dapr-components]`
- App code uses what every engine supports and loses features one engine has

Backstage (Apache-2.0) is CNCF's example of a web portal over such models `[extras-github]` `[cncf-platforms]`.

CNCF's Platforms White Paper names self-service as what a platform adds over a pile of tools: a user can "request a database and receive its locator and credentials" through one interface, and a platform stays "the thinnest reasonable layer" over implementations it does not run itself `[cncf-platforms]`.

## [15]-[HOSTING]

Self-hosted and hosted are positions on a scale of who does operating work. Amazon's comparison for a relational database names that work `[rds]`:

| [INDEX] | [TASK]                        | [OWN_HARDWARE] | [RENTED_MACHINE] | [MANAGED_SERVICE] |
| :-----: | :---------------------------- | :------------- | :--------------- | :---------------- |
|  [01]   | Query and schema tuning       | You            | You              | You               |
|  [02]   | Scaling                       | You            | You              | Vendor            |
|  [03]   | High availability             | You            | You              | Vendor            |
|  [04]   | Backups                       | You            | You              | Vendor            |
|  [05]   | Database patching and install | You            | You              | Vendor            |
|  [06]   | Operating system patching     | You            | You              | Vendor            |
|  [07]   | Server maintenance            | You            | Vendor           | Vendor            |
|  [08]   | Hardware, power, network      | You            | Vendor           | Vendor            |

Rows 02 to 06 are what self-hosting any stateful service means. Automation moves that work without removing its owner: operators on Kubernetes (CloudNativePG), and distributions for plain hosts (Pigsty, autobase, which calls itself "your own DBaaS") `[k8s-operator]` `[pg-ha-github]` `[pig]`.

Hosted comes in these forms:
- Same engine as a service: PostgreSQL at Amazon RDS, Valkey at ElastiCache, ClickHouse Cloud, Kafka at Confluent
- Client code is unchanged, and vendor limits apply: Amazon RDS fixes which extensions exist `[rds-extensions]`
- ClickHouse Cloud runs a storage engine that open-source releases lack `[clickhouse-cloud]`
- Service with no downloadable engine: DynamoDB, BigQuery, Snowflake, Amazon SQS, Pinecone `[warehouses]` `[vector-hosted]`
- Leaving such a service means rewriting code that calls it
- Vendor-operated software in your own cloud account: WarpStream and Pinecone offer it `[warpstream-confluent]` `[vector-hosted]`

Where a category has a shared interface, moving between self-hosted and hosted changes an address and credentials in config `[12f-backing]`:

| [INDEX] | [CATEGORY]       | [INTERFACE]          | [SELF_HOSTED]                       | [HOSTED]                                            |
| :-----: | :--------------- | :------------------- | :---------------------------------- | :-------------------------------------------------- |
|  [01]   | Relational       | PostgreSQL protocol  | PostgreSQL                          | RDS, Cloud SQL, Neon, Supabase, Aiven               |
|  [02]   | In-memory        | Redis protocol       | Valkey, Redis, Garnet               | ElastiCache, Memorystore, Upstash                   |
|  [03]   | Object storage   | S3 API               | Garage, SeaweedFS, RustFS, Ceph RGW | S3, R2, B2, Hetzner, OVHcloud, Scaleway             |
|  [04]   | Event log        | Kafka protocol       | Kafka, AutoMQ                       | Confluent Cloud, Redpanda Cloud                     |
|  [05]   | Documents        | MongoDB protocol     | MongoDB, DocumentDB, FerretDB       | MongoDB Atlas                                       |
|  [06]   | Analytical       | ClickHouse protocol  | ClickHouse                          | ClickHouse Cloud                                    |
|  [07]   | Lake catalog     | Iceberg REST catalog | Polaris, Lakekeeper, Unity Catalog  | AWS Glue, S3 Tables, R2 Data Catalog                |
|  [08]   | Telemetry intake | OTLP                 | Collector in front of any store     | Any observability vendor                            |
|  [09]   | Metric queries   | Prometheus query API | Prometheus, VictoriaMetrics, Mimir  | Grafana Cloud, Amazon and Google managed Prometheus |
|  [10]   | Sign-in          | OpenID Connect       | Keycloak, authentik, ZITADEL        | Auth0, Clerk, WorkOS, Cognito                       |
|  [11]   | Outbound email   | SMTP                 | Postal, Stalwart                    | Amazon SES, Postmark, Resend                        |
|  [12]   | Certificates     | ACME                 | step-ca                             | Let's Encrypt, ZeroSSL, Google Trust Services       |
|  [13]   | Images           | OCI distribution     | Harbor, Zot, CNCF Distribution      | GHCR, Amazon ECR, Docker Hub                        |
|  [14]   | Containers       | OCI image            | Docker Engine, Kubernetes           | Cloud Run, Fargate, Fly.io, Railway                 |

Every product in this table has its source in its own section. Categories with no shared interface, where a move rewrites code: durable execution, cloud queues, secrets, authorization, search, vector stores, warehouses, and product analytics.

"Compatible" is a claim each vendor scopes for itself. Object stores publish which S3 operations they lack, and engines that speak PostgreSQL's protocol (CockroachDB, YugabyteDB, Aurora DSQL, Spanner) run other software with other limits `[s3-compat]` `[pg-compatible]`.

## [16]-[CHOICES]

Standings are one standard, one clear product, a two-way choice, a menu, or a choice that follows workload or language. Open-source candidates below are maintained, released within a year before 2026-11-12, and carry licenses named in their sections:

| [INDEX] | [CATEGORY]                     | [STANDING]        | [OPEN_SOURCE_CANDIDATES]                                                         |
| :-----: | :----------------------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | Image format                   | One standard      | OCI image 1.1.1                                                                  |
|  [02]   | Container engine               | Two-way           | Docker Engine 29.9.3, containerd 2.4.2                                           |
|  [03]   | One-host scheduler or platform | Menu              | Docker Compose 5.7.1, or a platform: Kamal 2.12.0, Coolify 4.7.5, Dokploy 0.31.1 |
|  [04]   | Cluster scheduler              | One clear product | Kubernetes 1.37.2, as k3s, k0s, or Talos Linux 1.14.6                            |
|  [05]   | Host operating system          | Menu              | Debian 13, Ubuntu 26.04 LTS, Flatcar 4757.2.3, NixOS 26.05                       |
|  [06]   | Reverse proxy                  | Menu              | Caddy 2.11.9, Traefik 3.7.18, nginx 1.31.8, HAProxy 3.4.0, Envoy 1.40.2          |
|  [07]   | Certificates                   | One standard      | ACME, with step-ca 0.30.2 for private names                                      |
|  [08]   | Overlay network                | Menu              | Headscale 0.29.7, NetBird 0.82.1, Nebula 1.12.0                                  |
|  [09]   | Relational database            | One clear product | PostgreSQL 18.7                                                                  |
|  [10]   | In-process SQL                 | One per workload  | SQLite 3.54.1 for records, DuckDB 2.0.0 for analysis                             |
|  [11]   | Analytical server              | Menu              | ClickHouse 26.10, Apache Doris 4.1.5, StarRocks 4.1.7                            |
|  [12]   | In-memory store                | Menu              | Valkey 9.1.3, Redis 8.10.3, Garnet 2.2.11                                        |
|  [13]   | Object storage                 | One standard      | S3 API, served by Garage 2.5.0, SeaweedFS 4.53, RustFS 1.0.5, Ceph 21.2.0        |
|  [14]   | Lake table format              | Menu              | Apache Iceberg 1.12.0, Delta Lake 4.4.1, DuckLake 1.1                            |
|  [15]   | Search                         | Menu              | OpenSearch 3.9.0, Meilisearch 1.56.2, Typesense 30.2, `pg_search` 0.26.6         |
|  [16]   | Vector                         | Menu              | pgvector 0.8.7, Qdrant 1.20.0, Milvus 3.0.6                                      |
|  [17]   | Event log                      | One standard      | Kafka protocol, served by Apache Kafka 4.4.0, AutoMQ 1.7.4                       |
|  [18]   | Message queue                  | Menu              | RabbitMQ 4.4.0, NATS 2.15.3, pgmq 1.13.1                                         |
|  [19]   | Background jobs                | Follows language  | River 0.51.2, pg-boss 12.38.2, Oban 2.24.0, Solid Queue 1.7.2, BullMQ 6.7.5      |
|  [20]   | Durable execution              | Menu              | Temporal 1.32.2, DBOS Transact, Hatchet 0.114.2                                  |
|  [21]   | Identity provider              | Menu              | Keycloak 26.8.2, authentik 2026.8.5, ZITADEL 4.20.2, Better Auth 1.7.22          |
|  [22]   | Access proxy                   | Menu              | Authelia 4.39.28, oauth2-proxy 7.15.6, Pomerium 0.34.1                           |
|  [23]   | Authorization                  | Menu              | OpenFGA 1.22.1, SpiceDB 1.57.1, Open Policy Agent 1.22.4                         |
|  [24]   | Secrets                        | Menu              | OpenBao 2.7.2, Infisical 0.167.8, SOPS 3.13.3                                    |
|  [25]   | Telemetry pipeline             | One standard      | OpenTelemetry, Collector 0.165.0                                                 |
|  [26]   | Metric store                   | Menu              | Prometheus 3.16.0, VictoriaMetrics 1.156.0, Grafana Mimir 3.2.4                  |
|  [27]   | Log store                      | Menu              | VictoriaLogs 1.53.0, Grafana Loki 3.7.9, OpenSearch 3.9.0                        |
|  [28]   | Trace store                    | Menu              | Jaeger 2.23.0, Grafana Tempo 3.1.1, VictoriaTraces 0.13.0                        |
|  [29]   | All-signal platform            | Menu              | SigNoz 0.149.0, ClickStack, OpenObserve 1.1.2                                    |
|  [30]   | Dashboards                     | One clear product | Grafana 13.3.1, with Perses 0.55.1 in CNCF Sandbox                               |
|  [31]   | Error tracking                 | Menu              | GlitchTip 7.0.0, Bugsink 2.6.3, Sentry 26.10.1 (source available)                |
|  [32]   | Product analytics              | Menu              | PostHog, Plausible 3.2.1, Umami 3.5.0                                            |
|  [33]   | Provisioning                   | Two-way           | OpenTofu 1.13.9, Pulumi 3.276.0                                                  |
|  [34]   | Machine configuration          | Menu              | cloud-init 26.2, Ansible 2.22.0, pyinfra 3.11.0, NixOS 26.05                     |
|  [35]   | Source hosting                 | Menu              | Forgejo 17.0.1, Gitea 28.3.0, GitLab 19.5                                        |
|  [36]   | Registry                       | Menu              | Zot 2.1.23, Harbor 2.15.5, CNCF Distribution 3.1.2                               |
|  [37]   | Deployment agent               | Menu              | Argo CD 3.6.1, Flux 2.10.0, Komodo 2.3.4, Doco-CD 0.133.2                        |
|  [38]   | Backup                         | Menu              | Restic 0.19.1, Kopia 0.23.1, pgBackRest 2.60.0, Barman 3.20.1                    |
|  [39]   | Sandbox host                   | Menu              | Incus 7.6.0, Proxmox VE 9.2                                                      |

Grounds for each standing that is not a menu:
- One standard: a specification, or one vendor's published API, that independent products implement (OCI, ACME, OpenTelemetry, S3 API, Kafka protocol)
- S3 API and Kafka protocol are standards by adoption, no standards body defines either
- Choice moves to which implementation, and a wrong pick is replaceable
- PostgreSQL: most-used database in Stack Overflow's 2025 and 2026 surveys `[so-2026]`
- PostgreSQL has its own permissive license and extensions that reach other categories `[pg-license]` `[pg-extensions-github]`
- Kubernetes: CNCF's landscape lists certified distributions and hosted forms for no other cluster scheduler `[cncf-landscape]`
- Grafana: Loki, Tempo, Mimir, and Pyroscope come from its maker, and GitLab bundles it `[telemetry-github]` `[gitlab-arch]`
- VictoriaMetrics documents itself as a drop-in Prometheus source for Grafana `[vm-docs]`
- In-memory: Valkey has a foundation, BSD license, and hosted forms at AWS and Google `[valkey]`
- Redis has its original authors and an AGPL option `[redis-license]`
- Provisioning: OpenTofu (declarative files, foundation) or Pulumi (general-purpose languages, vendor) `[delivery-github]`
- Terraform's license is not open source `[tf-license]`
- Object storage servers split by design goal: Garage for few nodes across sites `[garage-goals]`
- SeaweedFS serves objects with files and tables, Ceph serves object, block, and file from one large cluster `[storage-github]`
- RustFS is a young server at release 1.0.5 `[storage-github]`

Categories that need no new server at first. Each row names what fills it inside PostgreSQL or inside an app's process, and section 06 names what replaces it when a workload outgrows that:

| [INDEX] | [CATEGORY]        | [FILLED_BY]                                |
| :-----: | :---------------- | :----------------------------------------- |
|  [01]   | Documents         | PostgreSQL `jsonb`                         |
|  [02]   | Search            | PostgreSQL full-text search, `pg_search`   |
|  [03]   | Vector            | pgvector                                   |
|  [04]   | Time series       | TimescaleDB                                |
|  [05]   | Background jobs   | Job library on PostgreSQL (River, pg-boss) |
|  [06]   | Schedule          | `pg_cron`, systemd timers                  |
|  [07]   | Analysis          | DuckDB in process                          |
|  [08]   | Lake catalog      | DuckLake tables in PostgreSQL              |
|  [09]   | Sign-in           | Library with users in your database        |
|  [10]   | Durable execution | DBOS Transact, a library on PostgreSQL     |

Categories with no such form start as a server or a bought service: in-memory store, object storage, event log, metric store, reverse proxy.

## [17]-[PUBLISHED_ARCHITECTURES]

Projects that publish what they run show which categories recur:

| [INDEX] | [SYSTEM]             | [RELATIONAL]                   | [IN_MEMORY]      | [ANALYTICAL] | [QUEUE_OR_LOG]   | [OBJECTS]      |
| :-----: | :------------------- | :----------------------------- | :--------------- | :----------- | :--------------- | :------------- |
|  [01]   | GitLab               | PostgreSQL, PgBouncer, Patroni | Redis            | None listed  | Sidekiq on Redis | S3-compatible  |
|  [02]   | Sentry self-hosted   | PostgreSQL, PgBouncer          | Redis, Memcached | ClickHouse   | Kafka            | SeaweedFS      |
|  [03]   | PostHog              | PostgreSQL                     | Redis            | ClickHouse   | Kafka            | Blob storage   |
|  [04]   | Supabase self-hosted | PostgreSQL, Supavisor          | None             | None         | Realtime service | Storage API    |
|  [05]   | Plausible            | PostgreSQL                     | None             | ClickHouse   | None             | None           |
|  [06]   | SigNoz               | SQLite, optional PostgreSQL    | None             | ClickHouse   | None             | None           |
|  [07]   | Trigger.dev          | PostgreSQL                     | Redis            | ClickHouse   | None             | Object storage |

Rows rest on `[gitlab-arch]`, `[sentry-compose]`, `[posthog-arch]`, `[supabase-compose]`, `[plausible]`, `[signoz-arch]`, and `[trigger-selfhost]`.

Other parts those documents name:
- GitLab: NGINX as reverse proxy, Prometheus with Alertmanager and Grafana, optional Elasticsearch for search `[gitlab-arch]`
- GitLab adds a container registry, runners, and Consul for database node discovery `[gitlab-arch]`
- GitLab's docs state that connections between components use Unix sockets unless noted `[gitlab-arch]`
- Sentry: nginx, its own Relay intake service, and Snuba consumers in front of ClickHouse, among 53 services `[sentry-compose]`
- Supabase: Kong as gateway, GoTrue for sign-in, PostgREST, imgproxy, an edge function runtime `[supabase-compose]`

Accounts of scale:
- OpenAI runs ChatGPT on one unsharded PostgreSQL primary with nearly 50 read replicas at Azure `[openai-pg]`
- OpenAI moved shardable write-heavy workloads to Cosmos DB `[openai-pg]`
- OpenAI keeps petabytes of logs per day in ClickHouse, 90 shards of 2 replicas fed by Fluent Bit `[openai-clickhouse]`
- Cloudflare moved a log-producing application from Elasticsearch to a smaller ClickHouse cluster in 2022 `[cloudflare-logs]`
- Roblox replaced separate Prometheus and InfluxDB installations with VictoriaMetrics: 200 storage nodes and 5 billion active series `[roblox-vm]`
- 37signals runs on owned servers and deploys with Kamal `[37signals-exit]` `[kamal]`
- 37signals publishes Solid Queue, a database-backed job queue `[messaging-github]`

PostgreSQL appears in every row, as SigNoz's option and as every other system's requirement `[signoz-db]`. Parts that recur beside it: a Redis-protocol store, ClickHouse, Kafka, and S3-compatible object storage.

## [18]-[UNVERIFIED]

Claims this map could not settle, each with a page that would:
- Unverified: Redpanda's present license, README and docs name different ones — `redpanda.com/legal/redpanda-community-edition-license`
- Unverified: FerretDB is maintained. Its last release is 2025-11-10 — `github.com/FerretDB/FerretDB/discussions/5651`
- Unverified: Hetzner sells no managed PostgreSQL, no product page was read — `hetzner.com/cloud`
- Unverified: Azure Blob Storage has no native S3 API. Source is a Microsoft Q&A answer from 2023 — `learn.microsoft.com/azure/storage/blobs`
- Unverified: whether AWS Fargate services scale to zero — `docs.aws.amazon.com/AmazonECS/latest/developerguide/service-auto-scaling.html`
- Unverified: whether paid Render services scale to zero — `render.com/docs/scaling`
- Unverified: Cloudflare Workers idle billing wording — `developers.cloudflare.com/workers/platform/pricing`
- Unverified: object lock on Tigris and DigitalOcean Spaces, lifecycle rules on Wasabi, Hetzner, and IDrive e2 — each vendor's S3 compatibility page
- Unverified: UpCloud, Akamai, Netcup, and Contabo object storage offers — each vendor's product page
- Unverified: official count of extensions in PGDG repositories. 114 is a count of package names — `wiki.postgresql.org/wiki/Apt`
- Unverified: whether PostGIS and DocumentDB need `shared_preload_libraries` — each project's install page
- Unverified: Cloudflare R2 Data Catalog left beta — `developers.cloudflare.com/r2/data-catalog`
- Unverified: corporate owners of Caddy, Traefik, HAProxy, and PowerDNS — each project's site
- Unverified: Teleport 19 is released. Newest GitHub release read is 18.10.0 — `goteleport.com/docs/changelog`
- Unverified: Buildkite and CircleCI facts, no page was read — `buildkite.com`, `circleci.com`
- Unverified: Temporal Cloud and DBOS Cloud as hosted forms — `temporal.io/cloud`, `dbos.dev`
- Unverified: Peter Krogh as originator of 3-2-1 — `backblaze.com/blog/the-3-2-1-backup-strategy`
- Unverified: Proxmox Backup Server's license — `pbs.proxmox.com`
- Unverified: Fedora CoreOS's distribution license — `docs.fedoraproject.org/en-US/legal`
- Unverified: a DuckDB statement of what it is not meant for. Its Why DuckDB page states target workloads only — `duckdb.org/faq`

## [19]-[SOURCES]

Pages with no version or date of their own were read on 2026-11-12. Keys ending in `-github` stand for GitHub REST API replies (`repos/<owner>/<repo>` for license, archive flag, and description, `releases/latest` or tags for releases) read on 2026-11-12, with LICENSE files read where the API named no license.

Concepts and taxonomies:
- `12f-backing`, `12f-config`, `12f-processes`, `12f-concurrency`, `12f-disposability`, `12f-parity`, `12f-admin`: https://12factor.net/ pages `backing-services`, `config`, `processes`, `concurrency`, `disposability`, `dev-prod-parity`, `admin-processes`, last updated 2017
- `cncf-landscape`: https://github.com/cncf/landscape/blob/master/landscape.yml, commit 75b47639d1, 2026-11-11
- `cncf-platforms`: https://tag-app-delivery.cncf.io/whitepapers/platforms/
- `docker-container`: https://docs.docker.com/get-started/docker-concepts/the-basics/what-is-a-container/
- `docker-run`: https://docs.docker.com/engine/containers/run/
- `docker-volumes`: https://docs.docker.com/engine/storage/volumes
- `incus-containers`: https://linuxcontainers.org/incus/docs/main/explanation/containers_and_vms/
- `firecracker`: https://github.com/firecracker-microvm/firecracker README, v1.18.0
- `k8s-workloads`: https://kubernetes.io/docs/concepts/workloads/, v1.37 docs
- `k8s-ephemeral`: https://kubernetes.io/docs/concepts/workloads/pods/ephemeral-containers/
- `k8s-operator`: https://kubernetes.io/docs/concepts/extend-kubernetes/operator/
- `gh-runners`: https://docs.github.com/en/actions/concepts/runners/github-hosted-runners
- `gh-services`: https://docs.github.com/en/actions/tutorials/use-containerized-services/use-docker-service-containers
- `gh-environments`: https://docs.github.com/en/actions/concepts/workflows-and-actions/deployment-environments
- `gh-oidc`: https://docs.github.com/en/actions/concepts/security/openid-connect
- `gh-rulesets`: https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets
- `pulumi-stacks`: https://www.pulumi.com/docs/iac/concepts/stacks/
- `aws-shared`: https://aws.amazon.com/compliance/shared-responsibility-model/
- `rds`: https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/Welcome.html
- `rds-extensions`: https://docs.aws.amazon.com/AmazonRDS/latest/PostgreSQLReleaseNotes/postgresql-extensions.html

Application model:
- `score`: https://docs.score.dev/docs/, specification 0.4.1 at https://github.com/score-spec/spec
- `radius`: https://docs.radapp.io/concepts/, v0.62.0
- `radius-recipes`: https://docs.radapp.io/concepts/recipe-packs/
- `radius-types`: https://github.com/radius-project/resource-types-contrib, commit 2c72454711, 2026-11-12
- `aspire`: https://aspire.dev/get-started/what-is-aspire/, v13.6.5 at https://github.com/microsoft/aspire
- `aspire-deploy`: https://aspire.dev/deployment/deploy-with-aspire/
- `dapr-components`: https://docs.dapr.io/concepts/components-concept/, v1.19.1
- `crossplane`: https://docs.crossplane.io/latest/whats-crossplane/, v2.4
- `servicebinding`: https://github.com/servicebinding/spec, v1.1.0

Workloads:
- `workloads-github`: `opencontainers/image-spec`, `runtime-spec`, `distribution-spec`, `opencontainers/runc`, `containers/crun`, `google/gvisor`, `kata-containers/kata-containers`, `containerd/containerd`, `moby/moby`, `docker/compose`, `abiosoft/colima`
- `workloads-github` continued: `kubernetes/kubernetes`, `k3s-io/k3s`, `k0sproject/k0s`, `siderolabs/talos`, `coollabsio/coolify`, `Dokploy/dokploy`, `basecamp/kamal`, `dokku/dokku`, `caprover/caprover`, `moghtech/komodo`, `portainer/portainer`
- `devmachine-github`: `abiosoft/colima` v0.10.3, `lima-vm/lima` v2.3.0, `rancher-sandbox/rancher-desktop` v1.25.0, `kubernetes-sigs/kind` v0.33.0, `kubernetes/minikube` v1.39.0
- `docker-desktop-license`: https://docs.docker.com/subscription/desktop-license/
- `systemd-daemon`: https://www.freedesktop.org/software/systemd/man/latest/daemon.html, systemd v262.2
- `launchd`: https://developer.apple.com/library/archive/documentation/MacOSX/Conceptual/BPSystemStartup/Chapters/CreatingLaunchdJobs.html
- `docker-restart`: https://docs.docker.com/engine/containers/start-containers-automatically/
- `docker-multi`: https://docs.docker.com/engine/containers/multi-service_container/
- `k8s-daemonset`: https://kubernetes.io/docs/concepts/workloads/controllers/daemonset/
- `k8s-sidecar`: https://kubernetes.io/docs/concepts/workloads/pods/sidecar-containers/
- `supervisors-github`: `systemd/systemd` v262.2, `F1bonacc1/process-compose` v1.124.0
- `nomad-license`: https://github.com/hashicorp/nomad/blob/main/LICENSE, v2.1.1
- `ibm-hashicorp`: https://newsroom.ibm.com/2025-02-27-ibm-completes-acquisition-of-hashicorp,-creates-comprehensive,-end-to-end-hybrid-cloud-platform
- `swarm`: https://docs.docker.com/engine/swarm, https://www.mirantis.com/blog/mirantis-guarantees-long-term-support-for-swarm (2025-07-16)
- `k3s-arch`: https://docs.k3s.io/architecture, v1.37.2+k3s1
- `kamal`: https://kamal-deploy.org/, v2.12.0
- `kamal-proxy`: https://kamal-deploy.org/docs/configuration/proxy/
- `fly-machines`: https://docs.fly.io/machines/overview, https://docs.fly.io/launch/autostop-autostart.md
- `railway`: https://docs.railway.com/services, https://docs.railway.com/deployments/serverless
- `render`: https://render.com/docs/web-services, https://render.com/docs/free
- `cloud-run`: https://cloud.google.com/run/docs/overview/what-is-cloud-run
- `fargate`: https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html
- `lambda`: https://docs.aws.amazon.com/lambda/latest/dg/welcome.html
- `cf-workers`: https://developers.cloudflare.com/workers/reference/how-workers-works/
- `cf-containers`: https://developers.cloudflare.com/containers/platform/pricing/
- `cf-queues`: https://developers.cloudflare.com/queues/reference/how-queues-works/, updated 2026-04-21
- `vercel-functions`: https://vercel.com/docs/functions

Network:
- `icann-registrar`: https://www.icann.org/resources/pages/what-2013-05-03-en
- `cf-dns-faq`: https://developers.cloudflare.com/dns/faq
- `nginx`: https://nginx.org/en/, release 1.31.8
- `nginx-request`: https://nginx.org/en/docs/http/request_processing.html
- `caddy-concepts`: https://caddyserver.com/docs/caddyfile/concepts, v2.11.9
- `caddy-forwardauth`: https://caddyserver.com/docs/caddyfile/directives/forward_auth
- `traefik-concepts`: https://doc.traefik.io/traefik/getting-started/concepts/, v3.7
- `traefik-forwardauth`: https://doc.traefik.io/traefik/reference/routing-configuration/http/middlewares/forwardauth/
- `network-github`: `caddyserver/caddy`, `traefik/traefik`, `nginx/nginx`, `haproxy/haproxy`, `envoyproxy/envoy`, `envoyproxy/gateway`, `cloudflare/pingora`, `cilium/cilium`, `PowerDNS/pdns`, `coredns/coredns`, `octodns/octodns`, `StackExchange/dnscontrol`, `kubernetes-sigs/external-dns`
- `network-github` continued: `cert-manager/cert-manager`, `certbot/certbot`, `acmesh-official/acme.sh`, `go-acme/lego`, `smallstep/certificates`, `hashicorp/consul`, `istio/istio`, `linkerd/linkerd2`, `juanfont/headscale`, `slackhq/nebula`, `fosrl/pangolin`, `fatedier/frp`
- `freenginx`: https://freenginx.org/pipermail/nginx/2024-February/000000.html
- `gateway-api`: https://github.com/kubernetes-sigs/gateway-api/releases/tag/v1.7.0, 2026-10-29
- `ingress-nginx-retirement`: https://www.kubernetes.io/blog/2025/11/11/ingress-nginx-retirement/
- `acme`: https://www.rfc-editor.org/rfc/rfc8555, March 2019
- `le-45`: https://letsencrypt.org/2025/12/02/from-90-to-45
- `le-shortlived`: https://letsencrypt.org/2026/01/15/6day-and-ip-general-availability
- `cabf-sc081`: https://cabforum.org/2025/04/11/ballot-sc081v3-introduce-schedule-of-reducing-validity-and-data-reuse-periods
- `tailscale-opensource`: https://tailscale.com/opensource, client v1.104.6
- `tailscale-ssh`: https://tailscale.com/kb/1193/tailscale-ssh
- `tailscale-wif`: https://tailscale.com/docs/features/workload-identity-federation
- `tailscale-serve`: https://tailscale.com/docs/features/tailscale-serve
- `headscale-faq`: https://headscale.net/stable/about/faq, v0.29.7
- `netbird-license`: https://github.com/netbirdio/netbird/blob/main/LICENSE, v0.82.1
- `zerotier-116`: https://docs.zerotier.com/changelog/2025/08/21/zt1-v1.16.0
- `teleport-license`: https://goteleport.com/blog/teleport-community-license (2024-03-08)
- `boundary-archive`: https://github.com/hashicorp/boundary README, archived 2026-10-08
- `linkerd-215`: https://linkerd.io/2024/02/21/announcing-linkerd-2.15

Identity:
- `identity-github`: `keycloak/keycloak`, `goauthentik/authentik`, `zitadel/zitadel`, `ory/kratos`, `ory/hydra`, `logto-io/logto`, `kanidm/kanidm`, `sebadob/rauthy`, `pocket-id/pocket-id`, `authelia/authelia`, `dexidp/dex`, `better-auth/better-auth`, `nextauthjs/next-auth`, `lucia-auth/lucia`
- `identity-github` continued: `oauth2-proxy/oauth2-proxy`, `pomerium/pomerium`, `open-policy-agent/opa`, `cedar-policy/cedar`, `openfga/openfga`, `authzed/spicedb`, `Permify/permify`, `casbin/casbin`, `cerbos/cerbos`, `Infisical/infisical`, `getsops/sops`, `FiloSottile/age`, `external-secrets/external-secrets`
- `keycloak-cncf`: https://www.cncf.io/projects/keycloak/
- `zitadel-v3`: https://zitadel.com/blog/zitadel-v3-announcement (2025-05-02)
- `authjs-betterauth`: https://better-auth.com/blog/authjs-joins-better-auth (2025-09-22)
- `duende`: https://duendesoftware.com/products/communityedition
- `auth0`: https://auth0.com/
- `stytch-twilio`: https://changelog.stytch.com/announcements/2025-11-14-a-new-chapter-begins-stytch-joins-twilio
- `oauth21`: https://datatracker.ietf.org/doc/draft-ietf-oauth-v2-1, updated 2026-10-30
- `webauthn3`: https://www.w3.org/TR/webauthn-3/, 2026-08-25
- `cf-access`: https://developers.cloudflare.com/cloudflare-one/access-controls/policies/
- `google-iap`: https://docs.cloud.google.com/iap/docs/concepts-overview
- `google-wif`: https://docs.cloud.google.com/iam/docs/best-practices-for-using-workload-identity-federation
- `pypi-trusted`: https://docs.pypi.org/trusted-publishers/
- `opa-apple`: https://blog.openpolicyagent.org/all?year=2025 (2025-08-20)
- `permify-fusionauth`: https://fusionauth.io/blog/fusionauth-permify-pr (2025-11-20)
- `zanzibar`: https://www.usenix.org/conference/atc19/presentation/pang, USENIX ATC 2019
- `spiffe`: https://www.cncf.io/projects/spire/, SPIRE v1.16.0
- `vault-license`: https://github.com/hashicorp/vault/blob/main/LICENSE, v2.2.2
- `openbao`: https://github.com/openbao/openbao, v2.7.2
- `doppler-onprem`: https://www.doppler.com/doppler-on-prem
- `1password-automation`: https://developer.1password.com/docs/secrets-automation
- `eso-pause`: https://infisical.com/blog/external-secrets-operator-paused, https://external-secrets.io/latest/contributing/burnout-mitigation
- `aws-kms`: https://docs.aws.amazon.com/kms/latest/developerguide/kms-cryptography.html

State:
- `pg-about`: https://www.postgresql.org/about/
- `pg-license`: https://www.postgresql.org/about/licence/
- `pg-versioning`: https://www.postgresql.org/support/versioning/
- `pg-19rc2`: https://www.postgresql.org/about/news/postgresql-187-1712-1616-1520-1425-and-19-rc-2-released-3442/ (2026-11-12)
- `pg-json`, `pg-textsearch`, `pg-preload`, `pg-connection`, `pg-pitr`: https://www.postgresql.org/docs/current/ pages `datatype-json.html`, `textsearch-intro.html`, `runtime-config-client.html`, `runtime-config-connection.html`, `continuous-archiving.html`, version 18.7
- `pg-extensions-github`: `pgvector/pgvector`, `timescale/pgvectorscale`, `timescale/timescaledb`, `paradedb/paradedb`, `apache/age`, `documentdb/documentdb`, `duckdb/pg_duckdb`, `Snowflake-Labs/pg_lake`, `postgis/postgis`, `pgRouting/pgrouting`, `pgmq/pgmq`, `citusdata/pg_cron`, `pgpartman/pg_partman`, `pgaudit/pgaudit`, `citusdata/citus`, `pgxman/pgxman`
- `pg-ha-github`: `pgbouncer/pgbouncer`, `pgdogdev/pgdog`, `supabase/supavisor`, `yandex/odyssey`, `pgagroal/pgagroal`, `patroni/patroni`, `EnterpriseDB/repmgr`, `hapostgres/pg_auto_failover`, `cloudnative-pg/cloudnative-pg`, `CrunchyData/postgres-operator`, `zalando/postgres-operator`, `autobase-tech/autobase`, `EnterpriseDB/barman`, `wal-g/wal-g`, `ariga/atlas`, `flyway/flyway`, `sqitchers/sqitch`, `xataio/pgroll`, `sbdchd/squawk`
- `pgdg-apt`: https://apt.postgresql.org/pub/repos/apt/dists/trixie-pgdg/main/binary-amd64/Packages, 2026-11-12
- `docker-postgres`: https://github.com/docker-library/docs/blob/master/postgres/content.md
- `pig`: https://github.com/pgsty/pig, v1.11.0, and https://github.com/pgsty/pigsty, v4.5.0
- `cnpg-imagevolume`: https://github.com/cloudnative-pg/cloudnative-pg/blob/main/docs/src/imagevolume_extensions.md, v1.30.1
- `cnpg-cncf`: `cncf-landscape` entry for CloudNativePG
- `trunk-shutdown`: https://pgt.dev/
- `pgbackrest-news`: https://pgbackrest.org/news.html, release 2.60.0
- `liquibase-fsl`: https://www.liquibase.com/blog/liquibase-community-for-the-future-fsl (2025-09-30)
- `atlas-license`: https://atlasgo.io/community-edition
- `neon-databricks`: https://www.databricks.com/company/newsroom/press-releases/databricks-agrees-acquire-neon-help-developers-deliver-ai-systems (2025-05-14)
- `crunchy-snowflake`: https://www.snowflake.com/en/blog/snowflake-postgres-enterprise-ai-database/ (2025-06-02)
- `planetscale-pg`: https://planetscale.com/changelog/postgres-ga (2025-09-22)
- `pg-hosted`: product and docs pages of each named vendor
- `cockroach-source`: https://www.cockroachlabs.com/blog/source-code-protection/ (2026-09-15)
- `pg-compatible`: https://github.com/yugabyte/yugabyte-db v2026.1.3.0, https://docs.aws.amazon.com/aurora-dsql/latest/userguide/what-is-aurora-dsql.html, https://cloud.google.com/spanner/docs/postgresql-interface
- `so-2026`: https://survey.stackoverflow.co/2026/technology/data/databases, https://survey.stackoverflow.co/2025/technology
- `openai-pg`: https://openai.com/index/scaling-postgresql/ (2026-01-22)
- `duckdb-why`: https://duckdb.org/why_duckdb, v2.0.0
- `duckdb-concurrency`: https://duckdb.org/docs/stable/connect/concurrency
- `duckdb-quack`: https://duckdb.org/2026/05/12/quack-remote-protocol, https://duckdb.org/docs/1.5/quack/overview
- `duckdb-iceberg`: https://duckdb.org/docs/2.0/core_extensions/iceberg/writing_to_iceberg
- `ducklake`: https://ducklake.select/, version 1.1
- `ducklake-manifesto`: https://ducklake.select/manifesto/
- `clickhouse-intro`: https://clickhouse.com/docs/intro, v26.10
- `clickhouse-olap`: https://clickhouse.com/docs/concepts/olap
- `clickhouse-updates`: https://clickhouse.com/docs/updating-data/overview
- `clickhouse-transactions`: https://clickhouse.com/docs/guides/developer/transactional
- `clickhouse-cloud`: https://clickhouse.com/docs/cloud/overview, https://clickhouse.com/docs/cloud/reference/shared-merge-tree
- `clickhouse-iceberg`: https://clickhouse.com/docs/guides/use-cases/data-warehousing/support-matrix
- `analytics-github`: `duckdb/duckdb`, `chdb-io/chdb`, `apache/datafusion`, `pola-rs/polars`, `ClickHouse/ClickHouse`, `apache/doris`, `StarRocks/starrocks`, `apache/druid`, `apache/pinot`, `questdb/questdb`, `apache/iceberg`, `delta-io/delta`, `apache/polaris`, `lakekeeper/lakekeeper`
- `analytics-github` continued: `opensearch-project/OpenSearch`, `meilisearch/meilisearch`, `typesense/typesense`, `apache/solr`, `qdrant/qdrant`, `milvus-io/milvus`, `weaviate/weaviate`, `chroma-core/chroma`, `lancedb/lancedb`, `trinodb/trino`
- `opsdb-github`: `mysql/mysql-server`, `MariaDB/server`, `sqlite/sqlite`, `tursodatabase/turso`, `mongodb/mongo`, `FerretDB/FerretDB`, `documentdb/documentdb`, `apache/cassandra`, `valkey-io/valkey`, `redis/redis`, `Snapchat/KeyDB`, `memcached/memcached`, `microsoft/garnet`, `neo4j/neo4j`, `memgraph/memgraph`, `FalkorDB/FalkorDB`, `kuzudb/kuzu`
- `documentdb-lf`: https://www.linuxfoundation.org/press/linux-foundation-welcomes-documentdb-to-advance-open-developer-first-nosql-innovation
- `scylla-license`: https://www.scylladb.com/2024/12/18/why-were-moving-to-a-source-available-license
- `redis-license`: https://github.com/redis/redis/blob/8.10.3/LICENSE.txt
- `valkey`: https://valkey.io/topics/migration, v9.1.3, https://docs.cloud.google.com/memorystore/docs/valkey/product-overview
- `valkey-persistence`: https://valkey.io/topics/persistence
- `redis-worst`: https://redis.io/blog/7-redis-worst-practices
- `dragonfly-license`: https://github.com/dragonflydb/dragonfly LICENSE, v2.1
- `timescale-rename`: https://www.tigerdata.com/blog/timescale-becomes-tigerdata (2025-06-17)
- `influxdb3`: https://docs.influxdata.com/influxdb3/core/install/, v3.12.2
- `elastic-license`: https://github.com/elastic/elasticsearch/blob/main/LICENSE.txt, v9.5.6
- `opensearch-lf`: https://forum.opensearch.org/t/21476 (2024-09-16)
- `meilisearch-ee`: https://www.meilisearch.com/blog/enterprise-license (2025-08-27)
- `weaviate-license`: https://github.com/weaviate/weaviate/blob/main/LICENSE, commit e0cc80ce0
- `vector-hosted`: https://docs.pinecone.io, https://turbopuffer.com/docs/architecture
- `databricks-lake`: https://www.databricks.com/discover/data-lakes
- `databricks-warehouse`: https://www.databricks.com/glossary/data-warehouse
- `lakehouse-paper`: https://www.cidrdb.org/cidr2021/papers/cidr2021_paper17.pdf, CIDR 2021
- `iceberg-spec`: https://iceberg.apache.org/spec/, format version 3, release 1.12.0
- `iceberg-rest`: https://iceberg.apache.org/rest-catalog-spec
- `warehouses`: https://docs.snowflake.com/en/user-guide/intro-key-concepts, https://cloud.google.com/bigquery/docs/introduction, https://docs.aws.amazon.com/redshift/latest/mgmt/welcome.html, https://docs.databricks.com/aws/en/sql/, https://motherduck.com/docs/getting-started/
- `aws-storage-types`: https://aws.amazon.com/compare/the-difference-between-block-file-object-storage/
- `s3-consistency`: https://docs.aws.amazon.com/AmazonS3/latest/userguide/Welcome.html
- `s3-conditional`: https://docs.aws.amazon.com/AmazonS3/latest/userguide/conditional-writes.html
- `s3-compat`: https://www.tigrisdata.com/docs/api/s3.md, with `r2-s3`, `spaces-s3`, and `gcs-interop`
- `r2-s3`: https://developers.cloudflare.com/r2/api/s3/api/, https://developers.cloudflare.com/r2/buckets/bucket-locks/
- `spaces-s3`: https://docs.digitalocean.com/products/spaces/reference/s3-compatibility/
- `gcs-interop`: https://docs.cloud.google.com/storage/docs/interoperability
- `azure-s3`: https://learn.microsoft.com/en-us/answers/questions/1183760/s3-api-support-over-azure-blob-storage (2023-02-23)
- `object-hosted`: https://developers.cloudflare.com/r2/pricing/, https://www.backblaze.com/cloud-storage/transaction-pricing, https://wasabi.com/cloud-object-storage, https://www.tigrisdata.com/pricing/, https://docs.hetzner.com/storage/object-storage/overview/, https://www.ovhcloud.com/en/public-cloud/object-storage/, https://www.scaleway.com/en/docs/object-storage/how-to/use-object-lock/, https://storj.dev/dcs/pricing/simplified, https://www.idrive.com/s3-storage-e2/object-lock
- `minio-readme`: https://github.com/minio/minio README, archived, last release 2025-10-15, and https://min.io/download
- `garage-goals`: https://garagehq.deuxfleurs.fr/documentation/design/goals/, v2.5.0
- `storage-github`: `seaweedfs/seaweedfs`, `rustfs/rustfs`, `ceph/ceph`, `rook/rook`, `versity/versitygw`, `pgsty/silo`, `longhorn/longhorn`, `openebs/openebs`
- `ebs`: https://docs.aws.amazon.com/ebs/latest/userguide/ebs-volumes.html
- `ec2-instance-store`: https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/instance-store-lifetime.html
- `hetzner-volumes`: https://docs.hetzner.com/cloud/volumes/faq/
- `btrfs`: https://btrfs.readthedocs.io/en/stable/Subvolumes.html

Messaging:
- `messaging-github`: `apache/kafka`, `redpanda-data/redpanda`, `AutoMQ/automq`, `apache/pulsar`, `nats-io/nats-server`, `rabbitmq/rabbitmq-server`, `pgmq/pgmq`, `riverqueue/river`, `graphile/worker`, `timgit/pg-boss`, `oban-bg/oban`, `rails/solid_queue`, `bensheldon/good_job`, `taskforcesh/bullmq`, `sidekiq/sidekiq`
- `messaging-github` continued: `temporalio/temporal`, `restatedev/restate`, `inngest/inngest`, `dbos-inc/dbos-transact-py`, `hatchet-dev/hatchet`, `triggerdotdev/trigger.dev`, `debezium/debezium`, `PeerDB-io/peerdb`, `sequinstream/sequin`, `svix/svix-webhooks`, `hookdeck/outpost`, `frain-dev/convoy`, `centrifugal/centrifugo`, `eclipse-mosquitto/mosquitto`, `emqx/emqx`
- `kafka-intro`: https://kafka.apache.org/0101/getting-started/introduction, 0.10.1 docs
- `kafka-40`: https://kafka.apache.org/40/getting-started/upgrade
- `kafka-42`: https://kafka.apache.org/42/getting-started/upgrade
- `rabbitmq-streams`: https://www.rabbitmq.com/docs/streams, 4.4
- `nats-jetstream`: https://docs.nats.io/reference/jetstream/, v2.15.3
- `nats-cncf`: https://www.cncf.io/blog/2025/05/01/protecting-nats-and-the-integrity-of-open-source-cncfs-commitment-to-the-community
- `redpanda-readme`: https://github.com/redpanda-data/redpanda README, 2026-11-12
- `redpanda-licensing`: https://docs.redpanda.com/streaming/current/get-started/licensing/overview
- `warpstream`: https://docs.warpstream.com/warpstream
- `warpstream-confluent`: https://www.confluent.io/press-release/confluent-acquires-warpstream-to-advance-next-gen-byoc-data-streaming (2024-09-09)
- `temporal-durable`: https://temporal.io/blog/durable-execution-in-distributed-systems-increasing-observability
- `temporal-persistence`: https://docs.temporal.io/temporal-service/persistence, v1.32.2
- `dbos-arch`: https://docs.dbos.dev/architecture
- `hatchet-arch`: https://docs.hatchet.run/v1/architecture-and-guarantees
- `restate-server`: https://docs.restate.dev/server/overview, v1.8.1
- `trigger-selfhost`: https://trigger.dev/docs/self-hosting/kubernetes, v4.8.5
- `sequin-notion`: https://www.notion.com/blog/welcome-sequin (2026-01-09)
- `debezium-commonhaus`: https://debezium.io/blog/2024/11/04/debezium-moving-to-commonhaus
- `electric-databricks`: https://www.databricks.com/blog/electric-joins-databricks-bring-wasm-postgres-ai-agent-sandboxes (2026-08-11)
- `emqx-busl`: https://github.com/emqx/emqx LICENSE, 6.3.3
- `cloudevents`: https://cloudevents.io/, specification 1.0.2
- `std-webhooks`: https://www.standardwebhooks.com/, v1.0.2
- `asyncapi`: https://github.com/asyncapi/spec, v3.1.0

Telemetry:
- `telemetry-github`: `open-telemetry/opentelemetry-collector`, `grafana/alloy`, `vectordotdev/vector`, `fluent/fluent-bit`, `prometheus/prometheus`, `prometheus/alertmanager`, `VictoriaMetrics/VictoriaMetrics`, `VictoriaMetrics/VictoriaLogs`, `VictoriaMetrics/VictoriaTraces`, `grafana/mimir`, `grafana/loki`, `grafana/tempo`, `grafana/pyroscope`, `grafana/grafana`
- `telemetry-github` continued: `thanos-io/thanos`, `cortexproject/cortex`, `jaegertracing/jaeger`, `parca-dev/parca`, `SigNoz/signoz`, `ClickHouse/ClickStack`, `openobserve/openobserve`, `uptrace/uptrace`, `coroot/coroot`, `metrico/gigapipe`, `GreptimeTeam/greptimedb`, `quickwit-oss/quickwit`, `perses/perses`, `getsentry/sentry`, `bugsink/bugsink`, `PostHog/posthog`, `plausible/analytics`, `umami-software/umami`, GitLab `glitchtip/glitchtip-backend`
- `otel-what`: https://opentelemetry.io/docs/what-is-opentelemetry/
- `otel-graduation`: https://www.cncf.io/announcements/2026/05/21/cloud-native-computing-foundation-announces-opentelemetrys-graduation-solidifying-status-as-the-de-facto-observability-standard
- `otel-status`: https://opentelemetry.io/docs/specs/status, https://opentelemetry.io/docs/specs/otlp (1.11.1), https://opentelemetry.io/blog/2026/profiles-alpha
- `otel-components`: https://opentelemetry.io/docs/concepts/components, Collector v0.165.0
- `otel-distributions`: https://opentelemetry.io/docs/collector/distributions
- `grafana-agent-eol`: https://grafana.com/docs/agent/latest
- `promtail-eol`: https://grafana.com/docs/loki/latest/send-data/promtail
- `prom-overview`: https://prometheus.io/docs/introduction/overview/, v3.16.0
- `prom-otlp`: https://prometheus.io/docs/guides/opentelemetry
- `vm-docs`: https://docs.victoriametrics.com/victoriametrics/, v1.156.0
- `vm-faq`: https://docs.victoriametrics.com/victoriametrics/faq
- `mimir-arch`: https://grafana.com/docs/mimir/latest/get-started/about-grafana-mimir-architecture, 3.2.4
- `thanos`: https://github.com/thanos-io/thanos README, v0.42.5
- `cortex`: https://github.com/cortexproject/cortex, v1.21.1
- `tempo-blocks`: https://grafana.com/docs/tempo/latest/reference-tempo-architecture/block-format
- `dynatrace-grail`: https://docs.dynatrace.com/docs/discover-dynatrace/platform/grail
- `signoz-arch`: https://signoz.io/docs/architecture, v0.149.0
- `signoz-db`: https://signoz.io/docs/manage/administrator-guide/configuration/relational-database/
- `clickstack`: https://github.com/ClickHouse/ClickStack
- `clickhouse-hyperdx`: https://clickhouse.com/blog/clickhouse-acquires-hyperdx-the-future-of-open-source-observability (2025-03-13)
- `uptrace`: https://uptrace.dev/get/hosted
- `coroot-arch`: https://docs.coroot.com/installation/architecture
- `openobserve-arch`: https://openobserve.ai/docs/architecture
- `greptime`: https://docs.greptime.com/user-guide/concepts/overview
- `sentry-compose`: https://github.com/getsentry/self-hosted/blob/26.10.1/docker-compose.yml
- `sentry-fsl`: https://blog.sentry.io/introducing-the-functional-source-license-freedom-without-free-riding/ (2023-11-17)
- `posthog-arch`: https://posthog.com/docs/how-posthog-works
- `plausible`: https://github.com/plausible/analytics README, v3.2.1
- `healthchecks`: https://healthchecks.io/faq
- `glitchtip`: https://glitchtip.com/documentation/install, v7.0.0
- `polarsignals-dash0`: https://www.polarsignals.com/blog/posts/2026/08/17/polar-signals-is-joining-dash0
- `quickwit-datadog`: https://quickwit.io/blog/quickwit-joins-datadog
- `oncall-archive`: https://grafana.com/blog/grafana-oncall-maintenance-mode
- `opsgenie-eol`: https://www.atlassian.com/software/opsgenie/pricing
- `highlight`: https://launchdarkly.com/blog/welcome-highlight-to-launchdarkly, https://highlight.io
- `openai-clickhouse`: https://clickhouse.com/blog/why-openai-uses-clickhouse-for-petabyte-scale-observability
- `cloudflare-logs`: https://blog.cloudflare.com/log-analytics-using-clickhouse/ (2022-09-02)
- `roblox-vm`: https://docs.victoriametrics.com/victoriametrics/casestudies/

Machines:
- `providers`: https://docs.hetzner.com/cloud/servers/overview/, https://www.ovhcloud.com/en/public-cloud/kubernetes/, https://www.scaleway.com/en/all-products/, https://docs.digitalocean.com/, https://docs.vultr.com/products, https://techdocs.akamai.com/cloud-computing/docs/compute-instance, https://upcloud.com/products/, https://www.latitude.sh/, https://www.netcup.com/en, https://contabo.com/en/, https://www.hetzner.com/blog/production-ready-kubernetes-auf-hetzner-cloud-ccm-csi-und-das-open-source-oekosystem-richtig-nutzen/ (2026-11-09)
- `tf-registry`: https://registry.terraform.io/v1/providers/ entries per provider
- `equinix-metal`: https://docs.equinix.com/goodbye-metal/
- `proxmox`: https://pve.proxmox.com/wiki/Roadmap, 9.2, and https://pve.proxmox.com/wiki/FAQ
- `incus`: https://github.com/lxc/incus README, v7.6.0
- `lxd-relicense`: https://discourse.ubuntu.com/t/lxd-5-20-has-been-released/40865
- `os-releases`: https://www.debian.org/releases/, https://changelogs.ubuntu.com/meta-release-lts, https://www.flatcar.org/releases-json/releases-stable.json, https://nixos.org/blog/announcements/
- `flatcar-cncf`: https://www.cncf.io/projects/flatcar-container-linux
- `machines-github`: `canonical/cloud-init`, `coreos/ignition`, `ansible/ansible`, `pyinfra-dev/pyinfra`, `saltstack/salt`, `hashicorp/packer`, `systemd/mkosi`, `bootc-dev/bootc`, `nix-community/nixos-anywhere`, `nix-community/colmena`, `serokell/deploy-rs`, `nlewo/comin`, `siderolabs/talos`, `bottlerocket-os/bottlerocket`
- `butane`: https://coreos.github.io/butane/release-notes/
- `chef-license`: https://docs.chef.io/client/19/license/
- `openvox`: https://voxpupuli.org/blog/2025/06/14/an-unsupportable-path/, https://www.puppet.com/blog/open-source-puppet-updates-2025
- `37signals-exit`: https://world.hey.com/dhh/the-big-cloud-exit-faq-20274010
- `37signals-savings`: https://world.hey.com/dhh/our-cloud-exit-savings-will-now-top-ten-million-over-five-years-c7d9b5bd (2024-10-17)
- `netflix-aws`: https://about.netflix.com/en/news/completing-the-netflix-cloud-migration

Delivery:
- `delivery-github`: `hashicorp/terraform`, `opentofu/opentofu`, `pulumi/pulumi`, `aws/aws-cdk`, `Azure/bicep`, `crossplane/crossplane`, `anomalyco/sst`, `alchemy-run/alchemy`, `systeminit/si`, `repository-settings/app`, `github-community-projects/safe-settings`, `argoproj/argo-cd`, `fluxcd/flux2`, `kubernetes-sigs/kustomize`
- `delivery-github` continued: `buildpacks/pack`, `railwayapp/railpack`, `ko-build/ko`, `GoogleContainerTools/jib`, `chainguard-dev/apko`, `bazel-contrib/rules_oci`, `dagger/dagger`, `GoogleContainerTools/kaniko`, `moby/buildkit`, `project-zot/zot`, `goharbor/harbor`, `distribution/distribution`, `oras-project/oras`, `sigstore/cosign`, `zizmorcore/zizmor`, `suzuki-shunsuke/pinact`, `dependabot/dependabot-core`, `go-gitea/gitea`, `woodpecker-ci/woodpecker`
- `tf-state`: https://github.com/hashicorp/web-unified-docs, `content/terraform/v1.14.x/docs/language/state/index.mdx`
- `tf-license`: https://github.com/hashicorp/terraform/blob/main/LICENSE, v1.17.2
- `tofu-state`: https://github.com/opentofu/opentofu, `website/docs/language/state/purpose.mdx`, v1.13.9
- `tofu-cncf`: https://www.cncf.io/projects/opentofu
- `tofu-encryption`: https://opentofu.org/docs/language/state/encryption
- `pulumi-how`: https://www.pulumi.com/docs/iac/guides/basics/how-pulumi-works, v3.276.0
- `pulumi-hcl`: https://www.pulumi.com/blog/all-iac-including-terraform-and-hcl/ (2025-12-18)
- `pulumi-state`: https://www.pulumi.com/docs/iac/concepts/state-and-backends
- `pulumi-any-tf`: https://www.pulumi.com/docs/iac/concepts/providers/any-terraform-provider
- `pulumi-automation`: https://www.pulumi.com/docs/iac/concepts/automation-api
- `sst-v3`: https://sst.dev/blog/sst-v3, https://sst.dev/docs/migrate-from-v3
- `alchemy`: https://alchemy.run/blog/2026-07-02-cloudflare-resource-factory, v2.0.0-beta.86
- `cdktf`: https://developer.hashicorp.com/terraform/cdktf
- `wing`: https://www.calcalistech.com/ctechnews/article/bj90wnmrjl (2025-04-09)
- `opengitops`: https://github.com/open-gitops/documents, `PRINCIPLES.md`, v1.0.0
- `flux-support`: https://www.cncf.io/announcements/2024/03/19/cloud-native-computing-foundations-fluxcd-project-gains-new-corporate-support/
- `watchtower`: https://github.com/containrrr/watchtower/discussions/2135
- `helm4`: https://helm.sh/blog/helm-4-released/ (2025-11-12)
- `buildpacks-cncf`: https://www.cncf.io/projects/buildpacks/
- `railpack`: https://blog.railway.com/p/introducing-railpack, https://nixpacks.com/
- `dotnet-containers`: https://learn.microsoft.com/en-us/dotnet/core/containers/sdk-publish
- `earthly`: https://earthly.dev/blog/shutting-down-earthfiles-cloud/
- `gh-pricing`: https://github.blog/changelog/2025-12-16-coming-soon-simpler-pricing-and-a-better-experience-for-github-actions/, https://docs.github.com/en/billing/concepts/product-billing/github-actions
- `dockerhub-limits`: https://docs.docker.com/docker-hub/usage/
- `tj-actions`: https://github.com/advisories/GHSA-mrrh-fwg8-r2c3
- `trivy-incident`: https://github.com/aquasecurity/trivy/security/advisories/GHSA-69fq-xp46-6x23
- `slsa`: https://slsa.dev/blog/2025/11/announce-slsa-v1.2
- `renovate`: https://docs.renovatebot.com/getting-started/running/, 44.236.4
- `forgejo`: https://forgejo.org/faq/, v17.0.1

Protection:
- `protection-github`: `restic/restic`, `restic/rest-server`, `kopia/kopia`, `borgbackup/borg`, `rustic-rs/rustic`, `PlakarKorp/plakar`, `rclone/rclone`, `offen/docker-volume-backup`, `velero-io/velero`, `zrepl/zrepl`, `jimsalterjrs/sanoid`, `Altinity/clickhouse-backup`, `benbjohnson/litestream`
- `backblaze-321`: https://www.backblaze.com/learning-center/3-2-1-backup-strategy
- `veeam-32110`: https://bp.veeam.com/vbr/1_Assessment/A_Infra_Overview/infra_overview.html
- `aws-rto`: https://docs.aws.amazon.com/wellarchitected/latest/reliability-pillar/disaster-recovery-dr-objectives.html
- `mariadb-replication`: https://mariadb.com/docs/server/server-usage/backup-and-restore/replication-as-a-backup-solution
- `gitlab-2017`: https://about.gitlab.com/blog/postmortem-of-database-outage-of-january-31/ (2017-02-10)
- `velero-cncf`: `cncf-landscape` entry for Velero
- `pbs`: https://proxmox.com/en/about/company-details/press-releases/proxmox-backup-server-4-2

Bought services:
- `bought-github`: `postalserver/postal`, `stalwartlabs/stalwart`, `axllent/mailpit`, `binwiederhier/ntfy`, `novuhq/novu`, `getlago/lago`, `killbill/killbill`, `Flagsmith/flagsmith`, `Unleash/unleash`, `growthbook/growthbook`, `flipt-io/flipt`, `open-feature/flagd`, `imgproxy/imgproxy`, `thumbor/thumbor`, `BerriAI/litellm`, `Portkey-AI/gateway`, `appwrite/appwrite`, `pocketbase/pocketbase`, `convex/convex-backend`, `nhost/nhost`
- `email-rfcs`: https://www.rfc-editor.org/ entries for RFC 5321, 7208, 6376, 9989, 9990, 9991
- `paddle`: https://developer.paddle.com/get-started/how-paddle-works/saas
- `polar`: https://polar.sh/resources/comparison/paddle
- `lemonsqueezy`: https://www.lemonsqueezy.com/blog/2026-update
- `openfeature`: `cncf-landscape` entry for OpenFeature
- `statsig`: https://www.statsig.com/blog/openai-acquisition
- `portkey`: https://www.paloaltonetworks.com/company/press/2026/palo-alto-networks-completes-acquisition-of-portkey-to-secure-ai-agents
- `supabase-compose`: https://github.com/supabase/supabase, `docker/docker-compose.yml` at v1.26.08
- `firebase`: https://firebase.google.com/products-build

Environments:
- `environments-github`: `testcontainers/testcontainers-java` 2.0.5, `testcontainers-node` v12.2.1, `testcontainers-dotnet` 4.16.0, `testcontainers-python` 4.15.0, `testcontainers-rs` 0.28.0, `tilt-dev/tilt`, `GoogleContainerTools/skaffold`, `devspace-sh/devspace`, `telepresenceio/telepresence`, `metalbear-co/mirrord`, `cachix/devenv`, `F1bonacc1/process-compose`, `getmoto/moto`, `89luca89/distrobox`, `containers/toolbox`
- `tc-start`: https://testcontainers.com/getting-started/
- `tc-junit`: https://java.testcontainers.org/test_framework_integration/junit_5/
- `tc-reuse`: https://java.testcontainers.org/features/reuse/
- `tc-runtimes`: https://java.testcontainers.org/supported_docker_environment/
- `tc-docker`: https://www.docker.com/press-release/testing-shifts-left-with-docker-acquisition-of-atomicjar/ (2023-12-11)
- `vercel-env`: https://vercel.com/docs/deployments/environments
- `devcontainers`: https://containers.dev/, https://github.com/devcontainers/spec
- `codespaces`: https://docs.github.com/en/codespaces/about-codespaces/what-are-codespaces
- `sandboxes`: https://e2b.dev/docs/llms.txt, https://modal.com/docs/guide/sandboxes.md, https://developers.cloudflare.com/sandbox/, https://docs.fly.io/sprites/index.md
- `daytona`: https://github.com/daytonaio/daytona README, archived, last release v0.190.0
- `tilt`: https://docs.tilt.dev/product_faq.html
- `localstack`: https://blog.localstack.cloud/2026-upcoming-pricing-changes/, https://floci.io/aws/

Published architectures:
- `gitlab-arch`: https://docs.gitlab.com/development/architecture/

Additions from review:
- `extras-github`: `Kong/kong` 3.9.4, `apache/apisix` 3.20.0, `apache/flink`, `risingwavelabs/risingwave` v3.1.1, `strimzi/strimzi-kafka-operator` 1.3.1, `PostgREST/postgrest` v16.5, `hasura/graphql-engine` v2.51.6, `airbytehq/airbyte` v2.0.0, `dlt-hub/dlt` 1.32.0, `dbt-labs/dbt-core` v1.12.7
- `extras-github` continued: `vllm-project/vllm` v0.33.0rc3, `ollama/ollama` v0.40.16, `coreruleset/coreruleset` v4.30.0, `crowdsecurity/crowdsec` v1.8.2, `falcosecurity/falco` 0.45.2, `chaos-mesh/chaos-mesh` v2.8.8, `litmuschaos/litmus` 3.33.0, `kyverno/kyverno` v1.20.0, `open-policy-agent/gatekeeper` v3.23.1, `kedacore/keda` v2.21.0, `knative/serving` v1.24.0, `opencost/opencost` v1.121.4, `backstage/backstage` v1.56.5
