import { LongestSide } from '@rasm/applications';
import { Declaration, HostError, Reference } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Option, Schema, SchemaTransformation, String, Struct } from 'effect';
import { Rpc, RpcGroup, RpcMiddleware } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BODY = 'function body() {';

// --- [MODELS] --------------------------------------------------------------------------

class ExecuteScript extends Schema.Class<ExecuteScript, { readonly brand: unique symbol }>('ExecuteScript')({ code: Schema.String.annotate({ description: 'Function expression the host evaluates and calls' }) }) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
    static readonly Awaits = Context.Reference<boolean>('@rasm/atelier-protocol/requests/ExecuteScript/Awaits', { defaultValue: () => false });
    static readonly expression = (body: string, awaits: boolean) => (awaits ? `(async ${BODY}\n${body}\n})` : `(${BODY}\n${body}\n})`);

    get label(): string {
        const [, ...body] = this.code.split(BODY);
        return `Run ${Option.getOrElse(Array.findFirst(Array.map(body.join(BODY).split('\n'), String.trim), String.isNonEmpty), () => 'script')}`;
    }
}
class Open extends Schema.Class<Open, { readonly brand: unique symbol }>('Open')({ file: Schema.URLFromString }) {
    static readonly Result = Reference;

    get label(): string {
        return `Open ${basename(this.file)}`;
    }
}
class Rasterize extends Schema.Class<Rasterize, { readonly brand: unique symbol }>('Rasterize')({
    target: Schema.String.annotate({ description: 'Document, page, spread, artboard, layer, composition, or page item specifier' }),
    longestSide: LongestSide,
    options: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Json).annotate({ description: 'Option fields of the host exporter the target selects' })),
    file: Schema.URLFromString,
}) {
    static readonly Result = Schema.Void;

    get label(): string {
        return `Rasterize ${this.target} to ${basename(this.file)}`;
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

// --- [SERVICES] ------------------------------------------------------------------------

class HostSession extends RpcMiddleware.Service<HostSession>()('@rasm/atelier-protocol/requests/HostSession', { error: HostError }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const basename = (file: URL) => decodeURIComponent(file.pathname.slice(file.pathname.lastIndexOf('/') + 1));
const request = <const Tag extends string, Payload extends Schema.Top & { readonly Result: Schema.Top }>(tag: Tag, payload: Payload) => Rpc.make<Tag, Payload, Payload['Result'], typeof Failure>(tag, { payload, success: payload.Result, error: Failure });

// --- [COMPOSITION] ---------------------------------------------------------------------

const Requests = RpcGroup.make(request('execute_script', ExecuteScript), request('open', Open), request('rasterize', Rasterize), request('object_model', ObjectModel));

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecuteScript, HostSession, ObjectModel, Open, Rasterize, Requests, request, Written };
