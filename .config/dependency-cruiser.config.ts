import { builtinModules } from 'node:module';
import type { IConfiguration } from 'dependency-cruiser';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUNTIME_BOUND = ['harfbuzzjs', 'icu', 'fontkit', 'pdfjs-dist', '@electric-sql/pglite', '@effect/platform-node'];

// --- [COMPOSITION] ---------------------------------------------------------------------

export default {
    forbidden: [
        {
            name: 'runtime-bound-root-entry',
            comment: 'Library root entries load in every runtime importing the package, a module reading Node, wasm, or an Adobe host joins a runtime-named entry',
            severity: 'error',
            from: { path: String.raw`^libs/typescript/[^/]+/[^/]+/index\.ts$` },
            to: { reachable: true, path: [`^(?:${builtinModules.map(RegExp.escape).join('|')})$`, '^adobe:', `node_modules/(?:${RUNTIME_BOUND.map(RegExp.escape).join('|')})/`] },
        },
    ],
    options: {
        doNotFollow: { path: 'node_modules' },
        tsPreCompilationDeps: false,
        enhancedResolveOptions: { exportsFields: ['exports'], conditionNames: ['import', 'node', 'default'], extensions: ['.ts', '.tsx'] },
    },
} satisfies IConfiguration;
