// engine.create, builds $ from NoEngineInterface, a hook adds or withholds a noun (EngineCreateInput, EngineCreateResult), no noun row yet

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const engineCreate = (_on: On, _options: Options): void => undefined;

// --- [EXPORTS] -------------------------------------------------------------------------

export { engineCreate };
