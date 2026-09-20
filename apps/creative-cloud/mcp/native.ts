/**
 * Connects Effect execution to macOS automation through a scoped process
 * Preserves native failures separately from process and protocol failures
 */

// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, type PlatformError, Schema, type Scope, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';

// --- [ERRORS] --------------------------------------------------------------------------

const Failure = Schema.TaggedUnion({
    nativeError: { code: Schema.Int, domain: Schema.String, message: Schema.String, reply: Schema.optionalKey(Schema.JsonObject), stage: Schema.Literals(['request', 'send', 'reply']) },
    processError: { cause: Schema.Defect() },
    protocolError: { cause: Schema.Defect() },
});

// --- [NATIVE] --------------------------------------------------------------------------

const invoke = Effect.fn('adobe.native')(
    function* <S extends Schema.Top>(
        request: Schema.JsonObject,
        response: S,
    ): Effect.fn.Return<S['Type'], typeof Failure.Type | PlatformError.PlatformError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner | Scope.Scope | S['DecodingServices']> {
        const child = yield* ChildProcess.make('AdobeAutomation', {
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Failure, invoke };
