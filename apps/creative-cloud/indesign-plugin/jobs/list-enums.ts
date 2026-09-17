// --- [IMPORTS] -------------------------------------------------------------------------

import { type Handler, handler } from '@rasm/creative-cloud-server/client';
import { enumerations } from '@rasm/creative-cloud-server/indesign';
import { Enums, ListEnums } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Record } from 'effect';
import type { Live } from '../enums.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const listEnums = (live: Live): Handler =>
    handler(ListEnums, Enums, ({ name }) =>
        Effect.succeed({
            kind: 'enums' as const,
            enums: Array.map(
                Record.toEntries(Option.match(name, { onNone: () => live, onSome: (wanted) => Record.filter(live, (_, enumeration) => enumeration === wanted) })),
                ([enumeration, constants]) => ({
                    name: enumeration,
                    constants: Array.map(Record.keys(constants), (constant) => ({ name: constant, value: Option.flatMap(Record.get(enumerations, enumeration), Record.get(constant)) })),
                }),
            ),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { listEnums };
