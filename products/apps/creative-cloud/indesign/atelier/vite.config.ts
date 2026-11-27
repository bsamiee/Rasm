import { join } from 'node:path';
import { Declaration } from '@rasm/object-model/model';
import { readTypeScript } from '@rasm/object-model/node';
import base from '@rasm/uxp/tsconfig.base.json' with { type: 'json' };
import { pluginConfig } from '@rasm/uxp/vite';
import { Effect, Option, Schema } from 'effect';
import { mergeConfig, type Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECLARED = /^indesign-uxp-types\/flat\?inline$/u;
const MOUNTED = '\0declared';
const INLINED = Declaration.Model.pipe(Schema.fromJsonString, Schema.fromJsonString);

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(pluginConfig(), {
    plugins: [
        {
            name: 'declared',
            enforce: 'pre',
            resolveId: { filter: { id: DECLARED }, handler: () => MOUNTED },
            load: {
                filter: { id: new RegExp(`^${MOUNTED}$`, 'u') },
                handler: Effect.fnUntraced(
                    function* () {
                        const open = yield* readTypeScript(Option.none(), join(import.meta.dirname, 'node_modules'));
                        const source = yield* open({ root: 'indesign-uxp-types/flat', types: ['indesign-uxp-types'], lib: base.compilerOptions.lib, preamble: '', enumeration: Option.some('Enumeration') });
                        return `export default ${yield* Schema.encodeEffect(INLINED)(yield* source.model)}`;
                    },
                    Effect.scoped,
                    Effect.runPromise,
                ),
            },
        } satisfies Plugin,
    ],
});
