import { Manifested } from '@rasm/nx-devkit/executor';
import { printed } from '@rasm/nx-devkit/process';
import { Effect } from 'effect';
import { SHORT_VERSION } from '#/targets';

// --- [SERVICES] ------------------------------------------------------------------------

class PlistVersionActions extends Manifested {
    override stated(text: string) {
        return Effect.map(printed('plutil', ['-extract', SHORT_VERSION, 'raw', '-o', '-', '-'], { input: text }), (version) => version.trim());
    }

    override restated(text: string, version: string) {
        return printed('plutil', ['-replace', SHORT_VERSION, '-string', version, '-o', '-', '-'], { input: text });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default PlistVersionActions;
