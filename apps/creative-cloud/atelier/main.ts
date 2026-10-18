import { extension } from '@rasm/indesign/mcp/server';
import { main } from '@rasm/mcp/main';
import { Activity, Attachment, Blocks, Image, Instructions, READING, register } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Font, Options, read } from '@rasm/typography/font';
import { Array, Effect, FileSystem, Function, Layer, Option, Path, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };
import { Application, Face, Screenshot, Scripting } from './scripting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [sender, files, path] = yield* Effect.all([Scripting, FileSystem.FileSystem, Path.Path]);
            const galley = { name: 'Galley', identifier: 'com.adobe.InDesign' };
            const dictionary = ((segments: TemplateStringsArray, ..._: readonly string[]) => segments)`adobe://dictionary/${'application'}`;
            const serves = (row: typeof Application.Type) => (peer: { readonly application: URL }) => row.running && path.relative(row.url.pathname, peer.application.pathname) === '';
            const application = Schema.OptionFromOptionalKey(
                Schema.URLFromString.annotate({ description: 'url from applications, absent for the one application with a linked extension' }).pipe(
                    Schema.decodeTo(Schema.toType(Application), {
                        decode: SchemaGetter.transformEffect((url: URL) => sender.application({ url }).pipe(Effect.mapError(({ message }) => new SchemaIssue.InvalidValue({ message }, url)))),
                        encode: SchemaGetter.transform(Struct.get('url')),
                    }),
                ),
            );
            const panel = yield* extension(galley.name, {
                schema: Schema.Struct({ application }),
                select: ({ application: given }, peers) => Option.match(given, { onNone: () => Option.liftPredicate(Function.constTrue, () => peers.length > 0), onSome: (row) => Option.liftPredicate(serves(row), () => row.identifier === galley.identifier) }),
                completion: { application: (peers) => peers.map(({ application: url }) => url.href) },
            });
            const listed = Effect.fnUntraced(function* (running: boolean) {
                const [rows, peers] = yield* Effect.all([sender.applications({ running }), panel.peers]);
                return rows
                    .map((row) => ({
                        ...row,
                        dictionary: row.scriptable ? Option.some(dictionary.join(encodeURIComponent(row.url.href))) : Option.none(),
                        extension: row.identifier === galley.identifier ? Option.some({ name: galley.name, linked: peers.some(serves(row)) }) : Option.none(),
                    }))
                    .filter(({ scriptable, extension: owner }) => scriptable || Option.exists(owner, Struct.get('linked')));
            });
            const target = Effect.fnUntraced(function* (given: Option.Option<typeof Application.Type>) {
                const rows = Option.isSome(given) ? [] : yield* listed(true);
                const linked = rows.filter(({ extension: owner }) => Option.exists(owner, Struct.get('linked')));
                const candidates = (linked.length === 0 ? rows : linked).map(({ url }) => url.href);
                return yield* Effect.fromOption(
                    Option.orElse(given, () => (linked.length === 1 ? Array.head(linked) : Option.none())),
                    () => Unmatched.make({ subject: 'single application with a linked extension', query: '', candidates }),
                );
            });
            const Command = Schema.TaggedStruct('Command', {
                command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
                arguments: Schema.JsonObject.annotate({ description: 'Dictionary parameters by name; direct supplies the direct parameter. Reuse native reply values unchanged' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                artifacts: Schema.Array(Schema.URLFromString)
                    .annotate({ description: 'File URLs the command writes, returned with the reply' })
                    .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            }).annotate({ description: 'Native Apple Event command; dictionary parameters select script language and undo behavior' });
            const reference = { uri: 'adobe://scripting/reference', name: 'scripting_reference', description: 'Apple Event command arguments, object references, native values, script languages, and results', mimeType: 'text/markdown' };
            yield* McpServer.registerResource({
                ...reference,
                content: Effect.succeed({ contents: [{ uri: reference.uri, mimeType: reference.mimeType, text: yield* files.readFileString(yield* path.fromFileUrl(new URL('./resources/scripting.md', import.meta.url))) }] }),
            });
            const definition = {
                name: 'dictionary',
                description: 'Installed application scripting suites; read a suite for its declarations, then a declaration for native parameters, types, and properties',
                mimeType: 'application/xml',
                completion: { application: () => Effect.map(sender.applications({ running: false }), (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('url'))) },
            };
            yield* McpServer.registerResource(
                dictionary,
                McpSchema.param('application', Schema.URL),
            )({
                ...definition,
                content: (uri, url) => sender.definition({ application: url }).pipe(Effect.map((text) => ({ contents: [{ uri, mimeType: definition.mimeType, text }] }))),
            });
            yield* McpServer.registerResource`adobe://dictionary/${McpSchema.param('application', Schema.URL)}/${McpSchema.param('kind', Schema.String)}/${McpSchema.param('name', Schema.String)}`({
                ...definition,
                name: 'dictionary_term',
                description: 'Suite declaration index or complete native definition by kind and name, including every matching overload; completion supplies installed names',
                completion: {
                    ...definition.completion,
                    kind: (_, context) =>
                        Option.match(Option.fromUndefinedOr(context?.arguments?.application), {
                            onNone: () => Effect.succeed([]),
                            onSome: Function.flow(
                                Schema.decodeEffect(Schema.URLFromString),
                                Effect.flatMap((url) => sender.terms({ application: url })),
                            ),
                        }),
                    name: (_, context) =>
                        Option.match(Option.all({ application: Option.fromUndefinedOr(context?.arguments?.application), kind: Option.fromUndefinedOr(context?.arguments?.kind) }), {
                            onNone: () => Effect.succeed([]),
                            onSome: ({ application: address, kind }) => Schema.decodeEffect(Schema.URLFromString)(address).pipe(Effect.flatMap((url) => sender.terms({ application: url, kind }))),
                        }),
                },
                content: (uri, url, kind, name) => sender.definition({ application: url, selection: { kind, name } }).pipe(Effect.map((text) => ({ contents: [{ uri, mimeType: definition.mimeType, text }] }))),
            });
            yield* register(
                Tool.make('applications', {
                    description: 'Lists installed Adobe applications an agent can act on, with the url other tools take',
                    success: Schema.Struct({ applications: Schema.Array(Application.pipe(Schema.fieldsAssign({ dictionary: Schema.OptionFromOptionalKey(Schema.String), extension: Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.String, linked: Schema.Boolean })) }))) }),
                })
                    .annotateMerge(READING)
                    .annotate(Instructions, ['- applications: each application with the url other tools take as application. Tools given no application act on the one application with a linked extension.']),
                Effect.fnUntraced(function* () {
                    const [applications, add] = yield* Effect.all([listed(false), Blocks]);
                    yield* Effect.forEach(applications, ({ dictionary: uri, name }) => Option.match(uri, { onNone: () => Effect.void, onSome: (value) => add(McpSchema.ResourceLink.make({ ...definition, uri: value, title: name })) }), { discard: true });
                    return { applications };
                }),
            );
            yield* register(
                Tool.make('screenshot', {
                    description: 'Brings the application forward and captures its frontmost window or a given window at native pixel size as a lossless PNG',
                    parameters: Schema.Struct({ application, window: Schema.OptionFromOptionalKey(Screenshot.fields.window.fields.id.annotate({ description: 'Window id from a previous screenshot result' })) }),
                    success: Screenshot.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
                })
                    .annotateMerge(READING)
                    .annotate(Instructions, ['- screenshot: brings the application forward and captures its frontmost window or dialog at native pixel size; pass a returned window id to capture another window.']),
                Effect.fnUntraced(function* ({ application: given, window: id }) {
                    const chosen = yield* target(given);
                    return yield* sender.screenshot({ application: chosen.url, window: id });
                }),
            );
            yield* register(
                Tool.make('font', {
                    description: "Lists installed faces when name is absent; a name reads that face's metrics in em, features, axes, named instances, and missing characters. An application's extension adds its view of the named face",
                    parameters: Options.pipe(Schema.fieldsAssign({ name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'PostScript name, full name, or family for its Regular style, in any case; absent lists installed faces' })) })),
                    success: Schema.Union([Schema.Array(Face), Face.pipe(Schema.fieldsAssign({ metrics: Font }))]),
                }).annotateMerge(READING),
                Effect.fnUntraced(function* ({ name, ...options }) {
                    const faces = yield* sender.faces();
                    return yield* Option.isNone(name)
                        ? Effect.succeed(faces)
                        : Effect.flatMap(
                              Array.match(
                                  faces.filter((each) => [each.postscriptName, each.fullName, ...(each.style === 'Regular' ? [each.family] : [])].map(String.toLowerCase).includes(String.toLowerCase(name.value))),
                                  { onEmpty: Option.none, onNonEmpty: Option.some },
                              ).pipe(
                                  Effect.fromOption(() => Unmatched.make({ subject: 'installed face', query: name.value, candidates: faces.map(Struct.get('postscriptName')) })),
                                  Effect.filterOrFail(
                                      (matched) => matched.length === 1,
                                      (matched) => Unmatched.make({ subject: 'single installed face', query: name.value, candidates: matched.map(Struct.get('postscriptName')) }),
                                  ),
                              ),
                              Effect.fnUntraced(function* ([face]) {
                                  return { ...face, metrics: yield* read(yield* files.readFile(face.file), face.postscriptName, options) };
                              }),
                          );
                }),
            );
            yield* register(
                Tool.make('execute', {
                    description: "Runs a scripting dictionary command, or a form an application's extension adds, in one application",
                    parameters: Schema.Struct({
                        description: Activity.fields.description.annotate({ description: 'Present-progressive sentence shown while the call runs' }),
                        application,
                        run: Command,
                    }),
                    success: Schema.Struct({ reply: Schema.Json, artifacts: Schema.Array(Attachment.schema), unread: Schema.Array(Schema.String).annotate({ description: 'Why each artifact the reply does not carry could not be read' }) }),
                }).annotate(Instructions, [`- execute Command: read ${reference.uri} for native arguments, references, and results, then the application's dictionary for its commands and script languages. An unanswered mutation can have run; inspect application state before repeating it.`]),
                Effect.fnUntraced(function* ({ application: given, run: { command, arguments: parameters, artifacts } }) {
                    const chosen = yield* target(given);
                    const reply = yield* sender.execute({ application: chosen.url, command, arguments: parameters });
                    const [attached, failures] = yield* Effect.partition(
                        artifacts,
                        Effect.fnUntraced(function* (url: URL) {
                            return new Attachment({ uri: url.href, ...(yield* Effect.all({ mimeType: sender.contentType({ url }), data: Effect.flatMap(path.fromFileUrl(url), files.readFile) }, { concurrency: 'unbounded' })) });
                        }),
                        { concurrency: 'unbounded' },
                    );
                    return { reply, artifacts: attached, unread: failures.map(Struct.get('message')) };
                }),
            );
            yield* panel.publish;
        }),
    ).pipe(Layer.provide(Scripting.layer)),
);
