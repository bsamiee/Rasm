import { ExecuteScript, HostError, Requests } from '@rasm/automation';
import { UxpManifest } from '@rasm/uxp';
import { RpcMiddleware } from 'effect/rpc';

// --- [SERVICES] ------------------------------------------------------------------------

class DisplayDialogs extends RpcMiddleware.Service<DisplayDialogs>()('@rasm/photoshop/DisplayDialogs', { error: HostError }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Photoshop = {
    identifier: 'com.adobe.Photoshop',
    Contract: Requests.middleware(DisplayDialogs).annotate(ExecuteScript.Awaits, true),
    DisplayDialogs,
    manifest: { host: { app: 'PS', minVersion: '27.0.0' }, requiredPermissions: { localFileSystem: 'fullAccess', allowCodeGenerationFromStrings: true } },
    target: UxpManifest.target,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Photoshop };
