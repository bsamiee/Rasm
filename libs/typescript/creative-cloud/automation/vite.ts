import { mergeConfig, type UserConfig } from 'vite';

// --- [OPERATIONS] ----------------------------------------------------------------------

const bundle = (config: UserConfig): UserConfig => mergeConfig({ build: { rolldownOptions: { output: { codeSplitting: false } } } }, config);

// --- [EXPORTS] -------------------------------------------------------------------------

export { bundle };
