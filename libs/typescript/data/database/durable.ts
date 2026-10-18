import { DurableObject } from 'cloudflare:workers';

// --- [SERVICES] ------------------------------------------------------------------------

class Store extends DurableObject {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Store };
