import type { Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFUSED = new Set(['CANNOT_CALL_NAMESPACE', 'COMMONJS_VARIABLE_IN_ESM', 'DUPLICATE_SHEBANG', 'EMPTY_IMPORT_META', 'FILE_NAME_CONFLICT', 'IMPORT_IS_UNDEFINED', 'MISSING_GLOBAL_NAME', 'MISSING_NAME_OPTION_FOR_IIFE_EXPORT', 'MIXED_EXPORTS', 'NAMESPACE_CONFLICT', 'SHIMMED_EXPORT', 'UNRESOLVED_ENTRY']);

// --- [COMPOSITION] ---------------------------------------------------------------------

const checks = (): Plugin => ({
    name: '@rasm/vite/checks',
    onLog(level, log) {
        return level === 'warn' && REFUSED.has(log.code ?? '') ? this.error(log) : undefined;
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { checks };
