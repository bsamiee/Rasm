import { HostError, Installation, Launched, Perform, Screenshot } from '@rasm/automation';
import { Automation } from '@rasm/automation/node';
import { Content, Resource, Route, Server } from '@rasm/mcp';
import { Context, Effect, FileSystem, flow, Path, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { imageSize } from 'image-size';
import type { routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

class Capture extends Schema.Class<Capture, { readonly brand: unique symbol }>('Capture')({ image: Content.image('image/png'), width: Schema.Int, height: Schema.Int }) {
    static readonly written = Effect.fnUntraced(function* <Input extends object, A, E, R>(input: Input, write: (request: Input & { readonly file: URL }) => Effect.Effect<A, E, R>) {
        const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const file = yield* files.makeTempFileScoped({ suffix: '.png' });
        const written = yield* write({ ...input, file: yield* path.toFileUrl(file) });
        const image = yield* files.readFile(file);
        const { width, height } = yield* Effect.try({ try: () => imageSize(image), catch: HostError.refused(`PNG size of ${file}`) });
        return [Capture.make({ image, width, height }), written] as const;
    }, Effect.scoped);
}

const Installations = Schema.Array(Installation);
const Parameterless = Schema.Struct({});
const Captured = Screenshot.Result.mapFields(Struct.assign(Capture.fields));

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Automation.use((automation) =>
    Resource.register({
        name: 'applications',
        description: 'Installed Adobe applications, each name an application value',
        schema: Installations,
        changes: SubscriptionRef.changes(automation.installations),
        content: automation.installed,
    }),
);
const launch = ({ application }: Effect.Success<ReturnType<typeof routes>>, { reported }: Effect.Success<typeof status>) =>
    Automation.use((automation) =>
        Route({
            fields: { application: application.listed },
            resolve: ({ application: given }) => Effect.flatMap(application.named(given), ({ url }) => automation.launch({ url })),
            depends: ({ pid }) => Effect.succeed([reported(pid)]),
            annotations: Context.omit(Server.Visible)(application.annotations),
        })(
            Tool.make('launch', {
                description: 'Opens application hidden beside other versions until quit. Returns once launching ends or a dialog requires an answer',
                parameters: Parameterless,
                success: Launched,
            }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false))),
            (_, launched) => Effect.succeed(launched),
        ),
    );
const screenshot = ({ application }: Effect.Success<ReturnType<typeof routes>>) =>
    Automation.use((automation) =>
        Route(application)(
            Tool.make('screenshot', {
                description: 'Captures on-screen windows as one PNG without raising them, with their window IDs',
                parameters: Screenshot,
                success: Captured,
            }).annotate(Tool.Readonly, true),
            flow(
                (shot, { pid }) => Capture.written({ ...shot, pid }, automation.screenshot),
                Effect.map(([capture, { windows }]) => ({ ...capture, windows })),
            ),
        ),
    );
const perform = ({ application }: Effect.Success<ReturnType<typeof routes>>, { launching }: Effect.Success<typeof status>) =>
    Automation.use((automation) =>
        Route({ ...application, depends: ({ pid }) => Effect.succeed([launching(pid)]) })(
            Tool.make('perform', {
                description: 'Runs Accessibility actions on status elements in order, refused while application launches',
                parameters: Perform,
                success: Schema.Void,
            }),
            (given, { pid }) => automation.perform({ ...given, pid }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, Capture, launch, perform, screenshot };
