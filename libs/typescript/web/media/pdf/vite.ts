import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { normalizePath } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { assets } from '#/pdf/assets';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pdfjs = () => {
    const root = dirname(createRequire(import.meta.url).resolve('pdfjs-dist/package.json'));
    return viteStaticCopy({ targets: Object.values(assets.directories).map((directory) => ({ src: normalizePath(join(root, directory, '*')), dest: `${assets.path}/${directory}`, rename: { stripBase: true } })) });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfjs };
