import { Installation, Launch, Launched, Perform, Screenshot, State, WindowID } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import { HostError } from '@rasm/automation';
import { Content, Resource, Route, Server } from '@rasm/mcp';
import { Context, Effect, FileSystem, Path, Record, Result, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { imageSize } from 'image-size';
import { Adobe } from '#/adobe';
import type { routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

class Capture extends Schema.Class<Capture, { readonly brand: unique symbol }>('Capture')({ image: Content.image('image/png'), width: Schema.Int, height: Schema.Int }) {
    static readonly read = Effect.fnUntraced(function* (file: string) {
        const image = yield* FileSystem.FileSystem.use((files) => files.readFile(file));
        const { width, height } = yield* Effect.try({ try: () => imageSize(image), catch: HostError.refused(`PNG size of ${file}`) });
        return Capture.make({ image, width, height });
    });
    static readonly written = Effect.fnUntraced(function* <Input extends object, A, E, R>(input: Input, write: (request: Input & { readonly file: URL }) => Effect.Effect<A, E, R>) {
        const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const file = yield* files.makeTempFileScoped({ suffix: '.png' });
        yield* write({ ...input, file: yield* path.toFileUrl(file) });
        return yield* Capture.read(file);
    }, Effect.scoped);
}

const Installations = Schema.Array(Installation);
const Parameterless = Schema.Struct({});
const Captured = Schema.Record(Schema.String.annotate({ description: 'Window number' }), Schema.Result(Capture, Schema.ErrorInstance()));
const Reading = Schema.Struct({ window: Schema.optionalKey(WindowID.annotate({ description: 'Walks that row alone, absent walks every element' })) });
const Quitting = Schema.Struct({ force: Schema.Boolean.annotate({ description: 'Kills instance without asking, for one no answer or quit frees' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Effect.map(Adobe, ({ installations, installed }) =>
    Resource.register({
        name: 'applications',
        description: 'Installed Adobe applications, each name an application value',
        schema: Installations,
        changes: SubscriptionRef.changes(installations),
        content: installed,
    }),
).pipe(Effect.flatten);
const control = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>, { reported, launching }: Effect.Success<typeof status>) {
    const [automation, { launched }] = yield* Effect.all([Automation, Adobe]);
    yield* Route({
        fields: { application: application.listed, ...Struct.omit(Launch.fields, ['url']) },
        resolve: ({ application: given, ...options }) => Effect.flatMap(application.named(given), ({ url }) => launched({ url, ...options })),
        depends: ({ instance }) => Effect.succeed([reported(instance)]),
        annotations: Context.omit(Server.Visible)(application.annotations),
    })(
        Tool.make('launch', {
            description: 'Opens the application without activating it, below the frontmost window, and reuses a running instance unless new is set. Arguments and environment reach only an instance this call starts. Returns when launching ends or a dialog needs an answer',
            parameters: Parameterless,
            success: Launched,
        }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false))),
        (_, opened) => Effect.succeed(opened),
    );
    yield* Route(application)(
        Tool.make('read', {
            description: 'Reads application state with element ids and controls for perform',
            parameters: Reading,
            success: State,
        }).annotate(Tool.Readonly, true),
        (given, { instance }) => automation.read({ ...given, instance }),
    );
    yield* Route(application)(
        Tool.make('screenshot', {
            description: 'Captures each presented window, or one window, as its own PNG with no activation, background and covered windows included',
            parameters: Screenshot,
            success: Captured,
        }).annotate(Tool.Readonly, true),
        Effect.fnUntraced(function* (shot, { instance }) {
            const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const directory = yield* files.makeTempDirectoryScoped();
            const written = yield* automation.screenshot({ ...shot, instance, directory: yield* path.toFileUrl(directory) });
            return yield* Effect.all(Record.map(written, Result.match({ onSuccess: (file) => Effect.result(Effect.flatMap(path.fromFileUrl(file), Capture.read)), onFailure: (error) => Effect.succeed(Result.fail<Error>(error)) })));
        }, Effect.scoped),
    );
    yield* Route({ ...application, depends: ({ instance }) => Effect.succeed([launching(instance)]) })(
        Tool.make('perform', {
            description: 'Runs Accessibility actions on read elements in order, refused while application launches, returning the state after the last',
            parameters: Perform,
            success: State,
        }),
        (given, { instance }) => automation.perform({ ...given, instance }),
    );
    yield* Route({ ...application, depends: () => Effect.succeed([]) })(
        Tool.make('quit', {
            description: 'Sends application the quit event, returning its last state, a dialog or unresponsive instance staying running',
            parameters: Quitting,
            success: State,
        }).annotate(Tool.Destructive, true),
        (given, { instance }) => automation.quit({ ...given, instance }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, Capture, control };
