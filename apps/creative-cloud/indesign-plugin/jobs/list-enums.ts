// --- [IMPORTS] -------------------------------------------------------------------------

import { enumerations } from '@rasm/creative-cloud-server/indesign';
import type { Body, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Predicate, Record } from 'effect';
import { hosted, live } from '../host.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const listEnums = ({ name }: Body<'listEnums'>): Effect.Effect<Reply<'listEnums'>> =>
    Effect.map(Effect.all([hosted, live]), ([host, table]) => ({
        kind: 'enums',
        enums: Array.map(
            Record.toEntries(Option.match(name, { onNone: () => table, onSome: (wanted) => Record.filter(table, (_, enumeration) => enumeration === wanted) })),
            ([enumeration, constants]) => ({
                name: enumeration,
                constants: Array.map(Record.keys(constants), (constant) => ({ name: constant, value: Option.flatMap(Record.get(enumerations, enumeration), Record.get(constant)) })),
            }),
        ),
        functions: Record.keys(Record.filter(host, Predicate.isFunction)),
    }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { listEnums };
