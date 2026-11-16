import { App } from '@rasm/hosting';
import { Identity } from '@rasm/hosting/identity';
import { ByteSize } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = new App({
    name: 'hooks',
    env: { POSTGRES_URL: App.Binding.postgres() },
    allowed: [],
    identity: Identity.sites,
    targets: [],
    limits: { cpus: 1, memory: ByteSize.mebibytes(512) },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default hosting;
