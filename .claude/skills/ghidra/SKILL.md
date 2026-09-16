---
name: ghidra
description: "Use when reading or annotating a binary through Ghidra, covering ghidra-cli, catalog, decompile, call site, and header scripts."
---

# [GHIDRA]

Binary research through one Ghidra project per binary under `$GHIDRA_PROJECT_DIR`, the program named after the binary's file:
- Bridge: `ghidra` (ghidra-cli) starts a bridge that keeps the program resident and answers each command in place
- Headless: `analyzeHeadless` opens the project in a process of one run
- Scripts: run through the bridge on the bridge's current program and write one file the session reads by line

[SCRIPTS]:
- [01]-[CATALOG](scripts/Catalog.java): Every string, import, and function with the functions referencing or calling each
- [02]-[DECOMPILE](scripts/Decompile.java): Seeds with callers and callees, the globals and types the bodies reference, in one indexed C file
- [03]-[CALLSITES](scripts/CallSites.java): Every call site of seed functions with the argument text the decompiler resolved there
- [04]-[HEADERS](scripts/Headers.java): C headers parsed into the program's types, prototypes applied to the named functions

- Project: every command takes `--project <name>` until `set-default project <name>`, subcommands and flags come from `ghidra <group> --help`
- Program: `script run` sends no program key, a script runs on the bridge's current program and `program open <name>` switches that program
- Program: second import into a project adds `<file>.<n>` and the bridge answers for the wrong program
- Lock: one process opens a project, the bridge holds the lock while resident and a headless run beside the bridge aborts with `LockException`
- Lock: `ghidra stop` releases the lock, a crash leaves `<name>.lock` and `<name>.lock~` under `$GHIDRA_PROJECT_DIR` for removal by hand
- Writes: every change through the bridge persists, `Headers.java` types included, a headless run under `-readOnly` discards every change
- Analysis: `program list` prints `analyzed: true` for an import with no analysis run, `function_count` then counts import stubs
- Analysis: stub-only programs list size-1 functions, `x-ref to` prints `[]` and `decompile` answers `No function at address`
- Keys: targets are a name, `0x<hex>`, or `FUN_<hex>` and rows print bare hex
- Keys: `FUN_<hex>` names change with a rename and the entry address stays
- Output: TTY prints compact rows and a pipe prints JSON
- Output: `--count` before a listing prints the row count, `--limit 0` lifts the 1000-row default
- Output: `graph export` prints to stdout, a file takes the redirect
- Filter: `--filter 'name ~ <value>'` rejects `/` and `::` in the value, `--filter 'name=~"<regex>"'` accepts both
- Heap: bridge takes `GHIDRA_HEADLESS_MAXMEM` at start, `2G` by default, and an import starts the bridge at `16G`
- Queue: bridge runs one job at a time and queues the rest, `ghidra jobs` lists the queue and `ghidra cancel` stops the active job
- Time: imports, exports, and whole-program script runs take `run_in_background`
- Time: decompiles past the 300 s socket read take `GHIDRA_CLI_READ_TIMEOUT=0` on the command
- Scripts: `script run <path>` compiles the scripts directory as one bundle at the bridge JDK, one file that fails to compile fails every load
- Scripts: subdirectories under the scripts directory become packages
- Scripts: `Job.java` holds the seeds, settings, and report markers the scripts share
- Scripts: traversals the commands lack are one `GhidraScript` class per file writing to the path in the first argument
- Scripts: `script list` prints the install's scripts, `script java` and `script python` answer unsupported in bridge mode
- Exit: script failures exit 1 with `Script threw: <message>`
- Exit: missing, empty, or short `--expect` outputs fail the job
- Use `search-code` for a Ghidra API signature from the install's jars

Numbered fence lines chain and each consumes the line before, unnumbered lines are alternatives with one case per comment.

## [01]-[READ]

Import once and prove analysis, then catalog the program and decompile seeds with neighborhood into one file read by block header:

```bash
# [IMPORT] Analysis through the bridge, run_in_background, the bridge stays resident at this heap
GHIDRA_HEADLESS_MAXMEM=16G ghidra import <binary> --project <name>
# [UNIVERSAL] One architecture of a universal Mach-O as the file to import
lipo -thin arm64 <binary> -output <binary>.arm64
# [RAW] Headerless bytes without a bridge on the project, the loader and base address without 0x and language named
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -import <file> -loader BinaryLoader -loader-baseAddr <hex> -processor <languageID> -log <log>

# 1. Function count against the import stubs, a stub-only count takes `ghidra analyze --project <name>` in run_in_background
ghidra program list --project <name>
# 2. Catalog of strings, imports, and functions ranked by reference or call count in run_in_background
ghidra script run .claude/skills/ghidra/scripts/Catalog.java --project <name> --expect <out> -- <out>
# 3. Seeds with neighborhood into one file the script creates the directory of, --expect proves the run, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 'str:<needle>' 0x<hex> callees=1 callers=1
# 4. Section and function dividers with line numbers, then Read <out> at the line of one function
rg -n '^// --- \[' <out>

# [ALL] Every non-external function as a seed, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 're:.'
# [FAILED] Seeds of the `// failed:` blocks again, timeout for a block that timed out, payload for `Response buffer size exceeded`
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 0x<hex> 0x<hex> timeout=300 payload=200
# [CALLS] Every call site of the seeds with the argument text resolved there, run_in_background
ghidra script run .claude/skills/ghidra/scripts/CallSites.java --project <name> --expect <out> -- <out> _objc_msgSend timeout=180 payload=200
```

Seeds and settings of `Decompile.java` and `CallSites.java`:
- Seed: `0x<hex>` names the function containing the address, an address in no function takes `ghidra function create <address>` first
- Seed: `<name>` or `<namespace>::<name>` names functions by plain or qualified name, `re:<regex>` those with a matching plain or qualified name
- Seed: `str:<needle>` names functions referencing a defined string holding the needle case-insensitively, through a `__cfstring` struct included
- Seed: `tag:<tag>` names functions carrying the tag, `re:.` every non-external function
- Setting: `callers=<depth>` and `callees=<depth>` at 0, `timeout=<seconds>` at 30 and `payload=<megabytes>` at 50 by default
- Setting: `CallSites.java` takes `timeout` and `payload` alone
- Errors: seeds matching no function and bad settings print together in one error before the usage text
- Neighbors: callers pass through thunks to the calling function and callees resolve a thunk to the thunked function
- Neighbors: thunk seeds print one stub line naming the thunked function
- Index: every file opens with `// --- [INDEX]` over `// <program> <language> <counts> args=<request>`, `Catalog.java` without `args=`
- Index: sections open with `// --- [NAME]` padded to column 90, items with `// --- [<item>]` over one fact line
- Blocks: `// --- [<name>]` over `// <address> size=<bytes> <role> callers=<count>: <names>`, the names sorted by entry address
- `[GLOBALS]` holds every global the blocks reference with a defined value, one row `<address> <type> <name> = <value>`
- `[TYPES]` holds Ghidra's built-in typedefs and then every composite, enum, and typedef the blocks name
- Calls: `// --- [<target>]` over `// <name> @ <address> callers=<count> calls=<count>`, rows `<address> <caller>: [$<selector> ]<arguments>`
- Calls: `$?` marks an objc stub with no selector string resolved, `[FAILED]` lists each caller the decompiler failed on with the cause

## [02]-[LOCATE]

Seeds of a read from the catalog or one live listing, a count before every listing:

```bash
# [COUNT] Rows a listing would print
ghidra function list --count --project <name>

# [STRING] Defined strings holding a needle case-insensitively with addresses, then the referring function per row
ghidra find string '<needle>' --project <name>
ghidra strings refs 0x<address> --project <name>

# [IMPORT] Imported symbols with library
ghidra dump imports --filter 'name ~ <needle>' --fields name,library --project <name>

# [NAME] Functions by glob, by regex, or every function outside the default prefix
ghidra find function '<glob>' --project <name>
ghidra function list --filter 'name=~"<regex>"' --fields name,address,size --project <name>
ghidra function list --filter 'NOT name ^ FUN_' --fields name,address,size --limit 0 --project <name>

# [BYTES] Addresses of a byte pattern
ghidra find bytes '<hex bytes>' --project <name>

# [SIZE] Functions over a size, largest first
ghidra function list --filter 'size > <bytes>' --fields name,address,size --sort -size --limit 0 --project <name>
# [INTERESTING] Large or often-referenced functions and names holding a credential word
ghidra find interesting --project <name>

# [TAG] Tags with meaning and use count, then the functions one tag marks
ghidra tag list --project <name>
ghidra tag get <tag> --project <name>
```

## [03]-[INSPECT]

One function or one address live, for what the file lacks:

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

# [STATE] Program info, memory map with block permissions, and bridge status
ghidra program info --project <name>
ghidra memory map --project <name>
ghidra status --project <name>
```

## [04]-[ANNOTATE]

Marks that persist in the project and sharpen every later read, the address kept beside each renamed symbol:

```bash
# [TAG] Tag with a meaning attached per function and listed by tag, then seeded through tag:<tag>
ghidra tag create <tag> --comment '<meaning>' --project <name>
ghidra tag add <target> <tag> --project <name>
ghidra function list --tag <tag> --fields name,address,tags --project <name>

# [BULK] Subcommands from a file in one connection, one per line without `ghidra` and `#` comments, split on whitespace with no quoting
ghidra batch <file> --project <name>

# [RENAME] Symbol rename, later FUN_ references break, the address stays
ghidra symbol rename FUN_<hex> <symbol> --project <name>

# [TYPE] Variable type and struct, enum, and typedef definitions the next decompile shows
ghidra function set-var-type <target> --var <local> --type '<type>' --project <name>
ghidra type create <Struct> --project <name>
ghidra type add-field <Struct> --name <field> --type <type> --project <name>
ghidra type create-enum <Enum> --values '<A>=0,<B>=1' --project <name>
ghidra type typedef <Alias> '<type>' --project <name>

# [HEADERS] SDK headers parsed into the program's types with prototypes applied by name, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Headers.java --project <name> --expect <report> -- <report> <header>... -I<dir>...

# [COMMENT] Comment at an address as EOL by default or PRE, POST, or PLATE
ghidra comment set 0x<address> '<note>' --comment-type PLATE --project <name>

# [ARCHIVE] Program with every mark as XML, run_in_background
ghidra program export xml -o <file> --project <name>

# [DELETE] Project with every program and the bridge
ghidra project delete <name>
```

Headers:
- Defines: Apple platform, target, and clang shim macros come from the program's format and language
- Defines: `-D<name>[=<value>]` on the command wins over a derived macro
- Defines: `__CF_ENUM_FIXED_IS_AVAILABLE 0` joins the Apple set, the preprocessor evaluates the macro true and CParser reads no fixed enum
- Include: every `-I<dir>` is checked as a directory
- Include: CoreFoundation parses with the framework headers, `usr/include`, and the clang resource `include` as `-I` values
- Include: `sys/cdefs.h` tests `__has_cpp_attribute(clang::unsafe_buffer_usage)` and the preprocessor cannot lex `::`
- Include: shadow copies of `sys/cdefs.h` with `#if 0` on the `unsafe_buffer_usage` line under an earlier `-I` parse
- Report: `[HEADERS]` rows parsed or failed per header, `[PREPROCESSOR]` and `[PARSER]` messages, then `[APPLIED]` rows per function
- Prototypes: definitions apply to functions named with and without a leading underscore and follow a thunk to the thunked function

## [05]-[API]

Behaviors of the Ghidra API that decide how a script reads or writes a program, verified at the installed release:
- Stubs: `__objc_stubs` functions are plain functions and a call through one names the stub, `CallSites.java` reads the selector the stub loads
- Types: `DataTypeWriter` writes the built-in typedefs from the constructor and skips a `FunctionDefinition` on write
- Strings: `DefinedDataIterator.byDataInstance` with `StringDataInstance::isString` walks defined strings, a `__cfstring` struct references the text
- Functions: `getFunctions(true)` skips externals, `getCallingFunctions` keeps call references to the entry alone
- Thunks: `getFunctionThunkAddresses` answers null with no thunks, `getThunkedFunction(true)` follows a chain to the end
- Decompile: `__got` on this loader's Mach-O sits in a `rw` block and the decompiler never folds a got load to the symbol
- Decompile: `setMaxWidth` at the printer's bound keeps an argument list on one line, a wrap replaces a space with a break token
- Decompile: `ParallelDecompiler.decompileFunctions` returns results in completion order, a join by function restores the request order
- Decompile: `ParallelDecompiler` answers null for a cancelled item and rethrows a callback exception, `checkCancelled` then makes a join total
- Preprocessor: `-D` takes no macro arguments and `PPToken` is package-private, a prelude through `ReInit` and `Input()` defines `__has_feature(x)`
- Preprocessor: `Define()` ignores a redefinition
- Parsers: `PreProcessor.parse` appends a parse error to the messages, `CParser.parse` reports one in the exception alone
