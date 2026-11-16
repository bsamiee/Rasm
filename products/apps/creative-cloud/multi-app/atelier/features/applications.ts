import { Capture, Installation, Launch, Launched, Launching, Perform, State, WindowID } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Content, Resource, Route, Server } from '@rasm/mcp';
import { Context, Effect, FileSystem, Path, Record, Result, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

const Installations = Schema.Array(Installation);
const Parameterless = Schema.Struct({});
const Reading = Schema.Struct({ window: Schema.optionalKey(WindowID.annotate({ description: 'Walks that row alone, absent walks every element' })) });
const Quitting = Schema.Struct({ force: Schema.Boolean.annotate({ description: 'Kills instance without asking, for one no answer or quit frees' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) });
const Image = Schema.Struct({ image: Content.image('image/png'), ...Struct.Record(['width', 'height'], Schema.Int.annotate({ description: 'Pixels' })) });
const Captured = Schema.Record(Schema.String.annotate({ description: 'Window number' }), Schema.Result(Image, Schema.ErrorInstance()));

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>, dependencies: Effect.Success<typeof status>) {
    const [server, files, path] = yield* Effect.all([ApplicationServer, FileSystem.FileSystem, Path.Path]);
    yield* Resource.register({ name: 'applications', description: 'Installed Adobe applications, each name an application value', schema: Installations, changes: SubscriptionRef.changes(server.installations), content: server.installed });
    yield* Route({
        fields: { application: application.listed, ...Struct.omit(Launch.fields, ['url']) },
        resolve: ({ application: given, ...options }) => Effect.flatMap(application.named(given), ({ url }) => Launching.launched(server.launch({ url, ...options }))),
        depends: ({ instance }) => Effect.succeed([dependencies.reported(instance)]),
        annotations: Context.omit(Server.Visible)(application.annotations),
    })(
        Tool.make('launch', {
            description: 'Opens the application without activating it, below the frontmost window, and reuses a running instance unless new is set. Arguments and environment reach only an instance this call starts. Returns when launching ends or a dialog needs an answer',
            parameters: Parameterless,
            success: Launched,
        }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false))),
        (_, opened) => Effect.succeed(opened),
    );
    yield* Route(application)(Tool.make('read', { description: 'Reads application state with element ids and controls for perform', parameters: Reading, success: State }).annotate(Tool.Readonly, true), (given, { instance }) => server.read({ ...given, instance }));
    yield* Route(application)(
        Tool.make('capture', { description: 'Captures each presented window, the given windows, or one element as its own PNG with no activation, background and covered windows included', parameters: Capture, success: Captured }).annotate(Tool.Readonly, true),
        Effect.fnUntraced(function* (given, { instance }) {
            const captured = yield* server.capture({ ...given, instance, directory: yield* path.toFileUrl(yield* files.makeTempDirectoryScoped()) });
            return Record.map(
                captured,
                Result.map(({ data, width, height }) => ({ image: data, width, height })),
            );
        }, Effect.scoped),
    );
    yield* Route({ ...application, depends: ({ instance }) => Effect.succeed([dependencies.launching(instance)]) })(
        Tool.make('perform', { description: 'Runs Accessibility actions on read elements in order, refused while application launches, returning the state after the last', parameters: Perform, success: State }),
        (given, { instance }) => server.perform({ ...given, instance }),
    );
    yield* Route({ ...application, depends: () => Effect.succeed([]) })(
        Tool.make('quit', {
            description: 'Sends application the quit event, returning its last state, a dialog or unresponsive instance staying running',
            parameters: Quitting,
            success: State,
        }).annotate(Tool.Destructive, true),
        (given, { instance }) => server.quit({ ...given, instance }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, Image };
