---
name: inspect
description: "Use when code for a vendor host (plug-in, extension, script, MCP client) needs a fact its documentation lacks or contradicts, covering evidence order across installed files, host state, binaries, and a running instance"
---

# [TOOL_INSPECT]

Investigation of closed vendor software reads evidence in cost order and stops at the first tier that settles the question.

- Use `tool:ghidra` for decompiling a function or emulating code
- Use `tool:watch-app` for launching, quitting, or reading the interface of a desktop app, Atelier for an Adobe app
- Use `terminal:history` for earlier sessions that probed the same host

## [01]-[QUESTION]

Investigations open with one written question before any command:
- Questions name host, build (release or Beta, version from `Info.plist`), and the code decision the answer changes
- Declaration (file format, manifest key, type, enumeration) -> answer sits in a file the vendor ships or the host writes
- Behavior (endpoint, return value, load order, error) -> answer comes from a running instance, after files name what to look for
- Gate (feature flag, tier, check that refuses a caller) -> answer sits in code that reads the flag, strings name it and decompiling decides it
- Memory files, `docs/research/` evidence, earlier sessions, and mazan-group `runbooks/tools/` precede every tier and hold most host facts

## [02]-[ORDER]

Each tier runs only when tiers above it leave the question open:

| [INDEX] | [TIER]        | [READS]                                                         | [SETTLES]                             |
| :-----: | :------------ | :-------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Identity      | `Info.plist`, entitlements, files found by name                 | Build, sandbox, scripting kind, paths |
|  [02]   | Declarations  | Manifests, JSON configs, shipped JS, type files, static `.sdef` | Declared API shape                    |
|  [03]   | Host state    | Logs, preferences, caches, crash reports                        | Behavior on its last run              |
|  [04]   | Publications  | Vendor docs, type packages, forums at the installed version     | Documented contracts, return values   |
|  [05]   | Binary text   | Strings, embedded JS and JSON, exports, .NET assemblies         | Candidate names, keys, paths          |
|  [06]   | Observation   | Ports, one protocol handshake, unified log                      | Served behavior, raw error codes      |
|  [07]   | Probe         | One launch, one change, readback, restore                       | Effect of one variable                |
|  [08]   | Decompilation | Function referencing a tier 5 string                            | Gate logic, evaluation order          |

Tier steps that decided past investigations:
- Identity: `fd` and `mdfind` find a file by name, a guessed path misses vendor folders with doubled or versioned names
- Identity: `codesign -d --entitlements - <host>` shows `disable-library-validation`, which decides whether a host loads a plug-in of another team
- Declarations: installed JS, JSON, and type files beat documentation, forums, and research summaries on any conflict
- Host state: one string searched across logs of every host of a vendor shows which host lacks a behavior the others log
- Host state: newest modification time among versioned preference folders names the folder a host reads
- Host state: `fq -d bplist 'torepr | from_ns_keyed_archiver' <file>` decodes keyed-archive preferences `plutil` prints as raw objects
- Publications: documented return value of a command comes before reproducing it as a defect
- Binary text: `rg -a -o '<literal>' <bundle>` with the exact key, path, or error message from the failure, then strings beside each hit
- Binary text: stack traces name internal script paths, and their source sits as plain text inside the host binary
- Binary text: `ipsw macho search <app> --sym '<regex>'` (`--class`, `--import`) names the Mach-O of a bundle holding a symbol before any read
- Binary text: declared members come from the binary's own metadata, never from strings alone:
    - Objective-C -> `ipsw class-dump <bin> --arch arm64` lists classes and selectors
    - Swift -> `swift-section interface -a arm64 <bin>` prints types, protocols, and conformances
    - C and C++ -> `nm -gU <bin> | c++filt` lists exports a host resolves from a plug-in
    - .NET -> `ilspycmd -t <Namespace.Type> <dll>` decompiles to source with no Ghidra
    - JavaScript -> shipped files, `asar extract-file` for Electron, `rg -a` for scripts inside a binary
- Observation: `lsof -nP -a -p <pid> -iTCP -sTCP:LISTEN` names each port per process, then one `initialize` and `tools/list` for an MCP server
- Observation: `log show --predicate` over the failing process and its daemons (`appleeventsd`, `tccd`) returns the raw OS error code
- Decompilation: `llvm-objdump` cross-references settle whether a slot or symbol is read, `tool:ghidra` the logic around it

## [03]-[EVIDENCE]

Evidence counts for its own tier alone:
- Strings and symbols prove code is compiled in -> confirm served behavior by observation
- Description strings beside a flag -> read its tier or value from code, never from its description
- Analogy across hosts, versions, or ports -> confirm each host and build on its own
- Shell reproduction of an app failure -> run it from the failing process, as privacy grants charge a shell run to the terminal
- Error swallowed by our code -> recover the raw OS code before forming a cause
- Web, forum, and research-agent claims -> treat as leads until an installed file or live read agrees

## [04]-[HAZARDS]

Reads that change host state take their safe form:

| [INDEX] | [READ]                                | [SAFE_FORM]                                                             |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | Dictionary read (`sdef`, `osascript`) | `Info.plist` key `OSAScriptingDefinitionIsDynamic`, then a disk `.sdef` |
|  [02]   | Tokens or paths guessed at a server   | Port per pid from `lsof`, auth and paths from tier 5 or 8               |
|  [03]   | Relaunch to retest                    | Probe naming its hypothesis and readback before launch                  |
|  [04]   | Extraction into working folder        | Scratch folder as working directory                                     |
|  [05]   | Code against an unproven route        | Probe proving the route before any library or product code              |

## [05]-[STOPPING]

Investigations end on an answer or a change of tier:
- Repeated attempt in one tier with no new evidence -> next tier
- Answers state source (file and literal, or command), build, and the code decision they change
- Facts found go to their owning memory file or mazan-group runbook with the command that reproduces them
