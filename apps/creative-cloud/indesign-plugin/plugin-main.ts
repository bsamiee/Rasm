// --- [IMPORTS] -------------------------------------------------------------------------

import { entrypoints } from 'adobe:uxp';
import { lifecycle } from '@rasm/creative-cloud-server/client';
import { client } from './client.ts';
import { registered } from './enumerations.ts';
import { bridge } from './uxp.config.ts';

// --- [LIFECYCLE] -----------------------------------------------------------------------

entrypoints.setup(lifecycle(client(registered), bridge.panel.id) as typeof entrypoints);
