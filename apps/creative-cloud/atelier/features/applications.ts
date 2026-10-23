import { Automation, Installation, Pid, Screenshot } from '@rasm/automation';
import { Image, READ_ONLY, registerResource, registerRoutedTool } from '@rasm/mcp';
import { Context, Effect, Function, Schema, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import type { routes } from '#/routes';
import type { status } from '#/status';

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Automation.use((automation) =>
    registerResource({
        uri: 'adobe://applications',
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
        fields: { application: listed(Function.constTrue) },
        resolve: ({ application: given }) => Automation.use((automation) => Effect.flatMap(automation.located(given), ({ url }) => automation.launch({ url }))),
        depends: ({ pid }) => Effect.succeed([reported(pid)]),
    })(
        Tool.make('launch', {
            description: 'Launches bundle in background beside other versions, returns pid once running',
            parameters: Schema.Struct({}),
            success: Schema.Struct({ pid: Pid }),
        }).annotateMerge(Context.make(Tool.Idempotent, true).pipe(Context.add(Tool.Destructive, false))),
        (_, { pid }) => Effect.succeed({ pid }),
    );
const screenshot = ({ application }: Effect.Success<ReturnType<typeof routes>>) =>
    Automation.use((automation) =>
        registerRoutedTool(application)(
            Tool.make('screenshot', {
                description: 'Captures on-screen windows as one PNG without raising them',
                parameters: Screenshot,
                success: Screenshot.Result.pipe(Schema.fieldsAssign({ image: Image('image/png') })),
            }).annotateMerge(READ_ONLY),
            (capture, { pid }) => automation.screenshot({ ...capture, pid }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, launch, screenshot };
