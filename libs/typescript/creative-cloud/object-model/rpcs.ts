import { Unmatched } from '@rasm/search';
import { Array, Cause, Option, Schema, SchemaTransformation, String, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { HostError, Reference, Specifier } from '#/model';

// --- [MODELS] --------------------------------------------------------------------------

class Evaluation extends Schema.Class<Evaluation, { readonly brand: unique symbol }>('Evaluation')({
    code: Schema.String.annotate({ description: 'Function expression the host evaluates and calls, awaiting what it returns' }),
    label: Schema.String.annotate({ description: 'Name of the undo step the evaluation runs in' }),
}) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
    static readonly expression = (body: string, awaits: boolean) => `(${awaits ? 'async ' : ''}function body() {\n${body}\n})`;
    static readonly formatLabel = (body: string) => `Run ${Array.findFirst(body.split('\n'), (line) => Option.liftPredicate(line.trim(), String.isNonEmpty)).pipe(Option.getOrElse(() => 'script'))}`;
}
class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')({
    target: Specifier.FromString.annotate({ description: 'Document, page, spread, artboard, layer, composition, or sequence specifier' }),
}) {
    static readonly Result = Schema.Struct({ image: Schema.Uint8Array, width: Schema.Int, height: Schema.Int });

    get label(): string {
        return `Render ${this.target.leaf}`;
    }
}
const WriteResult = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String).annotate({ description: 'Member names' }) }));

// --- [ERRORS] --------------------------------------------------------------------------

const IllegalArgument = Schema.declare(Cause.isIllegalArgumentError, {
    expected: 'IllegalArgumentError',
    toCodecJson: () => Schema.link<Cause.IllegalArgumentError>()(Schema.TaggedStruct('IllegalArgumentError', { message: Schema.String }), SchemaTransformation.transform({ decode: ({ message }) => new Cause.IllegalArgumentError(message), encode: ({ message }) => ({ _tag: 'IllegalArgumentError' as const, message }) })),
});
const Failure = Schema.Union([Unmatched, HostError, IllegalArgument, Schema.ErrorInstance()]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const makeRpc = <const Tag extends string, Payload extends Schema.Top & { readonly Result: Schema.Top }>(tag: Tag, payload: Payload) => Rpc.make<Tag, Payload, Payload['Result'], typeof Failure>(tag, { payload, success: payload.Result, error: Failure });

// --- [COMPOSITION] ---------------------------------------------------------------------

const EvaluationRpcs = RpcGroup.make(makeRpc('evaluate', Evaluation));
const RenderRpcs = RpcGroup.make(makeRpc('render', Render));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Evaluation, EvaluationRpcs, Failure, makeRpc, Render, RenderRpcs, WriteResult };
