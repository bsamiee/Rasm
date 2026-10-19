import { PATH, WIRE } from '@rasm/indesign/service/contract';
import { group, handlers, identity } from '@rasm/indesign/service/panel';
import { linked } from '@rasm/uxp/rpc';
import { launch } from '@rasm/uxp/runtime';
import { Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

launch(linked(group, { peer: 'Atelier', url: `${import.meta.env.ATELIER}${PATH}`, identity, serialization: WIRE }).pipe(Layer.provide(handlers)));
