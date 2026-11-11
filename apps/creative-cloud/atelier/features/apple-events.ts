import { Execute, TermKind, Terms } from '@rasm/applications';
import { Automation } from '@rasm/applications/node';
import { Content, Resource, Route } from '@rasm/mcp';
import { Array, Effect, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { Adobe } from '#/adobe';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const Application = McpSchema.param('application', Schema.String);
const CommandResult = Execute.Result.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Content.attachment(Execute.Artifact), Schema.ErrorInstance())).annotate({ description: SchemaAST.resolveDescription(Execute.Result.fields.artifacts.ast) }) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const appleEvents = Effect.fnUntraced(function* ({ dictionary }: Effect.Success<ReturnType<typeof routes>>) {
    const [automation, { installations }] = yield* Effect.all([Automation, Adobe]);
    yield* Resource.template`dictionary/${Application}`({
        name: 'dictionary',
        schema: Terms,
        completion: dictionary.completion,
        changes: SubscriptionRef.changes(installations),
        content: (_, given) => Effect.flatMap(dictionary.named(given), automation.terms),
    });
    yield* Resource.template`dictionary/${Application}/${McpSchema.param('kind', TermKind)}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML matching kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes: SubscriptionRef.changes(installations),
        completion: {
            ...dictionary.completion,
            kind: () => Effect.succeed([...TermKind.literals]),
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
    yield* Route(dictionary)(
        Tool.make('command', {
            description: 'Sends scripting dictionary command, refused while a dialog requires an answer',
            parameters: Execute,
            success: CommandResult,
        }),
        (given, { instance }) => automation.execute({ ...given, instance }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { appleEvents };
