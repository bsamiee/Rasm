import { join } from 'node:path';
import { Declaration } from '@rasm/object-model';
import { declared } from '@rasm/object-model/node';
import { uxp } from '@rasm/uxp/vite';
import { Effect, Option, Schema } from 'effect';
import type { Plugin } from 'vite';
import { ATELIER } from '#/tools';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECLARED = /^indesign-uxp-types\/flat\?inline$/u;
const MOUNTED = '\0declared';
const INLINED = Schema.fromJsonString(Schema.fromJsonString(Declaration.Model));

// --- [COMPOSITION] ---------------------------------------------------------------------

export default uxp(import.meta.dirname, {
    masters: { plugin: join(import.meta.dirname, '../../multi-app/atelier/macos/Resources/AppIcon.icon/Assets/Atelier.svg'), panels: { typesetting: join(import.meta.dirname, 'typesetting.svg') } },
    network: [ATELIER.origin],
    config: {
        plugins: [
            {
                name: 'declared',
                enforce: 'pre',
                resolveId: { filter: { id: DECLARED }, handler: () => MOUNTED },
                load: {
                    filter: { id: new RegExp(`^${MOUNTED}$`, 'u') },
                    handler: Effect.fnUntraced(
                        function* () {
                            const open = yield* declared(Option.none(), join(import.meta.dirname, 'node_modules'));
                            const source = yield* open({ root: 'indesign-uxp-types/flat', types: ['indesign-uxp-types'], preamble: '', enumeration: Option.some('Enumeration') });
                            return `export default ${yield* Schema.encodeEffect(INLINED)(yield* source.model)}`;
                        },
                        Effect.scoped,
                        Effect.runPromise,
                    ),
                },
            } satisfies Plugin,
        ],
    },
});
