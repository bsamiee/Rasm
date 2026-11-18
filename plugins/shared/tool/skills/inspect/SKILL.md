---
name: inspect
description: "Use when a question asks what a binary, app bundle, running process, system registration, crash report, file, host document, simulator, or web page holds, covering one installed command per question"
---

# [TOOL_INSPECT]

Each line answers one question with one installed command, read-only, grouped by target kind. Tools come from Parametric_Forge and Rasm mise, and `runbooks/tools/` in mazan-group holds every flag a line leaves out.

[FACTS]:
- Universal Mach-O files take `-a <arch>` in `ipsw` and `swift-section`, `lipo -archs <bin>` names each slice
- `ipsw` prints JSON for headers and load commands alone, `rabin2 -j` prints JSON for symbols, imports, exports, and sections
- `rea` prints TOON, `--format json` switches to JSON, `--filter-output <keys>` keeps named keys, `--token-limit <n>` caps output
- `rea` reads one slice: universal binaries take `--architecture arm64`, Apple binaries with two arm64e slices fail
- Gatekeeper is off on this Mac (`spctl` prints `override=security disabled`), so `syspolicy_check` gives the verdict
- `asar extract-file`, `rabin2 -x`, and `ipsw car --output` write into the current folder or the one named, run them from a scratch folder
- `sudo` reads (`spindump <pid>`, `eslogger`, `fs_usage`) wait on the owner's Touch ID
- Decompiling a function or emulating code goes through `tool:ghidra`, driving a desktop app through `tool:watch-app`
- Script-only answers name the library a script calls, no installed command answers them

## [01]-[BINARIES]

Mach-O executables, dylibs, frameworks, and managed assemblies:
- Header, load commands, linked libraries, signature blob: `ipsw macho info --json -a <arch> <bin>`
- Imports, exports, symbols, sections, strings as JSON: `rabin2 -j -i <bin>`, `-E`, `-s`, `-S`, `-z` in place of `-i`
- Fixup chains, `dlopen` and `dlsym` call sites: `dyld_info -fixup_chains -dlopens -dlsyms <bin>`
- Minimum OS and SDK of every Mach-O in a bundle: `dyld_info -platform -all_dir <app>`
- Code that runs before `main` (static initializers): `rea inspect-native-initializers <bin> --format json`
- Install-name and rpath resolution of every dependency: `rea trace-dylib-resolution <bin|app> --architecture <arch>`
- Hardening (PIE, stack canary, hardened runtime): `rea inspect-binary-hardening <bin>`
- Objective-C classes, protocols, categories: `ipsw class-dump <bin> --arch <arch>`
- Swift interface with types, protocols, and conformances: `swift-section interface -a <arch> <bin>`
- Mach-O files of a bundle by linked library: `ipsw macho search <app> --import '<regex>'`
- Same search by symbol, selector, class, protocol, section, or launch constraint: `--sym`, `--sel`, `--class`, `--protocol`, `--section`, `--launch-const`
- Swift ABI break between versions: `swift-section diff -a <arch> --summary-only <old> <new>`, a side a binary or `snapshot` JSON
- Swift symbol in plain form: `xcrun swift-demangle --compact '<symbol>'`
- .NET assembly types and members (`.dll`, `.rhp`, `.gha`): `ilspycmd -l cisde <dll>`
- One .NET type as C#: `ilspycmd -t <Namespace.Type> <dll>`
- Assembly a loaded solution references (`RhinoCommon`, `Grasshopper2`): roslyn-codelens `inspect_external_assembly`

## [02]-[APP_BUNDLES]

Apps, extensions, and installer packages:
- Signature, team, entitlements, notarization ticket: `codesign -dvvv --entitlements - --xml <app>`
- Gatekeeper and notarization verdict as JSON: `syspolicy_check distribution <app> --json`
- Signature integrity of nested code: `codesign --verify --deep --strict -vv <app>`
- Nested code with sandbox, hardened, notarized, Gatekeeper flags: `appy -c -q -T -C Kind -C Flags -C 'Signed By' <app>`
- Provisioning profile: `security cms -D -i <app>/Contents/embedded.provisionprofile | ipsw plist --no-color`
- `Info.plist` or any plist as JSON: `ipsw plist --no-color <file>`
- One plist key: `plutil -extract <keypath> raw <file>`
- Keyed archive or CoreData `.mom`: `fq -d bplist 'torepr | from_ns_keyed_archiver' <file>`
- AppleScript and JXA dictionary: `sdef <app> | yq -p xml -o json`
- App Intents of one app: `jq . <app>/Contents/Resources/Metadata.appintents/extract.actionsdata`
- Asset catalog keys: `ipsw car <Assets.car> --metadata-only --json`
- Asset catalog images: `ipsw car <Assets.car> --render --output <dir>`
- Compiled nibs and storyboards with objects and connections: `rea decode-interface-builder <app> --format json`
- Electron archive listing: `asar list <app>/Contents/Resources/app.asar`
- One file out of an Electron archive: `asar extract-file <asar> <path>`
- Electron fuses (`RunAsNode`, inspect arguments): `rea inspect-electron-fuses <app>`
- Electron app routes, modules, and endpoints: `rea analyze-javascript-application <asar>`
- Installer package signature, notarization, scripts: `spkg -q -w 0 -s <pkg>`

## [03]-[SYSTEM]

Registrations, preferences, and the dyld shared cache:
- Job a launchd label names: `launchctl print gui/$UID/<label>`
- Label behind a process: `witr --pid <pid> --json`, field `.Source.Name`
- App extensions: `pluginkit -mAvvv`, `-p com.apple.quicklook.preview` for one extension point
- Spotlight importers: `mdimport -L`
- Quick Look generators: `qlmanage -m plugins`
- Login items and background tasks: `sfltool dumpbtm`
- LaunchServices records: `/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister -dump Bundle`
- `defaults` domain as JSON: `defaults export <domain> - | ipsw plist --no-color`
- App Intents of every app: `sqlite3 -readonly -json ~/Library/Shortcuts/ToolKit/Tools-active '<query>'`, joining `Tools`, `ContainerMetadata`, `ToolLocalizations`
- Images of the dyld shared cache: `ipsw dyld info <dsc> --json`
- Symbol address inside the cache: `ipsw dyld symaddr <dsc> <symbol> --image <name>`
- Classes of a cache dylib: `ipsw class-dump <dsc> <dylib>`
- Public Swift interface of a system framework: `swift-section interface --uses-system-dyld-shared-cache -n <image> --exported-only`

`<dsc>` is `/System/Volumes/Preboot/Cryptexes/OS/System/Library/dyld/dyld_shared_cache_arm64e`. `ipsw dyld extract` fails on some images (`failed to remap symbol`), `ipsw dyld split` writes the whole cache instead.

## [04]-[REPORTS]

Crash, hang, and spin reports in `~/Library/Logs/DiagnosticReports` and `/Library/Logs/DiagnosticReports`:
- Newest reports of an app: `watch-app processes <app> --reports <count>`
- Crash report symbolicated by LLDB: `xcrun crashlog -c <file.ips>`
- Crash report threads, frames, and images as evidence: `rea inspect-apple-ips <file.ips> --format json`
- Hang or spin report: `spindump -i <file.hang> -timeline -noBinary -o <out>`
- Address in a binary or dSYM: `atos -o <bin|dSYM> -arch arm64 -l <load-address> <address>`
- Address in a running process: `atos -p <pid> <address>`

## [05]-[PROCESSES]

Running processes and the unified log:
- Parent chain, launchd source, sockets: `witr --pid <pid> --json`
- Process holding a port or file: `witr --port <port> --json`, `witr --file <path> --json`
- Open files as JSON: `lsof -n -P -p <pid> | jc --lsof`
- Listening ports as JSON: `lsof -n -P -iTCP -sTCP:LISTEN | jc --lsof`
- Entitlements in effect: `codesign -d --entitlements - --xml +<pid> | plutil -convert json -o - -`
- Memory footprint as JSON: `footprint -p <pid> --json <out.json>`
- Swift tasks, actors, and async backtraces of a debug build: `swift-inspect dump-concurrency <pid>`, outside the Bash sandbox
- Threads and loaded images: `sample <pid> 1 -file <out.txt>`
- Mapped regions: `vmmap -summary <pid>`
- Blocked threads of a hung process: `sudo spindump <pid> 2 10 -onlyTarget -onlyBlocked -noBinary -o <out>`
- Unified log as NDJSON: `log show --style ndjson --last <span> --predicate '<predicate>'`
- Accessibility tree of an app window: `watch-app read <pid>`

## [06]-[FILES]

Any file on disk:
- Content type as JSON: `magika --json <paths>`
- Type a bundle declares for an extension (`.3dm`, `.blend`): `mdls -name kMDItemContentTypeTree <path>`
- Metadata of any format: `exiftool -json -G1 -n <paths>`
- Media streams: `ffprobe -v error -show_format -show_streams -of json <file>`
- Structure of a binary format (Mach-O, bplist, MP4, PNG, ZIP, ICC): `fq '<expression>' <file>`
- Archive, disk image, or package members: `7zz l -slt -ba <file>`
- SQLite schema and rows: `sqlite3 -readonly -json <db> '<query>'`
- 3D scene meshes, materials, and node counts (FBX, OBJ, COLLADA, glTF): `assimp info <file>`
- Inventory and validation of a glTF file: `gltf-transform inspect <file.glb>`
- USD stage check: `usdchecker <file.usd>`
- IFC geometry out to glTF, OBJ, SVG: `IfcConvert <in.ifc> <out.glb>`
- IFC audit against IDS rules: `ifctester <rules.ids> <in.ifc> -r Json`
- IFC schema, element counts, spatial tree, materials as JSON: `ifcquery <in.ifc> summary`, `tree`, `materials`, `select <selector>`
- Any 3D file rendered to PNG with no window: `f3d --output=<out.png> --resolution=<w>,<h> <file>`
- PSD layers: `psd-tools show <file.psd>`
- PSD as SVG with text: `psd2svg <file.psd> <out.svg>`
- After Effects project tree: `aep-visualize --format json <file.aep>`
- Illustrator `.ai` as its PDF container: `mutool show <file.ai> trailer`
- Rhino `.3dm` objects: script over `rhino3dm` (`uv run --with rhino3dm <script>`), or `rhino:host` on a running Rhino
- Grasshopper `.gh` and `.ghx`: script over `GH_IO` or `GrasshopperIO`, or `rhino:host` on a running Rhino
- InDesign `.indd`: no reader without InDesign, IDML exports diff as text

## [07]-[HOSTS]

Adobe, Rhino, Unreal, and Blender projects and plug-ins:
- UXP plug-in manifest: `jq . <plugin>/manifest.json`
- CEP extension manifest: `yq -p xml -o json <extension>/CSXS/manifest.xml`
- UXP plug-ins a host loads: `jq '.plugins[]' ~/Library/Application\ Support/Adobe/UXP/PluginsInfo/v1/<ID|PS>.json`
- Extensions Creative Cloud records per release host: `UnifiedPluginInstallerAgent --list all`, which fails with Creative Cloud quit
- Adobe host scripting classes and enumerations: Atelier MCP resources `atelier://object-model/<host>` and `.../<host>/<name>`
- Live values, window image, Accessibility tree of the app `applications` `open` targets: Atelier MCP `execute_script`, `view`, `inspect`
- Adobe host crash reports, UXP logs, unified log, running or quit: Atelier MCP `diagnostics` with `application`
- Rhino packages installed: `yak list`
- Rhino package versions with platform and Rhino target per release: `xh -b https://yak.rhino3d.com/versions/<package>`
- Rhino settings folders and versions: `rh-transfer list`
- Live Rhino document and Grasshopper 2 canvas: `rhino:host`
- Unreal project or plug-in descriptor: `jq . <file.uproject|file.uplugin>`
- Unreal pak header and index: `repak info <file.pak>`, `repak list <file.pak>`
- Unreal pak files on disk, Oodle compression included: `UnrealPak <file.pak> -Extract <dir> -Filter=<glob>`
- Unreal IoStore container: `retoc info <file.utoc>`, `retoc list <file.utoc>`
- Unreal trace session, threads, and event types: `zen trace inspect <file.utrace>`, timer CSV export in `cpp:unreal`
- Unreal `.uasset` contents: script over UAssetAPI or CUE4Parse, or a `UnrealEditor-Cmd <project> -run=pythonscript -Script=<py>` run
- Blender file dependencies: `blender-asset-tracer list <file.blend>`
- Blender datablocks of a running Blender: Blender MCP `get_blendfile_summary_datablocks`, a file on disk served first by `blender --background <file.blend> --command blender_mcp --port 9877`
- Blender extension manifest check: `blender --command extension validate <dir>`

## [08]-[SIMULATORS]

iOS simulators on the installed runtime, booted headless with no window:
- Devices with `udid` and state: `xcrun simctl list devices available -j`
- Boot before a read: `xcrun simctl bootstatus <udid> -b`, `xcrun simctl shutdown <udid>` after
- Installed apps: `xcrun simctl listapps <udid> | plutil -convert json -o - -`, `appinfo <udid> <bundle-id>` for one
- App bundle or data container: `xcrun simctl get_app_container <udid> <bundle-id> app`, `data` for the container
- Device unified log as NDJSON: `xcrun simctl spawn <udid> log show --style ndjson --last <span>`
- Screen image: `xcrun simctl io <udid> screenshot <out.png>`
- UI hierarchy with frames: Xcode MCP `DeviceInteractionStartSession`, then `DeviceInteractionSynthesize` with no command, `DeviceInteractionEndSession` after

## [09]-[WEB]

Pages, browsers, and browser extensions:
- Page accessibility snapshot and refs: `playwright cli -s=<name> open <url>`, `snapshot`, then `close`
- Network requests, console, performance trace of a page: `chrome-devtools <command> --output-format json`, `chrome-devtools --help` lists commands
- Lighthouse scores and failing audits of a page: `chrome-devtools lighthouse_audit <pageId> --output-format json`
- Heap snapshot of a page: `chrome-devtools take_heapsnapshot <pageId> <file>`, then `get_heapsnapshot_summary <file>`
- Same reads as MCP tools: `chrome-devtools`, `firefox-devtools`, and `safari` servers of the `browser` plugin
- Firefox profile from a recording: `profiler-cli load <profile.json>`, then the queries `profiler-cli --help` lists
- Chrome extension manifest: `jq . ~/Library/Application\ Support/Google/Chrome/Default/Extensions/<id>/<version>/manifest.json`
