import { Data, Effect, Schema } from 'effect';
import { type HttpRouter, HttpServerRequest, UrlParams } from 'effect/http';

// --- [MODELS] --------------------------------------------------------------------------

class User extends Schema.Class<User, { readonly brand: unique symbol }>('User')({ id: Schema.NonEmptyString, email: Schema.OptionFromOptionalKey(Schema.String) }) {}

class Identity extends Data.Class<{
    readonly headers: { readonly id: string; readonly email: string };
    readonly paths: { readonly signin: HttpRouter.PathInput; readonly signout: HttpRouter.PathInput };
    readonly returnTo: string;
}> {
    readonly user = User.pipe(Schema.encodeKeys(this.headers));
    readonly current = HttpServerRequest.schemaHeaders(this.user).pipe(Effect.option);
    href(action: keyof Identity['paths'], target: string) {
        return `${this.paths[action]}?${UrlParams.toString(UrlParams.fromInput({ [this.returnTo]: target }))}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Identity, User };
