import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Context, Crypto, Deferred, Effect, Layer, Logger, Option, type PlatformError, Schema, type Scope, Stream, Struct } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
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
            "Dictionary parameters by name with direct for the direct parameter, in reply JSON form: numbers, booleans, text (an enumerator, class name, or file URL where the parameter's declared type takes one, a quoted four-character code like 'docu' as a raw code for a term that names several codes), lists, records keyed by property name, and object specifiers {want: class, form: 'indx' | 'name' | 'ID  ' | 'prop', seld: index, ordinal like 'all ', name, id, or property, from: container specifier or null}",
    }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URL)
        .annotate({ description: 'Output file URLs to capture after the native reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    command: Schema.String.annotate({ description: 'Exact command name from the scripting dictionary resource' }),
});

const Execution = Schema.fromJsonString(Schema.toCodecJson(Schema.Union([Schema.Struct({ state: Schema.Literal('pending') }), Schema.Struct({ result: McpSchema.CallToolResult, state: Schema.Literal('finished') })])));

// --- [ERRORS] --------------------------------------------------------------------------

const Failures = Schema.NonEmptyArray(Schema.JsonObject);

// --- [OPERATIONS] ----------------------------------------------------------------------

const invoke = Effect.fn('invoke')(
    function* <S extends Schema.Top>(request: Schema.JsonObject, response: S): Effect.fn.Return<S['Type'], typeof Failures.Type | PlatformError.PlatformError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Scope.Scope | S['DecodingServices']> {
        const child = yield* ChildProcess.make('AdobeScripting', { stdin: Stream.make(JSON.stringify(request)).pipe(Stream.encodeText), stderr: 'inherit' });
        const [output, status] = yield* Effect.all([child.stdout.pipe(Stream.decodeText, Stream.mkString), child.exitCode], { concurrency: 'unbounded' });
        return yield* status === 0 ? Schema.decodeEffect(Schema.fromJsonString(response))(output) : Schema.decodeEffect(Schema.fromJsonString(Failures))(output).pipe(Effect.flatMap(Effect.fail));
    },
    Effect.scoped,
    Effect.catchTag(['PlatformError', 'SchemaError'], (cause) => Effect.fail<typeof Failures.Type>([{ processError: Schema.encodeSync(Schema.toCodecJson(Schema.Defect()))(cause) }])),
);

const perform = Effect.fn('perform')(
    function* (request: typeof Request.Type, execution: McpSchema.ResourceLink) {
        const reply = yield* invoke({ execute: { application: request.application.href, arguments: request.arguments, command: request.command } }, Schema.Json);
        const [artifacts, failures] = yield* Effect.partition(request.artifacts, (url) => invoke({ file: { url: url.href } }, McpSchema.BlobResourceContents), { concurrency: 'unbounded' });
        const links = yield* Effect.forEach(artifacts, ({ blob, mimeType, uri: name }) => {
            const resource = { mimeType, name, uri: `${execution.uri}/artifact/${encodeURIComponent(name)}` };
            return McpServer.registerResource({ ...resource, content: Effect.succeed({ contents: [{ blob, mimeType, uri: resource.uri }] }) }).pipe(Effect.as(McpSchema.ResourceLink.make(resource)));
        });
        const images = artifacts.flatMap(({ blob, mimeType }) => (mimeType?.startsWith('image/') === true ? [McpSchema.ImageContent.make({ data: blob, mimeType })] : []));
        return { content: [...links, ...images], output: { artifacts: links.map(Struct.get('uri')), failures: failures.flat(), reply } };
    },
    (effect, _request, execution) =>
        effect.pipe(
            Effect.tapCause(Effect.logError),
            Effect.orElseSucceed((failures) => ({ content: [], output: { failures } })),
            Effect.map(({ content, output }) => {
                const structured = { ...output, execution: execution.uri };
                return new McpSchema.CallToolResult({
                    content: [McpSchema.TextContent.make({ text: JSON.stringify(structured) }), ...content, execution],
                    isError: output.failures.length > 0,
                    structuredContent: structured,
                });
            }),
        ),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

Layer.effectDiscard(
    Effect.gen(function* () {
        const scope = yield* Effect.scope;
        const server = yield* McpServer.McpServer;
        const crypto = yield* Crypto.Crypto;
        const services = yield* Effect.context<ChildProcessSpawner.ChildProcessSpawner | McpServer.McpServer>();
        const discovery = Toolkit.make(
            Tool.make('applications', {
                dependencies: [ChildProcessSpawner.ChildProcessSpawner],
                description: 'Lists installed and running Adobe applications with a scripting dictionary resource link for each scriptable one',
                failure: Failures,
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
                        const references = applications.filter(Struct.get('scriptable')).map((application) => McpSchema.ResourceLink.make({ mimeType: 'application/xml', name: application.name, uri: `adobe://dictionary/${encodeURIComponent(application.url)}` }));
                        return { applications, references };
                    }, Effect.tapCause(Effect.logError)),
                }),
            ),
        );

        yield* server.addTool({
            annotations: Context.empty(),
            tool: new McpSchema.Tool({
                description: 'Runs a scripting dictionary command in an installed Adobe application. Accepted commands continue after request cancellation. Results stay in resources/list while the server runs. A send the application never answers leaves its outcome in the application unknown',
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
                const execution = Deferred.complete(result, perform(request, link)).pipe(Effect.andThen(server.notifications['notifications/resources/updated']({ uri: link.uri })), Effect.forkIn(scope, { uninterruptible: true }));
                yield* registration.pipe(Effect.andThen(execution), Effect.uninterruptible);
                return yield* Deferred.await(result);
            }, Effect.provideContext(services)),
        });
    }),
).pipe(Layer.provide(McpServer.layerStdio({ name: packageJson.name, protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25], version: packageJson.version })), Layer.provide(NodeServices.layer), Layer.launch, Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain);
