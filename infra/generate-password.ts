import { RandomPassword } from '@pulumi/random';

// --- [OPERATIONS] ----------------------------------------------------------------------

const generatePassword = (resource: string) => new RandomPassword(resource, { length: 40, special: false });

// --- [EXPORTS] -------------------------------------------------------------------------

export { generatePassword };
