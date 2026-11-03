import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { Array, Deferred, type Duration, Effect, FileSystem, identity, Option, Path, type PlatformError, Record, Result, Schema, Stream, Struct, UndefinedOr } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/error';
import { Closed, Engine, HostCase, Info, type Placed, REFUSED, Recorded, Request, Rows, Sarif, Session, Source, Survey } from '#/rhino/contract';
import { precondition } from '#/rhino/precondition';
import { type Batch, execute, type Host as Harness, type Installed, type Live, type Prepared, type Rebuilt, recorded } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Case = typeof HostCase.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PREFIX = 'com.mcneel.rhinoceros.';
const HOST = new RegExp(`^${PREFIX.replaceAll('.', String.raw`\.`)}\\d+$`, 'u');
const HARNESS = 'RASM_HARNESS';
const SESSION = 'session.json';
const REQUEST = 'request';
const ENTRY = { type: 'Cases', method: 'Run' } as const;
const PROGRESS = [{ title: 'Please Wait...', buttons: ['Cancel'] }] as const;
const AWAKE = ['-NSAppSleepDisabled', 'YES'] as const;
const LINE = /\r?\n/u;
const PROJECT = fileURLToPath(new URL('Rasm.Harnesses.Rhino.csproj', import.meta.url));
const LOADER = [
    'using System;',
    'using System.Globalization;',
    'using System.IO;',
    'using System.Linq;',
    'using System.Runtime.InteropServices;',
    'using System.Runtime.Loader;',
    'using System.Text.Json;',
    'using Rhino;',
    'using Rhino.UI;',
    '',
    `var scratch = Environment.GetEnvironmentVariable(${JSON.stringify(HARNESS)}) ?? throw new InvalidOperationException(${JSON.stringify(`${HARNESS} names no folder`)});`,
    'var pid = Environment.ProcessId;',
    'var session = new {',
    '    application = RhinoApp.Name,',
    '    version = RhinoApp.Version.ToString(),',
    '    survey = new {',
    '        language = new { lcid = Localization.CurrentLanguageId, culture = CultureInfo.GetCultureInfo(Localization.CurrentLanguageId).Name },',
    '        environment = new { version = RhinoApp.Version.ToString(), runtime = RuntimeInformation.FrameworkDescription, system = RuntimeInformation.OSDescription },',
    '    },',
    '};',
    'File.WriteAllText(Path.Combine(scratch, $"{pid}.tmp"), JsonSerializer.Serialize(session));',
    `File.Move(Path.Combine(scratch, $"{pid}.tmp"), Path.Combine(scratch, $"{pid}.${SESSION}"));`,
    `var watcher = new FileSystemWatcher(scratch, $"{pid}.*.${REQUEST}") { EnableRaisingEvents = true };`,
    `AppDomain.CurrentDomain.SetData(${JSON.stringify(HARNESS)}, watcher);`,
    'FileSystemEventHandler arrived = (sender, args) => RhinoApp.InvokeOnUiThread((Action<string>)Run, args.FullPath);',
    'watcher.Created += arrived;',
    'watcher.Renamed += (sender, args) => arrived(sender, args);',
    '',
    'static void Run(string request) {',
    '    if (!File.Exists(request)) return;',
    '    var paths = JsonSerializer.Deserialize<string[]>(File.ReadAllText(request)) ?? Array.Empty<string>();',
    '    File.Delete(request);',
    '    if (paths.Length == 0) {',
    '        foreach (var doc in RhinoDoc.OpenDocuments()) doc.Modified = false;',
    '        RhinoApp.Exit(false);',
    '        return;',
    '    }',
    '    var resolver = new AssemblyDependencyResolver(paths[0]);',
    '    var context = new AssemblyLoadContext(paths[0], isCollectible: true);',
    '    context.Resolving += (loading, name) => resolver.ResolveAssemblyToPath(name) is string found ? loading.LoadFromStream(new MemoryStream(File.ReadAllBytes(found))) : AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(held => held.GetName().Name == name.Name);',
    '    try {',
    `        context.LoadFromStream(new MemoryStream(File.ReadAllBytes(paths[0]))).GetType(${JSON.stringify(ENTRY.type)}, throwOnError: true).GetMethod(${JSON.stringify(ENTRY.method)}).Invoke(null, new object[] { paths[1..] });`,
    '    } finally {',
    '        context.Unload();',
    '    }',
    '}',
    '',
].join('\n');

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESSES]
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

// --- [INSTALLATIONS]
const plist = Effect.fn('plist')(function* (bundle: string) {
    const path = yield* Path.Path;
    const printed = yield* output('plutil', ['-convert', 'json', '-o', '-', path.join(bundle, 'Contents', 'Info.plist')]);
    return yield* Schema.decodeEffect(Info)(printed);
});
const companions = Effect.fn('companions')(function* (bundle: string, identifier: string) {
    const path = yield* Path.Path;
    const apps = lines(yield* output('fd', ['--type', 'directory', '--case-sensitive', '--glob', '--no-ignore', '*.app', path.join(bundle, 'Contents')]));
    return Array.dedupe(yield* Effect.forEach(apps, (app) => Effect.map(plist(app), ({ CFBundleIdentifier }) => CFBundleIdentifier))).filter((held) => held !== identifier);
});
const installed = Effect.gen(function* () {
    const bundles = lines(yield* output('mdfind', [`kMDItemCFBundleIdentifier == '${PREFIX}*'`]));
    const found = yield* Effect.forEach(bundles, (bundle) => Effect.map(plist(bundle), (info) => [bundle, info] as const));
    return yield* Effect.forEach(
        found.filter(([, { CFBundleIdentifier }]) => HOST.test(CFBundleIdentifier)),
        ([bundle, info]) => Effect.map(companions(bundle, info.CFBundleIdentifier), (watched): Installed => ({ identifier: info.CFBundleIdentifier, name: info.CFBundleName, app: bundle, version: info.CFBundleVersion, watched: [info.CFBundleIdentifier, ...watched] })),
    );
});

// --- [FILES]
const appeared = Effect.fn('appeared')(function* (folder: string, name: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = path.join(folder, name);
    yield* Stream.concat(
        Stream.make(undefined),
        Stream.map(fs.watch(folder), () => undefined),
    ).pipe(
        Stream.mapEffect(() => fs.exists(file)),
        Stream.filter(identity),
        Stream.runHead,
    );
    return yield* fs.readFileString(file);
});
const requested = Effect.fn('requested')(function* (scratch: string, stem: string, paths: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const draft = path.join(scratch, `${stem}.tmp`);
    yield* fs.writeFileString(draft, yield* Schema.encodeEffect(Request)(paths));
    yield* fs.rename(draft, path.join(scratch, `${stem}.${REQUEST}`));
});
const digest = Effect.fn('digest')(function* (file: string) {
    const bytes = yield* (yield* FileSystem.FileSystem).readFile(file);
    return { file, size: bytes.byteLength, sha256: createHash('sha256').update(bytes).digest('hex') };
});

// --- [CASES]
const placed = (decoded: typeof Placed.Type, _folder: string, product: string) =>
    Effect.map(Path.Path, (path) => ({
        precondition: {
            ...decoded,
            source: Source.match(decoded.source, { Template: identity, Open: (held) => ({ ...held, path: path.join(product, held.path) }), Application: identity }),
            ...(decoded.canvas?.open === undefined ? {} : { canvas: { open: path.join(product, decoded.canvas.open) } }),
        },
        copies: [],
    }));
const refusal = (_: typeof Engine.Type, hosted: Case) =>
    Option.firstSomeOf([
        Option.map(
            Array.findFirst(REFUSED, (key) => hosted[key] !== undefined),
            (key) => `answers ${key}, which no Rhino case shows`,
        ),
        Option.map(
            Option.liftPredicate(hosted, (held) => held.precondition.source._tag === 'Application' && ((held.commands ?? []).length > 0 || held._tag === 'Run')),
            () => 'states commands or a script under an application precondition, which opens no document for them',
        ),
    ]);

// --- [BUILD]
const called = (name: string) => `Engine.Call(${name}, scope.Args)`;
const lambda = (locals: string, body: string) => `static scope => { ${locals} ${body} }`;
const bodyOf = (hosted: Case) => Option.orElse(Option.map(Option.fromUndefinedOr(hosted.fn), called), () => HostCase.match(hosted, { Probe: ({ evaluate }) => Option.some(`(object?)(${evaluate})`), Call: ({ call }) => Option.some(called(call)), Run: () => Option.none<string>() }));
const entry = (hosted: Case) => {
    const locals = [
        hosted.precondition.source._tag === 'Application' ? 'var made = scope.Made;' : 'var (doc, made) = (scope.Doc, scope.Made); var view = doc.Views.ActiveView;',
        hosted.precondition.canvas === undefined ? '' : 'var canvas = (Grasshopper2.Doc.Document)scope.Shown.Document; var editor = (Grasshopper2.UI.Canvas.Canvas)scope.Shown.Canvases.Editor;',
    ].join(' ');
    return [
        '        new Rasm.Harnesses.Rhino.Expressions.Compiled(',
        `            ${lambda(locals, (hosted.precondition.steps ?? []).join(' '))},`,
        `            ${Option.match(bodyOf(hosted), { onNone: () => 'null', onSome: (body) => lambda(locals, `return ${body};`) })},`,
        `            [${Object.values(hosted.read)
            .map((expression) => `\n            ${lambda(locals, `return (object?)(${expression});`)}`)
            .join(',')}],`,
        `            [${(hosted.precondition.add ?? []).map(({ geometry }) => `\n            ${lambda(locals, `return (GeometryBase)(${geometry});`)}`).join(',')}]),`,
    ];
};
const program = (blocks: readonly (readonly string[])[]) => {
    const head = ['// <auto-generated/>', '#nullable enable', 'using Rasm.Harnesses.Rhino;', 'using Rhino;', 'using Rhino.Display;', 'using Rhino.DocObjects;', `internal static class ${ENTRY.type} {`, `    public static void ${ENTRY.method}(string[] args) => Engine.Run(args, [`];
    const ends = Array.drop(
        Array.scan(blocks, head.length, (end, block) => end + block.length),
        1,
    );
    const spans = ends.map((end, index) => [end - (blocks[index]?.length ?? 0) + 1, end] as const);
    return { text: [...head, ...blocks.flat(), '    ], static () => new Rasm.Harnesses.Rhino.Canvases());', '}', ''].join('\n'), spans };
};
const spanned = (spans: readonly (readonly [number, number])[]) => (line: number) => Array.findFirstIndex(spans, ([start, end]) => line >= start && line <= end);
const failing = Effect.fn('failing')(function* (log: string, source: string, spans: readonly (readonly [number, number])[], printed: string) {
    const fs = yield* FileSystem.FileSystem;
    const errors = (yield* Effect.flatMap(fs.readFileString(log), Schema.decodeEffect(Sarif))).runs.flatMap(({ results }) => results ?? []).filter(({ level, suppressions }) => level === 'error' && (suppressions ?? []).length === 0);
    const owned = errors.map(({ ruleId, message, locations }) => ({
        text: `${ruleId ?? ''} ${message.text}`,
        index: Option.flatMap(
            Option.filter(Option.fromUndefinedOr(locations?.[0]?.physicalLocation), ({ artifactLocation }) => fileURLToPath(artifactLocation.uri) === source),
            ({ region }) => Option.flatMap(Option.fromUndefinedOr(region?.startLine), spanned(spans)),
        ),
    }));
    const foreign = owned.filter(({ index }) => Option.isNone(index)).map(({ text }) => text);
    yield* foreign.length === 0 && owned.length > 0 ? Effect.void : HarnessError.make({ reason: { _tag: 'Unavailable', because: foreign.length === 0 ? printed : foreign.join('; ') }, subject: `${ENTRY.type} build` });
    return Record.map(
        Array.groupBy(Array.getSomes(owned.map(({ text, index }) => Option.map(index, (held) => [String(held), text] as const))), ([index]) => index),
        (pairs): string => `did not compile: ${pairs.map(([, text]) => text).join('; ')}`,
    );
});
const building = (chosen: Installed, folder: string, cases: readonly Case[], module: Option.Option<string>, plugin: Option.Option<string>) => {
    const attempt: (reasons: Record.ReadonlyRecord<string, string>) => Effect.Effect<string, HarnessError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> = Effect.fnUntraced(function* (reasons: Record.ReadonlyRecord<string, string>) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [source, log] = [path.join(folder, `${ENTRY.type}.cs`), path.join(folder, 'build.sarif')];
        const { text, spans } = program(cases.map((hosted, index) => Option.match(Record.get(reasons, String(index)), { onNone: () => entry(hosted), onSome: (reason) => [`        new Rasm.Harnesses.Rhino.Expressions.Uncompiled(${JSON.stringify(reason)}),`] })));
        yield* fs.writeFileString(source, text);
        const properties = [
            `-p:RhinoInstallPath=${chosen.app}`,
            `-p:HarnessCases=${source}`,
            `-p:ArtifactsPath=${path.join(folder, 'artifacts')}`,
            `-p:ErrorLog=${log}%2Cversion=2.1`,
            ...Option.toArray(Option.map(module, (file) => `-p:HarnessRebuilt=${file}`)),
            ...Option.toArray(Option.map(plugin, (file) => `-p:HarnessPlugIn=${file}`)),
        ];
        const built = yield* spawned('dotnet', ['build', PROJECT, '-nologo', '-v:q', ...properties, '-t:Build', '-getProperty:TargetPath']);
        return built.code === 0 ? built.text.trim() : yield* Effect.flatMap(failing(log, source, spans, `${built.text}${built.failure}`), (found) => attempt({ ...reasons, ...found }));
    });
    return attempt({});
};

// --- [TRANSPORT]
const launch = Effect.fn('launch')(function* (_: Installed, scratch: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const loader = path.join(scratch, 'loader.cs');
    yield* fs.writeFileString(loader, LOADER);
    return { arguments: ['-nosplash', `-runscript=_-ScriptEditor _Run "${loader}"`, ...AWAKE], environment: { [HARNESS]: scratch } };
});
const sessionOf = (live: Live) => Effect.flatMap(appeared(live.scratch, `${live.pid}.${SESSION}`), Schema.decodeEffect(Session));
const opened = (live: Live) =>
    Effect.raceFirst(
        Effect.map(sessionOf(live), ({ survey }) => Option.some(survey)),
        Effect.flatMap(Deferred.await(live.verdict), (over) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `instance ${live.pid} ${over._tag.toLowerCase()} before the loader wrote ${SESSION}` }, subject: live.chosen.name })),
    );
const ran = Effect.fn('ran')(function* (live: Live, records: string, cases: Array.NonEmptyReadonlyArray<Case>, module: Option.Option<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.dirname(records);
    const plugin = Array.head(Array.getSomes(cases.map(({ plugin: file }) => Option.fromUndefinedOr(file))));
    const assembly = yield* building(live.chosen, folder, cases, module, plugin);
    const batch = path.join(folder, 'batch.json');
    const row = (hosted: Case) => ({
        name: hosted.name,
        folder: hosted.folder,
        precondition: hosted.precondition,
        reads: Record.keys(hosted.read),
        macros: hosted.fn === undefined ? (hosted.commands ?? []) : [],
        args: HostCase.match(hosted, { Probe: () => [], Call: ({ args }) => args, Run: ({ args }) => args }),
        ...(hosted._tag === 'Run' && hosted.fn === undefined ? { script: hosted.source.script } : {}),
    });
    yield* fs.writeFileString(batch, yield* Schema.encodeEffect(Rows)({ cases: cases.map(row), ...present('plugin', plugin) }));
    yield* requested(live.scratch, `${live.pid}.${path.basename(folder)}`, [assembly, batch, records]);
    const closed = yield* Effect.flatMap(appeared(folder, 'reply.json'), Schema.decodeEffect(Closed));
    return yield* UndefinedOr.match(closed.failed, {
        onUndefined: () => Effect.succeed(Struct.omit(closed, ['failed'])),
        onDefined: (because) => HarnessError.make({ reason: { _tag: 'Unavailable', because }, subject: `${ENTRY.type} batch` }),
    });
});
const send = (module: Option.Option<string>) =>
    Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, Case>) {
        const session = yield* sessionOf(live);
        const state = yield* Array.match(batch.cases, { onEmpty: () => Effect.succeedNone, onNonEmpty: (cases) => Effect.asSome(ran(live, batch.records, cases, module)) });
        return { application: session.application, version: session.version, survey: session.survey, ...present('state', state) };
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = (module: Option.Option<string>) => ({
    name: 'rhino',
    Engine,
    survey: { question: 'Language and environment the running instance reports', Schema: Survey, host: Option.some },
    Case: HostCase,
    shown: ['console', 'diagnostics'],
    progress: PROGRESS,
    engine: () => Result.succeed(Engine.literal),
    precondition,
    placed,
    refusal,
    installed,
    windowed: () => true,
    launch,
    opened,
    send: send(module),
    records: (_: Live, file: string) => recorded(Recorded)(file),
    quit: (live: Live) => requested(live.scratch, `${live.pid}.quit`, []),
    digest,
});
const Host = {
    ...hosting(Option.none()),
    compare: {
        _tag: 'Hosted',
        run: Effect.fn('run')(function* (chosen: Installed, calls: readonly Rebuilt<Case>[], scratch: string, limit: Duration.Duration) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const module = Array.head(Array.dedupe(calls.map(({ module: held }) => held)));
            const sendable = yield* Effect.forEach(calls, ({ hosted, fn }, index) => {
                const folder = path.join(scratch, 'rebuilt', String(index));
                return Effect.as(fs.makeDirectory(folder, { recursive: true }), { hosted: { ...hosted, fn, folder }, name: hosted.name, folder, map: [], presses: [], copies: [] } satisfies Prepared<Case>);
            });
            const done = yield* execute({ ...hosting(module), compare: { _tag: 'Imported' as const } }, chosen, scratch, ENTRY.type, limit, Option.none(), () => Effect.succeed({ engine: Engine.literal, sendable, settled: [] }));
            return done.outcomes;
        }),
    },
} as const satisfies Harness<typeof Survey, typeof HostCase, typeof Engine, typeof Placed.Type>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
