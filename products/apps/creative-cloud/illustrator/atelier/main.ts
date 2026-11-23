import { application, execute, render } from '@rasm/illustrator/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Effect } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

Plugin.extension({ application, execute, render, commands: { atelier: Effect.void } });
