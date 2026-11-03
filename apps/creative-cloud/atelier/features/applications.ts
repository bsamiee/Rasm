import { Installation, Perform, Pid, Screenshot } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { HostError } from '@rasm/automation/requests';
import { Content } from '@rasm/mcp/content';
import { Resource } from '@rasm/mcp/resource';
import { Route } from '@rasm/mcp/route';
import { registerTool } from '@rasm/mcp/tool';
import { Context, Effect, FileSystem, Path, Schema, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { imageSize } from 'image-size';
import type { routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

class Capture extends Schema.Class<Capture, { readonly brand: unique symbol }>('Capture')({ image: Content.image('image/png'), width: Schema.Int, height: Schema.Int }) {
    static readonly written = <A, E, R>(write: (file: URL) => Effect.Effect<A, E, R>) =>
        Effect.scoped(
            Effect.gen(function* () {
                const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const file = yield* files.makeTempFileScoped({ suffix: '.png' });
                const written = yield* Effect.flatMap(path.toFileUrl(file), write);
                const image = yield* files.readFile(file);
                const { width, height } = yield* Effect.try({ try: () => imageSize(image), catch: HostError.refused(`PNG size of ${file}`) });
                return [Capture.make({ image, width, height }), written] as const;
            }),
        );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Automation.use((automation) =>
    Resource.register({
        name: 'applications',
        description: 'Installed Adobe applications, each name an application value',
        schema: Schema.Array(Installation),
        changes: SubscriptionRef.changes(automation.installations),
        content: automation.installed,
    }),
);
const launch = ({ application }: Effect.Success<ReturnType<typeof routes>>, { reported }: Effect.Success<typeof status>) =>
    Automation.use((automation) =>
        Route.register({
            fields: { application: application.listed },
            resolve: ({ application: given }) => Effect.flatMap(application.named(given), ({ url }) => automation.launch({ url })),
            depends: ({ pid }) => Effect.succeed([reported(pid)]),
            annotations: Context.omit(registerTool.Visible)(application.annotations),
        })(
            Tool.make('launch', {
                description: 'Opens application hidden in background beside other versions until quit, returns pid once launching ends or a dialog requires an answer',
                parameters: Schema.Struct({}),
                success: Schema.Struct({ pid: Pid }),
            }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false))),
            (_, launched) => Effect.succeed(launched),
        ),
    );
const screenshot = ({ application }: Effect.Success<ReturnType<typeof routes>>) =>
    Automation.use((automation) =>
        Route.register(application)(
            Tool.make('screenshot', {
                description: 'Captures on-screen windows as one PNG without raising them, with their window IDs',
                parameters: Screenshot,
                success: Schema.Struct({ ...Capture.fields, ...Screenshot.Result.fields }),
            }).annotate(Tool.Readonly, true),
            (shot, { pid }) =>
                Effect.map(
                    Capture.written((file) => automation.screenshot({ ...shot, pid, file })),
                    ([capture, { windows }]) => ({ ...capture, windows }),
                ),
        ),
    );
const perform = ({ application }: Effect.Success<ReturnType<typeof routes>>, { launching }: Effect.Success<typeof status>) =>
    Automation.use((automation) =>
        Route.register({ ...application, depends: ({ pid }) => Effect.succeed([launching(pid)]) })(
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
