import { Installation, Pid, Screenshot } from '@rasm/automation/applications.ts';
import { Automation } from '@rasm/automation/automation.ts';
import { Image } from '@rasm/mcp/content.ts';
import { registerResource } from '@rasm/mcp/resources.ts';
import { READ_ONLY, registerRoutedTool } from '@rasm/mcp/tools.ts';
import { Context, Effect, Function, Schema, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { routes } from '../routes.ts';
import type { status } from '../status.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Automation.use((automation) =>
    registerResource({
        uri: 'adobe://applications',
        name: 'applications',
        description: 'Installed Adobe applications; each name is the application other tools take',
        mimeType: 'application/json',
        schema: Schema.Array(Installation),
        changes: SubscriptionRef.changes(automation.installations),
        content: automation.installed,
    }),
);
const launch = ({ application, listed }: Effect.Success<ReturnType<typeof routes>>, { reported }: Effect.Success<typeof status>) =>
    registerRoutedTool({
        ...application,
        fields: { application: listed(Function.constTrue) },
        resolve: ({ application: given }) => Automation.use((automation) => Effect.flatMap(automation.located(given), ({ url }) => automation.launch({ url }))),
        depends: ({ pid }) => Effect.succeed([reported(pid)]),
    })(
        Tool.make('launch', {
            description: "Launches the application's own bundle in the background, never another running version, and returns its process once launched or already running",
            parameters: Schema.Struct({}),
            success: Schema.Struct({ pid: Pid }),
        }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false))),
        (_, { pid }) => Effect.succeed({ pid }),
    );
const screenshot = ({ application }: Effect.Success<ReturnType<typeof routes>>) =>
    Automation.use((automation) =>
        registerRoutedTool(application)(
            Tool.make('screenshot', {
                description: "Captures the application's on-screen windows as one PNG without raising them",
                parameters: Screenshot,
                success: Screenshot.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
            }).annotateMerge(READ_ONLY),
            (capture, { pid }) => automation.screenshot({ ...capture, pid }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, launch, screenshot };
