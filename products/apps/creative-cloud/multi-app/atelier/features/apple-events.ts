import { Execute, TermKind, Terms } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Content, Resource, Route } from '@rasm/mcp';
import { Array, Effect, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import type { routes } from '#/routes';

// --- [MODELS] --------------------------------------------------------------------------

const Application = McpSchema.param('application', Schema.String);
const CommandResult = Execute.Result.pipe(Schema.fieldsAssign({ artifacts: Schema.Array(Schema.Result(Content.embedded(Execute.Artifact), Schema.ErrorInstance())).annotate({ description: SchemaAST.resolveDescription(Execute.Result.fields.artifacts.ast) }) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const appleEvents = Effect.fnUntraced(function* ({ dictionary }: Effect.Success<ReturnType<typeof routes>>) {
    const server = yield* ApplicationServer;
    const terms = (given: string) => Effect.flatMap(dictionary.named(given), ({ url }) => server.terms({ url }));
    const changes = SubscriptionRef.changes(server.installations);
    yield* Resource.template`dictionary/${Application}`({ name: 'dictionary', schema: Terms, completion: dictionary.completion, changes, content: (_, given) => terms(given) });
    yield* Resource.template`dictionary/${Application}/${McpSchema.param('kind', TermKind)}/${McpSchema.param('name', Schema.String)}`({
        name: 'dictionary_term',
        description: 'Scripting definition XML matching kind and name',
        mimeType: 'application/xml',
        schema: Schema.String,
        changes,
        completion: {
            ...dictionary.completion,
            kind: () => Effect.succeed([...TermKind.literals]),
            name: (_, { application, kind }) => (application === undefined || kind === undefined ? Effect.succeed([]) : Effect.map(terms(application), (found) => Array.map(found[kind], Struct.get('name')))),
        },
        content: (_, given, kind, name) => Effect.flatMap(dictionary.named(given), ({ url }) => server.definition({ url, kind, name })),
    });
    yield* Route(dictionary)(Tool.make('command', { description: 'Sends scripting dictionary command, refused while a dialog requires an answer', parameters: Execute, success: CommandResult }), (given, { instance }) => server.execute({ ...given, instance }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { appleEvents };
