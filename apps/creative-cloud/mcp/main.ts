import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Context, Crypto, Deferred, Effect, type FileSystem, Logger, Option, type Path, Schema, Struct } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcessSpawner } from 'effect/unstable/process';
import { Applications, execute, Failure, invoke, Request } from './execution.ts';
import packageJson from './package.json' with { type: 'json' };

// --- [MODELS] --------------------------------------------------------------------------

const Application = Schema.Struct({
    identifier: Schema.String,
    name: Schema.String,
    processIdentifiers: Schema.Array(Schema.Int),
    scriptable: Schema.Boolean,
    url: Schema.String,
    version: Schema.optionalKey(Schema.String),
});

const discovery = Toolkit.make(
    Tool.make('applications', {
        dependencies: [ChildProcessSpawner.ChildProcessSpawner],
        description: 'Discover installed and running Adobe applications. Read the installed scripting dictionary resource before executing commands.',
        failure: Failure,
        success: Schema.Struct({ applications: Schema.Array(Application), references: Schema.Array(McpSchema.ResourceLink) }),
    })
        .annotate(Tool.Readonly, true)
        .annotate(Tool.Destructive, false)
        .annotate(Tool.Idempotent, true),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const program = Effect.gen(function* () {
    const scope = yield* Effect.scope;
    const server = yield* McpServer.McpServer;
    const crypto = yield* Crypto.Crypto;
    const services = yield* Effect.context<Applications | ChildProcessSpawner.ChildProcessSpawner | McpServer.McpServer | FileSystem.FileSystem | Path.Path>();

    yield* McpServer.registerResource`adobe://dictionary/${Schema.URL}`({
        content: (_uri, application) => invoke({ dictionary: { application: application.href } }, Schema.String),
        description: 'Commands and object model supplied by an installed Adobe application.',
        mimeType: 'application/xml',
        name: 'Scripting dictionary',
    });

    yield* McpServer.registerToolkit(discovery).pipe(
        Effect.provide(
            discovery.toLayer({
                applications: Effect.fn('adobe.applications')(function* () {
                    const applications = yield* invoke({ applications: {} }, Schema.Array(Application));
                    const references = applications
                        .filter(Struct.get('scriptable'))
                        .map((application) => McpSchema.ResourceLink.make({ mimeType: 'application/xml', name: application.name, uri: `adobe://dictionary/${encodeURIComponent(application.url)}` }));
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
                'Execute an installed Adobe dictionary command. Accepted work continues after request cancellation. Results stay in resources/list while the server runs. Unconfirmed native outcomes block further execution in that application until a person checks the application and restarts the server.',
            inputSchema: Tool.getJsonSchemaFromSchema(Request),
            name: 'execute',
        }),
        handle: Effect.fn('adobe.request')(function* (payload: unknown) {
            const request = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(Request))(payload).pipe(
                Effect.mapError((error) => new McpSchema.InvalidParams({ message: Cause.pretty(Cause.fail(error)) })),
            );
            const id = yield* crypto.randomUUIDv4.pipe(Effect.mapError((error) => new McpSchema.InternalError({ message: Cause.pretty(Cause.fail(error)) })));
            const link = McpSchema.ResourceLink.make({ mimeType: 'application/json', name: id, uri: `adobe://execution/${id}` });
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
                description: `${request.application.href}: ${request.command}. Pending state means native completion is unconfirmed.`,
                mimeType: link.mimeType,
                name: link.name,
                uri: link.uri,
            });
            const execution = Deferred.complete(
                result,
                execute(request, link).pipe(
                    Effect.catchCause((cause) => {
                        const failure = Schema.encodeSync(Schema.toCodecJson(Schema.Cause(Schema.Defect(), Schema.Defect())))(cause);
                        return Effect.logError(cause).pipe(
                            Effect.as(
                                new McpSchema.CallToolResult({
                                    content: [McpSchema.TextContent.make({ text: JSON.stringify(failure) }), link],
                                    isError: true,
                                    structuredContent: { execution: link.uri, failure },
                                }),
                            ),
                        );
                    }),
                ),
            ).pipe(Effect.andThen(server.notifications['notifications/resources/updated']({ uri: link.uri })), Effect.uninterruptible, Effect.forkIn(scope));
            yield* registration.pipe(Effect.andThen(execution), Effect.uninterruptible);
            return yield* Deferred.await(result);
        }, Effect.provideContext(services)),
    });
    return yield* Effect.never;
});

program.pipe(
    Effect.provide(Applications.layer),
    Effect.provide(McpServer.layerStdio({ name: packageJson.name, protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25], version: packageJson.version })),
    Effect.provide(NodeServices.layer),
    Effect.provideService(Logger.LogToStderr, true),
    Effect.scoped,
    NodeRuntime.runMain,
);
