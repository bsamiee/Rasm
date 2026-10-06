---
name: use-ghidra
description: "Use when reading or annotating a binary through Ghidra, covering headless runs, catalog, decompile, call site, stub, and header scripts, and ghidra-cli edits."
---

# [GHIDRA]

`analyzeHeadless` imports and analyzes each binary once into its own Ghidra project under `$GHIDRA_PROJECT_DIR`, program named after its file. Later reads run scripts on the saved program, one file per script. ghidra-cli bridges serve edits of names, comments, and functions alone.

[SCRIPTS]:
- [01]-[STUBS](scripts/Stubs.java): Every `__objc_stubs` function named after the message it sends and typed as a message send
- [02]-[CATALOG](scripts/Catalog.java): Every string, import, and function with the functions referencing or calling each
- [03]-[DECOMPILE](scripts/Decompile.java): Seeds with callers and callees, the globals and types the bodies reference, in one indexed C file
- [04]-[CALLSITES](scripts/CallSites.java): Every call site of seed functions with the argument text the decompiler resolved there
- [05]-[HEADERS](scripts/Headers.java): C headers parsed into the program's types, prototypes applied to the named functions
- [06]-[ARGUMENTS](scripts/Arguments.java): Output path, seed, and setting parser that collects every argument error
- [07]-[FUNCTIONS](scripts/Functions.java): Call graph walks through thunks and stubs, stub selectors, string referrers, and parallel decompiles
- [08]-[REPORT](scripts/Report.java): `Result` type with `Success` and `Failure` cases, and the writer of every script's file
- [09]-[BRIDGES](scripts/bridges.py): Session hook that stops each bridge of an ended Claude Code session and clears files dead bridges left

[FACTS]:
- Run: Scripts take the output path as their first argument after the script name
- Run: Runs exit 0 after a script error, `REPORT SCRIPT ERROR` in output and a missing `<out>` mark failure
- Help: `$GHIDRA_INSTALL_DIR/support/analyzeHeadlessREADME.html` lists every headless flag, `ghidra <group> --help` every ghidra-cli flag
- Project: `mise.toml` names `$GHIDRA_PROJECT_DIR` outside the repository, Ghidra rejects a project path with a dot-prefixed component
- Project: `-deleteProject` on an `-import` deletes the project that run created with its analysis
- Program: Repeated imports into a project add `<file>.<n>`, `-process <file>` keeps reading the first
- Lock: One process opens a project, a second run aborts with `LockException` until the first exits, `-readOnly` included
- Lock: Bridges hold their project until `ghidra stop --project <name>`, runs on another project start in parallel
- Writes: Runs save every script change at exit, `-readOnly` discards them, bridges save at `ghidra stop`
- Keys: Targets are a name, `0x<hex>`, or `FUN_<hex>`, rows print hex with no `0x`
- Keys: Renames replace the `FUN_<hex>` name, the entry address stays the key
- Memory: Every run and bridge caps its heap at `GHIDRA_HEADLESS_MAXMEM`, the quarter of RAM `mise.toml` sets, and holds it until exit
- Memory: Decompiles spawn up to min(cores + 1, 10) native `decompile` processes outside the heap, `-max-cpu <n>` caps one run at n + 1
- Time: Imports, analysis, and script runs take `run_in_background`
- Bundle: `-scriptPath` compiles the scripts directory as one bundle at the JDK `JAVA_HOME` names with no release flag
- Bundle: Subdirectories become packages, unnamed variables need JDK 22 or later
- Bundle: Compile failures drop the failing file and every file referencing it
- Parallel: Runs and bridges share the settings, cache, and temp folders `mise.toml` names
- Parallel: Ghidra serializes bundle builds and loads across processes under `osgi/parallel.lock`
- Files: `<out>`, `<stubs>`, `<catalog>`, `<report>`, `<macros>`, `<log>`, and `<scriptlog>` go under `<main>/.artifacts/ghidra/<name>/`
- Files: `<main>` is the main worktree's absolute path, `<skill>` the absolute directory containing this `SKILL.md`
- Files: Scripts and `-log` create the folder, `lipo` and redirects need `mkdir -p`
- Logs: `<log>` holds one run's messages and its scripts' printed lines, `<scriptlog>` the printed lines alone
- Logs: Runs without `-log` or `-scriptlog` append to `application.log` or `script.log` under `<main>/.cache/ghidra/settings/ghidra/<release>/`
- Logs: Log4j size rollover is unreliable with concurrent runs appending to one file
- Logs: ghidra-cli writes a daily debug log under `~/Library/Application Support/ghidra-cli/`, a folder no setting moves
- Use `search-code` for a Ghidra API signature

Numbered steps consume the step before. Bulleted cases are alternatives, one per command line in order.

## [01]-[READ]

Import, analyze, name stubs, and catalog the program in one run, then decompile seeds with their callers and callees into one file.

Imports:
- Analysis, stubs, then catalog
- One architecture of a universal Mach-O as the file the first line imports
- Headerless bytes, loader, base address without `0x`, and language named, then catalog

```bash
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -import <binary> -scriptPath <skill>/scripts -postScript Stubs.java <stubs> -postScript Catalog.java <catalog> -log <log> -scriptlog <scriptlog>
lipo -thin arm64 <binary> -output <main>/.artifacts/ghidra/<name>/<file>.arm64
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -import <file> -loader BinaryLoader -loader-baseAddr <hex> -processor <languageID> -scriptPath <skill>/scripts -postScript Catalog.java <catalog> -log <log> -scriptlog <scriptlog>
```

`<run>` opens the saved program without analysis:

```bash
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -process <file> -noanalysis -scriptPath <skill>/scripts -log <log> -scriptlog <scriptlog>
```

After import:
1. Seeds with callers and callees into one file
2. Block dividers with line numbers, then `Read <out>` at the line of one function
3. Failed blocks, seeded again with `timeout=<seconds>` after a timeout and `payload=<megabytes>` after `Response buffer size exceeded`

```bash
<run> -readOnly -postScript Decompile.java <out> 'str:<needle>' 0x<hex> callees=1 callers=1
rg -n '^// --- \[' <out>
rg -n '^// failed:' <out>
```

Alternative runs:
- Whole program as the seed
- Call sites of the seeds

```bash
<run> -readOnly -postScript Decompile.java <out> 're:.'
<run> -readOnly -postScript CallSites.java <out> <seed>...
```

Seeds come from the catalog:
- `[STRINGS]` rows name the functions holding the feature their text belongs to, `[IMPORTS]` rows name the functions calling each library
- `[FUNCTIONS]` rows separate a dispatcher from a leaf by size and callee count, `callers=0` marks a function no call reaches
- Library `<EXTERNAL>` holds symbols no dylib exports
- `_objc_msgSend_<selector>` calls in a block are the message sends
- Stub seeds of `CallSites.java` list one selector's sends, seed `_objc_msgSend` lists every send per selector

Seeds and settings of `Decompile.java` and `CallSites.java`:
- Seed: `0x<hex>` names the function containing the address
- Seed: Addresses in no function take `ghidra function create <address> --project <name> --program <file>`, then `ghidra stop --project <name>`
- Seed: `<name>` or `<namespace>::<name>` names functions by plain or qualified name, `re:<regex>` those with a matching plain or qualified name
- Seed: `str:<needle>` names functions referencing a defined string holding the needle case-insensitively, through a `__cfstring` struct included
- Seed: `tag:<tag>` names functions with the tag, `re:.` every non-external function
- Setting: `callers=<depth>` and `callees=<depth>` take 0 or more and default to 0, `timeout=<seconds>` and `payload=<megabytes>` take 1 or more and default to 30 and 50
- Setting: `CallSites.java` takes `timeout` and `payload` alone
- Errors: Seeds matching no function and bad settings print together in one error before the usage text
- Neighbors: Thunks and stubs among callers stand for their own callers, among callees for the function each reaches
- Targets: `CallSites.java` reads a thunk seed as the function it reaches

## [02]-[LOCATE]

Seeds below the catalog's top rows:
- Defined strings holding a needle case-insensitively
- Imports and functions with a name matching a regex

```bash
rg -n -i '<needle>' <catalog>
rg -n '<regex>' <catalog>
```

## [03]-[ANNOTATE]

Types and prototypes from C headers:
1. Predefined macros of the target from clang without blocks, one file of `#define` lines
2. Headers parsed under the macros

```bash
clang -dM -E -x c /dev/null -target <triple> -fno-blocks -o <macros>
<run> -postScript Headers.java <report> <header>... -I<dir>... -imacros <macros>
```

Edits through a bridge:
1. Subcommands from a file in one connection, one per line without `ghidra` and `#` comments, split on whitespace with no quoting
2. Bridge stopped
3. Catalog again when edits renamed its rows

```bash
ghidra batch <commands> --project <name> --program <file>
ghidra stop --project <name>
<run> -readOnly -postScript Catalog.java <catalog>
```

`Headers.java`:
- Defines: `-D<name>[=<value>]` on the command wins over a macro in the `-imacros` file
- Defines: CParser rejects the `^` block pointers `__BLOCKS__` enables, `-fno-blocks` keeps the macro out of `<macros>`
- Arguments: Every `-I<dir>` names a directory and every header a file
- Include: CoreFoundation and C library headers parse under `-I` framework `Headers`, `usr/include`, and clang's `include`
- Report: `[HEADERS]` rows parsed or failed per header, `[PREPROCESSOR]` and `[PARSER]` messages, then `[APPLIED]` rows per function
- Prototypes: Definitions apply to functions named with and without a leading underscore and follow a thunk to the thunked function

## [04]-[FORMAT]

Every file opens with `// --- [INDEX]` over `// <program> <language> <counts>`, sections with `// --- [NAME]` padded to column 90:
- Index: `args=<arguments>` closes the counts of a script that takes arguments after `<out>`
- Catalog: `[STRINGS]` rows `<address> <text> functions=<count> <name>...`, `[IMPORTS]` rows `<library> <prototype> callers=<count> <name>...`
- Catalog: `[FUNCTIONS]` rows `<address> <name> size=<bytes> callers=<count> callees=<count>`
- Catalog: `[STRINGS]` and `[IMPORTS]` rows rank by count and `[FUNCTIONS]` rows by size, names sort by entry address
- Catalog: Callers of a thunk or stub count for the function it reaches, thunks and stubs hold no `[FUNCTIONS]` row
- Decompile: Blocks `// --- [<name>]` over `// <address> size=<bytes> <role> callers=<count>[: <names>]`
- Decompile: `<role>` is `seed`, `caller:<depth>`, or `callee:<depth>`, `<names>` the callers with a block in the file by entry address
- Decompile: Failed blocks hold `// failed: <cause>` under their header, thunk and stub seeds hold `// <address> thunk -> <name>` or `stub -> <name>`
- Decompile: Decompiler prints `$` and `:` of a stub name as `_`, `_objc_msgSend$length` appears as `_objc_msgSend_length` in a block
- Decompile: `[GLOBALS]` holds one row `// <address> <type> <name>[ = <value>]` per global the blocks reference
- Decompile: `[TYPES]` holds Ghidra's built-in typedefs, then every composite, enum, and typedef the blocks name
- CallSites: `// --- [<target>]` over `// <name> @ <address> callers=<count> calls=<count>`
- CallSites: Rows `<address> <caller>: [$<selector> ]<arguments>` sorted by caller entry then address
- CallSites: Stub call arguments open with the receiver, `$?` marks a stub with no selector string resolved
- CallSites: `[FAILED]` lists each caller the decompiler failed on and each call holding no argument list, with the cause
- Stubs: `[STUBS]` rows `<address> <old> -> <new>` per rename, `unchanged`, `unresolved`, or `rejected: <message>` after the name otherwise

## [05]-[API]

Behaviors of the Ghidra API that decide how a script reads or writes a program:
- Stubs: `Objective-C Message Analyzer` names and types stubs only with `ID` and `SEL` under `/_objc2_`, without them analysis leaves `FUN_<hex>`
- Stubs: `__objc_msgSend_stub`, the calling convention Mach-O import installs, passes receiver and message arguments without `x1`
- Stubs: Selector is the string the stub's load references
- Types: `DataTypeWriter` writes the built-in typedefs from the constructor and skips a `FunctionDefinition` on write
- Strings: `DefinedDataIterator.byDataInstance` with `StringDataInstance::isString` walks defined strings, a `__cfstring` struct references the text
- Functions: `getFunctions(true)` skips externals, `getCallingFunctions` keeps call references to the entry alone
- Thunks: `getFunctionThunkAddresses` returns null with no thunks, `getThunkedFunction(true)` follows a chain to the end
- Addresses: `AddressFactory.getAddress` parses hex with or without `0x` and returns null for text holding no address
- Decompile: `ParallelDecompiler.decompileFunctions` returns results in completion order, null for a cancelled item, and rethrows a callback exception
- Decompile: `DecompilerCallback.setTimeout` sets the timeout passed per function, `DecompileOptions.setDefaultTimeout` changes nothing under it
- Globals: `getGlobalSymbolMap` holds a fraction of the globals a body names, the C markup tokens hold every one
- Preprocessor: `-D` takes no macro arguments, a prelude through `ReInit` and `Input()` defines a function-like macro
- Preprocessor: `Define()` ignores a redefinition
- Preprocessor: `#if <name>` evaluates true for a definition that is no number, an undefined name compares as text and `<name> == 0` evaluates false
- Preprocessor: `DefineTable.subParams` cuts a `...` argument at its first `)` at depth 0, a parenthesized group in it leaves the body unexpanded
- Preprocessor: `DefineTable.getParams` toggles quote and apostrophe states apart, a `'` inside a string literal hides the closing `)`
- Preprocessor: `PreProcessor` defines no compiler built-in (`__has_include`, `__has_feature`, `__builtin_va_list`) and cannot lex `::`
- Preprocessor: Lexer reads a `__has_include` `<file>` argument at parenthesis depth 0 alone
- Preprocessor: `#include` lookup per directory matches `CParserUtils.getFile`, a path miss falls back to the file name in that directory
- Parser: `CParser` reads `__const` and `__restrict` as qualifiers, and `restrict` as an identifier
- Parser: `CParser` knows no fixed enum, `__int128_t`, `__uint128_t`, or `_Float16`, Ghidra's `int16`, `uint16`, and `float2` name the types
- Parser: `CParser` float literals end in `f`, `F`, `d`, or `D` alone, integer literals take `u`, `l`, and `ll`
- Parser: `CParser` rejects an unnamed pointer parameter with an array suffix, `char * []`, and reads the named form and `int []`
- Parser: `CParserUtils.parseHeaderFiles` swaps `System.out` and stops at the first failed header, a script drives each header itself
- Source: `lib/<module>-src.zip` beside each module jar under `$GHIDRA_INSTALL_DIR/Ghidra` holds the installed source

## [06]-[EXTEND]

Each new script is one `GhidraScript` file in the bundle, seeds through `Arguments.parse`, lines through `Report.write`:
1. Bundle compiled against the install's jars with every warning on
2. `google-java-format`, `pmd check`, and `ast-grep scan` of `<project>:lint` over the scripts directory, jdtls diagnostics
3. Run on a saved program

```bash
javac -d <main>/.artifacts/ghidra/classes -Xlint:all,-path -cp "$(fd -p '/lib/[^/]+\.jar$' "$GHIDRA_INSTALL_DIR/Ghidra" | paste -sd: -)" <skill>/scripts/*.java
<run> -readOnly -postScript <Script>.java <out> <seed>...
```
