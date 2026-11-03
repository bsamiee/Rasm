import { fileURLToPath } from 'node:url';
import { Array, Deferred, Effect, FileSystem, identity, Option, Path, type PlatformError, Record, Result, Schema, Struct, String as Text, UndefinedOr } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { HarnessError } from '#/error';
import { appeared, output, plist, printed } from '#/instance';
import { Closed, Engine, fields, HostCase, Info, type Placed, REFUSED, Request, Rows, Sarif, Session, Source, Survey } from '#/rhino/contract';
import { precondition } from '#/rhino/precondition';
import type { Batch, Host as Harness, Installed, Live } from '#/session';

// --- [TYPES] ---------------------------------------------------------------------------

type Case = typeof HostCase.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PREFIX = 'com.mcneel.rhinoceros.';
const HOST = new RegExp(`^${RegExp.escape(PREFIX)}\\d+$`, 'u');
const HARNESS = 'RASM_HARNESS';
const SESSION = 'session.json';
const REQUEST = 'request';
const ENTRY = { type: 'Cases', method: 'Run' } as const;
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
    '        host = new {',
    '            language = new { lcid = Localization.CurrentLanguageId, culture = CultureInfo.GetCultureInfo(Localization.CurrentLanguageId).Name },',
    '            environment = new { version = RhinoApp.Version.ToString(), runtime = RuntimeInformation.FrameworkDescription, system = RuntimeInformation.OSDescription },',
    '        },',
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
const lines = (command: string, args: readonly string[]) => Effect.map(printed(command, args), (text) => Array.fromIterable(Text.linesIterator(text)).filter(Text.isNonEmpty));
const present = <K extends string, A>(key: K, value: Option.Option<A>) => Option.match(value, { onNone: () => ({}), onSome: (held) => Record.singleton(key, held) });

// --- [INSTALLATIONS]
const companions = Effect.fn('companions')(function* (bundle: string, identifier: string) {
    const path = yield* Path.Path;
    const apps = yield* lines('fd', ['--type', 'directory', '--case-sensitive', '--glob', '--no-ignore', '*.app', path.join(bundle, 'Contents')]);
    return Array.dedupe(yield* Effect.forEach(apps, (app) => Effect.map(plist(app, Info), ({ CFBundleIdentifier }) => CFBundleIdentifier))).filter((held) => held !== identifier);
});
const installed = Effect.gen(function* () {
    const bundles = yield* lines('mdfind', [`kMDItemCFBundleIdentifier == '${PREFIX}*'`]);
    const found = yield* Effect.forEach(bundles, (bundle) => Effect.map(plist(bundle, Info), (info) => [bundle, info] as const));
    return yield* Effect.forEach(
        found.filter(([, { CFBundleIdentifier }]) => HOST.test(CFBundleIdentifier)),
        ([bundle, info]) => Effect.map(companions(bundle, info.CFBundleIdentifier), (watched): Installed => ({ identifier: info.CFBundleIdentifier, name: info.CFBundleName, app: bundle, version: info.CFBundleVersion, watched: [info.CFBundleIdentifier, ...watched] })),
    );
});

// --- [FILES]
const requested = Effect.fn('requested')(function* (scratch: string, stem: string, paths: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const draft = path.join(scratch, `${stem}.tmp`);
    yield* fs.writeFileString(draft, yield* Schema.encodeEffect(Request)(paths));
    yield* fs.rename(draft, path.join(scratch, `${stem}.${REQUEST}`));
});

// --- [CASES]
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
const bodyOf = (hosted: Case) =>
    Option.orElse(
        Option.map(Option.fromUndefinedOr(hosted.rebuilt), ({ fn }) => called(fn)),
        () => HostCase.match(hosted, { Probe: ({ evaluate }) => Option.some(`(object?)(${evaluate})`), Call: ({ call }) => Option.some(called(call)), Run: () => Option.none<string>() }),
    );
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
const failing = Effect.fn('failing')(function* (log: string, source: string, spans: readonly (readonly [number, number])[], reported: string) {
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
    yield* foreign.length === 0 && owned.length > 0 ? Effect.void : HarnessError.unavailable(`${ENTRY.type} build`)(foreign.length === 0 ? reported : foreign.join('; '));
    return Record.map(
        Array.groupBy(Array.getSomes(owned.map(({ text, index }) => Option.map(index, (held) => [String(held), text] as const))), ([index]) => index),
        (pairs): string => `did not compile: ${pairs.map(([, text]) => text).join('; ')}`,
    );
});
const building = (chosen: Installed, folder: string, cases: readonly Case[], module: Option.Option<string>, plugin: Option.Option<string>) => {
    const attempt: (reasons: Record.ReadonlyRecord<string, string>) => Effect.Effect<string, HarnessError | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> = Effect.fnUntraced(function* (reasons: Record.ReadonlyRecord<string, string>) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [source, log] = [path.join(folder, `${ENTRY.type}.cs`), path.join(folder, 'build.sarif')];
        const { text: generated, spans } = program(cases.map((hosted, index) => Option.match(Record.get(reasons, String(index)), { onNone: () => entry(hosted), onSome: (reason) => [`        new Rasm.Harnesses.Rhino.Expressions.Uncompiled(${JSON.stringify(reason)}),`] })));
        yield* fs.writeFileString(source, generated);
        const properties = [
            `-p:RhinoInstallPath=${chosen.app}`,
            `-p:HarnessCases=${source}`,
            `-p:ArtifactsPath=${path.join(folder, 'artifacts')}`,
            `-p:ErrorLog=${log}%2Cversion=2.1`,
            ...Option.toArray(Option.map(module, (file) => `-p:HarnessRebuilt=${file}`)),
            ...Option.toArray(Option.map(plugin, (file) => `-p:HarnessPlugIn=${file}`)),
        ];
        const { stdout, stderr, code } = yield* output('dotnet', ['build', PROJECT, '-nologo', '-v:q', ...properties, '-t:Build', '-getProperty:TargetPath']);
        const text = new TextDecoder().decode(stdout);
        return code === 0 ? text.trim() : yield* Effect.flatMap(failing(log, source, spans, `${text}${stderr}`), (found) => attempt({ ...reasons, ...found }));
    });
    return attempt({});
};

// --- [TRANSPORT]
const launch = Effect.fn('launch')(function* (_: Installed, scratch: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const loader = path.join(scratch, 'loader.cs');
    yield* fs.writeFileString(loader, LOADER);
    return { arguments: ['-nosplash', `-runscript=_-ScriptEditor _Run "${loader}"`], defaults: { NSAppSleepDisabled: 'YES' }, environment: { [HARNESS]: scratch } };
});
const sessionOf = (live: Live) =>
    Effect.flatMap(
        Effect.flatMap(Path.Path, (path) => appeared(path.join(live.scratch, `${live.pid}.${SESSION}`))),
        Schema.decodeEffect(Session),
    );
const ran = Effect.fn('ran')(function* (live: Live, records: string, cases: Array.NonEmptyReadonlyArray<Case>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.dirname(records);
    const plugin = Array.findFirst(cases, ({ plugin: file }) => Option.fromUndefinedOr(file));
    const assembly = yield* building(
        live.chosen,
        folder,
        cases,
        Array.findFirst(cases, ({ rebuilt }) => Option.fromUndefinedOr(rebuilt?.module)),
        plugin,
    );
    const batch = path.join(folder, 'batch.json');
    const row = (hosted: Case) => ({
        name: hosted.name,
        folder: hosted.folder,
        precondition: hosted.precondition,
        reads: Record.keys(hosted.read),
        macros: hosted.rebuilt === undefined ? (hosted.commands ?? []) : [],
        args: HostCase.match(hosted, { Probe: () => [], Call: ({ args }) => args, Run: ({ args }) => args }),
        ...(hosted._tag === 'Run' && hosted.rebuilt === undefined ? { script: hosted.source.script } : {}),
    });
    yield* fs.writeFileString(batch, yield* Schema.encodeEffect(Rows)({ cases: cases.map(row), ...present('plugin', plugin) }));
    const reply = path.join(folder, 'reply.json');
    yield* requested(live.scratch, `${live.pid}.${path.basename(folder)}`, [assembly, batch, records, reply]);
    const closed = yield* Effect.flatMap(appeared(reply), Schema.decodeEffect(Closed));
    return yield* UndefinedOr.match(closed.failed, {
        onUndefined: () => Effect.succeed(Struct.omit(closed, ['failed'])),
        onDefined: HarnessError.unavailable(`${ENTRY.type} batch`),
    });
});
const send = Effect.fn('send')(function* (live: Live, batch: Batch<typeof Survey.Type, Case>) {
    const session = yield* sessionOf(live);
    const state = yield* Array.match(batch.cases, { onEmpty: () => Effect.succeedNone, onNonEmpty: (cases) => Effect.asSome(ran(live, batch.records, cases)) });
    return { application: session.application, version: session.version, survey: session.survey, ...present('state', state) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Host = {
    Engine,
    survey: { question: 'Language and environment the running instance reports', Schema: Survey },
    Case: HostCase,
    fields,
    shown: ['console', 'diagnostics'],
    progress: [{ title: 'Please Wait...', buttons: ['Cancel'] }],
    engine: () => Result.succeed(Engine.literal),
    precondition,
    placed: (decoded, _folder, product) =>
        Effect.map(Path.Path, (path) => ({
            precondition: {
                ...decoded,
                source: Source.match(decoded.source, { Template: identity, Open: (held) => ({ ...held, path: path.join(product, held.path) }), Application: identity }),
                ...(decoded.canvas?.open === undefined ? {} : { canvas: { open: path.join(product, decoded.canvas.open) } }),
            },
            copies: [],
        })),
    refusal,
    installed,
    launch,
    opened: (live) =>
        Effect.raceFirst(
            Effect.map(sessionOf(live), ({ survey }) => survey.host),
            Effect.flatMap(Deferred.await(live.verdict), (over) => HarnessError.unavailable(live.chosen.name)(`instance ${live.pid} ${over._tag.toLowerCase()} before the loader wrote ${SESSION}`)),
        ),
    send,
    quit: (live) => requested(live.scratch, `${live.pid}.quit`, []),
    rebuilt: ({ hosted, fn, module }, folder) => Effect.succeed({ hosted: { ...hosted, folder, rebuilt: { fn, module } }, map: [] }),
} as const satisfies Harness<typeof Survey.Type, typeof HostCase.Type, typeof Engine.Type, typeof Placed.Type, typeof fields>;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host };
