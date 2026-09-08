// session.start prunes the session-scoped rows of every other session

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import { key, keys, type Namespace } from '../host/store.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// Session-scoped keys of another session, deleted at start, the store then holds one session's facts beside the durable rows
const _SESSION_SCOPED: readonly Namespace[] = ['injected', 'loaded', 'snapshot', 'dns', 'prompt'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// The keys the start deletes, other sessions' facts
const _expired = (all: readonly string[], session: string): readonly string[] =>
    _SESSION_SCOPED.flatMap((namespace) =>
        keys(namespace)(all).filter((name) => name !== key(namespace, session) && !name.startsWith(`${key(namespace, session)}/`)),
    );

// --- [REGISTRATION] --------------------------------------------------------------------

const sessionStart = (on: On): void => {
    on('session.start', async ($, e, next) => {
        const [session, all] = await Promise.all([$.session.id(), $.store.keys()]);
        await Promise.all(_expired(all, session).map((name) => $.store.delete(name)));
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { sessionStart };
