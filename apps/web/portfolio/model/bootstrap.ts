import { Schema } from 'effect';
import { PortfolioData } from '#/model/portfolio.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Session = Schema.Literals(['owner', 'signedIn', 'signedOut']);
const Bootstrap = Schema.Struct({ published: PortfolioData, session: Session });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Bootstrap, Session };
