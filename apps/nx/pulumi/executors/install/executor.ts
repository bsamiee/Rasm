import { Effect } from 'effect';
import { run, StackRequest } from '#/executors/deploy/executor';

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = run((_, context) => Effect.succeedSome({ request: StackRequest.Install({ context }), operation: 'up' }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
