import { createRequire } from 'node:module';
import { join } from 'node:path';
import { mergeConfig, searchForWorkspaceRoot, type UserConfig } from 'vite';

// --- [OPERATIONS] ----------------------------------------------------------------------

const bundle = (directory: string, config: UserConfig): UserConfig => {
    const { name }: { readonly name: string } = createRequire(join(directory, 'package.json'))('./package.json');
    const root = searchForWorkspaceRoot(directory);
    return mergeConfig({ build: { outDir: join(root, '.artifacts', 'vite', name), emptyOutDir: true, rolldownOptions: { output: { codeSplitting: false } } } }, config);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { bundle };
