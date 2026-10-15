import { Option } from 'effect';
import { Headers, type HttpServerRequest } from 'effect/http';

// --- [TYPES] ---------------------------------------------------------------------------

interface User {
    readonly id: string;
    readonly email: Option.Option<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const sessionPath = (action: 'signin' | 'signout', returnTo: string): string => `/${action}-with-chatgpt?${new URLSearchParams({ return_to: returnTo })}`;
const user = ({ headers }: HttpServerRequest.HttpServerRequest): Option.Option<User> => Option.map(Headers.get(headers, 'oai-authenticated-user-id'), (id) => ({ id, email: Headers.get(headers, 'oai-authenticated-user-email') }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { sessionPath, user };
