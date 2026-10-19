import { Face } from '@rasm/automation/face';
import { Helper } from '@rasm/automation/helper';
import { helper, Installation, Screenshot, Status } from '@rasm/automation/installation';
import { extension } from '@rasm/indesign/mcp/server';
import { routed } from '@rasm/mcp/link';
import { main } from '@rasm/mcp/main';
import { Activity, Attachment, Blocks, Image, Instructions, READING, register } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Instance, Shaped, Text, Face as Typeface } from '@rasm/typography/face';
import { Array, Config, Context, Effect, FileSystem, Function, flow, Layer, Match, Option, Path, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };

// --- [COMPOSITION] ---------------------------------------------------------------------

const executable = Config.String('ATELIER_PATH');
main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [client, files, path] = yield* Effect.all([Helper, FileSystem.FileSystem, Path.Path]);
            const galley = { name: 'Galley', identifier: 'com.adobe.InDesign' };
            const kinds = ['command', 'class', 'enumeration'];
            const dictionary = ((segments: TemplateStringsArray, ..._: readonly string[]) => segments)`adobe://dictionary/${'application'}`;
            const serves = (row: Installation) => (peer: { readonly application: URL }) => Option.isSome(row.running) && path.relative(row.url.pathname, peer.application.pathname) === '';
            const named = 'Bundle identifier, name, or url of an installed Adobe application';
            const application = Schema.OptionFromOptionalKey(
                Schema.String.annotate({ description: `${named}; absent for the one application with a linked extension` }).pipe(
                    Schema.decodeTo(Schema.toType(Installation), {
                        decode: SchemaGetter.transformEffect((given: string) =>
                            Installation.located(given).pipe(
                                Effect.provideService(Helper, client),
                                Effect.mapError(({ message }) => new SchemaIssue.InvalidValue({ message }, given)),
                            ),
                        ),
                        encode: SchemaGetter.transform((row) => row.url.href),
                    }),
                ),
            );
            const status = (url: URL) =>
                client.status({ url }).pipe(
                    Effect.flatMap(Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Status)))),
                    Effect.orDie,
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
                        const [block, add] = yield* Effect.all([status(chosen.url), Blocks]);
                        yield* add(block);
                        return value;
                    }),
                );
            const panel = yield* extension(
                galley.name,
                {
                    schema: Schema.Struct({ application }),
                    select: ({ application: given }, peers) => Option.match(given, { onNone: () => Option.liftPredicate(Function.constTrue, () => peers.length > 0), onSome: (row) => Option.liftPredicate(serves(row), () => row.identifier === galley.identifier) }),
                    completion: { application: (peers) => peers.map(({ application: url }) => url.href) },
                },
                (_, peer) => status(peer.application),
            );
            const Row = Schema.Struct({ ...Installation.fields, dictionary: Schema.OptionFromOptionalKey(Schema.String), extension: Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.String, linked: Schema.Boolean })) });
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
            const attached = ({ reply, artifacts, unread }: Effect.Success<ReturnType<Installation['command']>>) => ({ reply, artifacts: artifacts.map((row) => new Attachment(row)), unread });
            const definition = {
                name: 'dictionary',
                description: 'Installed application scripting suites; read a suite for its declarations, then a declaration for native parameters, types, and properties',
                mimeType: 'application/xml',
                completion: { application: () => Effect.map(client.applications(), (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('url'))) },
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
                        Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row)))),
                    ),
                    (text) => ({ contents: [{ uri: applications.uri, mimeType: applications.mimeType, text }] }),
                ),
            });
            yield* McpServer.registerResource(
                dictionary,
                McpSchema.param('application', Schema.URL),
            )({
                ...definition,
                content: (uri, url) => client.definition({ application: url, selection: Option.none() }).pipe(Effect.map((text) => ({ contents: [{ uri, mimeType: definition.mimeType, text }] }))),
            });
            yield* McpServer.registerResource`adobe://dictionary/${McpSchema.param('application', Schema.URL)}/${McpSchema.param('kind', Schema.String)}/${McpSchema.param('name', Schema.String)}`({
                ...definition,
                name: 'dictionary_term',
                description: 'Suite declaration index or complete native definition by kind and name, including every matching overload; completion supplies installed names',
                completion: {
                    ...definition.completion,
                    kind: () => Effect.succeed(kinds),
                    name: (_, context) =>
                        Option.match(Option.all({ application: Option.fromUndefinedOr(context?.arguments?.application), kind: Option.fromUndefinedOr(context?.arguments?.kind) }), {
                            onNone: () => Effect.succeed([]),
                            onSome: ({ application: address, kind }) =>
                                Schema.decodeEffect(Schema.URLFromString)(address).pipe(
                                    Effect.flatMap((url) => client.terms({ application: url, kind })),
                                    Effect.map(Array.map(Struct.get('name'))),
                                ),
                        }),
                },
                content: (uri, url, kind, name) => client.definition({ application: url, selection: Option.some({ kind, name }) }).pipe(Effect.map((text) => ({ contents: [{ uri, mimeType: definition.mimeType, text }] }))),
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
                    description: "Lists installed faces when name is absent; a name reads that face's metrics in em, features, axes, named instances, and missing characters. An application's extension adds its view of the named face",
                    parameters: Instance.pipe(Schema.fieldsAssign({ text: Text, name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'PostScript name, full name, or family for its Regular style, in any case; absent lists installed faces' })) })),
                    success: Schema.Union([Schema.Array(Face), Schema.Struct({ ...Face.fields, face: Typeface, shaped: Shaped })]),
                }).annotateMerge(READING),
                Effect.fnUntraced(function* ({ name, text, ...instance }) {
                    return yield* Option.match(name, {
                        onNone: () => Face.installed,
                        onSome: flow(
                            Face.named,
                            Effect.flatMap(
                                Effect.fnUntraced(function* (found) {
                                    const typeface = yield* Typeface.open(yield* files.readFile(found.file), found.postscriptName, instance);
                                    return { ...found, face: typeface, shaped: yield* typeface.shape(text) };
                                }),
                            ),
                        ),
                    });
                }),
            );
            yield* targeted(
                Tool.make('execute_script', {
                    description: "Runs source in one application's script runtime: ExtendScript (ES3) through do javascript or DoScript, Acrobat JavaScript through do script, and UXP JavaScript or TypeScript through a linked extension. The result names the runtime that ran the source",
                    parameters: Schema.Struct({
                        description: Activity.fields.description.annotate({ description: 'Present-progressive sentence shown while the call runs' }),
                        code: Schema.String.annotate({ description: 'Function body in the runtime the application takes; its return value is the result' }),
                    }),
                    success: Schema.Struct({ runtime: Schema.String.annotate({ description: 'Runtime that ran the source' }), result: Schema.Json }),
                }).annotate(Instructions, ['- execute_script: one runtime per application, stated in the result; ExtendScript hosts take ES3 source, UXP hosts take modern JavaScript or TypeScript. An unanswered mutation can have run; read application state before repeating it.']),
                ({ code }, chosen) => Effect.map(Effect.all([chosen.language(), chosen.script(code)]), ([{ runtime }, result]) => ({ runtime, result })),
            );
            const Run = Schema.TaggedUnion({
                Command: {
                    command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
                    arguments: Schema.JsonObject.annotate({ description: 'Dictionary parameters by name; direct supplies the direct parameter. Reuse native reply values unchanged' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                    artifacts: Schema.Array(Schema.URLFromString)
                        .annotate({ description: 'File URLs the command writes, returned with the reply' })
                        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                },
                Dialog: { element: Status.fields.elements.value.fields.id.annotate({ description: 'Element id from a status' }), button: Schema.String.annotate({ description: 'Button label the element lists' }) },
            });
            yield* targeted(
                Tool.make('command', {
                    description: "Drives one application's native commands: an Apple Event command from its dictionary, or a button on an open dialog, sheet, or window from its status",
                    parameters: Schema.Struct({ description: Activity.fields.description.annotate({ description: 'Present-progressive sentence shown while the call runs' }), run: Run }),
                    success: Schema.Union([Schema.Struct({ reply: Schema.Json, artifacts: Schema.Array(Attachment.schema), unread: Schema.Array(Schema.String).annotate({ description: 'Why each artifact the reply does not carry could not be read' }) }), Status]),
                }).annotate(Instructions, ["- command Command: read the application's dictionary for its commands, parameters, and script languages; Dialog presses a listed button on an open element. An unanswered mutation can have run; read application state before repeating it."]),
                ({ run: chosen }, found) =>
                    Match.value(chosen).pipe(
                        Match.tag('Command', ({ command, arguments: parameters, artifacts }) => Effect.map(found.command(command, parameters, artifacts), attached)),
                        Match.tag('Dialog', ({ element, button }) => found.press(element, button)),
                        Match.exhaustive,
                    ),
            );
            yield* panel.publish;
        }),
    ).pipe(Layer.provide(Layer.unwrap(Effect.map(executable, helper)))),
);
