// --- [IMPORTS] -------------------------------------------------------------------------

import './runtime.ts';
import { entrypoints } from 'adobe:uxp';
import { lifecycle } from '@rasm/creative-cloud-server/client';
import { client } from './client.ts';
import { panel } from './uxp.config.ts';

// --- [ENTRY] ---------------------------------------------------------------------------

entrypoints.setup(lifecycle(client, panel.id) as typeof entrypoints);
