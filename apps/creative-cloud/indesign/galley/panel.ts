import { panel } from '@rasm/indesign/service/panel';
import { plugin } from '@rasm/uxp/entry';
import { Plugin } from '@rasm/uxp/runtime';
import { Effect, Layer } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

plugin(Layer.unwrap(Plugin.use(({ requiredPermissions }) => Effect.map(Effect.fromOption(requiredPermissions.network), ({ domains: [origin] }) => panel(origin)))), { commands: {}, panels: {} });
