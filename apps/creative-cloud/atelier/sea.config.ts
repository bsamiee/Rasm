import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { helper } from '@rasm/applications/vite';
import { bundle } from '@rasm/automation/vite';
import { ExtensionManifest } from '@rasm/cep';
import { script } from '@rasm/cep/vite';
import { Effect, Schema } from 'effect';
import { NetAddress } from 'effect/net';
import type { UserConfigFnPromise } from 'vite';
import { atelierLink } from '#/atelier-link';
import { atelierBuild } from '#/plugin.config';
import { XcodeProject } from '../../../eng/typescript/manifest.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeJsonString = Schema.encodeEffect(Schema.fromJsonString(Schema.String));

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Effect.fnUntraced(
    function* () {
        const directory = import.meta.dirname;
        const [code, signing] = yield* Effect.all([script(createRequire(join(directory, 'package.json')).resolve('@rasm/extendscript'), { name: 'runtime', target: ExtensionManifest.target }), XcodeProject.signing(join(directory, 'macos', 'Atelier.xcodeproj'))], { concurrency: 2 });
        return bundle({
            ...atelierBuild(directory).config,
            define: { 'import.meta.env.EXTENDSCRIPT_RUNTIME': yield* encodeJsonString(code) },
            plugins: [helper(join(directory, 'main.ts'), signing, 'server', { KeepAlive: true, EnvironmentVariables: { MCP_LISTEN: NetAddress.formatInet(atelierLink.listen) } })],
        });
    },
    Effect.provide(layer),
    Effect.runPromise,
) satisfies UserConfigFnPromise;
