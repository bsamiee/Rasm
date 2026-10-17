use core::{hint::black_box, mem::ManuallyDrop};

// --- [MODELS] --------------------------------------------------------------------------

struct Guard<F>(Option<F>)
where F: FnOnce();

impl<F> Drop for Guard<F>
where F: FnOnce()
{
    fn drop(&mut self) {
        if let Some(run) = self.0.take() {
            run();
        }
    }
}

struct Deferred<F>(F)
where F: FnMut();

impl<F> Drop for Deferred<F>
where F: FnMut()
{
    fn drop(&mut self) { (self.0)(); }
}

struct Draining<F>
where F: FnMut(u8) -> bool
{
    keep: F,
    items: Vec<u8>,
}

impl<F> Drop for Draining<F>
where F: FnMut(u8) -> bool
{
    fn drop(&mut self) { self.items.retain(|&item| (self.keep)(item)); }
}

struct Manual<F>(ManuallyDrop<F>)
where F: FnMut();

impl<F> Drop for Manual<F>
where F: FnMut()
{
    fn drop(&mut self) { (*self.0)(); }
}

struct Boxed(Box<dyn FnMut()>);

impl Drop for Boxed {
    fn drop(&mut self) { (self.0)(); }
}

struct Kept<F>(F)
where F: FnMut();

impl<F> Drop for Kept<F>
where F: FnMut()
{
    fn drop(&mut self) {}
}

enum Pending<F>
where F: FnMut()
{
    Run(F),
}

impl<F> Drop for Pending<F>
where F: FnMut()
{
    fn drop(&mut self) {
        let Self::Run(run) = self;
        run();
    }
}

struct Repeat<F>(F)
where F: FnMut() -> u8;

impl<F> Iterator for Repeat<F>
where F: FnMut() -> u8
{
    type Item = u8;

    fn next(&mut self) -> Option<u8> { Some((self.0)()) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    drop(Guard(Some(|| ())));
    drop(Deferred(|| ()));
    drop(Draining { keep: |item: u8| item > 1, items: vec![1, 2] });
    drop(Manual(ManuallyDrop::new(|| ())));
    drop(Boxed(Box::new(|| ())));
    drop(Kept(|| ()));
    drop(Pending::Run(|| ()));
    black_box(&(Repeat(|| 1).next(), Repeat(|| 2).next()));
}
