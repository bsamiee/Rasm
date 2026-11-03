// --- [CONSTANTS] -----------------------------------------------------------------------

const sites = {
    headers: { id: 'oai-authenticated-user-id', email: 'oai-authenticated-user-email' },
    paths: { signin: '/signin-with-chatgpt', signout: '/signout-with-chatgpt' },
    returnTo: 'return_to',
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { sites };
