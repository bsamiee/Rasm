import { Effect, Function, type Record, Schema } from 'effect';
import { HttpServerRequest, UrlParams } from 'effect/http';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sites {
    readonly headers: Record.ReadonlyRecord<keyof typeof User.fields, string>;
    readonly paths: Record.ReadonlyRecord<'signin' | 'signout', `/${string}`>;
    readonly returnTo: string;
}

// --- [MODELS] --------------------------------------------------------------------------

class User extends Schema.Class<User, { readonly brand: unique symbol }>('User')({ id: Schema.NonEmptyString, email: Schema.OptionFromOptionalKey(Schema.String) }) {
    static readonly headers = Function.memoize((sites: Sites) => User.pipe(Schema.encodeKeys(sites.headers)));
    static readonly current = (sites: Sites) => HttpServerRequest.schemaHeaders(User.headers(sites)).pipe(Effect.option);
    static readonly href = (sites: Sites, action: keyof Sites['paths'], target: string) => `${sites.paths[action]}?${UrlParams.toString(UrlParams.fromInput({ [sites.returnTo]: target }))}`;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { User };
