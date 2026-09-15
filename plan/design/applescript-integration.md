# [APPLESCRIPT_INTEGRATION]

Decision record for the Apple Events channel in `apps/creative-cloud`. Measured 2026-09-11 on macOS 26.6.1 (25G76), Apple Silicon, Command Line Tools only (no Xcode), node 26.8.1 under mise, osascript from `/usr/bin`. Running hosts during the probes: Photoshop 27.11.0 (`com.adobe.Photoshop`), InDesign 21.6.0.58 (`com.adobe.InDesign`), Acrobat 26.002.21901 (`com.adobe.Acrobat.Pro`). Not running: Illustrator 30.9.0 (`com.adobe.illustratorBeta`), Typeface-beta 4.5.0 (`com.criminalbird.typeface.beta`); neither was launched, so every Illustrator and Typeface runtime claim below is labelled unverified. Dictionaries were read from the bundles on disk (`Contents/Resources/*.sdef`; InDesign synthesized through `OSACopyScriptingDefinitionFromURL`). Every claim carries a source in [08]; `unverified` marks what no document or probe settled. `<Sources>` = `plan/research/sources`.

## [01]-[PER_HOST]

| [INDEX] | [HOST] | [APPLE_EVENTS_NEEDED] | [IN-PROCESS_CHANNELS_THAT_EXIST] | [ALTERNATIVE_REJECTED] | [STATUS] |
| :-----: | :----- | :-------------------- | :------------------------------- | :--------------------- | :------- |
| [01] | Illustrator 30.9.0 | Yes, for every script | `do javascript` (sdef code `miscDjxM`, direct parameter `any` = "javascript code or file to execute", `with arguments` `JArg` list, `show debugger` `JXMd` enum `e940` = `before running`/`a942`, `never`/`Nevr`, `on runtime error`/`e941`, result `text`); `execute menu command` (`miscaEMC`, `menu command string` `pMCS`); `do script` (`miscdosc`) plays an Action; `get preset file of` (`miscaGPF`) | UXP: absent. Adobe's UXP support matrix (developer.adobe.com "UXP General Information", and the 2026-07-01 developer blog "UXP Changelog and Product Support Matrix") lists Photoshop, InDesign, InDesign Server, Premiere Pro, XD; Illustrator appears on no row. Built-in MCP (helpx "Connect Adobe Illustrator (Beta) to AI tools", last updated 2026-04-16; "Work with Adobe Illustrator (Beta) documents from AI tools", 2026-04-16): six tool categories (documents and artboards; structure and inspection; layers and groups; selection and transforms; art and text; export), no script-execution tool. CEP or an `.aip` plug-in: a second host runtime with its own install, signing, and panel process, adding no capability `do javascript` lacks; not built | Documented from the on-disk dictionary; runtime unverified (app not running) |
| [02] | Acrobat 26.002.21901 | Yes | `do script` (sdef `miscdosc`, direct parameter `text` "The actual text of the JavaScript to perform", optional `file` `alias`); `execute` (`CAROexec`) runs a menu item; core `open`/`save`/`close` | OLE `AcroExch.*`: Windows only ("This chapter describes how you can use OLE 2.0 support in Adobe Acrobat for Microsoft Windows"; "On Mac OS, you may use Apple events and AppleScript", Adobe IAC docs). Folder-level JavaScript with a file watcher: a handrolled transport inside Acrobat (`app.setInterval` polling a queue folder) duplicating what `do script` already provides; `javascripts/rasm-trusted.js` stays for the two privileged functions only | Proven: `do script "(function(){ return JSON.stringify({v: app.viewerVersion, n: app.activeDocs.length}); })()"` returned `{"v":26.00221901,"n":1}`, exit 0. The `file` parameter form is not used: a 2024-09-15 Adobe Community report records a crash on `do script file <alias>` (24.002.21005); unverified on 26.002 |
| [03] | InDesign 21.6.0.58 | Yes, until the UXP WebSocket plugin lands; none after | `do script` (sdef `K2  dosc`, direct `any`, `language` `doLg` enum `ScLg` = `unknown`/`javascript`/`uxpscript`/`applescript language`, `with arguments` `wArg` `any`, `undo mode` `pSUM` enum `eSUM` = `script request`/`entire script`/`auto undo`/`fast entire script`, `undo name` `unnm` text, result `any`) | `.idjs` through `do script f language uxpscript`: the enumerator exists in the 21.5 dictionary the app ships; execution of a `.idjs` file that way is unverified (not probed, a UXP script run is a job, and jobs run through the plugin once it lands) | Proven: `do script f language javascript with arguments {"alpha", "beta"}` with `f` a `POSIX file` set outside the `tell` returned `21.6.0.58 args=2 first=alpha`; the string form returned `21.6.0.58 args=2`; JXA `doScript(Path(...), {language: 'javascript', withArguments: [...]})` returned the same |
| [04] | Photoshop 27.11.0 | Yes, for one tool only (`system_report`) | `do javascript` (sdef `miscDjxM`, identical shape to Illustrator's), `do action` (`miscDoAc`) | UXP has no equivalent of `app.systemInformation`: the UXP Photoshop `app` class (developer.adobe.com ps_reference) lists `actionTree`, `activeDocument`, `backgroundColor`, `currentTool`, `displayDialogs`, `documents`, `fonts`, `foregroundColor`, `preferences`, `typename` and the methods `batchPlay`, `bringToFront`, `convertUnits`, `createDocument`, `getColorProfiles`, `open`, `showAlert`, `updateUI`; `require('uxp').host` gives `name`, `version`, `uiLocale`; `require('os').platform()` gives the OS. The ExtendScript `Application.systemInformation` ("System information of the host application and machine", string) has no UXP counterpart, so AppleScript stays for that one read; every other Photoshop job is the WebSocket plugin | Proven: `do javascript "app.version + ' args=' + arguments.length" with arguments {"alpha", "beta"}` returned `27.11.0 args=2`. The file form failed three ways (see [03] row [05]); `system_report` needs no file, its script is the one expression `app.systemInformation` |
| [05] | System Events (AX) | Only for key chords | Processes Suite: `click` (`prcsclic`, direct `UI element`, `at` `{x, y}`), `keystroke` (`prcskprs`, text, `using` modifiers), `key code` (`prcskcod`), `perform` (`prcsperf`, `action`), `select` (`miscslct`); hidden suite `cancel`/`confirm`/`increment`/`decrement`/`pick`/`key up`/`key down`; class `UI element` (`uiel`) with `entire contents`, `position`, `size`, `role`, `subrole`, `value`, elements `attribute` (`attr`: `name`, `settable`, `value`) | Reads and clicks move to `@crowecawcaw/xa11y` 0.14.0 (npm, published 2026-09-09; MIT; macOS backend `AXUIElement`): `App.byPid(pid)`, `app.tree(maxDepth)` returns a typed `TreeNode`, `app.dump()`, `app.locator('menu_item[name="Color Settings..."]').press()`, `Element.raw` carries `ax_role`/`ax_subrole`, `subscribe()` delivers `windowOpened`/`menuOpened` events. System Events' `entire contents` returns references without attributes and coerces `AXFrame` to `03918001121` (`ax-README.md` measurement, 2026-09-11); the Swift `ax-dump` binary is a handrolled copy of what the package ships; JXA `ObjC.bindFunction('AXUIElementCopyAttributeNames', ['int', ['id', 'id*']])` works after `ObjC.import('Cocoa')` (same README) but is an untyped FFI in a scripting language. xa11y's action set (`press`, `focus`, `blur`, `toggle`, `expand`, `collapse`, `select`, `set_value`, `type_text`, `increment`, `decrement`, `show_menu`) has no modifier chord, so the one System Events use that remains is `keystroke "." using command down` to dismiss a Drover dialog, which exposes no AX Cancel button and ignores Escape (`ax-README.md` [03]) | Reads and clicks: documented (package API), unverified in this repo. Key chord: proven by `<Sources>/ax/lib/dialog-close.applescript` `dialog-close.applescript` |
| [06] | Typeface-beta 4.5.0 | Yes | Typeface Suite (`tfsu`): `activatefonts` (`tfsuActi`, direct parameter list of `text` "The postscript names to activate", optional `requested by` `tfra` text), `ping` (`tfsuPing`); Standard Suite `count`/`delete`/`duplicate`/`exists`/`make`/`print`/`quit`; classes `application` (`name`, `frontmost`, `version`) and `window` | None: the app ships no CLI and no URL scheme in its dictionary | Documented from the dictionary; runtime unverified (app not running); no TCC row yet for the terminal toward `com.criminalbird.typeface.beta` (only `com.openai.codex` has one), so the first send prompts |

`tell application id "<bundle id>"` launches the target when a statement needs a reply ("AppleScript will launch the application if it is not already running", ASLG tell statements), so the process assertion stays before every call as the plan states. Reading `running of application id "…"` does not launch (same source).

## [02]-[NODE_BOUNDARY]

| [INDEX] | [OPTION] | [WHAT_IT_IS] | [ARGUMENTS] | [TIMEOUT] | [STDERR] | [ERROR_TYPE] | [VERDICT] |
| :-----: | :------- | :----------- | :---------- | :-------- | :------- | :----------- | :-------- |
| [01] | `run-applescript` 7.1.0 (npm, 2025-09-09; the lock holds 7.0.0 as a transitive dependency of `bundle-name@4.1.0`, `pnpm-lock.yaml` lines 10958, 19560, 22355) | 39 lines: `promisify(execFile)('osascript', ['-e', script, outputArguments], {signal})` and a sync twin | `-e` only; the async form passes `outputArguments` unspread, so `String([])` becomes an extra empty argument (`osascript -e <script> ''`, verified with `execFile('/bin/echo', ['a', []])` printing `a `) and `String(['-ss'])` works by coincidence | none in the async form (an `AbortSignal` only); 500 ms hard-coded in the sync form | discarded in the sync form (`stdio: ['ignore', 'pipe', 'ignore']`); in the async form present only on the rejected Node error | Node `ExecException`, message text | Not in the catalog. It is a wrapper around `execFile` that adds no fact (no stdin, no timeout, no exit code, no stderr on success); CLAUDE.md `[DIRECTNESS]` |
| [02] | `execa` 10.0.1 (npm, 2026-07-31; 9.6.1 in the lock) | Promise child-process library: `timeout` sends `killSignal` (`SIGTERM` default) then `SIGKILL` after `forceKillAfterDelay` 5 s, `error.timedOut`/`isTerminated`/`exitCode`/`stderr`, `killDescendants` (docs/termination.md, docs/errors.md, main branch) | argv array, no shell | yes, by killing | captured | `ExecaError` (a thrown class) | Not adopted: a second result type beside Effect; every field it classifies is one line over `Command.start` |
| [03] | Node `child_process.spawn`/`execFile` (Node 26.8.2 docs) | `timeout` (`execFile` default `0`, `spawn` default `undefined`) sends `killSignal` `SIGTERM`; `maxBuffer` `1024 * 1024` on `execFile` terminates the child when exceeded; `signal` aborts with `AbortError`; promisified `execFile` rejects on non-zero exit with `stdout`/`stderr` on the error | argv array | yes, by killing | captured (buffered on `execFile`, streamed on `spawn`) | `Error` with `code`, `killed`, `signal` | Not used directly: `@effect/platform-node` already owns this call (its executor calls `ChildProcess.spawn` with `detached: true` on POSIX) |
| [04] | `@effect/platform` 0.97.1 `Command` + `@effect/platform-node` 0.108.1 `NodeCommandExecutor.layer` (both in the catalog) | `Command.make(command, ...args)`, `Command.feed(string)` for stdin, `Command.start` → `Process` in a `Scope` with `exitCode`, `stdout`, `stderr` streams, `kill(signal)`, `pid`; `Command.string`/`lines`/`stream`/`streamLines`/`exitCode` for one-shot forms; `Command.workingDirectory`, `env`, `runInShell` | argv array (no shell unless `runInShell`) | `Effect.timeout(duration)` fails with `Cause.TimeoutException` and interrupts the fiber; closing the scope sends `SIGTERM` to the process group (`process.kill(-pid)`), verified: a `sleep 30` under `Effect.timeout('1 second')` failed after 1011 ms and no `sleep 30` process remained | `process.stderr` stream, decoded with `Stream.decodeText` | `PlatformError` = `BadArgument | SystemError` (`Schema.TaggedError` classes with `module: 'Command'`, `method`, `reason` ∈ `NotFound | PermissionDenied | TimedOut | …`); a non-zero exit is not an error, it is the `ExitCode` value; a signal death fails `exitCode` with `SystemError` "Process interrupted due to receipt of signal" | The owning API. One result type, no wrapper |

Call form for one job (Illustrator shape; Photoshop and InDesign differ only in the AppleScript text). Pure classification from `{exitCode, stderr}` to a tagged error, function-hooks style, exports at the end. `run` is the boundary; nothing above it sees a string.

```ts
// --- [IMPORTS] -------------------------------------------------------------------------

import { Command } from '@effect/platform';
import type { CommandExecutor } from '@effect/platform/CommandExecutor';
import type { PlatformError } from '@effect/platform/Error';
import { type Cause, Data, Duration, Effect, Fiber, Option, Stream } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Reply {
    readonly exitCode: number;
    readonly stdout: string;
    readonly stderr: string;
}

interface HostFields {
    readonly host: string;
}

interface CodeFields extends HostFields {
    readonly code: number;
}

interface ScriptNotCompiledFields extends CodeFields {
    readonly line: string;
}

interface HostRejectedFields extends CodeFields {
    readonly reason: string;
}

interface DeadlineExceededFields extends HostFields {
    readonly fiber: Fiber.RuntimeFiber<Reply, BridgeError | PlatformError>;
}

interface AutomationDenied extends Cause.YieldableError, HostFields {
    readonly _tag: 'AutomationDenied';
}

interface HostUnresponsive extends Cause.YieldableError, CodeFields {
    readonly _tag: 'HostUnresponsive';
}

interface HostNotRunning extends Cause.YieldableError, HostFields {
    readonly _tag: 'HostNotRunning';
}

interface ScriptNotCompiled extends Cause.YieldableError, ScriptNotCompiledFields {
    readonly _tag: 'ScriptNotCompiled';
}

interface HostRejected extends Cause.YieldableError, HostRejectedFields {
    readonly _tag: 'HostRejected';
}

interface DeadlineExceeded extends Cause.YieldableError, DeadlineExceededFields {
    readonly _tag: 'DeadlineExceeded';
}

type BridgeError = AutomationDenied | HostUnresponsive | HostNotRunning | ScriptNotCompiled | HostRejected;

// --- [ERRORS] --------------------------------------------------------------------------

// An exported `class extends Data.TaggedError(...)` is refused by `isolatedDeclarations` (TS9021); a field named `message` collides with `YieldableError.message` (TS2320)
const AutomationDenied: new (fields: HostFields) => AutomationDenied = Data.TaggedError('AutomationDenied')<HostFields>;
const HostUnresponsive: new (fields: CodeFields) => HostUnresponsive = Data.TaggedError('HostUnresponsive')<CodeFields>;
const HostNotRunning: new (fields: HostFields) => HostNotRunning = Data.TaggedError('HostNotRunning')<HostFields>;
const ScriptNotCompiled: new (fields: ScriptNotCompiledFields) => ScriptNotCompiled = Data.TaggedError('ScriptNotCompiled')<ScriptNotCompiledFields>;
const HostRejected: new (fields: HostRejectedFields) => HostRejected = Data.TaggedError('HostRejected')<HostRejectedFields>;
const DeadlineExceeded: new (fields: DeadlineExceededFields) => DeadlineExceeded = Data.TaggedError('DeadlineExceeded')<DeadlineExceededFields>;

// --- [CONSTANTS] -----------------------------------------------------------------------

// osascript prints `<line>:<column>: <syntax|execution> error: <message> (<code>)` on stderr and exits 1
const _LINE = /^(?<line>\d+:\d+): (?<phase>syntax|execution) error: (?<message>.*) \((?<code>-?\d+)\)$/mu;

// --- [SCRIPT] --------------------------------------------------------------------------

const _quoted = (value: string): string => `"${value.replace(/\\/gu, '\\\\').replace(/"/gu, '\\"')}"`;

// The file value is bound outside the tell block: inside it AppleScript compiles `POSIX file` as an
// object specifier of the target (Photoshop answered -1728 "Can't get POSIX file … of «script»")
const doJavascript = (bundleId: string, script: string, args: readonly string[], seconds: number): string => `set f to POSIX file ${_quoted(script)}
with timeout of ${seconds} seconds
tell application id ${_quoted(bundleId)}
do javascript f with arguments {${args.map(_quoted).join(', ')}} show debugger never
end tell
end timeout`;

// --- [CLASSIFICATION] ------------------------------------------------------------------

const _byCode = (host: string, code: number, reason: string): BridgeError => {
    if (code === -1743) {
        return new AutomationDenied({ host });
    }
    if (code === -1712) {
        return new HostUnresponsive({ host, code });
    }
    return code === -600 ? new HostNotRunning({ host }) : new HostRejected({ host, code, reason });
};

// `no-nullable-return` refuses `BridgeError | undefined`; absence is `Option.none()`
const classify = (host: string, reply: Reply): Option.Option<BridgeError> => {
    if (reply.exitCode === 0) {
        return Option.none();
    }
    const found = _LINE.exec(reply.stderr)?.groups;
    const code = Number(found?.['code'] ?? '0');
    return Option.some(
        found?.['phase'] === 'syntax'
            ? new ScriptNotCompiled({ host, code, line: found['line'] ?? '' })
            : _byCode(host, code, found?.['message'] ?? reply.stderr),
    );
};

// --- [RUN] -----------------------------------------------------------------------------

const _text = (stream: Stream.Stream<Uint8Array, PlatformError>): Effect.Effect<string, PlatformError> =>
    stream.pipe(Stream.decodeText(), Stream.runFold('', (a, b) => a + b));

const _reply = (script: string): Effect.Effect<Reply, PlatformError, CommandExecutor> =>
    Effect.scoped(
        Effect.gen(function* () {
            const process = yield* Command.start(Command.make('osascript', '-').pipe(Command.feed(script)));
            const [exitCode, stdout, stderr] = yield* Effect.all([process.exitCode, _text(process.stdout), _text(process.stderr)], { concurrency: 3 });
            return { exitCode, stdout, stderr };
        }),
    );

const _classified = (host: string, script: string): Effect.Effect<Reply, BridgeError | PlatformError, CommandExecutor> =>
    _reply(script).pipe(
        Effect.flatMap((reply) => Option.match(classify(host, reply), { onNone: () => Effect.succeed(reply), onSome: (error) => Effect.fail(error) })),
    );

// The deadline decides the reply alone. The daemon fiber keeps waiting on osascript, whose Apple Event
// keeps running inside the host after -1712 (InDesign finished a 2.5 s sleep issued under a 1 s timeout
// and answered the next call); `Fiber.join` interrupted by the timeout does not interrupt the fiber.
const run = (
    host: string,
    script: string,
    deadline: Duration.Duration,
): Effect.Effect<Reply, BridgeError | PlatformError | DeadlineExceeded, CommandExecutor> =>
    Effect.forkDaemon(_classified(host, script)).pipe(
        Effect.flatMap((fiber) => Fiber.join(fiber).pipe(Effect.timeoutFail({ duration: deadline, onTimeout: () => new DeadlineExceeded({ host, fiber }) }))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { BridgeError, Reply };
export { AutomationDenied, classify, DeadlineExceeded, doJavascript, HostNotRunning, HostRejected, HostUnresponsive, run, ScriptNotCompiled };
```

Facts the form rests on: `osascript -` reads the script from stdin and passes later arguments to `on run argv` (osascript(1), verified with `printf 'on run argv…' | osascript - hello` → `got hello`); `Command.feed` writes stdin (Effect docs "Feeding Input to a Command"); `Command.start` + `Effect.all` with `concurrency: 3` over `exitCode`, `stdout`, `stderr` is the documented process-detail form; `Effect.timeoutFail` is the typed variant of `Effect.timeout` (effect 3.22.1 `Effect.d.ts`); `Data.TaggedError` yields a `YieldableError` with `_tag` (effect 3.22.1 `Data.d.ts` line 610). The error form is the prover's (`rasm-integration.md` [03]-15, [09]-11, [09]-13): a fields interface, an interface extending `Cause.YieldableError`, and a typed constructor, because an exported `class extends Data.TaggedError(...)` is `TS9021` under the base `isolatedDeclarations` and a `message` field is `TS2320`; `classify` returns `Option` because the repo's `no-nullable-return` rule refuses `| undefined` (`rasm-integration.md` [09]-07). The same file, widened to the eleven variants of `architecture.md` [03], typechecks in the copy (`rasm-integration.md` [09] row 13). The layer is `NodeCommandExecutor.layer` (`Layer<CommandExecutor, never, FileSystem>`), provided through `NodeContext.layer`. Non-zero exit is a value, so classification is a total function over `Reply`; `PlatformError` stays what it is (spawn failure, signal death) and is mapped at the MCP boundary like every other host error. Catalog change: none (`effect`, `@effect/platform`, `@effect/platform-node` are rows already); `run-applescript` stays out of the catalog; the plan's `run-applescript 7.1.0` row is withdrawn.

## [03]-[SCRIPT_FORM]

| [INDEX] | [FORM] | [QUOTING_AND_TRANSPORT] | [TERMINOLOGY] | [TIMEOUT] | [VERDICT] |
| :-----: | :----- | :---------------------- | :------------ | :-------- | :-------- |
| [01] | AppleScript text as a TypeScript template string, sent on stdin (`osascript -`) | The only interpolated values are AppleScript string literals: `\` → `\\`, `"` → `\"`; no shell is involved (argv array, stdin body), so `quoted form of` never appears; a path is a string literal coerced by `POSIX file` outside the `tell`; `with arguments {…}` is a list literal of such strings and reaches ExtendScript as `arguments[i]` strings (InDesign: `args=2 first=alpha`; Photoshop: `args=2`); `-e` would put the body in argv (fine for `execFile`, wrong under a shell, capped by `ARG_MAX`); a file form means writing the script to disk first for nothing | Compiled against the target's dictionary at compile time ("Statements within a tell statement that use terminology from the targeted object are compiled against that object's dictionary", ASLG) | `with timeout of N seconds` per block; default two minutes ("AppleScript waits for two minutes before reporting an error"); on expiry "AppleScript does not cancel the operation—it merely stops execution of the script" (ASLG with timeout) | Chosen for every use |
| [02] | `.applescript`/`.scpt` files | `osascript <file> args…`; a compiled `.scpt` fixes terminology at compile time | Same as [01] | Same | Rejected: a source file with no linter (README: AppleScript has no tree-sitter grammar in the catalog) holding the same three lines the template holds |
| [03] | JXA (`osascript -l JavaScript`) | `Application('com.adobe.InDesign').doScript(Path('/…/x.jsx'), {language: 'javascript', withArguments: ['alpha', 'beta']})` works (verified, `21.6.0.58 args=2 first=alpha`) | Resolved at send time: an unknown verb is a runtime `-1708 Message not understood` (verified with `Application('Finder').doJavascript('1')`), an unknown application is `-2700 Application can't be found` (verified) | No `with timeout` equivalent found: assigning `se.timeout = 1` failed `-1700 Can't convert types`; the per-send timeout stays at the default; unverified whether any JXA property sets it | Rejected: the dictionary parameter labels (`with arguments`, `show debugger`, `language`, `undo mode`) are AppleScript terms, every Adobe example is AppleScript, and the timeout is not settable |

Error numbers the classifier reads, with Apple's meaning:

| [INDEX] | [CODE] | [NAME] | [MEANING] | [WHERE_SEEN] |
| :-----: | :----- | :----- | :-------- | :----------- |
| [01] | -1712 | `errAETimeout` | "The Apple event has timed out" (ASLG table B-3; `MacErrors.h` line 687) | Verified: `with timeout of 1 second` around `do script "$.sleep(2500); 'done'"` → `execution error: <indesign.processName> got an error: AppleEvent timed out. (-1712)`, exit 1; the host finished the script |
| [02] | -1708 | `errAEEventNotHandled` | "The script doesn't understand the message. The event was not handled" (B-3; `MacErrors.h` line 683, alias `OSAMessageNotUnderstood`) | Verified: `«event miscNOPE» "x"` to Acrobat → `"x" doesn't understand the "«event miscNOPE»" message. (-1708)` |
| [03] | -1743 | `errAEEventNotPermitted` | "Mac OS X 10.8 and later, the target of the AppleEvent does not allow this sender to execute this event" (`AppleEvents.h` line 121); returned when the user declined consent (`AEDeterminePermissionToAutomateTarget` comment) | Not reproducible without revoking a TCC grant; stderr text unverified |
| [04] | -1744 | `errAEEventWouldRequireUserConsent` | Permission undetermined and the sender asked not to prompt (`AppleEvents.h`) | Only from `AEDeterminePermissionToAutomateTarget(…, askUserIfNeeded = false)` |
| [05] | -600 | `procNotFound` | "Application isn't running" (B-2); "no eligible process with specified descriptor" (`MacErrors.h` line 473) | Not reproduced (reaching it means addressing a non-running app, which launches it) |
| [06] | -1728 | `errAENoSuchObject` | "The referenced object doesn't exist" (B-3); alias `errOSACantAccess` | Verified three ways: a bad bundle id at compile time (`syntax error: Can't get application id "com.adobe.doesnotexist". (-1728)`), a missing process (`System Events got an error: Can't get process "NoSuchProcess". (-1728)`), Photoshop refusing a file specifier |
| [07] | -2740 | AppleScript syntax error | "A <identifier> can't go after this <identifier>" (B-1) | Verified twice as a compile error: `tell application "Finder" to do javascript "1"` and `tell application id "com.adobe.Photoshop" to do javascript foo bar` → `syntax error: A identifier can't go after this identifier. (-2740)`. Mechanism: the term is absent from the dictionary the compiler fetched. The plan's "unresolvable while the app is busy" variant (the dictionary is fetched from the running process and an unanswered fetch leaves the compile with no terms) is consistent with the ASLG mechanism and unverified here. The plan's claim that Illustrator's bare `do javascript "…"` is -2740: unverified, and the same dictionary entry on Photoshop accepted `do javascript "app.version"` unparenthesized; Illustrator's sdef defines no term named `javascript`, so the plan's stated cause is not in the dictionary |
| [08] | -2741 | AppleScript syntax error | "Expected <x> but found <y>" (B-1) | Not exercised |
| [09] | -1700 | `errAECoercionFail` | Coercion failure (B-3; `MacErrors.h` line 675) | Verified in JXA `se.timeout = 1` |
| [10] | 8800 | Photoshop internal | "General Photoshop error occurred. This functionality may not be available in this version of Photoshop." | Verified: `do javascript ((POSIX file "…") as alias)`; cause unsettled |

Photoshop file form, all three attempts on 27.11.0 with the `.jsx` `<Sources>/design/probe-applescript/version.jsx`: `(POSIX file "…")` inside the tell → -1728 "Can't get POSIX file … of «script»"; `set f to POSIX file "…"` outside the tell → -1728 "Can't get file "Macintosh HD:…"" (the HFS coercion happened, Photoshop could not resolve it); `as alias` → 8800. InDesign accepted the second form. Whether the cause is the file's location, a Photoshop (Beta) file-access restriction, or the `.jsx` extension is unsettled; the only Photoshop AppleScript use (`app.systemInformation`) needs no file, so nothing in the design depends on it. Illustrator's file form (plan experiment R-C3) stays open.

Terminology: `syntax error:` lines come from compilation, before any event is sent; `execution error:` lines come from a reply. The classifier reads the phase, then the code. A `syntax error` in a template is a defect in the template (`ScriptNotCompiled`), never a host state.

## [04]-[FILES_AND_LINTING]

| [INDEX] | [FACT] | [CONSEQUENCE] |
| :-----: | :----- | :------------ |
| [01] | No `.applescript`, `.scpt`, or `.scptd` file exists in the repo; every script is a template string in a `.ts` file under `apps/creative-cloud/host/` | ast-grep: no grammar row, no `customLanguages` block, no `languageGlobs` change in `sgconfig.yml`; Biome: nothing to see; the TypeScript rules already lint the template's host file |
| [02] | JXA is not used, so no `.js` is generated for `osascript -l JavaScript` | Nothing under `.artifacts/` for this channel; the plan's `inspection` JXA bundle (`tsconfig.jxa.json`, `jxa.d.ts`) is withdrawn with it, xa11y is a Node dependency of `@rasm/inspection` |
| [03] | `.artifacts/` is ignored (`.gitignore` line 66, `**/.artifacts/`), where the Illustrator `.jsx` builds land | Never linted, ignored by the tree; the ExtendScript sources are `.ts` and are linted |
| [04] | The `ax/lib/*.applescript` files sit under `<Sources>/ax/lib/`, a path `.gitignore` line 150 (`docs/research`) ignores | No repo consequence |

## [05]-[PERMISSIONS]

| [INDEX] | [FACT] | [EVIDENCE] |
| :-----: | :----- | :--------- |
| [01] | TCC attributes an Apple Event to the responsible process, the app at the root of the launch chain, and children inherit its grants | Qt blog "The Curious Case of the Responsible Process" (2022-02-04): "permissions are inherited by child processes… TCC decides that iTerm2 is responsible for MyApp"; `responsibility_spawnattrs_setdisclaim` is the undocumented opt-out (same post; steipete 2025-07-03). This session: `osascript` ran under `bash` (44056) ← `claude` 2.1.269 (72406) ← `zsh` (12239) ← `zellij` 0.45.0 server (32825, reparented to launchd, started from a WezTerm pane); the only Automation rows that match the four hosts belong to `com.github.wez.wezterm`, so responsibility survived the zellij daemonization (`launchctl procinfo` printed nothing for the pid, so the responsible pid itself is inferred from the rows), and `tccd` logged `Handling access request: kTCCServiceAppleEvents:<private>:com.adobe.InDesign, default_allow: 0, authValue: 2` for each probe with no prompt |
| [02] | The user TCC database is readable without privileges (`~/Library/Application Support/com.apple.TCC/TCC.db`, mode 644) and holds the Automation rows | Query at 2026-09-11 21:1x: `kTCCServiceAppleEvents` client `com.github.wez.wezterm` → `com.adobe.Acrobat.Pro`, `com.adobe.InDesign`, `com.adobe.Photoshop`, `com.adobe.illustratorBeta`, `com.apple.finder`, `com.apple.systemevents`, all `auth_value 2`; no row for `com.github.wez.wezterm` → `com.criminalbird.typeface.beta`; no row for any `node` or `claude` client. The system database (`/Library/Application Support/com.apple.TCC/TCC.db`, mode 644) holds Accessibility: `com.github.wez.wezterm 2`, `com.openai.codex 2`, `com.anthropic.claudefordesktop 0` (denied) |
| [03] | WezTerm carries `com.apple.security.automation.apple-events` and `NSAppleEventsUsageDescription` = "An application launched via WezTerm would like to access AppleScript." | `codesign -d --entitlements -` and `PlistBuddy` on `/Applications/WezTerm.app` |
| [04] | Claude Code 2.1.269 carries `com.apple.security.automation.apple-events` (issue anthropics/claude-code#52712, opened 2026-04-24 against 2.1.119, is fixed in the installed build) and no embedded `Info.plist` section | `codesign -d --entitlements -` on `~/.local/share/claude/versions/2.1.269`; `otool -s __TEXT __info_plist` printed no section |
| [05] | mise's node 26.8.1 is signed with hardened runtime (`flags=0x10000(runtime)`, TeamIdentifier `HX7739G8FX`), carries no `apple-events` entitlement and no `Info.plist` | `codesign -dv --entitlements -` on `~/.local/share/mise/installs/node/26.8.1/bin/node` |
| [06] | First send from a new binary under the terminal: no prompt, the terminal's row answers (row [01]); the binary's own signature is irrelevant while it is a child of WezTerm | Probes [06] rows [04]–[06], [10]–[12] ran with exit 0 and no dialog |
| [07] | Under Claude Desktop (`com.anthropic.claudefordesktop`) the responsible process is Claude Desktop; it has no Automation rows here; if its `Info.plist` lacks `NSAppleEventsUsageDescription`, tccd denies without a dialog; a hardened-runtime responsible process without the entitlement is refused before prompting (`tccd: Prompting policy for hardened runtime; service: … requires entitlement … but it is missing for responsible={identifier=com.anthropic.claude-code…}`, quoted in #52712 for EventKit) | Documented; not exercised on this machine |
| [08] | Under a launchd agent node is the responsible process; with row [05]'s signature the send fails `-1743` with no prompt | Inference from rows [01], [05], [07]; unverified |
| [09] | The plan proves the grant before execution with one read-only Apple Event: `get_health` sends `tell application id "<bundle>" to get version` (verified: `26.002.21901`, `27.11.0`, `21.6.0.58`, exit 0) and classifies `-1743` → `AutomationDenied`; the TCC row query of row [02] is the README diagnostic beside it | `AEDeterminePermissionToAutomateTarget(target, typeWildCard, typeWildCard, askUserIfNeeded)` (macOS 10.14+, `AppleEvents.h`) is the API that answers `noErr`/`-1743`/`-1744`/`-600` without sending a real event, but only a native caller reaches it; not used |
| [10] | Re-prompt: `tccutil reset AppleEvents <bundle id>` "causing apps to prompt again the next time they access the service" | `tccutil(1)` |
| [11] | `sdef` and `sdp` refuse a Command Line Tools-only developer directory ("tool 'sdef' requires Xcode"); `OSACopyScriptingDefinitionFromURL` from Carbon reads the same dictionary without Xcode (InDesign: 3,633,545 bytes, err 0) | Probes [06] rows [02]–[03] |

## [06]-[PROBES]

| [INDEX] | [COMMAND] | [OUTPUT] | [VERDICT] |
| :-----: | :-------- | :------- | :-------- |
| [01] | `osascript -e 'with timeout of 60 seconds … tell application "System Events" to get name of every process whose background only is false'` | `Finder, Messages, Superhuman, Code, MSTeams, Adobe InDesign 2026 (Beta), Adobe Photoshop 2026, Creative Cloud, AdobeAcrobat, Arc, wezterm-gui`, exit 0 | System Events reachable; Illustrator and Typeface not running |
| [02] | `sdef "/Applications/Adobe Acrobat DC/Adobe Acrobat.app"` (and the other five) | `xcode-select: error: tool 'sdef' requires Xcode, but active developer directory '/Library/Developer/CommandLineTools' is a command line tools instance`, exit 1 | `sdef` unusable here |
| [03] | `Contents/Resources/Adobe Illustrator.sdef` (301,978 bytes), `Acrobat.sdef` (41,892), `Photoshop.sdef` (187,664), `SystemEvents.sdef` (111,857), `Scriptable.sdef` (Typeface, 9,586) copied; InDesign through `OSACopyScriptingDefinitionFromURL` via Python `ctypes` (`err 0`, 3,633,545 bytes; title "Adobe InDesign 21.5 AppleScript Dictionary") | Six dictionaries at the sizes listed | Dictionaries on record; commands in [01] |
| [04] | `tell application id "com.adobe.Acrobat.Pro" to get version` / Photoshop / InDesign | `26.002.21901` / `27.11.0` / `21.6.0.58`, exit 0 | The health read; no TCC prompt |
| [05] | `tell application id "com.adobe.doesnotexist" to get version` (plain and through Effect `Command`) | `32:71: syntax error: Can't get application id "com.adobe.doesnotexist". (-1728)`, exit 1; Effect reply `{"exitCode":1,"stdout":"","stderr":"…(-1728)"}` | A bad bundle id is a compile-time `syntax error`, classified `ScriptNotCompiled`, never a host state; the bundle ids are `as const`, so it cannot occur |
| [06] | `osascript -l JavaScript -e 'Application("System Events").processes.length'` | `149`, exit 0 | JXA runs; it counts background processes too |
| [07] | `tell application id "com.adobe.InDesign" to do script "app.version" language javascript`; Photoshop `do javascript "app.version"`; Acrobat `do script "app.viewerVersion"` | `21.6.0.58` / `27.11.0` / `26.00221901`, exit 0 | The three script channels answer a read |
| [08] | Acrobat `do script "(function(){ return JSON.stringify({v: app.viewerVersion, n: app.activeDocs.length}); })()"` | `{"v":26.00221901,"n":1}` | JSON round trip as the plan's Acrobat contract |
| [09] | Acrobat `«event miscNOPE» "x"` | `execution error: Adobe Acrobat got an error: "x" doesn't understand the "«event miscNOPE»" message. (-1708)`, exit 1 | -1708 shape |
| [10] | Photoshop `do javascript foo bar`; Finder `do javascript "1"` | `syntax error: A identifier can't go after this identifier. (-2740)`, exit 1, both | -2740 is a compile error for an absent term |
| [11] | System Events `tell process "NoSuchProcess" to get name of menu bar 1`; `tell process "Adobe Illustrator" …` | `System Events got an error: Can't get process "…". (-1728)`, exit 1 | A missing process is -1728 from System Events, not -600 |
| [12] | InDesign `do script f language javascript with arguments {"alpha", "beta"}` (`f` set outside the tell from `POSIX file`); string form; JXA `doScript(Path(…), {language: 'javascript', withArguments: […]})` | `21.6.0.58 args=2 first=alpha` / `21.6.0.58 args=2` / `21.6.0.58 args=2 first=alpha`, exit 0 | File and argument delivery proven on InDesign |
| [13] | Photoshop `do javascript (POSIX file "…/version.jsx") with arguments {…} show debugger never` through Effect; `set f to POSIX file` outside; `as alias` | `-1728 Can't get POSIX file "…" of «script»` / `-1728 Can't get file "Macintosh HD:private:tmp:…"` / `8800 General Photoshop error occurred…`, exit 1 | Photoshop file form unsettled; string form used |
| [14] | Photoshop `do javascript "app.version + ' args=' + arguments.length" with arguments {"alpha", "beta"}` | `27.11.0 args=2`, exit 0 | Arguments reach the string form |
| [15] | `with timeout of 1 second` around InDesign `do script "$.sleep(2500); 'done'" language javascript`, then `get version` | `execution error: <indesign.processName> got an error: AppleEvent timed out. (-1712)`, exit 1; then `21.6.0.58`, exit 0 | -1712 shape; the host keeps running the script and answers afterwards |
| [16] | Effect: `Command.start(Command.make('sleep', '30'))` → `p.exitCode` under `Effect.timeout(Duration.seconds(1))` | `SLEEP pid 47279`, `TIMEOUT after 1011 ms exit TimeoutException: Operation timed out after '1s'`, node exit 0; `ps` shows no `sleep 30` | The scope's release kills the process group on interruption |
| [17] | `printf 'on run argv\nreturn "got " & item 1 of argv\nend run\n' \| osascript - hello` | `got hello`, exit 0 | stdin body plus argv works |
| [18] | `node -e "execFile('/bin/echo', ['a', []], …)"` | stdout `"a \n"` | An unspread array becomes an empty argument (run-applescript 7.x async form) |
| [19] | `sqlite3 ~/Library/Application\ Support/com.apple.TCC/TCC.db "select … from access where service in (…)"` | 20 `kTCCServiceAppleEvents` rows (see [05] row [02]), exit 0 | Readable without privileges |
| [20] | `log show --last 20m --predicate 'process == "tccd"' --info \| rg kTCCServiceAppleEvents` | `tccd: [com.apple.TCC:access] Handling access request: kTCCServiceAppleEvents:<private>:com.adobe.InDesign, default_allow: 0, authValue: 2,` (and Photoshop, Acrobat.Pro) | Every probe was answered from a stored grant |
| [21] | JXA `se.timeout = 1` | `execution error: Error: Error: Can't convert types. (-1700)` | No per-send timeout found in JXA |
| [22] | JXA `Application("com.adobe.doesnotexist").version()`; `Application("Finder").doJavascript("1")` | `-2700 Application can't be found` / `-1708 Message not understood`, exit 1 | JXA defers terminology to send time |

## [07]-[DECISION_TABLE]

| [INDEX] | [USE] | [CHANNEL] | [PACKAGE] | [CALL_FORM] | [ERROR_TYPE] | [STATUS] |
| :-----: | :---- | :-------- | :-------- | :---------- | :----------- | :------- |
| [01] | Illustrator ExtendScript job | Apple Event `do javascript` (`miscDjxM`) | `@effect/platform` `Command` + `@effect/platform-node` | `set f to POSIX file "<jsx>"` / `with timeout of N seconds` / `tell application id "com.adobe.illustratorBeta"` / `do javascript f with arguments {"<request>", "<response>"} show debugger never`, body on stdin of `osascript -` | `BridgeError` from `classify`; `DeadlineExceeded` on the reply | Dictionary documented; runtime unverified (app not running); file form open (R-C3) |
| [02] | Acrobat JavaScript | Apple Event `do script` (`miscdosc`) text | same | `tell application id "com.adobe.Acrobat.Pro" to do script "<js>"`, JS wrapped in the IIFE returning `JSON.stringify`, body on stdin | same | Proven |
| [03] | InDesign ExtendScript until the UXP plugin | Apple Event `do script` (`K2  dosc`) | same | `set f to POSIX file "<jsx>"` / `tell application id "com.adobe.InDesign" to do script f language javascript with arguments {…} undo mode entire script undo name "<job>"` | same | Proven (file, arguments); `undo mode` documented in the dictionary, unverified |
| [04] | Photoshop `app.systemInformation` | Apple Event `do javascript` (`miscDjxM`) string | same | `tell application id "com.adobe.Photoshop" to do javascript "app.systemInformation"` | same | Proven for the string form with arguments |
| [05] | AX reads (menus, windows, dialog controls) | In-process AX through xa11y | `@crowecawcaw/xa11y` 0.14.0 (catalog row, `# Adobe hosts`) | `App.byPid(pid)` → `app.tree(depth)` / `app.locator(selector).elements()` | xa11y `PermissionDeniedError` / `SelectorNotMatchedError` mapped to `BridgeError` at the boundary | Documented; unverified in repo |
| [06] | AX clicks (menu items, dialog buttons, list rows) | In-process AX through xa11y | same | `app.locator('menu_item[name="…"]').press()`, `locator(...).select()` | same | Documented; unverified in repo |
| [07] | Drover dialog dismissal (`⌘.`) | Apple Event System Events `keystroke` (`prcskprs`) | `@effect/platform` `Command` | `tell application "System Events" to keystroke "." using command down` after `set frontmost of process "<name>" to true` | `BridgeError` | Proven by `<Sources>/ax/lib/dialog-close.applescript` |
| [08] | Typeface font activation | Apple Event `activatefonts` (`tfsuActi`) | `@effect/platform` `Command` | `tell application id "com.criminalbird.typeface.beta" to activatefonts {"<PostScript name>", …} requested by "Rasm"` | `BridgeError` | Dictionary documented; runtime unverified; first send prompts (no TCC row) |
| [09] | Health read per osascript host | Apple Event `get version` | `@effect/platform` `Command` | `tell application id "<bundle>" to get version` | `AutomationDenied` on -1743 | Proven on three hosts |
| [10] | Script form and transport for every Apple Event use | AppleScript template string in TypeScript, stdin | none beyond [01] | `Command.make('osascript', '-').pipe(Command.feed(script))` | as above | Proven |

Withdrawn from the plan by this table: the `run-applescript 7.1.0` catalog row, the JXA inspection bundle, the Swift `ax-dump` binary as a target, the `-2740` → `host-unresponsive` mapping (a `syntax error` line is `ScriptNotCompiled`; the busy-app variant is unverified), the `-1708` → Illustrator "not scriptable" reading (Acrobat answers -1708 for an unknown event; -2704 is "isn't scriptable"), and the "child is never killed" wording (the daemon fiber keeps the child; the deadline interrupts the join alone).

## [08]-[SOURCES]

| [INDEX] | [SOURCE] | [DATE] | [USED_FOR] |
| :-----: | :------- | :----- | :--------- |
| [01] | `osascript(1)`, `osalang(1)`, `osacompile(1)`, `tccutil(1)` man pages on this machine (`/usr/bin/man`) | osascript page dated 2014-04-24, tccutil 2012-04-03 | `-e`/file/stdin, `-` with arguments, `-s` flags, `tccutil reset` |
| [02] | Apple, AppleScript Language Guide, "Error Numbers and Error Messages" (developer.apple.com/library/archive/…/ASLR_error_codes.html), fetched 2026-09-11 | archive | Tables B-1 to B-4 |
| [03] | Apple, AppleScript Language Guide, "Control Statements Reference" (`tell`, `with timeout`, `considering`/`ignoring application responses`), fetched 2026-09-11 | archive | Terminology at compile time, launch on reply, two-minute default, "does not cancel the operation" |
| [04] | `MacErrors.h`, `AEDataModel.h`, `AppleEvents.h` in the Command Line Tools SDK (`/Library/Developer/CommandLineTools/SDKs/MacOSX.sdk`) | SDK for macOS 26 | Error constants, `AEDeterminePermissionToAutomateTarget`, `-1743`/`-1744`, send-mode flags |
| [05] | App dictionaries on disk (each bundle's `Contents/Resources/*.sdef`; InDesign through `OSACopyScriptingDefinitionFromURL`, [06] row 03), read 2026-09-11 | Illustrator 30.9.0, Acrobat 26.002.21901, Photoshop 27.11.0, InDesign 21.6.0.58 (dictionary title 21.5), System Events 1.3.6, Typeface-beta 4.5.0 | Commands, codes, parameters, enumerations |
| [06] | Adobe helpx, "Connect Adobe Illustrator (Beta) to AI tools" and "Work with Adobe Illustrator (Beta) documents from AI tools" | last updated 2026-04-16 | MCP setup (`http://localhost:18412/v1/mcp`, bearer key) and tool table |
| [07] | Adobe developer.adobe.com, "UXP General Information" (Photoshop and InDesign UXP reference sites); Adobe developer blog "UXP Changelog and Product Support Matrix" | pages fetched 2026-09-11; blog 2026-07-01 | Host list without Illustrator; UXP Scripting from Photoshop 24.1 and InDesign 18.0 |
| [08] | Adobe developer.adobe.com, Photoshop UXP `app` class reference, `require('uxp').host`, "Localization and Platforms" | fetched 2026-09-11 | No `systemInformation` in UXP |
| [09] | Photoshop ExtendScript reference mirror, `Application.systemInformation` (theiviaxx.github.io/photoshop-docs) | fetched 2026-09-11 | Property meaning |
| [10] | Adobe Acrobat SDK, "Developing for Interapplication Communication", "Using OLE", "Using Apple Events", "Apple Event Objects and Apple Events" (opensource.adobe.com/dc-acrobat-sdk-docs) | fetched 2026-09-11 | OLE is Windows; AppleScript on Mac; object names |
| [11] | Adobe Community, "AppleScript "do script" with a file" | 2024-09-15 | Crash report on the `file` parameter (24.002.21005) |
| [12] | Adobe Community, "Adobe Illustrator 2020 got an error: JavaScript code was missing" | 2020-12-01 | The historical `file "<HFS path>"` bug the plan cites |
| [13] | `run-applescript` 7.0.0 (`node_modules/.pnpm/run-applescript@7.0.0`) and 7.1.0 (`npm pack`, published 2025-09-09), `execa` docs/termination.md and docs/errors.md (main branch, fetched 2026-09-11), npm registry (`execa` 10.0.1 published 2026-07-31) | as listed | [02] rows [01]–[02] |
| [14] | Node.js v26.8.2 `child_process` documentation (nodejs.org/api/child_process.html), fetched 2026-09-11 | current | [02] row [03] |
| [15] | `@effect/platform` 0.97.1 `Command.d.ts`, `CommandExecutor.d.ts`, `Error.d.ts`; `@effect/platform-node-shared` 0.61.1 `internal/commandExecutor.js`; `effect` 3.22.1 `Effect.d.ts`, `Data.d.ts`; Effect docs "Command" (effect.website/docs/platform/command), fetched 2026-09-11 | installed versions | [02] row [04], the call form |
| [16] | Qt blog, "The Curious Case of the Responsible Process" | 2022-02-04 | Responsible-process attribution |
| [17] | Peter Steinberger, "Making AppleScript Work in macOS CLI Tools: The Undocumented Parts" | 2025-07-03 | `Info.plist` section, entitlement, `responsibility_spawnattrs_setdisclaim` |
| [18] | anthropics/claude-code issue #52712 | opened 2026-04-24, closed 2026-06-01 | Hardened-runtime denial log line; the entitlement the installed 2.1.269 now carries |
| [19] | xa11y README (github.com/xa11y/xa11y) and JavaScript API reference (xa11y.dev/api/javascript), npm `@crowecawcaw/xa11y` 0.14.0 | package published 2026-09-09; pages fetched 2026-09-11 | [01] row [05], [07] rows [05]–[06] |
| [20] | `ax-README.md`, `<Sources>/ax/lib/*.applescript`, `<Sources>/ax/lib/ax-dump.swift` | 2026-09-11 | System Events attribute shape, Drover dialogs, `⌘.`, JXA FFI finding |
| [21] | `architecture.md` [03] (the vocabulary), [05] (the transport per host) | current | The driver contract this document supplies the osascript forms for |
| [22] | TCC databases and `tccd` unified log on this machine, `codesign`, `PlistBuddy`, `otool` | 2026-09-11 | [05] |

## [09]-[ARCHIVED_SKILL_VERDICT]

`.archive/.claude/skills/coding-applescript/`, 16,707 words across 17 files, never used. Judged against Apple's documentation and the dictionary facts above, CLAUDE.md, and the channel decisions in [07].

| [INDEX] | [FILE] | [CURRENT_AND_CORRECT] | [CLAUDE.MD] | [RELEVANT_TO_[07]] |
| :-----: | :----- | :-------------------- | :---------- | :----------------- |
| [01] | `SKILL.md` (2,034 words) | Wrong on the one sentence that governs behaviour: "This machine runs with automation privilege already granted… never author a consent preflight, an entitlement check, a privilege fallback, or a denial branch, and never raise the subject in output." [05] shows no grant for Typeface, none for any non-terminal responsible process, and `-1743` as a documented reply; the driver's `AutomationDenied` variant is that denial branch. The UTI table, Automator, Shortcuts, App Intents, Folder Actions, Mail rules, stay-open applets, droplets, Script Menu, and the host dispatch matrix are unverified here and describe surfaces the app never touches. `osascript - args` for stdin scripts: correct (man page; probe [17]) | Description opens with a coined framing ("object-specifier compiler over the Apple Event ABI") and the body holds eleven sections of catalog prose about surfaces no target uses; CLAUDE.md wants a skill to hold the principle that decides a case | Two lines are relevant (stdin transport, `on run argv`); the routing section sends a reader to references that contradict [07] |
| [02] | `references/language.md` (3,018 words) | Language semantics (script objects, `parent`, `continue`, `a reference to`, `considering`/`ignoring`, `text item delimiters`, chevron literals, `error` slots) are consistent with the ASLG pages read; the `with timeout` example is correct in form; the claim that `load script` on a missing POSIX file raises `-1700` and that `store script` faults `-1700` under `use framework` are unverified; the `NSJSONSerialization` bridge example is AppleScriptObjC, a form the app never compiles | The dispatch-table and strategy-factory sections build script-object indirection CLAUDE.md `[DIRECTNESS]` forbids in any language | Not relevant: the driver's templates are four statements and never define a handler |
| [03] | `references/runtime.md` (2,004 words) | `osascript`/`osacompile`/`osadecompile`/`osalang` table matches the man pages (`-x`, `-s`, `-u` confirmed); "A script error yields a non-zero exit whose numeric mapping carries no published contract" matches the man page (silent) and the observed exit 1; the `-s s` versus `-s h` explanation matches the man page; the JXA `whose` and `ObjC.bindFunction` sections are consistent with the JXA finding in `ax-README.md` but omit the `Cocoa` import requirement that finding records; `NSTask` and `do shell script` sections describe shelling out from inside AppleScript, the inverse of this design | The `NSTask` kernel and `Shell` script object are wrappers CLAUDE.md rejects | One paragraph is relevant (invocation and argv); the rest is JXA and shell material [07] does not use |
| [04] | `references/events.md` (1,126 words) | Send-mode table matches `AEDataModel.h` exactly (`kAENoReply 0x01` … `kAEDontExecute 0x2000`, `kAEDoNotAutomaticallyAddAnnotationsToEvent 0x10000` = `NSAppleEventSendDontAnnotate`, `NSAppleEventSendDefaultOptions = WaitForReply \| CanInteract`, all confirmed in the headers); `AEBuildAppleEvent` injection warning is sound; the "Security patterns" and `-1712`/`-1728` recovery examples are correct by the tables; the C and Swift senders are for a native host | The gateway that "refuses to dispatch events to credential stores" is a guard added around a call, which CLAUDE.md names as the defect | Not relevant: the design sends through `osascript`, never builds descriptors |
| [05] | `references/embedding.md` (2,061 words) | "`sdef` and `sdp` … demand a full Xcode developer directory and refuse a Command Line Tools-only selection": verified (probe [02]); "`OSACopyScriptingDefinitionFromURL` … is the runtime dictionary path a Command Line Tools-only host reaches": verified (probe [03]); `NSAppleScript` main-thread confinement, `OSAScript` error keys, `NSScriptCommand` suspension: unverified, Cocoa-host material | Native host code for an app that has no native host | Not relevant beyond the two verified sentences, which [05] row [11] now carries |
| [06] | `references/distribution.md` (670 words) | `-1758` `errOSADataFormatObsolete` and `-1756` confirmed in `MacErrors.h`; `AEDebugSends`/`AEDebugReceives` and the `com.apple.appleevents` log subsystem are unverified; Nix and Homebrew packaging of applets is outside the tree | Nothing to judge | Not relevant: no applet is built |
| [07] | `examples/chevron-dispatch.applescript` | Compiles a term path and a chevron path per verb and elects one on `-1708`; the `-1708` reading is correct (probe [09]); the design has fixed dictionaries and needs no election | A fallback path around a call | Not relevant |
| [08] | `examples/cocoa-scripting-server.swift` | Receiver-side `NSScriptObjectSpecifier` resolution in Swift | Native app scaffolding | Not relevant |
| [09] | `examples/objc-ffi-bridge.js` | JXA FFI over `AEGetDescData` and `OSACopyScriptingDefinitionFromURL` with `Ref('void *', null)`; unverified, and `ax-README.md` records that `ObjC.import('ApplicationServices')` fails on `CFTypeRef *` out-parameters while `Cocoa` works, a fact this file does not carry | An FFI table in a scripting language | Not relevant: JXA is rejected in [03] |
| [10] | `examples/osakit-reentrancy.swift` | Suspended Apple Event handler in a Swift host | Native host | Not relevant |
| [11] | `examples/sdef-routing.js` | Routes verbs against the dictionary at runtime before sending; the `Ref('pointer')` claim contradicts `objc-ffi-bridge.js`'s `Ref('void *', null)` for the same out-parameter, unverified either way | Runtime routing where the dictionary is a build-time fact | Not relevant |
| [12] | `templates/osascript-runner.sh` | `perl -e 'alarm shift; exec @ARGV'` as the process timeout: a shell wrapper around `osascript` doing what `Effect.timeout` does in [02] | A wrapper; CLAUDE.md says a wrapper around the owning API is the defect | Contradicts [07] row [10] |
| [13] | `templates/osa-tool.js` | JXA `run(argv)` returning `JSON.stringify`; correct as JXA (probe [06]) | `JSON.parse` in a template the repo bans by rule | Contradicts [03] row [03] |
| [14] | `templates/applescript-library.applescript`, `templates/applet.sh`, `templates/launchd-osa-agent.plist`, `templates/osakit-host.swift` | Script libraries, applets, launchd agents, an OSAKit host: none is a shape [07] builds; the launchd template's `AssociatedBundleIdentifiers`/Team ID remark is unverified; the launchd route also makes node the responsible process ([05] row [08]) | Files beside an owner | Not relevant |
| [15] | `templates/scriptable-app.sdef` | Structure matches the sdef DTD forms seen in the six dictionaries (`suite`, `enumeration`, `class-extension`, `class` with `cocoa key`, `command` with `direct-parameter`, `parameter`, `result`, `access-group`); it authors a dictionary for an app the repo does not ship | Nothing to judge | Not relevant: the design reads dictionaries, never writes one |
| [16] | Coverage of what [07] needs, across all files: `with timeout` semantics, `-2740` as a compile fault, the `POSIX file` outside-the-tell rule, `with arguments` delivery, TCC responsible-process attribution, `-1743`, `AEDeterminePermissionToAutomateTarget` | Absent from every file; the skill's one sentence on permissions forbids handling them | | The knowledge the working agents need is in this document, not in the skill |

Decision: leave it archived. The skill is a catalog of AppleScript-as-a-product surfaces (applets, libraries, launchd, OSAKit, Cocoa scripting, Shortcuts, Automator) written for a machine with a standing grant, and the four things the working agents need from the channel ([07] rows [01]–[04], [07]–[09]: the four-line template, stdin transport, `with timeout`, and error classification including `-1743`) are either absent from it or contradicted by it (`SKILL.md` on denial branches, `osascript-runner.sh` on process timeouts, `osa-tool.js` on JXA and `JSON.parse`). A restore would keep three verified sentences (stdin argv, `sdef` needs Xcode, `OSACopyScriptingDefinitionFromURL` works without it) and this document already carries them.
