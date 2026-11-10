import { Redacted } from 'effect';
import { type Dialect, Engine } from '#/engine';
import { Pg } from '#/pg';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hyperdriveEngine = (pooled: { readonly connectionString: string }, names: Dialect['names'] = Engine.names.snake) => Pg.layer({ url: Redacted.make(pooled.connectionString) }, names);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hyperdriveEngine };
