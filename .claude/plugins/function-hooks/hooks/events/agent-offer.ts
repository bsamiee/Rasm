// agent.offer, one matched hook per OFFERS row answering the row's isOffered value (AgentOfferInput, AgentOfferResult)

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';
import { OFFERS, type OfferRow } from '../policies/agents.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

// Registration is the boundary, one matched hook per row, and the binding states the row type an empty tuple leaves as never
const agentOffer = (on: On, _options: Options): void => {
    const rows: readonly OfferRow[] = OFFERS;
    for (const row of rows) {
        on('agent.offer', { agent: row.agent }, () => ({ isOffered: row.isOffered }));
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { agentOffer };
