import { createHash } from 'node:crypto';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Effect, FileSystem, Number, Option, Path, type PlatformError, Record, Result, Schema, Stream, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import jsesc from 'jsesc';
import { HarnessError } from '#/error';
import { Engine, HostCase, Install, PASTEBOARD, type Placed, Recorded, Reply, Sent, Survey } from '#/illustrator/contract';
import { precondition } from '#/illustrator/precondition';
import { type Batch, type Host as Harness, type Installed, type Launched, type Live, recorded } from '#/session';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTER = 'com.adobe.crashreporter';
const EXTENSIONS: readonly string[] = ['jsx', 'jsxbin', 'js'];
const ENUMERATION = /_enumNameToId\["(?<name>\w+)"\] = (?<members>\{[^}]*\});/gu;
const SETTINGS = /^n(?<folder>.+\/Library\/Preferences\/[^/]+ Settings\/(?<locale>[^/]+))\//u;
const WRITING = /^a[wu]$/u;
const LOG = /\.log$/iu;
const PLUGIN = /^n(?<bundle>.+\/(?<name>[^/]+)\.aip)\/Contents\/MacOS\/[^/]+$/u;
const HOLDER = /^p(?<pid>\d+)$/u;
const PARENT = /^R(?<parent>\d+)$/u;
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
const SHARED = new URL('../extendscript.jsxinc', import.meta.url);
const ENGINE = new URL('extendscript.jsxinc', import.meta.url);
const BOARD = new URL('pasteboard.jxa', import.meta.url);

// --- [MODELS] --------------------------------------------------------------------------

const RunConfig = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ features: JsonObject })));
const Members = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Finite));
const Settings = Schema.Struct({ folder: Schema.String, locale: Schema.String });
const Thrown = Schema.Struct({ message: Schema.String });
const Named = Schema.Union([Schema.Struct({ type: Schema.Literal('Identifier'), name: Schema.String }), Schema.Struct({ type: Schema.Literal('Literal'), value: Schema.Unknown })]);
const Reached = Schema.Struct({ type: Schema.Literal('MemberExpression'), object: Schema.Struct({ type: Schema.Literal('ThisExpression') }), property: Schema.Unknown });
const pasting = Schema.is(Schema.Literals(PASTEBOARD));
const shadowed = Schema.is(Schema.Literals(['Window', 'alert', 'confirm', 'prompt', 'app', '$', 'BridgeTalk']));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const lines = Effect.fn('lines')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit' });
    const [printed, code] = yield* Effect.all([Stream.runCollect(Stream.splitLines(Stream.decodeText(handle.stdout))), handle.exitCode], { concurrency: 'unbounded' });
    return { printed, code };
}, Effect.scoped);
const installed = Effect.gen(function* () {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    return yield* Effect.forEach(
        (yield* automation.installed).filter((row) => row.scriptable && row.identifier.startsWith('com.adobe.illustrator')),
        (row) =>
            Effect.map(
                Effect.all([path.fromFileUrl(row.url), Effect.fromOption(row.version, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'Launch Services reports no version' }, subject: row.name }))]),
                ([app, version]): Installed => ({ identifier: row.identifier, name: row.name, app, version, watched: [row.identifier, REPORTER] }),
            ),
    );
});
const bundled = Effect.fn('bundled')(function* (app: string, key: string) {
    const path = yield* Path.Path;
    const { printed, code } = yield* lines('plutil', ['-extract', key, 'raw', '-o', '-', path.join(app, 'Contents', 'Info.plist')]);
    return yield* Effect.fromOption(
        Option.filter(Array.head(printed), () => code === 0),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `Info.plist names no ${key}` }, subject: app }),
    );
});
const launch = Effect.fn('launch')(function* (_: Installed, scratch: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const owned = path.join(scratch, 'owned.svg');
    yield* fs.writeFileString(owned, '<svg xmlns="http://www.w3.org/2000/svg" width="1" height="1"/>');
    return { fresh: true, documents: [owned] } satisfies Launched;
});

// --- [INSTANCE]
const held = Effect.fn('held')(function* (pid: number, app: string) {
    const { printed } = yield* lines('lsof', ['-Fan', '-p', String(pid)]);
    const settings = yield* Effect.fromOption(
        Array.findFirst(printed, (line) => Schema.decodeUnknownOption(Settings)(SETTINGS.exec(line)?.groups)),
        () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'holds no settings folder file open' }, subject: `process ${pid}` }),
    );
    const outside = (line: string) => Option.flatMap(Option.fromNullishOr(PLUGIN.exec(line)?.groups), ({ bundle = '', name = '' }) => Option.liftPredicate(name, () => !bundle.startsWith(`${app}/`)));
    return {
        ...settings,
        logs: Array.dedupe(Array.getSomes(Array.window(printed, 2).map(([access = '', name = '']) => Option.liftPredicate(name.slice(1), (file) => WRITING.test(access) && LOG.test(file))))),
        plugins: Array.dedupe(Array.getSomes(printed.map(outside))),
    };
});
const foreign = Effect.fn('foreign')(function* (pid: number, file: string) {
    const { printed } = yield* lines('lsof', ['-FpR', '--', file]);
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
    const [{ folder, locale, plugins }, build] = yield* Effect.all([held(live.pid, live.chosen.app), bundled(live.chosen.app, 'AdobeBuildNumber')]);
    const config = yield* Effect.flatMap(fs.readFileString(path.join(folder, '.runConfig')), Schema.decodeEffect(RunConfig));
    const features = Option.match(Record.get(config, live.chosen.version), { onNone: () => ({}), onSome: (row) => row.features });
    return Option.some({ build, locale, features, plugins } satisfies typeof Install.Type);
});

// --- [SOURCE]
const engine = (scripts: readonly string[]): Result.Result<typeof Engine.Type, string> => {
    const unknown = scripts.filter((script) => !EXTENSIONS.some((extension) => script.endsWith(`.${extension}`)));
    return unknown.length > 0 ? Result.fail(`${unknown.join(', ')} holds no extension of ${EXTENSIONS.join(' ')}`) : Result.succeed(Engine.literal);
};
const named = (node: unknown) => Option.map(Schema.decodeUnknownOption(Named)(node), (found) => (found.type === 'Identifier' ? found.name : String(found.value)));
const escaping = (_: string, node: unknown) => Option.exists(Schema.decodeUnknownOption(Reached)(node), ({ property }) => Option.exists(named(property), shadowed));
const escaped = (code: string) =>
    Result.match(
        Result.try(() => parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowHashBang: true })),
        {
            onFailure: (error) =>
                Option.some(
                    `source does not parse: ${Option.getOrElse(
                        Option.map(Schema.decodeUnknownOption(Thrown)(error), ({ message }) => message),
                        () => 'syntax error',
                    )}`,
                ),
            onSuccess: (tree) => Option.map(Option.fromNullishOr(findNodeAt(tree, undefined, undefined, escaping)), ({ node }) => `${code.slice(node.start, node.end)} reaches the host around the case shadows`),
        },
    );
const refusal = (_: typeof Engine.Type, hosted: typeof HostCase.Type, binary: boolean) => {
    const { set = [], add = [], select = [], steps = [] } = hosted.precondition;
    const expressions = [...Object.values(hosted.read), ...set.map(([target]) => target), ...add.flatMap(({ to, args = [] }) => [to, ...args]), ...select, ...HostCase.match(hosted, { Probe: ({ evaluate }) => [evaluate], Call: ({ call }) => [call], Run: () => [] })];
    const statements = [...steps, ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(hosted._tag === 'Probe' ? undefined : hosted.source), ({ code }) => Option.liftPredicate(code, () => !binary)))];
    return Option.firstSomeOf([...expressions.map((expression) => `(${expression.replaceAll('[*]', '[0]')});`), ...statements].map(escaped));
};

// --- [CASES]
const placed = Effect.fn('placed')(function* (decoded: typeof Placed.Type, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const documents = yield* Effect.forEach(
        decoded.documents,
        (document): Effect.Effect<(typeof Placed.Type.documents)[number], PlatformError.PlatformError> =>
            'open' in document ? Effect.as(fs.copyFile(path.join(product, document.open), path.join(folder, path.basename(document.open))), { ...document, open: path.join(folder, path.basename(document.open)) }) : Effect.succeed(document),
    );
    return { precondition: { ...decoded, documents }, copies: decoded.documents.flatMap((document) => ('open' in document ? [path.join(product, document.open)] : [])) };
});

// --- [DIGESTS]
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    const canonical = VOLATILE.extensions.some((extension) => file.endsWith(extension)) ? VOLATILE.patterns.reduce((text, pattern) => text.replace(pattern, ''), new TextDecoder('latin1').decode(bytes)) : bytes;
    return { file, size: bytes.byteLength, sha256: createHash('sha256').update(canonical).digest('hex') };
});

// --- [TRANSPORT]
const pasteboard = Effect.fn('pasteboard')(function* (mode: 'save' | 'restore', file: string) {
    const script = yield* Effect.flatMap(Path.Path, (path) => path.fromFileUrl(BOARD));
    const { code } = yield* lines('osascript', ['-l', 'JavaScript', script, mode, file]);
    return yield* code === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Unavailable', because: `pasteboard ${mode} exited with code ${code}` }, subject: file });
});
const called = Effect.fn('called')(function* (live: Live, sent: typeof Sent.Type) {
    const [automation, fs, path] = yield* Effect.all([Automation, FileSystem.FileSystem, Path.Path]);
    const sources = yield* Effect.forEach([SHARED, ENGINE], (url) => Effect.flatMap(path.fromFileUrl(url), fs.readFileString));
    const encoded = jsesc(yield* Schema.encodeEffect(Sent)(sent), { json: true });
    const { reply } = yield* automation.execute({ pid: live.pid, command: 'do javascript', arguments: { direct: `${sources.join('\n')}\nharness(arguments[0]);`, 'with arguments': [encoded] }, artifacts: [] });
    const answer = yield* Schema.decodeUnknownEffect(Reply)(reply);
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: `${sent._tag} batch` }) : Effect.succeed(answer);
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, typeof HostCase.Type>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const surveying = Effect.gen(function* () {
        const [listed, install] = yield* Effect.all([enumerations(live.chosen.app), Effect.orDie(Schema.decodeUnknownEffect(Install)(Option.getOrUndefined(batch.opened)))]);
        const header = yield* called(live, Sent.cases.Survey.make({ enumerations: listed }));
        return { ...Struct.omit(header, ['survey']), ...(header.survey === undefined ? {} : { survey: { enumerations: listed, ...header.survey, host: install } }) };
    });
    const proving = Effect.gen(function* () {
        const { folder, logs } = yield* held(live.pid, live.chosen.app);
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
    yield* automation.execute({ pid: live.pid, command: 'quit', arguments: { saving: { _tag: 'enumerator', name: 'no' } }, artifacts: [] });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    name: 'illustrator',
    Engine,
    survey: { question: 'Enumerations, startup presets, readonly application members, methods, open options, build, locale, feature flags, and outside plug-ins the installation holds', Schema: Survey, host: (survey) => Option.some(survey.host) },
    Case: HostCase,
    shown: ['alerts', 'activations', 'console', 'logs'],
    engine,
    precondition,
    placed,
    refusal,
    installed,
    launch,
    opened,
    send,
    records: (_, file) => recorded(Recorded)(file),
    quit,
    digest,
    compare: { _tag: 'Imported' },
} as const satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
