import { Option } from 'effect';
import { Headers, type HttpServerRequest } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const contract = {
    headers: { id: 'oai-authenticated-user-id', email: 'oai-authenticated-user-email' },
    paths: { signin: '/signin-with-chatgpt', signout: '/signout-with-chatgpt' },
    returnTo: 'return_to',
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const sessionPath = (action: keyof typeof contract.paths, returnTo: string) => `${contract.paths[action]}?${new URLSearchParams({ [contract.returnTo]: returnTo })}`;
const user = ({ headers }: HttpServerRequest.HttpServerRequest) => Option.map(Headers.get(headers, contract.headers.id), (id) => ({ id, email: Headers.get(headers, contract.headers.email) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { contract, sessionPath, user };
