import { AfterEffects, execute, render, suppressDialogs, toolbar } from '@rasm/aftereffects/uxp';
import { Description } from '@rasm/applications/source';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Duration, Layer, Stream } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application: Uxp.application, ...Plugin.scripting({ execute, render }), source: Description.tools({ ...toolbar, changes: Stream.tick(Duration.seconds(1)) }), dialogs: suppressDialogs }).pipe(Layer.provide(AfterEffects.layer)) });
