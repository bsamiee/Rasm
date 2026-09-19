// --- [IMPORTS] -------------------------------------------------------------------------

import { entrypoints } from 'adobe:uxp';
import { lifecycle } from '@rasm/creative-cloud-server/client';
import { client } from './client.ts';

// --- [ENTRY] ---------------------------------------------------------------------------

entrypoints.setup(lifecycle(client));
