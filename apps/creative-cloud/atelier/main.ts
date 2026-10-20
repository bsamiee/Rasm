import { Atelier, Installation, InstalledFace, Kind, Native, Screenshot, Status, stripped, WindowID } from '@rasm/automation';
import { FontView } from '@rasm/indesign/fonts';
import { Render } from '@rasm/indesign/render';
import { Script } from '@rasm/indesign/scripts';
import { Hold, InDesign, PATH, Peer, WIRE } from '@rasm/indesign/service/contract';
import { Invoke } from '@rasm/indesign/session';
import { Phase } from '@rasm/indesign/transactions';
import { listen, PublishedResource, PublishedTool, publish } from '@rasm/mcp/link';
import { main } from '@rasm/mcp/main';
import { Attachment, Image, Instructions, Progress, READING, targeted } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Face, Instance, Run, Shaped } from '@rasm/typography';
import { ORIGIN } from '@rasm/uxp/runtime';
import { Array, Config, Context, Effect, flow, Layer, Match, Option, pipe, Schema, SchemaAST, Stream, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const server = yield* McpServer.McpServer;
            const peer = 'InDesign panel';
            const preflight = 'indesign://preflight';
            const json = <S extends Schema.Top>(schema: S) => Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(schema)));
            const serves = (row: Installation) => (linked: typeof Peer.Type) => row.url.href === linked.application.href;
            const panel = yield* listen(InDesign, {
                path: PATH,
                origin: ORIGIN,
                peer,
                identity: Peer,
                serialization: WIRE,
                middleware: Hold.client((phase) =>
                    Effect.flatMap(Progress, (report) =>
                        Option.match(phase, {
                            onNone: () => report(`${peer}: awaiting an answer; an open menu, dialog, or drag, or long work, holds the call until released`),
                            onSome: Phase.match({ Idle: () => Effect.void, Entered: () => report(`${peer}: running the call`) }),
                        }),
                    ),
                ),
                serves,
                follow: (client) => Stream.runForEach(client('changed', {}), () => server.notifications['notifications/resources/updated']({ uri: preflight })),
            });
            const listed = Effect.map(Effect.all([Installation.installed, panel.peers]), ([rows, peers]) =>
                rows.map((installation) => ({ installation, extension: Option.map(Array.findFirst(peers, serves(installation)), Struct.get('extension')) })).filter(({ installation, extension }) => installation.scriptable || Option.isSome(extension)),
            );
            const application = Schema.String.annotate({ description: 'Bundle identifier, name, or url of an installed Adobe application' });
            const route = {
                fields: { application: Schema.OptionFromOptionalKey(application) },
                resolve: ({ application: given }: { readonly application: Option.Option<string> }) =>
                    Option.match(given, {
                        onSome: Installation.located,
                        onNone: () =>
                            Effect.map(
                                Effect.flatMap(listed, (rows) =>
                                    Unmatched.single('application with a linked extension', Option.none())(
                                        rows.filter(({ installation, extension }) => Option.isSome(installation.running) && Option.isSome(extension)),
                                        ({ installation }) => [installation.url.href],
                                    ),
                                ),
                                Struct.get('installation'),
                            ),
                    }),
                trailer: (found: Installation, value: unknown) =>
                    Schema.is(Status)(value)
                        ? Effect.succeedNone
                        : found.status().pipe(
                              Effect.flatMap(json(Status)),
                              Effect.map((text) => Option.some(McpSchema.TextContent.make({ text }))),
                          ),
                annotations: Context.make(Tool.OpenWorld, false),
                completion: {
                    application: () =>
                        Effect.map(
                            panel.peers,
                            Array.map(({ application: linked }) => linked.href),
                        ),
                },
            };
            const register = targeted(route);
            yield* McpServer.registerResource({
                uri: 'adobe://applications',
                name: 'applications',
                description: 'Installed Adobe applications with their running state, channel, version, scriptability, and linked extension; each url is the application other tools take',
                mimeType: 'application/json',
                annotations: Context.make(Instructions, ["Tools given no application act on the one application with a linked extension, and every result ends with that application's status: responsiveness, front window, and open windows, dialogs, sheets, and menus with their text, controls, and actions."]),
                content: Effect.flatMap(listed, json(Schema.Array(Schema.Struct({ installation: Installation, extension: Schema.OptionFromOptionalKey(Peer.fields.extension) })))),
            });
            const installed = McpSchema.param('application', Schema.URL);
            const definition = {
                name: 'dictionary',
                description: 'Installed application scripting suites; read a suite for its declarations, then a declaration for native parameters, types, and properties',
                mimeType: 'application/xml',
                completion: { application: () => Effect.map(Installation.installed, (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('url'))) },
            };
            yield* McpServer.registerResource`adobe://dictionary/${installed}`({
                ...definition,
                content: (_, url) => Effect.flatMap(Installation.located(url.href), (found) => found.definition(Option.none())),
            });
            yield* McpServer.registerResource`adobe://dictionary/${installed}/${McpSchema.param('kind', Kind)}/${McpSchema.param('name', Schema.String)}`({
                ...definition,
                name: 'dictionary_term',
                description: 'Suite declaration index or complete native definition by kind and name, including every matching overload; completion supplies installed names',
                completion: {
                    ...definition.completion,
                    kind: () => Effect.succeed([...Kind.literals]),
                    name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                        const { application: given, kind } = yield* Schema.decodeUnknownEffect(Schema.Struct({ application: Schema.String, kind: Kind }))(context?.arguments);
                        return Array.map(yield* (yield* Installation.located(given)).terms(kind), Struct.get('name'));
                    }),
                },
                content: (_, url, kind, name) => Effect.flatMap(Installation.located(url.href), (found) => found.definition(Option.some({ kind, name }))),
            });
            yield* targeted({ ...route, fields: { application }, resolve: ({ application: given }: { readonly application: string }) => Installation.located(given) })(
                Tool.make('launch', {
                    description: 'Opens an installed Adobe application without substituting another version and returns it once it has finished launching; a running instance returns as it is',
                    parameters: Schema.Struct({}),
                    success: Installation,
                }),
                (_, chosen) => chosen.launch(),
            );
            yield* register(
                Tool.make('screenshot', {
                    description: 'Brings the application forward and captures its frontmost window or a given window at native pixel size as a lossless PNG',
                    parameters: Schema.Struct({ window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Window id from a previous screenshot result or status' })) }),
                    success: Screenshot.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                }).annotateMerge(READING),
                ({ window: id }, chosen) => chosen.screenshot(id),
            );
            const faces = { uri: 'atelier://faces', name: 'faces', description: 'Installed faces CoreText finds, each with the file holding it', mimeType: 'application/json' };
            yield* McpServer.registerResource({ ...faces, content: Effect.flatMap(InstalledFace.all, json(Schema.Array(InstalledFace))) });
            yield* register(
                Tool.make('font', {
                    description: `Opens a face by name at its named instance, a generated instance PostScript name, or the variation settings, and reads its names, coverage, metrics in em, embedding rights, language systems, features, axes, named instances, and color data, then shapes the text with the feature settings, reporting missing characters; ${faces.uri} lists installed faces. An application's linked extension answers with its view of the named face`,
                    parameters: Instance.pipe(Schema.fieldsAssign({ ...Run.fields, name: Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, in any case, as CoreText matches it; a linked extension takes its native font name or specifier' }) })),
                    success: Schema.Union([Schema.Struct({ ...InstalledFace.fields, face: Face, shaped: Shaped }), Schema.Struct({ ...FontView.fields, face: Schema.OptionFromOptionalKey(Face), shaped: Schema.OptionFromOptionalKey(Shaped) })]),
                }).annotateMerge(READING),
                Effect.fnUntraced(function* ({ name, text, features, variations, size }, chosen) {
                    const typeset = ({ data, postscriptName }: Effect.Success<ReturnType<InstalledFace['source']>>) => Effect.map(Face.open(data, postscriptName, { variations, size }), (face) => ({ face, shaped: face.shape({ text, features }) }));
                    return yield* Option.match(yield* Effect.catchNoSuchElement(panel.call(chosen)), {
                        onNone: Effect.fnUntraced(function* () {
                            const found = yield* InstalledFace.named(name);
                            return { ...found, ...(yield* Effect.flatMap(found.source(), typeset)) };
                        }),
                        onSome: Effect.fnUntraced(function* (use) {
                            const { view, source } = yield* use('font', { name, text });
                            const set = yield* Effect.transposeOption(Option.map(source, typeset));
                            return { ...view, face: Option.map(set, Struct.get('face')), shaped: Option.map(set, Struct.get('shaped')) };
                        }),
                    });
                }),
            );
            yield* register(
                Tool.make('execute_script', {
                    description:
                        "Runs source in one application's script runtime: ExtendScript (ES3) through do javascript or DoScript, Acrobat JavaScript through do script, and UXP JavaScript or TypeScript through a linked extension, which strips TypeScript before the source runs. The result names the runtime that ran the source",
                    parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body in the runtime the application takes; its return value is the result' }) }),
                    success: Schema.Union([Installation.Script, Script.Result]),
                }),
                Effect.fnUntraced(function* ({ code }, chosen) {
                    return yield* Option.match(yield* Effect.catchNoSuchElement(panel.call(chosen)), {
                        onNone: () => chosen.script(code),
                        onSome: (use) => Effect.flatMap(stripped(code), (plain) => use('execute_script', { code: plain })),
                    });
                }),
            );
            yield* register(
                Tool.make('command', {
                    description: "Drives one application's native commands: an Apple Event command from its dictionary, an action of an open dialog, sheet, window, or menu from its status, or a menu action its linked extension lists",
                    parameters: Schema.Struct({
                        run: Schema.TaggedUnion({
                            Command: {
                                command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
                                arguments: Schema.Record(Schema.String, Native)
                                    .annotate({ description: 'Dictionary parameters by name; direct supplies the direct parameter. Reuse native reply values unchanged' })
                                    .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                                artifacts: Schema.Array(Schema.URLFromString)
                                    .annotate({ description: 'File URLs the command writes, returned with the reply' })
                                    .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                            },
                            Dialog: { element: WindowID.annotate({ description: 'Element id from a status' }), action: Schema.String.annotate({ description: 'Action the element lists' }) },
                            Menu: { action: Invoke.fields.action },
                        }),
                    }),
                    success: Schema.Union([Installation.Command.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment, Schema.ErrorInstance())) })), Status, Invoke.Result]),
                }),
                ({ run }, found) =>
                    Match.value(run).pipe(
                        Match.tag('Command', ({ command, arguments: parameters, artifacts }) => found.command(command, parameters, artifacts)),
                        Match.tag('Dialog', ({ element, action }) => found.press(element, action)),
                        Match.tag(
                            'Menu',
                            Effect.fnUntraced(function* ({ action }) {
                                return yield* (yield* panel.call(found))('invoke', { action });
                            }),
                        ),
                        Match.exhaustive,
                    ),
            );
            yield* publish(
                InDesign.omit('enter', 'changed'),
                {
                    inspect: new PublishedTool({ annotations: READING }),
                    change: new PublishedTool({ annotations: Context.empty() }),
                    create: new PublishedTool({ annotations: Context.empty() }),
                    remove: new PublishedTool({ annotations: Context.empty() }),
                    find_change: new PublishedTool({ annotations: Context.empty() }),
                    render: new PublishedTool({ annotations: READING, success: Render.Result.mapFields(Struct.assign({ image: Image('image/png') })) }),
                    menu: new PublishedResource({
                        uri: 'indesign://menu',
                        complete: {
                            area: flow(
                                panel.call,
                                Effect.flatMap((use) => use('menu', { area: Option.none() })),
                                Effect.map((rows) => Array.dedupe(Array.map(rows, Struct.get('area')))),
                            ),
                        },
                    }),
                    preflight: new PublishedResource({
                        uri: preflight,
                        complete: {
                            profile: flow(
                                panel.call,
                                Effect.flatMap((use) => use('preflight', { profile: Option.none() })),
                                Effect.map(Struct.get('profiles')),
                            ),
                        },
                    }),
                    queries: new PublishedResource({
                        uri: 'indesign://queries',
                        complete: {
                            name: flow(
                                panel.call,
                                Effect.flatMap((use) => use('queries', { name: Option.none() })),
                                Effect.map(Array.map(Struct.get('name'))),
                            ),
                        },
                    }),
                    kind: new PublishedResource({
                        uri: 'indesign://kinds',
                        complete: {
                            name: flow(
                                panel.call,
                                Effect.flatMap((use) => use('kind', { name: Option.none() })),
                                Effect.map(Match.valueTags({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] })),
                            ),
                        },
                    }),
                },
                {
                    route,
                    invoke: (rpc) =>
                        Effect.fnUntraced(function* (payload, chosen) {
                            return yield* (yield* panel.call(chosen))(rpc._tag, payload);
                        }),
                    annotations: Context.make(Instructions, [
                        'InDesign is served through its panel, which links when InDesign starts. Measurements are numbers in points. Every mutation runs as one undo step named by its verb and target, and its result carries the preflight of each document it touched: issue count and the issues it introduced. A mutation failing with modalState as its cause met an open dialog or alert: close it through command Dialog, then retry. Resource templates select an application; base resource URIs use the single linked application.',
                        ...Option.toArray(Option.fromUndefinedOr(SchemaAST.resolveDescription(Script.ast))),
                    ]),
                },
            );
        }),
    ).pipe(Layer.provide(pipe(Config.String('ATELIER_PATH'), Effect.map(Atelier.spawned), Layer.unwrap))),
);
