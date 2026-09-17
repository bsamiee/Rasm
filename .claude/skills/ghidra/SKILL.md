---
name: ghidra
description: "Use when reading or annotating a binary through Ghidra, covering ghidra-cli, catalog, decompile, call site, stub, and header scripts."
---

# [GHIDRA]

Binary research through one Ghidra project per binary under `$GHIDRA_PROJECT_DIR`, program named after the binary's file. ghidra-cli starts a bridge that keeps the program resident and answers each command in place. Traversals the commands lack are scripts, each writing one file the session reads by line.

[SCRIPTS]:
- [01]-[STUBS](scripts/Stubs.java): Every `__objc_stubs` function named after the message it sends
- [02]-[CATALOG](scripts/Catalog.java): Every string, import, and function with the functions referencing or calling each
- [03]-[DECOMPILE](scripts/Decompile.java): Seeds with callers and callees, the globals and types the bodies reference, in one indexed C file
- [04]-[CALLSITES](scripts/CallSites.java): Every call site of seed functions with the argument text the decompiler resolved there
- [05]-[HEADERS](scripts/Headers.java): C headers parsed into the program's types, prototypes applied to the named functions

[FACTS]:
- Project: Every command takes `--project <name>` until `set-default project <name>`, subcommands and flags come from `ghidra <group> --help`
- Program: Scripts run on the bridge's current program, `program open <name>` switches the program
- Program: Repeated imports into a project add `<file>.<n>`, the bridge then answers for the wrong program
- Lock: One process opens a project, headless runs beside the resident bridge abort with `LockException` until `ghidra stop`
- Lock: Crashes leave `<name>.lock` and `<name>.lock~` under `$GHIDRA_PROJECT_DIR` for removal by hand
- Writes: Every change through the bridge persists, script renames and types included, headless runs under `-readOnly` discard every change
- Analysis: `program list` prints `analyzed: true` for an import with no analysis run, `function_count` then counts size-1 import stubs
- Analysis: With no analysis run, `x-ref to` prints `[]` and `decompile` answers `No function at address`
- Keys: Targets are a name, `0x<hex>`, or `FUN_<hex>`, rows print hex with no `0x`
- Keys: Renames replace the `FUN_<hex>` name, the entry address stays the key
- Output: TTYs print compact rows and pipes print JSON
- Filter: `--filter 'name ~ <value>'` rejects `/` and `::` in the value, `--filter 'name=~"<regex>"'` accepts both
- Heap: `analyzeHeadless` reads `GHIDRA_HEADLESS_MAXMEM` at bridge start, 2G by default, set on the bridge's starting command
- Queue: Jobs run one at a time, later ones queue
- Time: Imports, analysis, exports, and script runs take `run_in_background`
- Time: Decompiles past the 300 s socket read take `GHIDRA_CLI_READ_TIMEOUT=0` on the command
- Bundle: `script run <path>` compiles the scripts directory as one bundle at the JDK `JAVA_HOME` names with no release flag
- Bundle: Subdirectories become packages, one file that fails to compile fails every load, unnamed variables need JDK 22 or later
- Bundle: `script java` and `script python` answer unsupported through the bridge
- Bundle: `Arguments.java`, `Functions.java`, and `Report.java` hold the seeds, settings, function facts, and report writer the scripts share
- Scripts: Output path is the first argument, `--expect <path>` on the command fails a job with that file missing or empty
- Use `search-code` for a Ghidra API signature from the install's jars

Each numbered fence line consumes the line before, unnumbered lines are alternatives with one case per comment.

## [01]-[READ]

Import once and prove analysis, name the stubs, catalog the program, then decompile seeds with their neighborhood into one file:

```bash
# [IMPORT] Analysis through the bridge started at this heap, run_in_background
GHIDRA_HEADLESS_MAXMEM=16G ghidra import <binary> --project <name>
# [UNIVERSAL] One architecture of a universal Mach-O as the file to import
lipo -thin arm64 <binary> -output <binary>.arm64
# [RAW] Headerless bytes without a bridge on the project, loader, base address without 0x, and language named
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -import <file> -loader BinaryLoader -loader-baseAddr <hex> -processor <languageID> -log <log>

# 1. Function count against the import stubs, a stub-only count takes `ghidra analyze --project <name>` in run_in_background
ghidra program list --project <name>
# 2. Stub names `objc_msgSend$<selector>` from the message each sends, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Stubs.java --project <name> --expect <out> -- <out>
# 3. Catalog of strings, imports, and functions, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Catalog.java --project <name> --expect <out> -- <out>
# 4. Seeds with neighborhood into one file under a directory the script creates, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 'str:<needle>' 0x<hex> callees=1 callers=1
# 5. Block dividers with line numbers, then Read <out> at the line of one function
rg -n '^// --- \[' <out>
# 6. Failed blocks, seeded again with timeout=<seconds> after a timeout and payload=<megabytes> after `Response buffer size exceeded`
rg -n '^// failed:' <out>

# [ALL] Whole program as the seed, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Decompile.java --project <name> --expect <out> -- <out> 're:.'
# [CALLS] Call sites of the seeds, run_in_background
ghidra script run .claude/skills/ghidra/scripts/CallSites.java --project <name> --expect <out> -- <out> <seed>...
```

Seeds come from the catalog:
- `[STRINGS]` rows name the functions holding the feature their text belongs to, `[IMPORTS]` rows name the functions calling each library
- `[FUNCTIONS]` rows separate a dispatcher from a leaf by size and callee count, `callers=0` marks a function no call reaches
- Library `<EXTERNAL>` holds symbols no dylib exports
- `objc_msgSend_<selector>` calls in a block are the message sends, `CallSites.java` lists them per selector

Seeds and settings of `Decompile.java` and `CallSites.java`:
- Seed: `0x<hex>` names the function containing the address, an address in no function takes `ghidra function create <address>` first
- Seed: `<name>` or `<namespace>::<name>` names functions by plain or qualified name, `re:<regex>` those with a matching plain or qualified name
- Seed: `str:<needle>` names functions referencing a defined string holding the needle case-insensitively, through a `__cfstring` struct included
- Seed: `tag:<tag>` names functions carrying the tag, `re:.` every non-external function
- Setting: `callers=<depth>` and `callees=<depth>` at 0, `timeout=<seconds>` at 30 and `payload=<megabytes>` at 50 by default
- Setting: `CallSites.java` takes `timeout` and `payload` alone
- Errors: Seeds matching no function and bad settings print together in one error before the usage text
- Neighbors: Thunks and stubs among callers stand for their own callers, among callees for the function each reaches

## [02]-[LOCATE]

Seeds outside the catalog from one live listing:

```bash
# [STRING] Defined strings holding a needle case-insensitively with addresses, then the referring function per row
ghidra find string '<needle>' --project <name>
ghidra strings refs 0x<address> --project <name>

# [NAME] Functions by regex, or every function outside the default prefix
ghidra function list --filter 'name=~"<regex>"' --fields name,address,size --project <name>
ghidra function list --filter 'NOT name ^ FUN_' --fields name,address,size --limit 0 --project <name>
```

## [03]-[ANNOTATE]

Marks that persist in the project and sharpen every later read:

```bash
# 1. Predefined macros of the target from clang, one file of #define lines
clang -dM -E -x c /dev/null -target <triple> > <macros>
# 2. Headers parsed under the macros, run_in_background
ghidra script run .claude/skills/ghidra/scripts/Headers.java --project <name> --expect <report> -- <report> <header>... -I<dir>... -imacros <macros>

# [BULK] Subcommands from a file in one connection, one per line without `ghidra` and `#` comments, split on whitespace with no quoting
ghidra batch <file> --project <name>
```

`Headers.java`:
- Defines: `-D<name>[=<value>]` on the command wins over a macro in the `-imacros` file
- Defines: Shims `__has_feature(x) 0` and its siblings, `__builtin_va_list`, and `restrict` are forms the preprocessor takes from no file
- Defines: Shim `__has_include(x) 1` overrides the SDK's fallback 0, under 0 MacTypes.h skips ConditionalMacros.h
- Defines: Shim `__CF_ENUM_FIXED_IS_AVAILABLE 0`, CParser reads no fixed enum
- Include: Every `-I<dir>` must be a directory
- Include: CFBase.h parses with the framework headers, `usr/include`, and the clang resource `include` as `-I` values
- Include: Preprocessor cannot lex `::`, a copy of the first `sys/cdefs.h` on the include path precedes the original
- Include: Copied `sys/cdefs.h` has its `__has_cpp_attribute(clang::unsafe_buffer_usage)` line neutralized
- Report: `[HEADERS]` rows parsed or failed per header, `[PREPROCESSOR]` and `[PARSER]` messages, then `[APPLIED]` rows per function
- Prototypes: Definitions apply to functions named with and without a leading underscore and follow a thunk to the thunked function

## [04]-[FORMAT]

Every file opens with `// --- [INDEX]` over `// <program> <language> <counts>`, sections with `// --- [NAME]` padded to column 90:
- Index: `args=<request>` closes the counts of a seeded script
- Catalog: `[STRINGS]` rows `<address> <text> functions=<count> <name>...`, `[IMPORTS]` rows `<library> <prototype> callers=<count> <name>...`
- Catalog: `[FUNCTIONS]` rows `<address> <name> size=<bytes> callers=<count> callees=<count>`
- Catalog: Rows rank by their count, names sort by entry address
- Catalog: Callers of a thunk or stub count for the function it reaches, thunks and stubs hold no `[FUNCTIONS]` row
- Decompile: Blocks `// --- [<name>]` over `// <address> size=<bytes> <role> callers=<count>[: <names>]`
- Decompile: `<role>` is `seed`, `caller:<depth>`, or `callee:<depth>`, `<names>` the callers with a block in the file by entry address
- Decompile: Failed blocks hold `// failed: <cause>` under their header, thunk and stub seeds hold `// <address> thunk -> <name>` or `stub -> <name>`
- Decompile: Decompiler prints `$` and `:` of a stub name as `_`, `objc_msgSend$length` reads `objc_msgSend_length` in a block
- Decompile: `[GLOBALS]` holds one row `// <address> <type> <name>[ = <value>]` per global the blocks reference
- Decompile: `[TYPES]` holds Ghidra's built-in typedefs, then every composite, enum, and typedef the blocks name
- CallSites: `// --- [<target>]` over `// <name> @ <address> callers=<count> calls=<count>`
- CallSites: Rows `<address> <caller>: [$<selector> ]<arguments>` sorted by caller entry then address
- CallSites: Stub call arguments open with the receiver, `$?` marks a stub with no selector string resolved
- CallSites: `[FAILED]` lists each caller the decompiler failed on with the cause
- Stubs: `[STUBS]` rows `<address> <old> -> <new>` per rename, `unchanged`, `unresolved`, or `rejected: <message>` after the name otherwise

## [05]-[API]

Behaviors of the Ghidra API that decide how a script reads or writes a program, verified at the installed release:
- Stubs: `__objc_stubs` functions are plain functions analysis leaves as `FUN_<hex>`, a call through one names the stub
- Stubs: Selector is the string the stub's load references
- Types: `DataTypeWriter` writes the built-in typedefs from the constructor and skips a `FunctionDefinition` on write
- Strings: `DefinedDataIterator.byDataInstance` with `StringDataInstance::isString` walks defined strings, a `__cfstring` struct references the text
- Functions: `getFunctions(true)` skips externals, `getCallingFunctions` keeps call references to the entry alone
- Thunks: `getFunctionThunkAddresses` answers null with no thunks, `getThunkedFunction(true)` follows a chain to the end
- Decompile: `ParallelDecompiler.decompileFunctions` returns results in completion order, null for a cancelled item, and rethrows a callback exception
- Decompile: `DecompilerCallback.setTimeout` sets the timeout passed per function, `DecompileOptions.setDefaultTimeout` changes nothing under it
- Globals: `getGlobalSymbolMap` holds a fraction of the globals a body names, the C markup tokens hold every one
- Preprocessor: `-D` takes no macro arguments, a prelude through `ReInit` and `Input()` defines a function-like macro
- Preprocessor: `Define()` ignores a redefinition
- Preprocessor: `__has_include` is no built-in

## [06]-[EXTEND]

Each new traversal is one `GhidraScript` file in the bundle, seeds through `Arguments.parse`, lines through `Report.write`, proven before a run:

```bash
# 1. Bundle compiled against the install's jars with every warning on
javac -d <dir> -Xlint:all,-path -cp "$(find "$GHIDRA_INSTALL_DIR/Ghidra" -path '*/lib/*.jar' | tr '\n' ':')" .claude/skills/ghidra/scripts/*.java
# 2. Format and lint over the tree
nx run rasm:lint
# 3. Run on a resident program with the file proven, run_in_background
ghidra script run .claude/skills/ghidra/scripts/<Script>.java --project <name> --expect <out> -- <out> <seed>...
```

- Bundle: Edited bundles recompile on the next `script run` without a bridge restart
