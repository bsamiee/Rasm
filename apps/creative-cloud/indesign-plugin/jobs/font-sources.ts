// --- [IMPORTS] -------------------------------------------------------------------------

import { app, FontStatus } from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import { FontAxes, FontSource, type Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Schema, Struct } from 'effect';

// --- [HANDLER] -------------------------------------------------------------------------

const fontSources: () => Effect.Effect<Reply<'fontSources'>, HostRejection> = Effect.fnUntraced(function* () {
    const available = yield* Effect.try({ try: () => app.fonts.everyItem().getElements(), catch: thrown });
    const [rejected, sources] = yield* Effect.partition(available, (font, index) =>
        Effect.try({
            try: () => {
                if (!font.status.equals(FontStatus.INSTALLED)) {
                    return Option.none();
                }
                const { properties } = font;
                return Option.some({
                    ...Schema.decodeUnknownSync(FontSource)(properties),
                    axes: Array.isArrayNonEmpty(Array.intersection(Struct.keys(FontAxes.fields), Struct.keys(properties)))
                        ? Option.some(Schema.decodeUnknownSync(FontAxes)(properties))
                        : Option.none(),
                });
            },
            catch: (cause) => ({ index, reason: thrown(cause) }),
        }),
    );
    return { kind: 'fonts', fonts: Array.getSomes(sources), rejected };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { fontSources };
