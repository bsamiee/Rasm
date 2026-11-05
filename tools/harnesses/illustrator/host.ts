import { matchesGlob } from 'node:path';
import { Automation } from '@rasm/applications/node';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, DateTime, Effect, FileSystem, Number, Option, Path, Record, Result, Schema, Struct } from 'effect';
import jsesc from 'jsesc';
import { ADOBE, crashes, REPORTER, uxp } from '#/adobe';
import { EXTENDSCRIPT } from '#/cases';
import { HarnessError, thrown } from '#/harness-error';
import { Engine, HostCase, HostOutcome, type Install, PASTEBOARD, Reply, Sent, Survey } from '#/illustrator/contract';
import { precondition } from '#/illustrator/precondition';
import { checked, Launched, output, plist } from '#/instance';
import { copied } from '#/precondition';
import { failing, HOME, lifetime, lined, local, type Rule } from '#/report';
import { type Batch, Harness, type Installed, type Instance, installation, type Live, scripted } from '#/session';
import { assembled, engineOf } from '#/source';
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
const Enumerations = Schema.Record(Schema.String, Schema.fromJsonString(Schema.Record(Schema.String, Schema.Finite))).annotate({ description: 'Enumeration name and its member ids the UXP host API binary declares' });
const Settings = Schema.Struct({ folder: Schema.String, locale: Schema.String });
const Bundled = Schema.Struct({ AdobeBuildNumber: Schema.String });
const Named = Schema.Union([Schema.Struct({ type: Schema.Literal('Identifier'), name: Schema.String }), Schema.Struct({ type: Schema.Literal('Literal'), value: Schema.Unknown })]).pipe(Schema.toTaggedUnion('type'));
const Reached = Schema.Struct({ type: Schema.Literal('MemberExpression'), object: Schema.Struct({ type: Schema.Literal('ThisExpression') }), property: Schema.Unknown });
const pasting = Schema.is(Schema.Literals(PASTEBOARD));
const shadowed = Schema.is(Schema.Literals(['Window', 'alert', 'confirm', 'prompt', 'app', '$', 'BridgeTalk']));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const installed = Effect.gen(function* () {
    const rows = yield* (yield* Automation).installed(ADOBE);
    return yield* Effect.forEach(
        rows.filter((row) => row.scriptable && row.identifier.startsWith('com.adobe.illustrator')),
        (row) => installation(row, rows, [REPORTER]),
    );
});

// --- [INSTANCE]
const lsof = Effect.fn('lsof')(function* (args: readonly string[]) {
    const { stdout, stderr, code } = yield* output('lsof', args);
    return code === 0 || stderr === ''
        ? new TextDecoder()
              .decode(stdout)
              .split('\n')
              .filter((line) => line !== '')
        : yield* HarnessError.unavailable(['lsof', ...args].join(' '))(`exited with code ${code}: ${stderr}`);
});
const logFolder = (version: string) => `Adobe Illustrator/${version.split('.').slice(0, 2).join('.')}/Adobe Illustrator`;
const sessions = (contents: string, instance: Instance) => {
    const launched = lifetime(instance).launch;
    const ours = (block: string) =>
        Option.exists(
            Option.flatMap(Option.fromNullishOr(HEADER.exec(block)?.groups), ({ date, time }) => local(`${date} ${time}`)),
            DateTime.between(launched),
        );
    return failing(HEADER.test(contents) ? contents.split(BLOCK).filter(ours).join('') : contents);
};
const held = Effect.fn('held')(function* (pid: number, { app, version }: Installed) {
    const printed = yield* lsof(['-Fan', '-p', String(pid)]);
    const settings = yield* Effect.fromOption(
        Array.findFirst(printed, (line) => Schema.decodeUnknownOption(Settings)(SETTINGS.exec(line)?.groups)),
        () => HarnessError.unavailable(`process ${pid}`)('holds no settings folder file open'),
    );
    const journal = `${HOME}/Library/Logs/Adobe/${logFolder(version)}/{AIGeneral.log,AIHangMonitor.log,UXPLogs_*.log}`;
    const outside = (line: string) =>
        Option.fromNullishOr(PLUGIN.exec(line)?.groups).pipe(
            Option.filter(({ bundle = '' }) => !bundle.startsWith(`${app}/`)),
            Option.map(({ name = '' }) => name),
        );
    return {
        ...settings,
        logs: Array.dedupe(Array.filterMap(Array.window(printed, 2), ([access = '', name = '']) => (WRITING.test(access) && matchesGlob(name.slice(1), journal) ? Result.succeed(name.slice(1)) : Result.failVoid))),
        plugins: Array.dedupe(Array.getSomes(printed.map(outside))),
    };
});
const foreign = Effect.fn('foreign')(function* (pid: number, file: string) {
    const printed = yield* lsof(['-FpR', '--', file]);
    const holders = Array.getSomes(Array.window(printed, 2).map(([process = '', parent = '']) => Option.all({ holder: Option.flatMap(Option.fromNullishOr(HOLDER.exec(process)?.groups?.pid), Number.parse), above: Option.flatMap(Option.fromNullishOr(PARENT.exec(parent)?.groups?.parent), Number.parse) })));
    return holders.filter(({ holder, above }) => holder !== pid && above !== pid).map(({ holder }) => holder);
});
const owned = (pid: number, logs: readonly string[]) => Effect.filter(logs, (file) => Effect.map(foreign(pid, file), (pids) => pids.length === 0));
const enumerations = Effect.fn('enumerations')(function* (app: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = new TextDecoder('latin1').decode(yield* fs.readFile(path.join(app, 'Contents', 'Frameworks', 'AIUXPExtensionHostAPI.framework', 'AIUXPExtensionHostAPI')));
    const listed = yield* Schema.decodeEffect(Enumerations)(Record.fromIterableWith(text.matchAll(ENUMERATION), ({ groups }) => [groups?.name ?? '', groups?.members ?? '{}']));
    return Record.map(listed, Record.keys);
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
const named = (node: unknown) => Option.map(Schema.decodeUnknownOption(Named)(node), Named.match({ Identifier: ({ name }) => name, Literal: ({ value }) => String(value) }));
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
    const encoded = jsesc(yield* Schema.encodeEffect(Sent)(sent), { json: true });
    const { reply } = yield* scripted(live, { direct: (yield* assembled(ENGINE)).code, values: [encoded] });
    return yield* Reply.replied(`${sent._tag} batch`)(reply);
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const copying = batch.cases.some(({ precondition: rows }) => rows.sends?.some(pasting) === true);
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

const illustrator = Harness({
    Engine,
    assembled: ENGINE,
    survey: { question: 'Enumerations, startup presets, readonly application members, methods, open options, build, locale, feature flags, and outside plug-ins the installation holds', Schema: Survey },
    Case: HostCase,
    Outcome: HostOutcome,
    engine,
    precondition,
    placed: copied,
    refusal,
    installed,
    launch: () => Effect.succeed(new Launched({})),
    opened,
    send,
    quit,
    logs: (instance) =>
        Effect.map(crashes(instance), (found): readonly Rule[] => [{ kind: 'host', pattern: `Library/Logs/Adobe/${logFolder(instance.chosen.version)}/{AIGeneral.log,AIHangMonitor.log,Illustrator*Session_*_PID${instance.pid}.log}`, root: HOME, text: lined(sessions) }, uxp(logFolder(instance.chosen.version)), ...found]),
    canonical: (file, bytes) => Effect.succeed([VOLATILE.extensions.some((extension) => file.endsWith(extension)) ? VOLATILE.patterns.reduce((text, pattern) => text.replace(pattern, ''), new TextDecoder('latin1').decode(bytes)) : bytes]),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { illustrator };
