# [ORIGIN]

Record of the owner's words that started this work, kept as they were written on 2026-11-12. It binds nothing: `BRIEF.md` states the product, and decisions are made there.

## [01]-[PRODUCT]

Packages a project imports to build every part of its backend in code. Owner's words for it:
- "a meta system for building anything we need/want via code, and the thing itself is then easy to make any of"
- "Infrastructure as pure code only, no docker files, composing headache, no yml, just pure code"
- "To set a host, create a container for w/e i want, to create the 'app'"
- "the backend itself isnt one monolithic thing, but for each component of it, its just one shape/etc and its a plug-in-play thing"
- "an app for example, only needs to just simply lists, here is the rows of what i want, thats it"
- "consumed like a build a <thing> all fields/etc is done for you, you just set configs for things/choices"
- "a thing to easily build all things backend/systemd (daemon, container, etc)"

Reach the owner named: a code host repository, remote machines and their structure, remote management that is "dead simple" for an agent, a self-hosted or hosted choice for each part, any number of hosts, containers made for any purpose, apps, and delivery with no "manual deploying, hand syncing".

Comparison to Pulumi is general. Owner's words: "the pilumi like thing shouldn't be misunderstood i meant it generally". It names what kind of product this is, a set of packages a project builds with, and settles nothing about engines or tools.

## [02]-[STATEMENTS]

Statements the owner made while describing it, each beside a plain reading:

| [INDEX] | [READING]                                                            | [OWNER_WORDS]                                                            |
| :-----: | :------------------------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | New product in a new repository, planned before it is built          | "we will map it out and fully define/understand the end state"           |
|  [02]   | Documents are written here and move there                            | "we will do our resarch/etc here, but make a new repo and move to docs"  |
|  [03]   | Rasm is fixed afterwards, as a consumer                              | "then we can finally come back to rasm and fix things"                   |
|  [04]   | Workers and daemons are separate packages, uncoupled from the others | "they just have the fields/etc to accept a 'thing' if needed"            |
|  [05]   | Vendor integrations are packages of this product                     | "not adapters for something like hostinger but its package intefration"  |
|  [06]   | Docker is the one container engine                                   | "jus docker"                                                             |
|  [07]   | Colima on a Mac is set up by this product, Linux runs Docker         | "on a mac, it has colima fully confugred ... on linux it just is docker" |
|  [08]   | macOS and Linux alone                                                | "its just unix, mac/linux keeps it simpler"                              |
|  [09]   | No dependence on one project's mise or pitchfork setup               | "shouldn't need a mise/pitchfork/etc setup we have now"                  |
|  [10]   | Cluster or not is set once at the highest level                      | "the highest level thing defined/set, so the subsequent shapes ..."      |
|  [11]   | Hosts are a field, any number of them                                | "a filed to set a host, so we can jusst swap ... even if we define 100"  |
|  [12]   | Self-hosted or hosted is a higher-level value with a default         | "so its not a per line item thing"                                       |
|  [13]   | Each part arrives fully set, and a consumer writes one row           | "sockets, ports, priviliege/etc its all set"                             |
|  [14]   | Each category holds its best open-source members                     | "the newest/most capable so that we can set those as choice"             |
|  [15]   | Daemons are declared like a logger and accepted by other shapes      | "you can build/make a daemon like a logger and its confgired already"    |
