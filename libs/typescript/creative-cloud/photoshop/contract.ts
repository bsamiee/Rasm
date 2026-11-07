import { HostError } from '@rasm/automation/error';
import { ExecuteScript, Requests } from '@rasm/automation/requests';
import { UxpManifest } from '@rasm/uxp/manifest';
import { RpcMiddleware } from 'effect/rpc';

// --- [SERVICES] ------------------------------------------------------------------------

class DisplayDialogs extends RpcMiddleware.Service<DisplayDialogs>()('@rasm/photoshop/contract/DisplayDialogs', { error: HostError }) {}

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
