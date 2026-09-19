// --- [IMPORTS] -------------------------------------------------------------------------

import { enumerations } from '@rasm/creative-cloud-server/indesign';
import type { Body, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Predicate, Record } from 'effect';
import { live } from '../host.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const listEnums: (body: Body<'listEnums'>) => Effect.Effect<Reply<'listEnums'>> = Effect.fnUntraced(function* ({ name }: Body<'listEnums'>) {
    const { host, constants } = yield* live;
    return {
        kind: 'enums',
        enums: Array.map(
            Record.toEntries(Option.match(name, { onNone: () => constants, onSome: (wanted) => Record.filter(constants, (_, enumeration) => enumeration === wanted) })),
            ([enumeration, values]) => ({
                name: enumeration,
                constants: Array.map(Record.keys(values), (constant) => ({
                    name: constant,
                    value: Option.flatMap(Record.get<string, Readonly<Record<string, number>>>(enumerations, enumeration), Record.get(constant)),
                })),
            }),
        ),
        functions: Record.keys(Record.filter(host, Predicate.isFunction)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { listEnums };
