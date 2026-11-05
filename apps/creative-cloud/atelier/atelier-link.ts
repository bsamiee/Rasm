import { NetAddress } from 'effect/net';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PORT = 7490;
const atelierLink = { path: '/link', redial: '1 second', listen: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, PORT) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { atelierLink };
