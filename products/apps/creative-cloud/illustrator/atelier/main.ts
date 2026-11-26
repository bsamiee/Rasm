import { Description } from '@rasm/applications/source';
import { application, execute, render, toolbar } from '@rasm/illustrator/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Duration, Effect, Stream } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application, ...Plugin.scripting({ execute, render }), source: Description.tools({ ...toolbar, changes: Stream.tick(Duration.seconds(1)) }) }), commands: { atelier: Effect.void } });
