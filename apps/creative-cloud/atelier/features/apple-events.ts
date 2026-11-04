import { Execute, Terms } from '@rasm/automation';
import { Automation } from '@rasm/automation/node';
import { Content, Resource, Route } from '@rasm/mcp';
import { Array, Effect, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const Application = McpSchema.param('application', Schema.String);
const CommandResult = Execute.Result.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Content.attachment(Execute.Artifact), Schema.ErrorInstance())).annotate({ description: SchemaAST.resolveDescription(Execute.Result.fields.artifacts.ast) }) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const dictionaries = Effect.fnUntraced(function* ({ dictionary }: Effect.Success<ReturnType<typeof routes>>) {
    const automation = yield* Automation;
    yield* Resource.template`dictionary/${Application}`({
        name: 'dictionary',
        schema: Terms,
        completion: dictionary.completion,
        changes: SubscriptionRef.changes(automation.installations),
        content: (_, given) => Effect.flatMap(dictionary.named(given), automation.terms),
    });
    yield* Resource.template`dictionary/${Application}/${McpSchema.param('kind', Terms.Kind)}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML matching kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes: SubscriptionRef.changes(automation.installations),
        completion: {
            ...dictionary.completion,
            kind: () => Effect.succeed([...Terms.Kind.literals]),
            name: (_, { application, kind }) =>
                application === undefined || kind === undefined
                    ? Effect.succeed([])
                    : dictionary.named(application).pipe(
                          Effect.flatMap(automation.terms),
                          Effect.map((terms) => Array.map(terms[kind], Struct.get('name'))),
                      ),
        },
        content: (_, given, selected, term) => Effect.flatMap(dictionary.named(given), ({ url }) => automation.definition({ url, kind: selected, name: term })),
    });
});
const command = ({ dictionary }: Effect.Success<ReturnType<typeof routes>>) =>
    Automation.use((automation) =>
        Route(dictionary)(
            Tool.make('command', {
                description: 'Sends scripting dictionary command, refused while a dialog requires an answer',
                parameters: Execute,
                success: CommandResult,
            }),
            (given, { pid }) => automation.execute({ ...given, pid }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { command, dictionaries };
