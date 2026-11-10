import type { WorkerConfig } from 'cf/config';
import { Array } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const workerConfig = <const C extends WorkerConfig>(config: C) => ({ ...config, compatibilityFlags: Array.dedupe([...(config.compatibilityFlags ?? []), 'enable_request_signal']) }) satisfies WorkerConfig;

// --- [EXPORTS] -------------------------------------------------------------------------

export { workerConfig };
