import { Identity } from '#/identity/identity';

// --- [CONSTANTS] -----------------------------------------------------------------------

const Sites = {
    identity: new Identity({ headers: { id: 'oai-authenticated-user-id', email: 'oai-authenticated-user-email' }, paths: { signin: '/signin-with-chatgpt', signout: '/signout-with-chatgpt' }, returnTo: 'return_to' }),
    local: { id: 'local_seedy', email: 'seedy@sites.test' },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Sites };
