// turn.start, the prompt a model turn begins with (TurnStartInput, TurnStartResult), observed alone, no observation row yet

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const turnStart = (_on: On, _options: Options): void => undefined;

// --- [EXPORTS] -------------------------------------------------------------------------

export { turnStart };
