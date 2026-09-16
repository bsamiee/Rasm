---
name: ghidra
description: "Use when a task reads or annotates a binary through Ghidra, decompiling or decomposing a binary, covering ghidra-cli, headless runs, and the decompile script."
---

# [GHIDRA]

One project per binary under `$GHIDRA_PROJECT_DIR`, the program named after its file. `ghidra` (ghidra-cli) starts a bridge that keeps the program resident and answers each command in place, `analyzeHeadless` opens the project in its own process for one run, `Decompile.java` runs through the bridge.

[SCRIPTS]:
- [01]-[DECOMPILE](scripts/Decompile.java): Seeds with their callers and callees into one indexed C file, through `ghidra script run`

- Lock: one process opens a project, the bridge holds it while resident, a headless run beside it aborts with `LockException: Unable to lock project!`
- Lock: `ghidra stop` releases the lock, a crash leaves `<name>.lock` and `<name>.lock~` in the store
- Writes: every change through the bridge persists, a script's `createFunction` included, a headless run under `-readOnly` discards its changes
- Program: a second import into a project adds `<file>.<n>` and the bridge answers for the wrong program, a second binary gets its own project
- Analysis: `program list` prints `analyzed: true` for an import whose analysis never ran, its `function_count` is the count of import stubs
- Analysis: a stub-only program lists size-1 functions, `x-ref to` prints `[]`, and `decompile` answers `No function at address`
- Keys: a target is a name, `0x<hex>`, or `FUN_<hex>`, rows print bare hex, `FUN_<hex>` names change with a rename and the entry address stays
- Output: a TTY prints compact rows, a pipe prints JSON, `--count` before a listing, `--limit 0` lifts the 1000-row default
- Output: `graph export` prints to stdout, a file takes the redirect
- Filter: `--filter 'name=~"<regex>"'` is the regex form, `name ~ <value>` holding `::` is rejected as an invalid filter expression
- Heap: the bridge takes `GHIDRA_HEADLESS_MAXMEM` at start, `2G` by default, an import starts it at `16G`
- Queue: the bridge runs one job at a time and queues the rest, runs enqueue under `run_in_background`, each completion notification reads in turn
- Time: an import, an export, and a whole-program script run take `run_in_background`
- Time: a decompile past the 300 s socket read takes `GHIDRA_CLI_READ_TIMEOUT=0` on the command
- Exit: a script failure through the bridge exits 1 with `Script threw: <message>`, a missing, empty, or short `--expect` output fails the job
- Scripts: `script run <path>` runs a Java file by path, Ghidra compiles it on the first run and a compile error is that run's error
- Scripts: `script java` and `script python` answer unsupported in bridge mode

Numbered lines chain, each consuming the line before, unnumbered lines are alternatives, one per case its comment names.

## [01]-[READ]

Import once, prove analysis, decompile the seeds with their neighborhood into one file, then read that file by its index:

```bash
# [IMPORT] Analysis through the bridge, run_in_background, the bridge stays resident at this heap
GHIDRA_HEADLESS_MAXMEM=16G ghidra import <binary> --project <name>
# [UNIVERSAL] One architecture of a universal Mach-O as the file to import
lipo -thin arm64 <binary> -output <binary>.arm64
# [RAW] Bytes with no header, no bridge on the project, loader, base address without 0x, and language named
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -import <file> -loader BinaryLoader -loader-baseAddr <hex> -processor <languageID> -log <log>

# 1. Function count against the import stubs, a stub-only count takes `ghidra analyze --project <name>` in run_in_background
ghidra program list --project <name>
# 2. Seeds with their neighborhood into one file the script creates the directory of, --expect proves the run, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 'str:<needle>' 0x<hex> callees=1 callers=1
# 3. Index rows and block headers with their line, then Read <out> at the line= of one function, the types section closes the file
rg -n '^// ==== ' <out>

# [ALL] Every function, roles print as all with no caller names, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> all
# [FAILED] Seeds of the rows marked failed again under a longer timeout, 60 s by default
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 0x<hex> 0x<hex> timeout=300
```

Seeds are `all`, `0x<hex>`, `<name>`, `<namespace>::<name>`, `re:<regex>`, `str:<needle>`, or `tag:<tag>`:
- Settings are `callers=<depth>`, `callees=<depth>`, and `timeout=<seconds>`, at 0, 0, and 60 by default
- Every seed that matches no function and every bad setting prints in one error before the usage text, one rerun fixes all of them
- `0x<hex>` inside no function disassembles there and creates one
- `str:<needle>` matches defined strings case-insensitively and takes the functions referencing them
- Callers pass through thunks to the calling function, callees resolve a thunk to its target, thunks and externals never decompile
- Seeded thunks print a `// ==== THUNK <name> @ <entry> -> <target>` line in place of a block
- File opens with `// ==== INDEX <program> <language> functions= failed= types= args=`, one row per function follows
- Rows read `// <entry> <name> size=<n> <role> line=<n>`, ending in `failed` for a timeout and `thunk` for a stub line
- Roles are `seed`, `all`, `callee:<depth>`, and `caller:<depth>`
- Blocks open with `// ==== FUNC <name> @ <entry> size=<n> <role> callers=<n>`, caller names follow the count on explicit seeds alone
- `// ==== TYPES <n>` and the struct, enum, typedef, and function definitions the decompiled code names close the file

## [02]-[LOCATE]

From a string, an import, a name, a byte pattern, or a size profile to the seeds of a read, a count before every listing:

```bash
# [COUNT] Rows a listing would print
ghidra function list --count --project <name>
# [STRING] Defined strings holding a needle, case-insensitive, with their addresses, then the referring function per row
ghidra find string '<needle>' --project <name>
ghidra strings refs 0x<address> --project <name>
# [IMPORT] Imported symbols with their library
ghidra dump imports --filter 'name ~ <needle>' --fields name,library --project <name>
# [NAME] Functions by glob, by regex, or every function outside the default prefix
ghidra find function '<glob>' --project <name>
ghidra function list --filter 'name=~"<regex>"' --fields name,address,size --project <name>
ghidra function list --filter 'NOT name ^ FUN_' --fields name,address,size --limit 0 --project <name>
# [BYTES] Addresses of a byte pattern
ghidra find bytes '<hex bytes>' --project <name>
# [PROFILE] Functions over a size, then large functions, many cross-references, and names holding password, key, crypt, auth, admin, or secret
ghidra function list --filter 'size > <bytes>' --fields name,address,size --sort -size --limit 0 --project <name>
ghidra find interesting --project <name>
# [TAG] Tags with their meaning and use count, then the functions one marks
ghidra tag list --project <name>
ghidra tag get <tag> --project <name>
```

## [03]-[INSPECT]

One function or one address live, without a file:

```bash
# [ONE] Decompiled C of one function, storage of parameters and locals beside it
ghidra decompile <target> --with-vars --with-params --project <name>
# [ASM] Instructions from an address, or a whole function
ghidra disasm 0x<address> -n <count> --project <name>
ghidra function disasm <target> --project <name>
# [BYTES] Bytes at an address with pointer candidates
ghidra memory read 0x<address> <size> --project <name>
# [PAIR] Decompiled lines two functions differ in
ghidra diff functions <target> <target> --project <name>
# [XREF] References to or from an address with the holding function and reference type per row, both directions over a function body as a count
ghidra x-ref to 0x<address> --project <name>
ghidra x-ref from 0x<address> --project <name>
ghidra x-ref list <target> --count --project <name>
# [GRAPH] Callers or callees to a depth with the call site per row, the whole call graph as DOT or JSON into a file
ghidra graph callers <target> --depth <n> --project <name>
ghidra graph callees <target> --depth <n> --project <name>
ghidra graph export dot --project <name> > <file>
# [STATE] Language, image base, function count, blocks with permissions, and the bridge
ghidra program info --project <name>
ghidra memory map --project <name>
ghidra status --project <name>
```

## [04]-[ANNOTATE]

Marks that persist in the project and sharpen every later read, the address kept beside each renamed symbol:

```bash
# [TAG] Tag with its meaning, attached per function, listed by tag, seeded through tag:<tag>
ghidra tag create <tag> --comment '<meaning>' --project <name>
ghidra tag add <target> <tag> --project <name>
ghidra function list --tag <tag> --fields name,address,tags --project <name>
# [BULK] Subcommands from a file in one connection, one per line without `ghidra`, `#` comments, split on whitespace with no quoting
ghidra batch <file> --project <name>
# [RENAME] Symbol rename, later FUN_ references break, the address stays
ghidra symbol rename FUN_<hex> <symbol> --project <name>
# [TYPE] Variable type, struct with fields, enum, and typedef, the next decompile shows them
ghidra function set-var-type <target> --var <local> --type '<type>' --project <name>
ghidra type create <Struct> --project <name>
ghidra type add-field <Struct> --name <field> --type <type> --project <name>
ghidra type create-enum <Enum> --values '<A>=0,<B>=1' --project <name>
ghidra type typedef <Alias> '<type>' --project <name>
# [COMMENT] Comment at an address, EOL default, PRE, POST, or PLATE
ghidra comment set 0x<address> '<note>' --comment-type PLATE --project <name>
# [ARCHIVE] Program with every mark as XML, run_in_background
ghidra program export xml -o <file> --project <name>
```

## [05]-[SCRIPT]

Traversals the commands and `Decompile.java` lack are Java `GhidraScript` classes, one class per file, output to the path in the first argument:

```bash
# [RUN] Script through the resident bridge, arguments after --, --expect fails the job on a missing or empty output
ghidra script run <path>.java --project <name> --expect <out> -- <out> <args>
# [BUNDLED] Script the install holds, by path from `ghidra script list`
ghidra script run "$GHIDRA_INSTALL_DIR/Ghidra/Features/<Feature>/ghidra_scripts/<Script>.java" --project <name>
```

- Use `search-code` for a Ghidra API signature from the install's jars

## [06]-[RESET]

Job, bridge, lock, and project, each removed at its owner:

```bash
# [JOB] Queue with the active job, then cancel it, no active job exits 1
ghidra jobs --project <name>
ghidra cancel --project <name>
# [STOP] Bridge off, the lock goes with it
ghidra stop --project <name>
# [LOCK] Stale lock after a crash, with no bridge and no headless process on the project
rm -f "$GHIDRA_PROJECT_DIR/<name>.lock" "$GHIDRA_PROJECT_DIR/<name>.lock~"
# [LOG] Bridge log, port, and PID files
ls ~/Library/Application\ Support/ghidra-cli
# [DELETE] Project with its programs, the bridge stopped with it
ghidra project delete <name>
```

## [07]-[REFERENCE]

Subcommands come from this table or `ghidra <group> --help`, a guessed name exits 2 with `unrecognized subcommand`:

| [INDEX] | [GROUP]    | [SUBCOMMANDS]                                                                                                     |
| :-----: | :--------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | Bridge     | `start`, `stop`, `restart`, `status`, `ping`, `jobs [<id>]`, `cancel [<id>]`                                      |
|  [02]   | Install    | `doctor`, `version`, `init`, `setup`, `config` with `list`, `get`, `set`, `reset`, `set-default <kind> <value>`   |
|  [03]   | Project    | `project` - `create`, `list`, `info`, `delete`                                                                    |
|  [04]   | Import     | `import <file>`, `analyze`, `summary`, `stats`                                                                    |
|  [05]   | Program    | `program` - `list`, `open`, `close`, `delete`, `info`, `export <format>`                                          |
|  [06]   | Function   | `function` - `list`, `get`, `decompile`, `disasm`, `calls`, `x-refs`, `rename`, `create`, `delete`                |
|  [07]   | Prototype  | `function` - `set-signature`, `set-return-type`, `set-calling-convention`, `set-var-type`                         |
|  [08]   | Code       | `decompile <target>`, `disasm <target>`, `rename <old> <new>`                                                     |
|  [09]   | Query      | `query` - `functions`, `strings`, `imports`, `exports`, `memory`                                                  |
|  [10]   | Strings    | `strings` - `list`, `refs <address>`                                                                              |
|  [11]   | Symbols    | `symbol` - `list`, `get`, `create`, `delete`, `rename`                                                            |
|  [12]   | Types      | `type` - `list`, `get`, `create`, `apply`, `delete`, `rename`, `create-enum`, `typedef`, `add-field`, `del-field` |
|  [13]   | Memory     | `memory` - `map`, `read <address> <size>`, `write`, `search <pattern>`                                            |
|  [14]   | References | `x-ref` - `to`, `from`, `list`                                                                                    |
|  [15]   | Search     | `find` - `string`, `bytes`, `function`, `calls`, `crypto`, `interesting`                                          |
|  [16]   | Graph      | `graph` - `calls`, `callers`, `callees`, `export <format>`                                                        |
|  [17]   | Tags       | `tag` - `list`, `get`, `create`, `delete`, `rename`, `set-comment`, `add`, `remove`                               |
|  [18]   | Comments   | `comment` - `list`, `get`, `set`, `delete`                                                                        |
|  [19]   | Compare    | `diff` - `functions <a> <b>`, `programs <a> <b>`                                                                  |
|  [20]   | Dump       | `dump` - `imports`, `exports`, `functions`, `strings`                                                             |
|  [21]   | Patch      | `patch` - `bytes <address> <hex>`, `nop <address>`, `export -o <file>`                                            |
|  [22]   | Scripts    | `script` - `run <path>`, `list`, `java`, `python`, `batch <file>`                                                 |

Flags per question, `--project <name>` and `--program <name>` sit before or after any subcommand:

| [INDEX] | [FLAG]                                      | [COMMANDS]                                                                   |
| :-----: | :------------------------------------------ | :--------------------------------------------------------------------------- |
|  [01]   | `--filter '<field><op><value>'`             | Every list, `= != > >= < <=`, `~` contains, `^` starts, `$` ends, `=~` regex |
|  [02]   | `AND`, `OR`, `NOT`                          | Inside `--filter`, a bare word is rejected                                   |
|  [03]   | `--fields <a,b>`, `--sort [-]<field>`       | Every list, columns kept and order, `-` descending                           |
|  [04]   | `--limit <n>`, `--offset <n>`, `--count`    | Every list, 1000 default, 0 unbounded, count alone                           |
|  [05]   | `--json`, `--pretty`, `-o <format>`         | Every command, compact JSON, indented JSON, `csv`, `tsv`, `table`, `ndjson`  |
|  [06]   | `--tag <name>`, `--untagged`                | `function list`, repeated `--tag` is AND                                     |
|  [07]   | `--with-vars`, `--with-params`              | `decompile`, storage of locals and parameters                                |
|  [08]   | `-n <count>`, `--depth <n>`                 | `disasm` instruction count, `graph callers` and `graph callees` depth        |
|  [09]   | `--comment-type <EOL\|PRE\|POST\|PLATE>`    | `comment set`, `EOL` default                                                 |
|  [10]   | `--comment '<text>'`, `--function <target>` | `tag create` meaning, `tag list` tags of one function                        |
|  [11]   | `--no-create`, `--all`                      | `tag add` errors on a missing tag, `tag remove` detaches every tag           |
|  [12]   | `--name`, `--type`, `--offset`, `--size`    | `type add-field`, `type del-field` takes `--name`                            |
|  [13]   | `--values '<A>=0,<B>=1'`, `--size <n>`      | `type create-enum`, size 1, 2, 4, or 8                                       |
|  [14]   | `--signature`, `--type`, `--convention`     | `function set-signature`, `set-return-type`, `set-calling-convention`        |
|  [15]   | `--var <local> --type <type>`               | `function set-var-type`                                                      |
|  [16]   | `--expect <path>[:<rows>]`, `--allow-empty` | `script run`, job fails on missing, empty, or short output                   |
|  [17]   | `--no-analyze`, `--count <n>`               | `import` without analysis, `patch nop` instruction count                     |
|  [18]   | `-o <file>`                                 | `program export` and `patch export` output file                              |
