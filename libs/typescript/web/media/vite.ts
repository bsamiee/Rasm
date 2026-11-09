import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { normalizePath } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { pdfjsAssets } from '#/pdfjs-assets';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pdfjs = () => {
    const root = dirname(fileURLToPath(import.meta.resolve('pdfjs-dist/package.json')));
    return viteStaticCopy({ targets: Object.values(pdfjsAssets.directories).map((directory) => ({ src: normalizePath(join(root, directory, '*')), dest: `${pdfjsAssets.path}/${directory}`, rename: { stripBase: true } })) });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfjs };
