use core::hint::black_box;
use core::marker::PhantomData;
use core::ops::Deref;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Draft;

#[derive(Debug)]
struct Sent;

#[derive(Debug)]
struct Message<State> {
    state: PhantomData<State>,
    body: String,
    id: u8,
}

impl<State> Deref for Message<State> {
    type Target = (u8, u8, u8);

    fn deref(&self) -> &Self::Target { &(0, 0, 9) }
}

impl Message<Draft> {
    fn send(self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: self.id } }

    fn merged(self, other: Self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: other.id } }

    fn projected(self) -> Message<Sent> { Message { state: PhantomData, id: self.2, body: self.body } }

    fn reordered(mut self) -> Message<Sent> {
        Message {
            state: PhantomData,
            id: self.id,
            body: {
                self.id = 0;
                self.body
            },
        }
    }
}

impl Message<Sent> {
    fn resend(self) -> Self { Self { state: PhantomData, body: self.body, id: self.id.strict_add(1) } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let sent = Message::<Draft> { state: PhantomData, body: String::from("hello"), id: 1 }.send().resend();
    let merged = Message::<Draft> { state: PhantomData, body: String::from("a"), id: 2 }.merged(Message { state: PhantomData, body: String::from("b"), id: 3 });
    let projected = Message::<Draft> { state: PhantomData, body: String::from("c"), id: 4 }.projected();
    let reordered = Message::<Draft> { state: PhantomData, body: String::from("d"), id: 5 }.reordered();
    black_box(&(sent.id, sent.body, merged.id, merged.body, projected.id, projected.body, reordered.id, reordered.body));
}
