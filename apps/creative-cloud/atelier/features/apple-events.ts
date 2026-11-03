import { Execute, Kind, Terms } from '@rasm/automation/apple-events';
import { Automation } from '@rasm/automation/automation';
import { Attachment } from '@rasm/mcp/content';
import { registerResourceTemplate } from '@rasm/mcp/resource';
import { registerRoutedTool } from '@rasm/mcp/tool';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const Dictionary = Schema.Struct({ application: McpSchema.param('application', Schema.String), kind: McpSchema.param('kind', Kind) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const dictionaries = Effect.fnUntraced(function* ({ scriptable }: Effect.Success<ReturnType<typeof routes>>) {
    const automation = yield* Automation;
    const described = (given: string) => scriptable.holding.pipe(Effect.flatMap(Unmatched.single('scriptable Adobe application', Option.some(given), (row) => [row.name])));
    const completion = { application: () => Effect.map(scriptable.holding, Array.map(Struct.get('name'))) };
    yield* registerResourceTemplate`dictionary/${Dictionary.fields.application}`({
        name: 'dictionary',
        description: SchemaAST.resolveDescription(Terms.ast),
        mimeType: 'application/json',
        schema: Terms,
        completion,
        changes: SubscriptionRef.changes(automation.installations),
        content: (_, given) => Effect.flatMap(described(given), automation.terms),
    });
    yield* registerResourceTemplate`dictionary/${Dictionary.fields.application}/${Dictionary.fields.kind}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML matching kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes: SubscriptionRef.changes(automation.installations),
        completion: {
            ...completion,
            kind: () => Effect.succeed([...Kind.literals]),
            name: Effect.fnUntraced(function* (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) {
                const chosen = yield* Schema.decodeEffect(Schema.toCodecStringTree(Dictionary))(context?.arguments);
                return Array.map((yield* automation.terms(yield* described(chosen.application)))[chosen.kind], Struct.get('name'));
            }),
        },
        content: (_, given, selected, term) => Effect.flatMap(described(given), ({ url }) => automation.definition({ url, kind: selected, name: term })),
    });
});
const command = ({ scriptable }: Effect.Success<ReturnType<typeof routes>>) =>
    Automation.use((automation) =>
        registerRoutedTool(scriptable)(
            Tool.make('command', {
                description: 'Sends scripting dictionary command, refused while a dialog requires an answer',
                parameters: Execute,
                success: Execute.Result.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Attachment(Execute.Artifact), Schema.ErrorInstance())) })),
            }),
            (given, { pid }) => automation.execute({ ...given, pid }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { command, dictionaries };
