// --- [IMPORTS] -------------------------------------------------------------------------

import { enumerations } from '@rasm/creative-cloud-server/indesign';
import { Enums, ListEnums } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Record } from 'effect';
import type { Live } from '../enums.ts';
import { type Handler, handler } from './handler.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const listEnums = (live: Live): Handler =>
    handler(ListEnums, Enums, ({ name }) =>
        Effect.succeed({
            kind: 'enums' as const,
            enums: Array.map(
                Array.filter(Record.toEntries(live), ([enumeration]) => Option.match(name, { onNone: () => true, onSome: (wanted) => wanted === enumeration })),
                ([enumeration, constants]) => ({
                    name: enumeration,
                    constants: Array.map(Record.keys(constants), (constant) => ({ name: constant, value: Option.flatMap(Record.get(enumerations, enumeration), Record.get(constant)) })),
                }),
            ),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { listEnums };
