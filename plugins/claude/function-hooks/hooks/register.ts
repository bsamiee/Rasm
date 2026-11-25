import { host } from '@rasm/policy/claude';
import { policy } from '#/policy';

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: Parameters<typeof host>[0]) {
    host(on, policy);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
