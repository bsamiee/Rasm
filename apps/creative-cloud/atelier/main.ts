import { extension } from '@rasm/indesign/extension';
import { main } from '@rasm/mcp/main';
import { Activity, Attachment, Image, Instructions, READING, register, Unmatched } from '@rasm/mcp/server';
import { Font, Options, read } from '@rasm/typography/font';
import { Array, Cause, Context, Effect, FileSystem, Function, Layer, Option, Path, Predicate, Record, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import manifest from './package.json' with { type: 'json' };
import { Application, Face, Scripting, Window } from './scripting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

main(
    manifest,
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [sender, files, path] = yield* Effect.all([Scripting, FileSystem.FileSystem, Path.Path]);
            const galley = { name: 'Galley', identifier: 'com.adobe.InDesign' };
            const dictionary = ((segments: TemplateStringsArray, ..._: readonly string[]) => segments)`adobe://dictionary/${'application'}`;
            const serves = (row: typeof Application.Type) => (peer: { readonly version: number }) => row.running && Option.exists(row.version, (version) => Number.parseInt(version, 10) === peer.version);
            const application = Schema.OptionFromOptionalKey(
                Schema.URLFromString.annotate({ description: 'url from applications, absent for the one application with a linked extension' }).pipe(
                    Schema.decodeTo(Schema.toType(Application), {
                        decode: SchemaGetter.transformEffect(
                            Effect.fnUntraced(
                                function* (url: URL) {
                                    const found = yield* sender.application({ url }).pipe(
                                        Effect.asSome,
                                        Effect.catchTag('ScriptingError', () => Effect.succeedNone),
                                    );
                                    const candidates = Option.isSome(found) ? [] : (yield* sender.applications({ running: false })).map((row) => row.url.href);
                                    return yield* Effect.fromOption(found, () => Unmatched.make({ subject: 'installed application', query: url.href, candidates }));
                                },
                                (located, url) => Effect.mapError(located, ({ message }) => new SchemaIssue.InvalidValue({ message }, url)),
                            ),
                        ),
                        encode: SchemaGetter.transform(Struct.get('url')),
                    }),
                ),
            );
            const panel = yield* extension(galley.name, {
                fields: { application },
                select: ({ application: given }) => Option.match(given, { onNone: () => Option.some(Function.constTrue), onSome: (row) => Option.liftPredicate(serves(row), () => row.identifier === galley.identifier) }),
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
            const steps: Record.ReadonlyRecord<string, { readonly command: string; readonly arguments: Schema.JsonObject }> = {
                'com.adobe.InDesign': {
                    command: 'do script',
                    arguments: {
                        language: 'javascript',
                        direct: `(function (source, name, preview) {
    var rest = Array.prototype.slice.call(arguments, 3), opened = {}, done = false, out, at;
    for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
    try {
        app.doScript(function () { out = (function () { return eval(source); }).apply(null, rest); done = true; if (preview) throw new Error(name); }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT, name);
    } catch (error) {
        if (!(done && preview)) throw error;
    }
    for (at = app.documents.length - 1; preview && at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.NO);
    return out;
}).apply(null, arguments)`,
                    },
                },
                'com.adobe.Photoshop': {
                    command: 'do javascript',
                    arguments: {
                        direct: `(function (source, name, preview) {
    var rest = Array.prototype.slice.call(arguments, 3), opened = {}, document = app.documents.length ? app.activeDocument : null, state = document && document.activeHistoryState, at;
    if (preview && !document) throw new Error('Photoshop records history per document and none is open, open one to preview');
    for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
    var undo = function () {
        for (at = app.documents.length - 1; at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.DONOTSAVECHANGES);
        if (document) { app.activeDocument = document; document.activeHistoryState = state; }
    };
    $.global.atelier = function () { $.global.atelier.out = (function () { return eval(source); }).apply(null, rest); };
    try {
        if (document) document.suspendHistory(name, 'atelier()'); else $.global.atelier();
    } catch (error) {
        undo();
        throw error;
    }
    if (preview) undo();
    return $.global.atelier.out;
}).apply(null, arguments)`,
                    },
                },
            };
            const Command = Schema.TaggedStruct('Command', {
                command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
                arguments: Schema.JsonObject.annotate({ description: 'Dictionary parameters by name in the JSON form replies use, direct for the direct parameter. Reply values go into later requests unchanged' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                artifacts: Schema.Array(Schema.URLFromString)
                    .annotate({ description: 'File URLs the command writes, returned with the reply' })
                    .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            }).annotate({ description: 'Scripting dictionary command sent as an Apple Event' });
            yield* McpServer.registerResource(
                dictionary,
                McpSchema.param('application', Schema.URL),
            )({
                name: 'dictionary',
                description: 'Commands, classes, and enumerations of a scriptable application',
                mimeType: 'application/xml',
                annotations: Context.make(Instructions, ['- adobe://dictionary/{application}: commands, classes, and enumerations of a scriptable application.']),
                completion: { application: () => Effect.map(Effect.orDie(sender.applications({ running: false })), (rows) => rows.filter(Struct.get('scriptable')).map(Struct.get('url'))) },
                content: (_uri, url) => sender.definition({ application: url }),
            });
            yield* register(
                Tool.make('applications', {
                    description: 'Lists installed Adobe applications an agent can act on, with the url other tools take',
                    success: Schema.Struct({ applications: Schema.Array(Application.pipe(Schema.fieldsAssign({ dictionary: Schema.OptionFromOptionalKey(Schema.String), extension: Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.String, linked: Schema.Boolean })) }))) }),
                })
                    .annotateMerge(READING)
                    .annotate(Instructions, ['- applications: each application with the url other tools take as application. Tools given no application act on the one application with a linked extension.']),
                () => Effect.map(listed(false), (applications) => ({ applications })),
            );
            yield* register(
                Tool.make('screenshot', {
                    description: 'Captures a window of the application at its native pixel size: the given one, else its open dialog or alert, else its front window',
                    parameters: Schema.Struct({ application, window: Schema.OptionFromOptionalKey(Window.fields.id.annotate({ description: 'Window id from a previous screenshot result' })) }),
                    success: Schema.Struct({ window: Window, windows: Schema.Array(Window), image: Image('image/jpeg') }),
                })
                    .annotateMerge(READING)
                    .annotate(Instructions, ['- screenshot: a window or open dialog of an application as an image.']),
                Effect.fnUntraced(function* ({ application: given, window: id }) {
                    const chosen = yield* target(given);
                    const windows = yield* sender.windows({ application: chosen.url });
                    const [candidates, query] = [windows.map(({ id: each, title }) => [each, ...Option.toArray(title)].join(' ')), Option.match(id, { onNone: () => '', onSome: (each) => `${each}` })];
                    const window = yield* Effect.fromOption(Option.isSome(id) ? Array.findFirst(windows, (each) => each.id === id.value) : Option.firstSomeOf([Array.findFirst(windows, ({ level }) => level === 'modal'), Array.findFirst(windows, ({ level }) => level === 'normal')]), () =>
                        Unmatched.make({ subject: `window of ${chosen.name}`, query, candidates }),
                    );
                    return { window, windows, image: yield* sender.capture({ window: window.id }) };
                }),
            );
            yield* register(
                Tool.make('font', {
                    description: "Reads an installed face from its file: names, metrics in em, features, axes, named instances, and characters it lacks. An application's extension adds that application's view of the face",
                    parameters: Options.pipe(Schema.fieldsAssign({ name: Schema.String.annotate({ description: 'Face by PostScript name, full name, or family for its Regular style, in any case' }) })),
                    success: Face.pipe(Schema.fieldsAssign({ metrics: Font })),
                })
                    .annotateMerge(READING)
                    .annotate(Instructions, ["- font: an installed face's metrics, features, axes, and glyph coverage."]),
                Effect.fnUntraced(function* ({ name, ...options }) {
                    const faces = yield* sender.faces();
                    const face = yield* Effect.fromOption(
                        Array.findFirst(faces, (each) => [each.postscriptName, each.fullName, ...(each.style === 'Regular' ? [each.family] : [])].map(String.toLowerCase).includes(String.toLowerCase(name))),
                        () => Unmatched.make({ subject: 'installed face', query: name, candidates: faces.map(Struct.get('postscriptName')) }),
                    );
                    return { ...face, metrics: yield* read(face.file, face.postscriptName, options) };
                }),
            );
            yield* register(
                Tool.make('execute', {
                    description: "Runs a scripting dictionary command, or a form an application's extension adds, in one application",
                    parameters: Schema.Struct({
                        description: Activity.fields.description.annotate({ description: 'Present-progressive sentence shown while the call runs, naming its undo step' }),
                        application,
                        preview: Schema.OptionFromOptionalKey(Schema.Literal(true).annotate({ description: 'Runs the call, then undoes every change, returning its result or refusal' })),
                        run: Command,
                    }),
                    success: Schema.Struct({ reply: Schema.Json, artifacts: Schema.Array(Attachment.schema), unread: Schema.Array(Schema.String).annotate({ description: 'Why each artifact the reply does not carry could not be read' }) }),
                })
                    .annotate(Tool.OpenWorld, false)
                    .annotate(Instructions, [
                        "- execute: runs a scripting dictionary command, or a form an application's extension adds, in one application. preview undoes it where the application records an undo step. A call failing because the Apple Events sender stopped may have run, read the application's state before repeating it.",
                    ]),
                Effect.fnUntraced(function* ({ application: given, description, preview, run: { command, arguments: parameters, artifacts } }) {
                    const chosen = yield* target(given);
                    const step = Option.filter(Record.get(steps, chosen.identifier), (row) => row.command === command && Predicate.isString(parameters.direct) && URL.parse(parameters.direct)?.protocol !== 'file:' && (parameters.language ?? row.arguments.language) === row.arguments.language);
                    const sent = yield* Option.match(step, {
                        onSome: (row) => Effect.succeed({ ...parameters, ...row.arguments, 'with arguments': [parameters.direct ?? null, description, Option.isSome(preview), ...Array.ensure(parameters['with arguments'] ?? [])] }),
                        onNone: () => (Option.isSome(preview) ? Effect.fail(new Cause.IllegalArgumentError(`${chosen.name} records no undo step for ${command}, run it without preview`)) : Effect.succeed(parameters)),
                    });
                    const reply = yield* sender.execute({ application: chosen.url, command, arguments: sent });
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
