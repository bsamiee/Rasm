// k6 input artifact: executed by the k6 binary's own runtime, never imported by node — the Effect
// rail begins at the spawning Command and the decoded summary receipt.
import { check } from 'k6';
import { Trend } from 'k6/metrics';
import type { Options } from 'k6/options';

const probe = new Trend('probe_ms', true);

export const options: Options = {
    iterations: 8,
    thresholds: { probe_ms: ['p(95)<1000'] },
    vus: 2,
};

export default function (): void {
    const opened = Date.now();
    let sum = 0;
    for (let step = 0; step < 10_000; step += 1) {
        sum += step;
    }
    probe.add(Date.now() - opened);
    check(sum, { 'sum computed': (value) => value === 49_995_000 });
}
