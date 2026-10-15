import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { normalizePath } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { base, directories } from './pdfjs.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pdfjs = (): ReturnType<typeof viteStaticCopy> => {
    const root = dirname(createRequire(import.meta.url).resolve('pdfjs-dist/package.json'));
    return viteStaticCopy({ targets: Object.values(directories).map((directory) => ({ src: normalizePath(join(root, directory, '*')), dest: `${base}/${directory}`, rename: { stripBase: true } })) });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfjs };
