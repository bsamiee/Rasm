import { NetAddress } from 'effect/net';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PORT = 7490;
const atelierLink = { path: '/link', redial: '1 second', listen: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, PORT), debug: { ILST: 7491, AEFT: 7492 } } as const;
const atelierMark = 'apps/creative-cloud/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg';

// --- [EXPORTS] -------------------------------------------------------------------------

export { atelierLink, atelierMark };
