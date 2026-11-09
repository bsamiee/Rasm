---
name: use-ghidra
description: "Use when reading, annotating, or emulating a binary through Ghidra, covering headless scripts and the Serve process"
---

# [GHIDRA]

`analyzeHeadless` imports each binary once into one Ghidra project under `$GHIDRA_PROJECT_DIR` named after the binary's file name, with its architecture slices and earlier releases as programs named after their files. Every question on one binary runs bundle scripts on that project's saved programs, run after run or through its one `Serve.java` process.

[SCRIPTS]:
- [01]-[SERVE](scripts/Serve.java): Postscript running bundle scripts on request against programs of its project
- [02]-[ANALYZERS](scripts/Analyzers.java): Prescript skipping named analyzers during an import
- [03]-[STUBS](scripts/Stubs.java): Postscript naming and typing Objective-C message-send stubs
- [04]-[CATALOG](scripts/Catalog.java): Postscript writing the program index seeds come from
- [05]-[GRAPH](scripts/Graph.java): Postscript writing every call edge and non-call reference to a function
- [06]-[DECOMPILE](scripts/Decompile.java): Postscript decompiling seeds and their call neighborhood into one file
- [07]-[CALLSITES](scripts/CallSites.java): Postscript listing seed call sites with resolved arguments
- [08]-[RTTI](scripts/Rtti.java): Postscript listing Itanium C++ typeinfo, vtables, slots, and vtable users
- [09]-[EMU](scripts/Emu.java): Postscript emulating a plan of runs with external calls stubbed
- [10]-[APPLY](scripts/Apply.java): Postscript applying names, labels, comments, prototypes, and locals from row files
- [11]-[HEADERS](scripts/Headers.java): Postscript applying C header types and prototypes
- [12]-[DROP](scripts/Drop.java): Postscript removing data types by path
- [13]-[ARGUMENTS](scripts/rasm/Arguments.java): Usage text, seeds, and settings every postscript shares
- [14]-[FUNCTIONS](scripts/rasm/Functions.java): Call graph and decompile operations every postscript shares
- [15]-[REPORT](scripts/rasm/Report.java): Result type, sections, and file writer every postscript shares

[FACTS]:
- Run: Postscripts take the output path as their first argument after the script name
- Run: Runs exit 0 after a script error, `REPORT SCRIPT ERROR` in output and a missing `<out>` mark failure
- Run: Decompilers that cannot open the program fail `Decompile.java` and `CallSites.java` with their message, one function's failure stays a row
- Install: The Ghidra row in `.config/mise.toml` builds the decompiler a release lacks for its platform
- Install: `mise install --force github:NationalSecurityAgency/ghidra` reruns `postinstall`, imports abort on a missing project folder
- Help: `$GHIDRA_INSTALL_DIR/support/analyzeHeadlessREADME.md` lists every headless flag
- Project: `.config/mise.toml` names `$GHIDRA_PROJECT_DIR` outside the repository, Ghidra rejects a project path with a dot-prefixed component
- Project: `<name>/<folder>` as the project argument places an import under `/<folder>`, one folder per slice or release
- Project: `-deleteProject` on an `-import` deletes the project that run created with its analysis
- Program: Repeated imports into one folder add `<file>.<n>`, `-process <file>` keeps reading the first
- Lock: One JVM opens a project, `-readOnly` included, a second run aborts with `LockException` while that JVM lives
- Lock: `lsof -t "$GHIDRA_PROJECT_DIR/<name>.lock~"` prints the holding JVM's pid, empty output means lock files a dead run left block nothing
- Lock: Runs stop by that pid, killing the `analyzeHeadless` shell leaves its child JVM holding the lock
- Lock: Imports into one project chain with `&&` in one Bash call, runs on other projects start in parallel
- Writes: Runs save every script change at exit and `-readOnly` discards them, `Serve.java` saves each changed program with or without `-readOnly`
- Keys: Targets are a name, `0x<hex>`, or `FUN_<hex>`, rows print hex with no `0x`
- Keys: Renames replace the `FUN_<hex>` name, the entry address stays the key
- Analysis: `Apply Data Archives` applies `mac_osx.gdt` to a Mach-O at import, libc and CoreFoundation prototypes need no header
- Analysis: `-process` without `-noanalysis` re-analyzes the whole program
- Analysis: `Objective-C Message Analyzer` recurses with no cancellation check past `-analysisTimeoutPerFile`, imports skip it for `Stubs.java`
- Memory: Every process caps its heap at the install's `MAXMEM_DEFAULT` and holds it until exit
- Memory: Decompiles spawn up to min(cores + 1, 10) native `decompile` processes outside the heap, `-max-cpu <n>` caps one run at n + 1
- Time: Imports and `Serve.java` take Bash `run_in_background: true` and end with a task completion notice, `-process` script runs take the foreground
- Bundle: `-scriptPath` compiles each listed directory as one bundle at the JDK `JAVA_HOME` names, subdirectories become packages
- Bundle: Compile failures drop the failing file and every file referencing it
- Parallel: Processes share the settings, cache, and temp folders `.config/mise.toml` names
- Parallel: Ghidra serializes bundle builds and loads across processes under `osgi/parallel.lock`
- Parallel: Each process pays a JVM start, bundle load, and project open, a worker runs one process over its whole share of inputs
- Files: `<out>`, `<stubs>`, `<catalog>`, and `<report>` go under `<main>/docs/research/<area>/<subject>/decompiled/<program>/` alone
- Files: `<macros>`, `<plan>`, and `<rows>` go under `<main>/.artifacts/ghidra/<name>/`
- Files: `<main>` is the repository root
- Files: Scripts and `-log` create their file's folder, `lipo` and redirects need `mkdir -p`
- Logs: `<main>/.artifacts/logs/ghidra.log` appends each run's messages and printed lines, `ghidra-script.log` the printed lines alone
- Logs: Log4j size rollover is unreliable with concurrent runs appending to one file
- Use `Skill(search-code)` for a Ghidra API signature

Numbered steps consume the step before. Bulleted cases are alternatives, one per command line in order.

## [01]-[READ]

Projects saved under `$GHIDRA_PROJECT_DIR`, a listed project takes no second import or copy, a binary without one takes an import:

```!
mkdir -p "$GHIDRA_PROJECT_DIR" && find "$GHIDRA_PROJECT_DIR" -maxdepth 1 -name '*.gpr' -exec basename -s .gpr -a {} +
```

`<headless>` opens project `<name>` with the scripts bundle:

```bash
analyzeHeadless "$GHIDRA_PROJECT_DIR" <name> -scriptPath ${CLAUDE_SKILL_DIR}/scripts
```

Program paths of project `<name>`, index files under `<name>.rep` belong to Ghidra alone:

```bash
<headless> -process -recursive -readOnly -noanalysis 2>/dev/null | rg -or '$1' 'project file: (\S+)'
```

Imports:
- Analysis with the Objective-C message analyzer off, stubs, then catalog
- One architecture `lipo -archs <binary>` lists of a universal Mach-O as the file the first line imports
- Headerless bytes, loader, base address without `0x`, and language named, then catalog

```bash
<headless> -import <binary> -preScript Analyzers.java 'Objective-C Message Analyzer' -postScript Stubs.java <stubs> -postScript Catalog.java <catalog>
lipo -thin <arch> <binary> -output <main>/.artifacts/ghidra/<name>/<file>.<arch>
<headless> -import <file> -loader BinaryLoader -loader-baseAddr <hex> -processor <languageID> -postScript Catalog.java <catalog>
```

`<run>` opens the saved program without analysis:

```bash
<headless> -process <file> -noanalysis
```

After import:
1. Seeds with callers and callees into one file
2. Block dividers with line numbers, then `Read` of `<out>` with `offset` at one function's line
3. Failed blocks, seeded again with `timeout=<seconds>` after a timeout and `payload=<megabytes>` after `Response buffer size exceeded`

```bash
<run> -readOnly -postScript Decompile.java <out> 'str:<needle>' 0x<hex> callees=1 callers=1
rg -n '^// --- \[' <out>
sed -n '/^\/\/ --- \[FAILED\]/,/^\/\/ --- \[GLOBALS\]/p' <out>
```

Alternative runs:
- Whole program as the seed
- Call sites of the seeds
- Every call edge and non-call reference
- C++ classes from typeinfo and vtables

```bash
<run> -readOnly -postScript Decompile.java <out> 're:.'
<run> -readOnly -postScript CallSites.java <out> <seed>...
<run> -readOnly -postScript Graph.java <out>
<run> -readOnly -postScript Rtti.java <out>
```

Seeds come from the catalog:
- `[STRINGS]` rows name the functions holding the feature their text belongs to, `[IMPORTS]` rows name the functions calling each library
- `[FUNCTIONS]` rows separate a dispatcher from a leaf by size and callee count, `callers=0` marks a function no call reaches
- Library `<EXTERNAL>` holds symbols no dylib exports
- `_objc_msgSend_<selector>` calls in a block are the message sends
- Stub seeds of `CallSites.java` list one selector's sends, seed `_objc_msgSend` lists every send per selector

Seeds and settings of `Decompile.java` and `CallSites.java`:
- Seed: `0x<hex>` names the function containing the address, an `Apply.java` row creates a function where none starts
- Seed: `<name>` or `<namespace>::<name>` names functions by plain or qualified name, `re:<regex>` those with a matching plain or qualified name
- Seed: `str:<needle>` names functions referencing a defined string holding the needle case-insensitively, through a `__cfstring` struct included
- Seed: `tag:<tag>` names functions with the tag, `re:.` every non-external function
- Setting: `callers=<depth>` and `callees=<depth>` take 0 or more and default to 0
- Setting: `timeout=<seconds>` and `payload=<megabytes>` take 1 or more and default to 30 and 50
- Setting: `CallSites.java` takes `timeout` and `payload` alone
- Errors: Seeds matching no function and bad settings print together in one error before the usage text
- Neighbors: Thunks and stubs among callers stand for their own callers, among callees for the function each reaches
- Targets: `CallSites.java` reads a thunk seed as the function it reaches

## [02]-[SERVE]

Repeated reads and edits on one project run through one `Serve.java` process listening on `$GHIDRA_PROJECT_DIR/<name>.sock`:
1. Process in the background on any program of the project
2. One request per connection, the reply printing `ok <seconds>s[ saved <program>]` or `failed <seconds>s <cause>`
3. Stop, then the headless run saves and releases the project

```bash
<run> -postScript Serve.java
printf '%s\n' <program> <script> <argument>... | paste -s - | nc -U "$GHIDRA_PROJECT_DIR/<name>.sock"
echo stop | nc -U "$GHIDRA_PROJECT_DIR/<name>.sock"
```

- Requests: `<program>` is a project path (`<file>` or `/<folder>/<file>`), `<script>` any script on `-scriptPath`, tab-split cells keep spaces
- Requests: Run complete lines one at a time, a script edited between requests rebuilds before its next run
- Socket: Intake and reply each allow five seconds, a request line holds at most 64 KiB
- Socket: `nc` exits 1 with no reply when no `Serve.java` runs on the project

## [03]-[LOCATE]

Seeds below the catalog's top rows:
- Defined strings holding a needle case-insensitively
- Imports and functions with a name matching a regex

```bash
rg -n -i '<needle>' <catalog>
rg -n '<regex>' <catalog>
```

## [04]-[ANNOTATE]

Edits from row files, one row per line with tabs between cells, apply names first, then prototypes, local removals, and renames over one decompile, then `Catalog.java` rewrites the catalog in the same process:

```bash
<run> -postScript Apply.java <report> <rows>... -postScript Catalog.java <catalog>
```

`Apply.java` rows:
- Cells: `<address> <namespace> <name> <prototype> <comment> <locals> <kind>`, `-` or an empty cell for none, `#` lines skipped
- Kind: `function` (default) names, comments, and creates a function where none starts, `label` places a primary label and plate comment
- Namespace: `<a>::<b>` creates the hierarchy and turns its innermost namespace into a class
- Prototype: C declaration parsed against the program's types, its function name ignored, a missing type rejects the row
- Locals: `<local>=<name>[:<type>]` renames and retypes a decompiled local or parameter, `<local>=` or `re:<regex>=` removes stored locals

Types and prototypes absent from `mac_osx.gdt`, from C headers:
1. Predefined macros of the target from clang without blocks, one file of `#define` lines
2. Headers parsed under the macros, a header of `typedef struct <name> <name>;` lines declares types a prototype names

```bash
clang -dM -E -x c /dev/null -target <triple> -fno-blocks -o <macros>
<run> -postScript Headers.java <report> <header>... -I<dir>... -imacros <macros>
```

`Headers.java`:
- Defines: `-D<name>[=<value>]` on the command wins over a macro in the `-imacros` file
- Defines: CParser rejects the `^` block pointers `__BLOCKS__` enables, `-fno-blocks` keeps the macro out of `<macros>`
- Arguments: Every `-I<dir>` names a directory and every header a file
- Include: CoreFoundation and C library headers parse under `-I` framework `Headers`, `usr/include`, and clang's `include`
- Prototypes: Definitions apply to functions named with and without a leading underscore and follow a thunk to the thunked function

Types removed by path:

```bash
<run> -postScript Drop.java <report> /<category>/<name>...
```

`AutoVersionTrackingScript.java` copies names, comments, and prototypes from an analyzed earlier release in the same project:

```bash
<run> -postScript AutoVersionTrackingScript.java /<sessions> <session> /<folder>/<file>
```

## [05]-[EMULATE]

`Emu.java` runs a JSON plan of runs on fresh p-code machines over the program image:

```json
{
    "steps": 2000000,
    "stubs": { "<external>": "<sleigh>" },
    "runs": [
        {
            "id": "<id>",
            "entry": "<seed> | 0x<hex>",
            "keep": false,
            "steps": 2000000,
            "registers": { "<register>": "0x<hex>" },
            "memory": { "0x<hex>": "<hex bytes>" },
            "reads": { "0x<hex>": 16 },
            "results": [ "<register>" ]
        }
    ]
}
```

```bash
<run> -readOnly -postScript Emu.java <out> <plan>
```

- Entry: Seed matching one function or an address, `keep` runs on memory the previous run left
- Stubs: Thunks and pointer slots of an external function run the plan's Sleigh for its name or zero the return register, then return
- Stubs: Sleigh writes registers and memory (`x0 = *[ram]:8 0x<hex>:8;`), a heap is a cell the stub advances
- Layout: Sentinel return address, stub traps, and stack sit above the program's last block, inputs go outside program blocks and layout
- Stops: `returned` at the sentinel, `exhausted` at the step budget, `faulted` with the p-code fault
- Results: Default calling convention's return register when a run names none
- Userops: Language operations with no definition run as no-ops and count per name

## [06]-[FORMAT]

Every file opens with `// --- [INDEX]` over `// <program> <language> <counts>`, sections with `// --- [NAME]` padded to column 90:
- Index: `args=<arguments>` closes the counts of a script that takes arguments after `<out>`
- Catalog: `[STRINGS]` rows `<address> <text> functions=<count> <name>...`, `[IMPORTS]` rows `<library> <prototype> callers=<count> <name>...`
- Catalog: `[FUNCTIONS]` rows `<address> <name> size=<bytes> callers=<count> callees=<count> source=<source>`
- Catalog: `[STRINGS]` and `[IMPORTS]` rows rank by count and `[FUNCTIONS]` rows by size, names sort by entry address
- Catalog: Callers of a thunk or stub count for the function it reaches, thunks and stubs hold no `[FUNCTIONS]` row
- Graph: `[CALLS]` rows `<address> <name> -> <address> <name>`, `[REFERENCES]` rows `<address> <name> <- <from> <address> <name>|-`
- Decompile: Blocks `// --- [<name>]` over `// <address> size=<bytes> <role> callers=<count>[: <names>]`
- Decompile: `<role>` is `seed`, `caller:<depth>`, or `callee:<depth>`, `<names>` the callers with a block in the file by entry address
- Decompile: `[FAILED]` rows `// <address> <name> <role>: <cause>`, thunk and stub seeds hold `// <address> thunk -> <name>` or `stub -> <name>`
- Decompile: Decompiler prints `$` and `:` of a stub name as `_`, `_objc_msgSend$length` appears as `_objc_msgSend_length` in a block
- Decompile: `[GLOBALS]` holds one row `// <address> <type> <name>[ = <value>]` per global the blocks reference
- Decompile: `[TYPES]` holds Ghidra's built-in typedefs, then every composite, enum, and typedef the blocks name
- CallSites: `// --- [<target>]` over `// <name> @ <address> callers=<count> calls=<count>`
- CallSites: Rows `<address> <caller>: [$<selector> ]<arguments>` sorted by caller entry then address
- CallSites: Stub call arguments open with the receiver, `$?` marks a stub with no selector string resolved
- CallSites: `[FAILED]` lists each caller the decompiler failed on and each call holding no argument list, with the cause
- Rtti: `[TYPEINFO]` rows `<address> class|si|vmi <type> [<base>@<offset>/<flags>...]`, types in Itanium encoding `c++filt _ZTS<type>` reads
- Rtti: `[VTABLES]` rows `<vtable> <typeinfo> <type> offset=<offset-to-top> slots=<count>`
- Rtti: `[SLOTS]` rows `<vtable> <slot> <target> function|import|code <name>`, `[USERS]` rows `<vtable> <from> <function>|-`
- Rtti: `[FAILED]` rows `<address> class|si|vmi|vtable|slot reads unmapped memory: <message>`
- Emu: `[LAYOUT]` rows `sentinel|traps|stack <address>`, `[STUBS]` rows `<address> <name> requested|fallback[ slot=<address>...]`
- Emu: `[RUNS]` rows `<id> <stop> steps=<count> pc=<address>[: <cause>] <register>=0x<hex>...`, `[CALLS]` rows `<id> <step> <name> from <address>`
- Emu: `[READS]` rows `<id> <address> <hex>`, `[USEROPS]` rows `<id> <name> count=<count>`
- Emu: `[EXECUTED]` and `[UNINITIALIZED]` rows `<id> [<first>, <last>]` per range executed or read outside the image and the run's writes
- Apply: `[NAMED]`, `[LABELED]`, `[TYPED]`, `[REMOVED]`, and `[RENAMED]` rows `<address> <result>`, `[REJECTED]` rows `<address> <step>: <cause>`
- Headers: `[PARSED]` and `[FAILED]` per header, `[PREPROCESSOR]` and `[PARSER]` messages, `[APPLIED]` and `[REJECTED]` per function
- Stubs: `[RENAMED]` rows `<address> <old> -> <new>`, `[UNCHANGED]`, `[UNRESOLVED]`, and `[REJECTED]` rows `<address> <name>[: <message>]`
- Drop: `[REMOVED]` and `[ABSENT]` rows per path

## [07]-[EXTEND]

Each new postscript is one `GhidraScript` file in the bundle, arguments through `Arguments.Usage` or `Arguments.parse`, lines through `Report.write`:
- Checks: `hk check <script paths>` passes on every script, its `javac`, `pmd`, `google-java-format`, and `ast-grep` steps included
- Source: `lib/<module>-src.zip` beside each module jar under `$GHIDRA_INSTALL_DIR/Ghidra` holds the installed source
