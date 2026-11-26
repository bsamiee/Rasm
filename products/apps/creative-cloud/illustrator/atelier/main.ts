import { Description } from '@rasm/applications/source';
import { application, execute, Illustrator, render, suppressDialogs, toolbar } from '@rasm/illustrator/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Duration, Effect, Layer, Stream } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application, ...Plugin.scripting({ execute, render }), source: Description.tools({ ...toolbar, changes: Stream.tick(Duration.seconds(1)) }), dialogs: suppressDialogs }).pipe(Layer.provide(Illustrator.layer)), commands: { atelier: Effect.void } });
