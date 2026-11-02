import { createHash } from 'node:crypto';
import { homedir } from 'node:os';
import { kill } from 'node:process';
import { pathToFileURL } from 'node:url';
import { type Installation, State } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { parse } from 'acorn';
import { findNodeAt } from 'acorn-walk';
import { Array, Data, Deferred, Duration, Effect, Exit, FileSystem, HashMap, Match, Number, Option, Order, Path, type PlatformError, Predicate, pipe, Record, Ref, Result, Schema, Scope, Stream, Struct, SubscriptionRef, String as Text, UndefinedOr } from 'effect';
import { ChildProcess } from 'effect/process';
import jsesc from 'jsesc';
import { Batch as KitBatch, Call as KitCall, Probe as KitProbe, Run as KitRun, Vector } from '#/cases';
import { HarnessError } from '#/error';
import { Engine, fields, Header, HostCase, Outcome, PASTEBOARD, Recorded, Reply, Batch as Sent, Survey } from '#/illustrator/contract';
import type { precondition } from '#/illustrator/precondition';
import { merged } from '#/indesign/precondition';
import { crashed, type Exited, ended, Launch, settled, watch } from '#/instance';
import { type Answered, Crash, type Hang, recorded as many, results } from '#/results';
import { bury, JsonObject } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Control = NonNullable<Element['detail']>['controls'][number];
type Row = (typeof Batch.Type.cases)[number];
type Note = readonly [Option.Option<number>, typeof Answered.Type];
type Verdict = Data.TaggedEnum<{ Exited: { readonly exit: Exited }; Hung: { readonly index: Option.Option<number>; readonly hang: typeof Hang.Type }; Lost: { readonly error: HarnessError | Schema.SchemaError | PlatformError.PlatformError } }>;
interface Mapped {
    readonly file: string;
    readonly line: number;
}
interface Prepared {
    readonly hosted: typeof HostCase.Type;
    readonly row: Row;
    readonly map: readonly Mapped[];
    readonly presses: readonly string[];
}
interface Plan {
    readonly sendable: readonly Prepared[];
    readonly settled: readonly (typeof Outcome.Type)[];
}
interface Session {
    readonly pid: number;
    readonly app: string;
    readonly relaunch: Option.Option<string>;
    readonly verdict: Deferred.Deferred<Verdict>;
    readonly gone: Deferred.Deferred<Exited>;
    readonly ready: Deferred.Deferred<void>;
    readonly state: SubscriptionRef.SubscriptionRef<typeof State.Type>;
    readonly notes: Ref.Ref<readonly Note[]>;
    readonly records: Ref.Ref<Option.Option<string>>;
    readonly presses: Ref.Ref<readonly (readonly string[])[]>;
    readonly parents: Ref.Ref<HashMap.HashMap<number, number>>;
    readonly engine: string;
    readonly reporter: Option.Option<number>;
}
interface Opened {
    readonly settings: string;
    readonly locale: string;
    readonly folders: readonly string[];
    readonly withheld: readonly { readonly file: string; readonly holders: readonly number[] }[];
    readonly plugins: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENUMERATION = /_enumNameToId\["(?<name>\w+)"\] = (?<members>\{[^}]*\});/gu;
const SETTINGS = /^n(?<folder>.+\/Library\/Preferences\/[^/]+ Settings\/(?<locale>[^/]+))\//u;
const WRITING = /^a[wu]$/u;
const LOG = /\.log$/iu;
const PLUGIN = /^n(?<bundle>.+\/(?<name>[^/]+)\.aip)\/Contents\/MacOS\/[^/]+$/u;
const SAFE_MODE = /\$\$\$\/app\/SafeMode\/LaunchNormally=(?<label>[^\0\n]+)/u;
const CRASH_ALERT = 'com.apple.UserNotificationCenter';
const REPORTER = { domain: 'com.adobe.crashreporter', key: 'always_never_send' } as const;
const CRASH_LOG = /^crmaccrashlog_.+\.log$/u;
const CRASH_SECTION = '---CR_SECTION_';
const COLOR_SETTINGS = 'AI Color Settings';
const SHADOWED: ReadonlySet<string> = new Set(['Window', 'alert', 'confirm', 'prompt', 'app', '$', 'BridgeTalk']);
const RECORD = /^(?<index>\d+)\.(?<kind>json|started)$/u;
const JSXBIN = /(?<quote>["'])(?<literal>@JSXBIN@(?:\\.|(?!\k<quote>)[^\\])*)\k<quote>/su;
const BYTE_ORDER_MARK = /^﻿/u;
const CONTINUATION = /\\\r?\n/gu;
const ESCAPE = /\\(?<character>.)/gu;
const INCLUDE = /^[ \t]*(?:\/\/[ \t]*@|#)include[ \t]+["'](?<name>[^"']+)["'][ \t;]*$/u;
const INCLUDE_PATH = /^[ \t]*(?:\/\/[ \t]*@|#)includepath[ \t]+["'](?<paths>[^"']*)["']/mu;
const DIRECTIVE = /^[ \t]*#(?:target|targetengine|strict|script)\b/u;
const VERSIONS = new Intl.Collator('en', { numeric: true });
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

// --- [MODELS] --------------------------------------------------------------------------

const Verdict = Data.taggedEnum<Verdict>();
const Probe = KitProbe.mapFields(Struct.omit(['dialogs']));
const Call = KitCall.mapFields(Struct.omit(['dialogs']));
const Run = KitRun.mapFields(Struct.omit(['dialogs']));
const Batch = KitBatch.mapFields((own) => ({ ...own, cases: Schema.Array(Schema.Union([Probe, Call, Run])) }));
const Results = results(
    Engine,
    {
        state: Schema.optionalKey(Header.fields.state),
        host: Survey.fields.host,
        reporter: Schema.optionalKey(Schema.Int.annotate({ description: 'com.adobe.crashreporter always_never_send at launch, 2 sending crashes to DiagnosticReports' })),
        shared: many(Schema.Int.annotate({ description: 'Instance of the installation running beside the run, sharing its settings folder and logs' })),
        withheld: many(Schema.Struct({ file: Schema.String, holders: Schema.Array(Schema.Int) }).annotate({ description: 'Log file another process holds open, left out of case logs' })),
        exits: many(Crash.annotate({ description: 'Exit of each launched instance, a quit reading status 0' })),
    },
    fields,
);
const RunConfig = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ features: JsonObject })));
const Members = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Finite));
const Pid = Schema.fromJsonString(Schema.Struct({ pid: Schema.Int }));
const sameHost = Schema.toEquivalence(Survey.fields.host);
const pasting = Schema.is(Schema.Literals(PASTEBOARD));
const newest = Order.flip(
    Order.mapInput(
        Order.make((left: string, right: string) => Number.sign(VERSIONS.compare(left, right))),
        (row: Installation) => Option.getOrElse(row.version, () => ''),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLATIONS]
const lines = Effect.fn('lines')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'ignore' });
    const [read, code] = yield* Effect.all([Stream.runCollect(Stream.splitLines(Stream.decodeText(handle.stdout))), handle.exitCode], { concurrency: 'unbounded' });
    return Option.liftPredicate(read, () => code === 0);
}, Effect.scoped);
const prefixed = (versions: readonly string[]) => (held: string) => versions.some((prefix) => held.startsWith(prefix));
const installations = Effect.fn('installations')(function* (versions: readonly string[]) {
    const automation = yield* Automation;
    const hosts = Array.sort(
        (yield* automation.installed).filter((row) => row.scriptable && row.identifier.startsWith('com.adobe.illustrator')),
        newest,
    );
    const listed = hosts.map((row) => Option.getOrElse(row.version, () => row.name)).join(', ');
    const chosen = versions.length === 0 ? hosts : hosts.filter((row) => Option.exists(row.version, prefixed(versions)));
    return yield* Effect.fromOption(Array.match(chosen, { onEmpty: Option.none, onNonEmpty: Option.some }), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installed versions ${listed}` }, subject: `Illustrator ${versions.join(', ')}` }));
});
const bundled = Effect.fn('bundled')(function* (app: string, key: string) {
    const path = yield* Path.Path;
    const read = Option.flatMap(yield* lines('plutil', ['-extract', key, 'raw', '-o', '-', path.join(app, 'Contents', 'Info.plist')]), Array.head);
    return yield* Effect.fromOption(read, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `Info.plist names no ${key}` }, subject: app }));
});
const helpers = Effect.fn('helpers')(function* (app: string) {
    const path = yield* Path.Path;
    return Option.getOrElse(yield* lines('find', [path.join(app, 'Contents'), '-name', '*.app', '-prune']), () => []);
});
const reporting = Effect.map(lines('defaults', ['read', REPORTER.domain, REPORTER.key]), (read) => Option.flatMap(Option.flatMap(read, Array.head), Number.parse));

// --- [SURVEY]
const enumerations = Effect.fn('enumerations')(function* (app: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = new TextDecoder('latin1').decode(yield* fs.readFile(path.join(app, 'Contents', 'Frameworks', 'AIUXPExtensionHostAPI.framework', 'AIUXPExtensionHostAPI')));
    const rows = yield* Effect.forEach(Array.fromIterable(text.matchAll(ENUMERATION)), (found) => Effect.map(Schema.decodeEffect(Members)(found.groups?.members ?? '{}'), (members) => [found.groups?.name ?? '', Object.keys(members)] as const));
    return Record.fromEntries(rows);
});
const hostOf = Effect.fn('hostOf')(function* (session: Session, opened: Opened, version: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const config = yield* Effect.flatMap(fs.readFileString(path.join(opened.settings, '.runConfig')), Schema.decodeEffect(RunConfig));
    return { build: yield* bundled(session.app, 'AdobeBuildNumber'), locale: opened.locale, features: Option.match(Record.get(config, version), { onNone: () => ({}), onSome: ({ features }) => features }), plugins: opened.plugins };
});

// --- [SOURCE]
const prepared = Effect.fn('prepared')(function* (script: string, kept: Option.Option<typeof Call.Type.lines & {}>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = (yield* fs.readFileString(script)).replace(BYTE_ORDER_MARK, '');
    const binary = Option.map(Option.fromNullishOr(JSXBIN.exec(text)?.groups?.literal), (literal) => literal.replace(CONTINUATION, '').replace(ESCAPE, '$<character>'));
    const located = Effect.fnUntraced(function* (file: string, searched: readonly string[], name: string, chain: readonly string[]) {
        const found = yield* Effect.findFirst(
            searched.map((entry): string => path.resolve(entry, name)),
            (candidate: string) => fs.exists(candidate),
        );
        const included = yield* Effect.fromOption(found, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `include ${name} found in none of ${searched.join(', ')}` }, subject: file }));
        return chain.includes(included) ? yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `include cycle ${[...chain, included].join(' > ')}` }, subject: file }) : included;
    });
    const expanded: (file: string, body: string, chain: readonly string[]) => Effect.Effect<readonly (readonly [string, Mapped])[], HarnessError | PlatformError.PlatformError> = Effect.fnUntraced(function* (file: string, body: string, chain: readonly string[]) {
        const folder = path.dirname(file);
        const searched = [
            folder,
            ...(INCLUDE_PATH.exec(body)?.groups?.paths ?? '')
                .split(';')
                .filter((entry) => entry !== '')
                .map((entry) => path.resolve(folder, entry)),
        ];
        const nested = (included: string) => Effect.flatMap(fs.readFileString(included), (source) => expanded(included, source.replace(BYTE_ORDER_MARK, ''), [...chain, included]));
        const lined = (source: string, index: number) =>
            Option.match(Option.fromNullishOr(INCLUDE.exec(source)?.groups?.name), {
                onNone: () => Effect.succeed([[DIRECTIVE.test(source) ? '' : source, { file, line: index + 1 }] as const]),
                onSome: (name) => Effect.flatMap(located(file, searched, name, chain), nested),
            });
        return (yield* Effect.forEach(Array.fromIterable(Text.linesIterator(body)), lined)).flat();
    });
    const rows = Option.isSome(binary) ? [[binary.value, { file: script, line: 1 }] as const] : yield* expanded(script, text, [script]);
    const ranged = Option.match(kept, { onNone: () => rows, onSome: (ranges) => ranges.flatMap(([from, to]) => rows.slice(from - 1, to)) });
    return { code: ranged.map(([line]) => line).join('\n'), map: ranged.map(([, at]) => at), binary: Option.isSome(binary) };
});
const field = (node: unknown, key: string): unknown => (Predicate.isObject(node) ? Reflect.get(node, key) : undefined);
const named = (node: unknown) =>
    Match.value(field(node, 'type')).pipe(
        Match.when('Identifier', () => Option.some(String(field(node, 'name')))),
        Match.when('Literal', () => Option.some(String(field(node, 'value')))),
        Match.orElse(() => Option.none<string>()),
    );
const escaping = (refused: ReadonlySet<string>) => (type: string, node: unknown) => {
    const callee = field(node, 'callee');
    const called = type === 'CallExpression' && field(callee, 'type') === 'MemberExpression' && Option.exists(named(field(callee, 'property')), (name) => refused.has(name)) && !Option.contains(named(field(callee, 'object')), 'app');
    const reached = type === 'MemberExpression' && field(field(node, 'object'), 'type') === 'ThisExpression' && Option.exists(named(field(node, 'property')), (name) => SHADOWED.has(name));
    return called || reached;
};
const escaped = (refused: ReadonlySet<string>) => (code: string) =>
    Result.match(
        Result.try(() => parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, allowHashBang: true })),
        {
            onFailure: (error) => Option.some(`source does not parse: ${String(error)}`),
            onSuccess: (tree) => Option.map(Option.fromNullishOr(findNodeAt(tree, undefined, undefined, escaping(refused))), ({ node }) => `${code.slice(node.start, node.end)} reaches the host around the case shadows`),
        },
    );
const refusal = (methods: readonly string[], hosted: typeof HostCase.Type, binary: boolean) => {
    const { set = [], add = [], select = [], steps = [], sends = [] } = hosted.precondition;
    const refused = new Set(methods.filter((name) => !sends.includes(name) && name !== 'open'));
    const expressions = [...Object.values(hosted.read), ...set.map(([target]) => target), ...add.flatMap(({ to, args = [] }) => [to, ...args]), ...select, ...HostCase.match(hosted, { Probe: ({ evaluate }) => [evaluate], Call: ({ call }) => [call], Run: () => [] })];
    const statements = [...steps, ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(hosted._tag === 'Probe' ? undefined : hosted.source), ({ code }) => Option.liftPredicate(code, () => !binary)))];
    return Option.firstSomeOf([...expressions.map((expression) => `(${expression.replaceAll('[*]', '[0]')});`), ...statements].map(escaped(refused)));
};

// --- [CASES]
const vectorCalls =
    ({ calls, script, lines: kept }: NonNullable<typeof Batch.Type.vector>) =>
    (rows: typeof Vector.Type.cases) =>
        Array.getSomes(rows.map((row) => Option.map(Record.get(calls, row.fn), (original) => ({ name: row.name, fn: row.fn, call: original, args: row.args, ...(script === undefined ? {} : { script }), ...(kept === undefined ? {} : { lines: kept }) }))));
const expand = Effect.fn('expand')(function* (product: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const batch = yield* Schema.decodeEffect(Schema.fromJsonString(Batch))(yield* fs.readFileString(file));
    const vectored: readonly (typeof Call.Type)[] = yield* UndefinedOr.match(batch.vector, {
        onUndefined: () => Effect.succeed([]),
        onDefined: (vector) =>
            fs.readFileString(path.join(product, vector.file)).pipe(
                Effect.flatMap(Schema.decodeEffect(Vector)),
                Effect.map(({ cases: rows }) => vectorCalls(vector)(rows)),
            ),
    });
    return { batch, rows: [...batch.cases, ...vectored] };
});
const conditionOf = (batch: Pick<typeof Batch.Type, 'precondition'>, row: Row) => merged(batch.precondition ?? {}, row.precondition ?? {});
const scriptOf = (batch: Pick<typeof Batch.Type, 'script'>, row: Row) => Option.fromUndefinedOr(Schema.is(Probe)(row) ? undefined : (row.script ?? batch.script));
const failed = (name: string) => (reason: string) => Effect.succeed(Result.fail(Outcome.make({ name, completion: { _tag: 'Failed', reason } })));
const hostCase = (product: string, captures: string, batch: Pick<typeof Batch.Type, 'script' | 'precondition' | 'read'>, Precondition: ReturnType<typeof precondition>, methods: readonly string[]) => (row: Row, index: number) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const folder = path.join(captures, String(index));
        yield* fs.makeDirectory(folder, { recursive: true });
        const placed = yield* Schema.decodeUnknownEffect(Precondition)(conditionOf(batch, row));
        const documents = yield* Effect.forEach(
            placed.documents,
            (document): Effect.Effect<(typeof placed.documents)[number], PlatformError.PlatformError> =>
                'open' in document ? Effect.as(fs.copyFile(path.join(product, document.open), path.join(folder, path.basename(document.open))), { ...document, open: path.join(folder, path.basename(document.open)) }) : Effect.succeed(document),
        );
        const answers = Record.filter({ windows: row.windows, confirms: row.confirms, prompts: row.prompts, files: row.files?.map((answer) => (answer === null ? null : path.join(product, answer))) }, (value) => value !== undefined);
        const shared = { name: row.name, precondition: { ...placed, documents }, read: { ...batch.read, ...row.read }, folder, ...answers };
        const loaded = yield* Option.match(scriptOf(batch, row), {
            onNone: () => Effect.succeedNone,
            onSome: (script) => Effect.asSome(Effect.map(prepared(path.join(product, script), Schema.is(Call)(row) ? Option.fromUndefinedOr(row.lines) : Option.none()), (held) => ({ ...held, source: { script: path.join(product, script), code: held.code } }))),
        });
        const args = (Schema.is(Probe)(row) ? [] : (row.args ?? [])).map((arg) => bury(arg));
        const source = Option.map(loaded, ({ source: held }) => held);
        const hosted = yield* Match.value(row).pipe(
            Match.when(Schema.is(Probe), ({ evaluate }) => Effect.succeed(HostCase.cases.Probe.make({ ...shared, evaluate }))),
            Match.when(Schema.is(Call), ({ call }) => Effect.succeed(HostCase.cases.Call.make({ ...shared, ...Option.match(source, { onNone: () => ({}), onSome: (held) => ({ source: held }) }), call, args }))),
            Match.orElse(() =>
                Effect.map(
                    Effect.fromOption(source, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no script for the run case or batch' }, subject: row.name })),
                    (held) => HostCase.cases.Run.make({ ...shared, source: held, args }),
                ),
            ),
        );
        return yield* Option.match(
            refusal(
                methods,
                hosted,
                Option.exists(loaded, ({ binary }) => binary),
            ),
            {
                onNone: () => Effect.succeed(Result.succeed<Prepared>({ hosted, row, map: Option.match(loaded, { onNone: () => [], onSome: ({ map }) => map }), presses: row.presses ?? [] })),
                onSome: failed(row.name),
            },
        );
    }).pipe(Effect.catchTags({ SchemaError: (error) => failed(row.name)(error.message), HarnessError: (error) => failed(row.name)(error.message) }));
const normalize = Effect.fn('normalize')(function* (product: string, file: string, captures: string, only: readonly string[], Precondition: ReturnType<typeof precondition>, methods: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const { batch, rows } = yield* expand(product, file);
    yield* Schema.decodeEffect(Schema.Array(Schema.Literals(rows.map(({ name }) => name))))(only);
    const kept = rows.filter(({ name }) => only.length === 0 || only.includes(name));
    yield* fs.remove(captures, { recursive: true, force: true });
    const made = yield* Effect.forEach(kept, hostCase(product, captures, batch, Precondition, methods));
    return { batch, rows: kept, sendable: Array.getSuccesses(made), settled: Array.getFailures(made) };
});

// --- [RECORDS]
const recordOf = (file: string) => Option.flatMap(Option.fromNullishOr(RECORD.exec(file)?.groups), ({ index = '', kind = '' }) => Option.map(Number.parse(index), (at) => ({ file, at, kind })));
const recorded = Effect.fn('recorded')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = Array.getSomes((yield* fs.readDirectory(folder)).map(recordOf));
    const finished = yield* Effect.forEach(
        entries.filter(({ kind }) => kind === 'json'),
        ({ file, at }) => Effect.map(Effect.flatMap(fs.readFileString(path.join(folder, file)), Schema.decodeEffect(Recorded)), (row) => [at, row] as const),
    );
    const done = new Map(finished);
    return {
        finished: done,
        running: Array.findFirst(
            Array.sort(
                entries.map(({ at }) => at),
                Order.flip(Number.Order),
            ),
            (at) => !done.has(at),
        ),
    };
});
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    const canonical = VOLATILE.extensions.some((extension) => file.endsWith(extension)) ? VOLATILE.patterns.reduce((text, pattern) => text.replace(pattern, ''), new TextDecoder('latin1').decode(bytes)) : bytes;
    return { file, size: bytes.byteLength, sha256: createHash('sha256').update(canonical).digest('hex') };
});
const filesIn = Effect.fn('filesIn')(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.filter(
        (yield* fs.readDirectory(folder, { recursive: true })).map((file) => path.join(folder, file)),
        (file) => Effect.map(fs.stat(file), ({ type }) => type === 'File'),
    );
});
const writtenIn = Effect.fn('writtenIn')(function* (folder: string, copies: ReadonlySet<string>) {
    return (yield* Effect.forEach(yield* filesIn(folder), digest)).filter(({ sha256 }) => !copies.has(sha256));
});
const mapped = (map: readonly Mapped[]) => (row: typeof Outcome.Type) => {
    const at = (line: number) => Option.getOrElse(Array.get(map, line - 1), () => ({ line }));
    return row.completion._tag === 'Thrown' ? { ...row, completion: { ...row.completion, ...(row.completion.line === undefined ? {} : at(row.completion.line)), ...(row.completion.stack === undefined ? {} : { stack: row.completion.stack.map(({ line }) => at(line)) }) } } : row;
};

// --- [CRASHES]
const archived = Effect.fn('archived')(function* (pid: number, folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const archive = path.join(homedir(), 'Library', 'Logs', REPORTER.domain);
    const logs = (yield* fs.exists(archive)) ? (yield* fs.readDirectory(archive, { recursive: true })).filter((file) => CRASH_LOG.test(path.basename(file))) : [];
    const report = (text: string) =>
        pipe(
            Array.fromIterable(Text.linesIterator(text)),
            Array.dropWhile((line) => !line.startsWith(CRASH_SECTION)),
            Array.drop(1),
            Array.takeWhile((line) => !line.startsWith(CRASH_SECTION)),
        );
    const reports = yield* Effect.forEach(logs, (file) => Effect.map(fs.readFileString(path.join(archive, file)), report));
    const found = Array.findFirst(reports, (held) => Option.exists(Schema.decodeOption(Pid)(held.slice(1).join('\n')), (row) => row.pid === pid));
    return yield* Option.match(found, {
        onNone: () => Effect.succeedNone,
        onSome: (held) => Effect.as(fs.writeFileString(path.join(folder, 'crash.ips'), held.join('\n')), Option.some(pathToFileURL(path.join(folder, 'crash.ips')))),
    });
});
const crashOf = Effect.fn('crashOf')(function* (exit: Exited, pid: number, folder: string) {
    const report = exit.report === undefined ? yield* archived(pid, folder) : Option.none();
    return yield* crashed(Option.match(report, { onNone: () => exit, onSome: (url) => ({ ...exit, report: { url, pid } }) }));
});

// --- [WATCHER]
const described = (state: typeof State.Type) =>
    State.match(state, {
        Launching: () => 'launching',
        Responding: () => 'responding',
        Unresponsive: ({ process, usage }) => `unresponsive ${process}${usage === undefined ? '' : ` cpu ${usage.cpu} s footprint ${usage.footprint} B`}`,
        Failed: ({ reason }) => `status failed: ${reason}`,
        Exited: ({ signal, status, report, reportFailure }) => `exited with ${signal === undefined ? `status ${status ?? 'unread'}` : `signal ${signal}`}${report === undefined ? '' : `, report ${report.url.href}`}${reportFailure === undefined ? '' : `, ${reportFailure}`}`,
    });
const verdictText = Verdict.$match({ Exited: ({ exit }) => described(exit), Hung: ({ hang }) => `hung ${hang.cpu}, ${hang.sample}`, Lost: ({ error }) => error.message });
const quiet = (state: typeof State.Type) => State.guards.Responding(state) && state.elements.every(({ requiresAnswer }) => !requiresAnswer);
const pressable = (element: Element, labels: readonly Option.Option<string>[], system: boolean): Option.Option<Control> => {
    const pressing = (element.detail?.controls ?? []).filter(({ actions }) => actions.includes('AXPress'));
    const cancel = [Array.findFirst(pressing, ({ attribute }) => attribute === 'AXCancelButton'), Array.findFirst(pressing, ({ identifier }) => identifier === 'CancelButton')];
    return system
        ? Option.firstSomeOf(cancel)
        : Option.firstSomeOf([
              ...labels.map((label) => Option.flatMap(label, (text) => Array.findFirst(pressing, (control) => control.label === text))),
              ...cancel,
              Array.findFirst(pressing, ({ attribute }) => attribute === 'AXDefaultButton'),
              Array.findFirst(pressing, ({ identifier }) => identifier === 'OKButton'),
              Array.head(pressing.length === 1 ? pressing : []),
          ]);
};
const parentOf = Effect.fn('parentOf')(function* (session: Session, pid: number) {
    const known = HashMap.get(yield* Ref.get(session.parents), pid);
    const parent = Option.isSome(known)
        ? known.value
        : Option.getOrElse(
              Option.flatMap(Option.flatMap(yield* lines('ps', ['-o', 'ppid=', '-p', String(pid)]), Array.head), (text) => Number.parse(text.trim())),
              () => 0,
          );
    yield* Ref.update(session.parents, HashMap.set(pid, parent));
    return parent;
});
const foreign = Effect.fn('foreign')(function* (session: Session, file: string) {
    const holders = Array.getSomes(Option.getOrElse(yield* lines('lsof', ['-t', '--', file]), () => []).map(Number.parse));
    const parents = yield* Effect.forEach(holders, (pid) => parentOf(session, pid));
    return holders.filter((pid, at) => pid !== session.pid && parents[at] !== session.pid);
});
const watcher = (chosen: Installation, session: Session) =>
    Effect.fn('watcher')(function* (row: { readonly identifier: string; readonly pid: number; readonly state: typeof State.Type }) {
        const automation = yield* Automation;
        const host = row.identifier === chosen.identifier && row.pid === session.pid;
        const exited = yield* Deferred.isDone(session.gone);
        const child = row.identifier !== chosen.identifier && (yield* parentOf(session, row.pid)) === session.pid;
        const system = row.identifier !== chosen.identifier && row.identifier !== REPORTER.domain && !child;
        const ours = host || system || child || (exited && row.identifier === REPORTER.domain);
        const running = Effect.flatMap(Ref.get(session.records), Option.match({ onNone: () => Effect.succeedNone, onSome: (held: string) => Effect.map(recorded(held), ({ running: at }) => at) }));
        const answer = Effect.fnUntraced(function* (element: Element) {
            const shown = [element.title ?? '', ...(element.detail?.text ?? [])].join(' ');
            const [at, presses] = yield* Effect.all([running, Ref.get(session.presses)]);
            const used = (yield* Ref.get(session.notes)).filter(([index]) => Option.getOrUndefined(index) === Option.getOrUndefined(at)).length;
            const wanted = Option.flatMap(at, (index) => Option.flatMap(Array.get(presses, index), Array.get(used)));
            const control = system && !(exited && row.identifier === CRASH_ALERT) ? Option.none() : pressable(element, [wanted, session.relaunch], system);
            const note = {
                ...(element.title === undefined ? {} : { title: element.title }),
                text: element.detail?.text ?? [],
                ...Option.match(
                    Option.flatMap(control, ({ label }) => Option.fromUndefinedOr(label)),
                    { onNone: () => ({}), onSome: (pressed) => ({ pressed }) },
                ),
            };
            yield* Ref.update(session.notes, Array.append([at, note] as const));
            yield* Option.match(control, {
                onNone: () => Effect.logWarning(`${row.identifier} ${element.title ?? element.role} left unanswered: ${shown}`),
                onSome: (pressed) => Effect.ignore(automation.perform({ pid: row.pid, actions: [{ perform: { element: pressed.id, action: 'AXPress' } }] }), { log: 'Warn', message: `press of ${pressed.label ?? pressed.id} refused` }),
            });
        });
        yield* host ? SubscriptionRef.set(session.state, row.state) : Effect.void;
        yield* State.match(row.state, {
            Launching: () => Effect.void,
            Responding: ({ elements }) =>
                Effect.andThen(
                    host && quiet(row.state) ? Deferred.succeed(session.ready, undefined) : Effect.void,
                    Effect.forEach(
                        elements.filter(({ requiresAnswer }) => requiresAnswer && ours),
                        answer,
                        { discard: true },
                    ),
                ),
            Unresponsive: () => Effect.void,
            Failed: () => Effect.logWarning(`process ${row.pid} ${described(row.state)}`),
            Exited: (exit) => (host && settled(exit) ? Effect.andThen(Deferred.succeed(session.gone, exit), Deferred.succeed(session.verdict, Verdict.Exited({ exit }))) : Effect.void),
        });
    });
const stalled = Effect.fn('stalled')(function* (session: Session, index: Option.Option<number>, folder: string) {
    const usage = yield* SubscriptionRef.get(session.state);
    const sample = yield* ended(session.pid, `${folder}/sample.txt`);
    yield* Deferred.succeed(session.verdict, Verdict.Hung({ index, hang: { cpu: usage._tag === 'Unresponsive' && usage.process === 'running' ? 'busy' : 'blocked', sample } }));
});
const progress = Effect.fn('progress')(function* (session: Session, records: string, folders: readonly string[], limit: Duration.Duration) {
    const fs = yield* FileSystem.FileSystem;
    yield* Stream.concat(Stream.make(undefined), fs.watch(records)).pipe(
        Stream.switchMap(() => Stream.fromEffect(Effect.sleep(limit))),
        Stream.runHead,
    );
    const { running, finished } = yield* recorded(records);
    yield* stalled(
        session,
        running,
        Option.getOrElse(
            Array.get(
                folders,
                Option.getOrElse(running, () => finished.size),
            ),
            () => records,
        ),
    );
});
const answeredFor = Effect.fn('answeredFor')(function* (session: Session, index: number, folder: string, limit: Duration.Duration) {
    const presses = Option.getOrElse(Array.get(yield* Ref.get(session.presses), index), () => []);
    const done = (state: typeof State.Type) => Effect.map(Ref.get(session.notes), (notes) => quiet(state) && notes.filter(([at]) => Option.contains(at, index)).length >= presses.length);
    yield* Stream.concat(Stream.fromEffect(SubscriptionRef.get(session.state)), SubscriptionRef.changes(session.state)).pipe(
        Stream.mapEffect(done),
        Stream.takeUntil((held) => held),
        Stream.runDrain,
        Effect.timeoutOrElse({ duration: limit, orElse: () => stalled(session, Option.some(index), folder) }),
    );
});

// --- [TRANSPORT]
const sent = Effect.fn('sent')(function* (session: Session, batch: typeof Sent.Type) {
    const automation = yield* Automation;
    const encoded = jsesc(yield* Schema.encodeEffect(Sent)(batch), { json: true });
    const { reply } = yield* automation.execute({ pid: session.pid, command: 'do javascript', arguments: { direct: `${session.engine}(arguments[0]);`, 'with arguments': [encoded] }, artifacts: [] });
    const answer = yield* Schema.decodeUnknownEffect(Reply)(reply);
    return yield* 'failed' in answer ? HarnessError.make({ reason: { _tag: 'Unavailable', because: answer.failed }, subject: 'extendscript batch' }) : Effect.succeed(answer);
});
const pasteboard = Effect.fn('pasteboard')(function* (mode: 'save' | 'restore', file: string) {
    const path = yield* Path.Path;
    const script = yield* path.fromFileUrl(new URL('pasteboard.jxa', import.meta.url));
    return yield* Effect.fromOption(yield* lines('osascript', ['-l', 'JavaScript', script, mode, file]), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `pasteboard ${mode} failed` }, subject: file }));
});
const acquired = Effect.fn('acquired')(function* (chosen: Installation, limit: Duration.Duration, owned: string, launching: Scope.Scope) {
    const [fs, path, automation] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Automation]);
    const app = yield* path.fromFileUrl(chosen.url);
    const lost = (pid: number) => (because: string) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: `process ${pid}` });
    const executable = yield* bundled(app, 'CFBundleExecutable');
    const [nested, binary, reporter, engine, running] = yield* Effect.all([helpers(app), fs.readFile(path.join(app, 'Contents', 'MacOS', executable)), reporting, Effect.flatMap(path.fromFileUrl(new URL('extendscript.jsxinc', import.meta.url)), fs.readFileString), SubscriptionRef.get(automation.instances)]);
    const instance = yield* Scope.provide(Launch({ app, fresh: true, documents: [owned] }), launching);
    const session: Session = {
        pid: instance.pid,
        app,
        relaunch: Option.map(Option.fromNullishOr(SAFE_MODE.exec(new TextDecoder('latin1').decode(binary))?.groups?.label), (label) => label.replace('&', '')),
        verdict: yield* Deferred.make<Verdict>(),
        gone: yield* Deferred.make<Exited>(),
        ready: yield* Deferred.make<void>(),
        state: yield* SubscriptionRef.make<typeof State.Type>(instance.state),
        notes: yield* Ref.make<readonly Note[]>([]),
        records: yield* Ref.make(Option.none<string>()),
        presses: yield* Ref.make<readonly (readonly string[])[]>([]),
        parents: yield* Ref.make(HashMap.empty<number, number>()),
        engine,
        reporter,
    };
    yield* Effect.forkScoped(
        Stream.runForEach(watch([chosen.identifier, ...nested], instance.pid), watcher(chosen, session)).pipe(
            Effect.andThen(lost(instance.pid)('watch ended')),
            Effect.catch((error) => Deferred.succeed(session.verdict, Verdict.Lost({ error }))),
        ),
    );
    yield* settled(instance.state) ? Effect.andThen(Deferred.succeed(session.gone, instance.state), Deferred.succeed(session.verdict, Verdict.Exited({ exit: instance.state }))) : Effect.void;
    yield* Effect.raceFirst(
        Deferred.await(session.ready),
        Effect.flatMap(Deferred.await(session.verdict), (over) => lost(instance.pid)(verdictText(over))),
    ).pipe(Effect.timeoutOrElse({ duration: limit, orElse: () => lost(instance.pid)(`no responding state within ${Duration.format(limit)}`) }));
    const held = Option.getOrElse(yield* lines('lsof', ['-Fan', '-p', String(instance.pid)]), () => []);
    const settings = yield* Effect.fromOption(
        Option.flatMap(
            Array.findFirst(held, (line) => SETTINGS.test(line)),
            (line) => Option.fromNullishOr(SETTINGS.exec(line)?.groups),
        ),
        () => lost(instance.pid)('holds no settings folder file open'),
    );
    const logs = Array.dedupe(Array.getSomes(Array.window(held, 2).map(([access = '', name = '']) => Option.liftPredicate(name.slice(1), (file) => WRITING.test(access) && LOG.test(file)))));
    const holders = yield* Effect.forEach(logs, (file) => Effect.map(foreign(session, file), (pids) => ({ file, holders: pids })));
    const outside = (line: string) => Option.flatMap(Option.fromNullishOr(PLUGIN.exec(line)?.groups), ({ bundle = '', name = '' }) => Option.liftPredicate(name, () => !bundle.startsWith(`${app}/`)));
    const opened: Opened = {
        settings: settings.folder ?? '',
        locale: settings.locale ?? '',
        folders: Array.dedupe(logs.map((file) => path.dirname(file))),
        withheld: holders.filter(({ holders: pids }) => pids.length > 0),
        plugins: Array.dedupe(Array.getSomes(held.map(outside))),
    };
    const shared = running.filter((row) => row.url.href === chosen.url.href && row.pid !== instance.pid && !State.guards.Exited(row.state)).map(({ pid }) => pid);
    return { session, opened, shared };
});
const batchIn = (session: Session, opened: Opened, make: (records: string) => typeof Sent.Type, folders: readonly string[], presses: readonly (readonly string[])[], limit: Duration.Duration) =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const records = yield* fs.makeTempDirectoryScoped({ prefix: 'illustrator-records-' });
        const sizes = yield* Ref.make<Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, number>>>({});
        const sized = (file: string) => Effect.map(fs.stat(file), ({ size }) => [file, globalThis.Number(size)] as const);
        const measure = Effect.map(
            Effect.forEach(opened.folders, (folder) => Effect.flatMap(filesIn(folder), Effect.forEach(sized))),
            (rows) => Record.fromEntries(rows.flat()),
        );
        yield* Effect.all([Ref.set(session.records, Option.some(records)), Ref.set(session.presses, presses), Ref.set(session.notes, [])]);
        yield* Effect.forkScoped(progress(session, records, folders, limit));
        yield* Effect.forkScoped(
            Stream.runForEach(
                Stream.filterMap(fs.watch(records), (event) =>
                    Result.fromOption(
                        Option.filter(recordOf(path.basename(event.path)), ({ kind }) => kind === 'started'),
                        () => undefined,
                    ),
                ),
                ({ at }) => Effect.flatMap(measure, (bytes) => Ref.update(sizes, Record.set(String(at), bytes))),
            ),
        );
        const start = yield* measure;
        const stopped = Effect.flatMap(Deferred.await(session.verdict), (ending) => HarnessError.make({ reason: { _tag: 'Unavailable', because: verdictText(ending) }, subject: `process ${session.pid}` }));
        const resumed = (header: typeof Header.Type, batch: typeof Sent.Type) =>
            Option.flatMap(Option.fromUndefinedOr(header.held), (from) =>
                Sent.match(batch, {
                    Survey: () => Option.none(),
                    Closed: () => Option.none(),
                    Cases: (held) => Option.some(Sent.cases.Resumed.make({ ...Struct.omit(held, ['_tag']), from, cases: held.cases.slice(from - held.from) })),
                    Resumed: (held) => Option.some(Sent.cases.Resumed.make({ ...held, from, cases: held.cases.slice(from - held.from) })),
                }),
            );
        const gated = (next: typeof Sent.Type & { readonly from: number }) =>
            Effect.raceFirst(
                answeredFor(
                    session,
                    next.from,
                    Option.getOrElse(Array.get(folders, next.from), () => records),
                    limit,
                ),
                stopped,
            );
        const phases = (batch: typeof Sent.Type): Effect.Effect<typeof Header.Type, Effect.Error<ReturnType<typeof sent>> | Effect.Error<ReturnType<typeof answeredFor>>, Effect.Services<ReturnType<typeof sent>> | Effect.Services<ReturnType<typeof answeredFor>>> =>
            Effect.flatMap(Effect.raceFirst(sent(session, batch), stopped), (header) =>
                Option.match(resumed(header, batch), {
                    onNone: () => Effect.succeed(header),
                    onSome: (next) => Effect.as(Effect.andThen(gated(next), phases(next)), header),
                }),
            );
        const reply = yield* Effect.result(phases(make(records)));
        const [{ finished, running }, notes, verdict, offsets, end] = yield* Effect.all([recorded(records), Ref.get(session.notes), Effect.flatMap(Deferred.isDone(session.verdict), (done) => (done ? Effect.asSome(Deferred.await(session.verdict)) : Effect.succeedNone)), Ref.get(sizes), measure]);
        const changed = yield* Effect.filter(Record.keys(Record.filter(end, (size, file) => size > (start[file] ?? 0))), (file) => Effect.map(foreign(session, file), (pids) => pids.length === 0));
        const texts = new Map(yield* Effect.forEach(changed, (file) => Effect.map(fs.readFile(file), (bytes) => [file, bytes] as const)));
        const logged = (index: number) =>
            changed.flatMap((file) => {
                const from = offsets[String(index)]?.[file] ?? start[file] ?? 0;
                const to = offsets[String(index + 1)]?.[file] ?? end[file] ?? 0;
                return Array.fromIterable(Text.linesIterator(new TextDecoder().decode(texts.get(file)?.slice(from, to)))).flatMap((line) => (line.trim() === '' ? [] : [{ file: path.basename(file), line }]));
            });
        return { reply, finished, running, notes, verdict, logged };
    }).pipe(Effect.scoped);
const surveyIn = Effect.fn('surveyIn')(function* (session: Session, opened: Opened, host: typeof Survey.Type.host, limit: Duration.Duration) {
    const listed = yield* enumerations(session.app);
    const { reply } = yield* batchIn(session, opened, () => Sent.cases.Survey.make({ types: listed.DocumentPresetType ?? [] }), [], [], limit);
    const header = yield* Effect.fromResult(reply);
    const survey = yield* Effect.fromOption(Option.fromUndefinedOr(header.survey), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'survey reply holds no survey' }, subject: `process ${session.pid}` }));
    return { enumerations: listed, ...survey, host };
});
const finishedIn = (rest: readonly Prepared[], ran: Effect.Success<ReturnType<typeof batchIn>>) => {
    const on = (index: number) => ran.notes.filter(([at]) => Option.contains(at, index)).map(([, note]) => note);
    const kept = rest.flatMap(({ map }, index) => Option.toArray(Option.map(Option.fromUndefinedOr(ran.finished.get(index)), (row) => ({ ...mapped(map)(row), answered: [...row.answered, ...on(index)], logs: ran.logged(index) }))));
    const stop = Option.getOrElse(ran.running, () => kept.length);
    const failure = Option.zipWith(Result.getFailure(ran.reply), Array.get(rest, stop), (error, { hosted }) => ({ name: hosted.name, folder: hosted.folder, message: error.message, answered: on(stop), logs: ran.logged(stop) }));
    const remaining = Result.isFailure(ran.reply) ? rest.slice(stop + 1) : [];
    const relaunch = Option.exists(ran.verdict, (verdict) => Verdict.$is('Exited')(verdict) || Verdict.$is('Hung')(verdict)) && remaining.length > 0;
    const unsent = relaunch ? [] : remaining.map(({ hosted }) => Outcome.make({ name: hosted.name, completion: { _tag: 'Failed', reason: 'batch ended before the case ran' } }));
    return { kept, failure, remaining, relaunch, unsent };
};
const fault =
    (limit: Duration.Duration, verdict: Option.Option<Verdict>, pid: number) =>
    ({ message, folder, ...row }: { readonly name: string; readonly folder: string; readonly message: string; readonly answered: readonly (typeof Answered.Type)[]; readonly logs: typeof Outcome.Type.logs }) =>
        Effect.map(
            Option.match(verdict, {
                onNone: () => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: message } }),
                onSome: Verdict.$match({
                    Exited: ({ exit }) => Effect.map(crashOf(exit, pid, folder), (crash) => ({ completion: { _tag: 'Failed' as const, reason: described(exit) }, crashed: crash })),
                    Hung: ({ hang }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: `hung ${hang.cpu} past ${Duration.format(limit)}` }, hung: hang }),
                    Lost: ({ error }) => Effect.succeed({ completion: { _tag: 'Failed' as const, reason: error.message } }),
                }),
            }),
            (parts) => Outcome.make({ ...row, ...parts }),
        );
const quitted = Effect.fn('quitted')(function* (session: Session, launching: Scope.Closeable, closing: typeof Sent.Type, folder: string, limit: Duration.Duration) {
    yield* (yield* Deferred.isDone(session.verdict)) ? Effect.void : Effect.ignore(sent(session, closing), { log: 'Warn', message: `closing case documents of process ${session.pid} failed` });
    yield* Scope.close(launching, Exit.void);
    const exit = yield* Deferred.await(session.gone).pipe(Effect.timeoutOrElse({ duration: limit, orElse: () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `no exit within ${Duration.format(limit)} of the quit` }, subject: `process ${session.pid}` }) }));
    return yield* crashOf(exit, session.pid, folder);
});
const execute = Effect.fn('execute')(function* <P extends Plan>(
    chosen: Installation,
    version: string,
    label: string,
    limit: Duration.Duration,
    known: (host: Option.Option<typeof Survey.Type.host>) => Option.Option<typeof Survey.Type>,
    planned: (survey: typeof Survey.Type) => Effect.Effect<P, HarnessError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path>,
) {
    interface Step {
        readonly survey: typeof Survey.Type;
        readonly plan: P;
        readonly header: Option.Option<typeof Header.Type>;
        readonly reporter: Option.Option<number>;
        readonly shared: readonly number[];
        readonly withheld: Opened['withheld'];
        readonly exit: Option.Option<typeof Crash.Type>;
        readonly answered: readonly (typeof Answered.Type)[];
        readonly outcomes: readonly (typeof Outcome.Type)[];
    }
    interface Held {
        readonly survey: Option.Option<typeof Survey.Type>;
        readonly plan: Option.Option<P>;
        readonly rest: Option.Option<readonly Prepared[]>;
    }
    const idle = (survey: typeof Survey.Type, plan: P): Step => ({ survey, plan, header: Option.none(), reporter: Option.none(), shared: [], withheld: [], exit: Option.none(), answered: [], outcomes: [] });
    const stepped = Effect.fnUntraced(function* (held: Held) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const early = Option.orElse(held.survey, () => known(Option.none()));
        const draft = yield* Option.match(held.plan, { onSome: Effect.succeedSome, onNone: () => Effect.transposeOption(Option.map(early, planned)) });
        const drained = (planned: Step) => Option.getOrElse(held.rest, () => planned.plan.sendable).length === 0;
        const idled = Option.filter(Option.zipWith(early, draft, idle), drained);
        if (Option.isSome(idled)) {
            return [[idled.value], Option.none<Held>()] as const;
        }
        const scratch = yield* fs.makeTempDirectoryScoped({ prefix: 'illustrator-' });
        const owned = path.join(scratch, `${label}.svg`);
        yield* fs.writeFileString(owned, '<svg xmlns="http://www.w3.org/2000/svg" width="1" height="1"/>');
        const launching = yield* Scope.make();
        yield* Effect.addFinalizer(() => Scope.close(launching, Exit.void));
        const { session, opened, shared } = yield* acquired(chosen, limit, owned, launching);
        const host = yield* hostOf(session, opened, version);
        const confirmed = Option.filter(early, (surveyed) => sameHost(surveyed.host, host));
        const survey = yield* Option.match(
            Option.orElse(confirmed, () => known(Option.some(host))),
            { onSome: Effect.succeed, onNone: () => surveyIn(session, opened, host, limit) },
        );
        const plan = Option.isSome(confirmed) && Option.isSome(draft) ? draft.value : yield* planned(survey);
        const rest = Option.getOrElse(held.rest, () => plan.sendable);
        const base = { survey, plan, reporter: session.reporter, shared, withheld: opened.withheld };
        const folders = rest.map(({ hosted }) => hosted.folder);
        const copying = rest.some(({ hosted }) => hosted.precondition.sends?.some(pasting) === true);
        const pressed = rest.map(({ presses }) => presses);
        const cases = rest.map(({ hosted }) => hosted);
        const saved = path.join(scratch, 'pasteboard.json');
        const color = path.join(scratch, 'color.csf');
        yield* fs.copyFile(path.join(opened.settings, COLOR_SETTINGS), color);
        yield* copying ? pasteboard('save', saved) : Effect.void;
        const ran = yield* batchIn(session, opened, (records) => Sent.cases.Cases.make({ records, label, colorSettings: color, from: 0, cases }), folders, pressed, limit);
        const { kept, failure, remaining, relaunch, unsent } = finishedIn(rest, ran);
        yield* copying && kept.flatMap(({ writes }) => writes).some(({ _tag }) => _tag === 'Pasteboard') ? pasteboard('restore', saved) : Effect.void;
        const interrupted = yield* Effect.forEach(
            Option.toArray(failure),
            fault(
                limit,
                Option.filter(ran.verdict, () => Option.isSome(ran.running)),
                session.pid,
            ),
        );
        yield* Option.exists(ran.verdict, Verdict.$is('Hung')) ? Effect.try({ try: () => kill(session.pid, 'SIGKILL'), catch: HarnessError.refused(`process ${session.pid}`) }) : Effect.void;
        const exit = yield* Effect.option(
            quitted(
                session,
                launching,
                Sent.cases.Closed.make({ label, folders: [...folders, scratch] }),
                Option.getOrElse(Array.last(folders), () => scratch),
                limit,
            ),
        );
        const step = { ...base, header: Result.getSuccess(ran.reply), exit, answered: ran.notes.filter(([at]) => Option.isNone(at)).map(([, note]) => note), outcomes: [...kept, ...interrupted, ...unsent] } satisfies Step;
        return [[step], Option.liftPredicate<Held>({ survey: Option.some(survey), plan: Option.some(plan), rest: Option.some(remaining) }, () => relaunch)] as const;
    }, Effect.scoped);
    const steps = yield* Stream.runCollect(Stream.paginate({ survey: Option.none(), plan: Option.none(), rest: Option.none() } satisfies Held, stepped));
    const first = yield* Effect.fromOption(Array.head(steps), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'no session ran' }, subject: chosen.name }));
    const outcomes = [...first.plan.settled, ...steps.flatMap(({ outcomes: held }) => held)];
    const copies = new Set(
        yield* Effect.forEach(
            first.plan.sendable.flatMap(({ hosted }) => hosted.precondition.documents.flatMap((document) => ('open' in document ? [document.open] : []))),
            (file) => Effect.map(digest(file), ({ sha256 }) => sha256),
        ),
    );
    const files = new Map(yield* Effect.forEach(first.plan.sendable, ({ hosted }) => Effect.map(writtenIn(hosted.folder, copies), (rows) => [hosted.name, rows] as const)));
    return {
        survey: first.survey,
        plan: first.plan,
        header: Array.findFirst(steps, ({ header }) => header),
        reporter: first.reporter,
        shared: Array.dedupe(steps.flatMap(({ shared }) => shared)),
        withheld: first.withheld,
        exits: Array.getSomes(steps.map(({ exit }) => exit)),
        answered: steps.flatMap(({ answered }) => answered),
        outcomes: outcomes.map((row) => ({ ...row, written: files.get(row.name) ?? [] })),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Batch, Call, conditionOf, execute, expand, installations, normalize, Probe, Results, scriptOf };
