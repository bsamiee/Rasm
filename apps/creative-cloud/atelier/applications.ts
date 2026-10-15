import { type Extension, make, READING, ranked, register, type routed } from '@rasm/mcp/extension';
import { fitted, Tokens } from '@rasm/mcp/vision';
import { CHARACTERS, Font, FontError, Options, read } from '@rasm/typography/font';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, Struct } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { Application, Face, Scripting, ScriptingError, Window } from './scripting.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Shown = Window.mapFields(Struct.pick(['id', 'title']));
const Atelier = make(
    {
        name: 'Atelier',
        serves: Application,
        instructions: Array.join(
            [
                'Atelier drives installed Adobe applications.',
                '- applications: lists each application with the url other tools take as application; a tool given no application acts on the one application with a linked extension.',
                "- execute: runs a scripting dictionary command, or a form an application's extension adds, in one application; preview undoes it where the application records an undo step.",
                '- screenshot: a window or open dialog of an application as an image.',
                "- font: an installed face's metrics, features, axes, and glyph coverage.",
                '- adobe://dictionary/{application}: commands, classes, and enumerations of a scriptable application.',
                '- adobe://executions: results of calls that change an application, cancelled before their response.',
            ],
            '\n',
        ),
        preview: Schema.Never,
        runs: {},
        tools: {
            screenshot: Tool.make('screenshot', {
                description: 'Captures a window of the application as a JPEG: the given one, else its open dialog or alert, else its front window',
                parameters: Schema.Struct({ window: Schema.OptionFromOptionalKey(Shown.fields.id.annotate({ description: 'Window id from a previous screenshot result' })), tokens: Tokens }),
                success: Schema.Struct({ ...Struct.Record(['width', 'height'], Schema.Int), window: Shown, windows: Schema.Array(Shown) }),
                failure: ScriptingError,
            }).annotateMerge(READING),
        },
        activity: {},
        face: { success: Face, failure: ScriptingError },
    },
    Effect.gen(function* () {
        const [scripting, files, path] = yield* Effect.all([Scripting, FileSystem.FileSystem, Path.Path]);
        return {
            runs: {},
            tools: {
                screenshot: Effect.fn('screenshot')(function* (application: typeof Application.Type, { window, tokens }: { readonly window: Option.Option<number>; readonly tokens: number }) {
                    const listed = yield* scripting.call((sender) => sender.windows({ application: application.url }));
                    const chosen = yield* Effect.fromOption(
                        Option.match(window, {
                            onNone: () =>
                                Option.orElse(
                                    Array.findFirst(listed, ({ level }) => level === 'modal'),
                                    () => Array.findFirst(listed, ({ level }) => level === 'normal'),
                                ),
                            onSome: (id) => Array.findFirst(listed, (each) => each.id === id),
                        }),
                        () => ScriptingError.make({ reason: { _tag: 'Missing', subject: `window of ${application.name}`, candidates: Array.map(listed, ({ id, title }) => Array.join([String(id), ...Option.toArray(title)], ' ')) } }),
                    );
                    const fit = fitted(tokens, chosen.width, chosen.height);
                    const file = yield* Effect.orDie(files.makeTempFileScoped({ suffix: '.jpg' }));
                    const url = yield* Effect.orDie(path.toFileUrl(file));
                    const size = yield* scripting.call((sender) => sender.capture({ window: chosen.id, url, longest: Math.max(fit.width, fit.height) }));
                    const data = yield* Effect.orDie(files.readFile(file));
                    return [{ ...size, window: Struct.pick(chosen, ['id', 'title']), windows: Array.map(listed, Struct.pick(['id', 'title'])) }, [McpSchema.ImageContent.make({ data, mimeType: 'image/jpeg' })]] as const;
                }, Effect.scoped),
            },
            face: Effect.fn('face')(function* (_application: typeof Application.Type, name: string) {
                const faces = yield* scripting.call((sender) => sender.faces());
                const face = yield* Effect.fromOption(
                    Array.findFirst(faces, (each) => Array.some([each.postscriptName, each.fullName, ...(each.style === 'Regular' ? [each.family] : [])], (known) => known.toLowerCase() === name.toLowerCase())),
                    () => ScriptingError.make({ reason: { _tag: 'Missing', subject: `installed face ${name}`, candidates: ranked(name, Array.map(faces, Struct.get('postscriptName'))) } }),
                );
                return [face, Option.some({ file: face.file, postscriptName: face.postscriptName, coordinates: Option.none() })] as const;
            }),
        };
    }),
);
const applications = Effect.fn('applications')(function* (extensions: readonly Extension<unknown, unknown>[]) {
    const [scripting, hosts] = yield* Effect.all([Scripting, Effect.forEach(extensions, ({ name, Service }) => Effect.map(Service, ({ linked }) => ({ name, linked })))]);
    const dictionary = ((segments: TemplateStringsArray, ..._: readonly string[]): TemplateStringsArray => segments)`adobe://dictionary/${'application'}`;
    const installed = Effect.fnUntraced(function* (running: boolean) {
        return yield* scripting.call((sender) => sender.applications({ running }));
    });
    const listed = Effect.fnUntraced(function* (running: boolean) {
        const rows = yield* installed(running);
        return Array.flatten(
            yield* Effect.forEach(
                rows,
                (application) =>
                    Array.findFirst(hosts, ({ name, linked }) =>
                        Option.map(
                            linked(application),
                            Effect.map((version) => ({ name, linked: Option.isSome(version), version })),
                        ),
                    ).pipe(
                        Effect.transposeOption,
                        Effect.map((extension) => (application.scriptable || Option.exists(extension, Struct.get('linked')) ? [{ ...application, dictionary: Option.liftPredicate(String.raw(dictionary, encodeURIComponent(application.url.href)), () => application.scriptable), extension }] : [])),
                    ),
                { concurrency: 'unbounded' },
            ),
        );
    });
    yield* McpServer.registerResource(
        dictionary,
        McpSchema.param('application', Schema.URL),
    )({
        name: 'Scripting dictionary',
        description: 'Commands, classes, and enumerations of a scriptable application',
        mimeType: 'application/xml',
        completion: {
            application: (input) =>
                Effect.map(Effect.orDie(installed(false)), (rows) =>
                    Array.map(
                        ranked(
                            input,
                            Array.map(Array.filter(rows, Struct.get('scriptable')), ({ url }) => url.href),
                        ),
                        (href) => new URL(href),
                    ),
                ),
        },
        content: (_uri, application) => scripting.call((sender) => sender.definition({ application })),
    });
    yield* register(
        Tool.make('applications', {
            description: 'Lists installed Adobe applications an agent can act on, with the url other tools take',
            success: Schema.Struct({
                applications: Schema.Array(
                    Application.mapFields(Struct.omit(['scriptable', 'identifier'])).pipe(
                        Schema.fieldsAssign({ dictionary: Schema.OptionFromOptionalKey(Schema.String), extension: Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.String, linked: Schema.Boolean, version: Schema.OptionFromOptionalKey(Schema.String) })) }),
                    ),
                ),
            }),
            failure: ScriptingError,
        }).annotateMerge(READING),
        () => Effect.succeed([Option.none(), Effect.map(listed(false), (rows) => [{ applications: rows }, []] as const)] as const),
    );
    return {
        fields: Schema.Struct({ application: Schema.OptionFromOptionalKey(Schema.URL.annotateKey({ description: 'url from applications; absent, the one application with a linked extension' })) }),
        failure: ScriptingError,
        route: ({ application }: { readonly application: Option.Option<URL> }): Effect.Effect<typeof Application.Type, ScriptingError> =>
            Option.match(application, {
                onSome: (given) =>
                    scripting
                        .call((sender) => sender.application({ url: given }))
                        .pipe(
                            Effect.catchReason('ScriptingError', 'Refused', () =>
                                Effect.flatMap(installed(false), (rows) =>
                                    Effect.fail(
                                        ScriptingError.make({
                                            reason: {
                                                _tag: 'Missing',
                                                subject: `installed application ${given.href}`,
                                                candidates: ranked(
                                                    given.href,
                                                    Array.map(rows, ({ url }) => url.href),
                                                ),
                                            },
                                        }),
                                    ),
                                ),
                            ),
                        ),
                onNone: () =>
                    Effect.flatMap(listed(true), (rows) => {
                        const linked = Array.filter(rows, ({ extension }) => Option.exists(extension, Struct.get('linked')));
                        return Effect.fromOption(
                            Option.filter(Array.head(linked), () => linked.length === 1),
                            () => ScriptingError.make({ reason: { _tag: 'Missing', subject: 'single application with a linked extension', candidates: Array.map(Array.isReadonlyArrayNonEmpty(linked) ? linked : rows, ({ url }) => url.href) } }),
                        );
                    }),
            }),
        unserved: ({ name }: typeof Application.Type) => ScriptingError.make({ reason: { _tag: 'Unserved', application: name } }),
        unpreviewable: ({ name }: typeof Application.Type) => ScriptingError.make({ reason: { _tag: 'Unpreviewable', application: name } }),
    };
});
const font = Effect.fn('font')(function* ({ faces, resolve }: Effect.Success<ReturnType<typeof routed>>, { fields, failure, route, unserved }: Effect.Success<ReturnType<typeof applications>>) {
    yield* register(
        Tool.make('font', {
            description: "Reads an installed face from its file: names, metrics in em, features, axes, named instances, and characters it lacks. An application's extension adds that application's view of the face",
            parameters: fields.pipe(Schema.fieldsAssign({ name: Schema.String.annotate({ description: 'Face by PostScript name, full name, or family for its Regular style, in any case' }), ...Options.fields })),
            success: Schema.Union(Array.map(faces, ({ success }) => success.pipe(Schema.fieldsAssign({ metrics: Schema.OptionFromOptionalKey(Font) })))),
            failure: Schema.Union([failure, FontError, ...Array.map(faces, Struct.get('failure'))]),
        }).annotateMerge(READING),
        (request) =>
            Effect.succeed([
                Option.none(),
                route(request).pipe(
                    Effect.flatMap((target) => Effect.fromOption(resolve(target), () => unserved(target))),
                    Effect.flatMap((resolver) => resolver(request.name, Array.join([...Option.toArray(request.text), ...Record.values(CHARACTERS)], ''))),
                    Effect.flatMap(([face, source]) => Effect.map(Effect.transposeOption(Option.map(source, ({ file, postscriptName, coordinates }) => read(file, postscriptName, { ...request, coordinates: Option.orElse(request.coordinates, () => coordinates) }))), (metrics) => [{ ...face, metrics }, []] as const)),
                ),
            ] as const),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Atelier, applications, font };
