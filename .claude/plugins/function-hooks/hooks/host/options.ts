// Plugin options narrowed once from the host-validated values, the manifest holds every type and default

// --- [IMPORTS] -------------------------------------------------------------------------

import type { PluginOptions } from 'claude-code';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPTIONS = { packageManager: 'string', speak: 'boolean', classify: 'boolean', dispatch: 'boolean' } as const;

// --- [TYPES] ---------------------------------------------------------------------------

type Options = { readonly [K in keyof typeof OPTIONS]: { string: string; boolean: boolean }[(typeof OPTIONS)[K]] };

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isOptions = (raw: PluginOptions): raw is PluginOptions & Options => Object.entries(OPTIONS).every(([name, type]) => typeof raw[name] === type);

const options = (raw: PluginOptions): Options => {
    if (_isOptions(raw)) {
        return raw;
    }
    throw new Error('Plugin options do not match their declared types, plugin.json validates each option before the module loads');
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Options };
export { options };
