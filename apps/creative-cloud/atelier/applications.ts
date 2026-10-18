import { Peer } from '@rasm/indesign/contract';
import { Image, Instructions, READING, Unmatched } from '@rasm/mcp/link';
import { register } from '@rasm/mcp/server';
import { Font, Options, read } from '@rasm/typography/font';
import { Array, Context, Effect, FileSystem, Function, Option, Path, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { Application, Face, Scripting, Size, Window } from './scripting.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const serves = (row: typeof Application.Type) => (peer: typeof Peer.Type) => row.running && Option.exists(Option.flatMap(row.version, Schema.decodeOption(Peer.fields.version)), (major) => major === peer.version);
const installed = Effect.gen(function* () {
    const sender = yield* Scripting;
    const located = Effect.fnUntraced(function* (url: URL) {
        const found = yield* sender.application({ url }).pipe(
            Effect.asSome,
            Effect.catchTag('ScriptingError', () => Effect.succeedNone),
        );
        const rows = yield* Option.isSome(found) ? Effect.succeed([]) : sender.applications({ running: false });
        const candidates = rows.map(({ url: each }) => each.href);
        return yield* Effect.fromOption(found, () => Unmatched.make({ subject: 'installed application', query: url.href, candidates }));
    });
    const application = Schema.OptionFromOptionalKey(
        Schema.URLFromString.annotate({ description: 'url from applications, absent for the one application with a linked extension' }).pipe(
            Schema.decodeTo(Schema.toType(Application), {
                decode: SchemaGetter.transformEffect((url: URL) => Effect.mapError(located(url), ({ message }) => new SchemaIssue.InvalidValue({ message }, url))),
                encode: SchemaGetter.transform(Struct.get('url')),
            }),
        ),
    );
    return {
        application,
        route: (identifier: string) => ({
            fields: { application },
            select: ({ application: given }: { readonly application: Option.Option<typeof Application.Type> }) => Option.match(given, { onNone: () => Option.some(Function.constTrue), onSome: (row) => Option.liftPredicate(serves(row), () => row.identifier === identifier) }),
        }),
    };
});
const applications = Effect.fn('applications')(function* (application: Effect.Success<typeof installed>['application'], extensions: readonly { readonly name: string; readonly identifier: string; readonly peers: Effect.Effect<readonly (typeof Peer.Type)[]> }[]) {
    const [sender, files, path] = yield* Effect.all([Scripting, FileSystem.FileSystem, Path.Path]);
    const dictionary = ((segments: TemplateStringsArray, ..._: readonly string[]) => segments)`adobe://dictionary/${'application'}`;
    const entry = Effect.fnUntraced(function* (row: typeof Application.Type) {
        const owner = Array.findFirst(extensions, ({ identifier }) => identifier === row.identifier);
        const peers = yield* Effect.transposeOption(Option.map(owner, Struct.get('peers')));
        return {
            ...row,
            dictionary: Option.map(Option.liftPredicate(row, Struct.get('scriptable')), ({ url }) => dictionary.join(encodeURIComponent(url.href))),
            extension: Option.map(Option.all({ owner, peers }), ({ owner: { name }, peers: linked }) => ({ name, linked: linked.some(serves(row)) })),
        };
    });
    const listed = Effect.fnUntraced(function* (running: boolean) {
        const rows = yield* Effect.forEach(yield* sender.applications({ running }), entry, { concurrency: 'unbounded' });
        return rows.filter(({ scriptable, extension }) => scriptable || Option.exists(extension, Struct.get('linked')));
    });
    const target = Effect.fnUntraced(function* (given: Option.Option<typeof Application.Type>) {
        const rows = yield* Option.isSome(given) ? Effect.succeed([]) : listed(true);
        const linked = rows.filter(({ extension }) => Option.exists(extension, Struct.get('linked')));
        const single = linked.length === 1 ? Array.head(linked) : Option.none();
        const candidates = (linked.length === 0 ? rows : linked).map(({ url }) => url.href);
        return yield* Effect.fromOption(
            Option.orElse(given, () => single),
            () => Unmatched.make({ subject: 'single application with a linked extension', query: '', candidates }),
        );
    });
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
            success: Schema.Struct({
                applications: Schema.Array(Application.mapFields(Struct.omit(['scriptable', 'identifier'])).pipe(Schema.fieldsAssign({ dictionary: Schema.OptionFromOptionalKey(Schema.String), extension: Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.String, linked: Schema.Boolean })) }))),
            }),
        })
            .annotateMerge(READING)
            .annotate(Instructions, ['- applications: each application with the url other tools take as application. Tools given no application act on the one application with a linked extension.']),
        () => Effect.map(listed(false), (rows) => ({ applications: rows })),
    );
    yield* register(
        Tool.make('screenshot', {
            description: 'Captures a window of the application at its native pixel size: the given one, else its open dialog or alert, else its front window',
            parameters: Schema.Struct({ application, window: Schema.OptionFromOptionalKey(Window.fields.id.annotate({ description: 'Window id from a previous screenshot result' })) }),
            success: Size.pipe(Schema.fieldsAssign({ window: Window, windows: Schema.Array(Window), image: Image('image/jpeg') })),
        })
            .annotateMerge(READING)
            .annotate(Instructions, ['- screenshot: a window or open dialog of an application as an image.']),
        Effect.fn('screenshot')(function* (parameters) {
            const chosen = yield* target(parameters.application);
            const windows = yield* sender.windows({ application: chosen.url });
            const [given, dialog, front] = [Array.findFirst(windows, ({ id }) => Option.contains(parameters.window, id)), Array.findFirst(windows, ({ level }) => level === 'modal'), Array.findFirst(windows, ({ level }) => level === 'normal')];
            const candidates = windows.map(({ id, title }) => [id, ...Option.toArray(title)].join(' '));
            const query = Option.match(parameters.window, { onNone: () => '', onSome: globalThis.String });
            const window = yield* Effect.fromOption(Option.isSome(parameters.window) ? given : Option.orElse(dialog, () => front), () => Unmatched.make({ subject: `window of ${chosen.name}`, query, candidates }));
            const file = yield* files.makeTempFileScoped({ suffix: '.jpg' });
            const size = yield* sender.capture({ window: window.id, url: yield* path.toFileUrl(file) });
            return { ...size, window, windows, image: yield* files.readFile(file) };
        }, Effect.scoped),
    );
    yield* register(
        Tool.make('font', {
            description: "Reads an installed face from its file: names, metrics in em, features, axes, named instances, and characters it lacks. An application's extension adds that application's view of the face",
            parameters: Options.pipe(Schema.fieldsAssign({ name: Schema.String.annotate({ description: 'Face by PostScript name, full name, or family for its Regular style, in any case' }) })),
            success: Face.pipe(Schema.fieldsAssign({ metrics: Font })),
        })
            .annotateMerge(READING)
            .annotate(Instructions, ["- font: an installed face's metrics, features, axes, and glyph coverage."]),
        Effect.fn('font')(function* ({ name, ...options }) {
            const faces = yield* sender.faces();
            const face = yield* Effect.fromOption(
                Array.findFirst(faces, (each) => [each.postscriptName, each.fullName, ...(each.style === 'Regular' ? [each.family] : [])].map(String.toLowerCase).includes(String.toLowerCase(name))),
                () => Unmatched.make({ subject: 'installed face', query: name, candidates: faces.map(Struct.get('postscriptName')) }),
            );
            return { ...face, metrics: yield* read(face.file, face.postscriptName, options) };
        }),
    );
    return target;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, installed };
