import { hooks } from '@rasm/policy/claude';
import { policy } from '#/policy';

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: Parameters<typeof hooks>[0]) {
    hooks(on, policy);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
