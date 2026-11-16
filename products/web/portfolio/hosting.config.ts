import { App } from '@rasm/hosting';
import { Identity } from '@rasm/hosting/identity';
import { ByteSize, Duration } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = new App({
    name: 'portfolio',
    env: { DB: App.Binding.sqlite(), BUCKET: App.Binding.bucket({ uploads: '.uploads/', abort: Duration.weeks(1), expire: { production: {}, disposable: { '': Duration.weeks(1) } }, infrequentAccess: {}, cors: [] }) },
    allowed: ['b.samiee93@gmail.com'],
    identity: Identity.sites,
    targets: [App.Target.Cloudflare(), App.Target.Sites({ project: 'appgprj_6ac55bbed6dc81918f6bb52a4f324fbc' })],
    limits: { cpus: 1, memory: ByteSize.gibibytes(1) },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default hosting;
