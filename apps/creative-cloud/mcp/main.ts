/**
 * Exposes Adobe automation through MCP with references from installed applications
 * Keeps accepted execution results available after request cancellation
 */

// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Cause, Context, Crypto, Deferred, Effect, type FileSystem, Logger, Option, type Path, Predicate, Schema } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { Applications, execute, Request } from './execution.ts';
import { Failure, invoke } from './native.ts';
import packageJson from './package.json' with { type: 'json' };

// --- [SCHEMAS] -------------------------------------------------------------------------

const Application = Schema.Struct({
    identifier: Schema.String,
    name: Schema.String,
    processes: Schema.Array(Schema.Int),
    scriptable: Schema.Boolean,
    url: Schema.String,
    version: Schema.optionalKey(Schema.String),
});

// --- [TOOLS] ---------------------------------------------------------------------------

const discovery = Toolkit.make(
    Tool.make('applications', {
        dependencies: [ChildProcessSpawner.ChildProcessSpawner],
        description: 'Discover installed and running Adobe applications. Read the installed scripting dictionary resource before executing commands.',
        failure: Failure,
        success: Schema.Struct({ applications: Schema.Array(Application), references: Schema.Array(McpSchema.ResourceLink) }),
    })
        .annotate(Tool.Readonly, true)
        .annotate(Tool.Idempotent, true),
);

// --- [SERVER] --------------------------------------------------------------------------

const program = Effect.gen(function* () {
    const scope = yield* Effect.scope;
    const server = yield* McpServer.McpServer;
    const crypto = yield* Crypto.Crypto;
    const services = yield* Effect.context<Applications | ChildProcessSpawner.ChildProcessSpawner | McpServer.McpServer | FileSystem.FileSystem | Path.Path>();

    yield* McpServer.registerResource`adobe://dictionary/${Schema.URL}`({
        content: (_uri, application) => invoke({ application: application.href, operation: 'dictionary' }, Schema.String),
        description: 'Commands and object model supplied by an installed Adobe application.',
        mimeType: 'application/xml',
        name: 'Scripting dictionary',
    });

    yield* McpServer.registerToolkit(discovery).pipe(
        Effect.provide(
            discovery.toLayer({
                applications: Effect.fn('adobe.applications')(function* () {
                    const applications = yield* invoke({ operation: 'applications' }, Schema.Array(Application));
                    const references = Array.map(Array.filter(applications, Predicate.Struct({ scriptable: Predicate.isTruthy })), (application) => {
                        const uri = `adobe://dictionary/${encodeURIComponent(application.url)}`;
                        return McpSchema.ResourceLink.make({ mimeType: 'application/xml', name: application.name, uri });
                    });
                    return { applications, references };
                }, Effect.tapError(Effect.logError)),
            }),
        ),
    );

    yield* server.addTool({
        annotations: Context.empty(),
        tool: new McpSchema.Tool({
            annotations: { destructiveHint: true, idempotentHint: false, openWorldHint: true, readOnlyHint: false },
            description:
                'Execute an installed Adobe dictionary command. Accepted work continues after request cancellation. Results remain in resources/list for this server lifetime. Unconfirmed native outcomes block further execution in that application until the server is restarted after checking the application.',
            inputSchema: Tool.getJsonSchemaFromSchema(Request),
            name: 'execute',
        }),
        handle: Effect.fn('adobe.request')(function* (payload: unknown) {
            const request = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(Request))(payload).pipe(
                Effect.mapError((error) => new McpSchema.InvalidParams({ message: Cause.pretty(Cause.fail(error)) })),
            );
            const id = yield* crypto.randomUUIDv4.pipe(Effect.mapError((error) => new McpSchema.InternalError({ message: Cause.pretty(Cause.fail(error)) })));
            const uri = `adobe://execution/${id}`;
            const result = yield* Deferred.make<McpSchema.CallToolResult>();
            const registration = McpServer.registerResource({
                content: Deferred.poll(result).pipe(
                    Effect.flatMap(
                        Option.match({
                            onNone: () => Effect.succeed(JSON.stringify({ state: 'pending' })),
                            onSome: Effect.map((value) => JSON.stringify({ result: Schema.encodeSync(Schema.toCodecJson(McpSchema.CallToolResult))(value), state: 'finished' })),
                        }),
                    ),
                ),
                description: `${request.application.href}: ${request.command}. Pending means native completion has not been confirmed.`,
                mimeType: 'application/json',
                name: id,
                uri,
            });
            const execution = Deferred.complete(
                result,
                execute(request, uri).pipe(
                    Effect.catchCause((cause) => {
                        const failure = Schema.encodeSync(Schema.toCodecJson(Schema.Cause(Schema.Defect(), Schema.Defect())))(cause);
                        return Effect.logError(cause).pipe(
                            Effect.as(
                                new McpSchema.CallToolResult({
                                    content: [McpSchema.TextContent.make({ text: JSON.stringify(failure) }), McpSchema.ResourceLink.make({ mimeType: 'application/json', name: id, uri })],
                                    isError: true,
                                    structuredContent: { execution: uri, failure },
                                }),
                            ),
                        );
                    }),
                ),
            ).pipe(Effect.andThen(server.notifications['notifications/resources/updated']({ uri })), Effect.uninterruptible, Effect.forkIn(scope));
            yield* registration.pipe(Effect.andThen(execution), Effect.uninterruptible);
            return yield* Deferred.await(result);
        }, Effect.provideContext(services)),
    });
    return yield* Effect.never;
});

// --- [ENTRY] ---------------------------------------------------------------------------

program.pipe(
    Effect.provide(Applications.layer),
    Effect.provide(McpServer.layerStdio({ name: packageJson.name, protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25], version: packageJson.version })),
    Effect.provide(NodeServices.layer),
    Effect.provideService(Logger.LogToStderr, true),
    Effect.scoped,
    NodeRuntime.runMain,
);
