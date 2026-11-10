import { App } from '@rasm/hosting/app';
import { ByteSize, Duration } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWNER = 'b.samiee93@gmail.com';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default new App({
    name: 'portfolio',
    env: { DB: App.Binding.sqlite(), BUCKET: App.Binding.bucket({ abort: Duration.weeks(1), expire: { production: {}, disposable: { '': Duration.weeks(1) } }, infrequentAccess: {}, cors: [] }), OWNER_EMAIL: App.Binding.text({ value: OWNER }) },
    allowed: [OWNER],
    limits: { cpus: 1, memory: ByteSize.gibibytes(1) },
});
