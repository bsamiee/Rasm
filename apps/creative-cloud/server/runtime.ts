// --- [IMPORTS] -------------------------------------------------------------------------

import { TextDecoder, TextEncoder } from '@exodus/bytes/encoding-lite.js';

// --- [GLOBALS] -------------------------------------------------------------------------

globalThis.TextDecoder = TextDecoder;
globalThis.TextEncoder = TextEncoder;
Reflect.deleteProperty(Reflect.get(globalThis, 'process').hrtime, 'bigint');
