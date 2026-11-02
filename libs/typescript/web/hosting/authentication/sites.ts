import { Schema } from 'effect';
import { type HttpServerRequest, UrlParams } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const contract = {
    headers: { id: 'oai-authenticated-user-id', email: 'oai-authenticated-user-email' },
    paths: { signin: '/signin-with-chatgpt', signout: '/signout-with-chatgpt' },
    returnTo: 'return_to',
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const User = Schema.Struct({ id: Schema.String, email: Schema.OptionFromOptionalKey(Schema.String) }).pipe(Schema.encodeKeys(contract.headers));

// --- [OPERATIONS] ----------------------------------------------------------------------

const sessionPath = (action: keyof typeof contract.paths, returnTo: string) => `${contract.paths[action]}?${UrlParams.toString({ [contract.returnTo]: returnTo })}`;
const user = ({ headers }: HttpServerRequest.HttpServerRequest) => Schema.decodeUnknownOption(User)(headers);

// --- [EXPORTS] -------------------------------------------------------------------------

export { contract, sessionPath, user };
