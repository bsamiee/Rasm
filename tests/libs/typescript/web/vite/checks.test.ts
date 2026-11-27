import { checks } from '@rasm/vite/checks';
import { build, createLogger, type Plugin, type Rolldown } from 'vite';
import { describe, expect, it } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = (modules: Readonly<Record<string, string>>, options: Rolldown.InputOptions & { readonly output: Rolldown.OutputOptions }, plugins: readonly Plugin[]) => {
    const logged: (string | undefined)[] = [];
    const sources: Plugin = {
        name: 'sources',
        enforce: 'pre',
        resolveId: (id) => (Object.hasOwn(modules, id) ? `\0${id}` : undefined),
        load: (id) => modules[id.slice(1)],
    };
    const seen: Plugin = {
        name: 'seen',
        onLog(level, log) {
            logged.push(...(level === 'warn' ? [log.code] : []));
        },
    };
    const result = build({ configFile: false, root: import.meta.dirname, customLogger: createLogger('silent'), logLevel: 'silent', plugins: [sources, ...plugins, seen], build: { write: false, rolldownOptions: { input: 'entry', ...options } } });
    return { result, logged };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('rolldown warnings the checks refuse', () => {
    it.for<{ readonly code: string; readonly options: Parameters<typeof built>[1]; readonly modules: Readonly<Record<string, string>> }>([
        { code: 'CANNOT_CALL_NAMESPACE', options: { output: { format: 'es' } }, modules: { entry: "import * as ns from 'a';\nconsole.log(ns());\n", a: 'export const b = 1;\n' } },
        { code: 'COMMONJS_VARIABLE_IN_ESM', options: { output: { format: 'es' } }, modules: { entry: 'export const a = 1;\nmodule.exports.b = 2;\n' } },
        { code: 'DUPLICATE_SHEBANG', options: { output: { format: 'es', postBanner: '#!/usr/bin/env node' } }, modules: { entry: '#!/usr/bin/env node\nconsole.log(process.argv);\n' } },
        { code: 'EMPTY_IMPORT_META', options: { output: { format: 'cjs' } }, modules: { entry: 'console.log(import.meta.url);\n' } },
        {
            code: 'FILE_NAME_CONFLICT',
            options: {
                plugins: {
                    name: 'sheets',
                    buildStart() {
                        this.emitFile({ type: 'asset', fileName: 'sheet.txt', source: 'Section A-A' });
                        this.emitFile({ type: 'asset', fileName: 'Sheet.txt', source: 'Section B-B' });
                    },
                },
                output: { format: 'es' },
            },
            modules: { entry: 'console.log(1);\n' },
        },
        { code: 'IMPORT_IS_UNDEFINED', options: { output: { format: 'es' } }, modules: { entry: "import * as ns from 'a';\nconsole.log(ns.c);\n", a: 'export const b = 1;\n' } },
        { code: 'MISSING_GLOBAL_NAME', options: { external: ['outside'], preserveEntrySignatures: 'strict', output: { format: 'iife', name: 'bundle' } }, modules: { entry: "import value from 'outside';\nexport const shown = value;\n" } },
        { code: 'MISSING_NAME_OPTION_FOR_IIFE_EXPORT', options: { preserveEntrySignatures: 'strict', output: { format: 'iife' } }, modules: { entry: 'export const a = 1;\n' } },
        { code: 'MIXED_EXPORTS', options: { preserveEntrySignatures: 'strict', output: { format: 'cjs' } }, modules: { entry: 'export default 1;\nexport const a = 2;\n' } },
        { code: 'NAMESPACE_CONFLICT', options: { output: { format: 'es' } }, modules: { entry: "export * from 'a';\nexport * from 'b';\n", a: 'export const x = 1;\n', b: 'export const x = 2;\n' } },
        { code: 'SHIMMED_EXPORT', options: { shimMissingExports: true, output: { format: 'es' } }, modules: { entry: "import { missing } from 'a';\nconsole.log(missing);\n", a: 'export const b = 1;\n' } },
    ])('reports $code as a warning a build survives, and fails the build on it under the checks', async ({ code, options, modules }) => {
        const plain = built(modules, options, []);
        await expect(plain.result).resolves.toBeDefined();
        expect(plain.logged).toContain(code);
        await expect(built(modules, options, [checks()]).result).rejects.toMatchObject({ errors: [{ code }] });
    });

    it('keeps EVAL a warning and emits the chunk', async () => {
        const { result, logged } = built({ entry: "console.log(eval('1'));\n" }, { output: { format: 'es' } }, [checks()]);
        await expect(result).resolves.toMatchObject({ output: [{ type: 'chunk' }] });
        expect(logged).toContain('EVAL');
    });
});
