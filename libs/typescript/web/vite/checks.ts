import type { Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFUSED = new Set(['CANNOT_CALL_NAMESPACE', 'COMMONJS_VARIABLE_IN_ESM', 'DUPLICATE_SHEBANG', 'EMPTY_IMPORT_META', 'FILE_NAME_CONFLICT', 'IMPORT_IS_UNDEFINED', 'MISSING_GLOBAL_NAME', 'MISSING_NAME_OPTION_FOR_IIFE_EXPORT', 'MIXED_EXPORTS', 'NAMESPACE_CONFLICT', 'SHIMMED_EXPORT', 'UNRESOLVED_ENTRY']);

// --- [COMPOSITION] ---------------------------------------------------------------------

const checks = (): Plugin => ({
    name: '@rasm/vite/checks',
    config: () => ({ build: { rolldownOptions: { experimental: { lazyBarrel: true } } } }),
    onLog(level, log) {
        return level === 'warn' && REFUSED.has(log.code ?? '') ? this.error(log) : undefined;
    },
    resolveId: {
        order: 'pre',
        handler(source, importer, options) {
            const query = source.indexOf('?');
            return source.startsWith('#') && query > 0 ? this.resolve(source.slice(0, query), importer, options).then((resolved) => resolved && { ...resolved, id: `${resolved.id}${source.slice(query)}` }) : undefined;
        },
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { checks };
