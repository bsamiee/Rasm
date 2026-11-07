import { Button, Dialog, DialogTrigger, Heading, Modal } from 'react-aria-components';
import preview from '#/.storybook/preview.ts';
import { Backdrop } from '#/backdrop.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const meta = preview.meta({
    component: Backdrop,
    args: {
        isDismissable: true,
        children: (
            <Modal>
                <Dialog>
                    <Heading slot="title">Remove entry</Heading>
                </Dialog>
            </Modal>
        ),
    },
});

const Open = meta.story({ args: { defaultOpen: true } });
const Triggered = meta.story({
    render: (args) => (
        <DialogTrigger>
            <Button>Remove entry</Button>
            <Backdrop {...args} />
        </DialogTrigger>
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Open, Triggered };
