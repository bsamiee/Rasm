import { LongestSide } from '@rasm/applications';
import { HostError, ObjectModel, Reference } from '@rasm/automation';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, Option, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, String } from 'effect';
import { Url } from 'effect/http';
import { Rpc, RpcGroup, RpcMiddleware } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Requests = RpcGroup.Rpcs<typeof Requests>;
type Envelope<Current extends Requests = Requests> = Current extends Requests ? ReturnType<typeof Envelope<Current['_tag'], Current['payloadSchema']>>['Encoded'] : never;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXECUTE_SCRIPT = 'execute_script';
const BODY = 'function body() {';

// --- [MODELS] --------------------------------------------------------------------------

// --- [REQUESTS]

const HostUrl = Schema.String.pipe(
    Schema.decodeTo(Schema.URL, {
        decode: SchemaGetter.transformEffect((text, options) => Effect.fromResult(Url.fromString(text)).pipe(Effect.mapError(() => new SchemaIssue.InvalidValue({ expected: 'a valid URL string' }, text, options)))),
        encode: SchemaGetter.transform(({ href }) => href),
    }),
);
class ExecuteScript extends Schema.Class<ExecuteScript, { readonly brand: unique symbol }>('ExecuteScript')({ code: Schema.String.annotate({ description: 'Function expression the host evaluates and calls' }) }) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
    static readonly Awaits = Context.Reference<boolean>('@rasm/atelier-protocol/requests/ExecuteScript/Awaits', { defaultValue: () => false });
    static readonly expression = (body: string, awaits: boolean) => (awaits ? `(async ${BODY}\n${body}\n})` : `(${BODY}\n${body}\n})`);

    get label(): string {
        const [, ...body] = this.code.split(BODY);
        return `Run ${Option.getOrElse(Array.findFirst(Array.map(body.join(BODY).split('\n'), String.trim), String.isNonEmpty), () => EXECUTE_SCRIPT)}`;
    }
}
class Open extends Schema.Class<Open, { readonly brand: unique symbol }>('Open')({ file: HostUrl }) {
    static readonly Result = Reference;

    get label(): string {
        return `Open ${basename(this.file)}`;
    }
}
class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')({
    target: Schema.String.annotate({ description: 'Document, page, spread, artboard, layer, composition, or page item specifier' }),
    longestSide: LongestSide,
    options: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Json).annotate({ description: 'Option fields of the host exporter the target selects' })),
    file: HostUrl,
}) {
    static readonly Result = Schema.Void;

    get label(): string {
        return `Render ${this.target} to ${basename(this.file)}`;
    }
}

// --- [WIRE]

const Envelope = <Tag extends Requests['_tag'], Payload extends Requests['payloadSchema']>({ _tag, payloadSchema }: { readonly _tag: Tag; readonly payloadSchema: Payload }) => Schema.Struct({ tag: Schema.Literal(_tag), label: Schema.String, payload: payloadSchema });
const completions = Schema.Union([
    Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
    Unmatched,
    Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
    Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.OptionFromOptionalKey(Schema.Finite), line: Schema.OptionFromOptionalKey(Schema.Int) }),
]).pipe(Schema.toTaggedUnion('_tag'));
const Completion = Object.assign(completions, {
    answered: <S extends Schema.Top>(success: S, subject: string) =>
        completions.match({
            Value: ({ value }) => Schema.decodeEffect(success)(value),
            Unmatched: Effect.fail<Unmatched>,
            IllegalArgument: ({ message }) => Effect.fail(new Cause.IllegalArgumentError(message)),
            Thrown: (failed) => {
                const coded = [subject, ...Option.toArray(Option.map(failed.number, (code) => `Error ${code}`))].join(': ');
                return Effect.fail(Option.isSome(failed.line) ? HostError.thrown(`${coded}: ${failed.message}`, failed.line)(failed) : HostError.refused(coded)(failed));
            },
        }),
});

// --- [ERRORS] --------------------------------------------------------------------------

const IllegalArgument = Schema.declare(Cause.isIllegalArgumentError, {
    expected: 'IllegalArgumentError',
    toCodecJson: () => Schema.link<Cause.IllegalArgumentError>()(Schema.TaggedStruct('IllegalArgumentError', { message: Schema.String }), SchemaTransformation.transform({ decode: ({ message }) => new Cause.IllegalArgumentError(message), encode: ({ message }) => ({ _tag: 'IllegalArgumentError' as const, message }) })),
});
const Failure = Schema.Union([Unmatched, HostError, IllegalArgument, Schema.ErrorInstance()]);

// --- [SERVICES] ------------------------------------------------------------------------

class DisplayDialogs extends RpcMiddleware.Service<DisplayDialogs>()('@rasm/atelier-protocol/requests/DisplayDialogs', { error: HostError }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const basename = (file: Open['file']) => decodeURIComponent(file.pathname.slice(file.pathname.lastIndexOf('/') + 1));
const request = <const Tag extends string, Payload extends Schema.Top & { readonly Result: Schema.Top }>(tag: Tag, payload: Payload) => Rpc.make<Tag, Payload, Payload['Result'], typeof Failure>(tag, { payload, success: payload.Result, error: Failure });

// --- [COMPOSITION] ---------------------------------------------------------------------

const Requests = RpcGroup.make(request(EXECUTE_SCRIPT, ExecuteScript), request('open', Open), request('render', Render), request('object_model', ObjectModel));
const Photoshop = { Contract: Requests.middleware(DisplayDialogs).annotate(ExecuteScript.Awaits, true), DisplayDialogs } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Completion, Envelope, ExecuteScript, HostUrl, Open, Photoshop, Render, Requests, request };
