import { Template } from '@rasm/hosting/template';
import { crossed } from '@rasm/testing/errors';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';

describe('HostingError', () => {
    it('should keep the message the template refusal derives across structured clone and Nx serialization', async () => {
        const refused = await Effect.runPromise(Effect.flip(Effect.provide(Effect.flatten(Template), Template.layer({ fetch: () => Effect.runPromise(Effect.succeed(new Response(null, { status: 503 }))) }))));

        expect(crossed(refused)).toStrictEqual(['Template: answer 503', 'Template: answer 503']);
    });
});
