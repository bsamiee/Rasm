// The seeded breach twin of probe.k6.ts: an unmeetable floor proves the threshold gate can fail.
import { Trend } from 'k6/metrics';
import type { Options } from 'k6/options';

const probe = new Trend('probe_ms', true);

export const options: Options = {
    iterations: 2,
    thresholds: { probe_ms: [{ abortOnFail: true, threshold: 'p(95)<0' }] },
    vus: 1,
};

export default function (): void {
    const opened = Date.now();
    let sum = 0;
    for (let step = 0; step < 10_000; step += 1) {
        sum += step;
    }
    probe.add(Math.max(1, Date.now() - opened));
    void sum;
}
