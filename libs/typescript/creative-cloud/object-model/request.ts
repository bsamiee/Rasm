import { Unmatched } from '@rasm/search';
import { Array, Cause, Option, Schema, SchemaTransformation, String, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Declaration, HostError, Reference } from '#/model';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BODY = 'function body() {';

// --- [MODELS] --------------------------------------------------------------------------

class Evaluation extends Schema.Class<Evaluation, { readonly brand: unique symbol }>('Evaluation')({ code: Schema.String.annotate({ description: 'Function expression the host evaluates and calls, awaiting what it returns' }) }) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
    static readonly expression = (body: string, awaits: boolean) => (awaits ? `(async ${BODY}\n${body}\n})` : `(${BODY}\n${body}\n})`);

    get label(): string {
        const [, ...body] = this.code.split(BODY);
        return `Run ${Option.getOrElse(Array.findFirst(Array.map(body.join(BODY).split('\n'), String.trim), String.isNonEmpty), () => 'script')}`;
    }
}
class Open extends Schema.Class<Open, { readonly brand: unique symbol }>('Open')({ file: Schema.String.check(Schema.isStartingWith('/')).annotate({ description: 'Document path' }) }) {
    static readonly Result = Reference;

    get label(): string {
        return `Open ${this.file.slice(this.file.lastIndexOf('/') + 1)}`;
    }
}
class Rasterize extends Schema.Class<Rasterize, { readonly brand: unique symbol }>('Rasterize')({
    target: Schema.String.annotate({ description: 'Document, page, spread, artboard, layer, composition, or page item specifier' }),
    longestSide: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Limit in pixels' }),
    options: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Json).annotate({ description: 'Option fields of the host exporter the target selects' })),
}) {
    get label(): string {
        return `Rasterize ${this.target}`;
    }
}
class ObjectModel extends Schema.Class<ObjectModel, { readonly brand: unique symbol }>('ObjectModel')({ name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name in any case, absent lists every name' })) }) {
    static readonly Result = Declaration;

    get label(): string {
        return Option.match(this.name, { onNone: () => 'List classes and enumerations', onSome: (name) => `Describe ${name}` });
    }
}
const Written = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String).annotate({ description: 'Member names' }) }));

// --- [ERRORS] --------------------------------------------------------------------------

const IllegalArgument = Schema.declare(Cause.isIllegalArgumentError, {
    expected: 'IllegalArgumentError',
    toCodecJson: () => Schema.link<Cause.IllegalArgumentError>()(Schema.TaggedStruct('IllegalArgumentError', { message: Schema.String }), SchemaTransformation.transform({ decode: ({ message }) => new Cause.IllegalArgumentError(message), encode: ({ message }) => ({ _tag: 'IllegalArgumentError' as const, message }) })),
});
const Failure = Schema.Union([Unmatched, HostError, IllegalArgument, Schema.ErrorInstance()]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const request = <const Tag extends string, Payload extends Schema.Top & { readonly Result: Schema.Top }>(tag: Tag, payload: Payload) => Rpc.make<Tag, Payload, Payload['Result'], typeof Failure>(tag, { payload, success: payload.Result, error: Failure });

// --- [COMPOSITION] ---------------------------------------------------------------------

const Script = RpcGroup.make(request('evaluate', Evaluation));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Evaluation, Failure, ObjectModel, Open, Rasterize, request, Script, Written };
