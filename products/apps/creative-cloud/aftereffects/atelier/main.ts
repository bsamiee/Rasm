import { describe, events, execute, operate, render } from '@rasm/aftereffects/uxp';
import { SourceRpcs } from '@rasm/applications/source';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const { group, toolkit, handlers } = Plugin.scripting({ execute, render });

Plugin.serve({ link: Plugin.dial({ application: Uxp.application, group: group.merge(SourceRpcs), toolkit, handlers: Layer.merge(handlers, SourceRpcs.toLayer({ describe, operate })), events }) });
