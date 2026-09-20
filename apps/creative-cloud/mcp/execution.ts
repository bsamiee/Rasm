/**
 * Serializes work per application and preserves native replies with output artifacts
 * Holds application ownership when native completion cannot be confirmed
 */

// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Context, Duration, Effect, FileSystem, Layer, Option, Path, RcMap, Result, Schema, SynchronizedRef } from 'effect';
import { McpSchema, McpServer } from 'effect/unstable/ai';
import { Failure, invoke } from './native.ts';

// --- [SCHEMA] --------------------------------------------------------------------------

const Request = Schema.Struct({
    application: Schema.URL.annotateKey({ description: 'Application file URL returned by applications. The application must already be running.' }),
    arguments: Schema.Record(Schema.String, Schema.Json).annotate({
        description: 'Parameters named by the installed dictionary; direct names its direct parameter. Values are scalars, lists, or native {type, data} descriptors.',
    }),
    artifacts: Schema.Array(Schema.URL).annotate({ description: 'File URLs of outputs to capture after the native reply. Use an empty array when no files are produced.' }),
    command: Schema.String.annotate({ description: 'Exact command name from the application scripting dictionary resource.' }),
});

// --- [OWNERSHIP] -----------------------------------------------------------------------

class Applications extends Context.Service<Applications, RcMap.RcMap<string, SynchronizedRef.SynchronizedRef<Option.Option<typeof Failure.Type>>>>()('adobe/Applications') {
    static readonly layer = Layer.effect(
        Applications,
        RcMap.make({
            idleTimeToLive: Duration.infinity,
            lookup: (_application: string) => SynchronizedRef.make(Option.none<typeof Failure.Type>()),
        }),
    );
}

// --- [EXECUTION] -----------------------------------------------------------------------

const perform = Effect.fn('adobe.perform')(function* (request: typeof Request.Type, uri: string) {
    const reply = yield* invoke({ application: request.application.href, arguments: request.arguments, command: request.command, operation: 'execute' }, Schema.JsonObject);
    const [errors, artifacts] = yield* Effect.partition(request.artifacts, (url) => invoke({ operation: 'artifact', url: url.href }, McpSchema.BlobResourceContents), { concurrency: 'unbounded' });
    const content = yield* Effect.forEach(
        artifacts,
        Effect.fn(function* (artifact) {
            const address = `${uri}/artifact/${encodeURIComponent(artifact.uri)}`;
            const resource = McpSchema.BlobResourceContents.make({ ...artifact, uri: address });
            yield* McpServer.registerResource({
                content: Effect.succeed(McpSchema.ReadResourceResult.make({ contents: [resource] })),
                mimeType: artifact.mimeType,
                name: artifact.uri,
                uri: address,
            });
            return [
                McpSchema.ResourceLink.make({ mimeType: artifact.mimeType, name: artifact.uri, uri: address }),
                ...(artifact.mimeType?.startsWith('image/') === true ? [McpSchema.ImageContent.make({ data: artifact.blob, mimeType: artifact.mimeType })] : []),
            ];
        }),
    );
    const output = {
        artifacts: Array.map(artifacts, (artifact) => `${uri}/artifact/${encodeURIComponent(artifact.uri)}`),
        errors: Schema.encodeSync(Schema.toCodecJson(Schema.Array(Failure)))(errors),
        execution: uri,
        reply,
    };
    return new McpSchema.CallToolResult({
        content: [McpSchema.TextContent.make({ text: JSON.stringify(output) }), ...Array.flatten(content), McpSchema.ResourceLink.make({ mimeType: 'application/json', name: uri, uri })],
        isError: Array.isReadonlyArrayNonEmpty(errors),
        structuredContent: output,
    });
});

const execute = Effect.fn('adobe.execute')(function* (request: typeof Request.Type, uri: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const applications = yield* Applications;
    const location = yield* path.fromFileUrl(request.application).pipe(Effect.flatMap(fs.realPath));
    const application = yield* path.toFileUrl(location);
    const connection = yield* RcMap.get(applications, location);
    const result = yield* SynchronizedRef.modifyEffect(connection, (blocked) =>
        Option.match(blocked, {
            onNone: () =>
                perform({ ...request, application }, uri).pipe(
                    Effect.result,
                    Effect.map(
                        (value) => [value, Result.isFailure(value) && (value.failure._tag !== 'nativeError' || value.failure.stage === 'send') ? Option.some(value.failure) : Option.none()] as const,
                    ),
                ),
            onSome: (failure) => Effect.succeed([Result.fail(failure), blocked] as const),
        }),
    );
    return yield* Effect.fromResult(result);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Applications, execute, Request };
