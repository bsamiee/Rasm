// --- [IMPORTS] -------------------------------------------------------------------------

import { entrypoints } from 'adobe:uxp';
import { lifecycle } from '@rasm/creative-cloud-server/client';
import { client } from './client.ts';
import { bridge } from './uxp.config.ts';

// --- [ENTRY] ---------------------------------------------------------------------------

entrypoints.setup(lifecycle(client, bridge.panel.id) as typeof entrypoints);
