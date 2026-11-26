import { App } from '@rasm/hosting/app';
import { ByteSize, Duration } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = new App({
    name: 'portfolio',
    env: {
        DB: App.Binding.sqlite(),
        BUCKET: App.Binding.bucket({ abort: Duration.weeks(1), expire: { production: {}, disposable: { '': Duration.weeks(1) } }, infrequentAccess: {}, cors: [{ origins: ['*'], methods: ['PUT', 'POST', 'GET', 'DELETE'], headers: ['Content-Type', 'If-None-Match'], exposed: ['ETag'], age: Duration.days(1) }] }),
    },
    allowed: ['b.samiee93@gmail.com'],
    targets: [App.Target.Cloudflare()],
    limits: { cpus: 1, memory: ByteSize.gibibytes(1) },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default hosting;
