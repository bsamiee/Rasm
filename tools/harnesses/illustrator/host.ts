import { matchesGlob } from 'node:path';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, DateTime, Duration, Effect, FileSystem, Number, Option, Path, Record, Result, Schema, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import jsesc from 'jsesc';
import { crashes, HOME, REPORTER, uxp } from '#/adobe';
import { HarnessError, thrown } from '#/error';
import { Engine, fields, HostCase, type Install, PASTEBOARD, type Placed, Reply, Sent, Survey } from '#/illustrator/contract';
import { precondition } from '#/illustrator/precondition';
import { checked, Launched, plist } from '#/instance';
import { copied } from '#/precondition';
import { local, type Rule, type Text } from '#/report';
import { type Batch, type Host as Harness, type Installed, installation, type Live } from '#/session';
import { assembled, EXTENDSCRIPT, engineOf } from '#/source';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENUMERATION = /_enumNameToId\["(?<name>\w+)"\] = (?<members>\{[^}]*\});/gu;
const SETTINGS = /^n(?<folder>.+\/Library\/Preferences\/[^/]+ Settings\/(?<locale>[^/]+))\//u;
const WRITING = /^a[wu]$/u;
const PLUGIN = /^n(?<bundle>.+\/(?<name>[^/]+)\.aip)\/Contents\/MacOS\/[^/]+$/u;
const HOLDER = /^p(?<pid>\d+)$/u;
const PARENT = /^R(?<parent>\d+)$/u;
const HEADER = /^===== \w+ (?<date>\w+ \d+ \d+) ===== (?<time>[\d:]+) \S+ =====$/mu;
const BLOCK = /^(?====== )/mu;
const NEAR = Duration.minutes(1);
const VOLATILE = {
    extensions: ['.ai', '.eps', '.pdf', '.svg'],
    patterns: [
        /<(?<tag>xmpMM:(?:Instance|Document|OriginalDocument)ID|xmp:(?:Create|Modify|Metadata)Date|stEvt:(?:instanceID|when)|stRef:(?:instance|document)ID)>[^<]*<\/\k<tag>>/gu,
        /(?:xmpMM:(?:Instance|Document|OriginalDocument)ID|xmp:(?:Create|Modify|Metadata)Date|stEvt:(?:instanceID|when)|stRef:(?:instance|document)ID)="[^"]*"/gu,
        /\/(?:CreationDate|ModDate)\s*\([^)]*\)/gu,
        /\/ID\s*\[\s*<[0-9A-Fa-f]*>\s*<[0-9A-Fa-f]*>\s*\]/gu,
        /<!-- Generator: [^>]*-->/gu,
        /%%CreationDate:[^\r\n]*/gu,
    ],
} as const;
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);
const BOARD = new URL('pasteboard.jxa', import.meta.url);

// --- [MODELS] --------------------------------------------------------------------------

const RunConfig = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ features: JsonObject })));
const Members = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Finite));
const Settings = Schema.Struct({ folder: Schema.String, locale: Schema.String });
const Bundled = Schema.Struct({ AdobeBuildNumber: Schema.String });
const Named = Schema.Union([Schema.Struct({ type: Schema.Literal('Identifier'), name: Schema.String }), Schema.Struct({ type: Schema.Literal('Literal'), value: Schema.Unknown })]);
const Reached = Schema.Struct({ type: Schema.Literal('MemberExpression'), object: Schema.Struct({ type: Schema.Literal('ThisExpression') }), property: Schema.Unknown });
const pasting = Schema.is(Schema.Literals(PASTEBOARD));
const shadowed = Schema.is(Schema.Literals(['Window', 'alert', 'confirm', 'prompt', 'app', '$', 'BridgeTalk']));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const installed = Effect.gen(function* () {
    const rows = yield* (yield* Automation).installed;
    return yield* Effect.forEach(
        rows.filter((row) => row.scriptable && row.identifier.startsWith('com.adobe.illustrator')),
        (row) => installation(row, rows, [REPORTER]),
    );
});
const launch = Effect.fn('launch')(function* (_: Installed, scratch: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const owned = path.join(scratch, 'owned.svg');
    yield* fs.writeFileString(owned, '<svg xmlns="http://www.w3.org/2000/svg" width="1" height="1"/>');
    return new Launched({ documents: [owned] });
});

// --- [INSTANCE]
const lsof = (args: readonly string[]) => ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.lines(ChildProcess.make('lsof', args)));
const logFolder = (version: string) => `Adobe Illustrator/${version.split('.').slice(0, 2).join('.')}/Adobe Illustrator`;
const journals = (version: string) => `Library/Logs/Adobe/${logFolder(version)}/{AIGeneral.log,AIHangMonitor.log,UXPLogs_*.log}`;
const sessions: Text = (contents, instance) => {
    const near = { minimum: DateTime.subtractDuration(instance.started, NEAR), maximum: DateTime.addDuration(instance.started, NEAR) };
    const ours = (block: string) =>
        Option.exists(
            Option.flatMap(Option.fromNullishOr(HEADER.exec(block)?.groups), ({ date, time }) => local(`${date} ${time}`)),
            (at) => DateTime.between(at, near),
        );
    return HEADER.test(contents) ? contents.split(BLOCK).filter(ours).join('') : contents;
};
const held = Effect.fn('held')(function* (pid: number, { app, version }: Installed) {
    const printed = yield* lsof(['-Fan', '-p', String(pid)]);
    const settings = yield* Effect.fromOption(
        Array.findFirst(printed, (line) => Schema.decodeUnknownOption(Settings)(SETTINGS.exec(line)?.groups)),
        () => HarnessError.unavailable(`process ${pid}`)('holds no settings folder file open'),
    );
    const outside = (line: string) => Option.flatMap(Option.fromNullishOr(PLUGIN.exec(line)?.groups), ({ bundle = '', name = '' }) => Option.liftPredicate(name, () => !bundle.startsWith(`${app}/`)));
    return {
        ...settings,
        logs: Array.dedupe(Array.getSomes(Array.window(printed, 2).map(([access = '', name = '']) => Option.liftPredicate(name.slice(1), (file) => WRITING.test(access) && matchesGlob(file, `${HOME}/${journals(version)}`))))),
        plugins: Array.dedupe(Array.getSomes(printed.map(outside))),
    };
});
const foreign = Effect.fn('foreign')(function* (pid: number, file: string) {
    const printed = yield* lsof(['-FpR', '--', file]);
    const holders = Array.getSomes(
        Array.window(printed, 2).map(([process = '', parent = '']) => Option.zipWith(Option.flatMap(Option.fromNullishOr(HOLDER.exec(process)?.groups?.pid), Number.parse), Option.flatMap(Option.fromNullishOr(PARENT.exec(parent)?.groups?.parent), Number.parse), (holder, above) => ({ holder, above }))),
    );
    return holders.filter(({ holder, above }) => holder !== pid && above !== pid).map(({ holder }) => holder);
});
const owned = (pid: number, logs: readonly string[]) => Effect.filter(logs, (file) => Effect.map(foreign(pid, file), (pids) => pids.length === 0));
const enumerations = Effect.fn('enumerations')(function* (app: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = new TextDecoder('latin1').decode(yield* fs.readFile(path.join(app, 'Contents', 'Frameworks', 'AIUXPExtensionHostAPI.framework', 'AIUXPExtensionHostAPI')));
    const rows = yield* Effect.forEach(Array.fromIterable(text.matchAll(ENUMERATION)), (found) => Effect.map(Schema.decodeEffect(Members)(found.groups?.members ?? '{}'), (members) => [found.groups?.name ?? '', Object.keys(members)] as const));
    return Record.fromEntries(rows);
});
const opened = Effect.fn('opened')(function* (live: Live) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [{ folder, locale, plugins }, { AdobeBuildNumber: build }] = yield* Effect.all([held(live.pid, live.chosen), plist(live.chosen.app, Bundled)]);
    const config = yield* Effect.flatMap(fs.readFileString(path.join(folder, '.runConfig')), Schema.decodeEffect(RunConfig));
    const features = Option.match(Record.get(config, live.chosen.version), { onNone: () => ({}), onSome: (row) => row.features });
    return { build, locale, features, plugins } satisfies typeof Install.Type;
});

// --- [SOURCE]
const engine = engineOf({ [Engine.literal]: { extensions: EXTENDSCRIPT } }, Engine.literal);
const named = (node: unknown) => Option.map(Schema.decodeUnknownOption(Named)(node), (found) => (found.type === 'Identifier' ? found.name : String(found.value)));
const escaping = (_: string, node: unknown) => Option.exists(Schema.decodeUnknownOption(Reached)(node), ({ property }) => Option.exists(named(property), shadowed));
const escaped = (code: string) =>
    Result.match(
        Result.try(() => parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowHashBang: true })),
        {
            onFailure: (error) => Option.some(`source does not parse: ${thrown(error)}`),
            onSuccess: (tree) => Option.map(Option.fromNullishOr(findNodeAt(tree, undefined, undefined, escaping)), ({ node }) => `${code.slice(node.start, node.end)} reaches the host around the case shadows`),
        },
    );
const refusal = (_: typeof Engine.Type, hosted: typeof HostCase.Type, binary: boolean) => {
    const { set = [], add = [], select = [], steps = [] } = hosted.precondition;
    const expressions = [...Object.values(hosted.read), ...set.map(([target]) => target), ...add.flatMap(({ to, args = [] }) => [to, ...args]), ...select, ...HostCase.match(hosted, { Probe: ({ evaluate }) => [evaluate], Call: ({ call }) => [call], Run: () => [] })];
    const statements = [...steps, ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(hosted._tag === 'Probe' ? undefined : hosted.source), ({ code }) => Option.liftPredicate(code, () => !binary)))];
    return Option.firstSomeOf([...expressions.map((expression) => `(${expression.replaceAll('[*]', '[0]')});`), ...statements].map(escaped));
};

// --- [TRANSPORT]
const pasteboard = (mode: 'save' | 'restore', file: string) =>
    Effect.flatMap(
        Effect.flatMap(Path.Path, (path) => path.fromFileUrl(BOARD)),
        (script) => checked('osascript', ['-l', 'JavaScript', script, mode, file]),
    );
const called = Effect.fn('called')(function* (live: Live, sent: typeof Sent.Type) {
    const automation = yield* Automation;
    const encoded = jsesc(yield* Schema.encodeEffect(Sent)(sent), { json: true });
    const { reply } = yield* automation.script({ pid: live.pid, direct: yield* assembled(ENGINE), values: [encoded] });
    return yield* Reply.replied(`${sent._tag} batch`)(reply);
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const surveying = Effect.gen(function* () {
        const [listed, install] = yield* Effect.all([enumerations(live.chosen.app), opened(live)]);
        const header = yield* called(live, Sent.cases.Survey.make({ enumerations: listed }));
        return { ...Struct.omit(header, ['survey']), ...(header.survey === undefined ? {} : { survey: { enumerations: listed, ...header.survey, host: install } }) };
    });
    const proving = Effect.gen(function* () {
        const { folder, logs } = yield* held(live.pid, live.chosen);
        const color = path.join(live.scratch, 'color.csf');
        yield* fs.copyFile(path.join(folder, 'AI Color Settings'), color);
        const kept = yield* owned(live.pid, logs);
        const copying = batch.cases.some(({ precondition: rows }) => rows.sends?.some(pasting) === true);
        const board = path.join(live.scratch, 'pasteboard.json');
        yield* copying ? pasteboard('save', board) : Effect.void;
        const reply = yield* Effect.result(called(live, Sent.cases.Cases.make({ records: batch.records, label: batch.label, colorSettings: color, logs: kept, cases: batch.cases })));
        yield* copying ? pasteboard('restore', board) : Effect.void;
        return Struct.omit(yield* Effect.fromResult(reply), ['survey']);
    });
    return yield* Option.match(batch.survey, { onNone: () => surveying, onSome: () => proving });
});
const quit = Effect.fn('quit')(function* (live: Live) {
    const automation = yield* Automation;
    yield* called(live, Sent.cases.Closed.make({ folders: [live.scratch] }));
    yield* automation.execute({ pid: live.pid, command: 'quit' });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    Engine,
    survey: { question: 'Enumerations, startup presets, readonly application members, methods, open options, build, locale, feature flags, and outside plug-ins the installation holds', Schema: Survey },
    Case: HostCase,
    fields,
    engine,
    precondition,
    placed: copied,
    refusal,
    installed,
    launch,
    opened,
    send,
    quit,
    logs: (instance) =>
        Effect.map(crashes(instance), (found): readonly Rule[] => [{ kind: 'host', pattern: `Library/Logs/Adobe/${logFolder(instance.chosen.version)}/{AIGeneral.log,AIHangMonitor.log,Illustrator*Session_*_PID${instance.pid}.log}`, root: HOME, text: sessions }, uxp(logFolder(instance.chosen.version)), ...found]),
    canonical: (file, bytes) => Effect.succeed([VOLATILE.extensions.some((extension) => file.endsWith(extension)) ? VOLATILE.patterns.reduce((text, pattern) => text.replace(pattern, ''), new TextDecoder('latin1').decode(bytes)) : bytes]),
} as const satisfies Harness<typeof Survey.Type, typeof HostCase.Type, typeof Engine.Type, typeof Placed.Type, typeof fields>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
