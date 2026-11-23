import { Button, type ButtonProps, Dialog, type DialogProps, DialogTrigger, Heading, Modal, ModalOverlay, type ModalOverlayProps, Text } from 'react-aria-components';
import { flushSync } from 'react-dom';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ModalDialog = Object.assign(
    ({ modal, dialog, children, ...overlay }: Omit<ModalOverlayProps, 'children'> & Pick<DialogProps, 'children'> & { readonly modal?: Omit<ModalOverlayProps, 'children'>; readonly dialog?: Omit<DialogProps, 'children'> }) => (
        <ModalOverlay {...overlay}>
            <Modal {...modal}>
                <Dialog {...dialog}>{children}</Dialog>
            </Modal>
        </ModalOverlay>
    ),
    {
        Confirm: ({ label, description, cancel, onConfirm, isDisabled = false, trigger }: { readonly label: string; readonly description: string; readonly cancel: string; readonly onConfirm: () => void; readonly isDisabled?: boolean; readonly trigger?: Omit<ButtonProps, 'children' | 'isDisabled'> }) => (
            <DialogTrigger>
                <Button {...trigger} isDisabled={isDisabled}>
                    {label}
                </Button>
                <ModalDialog dialog={{ role: 'alertdialog' }}>
                    {({ close }) => (
                        <>
                            <Heading slot="title">{label}</Heading>
                            <Text elementType="p" slot="description">
                                {description}
                            </Text>
                            <div>
                                <Button autoFocus slot="close">
                                    {cancel}
                                </Button>
                                <Button
                                    onPress={() => {
                                        flushSync(close);
                                        onConfirm();
                                    }}
                                >
                                    {label}
                                </Button>
                            </div>
                        </>
                    )}
                </ModalDialog>
            </DialogTrigger>
        ),
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalDialog };
