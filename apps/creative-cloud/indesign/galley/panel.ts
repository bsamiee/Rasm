import { panel } from '@rasm/indesign/service/panel';
import { plugin } from '@rasm/uxp/entry';
import { Plugin } from '@rasm/uxp/runtime';
import { Effect, Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

plugin(
    Layer.unwrap(
        Effect.gen(function* () {
            const {
                domains: [origin],
            } = yield* Effect.fromOption((yield* Plugin).requiredPermissions.network);
            return panel({ peer: 'Atelier', origin });
        }),
    ),
    { commands: {}, panels: {} },
);
