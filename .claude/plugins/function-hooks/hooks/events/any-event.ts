// The audit over every event, registered first in register.ts to wrap every other hook of the plugin

// --- [IMPORTS] -------------------------------------------------------------------------

import type { EventName, On } from 'claude-code';
import { fromBoolean } from '../composition/option.ts';
import type { Options } from '../host/options.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _AUDIT: readonly EventName[] = ['tool.call', 'prompt.submit', 'agent.spawn'];

// --- [REGISTRATION] --------------------------------------------------------------------

// The membership test precedes every $ call, because $ is NoEngineInterface at engine.create
const anyEvent = (on: On, _options: Options): void => {
    on('*', ($, e, next) => {
        fromBoolean(_AUDIT.includes(next.event)).match<void>({
            some: () => $.ui.log(`${next.event} from ${next.origin}`),
            none: () => undefined,
        });
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { anyEvent };
