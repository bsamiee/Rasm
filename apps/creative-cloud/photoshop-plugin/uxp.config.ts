// --- [IMPORTS] -------------------------------------------------------------------------

import { type Bridge, type Facts, plugin } from '@rasm/creative-cloud-server/manifest';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import manifest from './package.json' with { type: 'json' };

// --- [MANIFEST] ------------------------------------------------------------------------

const facts: Facts = {
    id: 'rasm.photoshop.bridge',
    name: 'Rasm Photoshop Bridge',
    panel: {
        label: { default: 'Rasm Bridge' },
        minimumSize: { width: 180, height: 80 },
        maximumSize: { width: 2000, height: 2000 },
        preferredDockedSize: { width: 235, height: 120 },
        preferredFloatingSize: { width: 235, height: 120 },
        icons: [
            { width: 23, height: 23, path: 'icons/dark.png', scale: [1, 2], theme: ['darkest', 'dark', 'medium'] },
            { width: 23, height: 23, path: 'icons/light.png', scale: [1, 2], theme: ['lightest', 'light'] },
        ],
    },
    permissions: {},
};

const bridge: Bridge = plugin(HOSTS.photoshop, manifest, facts);

// --- [EXPORTS] -------------------------------------------------------------------------

export { bridge };
