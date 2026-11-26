import { execute, render, toolbar } from '@rasm/aftereffects/uxp';
import { Description } from '@rasm/applications/source';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Duration, Stream } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.serve({ link: Plugin.dial({ application: Uxp.application, ...Plugin.scripting({ execute, render }), source: Description.tools({ ...toolbar, changes: Stream.tick(Duration.seconds(1)) }) }) });
