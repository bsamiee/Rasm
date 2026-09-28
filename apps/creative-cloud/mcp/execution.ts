import { Context, Duration, Effect, FileSystem, Layer, Option, Path, type PlatformError, RcMap, Result, Schema, type Scope, Stream, SynchronizedRef } from 'effect';
import { McpSchema, McpServer } from 'effect/unstable/ai';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';

// --- [MODELS] --------------------------------------------------------------------------

const Request = Schema.Struct({
    application: Schema.URL.annotateKey({ description: 'Application file URL returned by applications. The application must be running.' }),
    arguments: Schema.Record(Schema.String, Schema.Json).annotate({
        description:
            "Parameters named by the installed dictionary, with direct naming its direct parameter. Values take the JSON form replies use: numbers, booleans, text (enumerator and class names resolve by the parameter type, file URLs become files, a quoted four-character code like 'docu' names a raw code), lists, records keyed by property name, and object specifiers {want: class, form: 'indx' | 'name' | 'ID  ' | 'prop', seld: index, ordinal like 'all ', name, id, or property, from: container specifier or null}.",
    }),
    artifacts: Schema.Array(Schema.URL).annotate({ description: 'File URLs of outputs to capture after the native reply. Use an empty array when the command writes no files.' }),
    command: Schema.String.annotate({ description: 'Exact command name from the application scripting dictionary resource.' }),
});

// --- [ERRORS] --------------------------------------------------------------------------

const Failure = Schema.TaggedUnion({
    nativeError: { code: Schema.Int, domain: Schema.String, message: Schema.String, reply: Schema.optionalKey(Schema.JsonObject), stage: Schema.Literals(['request', 'send', 'reply']) },
    processError: { cause: Schema.Defect() },
    protocolError: { cause: Schema.Defect() },
});

// --- [SERVICES] ------------------------------------------------------------------------

class Applications extends Context.Service<Applications, RcMap.RcMap<string, SynchronizedRef.SynchronizedRef<Option.Option<typeof Failure.Type>>>>()('adobe/Applications') {
    static readonly layer = Layer.effect(
        Applications,
        RcMap.make({
            idleTimeToLive: Duration.infinity,
            lookup: () => SynchronizedRef.make(Option.none<typeof Failure.Type>()),
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const invoke = Effect.fn('adobe.native')(
    function* <S extends Schema.Top>(
        request: Schema.JsonObject,
        response: S,
    ): Effect.fn.Return<S['Type'], typeof Failure.Type | PlatformError.PlatformError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Scope.Scope | S['DecodingServices']> {
        const child = yield* ChildProcess.make('AdobeScripting', {
            stdin: Stream.make(JSON.stringify(request)).pipe(Stream.encodeText),
            stderr: 'inherit',
        });
        const [output, status] = yield* Effect.all([child.stdout.pipe(Stream.decodeText, Stream.mkString), child.exitCode], { concurrency: 'unbounded' });
        return yield* status === 0
            ? Schema.decodeUnknownEffect(Schema.fromJsonString(response))(output)
            : Schema.decodeUnknownEffect(Schema.fromJsonString(Failure.cases.nativeError))(output).pipe(Effect.flatMap(Effect.fail));
    },
    Effect.scoped,
    Effect.catchTag('PlatformError', (cause) => Effect.fail(Failure.cases.processError.make({ cause }))),
    Effect.catchTag('SchemaError', (cause) => Effect.fail(Failure.cases.protocolError.make({ cause }))),
);

const perform = Effect.fn('adobe.perform')(function* (request: typeof Request.Type, execution: McpSchema.ResourceLink) {
    const reply = yield* invoke({ execute: { application: request.application.href, arguments: request.arguments, command: request.command } }, Schema.Json);
    const [errors, artifacts] = yield* Effect.partition(request.artifacts, (url) => invoke({ file: { url: url.href } }, McpSchema.BlobResourceContents), { concurrency: 'unbounded' });
    const content = yield* Effect.forEach(
        artifacts,
        Effect.fn(function* (artifact) {
            const resource = McpSchema.BlobResourceContents.make({ ...artifact, uri: `${execution.uri}/artifact/${encodeURIComponent(artifact.uri)}` });
            yield* McpServer.registerResource({
                content: Effect.succeed(McpSchema.ReadResourceResult.make({ contents: [resource] })),
                mimeType: artifact.mimeType,
                name: artifact.uri,
                uri: resource.uri,
            });
            return [
                McpSchema.ResourceLink.make({ mimeType: artifact.mimeType, name: artifact.uri, uri: resource.uri }),
                ...(artifact.mimeType?.startsWith('image/') === true ? [McpSchema.ImageContent.make({ data: artifact.blob, mimeType: artifact.mimeType })] : []),
            ] as const;
        }),
    );
    const output = {
        artifacts: content.map(([link]) => link.uri),
        errors: Schema.encodeSync(Schema.toCodecJson(Schema.Array(Failure)))(errors),
        execution: execution.uri,
        reply,
    };
    return new McpSchema.CallToolResult({
        content: [McpSchema.TextContent.make({ text: JSON.stringify(output) }), ...content.flat(), execution],
        isError: errors.length > 0,
        structuredContent: output,
    });
});

const execute = Effect.fn('adobe.execute')(function* (request: typeof Request.Type, execution: McpSchema.ResourceLink) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const applications = yield* Applications;
    const location = yield* path.fromFileUrl(request.application).pipe(Effect.flatMap(fs.realPath));
    const application = yield* path.toFileUrl(location);
    const block = yield* RcMap.get(applications, location);
    const result = yield* SynchronizedRef.modifyEffect(
        block,
        Option.match({
            onNone: () =>
                perform({ ...request, application }, execution).pipe(
                    Effect.result,
                    Effect.map((value) => [value, Option.filter(Result.getFailure(value), (failure) => failure._tag !== 'nativeError' || failure.stage === 'send')] as const),
                ),
            onSome: (failure) => Effect.succeed([Result.fail(failure), Option.some(failure)] as const),
        }),
    );
    return yield* Effect.fromResult(result);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Applications, execute, Failure, invoke, Request };
