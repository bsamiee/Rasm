import { Schema } from 'effect';
import { PortfolioData } from '#/model/portfolio';

// --- [MODELS] --------------------------------------------------------------------------

const Session = Schema.Literals(['owner', 'signedIn', 'signedOut']);
const Bootstrap = Schema.Struct({ initial: PortfolioData, session: Session });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Bootstrap, Session };
