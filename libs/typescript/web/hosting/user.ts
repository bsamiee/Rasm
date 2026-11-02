import { Effect, Schema } from 'effect';
import { HttpServerRequest, UrlParams } from 'effect/http';

// --- [MODELS] --------------------------------------------------------------------------

class User extends Schema.Class<User, { readonly brand: unique symbol }>('User')({ id: Schema.String, email: Schema.OptionFromOptionalKey(Schema.String) }) {
    static readonly sites = {
        headers: { id: 'oai-authenticated-user-id', email: 'oai-authenticated-user-email' },
        paths: { signin: '/signin-with-chatgpt', signout: '/signout-with-chatgpt' },
        returnTo: 'return_to',
    } as const;
    static readonly current = HttpServerRequest.schemaHeaders(User.pipe(Schema.encodeKeys(User.sites.headers))).pipe(Effect.option);
    static readonly path = (action: keyof typeof User.sites.paths, returnTo: string) => `${User.sites.paths[action]}?${UrlParams.toString(UrlParams.fromInput({ [User.sites.returnTo]: returnTo }))}`;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { User };
