import { AppleEventValue, Automation, ElementAction, Installation, InstalledFace, Kind, Screenshot, ScreenshotImage, ScreenshotOptions, Status, WindowID } from '@rasm/automation';
import { FontView } from '@rasm/indesign/fonts';
import { Render } from '@rasm/indesign/render';
import { Script } from '@rasm/indesign/scripts';
import { Hold, InDesign, PATH, WIRE } from '@rasm/indesign/service/contract';
import { Invoke } from '@rasm/indesign/session';
import { channel } from '@rasm/mcp/channel';
import { listen, PublishedResource, PublishedTool, publish } from '@rasm/mcp/link';
import { main } from '@rasm/mcp/main';
import { Attachment, Content, Image, Instructions, Progress, READING, resource, resourceTemplate, targeted } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Face, Instance, Run, Shaped } from '@rasm/typography/main';
import type { Source } from '@rasm/typography/source';
import { ORIGIN, Peer } from '@rasm/uxp/link';
import { Array, Config, Context, Effect, FileSystem, flow, Layer, Match, Option, Schema, SchemaAST, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const json = <S extends Schema.Top>(schema: S) => Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(schema)));
            const serves = (row: Installation) => (linked: typeof Peer.Type) => row.url.href === linked.application.href;
            const [automation, panel] = yield* Effect.all([Automation, listen(InDesign, { path: PATH, origin: ORIGIN, identity: Peer, serialization: WIRE, middleware: Hold.client((message) => Progress.use((report) => report(message))), serves })]);
            const installed = SubscriptionRef.get(automation.installed);
            const served = Effect.zipWith(installed, panel.peers, (rows, peers) => rows.filter((row) => peers.some(serves(row))));
            const status = (found: Installation) =>
                automation.status(found).pipe(
                    Effect.flatMap(json(Status)),
                    Effect.map((text) => McpSchema.TextContent.make({ text })),
                );
            const repeatable = Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false));
            const shown = Schema.OptionFromOptionalKey(Content(ScreenshotImage, 'Media type and pixel size of the image content block beside the result', ({ data, mimeType, width, height }) => [McpSchema.ImageContent.make({ data, mimeType }), `${mimeType} ${width}x${height}`]));
            const specimen = Schema.Struct({ face: Face, shaped: Shaped });
            yield* channel({
                name: manifest.name,
                version: manifest.version,
                path: '/channel',
                instructions:
                    'kind="status" events follow every running Adobe application the server serves: its status when observation starts and after its windows, dialogs, sheets, or menus changed, as a Result holding the status in the form every tool result ends with, or why it can no longer be observed. Events are one-way: read them and act through the atelier tools.',
                messages: automation.statuses().pipe(
                    Stream.mapEffect(
                        Effect.fnUntraced(function* ({ pid, status: read }) {
                            const [rows, peers, content] = yield* Effect.all([installed, panel.peers, json(Schema.Result(Status, Schema.ErrorInstance()))(read)]);
                            return rows.filter((row) => row.instances.includes(pid) && (row.scriptable || peers.some(serves(row)))).map((row) => ({ content, meta: { application: row.name, pid: String(pid), kind: 'status' } }));
                        }),
                    ),
                    Stream.flattenIterable,
                    Stream.orDie,
                ),
            });
            yield* resource({
                uri: 'adobe://applications',
                name: 'applications',
                description: 'Installed Adobe applications, each with its linked extension when one is linked; each name is the application other tools take',
                mimeType: 'application/json',
                content: Effect.flatMap(
                    Effect.zipWith(installed, panel.peers, (rows, peers) => rows.map((installation) => ({ installation, extension: Option.map(Array.findFirst(peers, serves(installation)), Struct.get('extension')) }))),
                    json(Schema.Array(Schema.Struct({ installation: Installation, extension: Schema.OptionFromOptionalKey(Peer.fields.extension) }))),
                ),
            });
            yield* resource({ uri: 'atelier://faces', name: 'faces', description: 'Installed faces CoreText finds, each with the file holding it', mimeType: 'application/json', content: Effect.flatMap(automation.faces(), json(Schema.Array(InstalledFace))) });
            const [named, kind] = [McpSchema.param('application', Schema.String), McpSchema.param('kind', Kind)];
            const scriptable = { application: () => Effect.map(installed, (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('name'))) };
            yield* resourceTemplate`adobe://dictionary/${named}`({
                name: 'dictionary',
                description: 'Every suite, command, class, record type, value type, and enumeration of an installed application scripting dictionary by name and code; read a declaration for its native parameters, types, and properties',
                mimeType: 'application/json',
                completion: scriptable,
                content: (_, given) => Installation.located(given).pipe(Effect.flatMap(automation.terms), Effect.flatMap(json(Installation.Terms))),
            });
            yield* resourceTemplate`adobe://dictionary/${named}/${kind}/${McpSchema.param('name', Schema.String)}`({
                name: 'dictionary_term',
                description: 'Complete native definition by kind and name, including every matching overload',
                mimeType: 'application/xml',
                completion: {
                    ...scriptable,
                    kind: () => Effect.succeed([...Kind.literals]),
                    name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                        const chosen = yield* Schema.decodeUnknownEffect(Schema.Struct({ application: named, kind }))(context?.arguments);
                        return Array.map((yield* automation.terms(yield* Installation.located(chosen.application)))[chosen.kind], Struct.get('name'));
                    }),
                },
                content: (_, given, selected, term) => Effect.flatMap(Installation.located(given), (found) => automation.definition({ ...found, selection: { kind: selected, name: term } })),
            });
            const tools = (names: readonly string[]) => {
                const application = Schema.Literals(names).annotate({ description: 'Installed Adobe application by name' });
                const route = {
                    fields: { application: Schema.OptionFromOptionalKey(application), pid: Installation.fields.pid },
                    resolve: ({ application: given, pid }: { readonly application: Option.Option<string>; readonly pid: Option.Option<number> }) =>
                        Effect.map(
                            Option.match(given, {
                                onSome: Installation.located,
                                onNone: () => Effect.flatMap(served, (rows) => Unmatched.single('application with a linked extension', Option.none())(rows, (row) => [row.name])),
                            }),
                            (found) => Installation.make({ ...found, pid }),
                        ),
                    trailer: flow(status, Effect.asSome),
                    annotations: Context.make(Tool.OpenWorld, false).pipe(
                        Context.add(Instructions, ["Tools given no application act on the one application with a linked extension, and every tool result ends with that application's status: responsiveness, front window, and open windows, dialogs, sheets, and menus with their text, controls, and actions."]),
                    ),
                    completion: { application: () => Effect.map(served, Array.map(Struct.get('name'))) },
                };
                const register = targeted(route);
                return Effect.all(
                    [
                        targeted({ ...route, fields: { application }, resolve: ({ application: given }: { readonly application: string }) => Installation.located(given) })(
                            Tool.make('launch', {
                                description: 'Opens an installed Adobe application without substituting another version and returns once it has finished launching; a running instance stays as it is',
                                parameters: Schema.Struct({}),
                            }).annotateMerge(repeatable),
                            (_, chosen) => Effect.asVoid(automation.launch(chosen)),
                        ),
                        register(
                            Tool.make('screenshot', {
                                description: 'Brings the application forward and captures its frontmost window, a given window, or every window of the application at native pixel size or scaled to a maximum dimension',
                                parameters: ScreenshotOptions,
                                success: Screenshot.pipe(Schema.fieldsAssign({ sdrImage: shown, hdrImage: shown })),
                            }).annotateMerge(repeatable),
                            (options, chosen) => automation.screenshot({ ...chosen, ...options }),
                        ),
                        targeted({
                            ...route,
                            fields: { application: route.fields.application },
                            resolve: ({ application: given }: { readonly application: Option.Option<string> }) => Effect.transposeOption(Option.map(given, Installation.located)),
                            trailer: (found: Option.Option<Installation>) => Effect.transposeOption(Option.map(found, status)),
                        })(
                            Tool.make('font', {
                                description: "Opens a face by name at its named instance, a generated instance PostScript name, or the variation settings, and shapes the text in it; with an application, the application's linked extension answers with its own view of the font",
                                parameters: Instance.pipe(Schema.fieldsAssign({ ...Run.fields, name: Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, in any case, as CoreText resolves it; a linked extension takes its native font name or specifier' }) })),
                                success: Schema.Union([Schema.Struct({ ...InstalledFace.fields, specimen }), Schema.Struct({ ...FontView.fields, specimen: Schema.OptionFromOptionalKey(specimen) })]),
                            }).annotateMerge(READING),
                            Effect.fnUntraced(function* ({ name, variations, size, ...run }, chosen) {
                                const typeset = (source: typeof Source.Type) => Effect.map(Face.open(source, { variations, size }), (face) => ({ face, shaped: face.shape(run) }));
                                return yield* Option.match(chosen, {
                                    onNone: Effect.fnUntraced(function* () {
                                        const [found, files] = yield* Effect.all([automation.face({ name }), FileSystem.FileSystem]);
                                        return { ...found, specimen: yield* typeset({ data: yield* files.readFile(found.file), postscriptName: Option.some(found.postscriptName) }) };
                                    }),
                                    onSome: Effect.fnUntraced(function* (found) {
                                        const { view, source } = yield* (yield* panel.call(found))('font', { name, text: run.text });
                                        return { ...view, specimen: yield* Effect.transposeOption(Option.map(source, typeset)) };
                                    }),
                                });
                            }),
                        ),
                        register(
                            Tool.make('execute_script', {
                                description: "Runs source in one application's script runtime: ExtendScript (ES3) through do javascript or DoScript, Acrobat JavaScript through do script, and UXP JavaScript or TypeScript through a linked extension, which strips TypeScript before the source runs",
                                parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body in the runtime the application takes; its return value is the result' }) }),
                                success: Installation.Script.pipe(Schema.fieldsAssign({ preflight: Schema.optionalKey(Script.Result.fields.preflight) })),
                            }).annotateMerge(
                                Context.make(
                                    Instructions,
                                    Array.map(Array.fromNullishOr(SchemaAST.resolveDescription(Script.ast)), (text) => `execute_script: ${text}`),
                                ),
                            ),
                            ({ code }, chosen) =>
                                Effect.catchNoSuchElement(panel.call(chosen)).pipe(
                                    Effect.map(Option.map((use) => (source: string) => use('execute_script', { code: source }))),
                                    Effect.flatMap((linked) => chosen.script(code, linked)),
                                ),
                        ),
                        register(
                            Tool.make('command', {
                                description: "Drives one application's native commands: an Apple Event command from its dictionary, actions of an open dialog, sheet, window, or menu from its status, or a menu action its linked extension lists",
                                parameters: Schema.Struct({
                                    run: Schema.TaggedUnion({
                                        Command: {
                                            command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
                                            arguments: Schema.Record(Schema.String, AppleEventValue)
                                                .annotate({ description: 'Dictionary parameters by name; direct supplies the direct parameter. Reuse native reply values unchanged' })
                                                .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                                            artifacts: Schema.Array(Schema.URLFromString)
                                                .annotate({ description: 'File URLs the command writes, returned with the reply' })
                                                .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                                        },
                                        Dialog: { element: WindowID.annotate({ description: 'Element id from a status' }), actions: Schema.NonEmptyArray(ElementAction).annotate({ description: 'Performed in order, stopping at the first refusal' }) },
                                        Menu: { action: Invoke.fields.action },
                                    }),
                                }),
                                success: Schema.Union([Installation.Executed.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment, Schema.ErrorInstance())) })), Invoke.Result, Schema.Void]),
                            }),
                            ({ run }, found) =>
                                Match.valueTags(run, {
                                    Command: ({ command, arguments: parameters, artifacts }) => automation.execute({ ...found, command, arguments: parameters, artifacts }),
                                    Dialog: ({ element, actions }) => automation.press({ ...found, element, actions }),
                                    Menu: ({ action }) =>
                                        Effect.andThen(
                                            automation.activate(found),
                                            Effect.flatMap(panel.call(found), (use) => use('invoke', { action })),
                                        ),
                                }),
                        ),
                        publish(
                            InDesign.omit('enter'),
                            {
                                inspect: new PublishedTool({ annotations: READING }),
                                change: new PublishedTool({ annotations: Context.empty() }),
                                create: new PublishedTool({ annotations: Context.empty() }),
                                remove: new PublishedTool({ annotations: Context.empty() }),
                                find_change: new PublishedTool({ annotations: Context.empty() }),
                                render: new PublishedTool({ annotations: READING, success: Render.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })) }),
                                menu: new PublishedResource({ uri: 'indesign://menu', complete: { area: (rows) => Array.dedupe(Array.map(rows, Struct.get('area'))) } }),
                                preflight: new PublishedResource({ uri: 'indesign://preflight', complete: { profile: Struct.get('profiles') } }),
                                queries: new PublishedResource({ uri: 'indesign://queries', complete: { name: Array.map(Struct.get('name')) } }),
                                kind: new PublishedResource({ uri: 'indesign://kinds', complete: { name: Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } }),
                            },
                            {
                                route,
                                invoke: (rpc) =>
                                    Effect.fnUntraced(function* (payload, chosen) {
                                        return yield* (yield* panel.call(chosen))(rpc._tag, payload);
                                    }),
                                annotations: Context.make(Instructions, ['InDesign is served through its panel, which links when InDesign starts.']),
                            },
                        ),
                    ],
                    { discard: true },
                );
            };
            const [first, rest] = yield* Stream.peel(SubscriptionRef.changes(automation.installed).pipe(Stream.map(flow(Array.map(Struct.get('name')), Array.dedupe)), Stream.changes), Sink.take(1));
            yield* Effect.forEach(first, tools, { discard: true });
            yield* Effect.forkScoped(Stream.runForEach(rest, tools));
        }),
    ).pipe(Layer.provide(Config.String('ATELIER_PATH').pipe(Effect.map(Automation.spawned), Layer.unwrap))),
);
