import { ExecuteScript, HostSession, Requests } from '@rasm/atelier-protocol';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Photoshop = Requests.middleware(HostSession).annotate(ExecuteScript.Awaits, true);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Photoshop };
