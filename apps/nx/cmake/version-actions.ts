import { Manifested } from '@rasm/nx-devkit/executor';
import { Effect } from 'effect';
import { restated, versioned } from '#/workspace';

// --- [SERVICES] ------------------------------------------------------------------------

class CmakeVersionActions extends Manifested {
    override stated(text: string, at: string) {
        return Effect.map(versioned(text, at), ({ version }) => version);
    }

    override restated(text: string, version: string, at: string) {
        return restated(text, version, at);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default CmakeVersionActions;
