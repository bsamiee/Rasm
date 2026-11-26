import { App } from '@rasm/hosting/app';
import { ByteSize } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = new App({
    name: 'hooks',
    env: { POSTGRES_URL: App.Binding.postgres(), TAILSCALE_WEBHOOK_SECRET: App.Binding.secret() },
    allowed: [],
    targets: [],
    limits: { cpus: 1, memory: ByteSize.mebibytes(512) },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default hosting;
