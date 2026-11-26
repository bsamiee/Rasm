import { SourceRpcs } from '@rasm/applications/source';
import { application, describe, events, execute, operate, render } from '@rasm/illustrator/uxp';
import { Plugin } from '@rasm/uxp/plugin';
import { Effect, Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { group, toolkit, handlers } = Plugin.scripting({ execute, render });

Plugin.serve({ link: Plugin.dial({ application, group: group.merge(SourceRpcs), toolkit, handlers: Layer.merge(handlers, SourceRpcs.toLayer({ describe, operate })), events }), commands: { atelier: Effect.void } });
