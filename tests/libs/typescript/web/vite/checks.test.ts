import { checks } from '@rasm/vite';
import { build, createLogger, type Plugin } from 'vite';
import { describe, expect, it } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = (modules: Readonly<Record<string, string>>, format: 'es' | 'cjs') => {
    const logged: (readonly [level: string, code: string | undefined])[] = [];
    const sources: Plugin = {
        name: 'sources',
        enforce: 'pre',
        resolveId: (id) => (Object.hasOwn(modules, id) ? `\0${id}` : undefined),
        load: (id) => modules[id.slice(1)],
    };
    const seen: Plugin = {
        name: 'seen',
        onLog(level, log) {
            logged.push([level, log.code]);
        },
    };
    const result = build({ configFile: false, root: import.meta.dirname, customLogger: createLogger('silent'), logLevel: 'silent', plugins: [sources, checks(), seen], build: { write: false, rolldownOptions: { input: 'entry', output: { format } } } });
    return { result, logged };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('rolldown checks', { timeout: 30_000 }, () => {
    it.for([
        { code: 'CANNOT_CALL_NAMESPACE', format: 'es', modules: { entry: "import * as ns from 'a';\nconsole.log(ns());\n", a: 'export const b = 1;\n' } },
        { code: 'EMPTY_IMPORT_META', format: 'cjs', modules: { entry: 'console.log(import.meta.url);\n' } },
        { code: 'IMPORT_IS_UNDEFINED', format: 'es', modules: { entry: "import * as ns from 'a';\nconsole.log(ns.c);\n", a: 'export const b = 1;\n' } },
    ] as const)('fails the build on $code', async ({ code, format, modules }) => {
        const { result } = built(modules, format);

        await expect(result).rejects.toMatchObject({ errors: [{ code }] });
    });

    it('keeps EVAL a warning and writes the chunk', async () => {
        const { result, logged } = built({ entry: "console.log(eval('1'));\n" }, 'es');

        await expect(result).resolves.toMatchObject({ output: [{ type: 'chunk' }] });
        expect(logged).toContainEqual(['warn', 'EVAL']);
    });
});
