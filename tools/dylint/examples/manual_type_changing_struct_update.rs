use core::{hint::black_box, marker::PhantomData};

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

impl Message<Draft> {
    fn send(self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: self.id } }

    fn merged(self, other: Self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: other.id } }
}

impl Message<Sent> {
    fn resend(self) -> Self { Self { state: PhantomData, body: self.body, id: self.id.strict_add(1) } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let sent = Message::<Draft> { state: PhantomData, body: String::from("hello"), id: 1 }.send().resend();
    let merged = Message::<Draft> { state: PhantomData, body: String::from("a"), id: 2 }.merged(Message { state: PhantomData, body: String::from("b"), id: 3 });
    black_box(&(sent.id, sent.body, merged.id, merged.body));
}
