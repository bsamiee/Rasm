import type { posix } from 'node:path';

// --- [OPERATIONS] ----------------------------------------------------------------------

const windows = (actual: typeof posix) => {
    const separated =
        (operation: (...paths: string[]) => string) =>
        (...paths: string[]) => {
            const result = operation(...paths.map((at) => at.replaceAll('\\', '/')));
            return actual.posix.isAbsolute(result) ? result : result.replaceAll('/', '\\');
        };
    const shaped: typeof posix = {
        ...actual.posix,
        dirname: actual.win32.dirname,
        basename: actual.win32.basename,
        extname: actual.win32.extname,
        isAbsolute: actual.win32.isAbsolute,
        parse: actual.win32.parse,
        matchesGlob: actual.win32.matchesGlob,
        join: separated(actual.posix.join),
        resolve: separated(actual.posix.resolve),
        normalize: separated(actual.posix.normalize),
        relative: separated(actual.posix.relative),
        posix: actual.posix,
        win32: actual.win32,
    };
    return { ...shaped, default: shaped };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { windows };
