import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cache, Context, Crypto, Deferred, Effect, FileSystem, Layer, Logger, Option, Path, type PlatformError, Result, Schema, type Scope, Stream, Struct, SynchronizedRef } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
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

const Request = Schema.Struct({
    application: Schema.URL.annotateKey({ description: 'File URL of a running application from the applications tool' }),
    arguments: Schema.JsonObject.annotate({
        description:
            "Dictionary parameters by name with direct for the direct parameter, in reply JSON form: numbers, booleans, text (enumerator or class name by parameter type, file URL as a file, quoted four-character code like 'docu' as a raw code), lists, records keyed by property name, and object specifiers {want: class, form: 'indx' | 'name' | 'ID  ' | 'prop', seld: index, ordinal like 'all ', name, id, or property, from: container specifier or null}",
    }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URL)
        .annotate({ description: 'Output file URLs to capture after the native reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    command: Schema.String.annotate({ description: 'Exact command name from the scripting dictionary resource' }),
});

const Execution = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Union([Schema.Struct({ state: Schema.Literal('pending') }), Schema.Struct({ result: McpSchema.CallToolResult, state: Schema.Literal('finished') })])),
);

// --- [ERRORS] --------------------------------------------------------------------------

const Failure = Schema.TaggedUnion({
    nativeError: { code: Schema.Int, domain: Schema.String, message: Schema.String, reply: Schema.optionalKey(Schema.JsonObject), stage: Schema.Literals(['request', 'send', 'reply']) },
    processError: { cause: Schema.Defect() },
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const invoke = Effect.fn('invoke')(
    function* <S extends Schema.Top>(
        request: Schema.JsonObject,
        response: S,
    ): Effect.fn.Return<S['Type'], typeof Failure.Type | PlatformError.PlatformError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Scope.Scope | S['DecodingServices']> {
        const child = yield* ChildProcess.make('AdobeScripting', { stdin: Stream.make(JSON.stringify(request)).pipe(Stream.encodeText), stderr: 'inherit' });
        const [output, status] = yield* Effect.all([child.stdout.pipe(Stream.decodeText, Stream.mkString), child.exitCode], { concurrency: 'unbounded' });
        return yield* status === 0
            ? Schema.decodeEffect(Schema.fromJsonString(response))(output)
            : Schema.decodeEffect(Schema.fromJsonString(Failure.cases.nativeError))(output).pipe(Effect.flatMap(Effect.fail));
    },
    Effect.scoped,
    Effect.catchTag(['PlatformError', 'SchemaError'], (cause) => Effect.fail(Failure.cases.processError.make({ cause }))),
);

const perform = Effect.fn('perform')(function* (request: typeof Request.Type, execution: McpSchema.ResourceLink) {
    const reply = yield* invoke({ execute: { application: request.application.href, arguments: request.arguments, command: request.command } }, Schema.Json);
    const [errors, artifacts] = yield* Effect.partition(request.artifacts, (url) => invoke({ file: { url: url.href } }, McpSchema.BlobResourceContents), { concurrency: 'unbounded' });
    const links = yield* Effect.forEach(artifacts, ({ blob, mimeType, uri: name }) => {
        const resource = { mimeType, name, uri: `${execution.uri}/artifact/${encodeURIComponent(name)}` };
        return McpServer.registerResource({ ...resource, content: Effect.succeed({ contents: [{ blob, mimeType, uri: resource.uri }] }) }).pipe(Effect.as(McpSchema.ResourceLink.make(resource)));
    });
    const images = artifacts.flatMap(({ blob, mimeType }) => (mimeType?.startsWith('image/') === true ? [McpSchema.ImageContent.make({ data: blob, mimeType })] : []));
    const output = { artifacts: links.map(Struct.get('uri')), errors: Schema.encodeSync(Schema.toCodecJson(Schema.Array(Failure)))(errors), execution: execution.uri, reply };
    return new McpSchema.CallToolResult({
        content: [McpSchema.TextContent.make({ text: JSON.stringify(output) }), ...links, ...images, execution],
        isError: errors.length > 0,
        structuredContent: output,
    });
});

const execute = Effect.fn('execute')(
    function* (blocks: Cache.Cache<string, SynchronizedRef.SynchronizedRef<Option.Option<typeof Failure.Type>>>, request: typeof Request.Type, execution: McpSchema.ResourceLink) {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const location = yield* path.fromFileUrl(request.application).pipe(Effect.flatMap(fs.realPath));
        const result = yield* SynchronizedRef.modifyEffect(
            yield* Cache.get(blocks, location),
            Option.match({
                onNone: () =>
                    perform(request, execution).pipe(
                        Effect.result,
                        Effect.map((value) => [value, Option.filter(Result.getFailure(value), (failure) => failure._tag !== 'nativeError' || failure.stage === 'send')] as const),
                    ),
                onSome: (failure) => Effect.succeed([Result.fail(failure), Option.some(failure)] as const),
            }),
        );
        return yield* Effect.fromResult(result);
    },
    Effect.catchTag(['BadArgument', 'PlatformError'], (cause) => Effect.fail(Failure.cases.processError.make({ cause }))),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

Layer.effectDiscard(
    Effect.gen(function* () {
        const scope = yield* Effect.scope;
        const server = yield* McpServer.McpServer;
        const crypto = yield* Crypto.Crypto;
        const blocks = yield* Cache.make({ capacity: Number.POSITIVE_INFINITY, lookup: (_: string) => SynchronizedRef.make(Option.none<typeof Failure.Type>()) });
        const services = yield* Effect.context<ChildProcessSpawner.ChildProcessSpawner | McpServer.McpServer | FileSystem.FileSystem | Path.Path>();
        const discovery = Toolkit.make(
            Tool.make('applications', {
                dependencies: [ChildProcessSpawner.ChildProcessSpawner],
                description: 'Lists installed and running Adobe applications with a scripting dictionary resource link for each scriptable one',
                failure: Failure,
                success: Schema.Struct({ applications: Schema.Array(Application), references: Schema.Array(McpSchema.ResourceLink) }),
            }).annotate(Tool.Readonly, true),
        );

        yield* McpServer.registerResource`adobe://dictionary/${Schema.URL}`({
            content: (_uri, application) => invoke({ dictionary: { application: application.href } }, Schema.String),
            description: 'Commands and object model of an installed Adobe application',
            mimeType: 'application/xml',
            name: 'Scripting dictionary',
        });

        yield* McpServer.registerToolkit(discovery).pipe(
            Effect.provide(
                discovery.toLayer({
                    applications: Effect.fn('applications')(function* () {
                        const applications = yield* invoke({ applications: {} }, Schema.Array(Application));
                        const references = applications
                            .filter(Struct.get('scriptable'))
                            .map((application) =>
                                McpSchema.ResourceLink.make({ mimeType: 'application/xml', name: application.name, uri: `adobe://dictionary/${encodeURIComponent(application.url)}` }),
                            );
                        return { applications, references };
                    }, Effect.tapCause(Effect.logError)),
                }),
            ),
        );

        yield* server.addTool({
            annotations: Context.empty(),
            tool: new McpSchema.Tool({
                description:
                    'Runs a scripting dictionary command in an installed Adobe application. Accepted commands continue after request cancellation. Results stay in resources/list while the server runs. Unconfirmed native outcomes block further commands to that application until a person checks the application and restarts the server',
                inputSchema: Tool.getJsonSchemaFromSchema(Request),
                name: 'execute',
            }),
            handle: Effect.fn('handle')(function* (payload: unknown) {
                const request = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(Request))(payload).pipe(Effect.mapError((error) => new McpSchema.InvalidParams({ message: error.message })));
                const id = yield* crypto.randomUUIDv4.pipe(Effect.orDie);
                const resource = { mimeType: 'application/json', name: id, uri: `adobe://execution/${id}` };
                const link = McpSchema.ResourceLink.make(resource);
                const result = yield* Deferred.make<McpSchema.CallToolResult>();
                const registration = McpServer.registerResource({
                    ...resource,
                    content: Deferred.poll(result).pipe(
                        Effect.flatMap(
                            Option.match({
                                onNone: () => Effect.succeed<typeof Execution.Type>({ state: 'pending' }),
                                onSome: Effect.map((value) => ({ result: value, state: 'finished' }) as const),
                            }),
                        ),
                        Effect.map(Schema.encodeSync(Execution)),
                    ),
                    description: `${request.application.href}: ${request.command}, pending while native completion is unconfirmed`,
                });
                const execution = Deferred.complete(
                    result,
                    execute(blocks, request, link).pipe(
                        Effect.catchCause((cause) => {
                            const output = { execution: link.uri, failure: Schema.encodeSync(Schema.toCodecJson(Schema.Cause(Failure, Schema.Defect())))(cause) };
                            return Effect.logError(cause).pipe(
                                Effect.as(new McpSchema.CallToolResult({ content: [McpSchema.TextContent.make({ text: JSON.stringify(output) }), link], isError: true, structuredContent: output })),
                            );
                        }),
                    ),
                ).pipe(Effect.andThen(server.notifications['notifications/resources/updated']({ uri: link.uri })), Effect.forkIn(scope, { uninterruptible: true }));
                yield* registration.pipe(Effect.andThen(execution), Effect.uninterruptible);
                return yield* Deferred.await(result);
            }, Effect.provideContext(services)),
        });
    }),
).pipe(
    Layer.provide(McpServer.layerStdio({ name: packageJson.name, protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25], version: packageJson.version })),
    Layer.provide(NodeServices.layer),
    Layer.launch,
    Effect.provideService(Logger.LogToStderr, true),
    NodeRuntime.runMain,
);
