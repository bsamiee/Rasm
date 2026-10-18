import uFuzzy from '@leeoniya/ufuzzy';
import { Context, Data, Duration, Effect, Option, Schema, SchemaGetter, type SchemaIssue, Scope, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { Rpc, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

type Link = 'linked' | 'closed' | 'unreachable';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READING = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.Destructive, false), Context.add(Tool.OpenWorld, false));
const WRITING = Context.make(Tool.Readonly, false).pipe(Context.add(Tool.Idempotent, false), Context.add(Tool.Destructive, true), Context.add(Tool.OpenWorld, false));
const WIRE = RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true });
const REDIAL = Duration.seconds(2);

// --- [MODELS] --------------------------------------------------------------------------

const Phase = Schema.Literals(['Idle', 'Missed', 'Held', 'Entered']);
class Attachment extends Data.Class<{ readonly uri: string; readonly mimeType: string; readonly data: Uint8Array }> {
    static readonly schema = Schema.instanceOf(Attachment, {
        toCodecJson: () =>
            Schema.link<Attachment>()(Schema.String.annotate({ description: 'URI of the file the content block beside the result carries' }), {
                decode: SchemaGetter.forbidden(() => 'files travel as content blocks'),
                encode: SchemaGetter.transformEffect(
                    Effect.fnUntraced(function* ({ uri, mimeType, data }: Attachment) {
                        yield* (yield* Blocks)({ data, mimeType, uri: Option.liftPredicate(uri, () => !mimeType.startsWith('image/')) });
                        return uri;
                    }),
                ),
            }),
    });
}
const Activity = Schema.Struct({ description: Schema.String.annotate({ description: 'Present-progressive sentence shown while the call runs, naming its undo step' }) });
const unit = <P extends Schema.Top>(preview: P) => Activity.mapFields(Struct.assign({ preview: Schema.OptionFromOptionalKey(Schema.Union([Schema.Literal(true), preview]).annotate({ description: 'Runs the call, then undoes every change, returning its result or refusal, with an image of a given target' })) }));
const Image = (mimeType: string) =>
    Schema.Uint8Array.annotate({
        toCodecJson: () =>
            Schema.link<Uint8Array>()(Schema.String.annotate({ description: 'Media type of the image content block the result carries' }), {
                decode: SchemaGetter.forbidden(() => 'images travel as content blocks'),
                encode: SchemaGetter.transformEffect(
                    Effect.fnUntraced(function* (data: Uint8Array) {
                        yield* (yield* Blocks)({ data, mimeType, uri: Option.none() });
                        return mimeType;
                    }),
                ),
            }),
    });
const TypeScript = Schema.String.annotate({ description: 'JavaScript or TypeScript function body' }).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transformEffect((code: string) => Effect.flatMap(Transpile, (strip) => strip(code))), encode: SchemaGetter.passthrough() }));

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', { subject: Schema.String, query: Schema.String, candidates: Schema.Array(Schema.String) }) {
    static readonly ranked = (query: string, candidates: readonly string[]): readonly string[] => {
        const listed = 100;
        const [found, info, order] = new uFuzzy({ intraMode: 1 }).search([...candidates], query, 0, Number.POSITIVE_INFINITY);
        const matched = (order === null || info === null ? (found ?? []) : order.flatMap((position) => info.idx[position] ?? [])).flatMap((index) => candidates[index] ?? []);
        return (matched.length === 0 ? candidates : matched).slice(0, listed);
    };
    override get message(): string {
        return [`No ${this.subject} matching ${this.query}`, ...(this.candidates.length === 0 ? [] : [`candidates: ${Unmatched.ranked(this.query, this.candidates).join(', ')}`])].join(', ');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

const Surface = Context.Reference<{ readonly _tag: 'Tool' } | { readonly _tag: 'Resource'; readonly uri: string } | { readonly _tag: 'Prompt' } | { readonly _tag: 'None' }>('@rasm/mcp/link/Surface', { defaultValue: () => ({ _tag: 'Tool' }) });
const Instructions = Context.Reference<readonly string[]>('@rasm/mcp/link/Instructions', { defaultValue: () => [] });
const Transpile = Context.Reference<(code: string) => Effect.Effect<string, SchemaIssue.Issue>>('@rasm/mcp/link/Transpile', { defaultValue: () => Effect.succeed });
const Blocks = Context.Reference<(block: { readonly data: Uint8Array; readonly mimeType: string; readonly uri: Option.Option<string> }) => Effect.Effect<void>>('@rasm/mcp/link/Blocks', { defaultValue: () => () => Effect.void });

// --- [RPCS] ----------------------------------------------------------------------------

const Enter = Rpc.make('enter', { payload: {}, success: Phase }).annotate(Surface, { _tag: 'None' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const serve = Effect.fn('serve')(function* <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, { url, identity, concurrency, link }: { readonly url: string; readonly identity: UrlParams.Input; readonly concurrency: number | 'unbounded'; readonly link: (state: Link) => Effect.Effect<void> }) {
    const [{ reader, writer }, linked] = yield* Effect.all([Socket.makeWebSocket(`${url}?${UrlParams.toString(UrlParams.fromInput(identity))}`), Effect.flatMap(Effect.scope, Scope.fork)]);
    const protocol = yield* RpcServer.makeProtocolSocketServer.pipe(
        Effect.provideService(SocketServer.SocketServer, {
            address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
            run: (handler) =>
                handler(
                    Socket.make({
                        reader: reader.pipe(
                            Effect.tapError(() => link('unreachable')),
                            Effect.tap(() => Effect.acquireRelease(link('linked'), () => link('closed'))),
                        ),
                        writer,
                    }),
                ).pipe(Effect.ignoreCause, Effect.andThen(Effect.sleep(REDIAL)), Effect.forever),
        }),
        Effect.provide(WIRE),
        Scope.provide(linked),
    );
    yield* Effect.forkScoped(RpcServer.make(group, { concurrency, disableTracing: true }).pipe(Effect.provideService(RpcServer.Protocol, protocol)));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Activity, Attachment, Blocks, Enter, Image, Instructions, type Link, Phase, READING, REDIAL, Surface, serve, Transpile, TypeScript, Unmatched, unit, WIRE, WRITING };
