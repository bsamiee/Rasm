import { Installation, Perform, Pid, Screenshot } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Image } from '@rasm/mcp/content';
import { registerResource } from '@rasm/mcp/resource';
import { Visible } from '@rasm/mcp/server';
import { registerRoutedTool } from '@rasm/mcp/tool';
import { Unmatched } from '@rasm/search/unmatched';
import { Context, Effect, FileSystem, Option, Path, Schema, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { imageSize } from 'image-size';
import type { routes } from '#/routes';
import type { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

class Capture extends Schema.Class<Capture, { readonly brand: unique symbol }>('Capture')({ image: Image('image/png'), width: Schema.Int, height: Schema.Int }) {
    static readonly written = <A, E, R>(write: (file: string) => Effect.Effect<A, E, R>) =>
        Effect.scoped(
            Effect.gen(function* () {
                const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const file = path.join(yield* files.makeTempDirectoryScoped(), 'capture.png');
                const written = yield* write(file);
                const image = yield* files.readFile(file);
                const { width, height } = yield* Effect.sync(() => imageSize(image));
                return [Capture.make({ image, width, height }), written] as const;
            }),
        );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Automation.use((automation) =>
    registerResource({
        path: 'applications',
        name: 'applications',
        description: 'Installed Adobe applications, each name an application value',
        mimeType: 'application/json',
        schema: Schema.Array(Installation),
        changes: SubscriptionRef.changes(automation.installations),
        content: automation.installed,
    }),
);
const launch = ({ application, listed }: Effect.Success<ReturnType<typeof routes>>, { reported }: Effect.Success<typeof status>) =>
    registerRoutedTool({
        ...application,
        fields: { application: listed(() => true) },
        resolve: ({ application: given }) =>
            Automation.use((automation) =>
                automation.installed.pipe(
                    Effect.flatMap(Unmatched.single('installed Adobe application', Option.some(given), (row: Installation) => [row.name])),
                    Effect.flatMap(({ url }) => automation.launch({ url })),
                ),
            ),
        depends: ({ pid }) => Effect.succeed([reported(pid)]),
        serves: () => true,
    })(
        Tool.make('launch', {
            description: 'Launches application in background beside other versions, returns pid once running',
            parameters: Schema.Struct({}),
            success: Schema.Struct({ pid: Pid }),
        }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false), Context.add(Visible, Effect.succeed(true)))),
        (_, { pid }) => Effect.succeed({ pid }),
    );
const screenshot = Effect.fnUntraced(function* ({ application }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, path] = yield* Effect.all([Automation, Path.Path]);
    const captured = (shot: Screenshot, pid: number) => Capture.written((file) => Effect.flatMap(path.toFileUrl(file), (url) => automation.screenshot({ ...shot, pid, file: url })));
    yield* registerRoutedTool(application)(
        Tool.make('screenshot', {
            description: 'Captures on-screen windows as one PNG without raising them, with their window IDs',
            parameters: Screenshot,
            success: Schema.Struct({ ...Capture.fields, ...Screenshot.Result.fields }),
        }).annotate(Tool.Readonly, true),
        (shot, { pid }) => Effect.map(captured(shot, pid), ([capture, { windows }]) => ({ ...capture, windows })),
    );
});
const perform = ({ application }: Effect.Success<ReturnType<typeof routes>>, { launch: launching }: Effect.Success<typeof status>) =>
    Automation.use((automation) =>
        registerRoutedTool({ ...application, depends: ({ pid }) => Effect.succeed([launching(pid)]) })(
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
