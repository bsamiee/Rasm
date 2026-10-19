import { group, handlers, identity } from '@rasm/indesign/mcp/panel';
import { PATH, WIRE } from '@rasm/indesign/mcp/protocol';
import { linked } from '@rasm/uxp/rpc';
import { launch } from '@rasm/uxp/runtime';
import { Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

launch(linked(group, { peer: 'Atelier', url: `${import.meta.env.ATELIER}${PATH}`, identity, serialization: WIRE }).pipe(Layer.provide(handlers)));
