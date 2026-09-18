// --- [IMPORTS] -------------------------------------------------------------------------

import { type Bridge, plugin } from '@rasm/creative-cloud-server/manifest';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import manifest from './package.json' with { type: 'json' };

// --- [MANIFEST] ------------------------------------------------------------------------

const bridge: Bridge = plugin(HOSTS.indesign, manifest, {
    id: 'rasm.indesign.bridge',
    name: 'Rasm InDesign Bridge',
    panel: { label: { default: 'Rasm' }, minimumSize: { width: 200, height: 60 }, preferredDockedSize: { width: 235, height: 80 } },
    permissions: { localFileSystem: 'fullAccess' },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { bridge };
