# [BRIEF]

Brief for a new product built in its own repository. It states what the product is, which problems it solves, how a project uses it, and what is decided. Other documents in this folder supply research and design in support of it, and `ORIGIN.md` keeps the words that started it.

## [01]-[PRODUCT]

Packages that a project imports to declare and run its whole backend in code: machines, containers, backend parts (a database, object storage, a cache, a reverse proxy), apps, background processes, and background work.

Consumers state what they want as short declarations with optional values. Packages hold everything else: correct settings for each part, differences between one computer and another, how a thing is started and kept running, and how a declaration reaches a real system.

It is one software development kit for a backend: separate packages built to work as one, in the way a logging library is a product for logging. A project sets it up once where it starts, and from then on asks for a machine, a container, a database, a daemon, or a worker and receives one that is already configured.

## [02]-[PROBLEMS]

Problems of present practice that it removes:

| [INDEX] | [PROBLEM]                  | [TODAY]                                                                               |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Assembly by hand           | Build files, service files, YAML, and shell scripts written and kept per project      |
|  [02]   | Parts are never ready      | Each project sets up its database again: version, extensions, sockets, roles, backup  |
|  [03]   | Place is baked in          | Moving a part from a project's own machine to a vendor service rewrites code          |
|  [04]   | Machines hold everything   | One machine definition lists every service, and a second machine is a copy            |
|  [05]   | Computers differ           | Mac and Linux computers run containers differently, and projects carry the fix        |
|  [06]   | No form for a whim         | Sandboxes and lone databases for a trial are made by hand and forgotten               |
|  [07]   | Delivery by hand           | Changes reach real systems through commands a person types in a certain order         |
|  [08]   | Background work is tangled | Daemons and workers are set up per operating system and per vendor, inside infra code |
|  [09]   | Remote work is hard        | Reaching a machine takes knowledge of keys, addresses, and file paths                 |

`current-state.md` records each row as it occurred in one repository.

## [03]-[GOALS]

- Declare, never assemble: consumer code holds declarations and values, and no person writes a container build file, a service file, or YAML
- Complete by default: each part arrives with version, sockets, ports, privileges, backup, and telemetry set, and a bare declaration is valid
- One choice, stated once: each choice sits at its highest level with a default, and everything below follows it
- Same on every supported computer: one declaration runs unchanged on a Mac and on Linux, and packages absorb every difference
- Small separate packages: each is usable alone, and packages join through fields that accept each other's values
- Nothing of one project inside: packages assume no build tool, tool manager, or convention of a consumer
- No delivery by hand: a merged change is applied, and a schedule finds drift
- Behavior, not passed values alone: lifetimes, restart, readiness, and expiry are logic inside packages that a consumer selects

Choices that are stated once:

| [INDEX] | [CHOICE]                   | [STATED_AT] | [EFFECT]                                              |
| :-----: | :------------------------- | :---------- | :---------------------------------------------------- |
|  [01]   | Self-hosted or hosted      | Environment | Every part takes that form, with exceptions per part  |
|  [02]   | Single machine or cluster  | Environment | Every self-hosted part takes its form for that choice |
|  [03]   | Which machine              | One field   | Parts and apps move by changing that value            |
|  [04]   | Which member of a category | Catalog     | Defaults serve until a consumer names another         |

## [04]-[PACKAGES]

Package names below are working names, and so is every section name of this brief. Product and package names are chosen later.

| [INDEX] | [PACKAGE]  | [PURPOSE]                                                      | [CONSUMER_STATES]               |
| :-----: | :--------- | :------------------------------------------------------------- | :------------------------------ |
|  [01]   | Builder    | Accounts, machines, environments, and apps, applied from code  | Rows for each                   |
|  [02]   | Catalog    | Ready backend parts by category, self-hosted and hosted        | Part name, with optional values |
|  [03]   | Containers | Docker on any supported computer, and containers with defaults | Image and values                |
|  [04]   | Daemons    | Processes started, kept running, and stopped                   | What runs, and lifecycle values |
|  [05]   | Workers    | Background work of an app, on its own machines or a vendor's   | Handlers and their triggers     |
|  [06]   | Fixtures   | Catalog part started for one test run                          | Part name                       |
|  [07]   | Operations | Connect, read logs, run a command, make a sandbox              | Name                            |
|  [08]   | Vendors    | One package per vendor a consumer uses                         | Its credentials                 |

Infrastructure builder holds the levels of a backend and keeps them apart:
- Account: a code host repository and its settings, DNS zones, a private network
- Machine: a rented or owned computer, declared as capacity alone
- Environment: a group of machines with its choices of section 03
- App: a built program and a list of parts it needs
- Machine rows name no service, and app rows name no machine, vendor, port, or credential

Parts catalog holds each backend part once, complete:
- Categories follow `backend-map.md`: relational database, object storage, cache, queue, reverse proxy, telemetry, identity, backup, and others
- Each category has a default member and replaceable alternatives, chosen as its best maintained open-source products
- Each member has a self-hosted form and, where vendors sell one, a hosted form behind one interface
- Self-hosted forms set every value themselves, and hosted forms call a vendor package that sets what that vendor exposes
- Options differ by form: some work in both, some exist self-hosted alone (an extension a vendor lacks), some at one vendor alone
- Requests for an option a chosen form lacks are refused at planning with that option named, before anything changes
- Versions default to the newest release of each part, and consumer code names no version
- Minor releases are taken as they appear
- Major releases of a part that stores data convert its stored files, and this product runs that conversion itself
- One setting states whether a major release is taken as it appears or held until a person allows it
- App database structure is declared as a wanted state, and this product compares it with a live database and applies each difference
- Consumers write no version file, migration file, or upgrade script for either case
- Apps ask for a share of a part (a database on a server, a bucket in a store), and an environment owns that part

Containers holds Docker, and Docker alone:
- Consumers declare a container, and it runs on whichever supported computer executes that code
- Linux runs Docker directly, and a Mac needs a Linux guest, which this package installs, configures, and runs through Colima
- Consumers never state which case applies and write no configuration for either
- Defaults for logging, restart, health, and limits are set once here
- Containers made for no app (a sandbox, a lone database for a trial) use this package directly
- Lifetime is logic of this package, chosen by one value: ephemeral, expiring, or stable
- Ephemeral containers are removed when they stop or their session closes
- Expiring containers are removed after a stated time, and a stated condition (activity) can renew that time
- Stable containers stay until a person removes them

Daemons holds one declaration for a process that must stay running:
- Fields are what runs, when it starts, how it restarts, how readiness is checked, how it stops, and where logs go
- Each field has a default
- Where a daemon is placed decides what keeps it running: launchd on a Mac, systemd on Linux, Docker for a container, a cluster for cluster workloads
- Consumers never name one of those or write a file for it

Workers holds background work as app code:
- Handlers take a request, a queue message, or a schedule
- One handler runs on a consumer's own machines and on a vendor's function platform
- Retry, acknowledgment, shutdown, and telemetry are written once here
- This package knows nothing of infrastructure, and an app using it alone supplies queue addresses itself

Operations gives remote work one form: a person or an agent names a machine, part, or app, and connects, reads logs, runs a command, or makes a sandbox, with addresses and keys derived.

Vendors holds each integration as a package of this product (a machine vendor, a code host, a DNS host, a secret store, a hosted service), and no consumer writes an adapter for one.

## [05]-[USE]

Sketches show shape alone. Language and names are undecided.

Machines and one environment:

```typescript
machine('<machine>', { vendor: '<vendor>', size: '<plan>' });
environment('production', { machines: ['<machine>'], provision: 'self-hosted', runtime: 'engine' });
```

App and what it needs:

```typescript
app('<app>', {
    needs: { DB: postgres({ extensions: ['vector'] }), FILES: bucket(), CACHE: cache() },
});
```

Container for any purpose, identical on a Mac and on Linux:

```typescript
container('<name>', { image: '<image>', lifetime: expiring(Duration.days(2), { renew: 'activity' }) });
```

Docker with a daemon, where both packages are imported and joined by one field:

```typescript
engine({ daemon: daemon({ start: 'login', restart: 'on-failure' }) });
```

Worker, stated by an app and run wherever that app runs:

```typescript
worker('<name>', { queue: '<queue>', handle: (message) => handled(message) });
```

What a consumer does not do in any of these: write a build file or service file, state a socket or port, branch on operating system, configure a tool manager, or type a deploy command.

## [06]-[JOINING]

Packages stay apart and meet through fields:
- No package imports logic of another
- Fields of one package accept values another package produces: engine takes a daemon, app takes workers, catalog member takes a container
- Each such field has a default, so a consumer of one package never needs another
- Importing a second package adds control: Docker starts on first use by default, and a daemon value sets start at login, restart, and stop behavior

## [07]-[PEERS]

Products that cover one part of this one. None matches it whole, and each shows that its part works in practice:

| [INDEX] | [PEER]                         | [COVERS]                                         | [STOPS_AT]                            | [PACKAGE]  |
| :-----: | :----------------------------- | :----------------------------------------------- | :------------------------------------ | :--------- |
|  [01]   | Pulumi                         | Vendor resources declared in real code           | Machine contents, parts, lifetimes    | Builder    |
|  [02]   | SST                            | Own components and CLI over an existing engine   | AWS and Cloudflare components         | Builder    |
|  [03]   | Aspire                         | App and its needs in code: run, test, deploy     | Machines and repository settings      | Builder    |
|  [04]   | Encore                         | Resources declared in app code, test databases   | Its own framework and paid cloud      | Builder    |
|  [05]   | Radius, Score                  | App asks for a type, environment picks its form  | YAML, and Radius needs Kubernetes     | Catalog    |
|  [06]   | NixOS, comin, nixos-anywhere   | Machine and services in code, applied from Git   | Hosted vendors, a Mac, app requests   | Builder    |
|  [07]   | Pigsty, CloudNativePG          | PostgreSQL with extensions and backup preset     | One part, on Ansible or Kubernetes    | Catalog    |
|  [08]   | Kamal, Coolify, Dokploy        | Apps and databases on a person's own machines    | YAML or a dashboard, no drift check   | Builder    |
|  [09]   | Dagger                         | Modules accepting each other's values            | Lifetime of one pipeline run          | Joining    |
|  [10]   | Colima                         | Docker on a Mac inside a Linux guest             | Set up by each person or project      | Containers |
|  [11]   | Incus                          | Containers and VMs, ephemeral on request         | Imperative commands                   | Containers |
|  [12]   | Coder                          | Sandbox from a template, stopped when idle       | Development workspaces                | Operations |
|  [13]   | Hosted sandboxes (E2B, Modal)  | Sandbox through an API, stopped when idle        | Machines of that vendor alone         | Operations |
|  [14]   | Fly.io Machines                | Machine stopped when idle, started on request    | One vendor's platform                 | Containers |
|  [15]   | Testcontainers                 | Containers that live for one test run            | Tests alone                           | Fixtures   |
|  [16]   | `launchd`, `systemd`           | Process started, kept running, and stopped       | One system each, own file format      | Daemons    |
|  [17]   | Cloudflare Workers, job queues | Background work by request, queue, or schedule   | One vendor, or one language and store | Workers    |
|  [18]   | CloudNativePG, pgautoupgrade   | PostgreSQL major upgrade run without a person    | One part                              | Catalog    |
|  [19]   | Atlas, pg-schema-diff, pgroll  | Database structure applied from a declared state | Structure of one database kind        | Catalog    |

Package column uses working names of section 04. Rows rest on section 13 of `target-architecture.md`, sections 04 to 10 of `iac-tools.md`, and sections 04 and 13 of `backend-map.md`. That search covered about 20 systems and found none joining every part. Rows [18] and [19] rest on each project's README and on PostgreSQL's `pg_upgrade` page, read on 2026-11-12.

What this product adds is its shape, since most parts exist:
- One kit, where a project today combines five or more of those peers, each with its own file format and its own idea of a machine, a part, and an app
- One declaration style in code across machines, parts, containers, daemons, and workers
- One place for each choice of section 03, where each peer fixes that choice or leaves it to its user
- Lifetimes, restart, and readiness as built-in behavior of any container or part, where peers offer each for one kind of thing

## [08]-[SCOPE]

Inside:
- Developer computers running macOS or Linux, and machines running Linux
- Docker as container engine
- Single machines and clusters
- Self-hosted parts and hosted parts behind one interface

Outside:
- App frameworks and app code beyond worker handlers
- Operating a hosting service for others
- Conventions, build tools, or tool managers of any one consumer

## [09]-[SUCCESS]

| [INDEX] | [ACT]                                 | [COST_TO_A_CONSUMER]                           |
| :-----: | :------------------------------------ | :--------------------------------------------- |
|  [01]   | Add a machine                         | One row                                        |
|  [02]   | Add an app                            | Rows in that app's folder                      |
|  [03]   | Move a part to a vendor               | One value, where both forms hold its options   |
|  [04]   | Move a part to another machine        | One value                                      |
|  [05]   | Run a container on a Mac and on Linux | One declaration, no configuration per computer |
|  [06]   | Keep a process running                | One declaration, no file per operating system  |
|  [07]   | Deliver a change                      | One merge                                      |
|  [08]   | Try something in a sandbox            | One command, removed as its lifetime states    |
|  [09]   | Test against a real part              | Its name, with production's own image          |
|  [10]   | Reach a machine                       | Its name                                       |

## [10]-[DECISIONS]

- Product sits in its own repository and is planned before it is built
- Planning documents are written in this folder and move to that repository
- Packages of section 04 are separate, and workers and daemons are uncoupled from infrastructure
- Vendor integrations are packages of this product
- Docker is its one container engine, and its Mac guest is part of containers package
- Supported systems are macOS and Linux
- Container lifetime is ephemeral, expiring with an optional renewing condition, or stable
- Packages depend on no consumer's tool manager
- Single machine or cluster is one choice at environment level
- Machines are declared in any number and selected by one field
- Self-hosted or hosted is one choice at environment level with a default
- Each catalog part is complete by default, and each category holds its best open-source members
- Parts default to their newest release, and this product performs major upgrades and structure changes of stored data
- First consumer is Rasm, after this product exists

## [11]-[OPEN]

| [INDEX] | [QUESTION]                                           | [SETTLED_BY]                                          |
| :-----: | :--------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Every use of this product, as declaration and result | Planning, before any package is built                 |
|  [02]   | Default member of each category                      | Comparison on a target machine, from `backend-map.md` |
|  [03]   | Language or languages of its packages                | Languages of first consumers                          |
|  [04]   | Tools its packages stand on                          | Recommendations of `iac-tools.md`, none settled       |
|  [05]   | First environments: single machine, cluster, or both | Machine count of first consumer                       |
|  [06]   | Full form of operations package                      | Design beyond one table in `target-architecture.md`   |
|  [07]   | How containers package keeps its Mac guest running   | One trial on a Mac                                    |
|  [08]   | Marking of each option by form that supports it      | Catalog design, so a writer sees it before planning   |
|  [09]   | Default for a major release: taken or held           | Risk a first consumer accepts for stored data         |
|  [10]   | Whether newest includes release candidates           | Same                                                  |
|  [11]   | Names of this product and its packages               | Owner                                                 |

Further open questions of design sit in section 15 of `target-architecture.md`.

## [12]-[DOCUMENTS]

| [INDEX] | [FILE]                   | [HOLDS]                                                       | [STANDING]               |
| :-----: | :----------------------- | :------------------------------------------------------------ | :----------------------- |
|  [01]   | `BRIEF.md`               | This brief                                                    | Governs every other file |
|  [02]   | `target-architecture.md` | Design: packages, levels, declarations, lifetimes, costs      | One reviewer's proposal  |
|  [03]   | `backend-map.md`         | Every area and category of a backend, with candidates         | Reference                |
|  [04]   | `iac-tools.md`           | Tools packages could stand on, with recommendations           | One reviewer's proposal  |
|  [05]   | `current-state.md`       | Audit of Rasm's present infrastructure, which stays with Rasm | Evidence for section 02  |
|  [06]   | `ORIGIN.md`              | Owner's words that started this work                          | Record, binds nothing    |

Limits of those files:
- Each proposal and reference is one independent reviewer's work
- `target-architecture.md` rests on 9 sources read in full and others in part
- `iac-tools.md` and `backend-map.md` rest mostly on sources read by reading agents
- `backend-map.md` alone had an adversarial review, and no review covered these files as one set
