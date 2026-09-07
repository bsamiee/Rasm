// The plain hook over every event (AnyEventInput), observed alone, no row because no reader consumes a decision row

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';

// --- [REGISTRATION] --------------------------------------------------------------------

const anyEvent = (_on: On, _options: Options): void => undefined;

// --- [EXPORTS] -------------------------------------------------------------------------

export { anyEvent };
