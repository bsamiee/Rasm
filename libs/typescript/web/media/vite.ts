import { findPackageJSON } from 'node:module';
import { dirname, join } from 'node:path';
import { Cause, UndefinedOr } from 'effect';
import { normalizePath } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { pdfjsAssets } from '#/pdfjs-assets';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pdfjs = () => {
    const root = dirname(UndefinedOr.getOrThrowWith(findPackageJSON('pdfjs-dist', import.meta.url), () => new Cause.NoSuchElementError('pdfjs-dist resolves to no package.json')));
    return viteStaticCopy({ targets: Object.values(pdfjsAssets.directories).map((directory) => ({ src: normalizePath(join(root, directory, '*')), dest: `${pdfjsAssets.path}/${directory}`, rename: { stripBase: true } })) });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pdfjs };
