import { Automation, Installation, InstalledFace, Kind, Native, Screenshot, Status, WindowID } from '@rasm/automation';
import { FontView } from '@rasm/indesign/fonts';
import { Preflight } from '@rasm/indesign/preflight';
import { Render } from '@rasm/indesign/render';
import { Script } from '@rasm/indesign/scripts';
import { Hold, InDesign, LINK } from '@rasm/indesign/service/contract';
import { Invoke } from '@rasm/indesign/session';
import { channel, type Message } from '@rasm/mcp/channel';
import { listen, PublishedResource, PublishedTool, publish } from '@rasm/mcp/link';
import { main } from '@rasm/mcp/main';
import { Attachment, Image, Instructions, Progress, READING, resource, resourceTemplate, targeted } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Face, Instance, Run, Shaped } from '@rasm/typography/main';
import type { Source } from '@rasm/typography/source';
import { Peer } from '@rasm/uxp/link';
import { Array, Cause, Config, Context, Effect, Exit, flow, Layer, Match, Option, Queue, Schema, SchemaAST, Stream, Struct } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const json = <S extends Schema.Top>(schema: S) => Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(schema)));
            const serves = (row: Installation) => (linked: typeof Peer.Type) => row.url.href === linked.application.href;
            const panel = yield* listen(InDesign, { ...LINK, middleware: Hold.client((message) => Progress.use((report) => report(message))), serves });
            const [launched, events] = yield* Effect.all([Queue.unbounded<Installation>(), Queue.unbounded<typeof Message.Type>()]);
            yield* channel({
                name: manifest.name,
                version: manifest.version,
                path: '/channel',
                instructions:
                    'kind="status" events follow each application launched through atelier: its status after its windows, dialogs, sheets, or menus changed, in the form every tool result ends with, or the reason its watch ended. kind="preflight" events follow each InDesign mutation: the issues it introduced in the document the event names. Events are one-way: read them and act through the atelier tools.',
                messages: Stream.merge(
                    Stream.fromQueue(events),
                    Stream.fromQueue(launched).pipe(
                        Stream.groupByKey(({ url }) => url.href),
                        Stream.flatMap(([, launches]) => Stream.switchMap(launches, (found) => Stream.cross(Stream.succeed(found), found.watch().pipe(Stream.drop(1), Stream.map(Exit.succeed), Stream.catchCause(flow(Exit.failCause, Stream.succeed))))), { concurrency: 'unbounded' }),
                        Stream.mapEffect(([found, exit]) => Effect.map(Exit.match(exit, { onSuccess: flow(json(Status), Effect.orDie), onFailure: flow(Cause.pretty, Effect.succeed) }), (content) => ({ content, meta: { application: found.name, kind: 'status' } }))),
                    ),
                ),
            });
            const status = (found: Installation) =>
                found.status().pipe(
                    Effect.flatMap(json(Status)),
                    Effect.map((text) => McpSchema.TextContent.make({ text })),
                );
            const application = Schema.String.annotate({ description: 'Bundle identifier, name, or url of an installed Adobe application' });
            const route = {
                fields: { application: Schema.OptionFromOptionalKey(application) },
                resolve: ({ application: given }: { readonly application: Option.Option<string> }) =>
                    Option.match(given, {
                        onSome: Installation.located,
                        onNone: () =>
                            panel.peers.pipe(
                                Effect.flatMap((peers) => Unmatched.single('application with a linked extension', Option.none())(peers, ({ application: linked }) => [linked.href])),
                                Effect.flatMap(({ application: linked }) => Installation.located(linked.href)),
                            ),
                    }),
                trailer: (found: Installation, exit: Exit.Exit<unknown, Error>) =>
                    Effect.andThen(
                        Effect.forEach(
                            Array.flatMap(Option.toArray(Exit.getSuccess(exit)), Preflight.introduced).filter(({ issues }) => Array.isReadonlyArrayNonEmpty(issues)),
                            (row) => Effect.flatMap(json(Preflight.Summary.value)(row), (content) => Queue.offer(events, { content, meta: { application: found.name, document: row.specifier, kind: 'preflight' } })),
                            { discard: true },
                        ),
                        Effect.asSome(status(found)),
                    ),
                annotations: Context.make(Tool.OpenWorld, false).pipe(
                    Context.add(Instructions, ["Tools and resources given no application act on the one application with a linked extension, and every tool result ends with that application's status: responsiveness, front window, and open windows, dialogs, sheets, and menus with their text, controls, and actions."]),
                ),
                completion: {
                    application: () =>
                        Effect.map(
                            panel.peers,
                            Array.map(({ application: linked }) => linked.href),
                        ),
                },
            };
            const register = targeted(route);
            yield* resource({
                uri: 'adobe://applications',
                name: 'applications',
                description: 'Installed Adobe applications that are scriptable or have a linked extension; each url is the application other tools take',
                mimeType: 'application/json',
                content: Effect.flatMap(
                    Effect.zipWith(Installation.installed, panel.peers, (rows, peers) =>
                        rows.map((installation) => ({ installation, extension: Option.map(Array.findFirst(peers, serves(installation)), Struct.get('extension')) })).filter(({ installation, extension }) => installation.scriptable || Option.isSome(extension)),
                    ),
                    json(Schema.Array(Schema.Struct({ installation: Installation, extension: Schema.OptionFromOptionalKey(Peer.fields.extension) }))),
                ),
            });
            const installed = McpSchema.param('application', Schema.URL);
            const definition = {
                name: 'dictionary',
                description: 'Installed application scripting suites; read a suite for its declarations, then a declaration for native parameters, types, and properties',
                mimeType: 'application/xml',
                completion: { application: () => Effect.map(Installation.installed, (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('url'))) },
            };
            yield* resourceTemplate`adobe://dictionary/${installed}`({
                ...definition,
                content: (_, url) => Effect.flatMap(Installation.located(url.href), (found) => found.definition(Option.none())),
            });
            yield* resourceTemplate`adobe://dictionary/${installed}/${McpSchema.param('kind', Kind)}/${McpSchema.param('name', Schema.String)}`({
                ...definition,
                name: 'dictionary_term',
                description: 'Suite declaration index or complete native definition by kind and name, including every matching overload',
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
                (_, chosen) => Effect.tap(chosen.launch(), (found) => Queue.offer(launched, found)),
            );
            yield* register(
                Tool.make('screenshot', {
                    description: 'Brings the application forward and captures its frontmost window or a given window at native pixel size',
                    parameters: Schema.Struct({ window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Window id from a previous screenshot result or status' })) }),
                    success: Screenshot.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                }).annotateMerge(READING),
                ({ window: id }, chosen) => chosen.screenshot(id),
            );
            const faces = { uri: 'atelier://faces', name: 'faces', description: 'Installed faces CoreText finds, each with the file holding it', mimeType: 'application/json' };
            yield* resource({ ...faces, content: Effect.flatMap(InstalledFace.installed, json(Schema.Array(InstalledFace))) });
            const specimen = Schema.Struct({ face: Face, shaped: Shaped });
            yield* targeted({
                ...route,
                resolve: ({ application: given }: { readonly application: Option.Option<string> }) => Effect.transposeOption(Option.map(given, Installation.located)),
                trailer: (found: Option.Option<Installation>) => Effect.transposeOption(Option.map(found, status)),
                annotations: Context.make(Tool.OpenWorld, false),
            })(
                Tool.make('font', {
                    description: `Opens a face by name at its named instance, a generated instance PostScript name, or the variation settings, and shapes the text in it. Without an application it reads the faces ${faces.uri} lists; with one, the application's linked extension answers with its own view of the font`,
                    parameters: Instance.pipe(Schema.fieldsAssign({ ...Run.fields, name: Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, in any case, as CoreText matches it; a linked extension takes its native font name or specifier' }) })),
                    success: Schema.Union([Schema.Struct({ ...InstalledFace.fields, specimen }), Schema.Struct({ ...FontView.fields, specimen: Schema.OptionFromOptionalKey(specimen) })]),
                }).annotateMerge(READING),
                Effect.fnUntraced(function* ({ name, variations, size, ...run }, chosen) {
                    const typeset = (source: typeof Source.Type) => Effect.map(Face.open(source, { variations, size }), (face) => ({ face, shaped: face.shape(run) }));
                    return yield* Option.match(chosen, {
                        onNone: Effect.fnUntraced(function* () {
                            const found = yield* InstalledFace.named(name);
                            return { ...found, specimen: yield* Effect.flatMap(found.source(), typeset) };
                        }),
                        onSome: Effect.fnUntraced(function* (found) {
                            const { view, source } = yield* (yield* panel.call(found))('font', { name, text: run.text });
                            return { ...view, specimen: yield* Effect.transposeOption(Option.map(source, typeset)) };
                        }),
                    });
                }),
            );
            yield* register(
                Tool.make('execute_script', {
                    description: "Runs source in one application's script runtime: ExtendScript (ES3) through do javascript or DoScript, Acrobat JavaScript through do script, and UXP JavaScript or TypeScript through a linked extension, which strips TypeScript before the source runs",
                    parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body in the runtime the application takes; its return value is the result' }) }),
                    success: Installation.Script.pipe(Schema.fieldsAssign({ preflight: Schema.optionalKey(Script.Result.fields.preflight) })),
                }),
                ({ code }, chosen) =>
                    Effect.catchNoSuchElement(panel.call(chosen)).pipe(
                        Effect.map(Option.map((use) => (source: string) => use('execute_script', { code: source }))),
                        Effect.flatMap((linked) => chosen.script(code, linked)),
                    ),
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
                    Match.valueTags(run, {
                        Command: ({ command, arguments: parameters, artifacts }) => found.command(command, parameters, artifacts),
                        Dialog: ({ element, action }) => found.press(element, action),
                        Menu: ({ action }) => Effect.flatMap(panel.call(found), (use) => use('invoke', { action })),
                    }),
            );
            yield* publish(
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
                    annotations: Context.make(Instructions, ['InDesign is served through its panel, which links when InDesign starts.', ...Array.map(Array.fromNullishOr(SchemaAST.resolveDescription(Script.ast)), (text) => `execute_script: ${text}`)]),
                },
            );
        }),
    ).pipe(Layer.provide(Config.String('ATELIER_PATH').pipe(Effect.map(Automation.spawned), Layer.unwrap))),
);
