import { Face, helper, Installation, Screenshot, Status } from '@rasm/automation';
import { Invocation } from '@rasm/indesign/service/contract';
import { extension } from '@rasm/indesign/service/peer';
import { routed } from '@rasm/mcp/link';
import { main } from '@rasm/mcp/main';
import { Attachment, Blocks, Image, Instructions, READING, register } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Instance, Run, Shaped, Face as Typeface } from '@rasm/typography/face';
import { Array, Cause, Config, Context, Effect, FileSystem, Function, flow, Layer, Match, Option, Path, pipe, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { RpcClient } from 'effect/rpc';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [protocol, files, path] = yield* Effect.all([RpcClient.Protocol, FileSystem.FileSystem, Path.Path]);
            const galley = { name: 'Galley', identifier: 'com.adobe.InDesign' };
            const dictionary = ((segments: TemplateStringsArray, ..._: readonly string[]) => segments)`adobe://dictionary/${'application'}`;
            const serves = (row: Installation) => (peer: { readonly application: URL }) => Option.isSome(row.running) && path.relative(row.url.pathname, peer.application.pathname) === '';
            const application = Schema.OptionFromOptionalKey(
                Schema.String.annotate({ description: 'Bundle identifier, name, or url of an installed Adobe application; absent for the one application with a linked extension' }).pipe(
                    Schema.decodeTo(Schema.toType(Installation), {
                        decode: SchemaGetter.transformEffect((given: string) =>
                            Installation.located(given).pipe(
                                Effect.provideService(RpcClient.Protocol, protocol),
                                Effect.mapError(({ message }) => new SchemaIssue.InvalidValue({ message }, given)),
                            ),
                        ),
                        encode: SchemaGetter.transform((row) => row.url.href),
                    }),
                ),
            );
            const status = (found: Installation) =>
                found.status().pipe(
                    Effect.flatMap(Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Status)))),
                    Effect.map((text) => McpSchema.TextContent.make({ text })),
                );
            const Route = Schema.Struct({ application });
            const targeted = <P extends Schema.Codec<unknown, unknown> & { readonly fields: Readonly<Record<string, Schema.Codec<unknown, unknown>>> }, S extends Schema.Codec<unknown, unknown>, R>(
                tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
                handle: (parameters: P['Type'], installation: Installation) => Effect.Effect<S['Type'], Error, R>,
            ) =>
                register(
                    tool.setParameters(routed(tool.parametersSchema, Route)),
                    Effect.fnUntraced(function* ([parameters, { application: given }]) {
                        const chosen = yield* target(given);
                        const value = yield* handle(parameters, chosen);
                        const [block, add] = yield* Effect.all([status(chosen), Blocks]);
                        yield* add(block);
                        return value;
                    }),
                );
            const panel = yield* extension(
                galley.name,
                {
                    schema: Route,
                    select: ({ application: given }, peers) => Option.match(given, { onNone: () => Option.liftPredicate(Function.constTrue, () => peers.length > 0), onSome: (row) => Option.liftPredicate(serves(row), () => row.identifier === galley.identifier) }),
                    completion: { application: (peers) => peers.map(({ application: url }) => url.href) },
                },
                (_, peer) =>
                    Installation.installed.pipe(
                        Effect.flatMap(
                            flow(
                                Array.findFirst((row) => serves(row)(peer)),
                                Effect.fromOption(() => new Cause.NoSuchElementError(`No running installation serves the linked ${peer.application.href}`)),
                            ),
                        ),
                        Effect.flatMap(status),
                        Effect.provideService(RpcClient.Protocol, protocol),
                    ),
            );
            const listed = Effect.gen(function* () {
                const [rows, peers] = yield* Effect.all([Installation.installed, panel.peers]);
                return rows
                    .map((row) => ({
                        installation: row,
                        dictionary: Option.liftPredicate(dictionary.join(encodeURIComponent(row.url.href)), Function.constant(row.scriptable)),
                        extension: Option.liftPredicate({ name: galley.name, linked: peers.some(serves(row)) }, Function.constant(row.identifier === galley.identifier)),
                    }))
                    .filter(({ installation, extension: owner }) => installation.scriptable || Option.exists(owner, Struct.get('linked')));
            });
            const target = Effect.fnUntraced(function* (given: Option.Option<Installation>) {
                const rows = Option.isSome(given) ? [] : (yield* listed).filter(({ installation }) => Option.isSome(installation.running));
                const linked = rows.filter(({ extension: owner }) => Option.exists(owner, Struct.get('linked')));
                const candidates = (linked.length === 0 ? rows : linked).map(({ installation }) => installation.url.href);
                return yield* Effect.fromOption(
                    Option.orElse(given, () => Option.map(linked.length === 1 ? Array.head(linked) : Option.none(), Struct.get('installation'))),
                    () => Unmatched.make({ subject: 'single application with a linked extension', query: '', candidates }),
                );
            });
            const definition = {
                name: 'dictionary',
                description: 'Installed application scripting suites; read a suite for its declarations, then a declaration for native parameters, types, and properties',
                mimeType: 'application/xml',
                completion: { application: () => Effect.map(Installation.installed, (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('url'))) },
            };
            const applications = { uri: 'adobe://applications', name: 'applications', description: 'Installed Adobe applications with their running state, channel, version, dictionary, and linked extension; each url is the application other tools take', mimeType: 'application/json' };
            yield* McpServer.registerResource({
                ...applications,
                annotations: Context.make(Instructions, [
                    `- ${applications.uri}: ${applications.description}. Tools given no application act on the one application with a linked extension, and every result ends with that application's status: responsiveness, front window, and open windows, dialogs, sheets, and menus with their text and buttons.`,
                ]),
                content: Effect.map(
                    Effect.flatMap(
                        Effect.map(
                            listed,
                            Array.map(({ installation, ...owned }) => ({ ...installation, ...owned })),
                        ),
                        Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Schema.Struct({ ...Installation.fields, dictionary: Schema.OptionFromOptionalKey(Schema.String), extension: Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.String, linked: Schema.Boolean })) }))))),
                    ),
                    (text) => ({ contents: [{ uri: applications.uri, mimeType: applications.mimeType, text }] }),
                ),
            });
            yield* McpServer.registerResource(
                dictionary,
                McpSchema.param('application', Schema.URL),
            )({
                ...definition,
                content: (uri, url) =>
                    Installation.located(url.href).pipe(
                        Effect.flatMap((found) => found.definition(Option.none())),
                        Effect.map((text) => ({ contents: [{ uri, mimeType: definition.mimeType, text }] })),
                    ),
            });
            yield* McpServer.registerResource`adobe://dictionary/${McpSchema.param('application', Schema.URL)}/${McpSchema.param('kind', Schema.String)}/${McpSchema.param('name', Schema.String)}`({
                ...definition,
                name: 'dictionary_term',
                description: 'Suite declaration index or complete native definition by kind and name, including every matching overload; completion supplies installed names',
                completion: {
                    ...definition.completion,
                    kind: () => Effect.succeed(['command', 'class', 'enumeration']),
                    name: (_, context) =>
                        Option.match(Option.all({ application: Option.fromUndefinedOr(context?.arguments?.application), kind: Option.fromUndefinedOr(context?.arguments?.kind) }), {
                            onNone: () => Effect.succeed([]),
                            onSome: ({ application: address, kind }) =>
                                Installation.located(address).pipe(
                                    Effect.flatMap((found) => found.terms(kind)),
                                    Effect.map(Array.map(Struct.get('name'))),
                                ),
                        }),
                },
                content: (uri, url, kind, name) =>
                    Installation.located(url.href).pipe(
                        Effect.flatMap((found) => found.definition(Option.some({ kind, name }))),
                        Effect.map((text) => ({ contents: [{ uri, mimeType: definition.mimeType, text }] })),
                    ),
            });
            yield* targeted(
                Tool.make('launch', {
                    description: 'Opens an installed Adobe application, or finds its running instance, without substituting another version, and returns it once it has finished launching',
                    parameters: Schema.Struct({}),
                    success: Installation,
                }).annotate(Instructions, [`- launch: opens or finds any installed Adobe application by identifier, name, or url from ${applications.uri}, across versions and channels.`]),
                (_, chosen) => chosen.launch(),
            );
            yield* targeted(
                Tool.make('screenshot', {
                    description: 'Brings the application forward and captures its frontmost window or a given window at native pixel size as a lossless PNG',
                    parameters: Schema.Struct({ window: Schema.OptionFromOptionalKey(Screenshot.fields.window.fields.id.annotate({ description: 'Window id from a previous screenshot result or status' })) }),
                    success: Screenshot.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                })
                    .annotateMerge(READING)
                    .annotate(Instructions, ['- screenshot: brings the application forward and captures its frontmost window or dialog at native pixel size; pass a window id from a status or screenshot result to capture another window.']),
                ({ window: id }, chosen) => chosen.screenshot(id),
            );
            yield* register(
                Tool.make('font', {
                    description: "Lists installed faces when name is absent; a name reads that face's names, coverage, metrics in em, features, axes, and named instances, and shapes the text with the feature settings, reporting missing characters. An application's extension adds its view of the named face",
                    parameters: Instance.pipe(Schema.fieldsAssign({ ...Run.fields, name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'PostScript name, full name, or family for its Regular style, in any case; absent lists installed faces' })) })),
                    success: Schema.Union([Schema.Array(Face), Schema.Struct({ ...Face.fields, face: Typeface, shaped: Shaped })]),
                }).annotateMerge(READING),
                Effect.fnUntraced(function* ({ name, text, features, ...instance }) {
                    return yield* Option.match(name, {
                        onNone: () => Face.installed,
                        onSome: flow(
                            Face.named,
                            Effect.flatMap(
                                Effect.fnUntraced(function* (found) {
                                    const typeface = yield* Typeface.open(yield* files.readFile(found.file), Option.some(found.postscriptName), instance);
                                    return { ...found, face: typeface, shaped: yield* typeface.shape({ text, features }) };
                                }),
                            ),
                        ),
                    });
                }),
            );
            yield* targeted(
                Tool.make('execute_script', {
                    description: "Runs source in one application's script runtime: ExtendScript (ES3) through do javascript or DoScript, Acrobat JavaScript through do script, and UXP JavaScript or TypeScript through a linked extension. The result names the runtime that ran the source",
                    parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Function body in the runtime the application takes; its return value is the result' }) }),
                    success: Schema.Struct({ runtime: Schema.String.annotate({ description: 'Runtime that ran the source' }), result: Schema.Json }),
                }).annotate(Instructions, ['- execute_script: one runtime per application, stated in the result; ExtendScript hosts take ES3 source, UXP hosts take modern JavaScript or TypeScript. An unanswered mutation can have run; read application state before repeating it.']),
                ({ code }, chosen) => chosen.script(code),
            );
            yield* targeted(
                Tool.make('command', {
                    description: "Drives one application's native commands: an Apple Event command from its dictionary, a button on an open dialog, sheet, or window from its status, or a menu action its linked extension lists",
                    parameters: Schema.Struct({
                        run: Schema.TaggedUnion({
                            Command: {
                                command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
                                arguments: Schema.JsonObject.annotate({ description: 'Dictionary parameters by name; direct supplies the direct parameter. Reuse native reply values unchanged' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                                artifacts: Schema.Array(Schema.URLFromString)
                                    .annotate({ description: 'File URLs the command writes, returned with the reply' })
                                    .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                            },
                            Dialog: { element: Status.fields.elements.value.fields.id.annotate({ description: 'Element id from a status' }), button: Schema.String.annotate({ description: 'Button label the element lists' }) },
                            Menu: { action: Invocation.payloadSchema.fields.action },
                        }),
                    }),
                    success: Schema.Union([Schema.Struct({ reply: Schema.Json, artifacts: Schema.Array(Attachment), unread: Schema.Array(Schema.String).annotate({ description: 'Why each artifact the reply does not carry could not be read' }) }), Status, Invocation.successSchema]),
                }).annotate(Instructions, [
                    "- command Command: read the application's dictionary for its commands, parameters, and script languages; Dialog presses a listed button on an open element; Menu invokes an action from indesign://menu by id or name. An unanswered mutation can have run; read application state before repeating it.",
                ]),
                ({ run }, found) =>
                    Match.value(run).pipe(
                        Match.tag('Command', ({ command, arguments: parameters, artifacts }) => found.command(command, parameters, artifacts)),
                        Match.tag('Dialog', ({ element, button }) => found.press(element, button)),
                        Match.tag('Menu', ({ action }) => panel.invoke({ application: Option.some(found) }, action)),
                        Match.exhaustive,
                    ),
            );
            yield* panel.publish;
        }),
    ).pipe(Layer.provide(pipe(Config.String('ATELIER_PATH'), Effect.map(helper), Layer.unwrap))),
);
