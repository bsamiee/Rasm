import type { Path } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VITEST_INTEGRATION_TESTS = '**/*.integration.test.{ts,tsx}';

// --- [OPERATIONS] ----------------------------------------------------------------------

const suitePath = (path: Path.Path, base: string, directory: string) => (directory === base || directory.startsWith(`${base}${path.sep}`) ? directory : path.join(base, ...directory.split(path.sep).filter((segment, index) => index !== 1 || segment !== path.basename(base))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { suitePath, VITEST_INTEGRATION_TESTS };
