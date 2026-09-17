// --- [IMPORTS] -------------------------------------------------------------------------

import { action } from 'adobe:photoshop';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { ListPresets, PRESET_CLASSES, Presets } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _READ = [
    {
        _obj: 'get',
        _target: [
            { _ref: 'property', _property: 'presetManager' },
            { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' },
        ],
    },
];

// --- [MODELS] --------------------------------------------------------------------------

const _store = Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ presetManager: Schema.Array(Schema.Struct({ _obj: Schema.String, name: Schema.Array(Schema.String) })) })]));

// --- [HANDLER] -------------------------------------------------------------------------

const listPresets: Handler = handler(ListPresets, Presets, ({ kind }) =>
    Effect.gen(function* () {
        const answer = yield* Effect.tryPromise({ try: () => action.batchPlay(_READ, {}), catch: thrown });
        const [store] = yield* Effect.mapError(_store(answer), (cause) => HostRejection.cases.resultNotJson.make({ cause }));
        const [group, groupIndex] = yield* Effect.fromOption(
            Array.findFirstWithIndex(store.presetManager, (row) => row._obj === PRESET_CLASSES[kind]),
            () => HostRejection.cases.resultNotJson.make({ cause: `presetManager holds no ${PRESET_CLASSES[kind]} group` }),
        );
        return { kind: 'presets' as const, groupIndex, names: group.name, count: group.name.length };
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { listPresets };
