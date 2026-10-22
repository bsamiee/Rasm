import type { Shell } from '@rasm/hosting';
import { Schema } from 'effect';
import { PortfolioData } from '#/model/portfolio';

// --- [MODELS] --------------------------------------------------------------------------

const Session = Schema.Union([Schema.Struct({ kind: Schema.Literal('owner') }), Schema.Struct({ kind: Schema.Literal('visitor'), signedIn: Schema.Boolean })]);
const Bootstrap = Schema.Struct({ initial: PortfolioData, session: Session });
const shell: Shell<typeof Bootstrap> = { root: 'portfolio', data: 'portfolio-data', schema: Bootstrap };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Session, shell };
