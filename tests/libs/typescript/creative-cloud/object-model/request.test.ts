import { Evaluation, Failure, HostError } from '@rasm/object-model';
import { Cause, Effect, ErrorReporter, type LogLevel, Predicate, Schema } from 'effect';
import { describe, expect, it } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const severities = <E>(failing: Effect.Effect<never, E>) => {
    const seen: (LogLevel.Severity | undefined)[] = [];
    const reporter = ErrorReporter.make(({ error }) => {
        seen.push(Predicate.hasProperty(error, ErrorReporter.severity) ? ErrorReporter.getSeverity(error) : undefined);
    });
    return Effect.runPromise(Effect.as(Effect.exit(Effect.provide(Effect.withErrorReporting(failing), ErrorReporter.layer([reporter]))), seen));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('request failure', () => {
    it('reports a host refusal answered to its caller at Info', async () => {
        await expect(severities(Effect.fail(HostError.refused('strokeWeight')(new Error('Expected Unit'))))).resolves.toStrictEqual(['Info']);
    });

    it('reports a call the host could not serve at Warn', async () => {
        await expect(severities(Effect.fail(HostError.of({ _tag: 'Unavailable' }, 'modal scope')()))).resolves.toStrictEqual(['Warn']);
    });

    it('reports an unmatched lookup answered to its caller at Info', async () => {
        const unmatched = Schema.decodeEffect(Schema.toCodecJson(Failure))({ _tag: 'Unmatched', subject: 'document', candidates: ['cover.psd'], message: 'No document, candidates: cover.psd' });
        await expect(severities(Effect.flatMap(unmatched, Effect.fail))).resolves.toStrictEqual(['Info']);
    });

    it('leaves an error no request declares without a severity of its own', async () => {
        await expect(severities(Effect.fail(new Cause.NoSuchElementError('plain')))).resolves.toStrictEqual([undefined]);
    });
});

describe('evaluation label', () => {
    it.each([
        { body: 'return 7;', label: 'Run return 7;' },
        { body: '\n   \n  const first = 1;\nreturn first;', label: 'Run const first = 1;' },
        { body: ' \n', label: 'Run script' },
    ])('names the undo step of body $body as $label', ({ body, label }) => {
        expect(Evaluation.labelled(body)).toBe(label);
    });
});
