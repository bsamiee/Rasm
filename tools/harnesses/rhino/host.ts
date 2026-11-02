import { fileURLToPath } from 'node:url';
import { State } from '@rasm/automation/applications';
import { Array, Data, Deferred, Duration, Effect, FileSystem, Match, Option, Order, Path, type PlatformError, Record, Ref, Result, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { Batch as KitBatch, Call as KitCall, Probe as KitProbe, Run as KitRun } from '#/cases';
import { HarnessError } from '#/error';
import { type Exited, Launch, settled, watch } from '#/instance';
import { type Answered, outcome, results } from '#/results';
import { merged, precondition } from '#/rhino/precondition';

// --- [TYPES] ---------------------------------------------------------------------------

type Case = (typeof Batch.Type)['cases'][number];
type Decoded = ReturnType<typeof precondition>['Type'];
type Placed = Exclude<Decoded, { readonly application: true }>;
type Kind = Data.TaggedEnum<{ Probe: { readonly row: typeof Probe.Type }; Call: { readonly row: typeof Call.Type }; Run: { readonly row: typeof Run.Type } }>;
type Mode = Data.TaggedEnum<{ Run: Record.ReadonlyRecord<never, never>; Compare: { readonly module: string } }>;
interface Hosted {
    readonly name: string;
    readonly kind: Kind['_tag'];
    readonly expression: string;
    readonly precondition: Schema.Json;
    readonly expect: NonNullable<Case['expect']>;
    readonly keys: readonly string[];
    readonly answers: NonNullable<Case['windows']>;
    readonly canvas: boolean;
    readonly row: object;
    readonly steps: string;
    readonly body: Option.Option<string>;
    readonly reads: readonly string[];
    readonly geometry: readonly string[];
    readonly locals: string;
}
interface Row {
    readonly identifier: string;
    readonly pid: number;
    readonly state: typeof State.Type;
}
type Entries = readonly (typeof Entry.Type)[];
interface Progress {
    readonly entries: Entries;
    readonly cancelled: ReadonlySet<number>;
}
type Note = Data.TaggedEnum<{ Answered: { readonly answered: typeof Answered.Type }; Unanswered: (typeof fields.unanswered.Type & {})[number] }>;
interface Dialog {
    readonly flight: Option.Option<string>;
    readonly system: boolean;
    readonly note: Note;
}
type Element = Extract<typeof State.Type, { readonly _tag: 'Responding' }>['elements'][number];
type Control = NonNullable<Element['detail']>['controls'][number];
type Hung = typeof fields.hung.Type & {};
type End = Data.TaggedEnum<{ Closed: Record.ReadonlyRecord<never, never>; Blocked: { readonly reason: string }; Crashed: { readonly exit: Exited }; Hung: Hung }>;
interface Watched {
    readonly pid: number;
    readonly journal: string;
    readonly key: string;
    readonly states: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>;
    readonly progress: SubscriptionRef.SubscriptionRef<Progress>;
    readonly limit: Duration.Duration;
    readonly captures: string;
}
interface Built {
    readonly assembly: string;
    readonly batch: string;
    readonly kept: readonly Hosted[];
    readonly dropped: readonly (typeof Outcome.Type)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PREFIX = 'com.mcneel.rhinoceros.';
const HOST = new RegExp(`^${PREFIX.replaceAll('.', String.raw`\.`)}\\d+$`, 'u');
const LOCALIZATION = /^(?<code>.+)\.lproj$/u;
const RESOURCES = ['Contents', 'Frameworks', 'RhCore.framework', 'Versions', 'Current', 'Resources'] as const;
const ENTRY = { type: 'Batch', method: 'Run' } as const;
const HARNESS = 'RASM_HARNESS';
const LINE = /\r?\n/u;
const REFUSED = ['dialogs', 'confirms', 'prompts', 'files'] as const;
const IGNORE = 'action-button-2';
const LOADER = [
    'using System;',
    'using System.IO;',
    'using System.Linq;',
    'using System.Runtime.Loader;',
    `var harness = System.Text.Json.JsonSerializer.Deserialize<string[]>(Environment.GetEnvironmentVariable(${JSON.stringify(HARNESS)}) ?? "[]") ?? Array.Empty<string>();`,
    'EventHandler run = null;',
    'run = (sender, args) => {',
    '    Rhino.RhinoApp.Idle -= run;',
    '    var resolver = new AssemblyDependencyResolver(harness[0]);',
    '    var context = new AssemblyLoadContext(harness[0], isCollectible: true);',
    '    context.Resolving += (loading, name) => resolver.ResolveAssemblyToPath(name) is string found ? loading.LoadFromStream(new MemoryStream(File.ReadAllBytes(found))) : AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(held => held.GetName().Name == name.Name);',
    '    try {',
    `        context.LoadFromStream(new MemoryStream(File.ReadAllBytes(harness[0]))).GetType(${JSON.stringify(ENTRY.type)}, throwOnError: true).GetMethod(${JSON.stringify(ENTRY.method)}).Invoke(null, new object[] { harness[1..] });`,
    '    } finally {',
    '        context.Unload();',
    '        foreach (var doc in Rhino.RhinoDoc.OpenDocuments()) doc.Modified = false;',
    '        EventHandler quit = null;',
    '        quit = (sender, args) => { Rhino.RhinoApp.Idle -= quit; Rhino.RhinoApp.Exit(false); };',
    '        Rhino.RhinoApp.Idle += quit;',
    '    }',
    '};',
    'Rhino.RhinoApp.Idle += run;',
    '',
].join('\n');

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Data.taggedEnum<Mode>();
const Kind = Data.taggedEnum<Kind>();
const End = Data.taggedEnum<End>();
const Note = Data.taggedEnum<Note>();
const rhino = <F extends Schema.Struct.Fields>(own: F) => ({
    ...Struct.omit(own, REFUSED),
    commands: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Macros run one per RunScript on the case document, each answering its own prompts' })),
});
const Probe = KitProbe.mapFields(rhino);
const Call = KitCall.mapFields(rhino);
const Run = KitRun.mapFields(rhino);
const Batch = KitBatch.mapFields((own) => ({
    ...Struct.omit(own, ['vector']),
    plugin: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Plug-in binary under the product folder, referenced and publicized at build and loaded before the cases' })),
    cases: Schema.Array(Schema.Union([Probe, Call, Run])),
}));
const Engine = Schema.Literal('csharp');
const fields = {
    beforeFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    document: Schema.optionalKey(
        Schema.Struct({
            serial: Schema.Int,
            path: Schema.optionalKey(Schema.String),
            template: Schema.optionalKey(Schema.String),
            units: Schema.Struct({ model: Schema.String, page: Schema.String }),
            tolerance: Schema.Struct({ absolute: Schema.Finite, angle: Schema.Finite, relative: Schema.Finite }),
            active: Schema.Boolean,
            modified: Schema.Boolean,
            selected: Schema.Int,
            view: Schema.optionalKey(Schema.Struct({ name: Schema.String, display: Schema.String, parallel: Schema.Boolean })),
            undo: Schema.Array(Schema.String),
        }).annotate({ description: 'Case document as the host reports it after the case' }),
    ),
    saved: Schema.optionalKey(Schema.Struct({ before: Schema.String, after: Schema.String }).annotate({ description: '.3dm files of the case document before and after the case' })),
    macros: Schema.optionalKey(Schema.Array(Schema.Struct({ text: Schema.String, completed: Schema.Boolean }))),
    commands: Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.String, plugin: Schema.optionalKey(Schema.String), result: Schema.String, document: Schema.Int })).annotate({ description: 'Every command that ended while the case ran' })),
    prompts: Schema.optionalKey(Schema.Array(Schema.Struct({ text: Schema.String, default: Schema.String, options: Schema.Array(Schema.String) }))),
    printed: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Captured command window strings' })),
    console: Schema.optionalKey(Schema.Struct({ stdout: Schema.String, stderr: Schema.String })),
    reported: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'HostUtils.OnExceptionReport exceptions Rhino swallowed' })),
    logged: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'RhinoCode logger warnings and errors' })),
    writes: Schema.optionalKey(
        Schema.Array(Schema.Struct({ kind: Schema.String, subject: Schema.String, made: Schema.optionalKey(Schema.String), archive: Schema.optionalKey(Schema.Json) })).annotate({
            description: 'Document object, layer, and material changes while macros, script, and body ran',
        }),
    ),
    diagnostics: Schema.optionalKey(Schema.Array(Schema.Struct({ time: Schema.String, kind: Schema.String, message: Schema.String })).annotate({ description: 'Grasshopper 2 logger entries' })),
    cancelled: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Prompts left waiting that the harness cancelled' })),
    unanswered: Schema.optionalKey(Schema.Array(Schema.Struct({ title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), reason: Schema.String }))),
    crashed: Schema.optionalKey(
        Schema.Struct({
            status: Schema.optionalKey(Schema.Int),
            signal: Schema.optionalKey(Schema.Int),
            report: Schema.optionalKey(Schema.String),
            exception: Schema.optionalKey(Schema.String),
            type: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type, signal, and subtype of the crash report' })),
            termination: Schema.optionalKey(Schema.Json),
            frames: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Faulting thread frames of the crash report' })),
            logged: Schema.optionalKey(Schema.String.annotate({ description: 'RhinoCode process log, the fatal exception Logger.DumpLogs wrote' })),
        }),
    ),
    hung: Schema.optionalKey(Schema.Struct({ cpu: Schema.Literals(['busy', 'blocked']), sample: Schema.String, stack: Schema.optionalKey(Schema.String.annotate({ description: 'dotnet-stack report of every managed thread' })) })),
};
const Outcome = outcome(fields);
const Engined = Schema.Struct(Struct.omit(Outcome.fields, ['answered']));
const Loaded = Schema.Struct({ name: Schema.String, version: Schema.String });
const Grasshopper = Schema.Struct({ version: Schema.String, loaded: Schema.Array(Schema.String), failed: Schema.Array(Schema.String), incomplete: Schema.optionalKey(Schema.String) });
const Environment = Schema.Struct({ version: Schema.String, runtime: Schema.String, system: Schema.String });
const Results = results(
    Engine,
    {
        failed: Schema.optionalKey(Schema.String),
        language: Schema.optionalKey(Schema.Struct({ lcid: Schema.Int, culture: Schema.String })),
        environment: Schema.optionalKey(Environment.annotate({ description: 'RhinoApp.Version, .NET runtime, and OS the engine reports' })),
        plugins: Schema.optionalKey(Schema.Array(Loaded).annotate({ description: 'Plug-ins loaded when the batch ended' })),
        plugIn: Schema.optionalKey(Schema.Json.annotate({ description: 'Batch plug-in load and whether the loaded module is the file build' })),
        grasshopper: Schema.optionalKey(Grasshopper),
    },
    fields,
);
const Entry = Schema.TaggedUnion({
    Session: { language: Schema.Int, culture: Schema.String, ...Environment.fields, logs: Schema.String, plugIn: Schema.optionalKey(Schema.Json) },
    Started: { name: Schema.String },
    Began: { name: Schema.String },
    Ended: { name: Schema.String },
    Prompted: { text: Schema.String, default: Schema.String, options: Schema.Array(Schema.String) },
    Reported: { source: Schema.String, exception: Schema.String },
    Unhandled: { exception: Schema.String },
    Finished: { outcome: Engined, grasshopper: Schema.optionalKey(Grasshopper) },
    Closed: { plugins: Schema.Array(Loaded) },
});
const Entried = Schema.fromJsonString(Entry);
const Info = Schema.fromJsonString(Schema.Struct({ CFBundleIdentifier: Schema.String, CFBundleVersion: Schema.String, CFBundleName: Schema.String, CFBundleExecutable: Schema.String }));
const Ips = Schema.fromJsonString(
    Schema.Struct({
        exception: Schema.optionalKey(Schema.Struct({ type: Schema.String, signal: Schema.optionalKey(Schema.String), subtype: Schema.optionalKey(Schema.String) })),
        termination: Schema.optionalKey(Schema.Json),
        asi: Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String))),
        faultingThread: Schema.optionalKey(Schema.Int),
        threads: Schema.optionalKey(Schema.Array(Schema.Struct({ frames: Schema.Array(Schema.Struct({ imageIndex: Schema.Int, imageOffset: Schema.Int, symbol: Schema.optionalKey(Schema.String) })) }))),
        usedImages: Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String) }))),
    }),
);
const Sarif = Schema.fromJsonString(
    Schema.Struct({
        runs: Schema.Array(
            Schema.Struct({
                results: Schema.optionalKey(
                    Schema.Array(
                        Schema.Struct({
                            ruleId: Schema.optionalKey(Schema.String),
                            level: Schema.optionalKey(Schema.String),
                            message: Schema.Struct({ text: Schema.String }),
                            suppressions: Schema.optionalKey(Schema.Array(Schema.Unknown)),
                            locations: Schema.optionalKey(Schema.Array(Schema.Struct({ physicalLocation: Schema.optionalKey(Schema.Struct({ artifactLocation: Schema.Struct({ uri: Schema.String }), region: Schema.optionalKey(Schema.Struct({ startLine: Schema.Int })) })) }))),
                        }),
                    ),
                ),
            }),
        ),
    }),
);
const Encoded = Schema.fromJsonString(Schema.Unknown);
const Harnessed = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [INSTALLS]
const spawned = Effect.fn('spawned')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'pipe' });
    const [text, failure, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return { text, failure, code };
}, Effect.scoped);
const output = Effect.fn('output')(function* (command: string, args: readonly string[]) {
    const { text, failure, code } = yield* spawned(command, args);
    return code === 0 ? text : yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `exited with code ${code} ${text}${failure}` }, subject: [command, ...args].join(' ') });
});
const lines = (text: string) => text.split(LINE).filter((line) => line.length > 0);
const present = <K extends string, A>(key: K, value: Option.Option<A>) => Option.match(value, { onNone: () => ({}), onSome: (held) => Record.singleton(key, held) });
const plist = (bundle: string) => Effect.flatMap(Path.Path, (path) => Effect.flatMap(output('plutil', ['-convert', 'json', '-o', '-', path.join(bundle, 'Contents', 'Info.plist')]), Schema.decodeEffect(Info)));
class Install extends Data.Class<{ readonly path: string; readonly bundle: string; readonly version: string; readonly name: string; readonly executable: string; readonly languages: readonly string[] }> {
    static readonly identify = Effect.fn('Install.identify')(function* (bundle: string) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const info = yield* plist(bundle);
        const localized = Array.getSomes((yield* fs.readDirectory(path.join(bundle, ...RESOURCES))).map((name) => Option.fromNullishOr(LOCALIZATION.exec(name)?.groups?.code)));
        return new Install({ path: bundle, bundle: info.CFBundleIdentifier, version: info.CFBundleVersion, name: info.CFBundleName, executable: path.join(bundle, 'Contents', 'MacOS', info.CFBundleExecutable), languages: localized.filter((code) => code !== 'Base') });
    });
    static readonly byVersion = Order.mapInput(Array.makeOrder(Order.Number), ({ version }: Install) => version.split('.').map(Number));
    static readonly all = Effect.gen(function* () {
        const found = yield* Effect.forEach(lines(yield* output('mdfind', [`kMDItemCFBundleIdentifier == '${PREFIX}*'`])), Install.identify);
        return Array.sort(
            found.filter(({ bundle }) => HOST.test(bundle)),
            Install.byVersion,
        );
    });
    static readonly listed = (rows: readonly Install[]) => rows.map(({ path, bundle, version }) => `${path} ${bundle} ${version}`).join(', ');
    static readonly chosen = (rows: readonly Install[], keys: readonly string[]) =>
        Effect.fromOption(
            Array.match(
                rows.filter((row) => keys.length === 0 || keys.some((key) => [row.bundle, row.name, row.path].includes(key) || row.version.startsWith(key))),
                { onEmpty: Option.none, onNonEmpty: Option.some },
            ),
            () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `installs are ${Install.listed(rows)}` }, subject: keys.join(', ') }),
        );
    static readonly reporting = (rows: readonly Install[], identifier: string, version: string) =>
        Effect.fromOption(
            Array.findFirst(rows, (row) => row.bundle === identifier && row.version === version),
            () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `no install reports it, installs are ${Install.listed(rows)}` }, subject: `${identifier} ${version}` }),
        );
}

// --- [PREPARE]
const located = (decoded: Decoded, product: string, join: (...parts: readonly string[]) => string): { readonly source: object; readonly placed: Option.Option<Placed> } =>
    Match.value(decoded).pipe(
        Match.when({ template: Match.string }, (held) => ({ source: { _tag: 'Template', name: held.template, units: held.units, tolerance: held.tolerance }, placed: Option.some(held) })),
        Match.when({ open: Match.string }, (held) => ({ source: { _tag: 'Open', path: join(product, held.open), units: held.units, tolerance: held.tolerance }, placed: Option.some(held) })),
        Match.orElse(() => ({ source: { _tag: 'Application' }, placed: Option.none() })),
    );
const called = (name: string) => `Engine.Call(${name}, scope.Args)`;
const facetsOf = (row: Case, mode: Mode, batch: typeof Batch.Type) => {
    const running = Mode.$is('Run')(mode);
    return Kind.$match(
        Match.value(row).pipe(
            Match.when(Schema.is(Probe), (held) => Kind.Probe({ row: held })),
            Match.when(Schema.is(Call), (held) => Kind.Call({ row: held })),
            Match.orElse((held) => Kind.Run({ row: held })),
        ),
        {
            Probe: ({ row: held }) => ({ kind: 'Probe' as const, subject: Option.some(held.evaluate), fn: Option.none<string>(), body: Option.liftPredicate(`(object?)(${held.evaluate})`, () => running), args: [], script: Option.none<string>() }),
            Call: ({ row: held }) => ({ kind: 'Call' as const, subject: Option.some(held.call), fn: Option.fromUndefinedOr(held.fn), body: running ? Option.some(called(held.call)) : Option.map(Option.fromUndefinedOr(held.fn), called), args: held.args, script: Option.none<string>() }),
            Run: ({ row: held }) => ({
                kind: 'Run' as const,
                subject: Option.none<string>(),
                fn: Option.fromUndefinedOr(held.fn),
                body: running ? Option.none<string>() : Option.map(Option.fromUndefinedOr(held.fn), called),
                args: held.args ?? [],
                script: Option.filter(Option.fromUndefinedOr(held.script ?? batch.script), () => running),
            }),
        },
    );
};
const expected = (row: Case, batch: typeof Batch.Type, kind: Hosted['kind']) => ({
    expect: row.expect ?? batch.expect ?? (kind === 'Run' ? 'read' : 'value'),
    keys: row.keys ?? batch.keys ?? [],
    answers: row.windows ?? (row.presses ?? []).map((label) => ({ presses: [label] })),
});
const localsOf = (placed: Option.Option<Placed>, canvas: boolean) =>
    [Option.isNone(placed) ? 'var made = scope.Made;' : 'var (doc, view, made) = (scope.Doc, scope.View, scope.Made);', canvas ? 'var canvas = (Grasshopper2.Doc.Document)scope.Canvas; var editor = (Grasshopper2.UI.Canvas.Canvas)scope.Editor;' : ''].join(' ');
const prepare = Effect.fn('prepare')(function* (install: Install, product: string, batch: typeof Batch.Type, only: readonly string[], mode: Mode) {
    const path = yield* Path.Path;
    const Precondition = precondition(install.languages);
    const picked = batch.cases
        .filter(({ name }) => only.length === 0 || only.includes(name))
        .map((row) => [row, facetsOf(row, mode, batch)] as const)
        .filter(([, facets]) => Mode.$is('Run')(mode) || Option.isSome(facets.fn));
    const repeated = picked.filter(([{ name }], index) => picked.findIndex(([row]) => row.name === name) !== index);
    yield* repeated.length === 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Unavailable', because: `case names repeat: ${repeated.map(([{ name }]) => name).join(', ')}` }, subject: product });
    const hosted = Effect.fn('hosted')(function* ([row, facets]: readonly [Case, ReturnType<typeof facetsOf>]) {
        const stated = merged(batch.precondition ?? {}, row.precondition ?? {});
        const decoded = yield* Schema.decodeUnknownEffect(Precondition)(stated, { onExcessProperty: 'error' });
        const { source, placed } = located(decoded, product, path.join);
        const macros = Mode.$is('Run')(mode) ? (row.commands ?? []) : [];
        yield* Option.isNone(placed) && (macros.length > 0 || Option.isSome(facets.script)) ? HarnessError.make({ reason: { _tag: 'Unavailable', because: 'states commands or a script under an application precondition, which opens no document for them' }, subject: row.name }) : Effect.void;
        const read = { ...(Option.isNone(placed) ? {} : batch.read), ...row.read };
        const add = Option.match(placed, { onNone: () => [], onSome: (held) => held.add ?? [] });
        const under = (relative: string) => path.join(product, relative);
        const canvas = Option.map(Option.fromUndefinedOr(decoded.canvas), ({ open }) => present('open', Option.map(Option.fromUndefinedOr(open), under)));
        const engine = {
            name: row.name,
            source,
            reads: Record.keys(read),
            macros,
            args: facets.args,
            language: decoded.language,
            batch: decoded.batch === true,
            add: add.map(({ as, layer }) => ({ as, layer })),
            ...Option.match(placed, { onNone: () => ({}), onSome: ({ view, layout, layers, select }) => ({ view, layout, layers, select }) }),
            ...present('canvas', canvas),
            ...present('script', Option.map(facets.script, under)),
        };
        return {
            ...expected(row, batch, facets.kind),
            name: row.name,
            kind: facets.kind,
            expression: [...Option.toArray(facets.subject), ...Option.toArray(facets.script), ...macros].join('\n'),
            precondition: stated,
            canvas: Option.isSome(canvas),
            row: engine,
            steps: (decoded.steps ?? []).join(' '),
            body: facets.body,
            reads: Record.values(read),
            geometry: add.map(({ geometry }) => geometry),
            locals: localsOf(placed, Option.isSome(canvas)),
        } satisfies Hosted;
    });
    return yield* Effect.forEach(picked, hosted);
});

// --- [BUILD]
const lambda = (row: Hosted, body: string) => `static scope => { ${row.locals} ${body} }`;
const entry = (row: Hosted) => [
    '        new Rasm.Harnesses.Rhino.Expressions(',
    `            ${lambda(row, row.steps)},`,
    `            ${Option.match(row.body, { onNone: () => 'null', onSome: (body) => lambda(row, `return ${body};`) })},`,
    `            [${row.reads.map((expression) => `\n            ${lambda(row, `return (object?)(${expression});`)}`).join(',')}],`,
    `            [${row.geometry.map((expression) => `\n            ${lambda(row, `return (GeometryBase)(${expression});`)}`).join(',')}]),`,
];
const program = (rows: readonly Hosted[]) => {
    const head = ['// <auto-generated/>', '#nullable enable', 'using Rasm.Harnesses.Rhino;', 'using Rhino;', 'using Rhino.Display;', 'using Rhino.DocObjects;', `internal static class ${ENTRY.type} {`, `    public static void ${ENTRY.method}(string[] args) => Engine.Run(args, [`];
    const blocks = rows.map((row) => entry(row).join('\n').split('\n'));
    const ends = blocks.reduce<readonly number[]>((held, block) => held.concat((held.at(-1) ?? head.length) + block.length), []);
    const spans = ends.map((end, index) => [end - (blocks[index]?.length ?? 0) + 1, end] as const);
    return { text: [...head, ...blocks.flat(), '    ], static () => new Rasm.Harnesses.Rhino.Canvases());', '}', ''].join('\n'), spans };
};
const build = (install: Install, folder: string, rows: readonly Hosted[], mode: Mode, plugin: Option.Option<string>): Effect.Effect<Built, HarnessError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [cases, batch, log] = [path.join(folder, 'Cases.cs'), path.join(folder, 'batch.json'), path.join(folder, 'build.sarif')];
        const { text, spans } = program(rows);
        yield* fs.writeFileString(cases, text);
        yield* fs.writeFileString(batch, yield* Schema.encodeEffect(Encoded)({ cases: rows.map(({ row }) => row), ...present('plugin', plugin) }));
        const properties = [
            `-p:RhinoInstallPath=${install.path}`,
            `-p:HarnessCases=${cases}`,
            `-p:ArtifactsPath=${path.join(folder, 'artifacts')}`,
            `-p:ErrorLog=${log}%2Cversion=2.1`,
            ...(Mode.$is('Compare')(mode) ? [`-p:HarnessRebuilt=${mode.module}`] : []),
            ...Option.toArray(Option.map(plugin, (file) => `-p:HarnessPlugIn=${file}`)),
        ];
        const built = yield* spawned('dotnet', ['build', path.join(import.meta.dirname, 'Rasm.Harnesses.Rhino.csproj'), '-nologo', '-v:q', ...properties, '-t:Build', '-getProperty:TargetPath']);
        return built.code === 0 ? { assembly: built.text.trim(), batch, kept: rows, dropped: [] } : yield* rebuilt(install, folder, rows, mode, plugin, { cases, log, spans, printed: `${built.text}${built.failure}` });
    });
const spanned = (spans: readonly (readonly [number, number])[]) => (line: number) => Array.findFirstIndex(spans, ([start, end]) => line >= start && line <= end);
const uncompiled =
    (name: string) =>
    (texts: readonly string[]): typeof Outcome.Type => ({ name, completion: { _tag: 'Failed', reason: `did not compile: ${texts.join('; ')}` }, answered: [], leaked: [] });
const rebuilt = Effect.fn('rebuilt')(function* (install: Install, folder: string, rows: readonly Hosted[], mode: Mode, plugin: Option.Option<string>, failed: { readonly cases: string; readonly log: string; readonly spans: readonly (readonly [number, number])[]; readonly printed: string }) {
    const fs = yield* FileSystem.FileSystem;
    const errors = (yield* Effect.flatMap(fs.readFileString(failed.log), Schema.decodeEffect(Sarif))).runs.flatMap(({ results: found }) => found ?? []).filter(({ level, suppressions }) => level === 'error' && (suppressions ?? []).length === 0);
    const owned = errors.map(({ ruleId, message, locations }) => ({
        text: `${ruleId ?? ''} ${message.text}`,
        index: Option.flatMap(
            Option.filter(Option.fromUndefinedOr(locations?.[0]?.physicalLocation), ({ artifactLocation }) => fileURLToPath(artifactLocation.uri) === failed.cases),
            ({ region }) => Option.flatMap(Option.fromUndefinedOr(region?.startLine), spanned(failed.spans)),
        ),
    }));
    const foreign = owned.filter(({ index }) => Option.isNone(index)).map(({ text }) => text);
    yield* foreign.length === 0 && owned.length > 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Unavailable', because: foreign.length === 0 ? failed.printed : foreign.join('; ') }, subject: 'Rasm.Harnesses.Rhino build' });
    const failing = Record.map(
        Array.groupBy(
            owned.flatMap(({ text, index }) => Option.toArray(Option.map(index, (held) => [String(held), text] as const))),
            ([index]) => index,
        ),
        (pairs) => pairs.map(([, text]) => text),
    );
    const next = yield* build(
        install,
        folder,
        rows.filter((_, index) => !(String(index) in failing)),
        mode,
        plugin,
    );
    return { ...next, dropped: [...next.dropped, ...rows.flatMap((row, index) => Option.toArray(Option.map(Record.get(failing, String(index)), uncompiled(row.name))))] };
});

// --- [WATCH]
const keyOf = ({ identifier, pid }: Pick<Row, 'identifier' | 'pid'>) => `${identifier} ${pid}`;
const watching = Effect.fn('watching')(function* (names: Array.NonEmptyReadonlyArray<string>, launched: Row) {
    const states = yield* SubscriptionRef.make<Record.ReadonlyRecord<string, Row>>({ [keyOf(launched)]: launched });
    yield* watch(names, launched.pid).pipe(
        Stream.runForEach((row) => SubscriptionRef.update(states, (held) => ({ ...held, [keyOf(row)]: row }))),
        Effect.forkScoped,
    );
    return states;
});
const stateOf = <S extends typeof State.Type>(states: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, key: string, done: (state: typeof State.Type) => state is S) =>
    SubscriptionRef.changes(states).pipe(
        Stream.filterMap((held) =>
            Result.fromOption(
                Option.filter(
                    Option.map(Record.get(held, key), ({ state }) => state),
                    done,
                ),
                () => undefined,
            ),
        ),
        Stream.runHead,
        Effect.flatMap(Effect.fromOption(() => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'watch ended before the state' }, subject: key }))),
    );
const parent = Effect.fn('parent')(function* (pid: number) {
    return Number((yield* spawned('ps', ['-o', 'ppid=', '-p', String(pid)])).text.trim());
});
const button = ({ role, subrole }: Control) => role === 'AXButton' && subrole === undefined;
const caseOf = (rows: readonly Hosted[], flight: Option.Option<string>) => Option.flatMap(flight, (name) => Array.findFirst(rows, (row) => row.name === name));
const choice = (element: Element, wanted: Option.Option<string>, system: boolean) => {
    const buttons = (element.detail?.controls ?? []).filter(button);
    return Option.firstSomeOf([
        Option.flatMap(wanted, (label) => Array.findFirst(buttons, (held) => held.label === label)),
        Array.findFirst(buttons, ({ attribute }) => attribute === 'AXCancelButton'),
        Option.filter(
            Array.findFirst(buttons, ({ identifier }) => identifier === IGNORE),
            () => system,
        ),
        Option.filter(Array.head(buttons), () => buttons.length === 1),
    ]);
};
const fill = (control: Control, value: Schema.Json) =>
    typeof value === 'boolean' ? Effect.asVoid(Effect.when(output('watch-apps', ['perform', control.id, 'AXPress']), Effect.succeed(control.value !== value && control.value !== Number(value)))) : Effect.asVoid(output('watch-apps', ['set', control.id, '--', String(value)]));
const field =
    (element: Element) =>
    ([key, value]: readonly [string, Schema.Json]) =>
        Option.match(
            Array.findFirst(element.detail?.controls ?? [], (control) => control.label === key || control.identifier === key),
            { onNone: () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `holds no control ${key}` }, subject: element.title ?? element.role }), onSome: (control) => fill(control, value) },
        );
const answering = Effect.fn('answering')(function* (install: Install, names: readonly string[], watched: Watched, rows: readonly Hosted[], dialogs: SubscriptionRef.SubscriptionRef<readonly Dialog[]>, blocked: Deferred.Deferred<string>) {
    const handled = yield* Ref.make<ReadonlySet<string>>(new Set());
    const owners = yield* Ref.make<ReadonlyMap<number, boolean>>(new Map());
    const requiring =
        (done: ReadonlySet<string>) =>
        ({ identifier, state }: Row) =>
            State.guards.Responding(state) ? state.elements.filter(asking(identifier, done)).map((element) => ({ element, system: !names.includes(identifier) })) : [];
    const asking = (identifier: string, done: ReadonlySet<string>) => (element: Element) =>
        element.requiresAnswer && !done.has(element.id) && (element.detail?.controls ?? []).some(button) && (names.includes(identifier) || [element.title ?? '', ...(element.detail?.text ?? [])].some((text) => text.includes(install.name)));
    const owned = Effect.fn('owned')(function* (row: Row) {
        const known = (yield* Ref.get(owners)).get(row.pid);
        const companion = row.identifier !== install.bundle && names.includes(row.identifier);
        const decided = !companion || (known ?? (yield* parent(row.pid)) === watched.pid);
        yield* Ref.update(owners, (held) => new Map([...held, [row.pid, decided]]));
        return decided;
    });
    const answer = Effect.fn('answer')(function* ({ element, system }: { readonly element: Element; readonly system: boolean }) {
        const flight = flightOf((yield* SubscriptionRef.get(watched.progress)).entries);
        const used = (yield* SubscriptionRef.get(dialogs)).filter((held) => Option.getOrUndefined(held.flight) === Option.getOrUndefined(flight)).length;
        const reply = Option.flatMap(caseOf(rows, flight), ({ answers }) => Array.get(answers, used));
        const header = { ...(element.title === undefined ? {} : { title: element.title }), text: element.detail?.text ?? [] };
        const unanswered = (reason: string) => Effect.andThen(SubscriptionRef.update(dialogs, Array.append({ flight, system, note: Note.Unanswered({ ...header, reason }) })), Option.isNone(flight) ? Deferred.succeed(blocked, `${header.title ?? header.text.join(' ')} ${reason}`) : Effect.void);
        yield* Ref.update(handled, (held) => new Set([...held, element.id]));
        const filled = Effect.forEach(
            Record.toEntries(
                Option.getOrElse(
                    Option.flatMapNullishOr(reply, ({ fields: given }) => given),
                    () => ({}),
                ),
            ),
            field(element),
        );
        yield* Option.match(
            choice(
                element,
                Option.flatMap(reply, ({ presses }) => Array.head(presses ?? [])),
                system,
            ),
            {
                onNone: () => unanswered('holds no cancel or sole button, and no case names one'),
                onSome: (control) =>
                    Effect.andThen(filled, output('watch-apps', ['perform', control.id, 'AXPress'])).pipe(
                        Effect.andThen(SubscriptionRef.update(dialogs, Array.append({ flight, system, note: Note.Answered({ answered: { ...header, pressed: control.label ?? control.id } }) }))),
                        Effect.catch((error) => unanswered(error.message)),
                    ),
            },
        );
    });
    yield* Stream.merge(
        Stream.map(SubscriptionRef.changes(watched.states), () => true),
        Stream.map(SubscriptionRef.changes(watched.progress), () => true),
    ).pipe(
        Stream.mapEffect(
            Effect.fnUntraced(function* () {
                const [held, { entries }, done] = yield* Effect.all([SubscriptionRef.get(watched.states), SubscriptionRef.get(watched.progress), Ref.get(handled)]);
                const kept = yield* Effect.filter(Record.values(held), owned);
                return (entries.some(Entry.guards.Session) ? kept : []).flatMap(requiring(done));
            }),
        ),
        Stream.runForEach((elements) => Effect.forEach(elements, answer, { discard: true })),
        Effect.forkScoped,
    );
});

// --- [PROGRESS]
const flightOf = (entries: Entries) =>
    Option.flatMap(
        Array.findLast(entries, (held) => Entry.guards.Started(held) || Entry.guards.Finished(held)),
        (held) => (Entry.guards.Started(held) ? Option.some(held.name) : Option.none()),
    );
const sinceStart = (entries: Entries) => entries.slice(Option.getOrElse(Array.findLastIndex(entries, Entry.guards.Started), () => 0));
const pending = ({ entries, cancelled }: Progress) =>
    Option.filter(
        Option.flatMap(Array.last(entries), (held) => (Entry.guards.Prompted(held) ? Option.some([entries.length - 1, held.text] as const) : Option.none())),
        ([index]) => !cancelled.has(index) && Option.isSome(flightOf(entries)),
    );
const cancelledOf = (entries: Entries, cancelled: ReadonlySet<number>) => Array.groupBy(Array.getSomes(entries.map((held, index) => (Entry.guards.Prompted(held) && cancelled.has(index) ? Option.map(flightOf(entries.slice(0, index)), (name) => [name, held.text] as const) : Option.none()))), ([name]) => name);
const journaled = Effect.fn('journaled')(function* (file: string, progress: SubscriptionRef.SubscriptionRef<Progress>) {
    const fs = yield* FileSystem.FileSystem;
    const complete = (yield* fs.readFileString(file)).split('\n').slice(0, -1);
    const entries = yield* Effect.forEach(complete, (line) => Schema.decodeEffect(Entried)(line));
    yield* SubscriptionRef.update(progress, (held) => ({ ...held, entries }));
});
const tail = Effect.fn('tail')(function* (file: string, progress: SubscriptionRef.SubscriptionRef<Progress>) {
    const fs = yield* FileSystem.FileSystem;
    yield* Stream.runForEach(fs.watch(file), () => journaled(file, progress));
    return yield* Effect.never;
});
const hang = Effect.fn('hang')(function* (watched: Watched) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const busy = Option.exists(Record.get(yield* SubscriptionRef.get(watched.states), watched.key), ({ state }) => state._tag === 'Unresponsive' && state.process === 'running');
    const [sample, stack] = [path.join(watched.captures, `${watched.pid}.sample.txt`), path.join(watched.captures, `${watched.pid}.stack.txt`)];
    yield* output('sample', [String(watched.pid), '1', '-file', sample]);
    const reported = yield* Effect.option(
        Effect.as(
            Effect.flatMap(output('dotnet', ['dnx', '-y', 'dotnet-stack', '--', 'report', '-p', String(watched.pid)]), (text) => fs.writeFileString(stack, text)),
            stack,
        ),
    );
    return { cpu: busy ? ('busy' as const) : ('blocked' as const), sample, ...Option.match(reported, { onNone: () => ({}), onSome: (held) => ({ stack: held }) }) } satisfies Hung;
});
const decide = Effect.fn('decide')(function* (watched: Watched, held: Progress) {
    const waiting = pending(held);
    return Option.isSome(waiting)
        ? yield* Effect.as(
              Effect.andThen(
                  Effect.flatMap(FileSystem.FileSystem, (fs) => fs.writeFileString(`${watched.journal}.cancel`, String(waiting.value[0]))),
                  SubscriptionRef.update(watched.progress, (current) => ({ ...current, cancelled: new Set([...current.cancelled, waiting.value[0]]) })),
              ),
              Option.none<End>(),
          )
        : Option.some(End.Hung(yield* hang(watched)));
});
const stalled = Effect.fn('stalled')(function* (watched: Watched) {
    const ended = yield* SubscriptionRef.changes(watched.progress).pipe(
        Stream.changesWith((left, right) => left.entries.length === right.entries.length && left.cancelled.size === right.cancelled.size),
        Stream.switchMap((held) => Stream.fromEffect(Effect.as(Effect.sleep(watched.limit), held))),
        Stream.mapEffect((held) => decide(watched, held)),
        Stream.filterMap((held) => Result.fromOption(held, () => undefined)),
        Stream.runHead,
    );
    return yield* Effect.fromOption(ended, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: 'progress ended' }, subject: watched.key }));
});

// --- [OUTCOMES]
const typed = ({ type, signal, subtype }: NonNullable<(typeof Ips.Type)['exception']>) => [type, signal, subtype].filter((part) => part !== undefined).join(' ');
const framesOf = ({ faultingThread, threads, usedImages }: typeof Ips.Type) => Option.map(Array.get(threads ?? [], faultingThread ?? 0), ({ frames }) => frames.map(({ imageIndex, imageOffset, symbol }) => `${usedImages?.[imageIndex]?.name ?? imageIndex} ${symbol ?? `+${imageOffset}`}`));
const crashOf = Effect.fn('crashOf')(function* (exit: Exited, entries: Entries, pid: number) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const since = sinceStart(entries);
    const exception = Option.orElse(
        Option.map(Array.findLast(since, Entry.guards.Unhandled), ({ exception: text }) => text),
        () => Option.map(Array.findLast(since, Entry.guards.Reported), ({ source: origin, exception: text }) => `${origin}: ${text}`),
    );
    const report = Option.map(Option.fromUndefinedOr(exit.report), ({ url }) => fileURLToPath(url));
    const decoded = yield* Option.match(report, {
        onNone: () => Effect.succeed(Option.none<typeof Ips.Type>()),
        onSome: (file) => Effect.option(Effect.flatMap(fs.readFileString(file), (text) => Schema.decodeEffect(Ips)(text.slice(text.indexOf('\n') + 1)))),
    });
    const logs = Option.map(Array.findFirst(entries, Entry.guards.Session), ({ logs: folder }) => path.join(folder, `rhinocode_${pid}`));
    const logged = yield* Option.match(logs, {
        onNone: () => Effect.succeed(Option.none<string>()),
        onSome: (file) =>
            Effect.map(
                Effect.option(fs.readFileString(file)),
                Option.filter((text) => text.trim() !== ''),
            ),
    });
    return {
        ...(exit.status === undefined ? {} : { status: exit.status }),
        ...(exit.signal === undefined ? {} : { signal: exit.signal }),
        ...present('report', report),
        ...present('exception', exception),
        ...present(
            'type',
            Option.flatMap(decoded, ({ exception: held }) => Option.map(Option.fromUndefinedOr(held), typed)),
        ),
        ...present(
            'termination',
            Option.flatMap(decoded, ({ termination, asi }) => (termination === undefined && asi === undefined ? Option.none() : Option.some<Schema.Json>({ ...(termination === undefined ? {} : { termination }), ...(asi === undefined ? {} : { asi }) }))),
        ),
        ...present('frames', Option.flatMap(decoded, framesOf)),
        ...present('logged', logged),
    };
});
const interrupted = Effect.fn('interrupted')(function* (end: End, entries: Entries, name: string, pid: number) {
    const base = { name, leaked: [], answered: [] } as const;
    return yield* End.$match(end, {
        Closed: () => Effect.succeed<typeof Outcome.Type>({ ...base, completion: { _tag: 'Failed', reason: 'batch closed before the case finished' } }),
        Blocked: ({ reason }) => Effect.succeed<typeof Outcome.Type>({ ...base, completion: { _tag: 'Failed', reason } }),
        Hung: (hung) => Effect.succeed<typeof Outcome.Type>({ ...base, completion: { _tag: 'Failed', reason: `hung ${hung.cpu}` }, hung: Struct.omit(hung, ['_tag']) }),
        Crashed: ({ exit }) => Effect.map(crashOf(exit, entries, pid), (crashed): typeof Outcome.Type => ({ ...base, completion: { _tag: 'Failed', reason: 'crashed' }, crashed })),
    });
});
const headerOf = (entries: Entries) => ({
    ...Option.match(Array.findFirst(entries, Entry.guards.Session), {
        onNone: () => ({}),
        onSome: ({ language, culture, plugIn, ...rest }) => ({ language: { lcid: language, culture }, environment: Struct.pick(rest, ['version', 'runtime', 'system']), ...(plugIn === undefined ? {} : { plugIn }) }),
    }),
    ...present('grasshopper', Array.head(Array.getSomes(entries.map((held) => (Entry.guards.Finished(held) ? Option.fromUndefinedOr(held.grasshopper) : Option.none()))))),
    ...present(
        'plugins',
        Option.map(Array.findLast(entries, Entry.guards.Closed), ({ plugins }) => plugins),
    ),
});

// --- [SESSION]
const session = Effect.fn('session')(function* (install: Install, companions: readonly string[], built: Built, folder: string, captures: string, remaining: readonly number[], limit: Duration.Duration, dialogs: SubscriptionRef.SubscriptionRef<readonly Dialog[]>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const home = yield* fs.makeTempDirectoryScoped({ directory: folder, prefix: 'session-' });
    const journal = path.join(home, 'journal.jsonl');
    yield* fs.writeFileString(journal, '');
    const launched = yield* Launch({
        app: install.path,
        fresh: true,
        arguments: [`-runscript=_-ScriptEditor _Run "${path.join(folder, 'loader.cs')}"`],
        environment: { [HARNESS]: yield* Schema.encodeEffect(Harnessed)([built.assembly, journal, captures, built.batch, ...remaining.map(String)]) },
    });
    const { pid } = launched;
    const names = [install.bundle, ...companions] as const;
    const key = keyOf({ identifier: install.bundle, pid });
    const watched: Watched = { pid, journal, key, states: yield* watching(names, launched), progress: yield* SubscriptionRef.make<Progress>({ entries: [], cancelled: new Set() }), limit, captures };
    const blocked = yield* Deferred.make<string>();
    const alerts = (held: readonly Dialog[]) => held.filter(({ system }) => system).length;
    const seen = alerts(yield* SubscriptionRef.get(dialogs));
    yield* answering(
        install,
        names,
        watched,
        built.kept.filter((_, index) => remaining.includes(index)),
        dialogs,
        blocked,
    );
    const closed = Effect.as(
        SubscriptionRef.changes(watched.progress).pipe(
            Stream.filter((held) => held.entries.some(Entry.guards.Closed)),
            Stream.runHead,
        ),
        End.Closed(),
    );
    const crashed = Effect.map(stateOf(watched.states, key, settled), (exit) => End.Crashed({ exit }));
    const raced = Effect.raceAll([closed, crashed, stalled(watched), Effect.map(Deferred.await(blocked), (reason) => End.Blocked({ reason }))]);
    const raw = yield* Effect.raceFirst(tail(journal, watched.progress), raced);
    yield* journaled(journal, watched.progress);
    const end = End.$is('Crashed')(raw) && raw.exit.status === 0 && (yield* SubscriptionRef.get(watched.progress)).entries.some(Entry.guards.Closed) ? End.Closed() : raw;
    const quit = End.$is('Closed')(end) ? yield* Effect.timeoutOption(stateOf(watched.states, key, settled), limit) : Option.some(undefined);
    const stuck = yield* Option.match(quit, { onSome: () => Effect.succeed(Option.none<Hung>()), onNone: () => Effect.asSome(hang(watched)) });
    yield* End.$is('Crashed')(end) ? Effect.timeoutOption(Stream.runHead(Stream.filter(SubscriptionRef.changes(dialogs), (held) => alerts(held) > seen)), limit) : Effect.void;
    const { entries, cancelled } = yield* SubscriptionRef.get(watched.progress);
    const prompts = cancelledOf(entries, cancelled);
    const left = (name: string) =>
        Option.map(
            Record.get(prompts, name),
            Array.map(([, text]: readonly [string, string]) => text),
        );
    const finished = entries.filter(Entry.guards.Finished).map(({ outcome: held }) => ({ ...held, answered: [], ...present('cancelled', left(held.name)) }));
    const flight = yield* Option.match(flightOf(entries), { onNone: () => Effect.succeed([]), onSome: (name) => Effect.map(interrupted(end, entries, name, pid), Array.of) });
    return {
        finished: [...finished, ...flight],
        failed: Option.firstSomeOf([
            End.$is('Blocked')(end) ? Option.some(end.reason) : Option.none(),
            Option.map(stuck, ({ cpu, sample }) => `process ${pid} did not quit within ${Duration.format(limit)}, ${cpu}, sampled to ${sample}`),
            finished.length === 0 && flight.length === 0 ? Option.some(`${end._tag} before any case started`) : Option.none(),
        ]),
        header: headerOf(entries),
    };
}, Effect.scoped);
const restOf = (kept: readonly Hosted[], remaining: readonly number[], finished: readonly { readonly name: string }[]) => {
    const done = new Set(finished.map(({ name }) => name));
    return remaining.filter((index) => Option.exists(Array.get(kept, index), ({ name }) => !done.has(name)));
};
const execute = Effect.fn('execute')(function* (install: Install, rows: readonly Hosted[], mode: Mode, captures: string, limit: Duration.Duration, plugin: Option.Option<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped({ prefix: 'rhino-harness-' });
    const built = yield* build(install, folder, rows, mode, plugin);
    yield* fs.writeFileString(path.join(folder, 'loader.cs'), LOADER);
    const apps = lines(yield* output('fd', ['--type', 'directory', '--case-sensitive', '--glob', '--no-ignore', '*.app', path.join(install.path, 'Contents')]));
    const companions = Array.dedupe(yield* Effect.forEach(apps, (app) => Effect.map(plist(app), ({ CFBundleIdentifier }) => CFBundleIdentifier))).filter((bundle) => bundle !== install.bundle);
    const dialogs = yield* SubscriptionRef.make<readonly Dialog[]>([]);
    const indices: readonly number[] = built.kept.map((_, index) => index);
    yield* fs.makeDirectory(captures, { recursive: true });
    const pages = yield* Stream.runCollect(
        Stream.paginate(indices, (remaining: readonly number[]) =>
            Effect.map(session(install, companions, built, folder, captures, remaining, limit, dialogs), (page) => {
                const rest = restOf(built.kept, remaining, page.finished);
                return [[page], Option.isSome(page.failed) || rest.length === 0 ? Option.none<readonly number[]>() : Option.some(rest)] as const;
            }),
        ),
    );
    const noted = yield* SubscriptionRef.get(dialogs);
    const of = (flight: Option.Option<string>) => noted.filter((held) => Option.getOrUndefined(held.flight) === Option.getOrUndefined(flight));
    const answered = (flight: Option.Option<string>) => of(flight).flatMap(({ note }) => (Note.$is('Answered')(note) ? [note.answered] : []));
    const unanswered = (name: string) => of(Option.some(name)).flatMap(({ note }) => (Note.$is('Unanswered')(note) ? [Struct.omit(note, ['_tag'])] : []));
    const outcomes = [...pages.flatMap(({ finished }) => finished), ...built.dropped];
    return {
        application: install.name,
        identifier: install.bundle,
        version: install.version,
        engine: Engine.literal,
        ...pages[0]?.header,
        ...present(
            'plugins',
            Option.flatMap(Array.last(pages), ({ header }) => Option.fromUndefinedOr(header.plugins)),
        ),
        ...present('failed', Option.firstSomeOf(pages.map(({ failed }) => failed))),
        answered: answered(Option.none()),
        cases: Array.getSomes(rows.map(({ name }) => Array.findFirst(outcomes, (held) => held.name === name))).map((row) => ({ ...row, answered: answered(Option.some(row.name)), ...(unanswered(row.name).length === 0 ? {} : { unanswered: unanswered(row.name) }) })),
    } satisfies typeof Results.Type;
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Batch, Engine, execute, type Hosted, Install, Mode, Outcome, prepare, Results };
