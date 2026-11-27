import { events, execute, Premiere, render } from '@rasm/premierepro/uxp';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Effect, Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application: Uxp.application, ...Plugin.scripting({ execute, render }), events, dialogs: Effect.void }).pipe(Layer.provide(Premiere.layer)) });
