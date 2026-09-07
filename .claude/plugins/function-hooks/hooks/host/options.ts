// Plugin options narrowed from the host-validated values and the registration gate of an option, the manifest holds every type and default

// --- [IMPORTS] -------------------------------------------------------------------------

import type { PluginOptions } from 'claude-code';
import { fromBoolean, fromPredicate } from '../composition/option.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPTIONS = {
    packageManager: { type: 'string' },
    speak: { type: 'boolean' },
    classify: { type: 'boolean' },
    dispatch: { type: 'boolean' },
} as const satisfies Record<string, { readonly type: 'string' | 'boolean' }>;

// --- [TYPES] ---------------------------------------------------------------------------

type Options = {
    readonly [K in keyof typeof OPTIONS]: { string: string; boolean: boolean }[(typeof OPTIONS)[K]['type']];
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isOptions = (raw: PluginOptions): raw is PluginOptions & Options =>
    Object.entries(OPTIONS).every(([name, row]) => typeof raw[name] === row.type);

const options = (raw: PluginOptions): Options =>
    fromPredicate(_isOptions)(raw).match<Options>({
        some: (valid) =>
            Object.freeze({
                packageManager: valid.packageManager,
                speak: valid.speak,
                classify: valid.classify,
                dispatch: valid.dispatch,
            }),
        none: () => {
            throw new Error('Plugin options do not match their declared types, plugin.json validates each option before the module loads');
        },
    });

// The registration gate of an option, the hooks under it register while the option is on
const whenEnabled = (enabled: boolean, register: () => void): void => fromBoolean(enabled).match<void>({ some: register, none: () => undefined });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Options };
export { OPTIONS, options, whenEnabled };
