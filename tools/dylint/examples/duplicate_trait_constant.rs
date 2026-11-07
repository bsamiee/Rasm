use core::hint::black_box;

// --- [TYPES] ---------------------------------------------------------------------------

trait Arity {
    const ARITY: usize;
}

trait Width {
    const WIDTH: u32;
}

trait Depth {
    const DEPTH: u8 = 1;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RETRIES: u32 = 3;
const WORKERS: u32 = 3;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Add;

#[derive(Debug)]
struct Sub;

#[derive(Debug)]
struct Narrow;

#[derive(Debug)]
struct Wide;

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Arity for Add {
    const ARITY: usize = 2;
}

impl Arity for Sub {
    const ARITY: usize = 2;
}

impl Width for Narrow {
    const WIDTH: u32 = 8;
}

impl Width for Wide {
    const WIDTH: u32 = 16;
}

impl Depth for Narrow {
    const DEPTH: u8 = 4;
}

impl Depth for Wide {
    const DEPTH: u8 = 4;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    black_box(&(
        [Add::ARITY, Sub::ARITY],
        [Narrow::WIDTH, Wide::WIDTH, RETRIES, WORKERS],
        [Narrow::DEPTH, Wide::DEPTH],
        associated_types::values(),
        generic_types::values(),
        private_paths::values(),
        self_dependency::values(),
        generic_dependency::values(),
        (declaration_references::values(), lifetime_relationships::values()),
    ));
}

mod associated_types {

    // --- [TYPES]

    trait Meta {
        type Value;
        const VALUE: Self::Value;
    }

    // --- [OPERATIONS]

    impl Meta for super::Narrow {
        type Value = u8;
        const VALUE: Self::Value = 1;
    }

    impl Meta for super::Wide {
        type Value = u16;
        const VALUE: Self::Value = 1;
    }

    pub(super) fn values() -> (u8, u16) { (<super::Narrow as Meta>::VALUE, <super::Wide as Meta>::VALUE) }
}

mod generic_types {

    // --- [TYPES]

    trait Meta<T> {
        const VALUE: T;
    }

    // --- [OPERATIONS]

    impl Meta<u8> for super::Narrow {
        const VALUE: u8 = 1;
    }

    impl Meta<u16> for super::Wide {
        const VALUE: u16 = 1;
    }

    pub(super) fn values() -> (u8, u16) { (<super::Narrow as Meta<u8>>::VALUE, <super::Wide as Meta<u16>>::VALUE) }
}

mod private_paths {

    // --- [TYPES]

    trait Meta {
        const VALUE: u32;
    }

    // --- [OPERATIONS]

    pub(super) fn values() -> [u32; 2] { [<super::Narrow as Meta>::VALUE, <super::Wide as Meta>::VALUE] }

    // --- [COMPOSITION]

    mod first {

        // --- [CONSTANTS]

        const LIMIT: u32 = 5;

        // --- [OPERATIONS]

        impl super::Meta for super::super::Narrow {
            const VALUE: u32 = LIMIT;
        }
    }

    mod last {

        // --- [CONSTANTS]

        const LIMIT: u32 = 5;

        // --- [OPERATIONS]

        impl super::Meta for super::super::Wide {
            const VALUE: u32 = LIMIT;
        }
    }
}

mod self_dependency {

    // --- [TYPES]

    trait Here {
        const VALUE: &'static str;
    }

    trait Other {
        const NAME: &'static str;
    }

    // --- [OPERATIONS]

    impl Other for super::Narrow {
        const NAME: &'static str = "shared";
    }

    impl Other for super::Wide {
        const NAME: &'static str = "shared";
    }

    impl Here for super::Narrow {
        const VALUE: &'static str = <Self as Other>::NAME;
    }

    impl Here for super::Wide {
        const VALUE: &'static str = <Self as Other>::NAME;
    }

    pub(super) fn values() -> [&'static str; 2] { [<super::Narrow as Here>::VALUE, <super::Wide as Here>::VALUE] }
}

mod generic_dependency {
    use core::marker::PhantomData;

    // --- [TYPES]

    trait Named {
        const NAME: &'static str;
    }

    trait Here {
        const VALUE: &'static str;
    }

    // --- [MODELS]

    #[derive(Debug)]
    struct Wrapper<T>(PhantomData<T>);

    #[derive(Debug)]
    struct Other<T>(PhantomData<T>);

    // --- [OPERATIONS]

    impl Named for u8 {
        const NAME: &'static str = "byte";
    }

    impl Named for u16 {
        const NAME: &'static str = "word";
    }

    impl<T: Named> Here for Wrapper<T> {
        const VALUE: &'static str = T::NAME;
    }

    impl<T: Named> Here for Other<T> {
        const VALUE: &'static str = T::NAME;
    }

    pub(super) fn values() -> [&'static str; 2] { [<Wrapper<u8> as Here>::VALUE, <Other<u16> as Here>::VALUE] }
}

mod declaration_references {

    // --- [TYPES]

    trait Own {
        const NAME: &'static str;
        const VALUE: &'static str;
    }

    trait Base<T> {
        const NAME: &'static str;
    }

    trait Inherited<T>: Base<T> {
        const VALUE: &'static str;
    }

    trait Wrong: Base<u8> {
        const VALUE: &'static str;
    }

    trait Named {
        const NAME: &'static str;
    }

    trait Generic<T: Named> {
        const VALUE: &'static str;
    }

    // --- [OPERATIONS]

    impl Own for super::Narrow {
        const NAME: &'static str = "narrow";
        const VALUE: &'static str = <Self as Own>::NAME;
    }

    impl Own for super::Wide {
        const NAME: &'static str = "wide";
        const VALUE: &'static str = <Self as Own>::NAME;
    }

    impl Base<u8> for super::Narrow {
        const NAME: &'static str = "narrow-byte";
    }

    impl Base<u8> for super::Wide {
        const NAME: &'static str = "wide-byte";
    }

    impl Base<u16> for super::Narrow {
        const NAME: &'static str = "narrow-word";
    }

    impl Base<u16> for super::Wide {
        const NAME: &'static str = "wide-word";
    }

    impl Inherited<u8> for super::Narrow {
        const VALUE: &'static str = <Self as Base<u8>>::NAME;
    }

    impl Inherited<u8> for super::Wide {
        const VALUE: &'static str = <Self as Base<u8>>::NAME;
    }

    impl Wrong for super::Narrow {
        const VALUE: &'static str = <Self as Base<u16>>::NAME;
    }

    impl Wrong for super::Wide {
        const VALUE: &'static str = <Self as Base<u16>>::NAME;
    }

    impl Named for u8 {
        const NAME: &'static str = "named-byte";
    }

    impl Named for u16 {
        const NAME: &'static str = "named-word";
    }

    impl<T: Named> Generic<T> for super::Narrow {
        const VALUE: &'static str = T::NAME;
    }

    impl<T: Named> Generic<T> for super::Wide {
        const VALUE: &'static str = T::NAME;
    }

    pub(super) fn values() -> [&'static str; 8] {
        [
            <super::Narrow as Own>::VALUE,
            <super::Wide as Own>::VALUE,
            <super::Narrow as Inherited<u8>>::VALUE,
            <super::Wide as Inherited<u8>>::VALUE,
            <super::Narrow as Wrong>::VALUE,
            <super::Wide as Wrong>::VALUE,
            <super::Narrow as Generic<u8>>::VALUE,
            <super::Wide as Generic<u16>>::VALUE,
        ]
    }
}

mod lifetime_relationships {

    // --- [TYPES]

    trait Source<'a> {
        const NAME: &'a str;
    }

    trait Destination<'a, 'b>: Source<'a> {
        const VALUE: &'b str;
    }

    trait Constrained<'a: 'b, 'b>: Source<'a> {
        const VALUE: &'b str;
    }

    trait StaticSource {
        const NAME: &'static str;
    }

    trait Shortened<'a>: StaticSource {
        const VALUE: &'a str;
    }

    // --- [OPERATIONS]

    impl<'a> Source<'a> for super::Narrow {
        const NAME: &'a str = "narrow";
    }

    impl<'a> Source<'a> for super::Wide {
        const NAME: &'a str = "wide";
    }

    impl<'a> Destination<'a, 'a> for super::Narrow {
        const VALUE: &'a str = <Self as Source<'a>>::NAME;
    }

    impl<'a> Destination<'a, 'a> for super::Wide {
        const VALUE: &'a str = <Self as Source<'a>>::NAME;
    }

    impl<'a> Constrained<'a, 'a> for super::Narrow {
        const VALUE: &'a str = <Self as Source<'a>>::NAME;
    }

    impl<'a> Constrained<'a, 'a> for super::Wide {
        const VALUE: &'a str = <Self as Source<'a>>::NAME;
    }

    impl StaticSource for super::Narrow {
        const NAME: &'static str = "narrow-static";
    }

    impl StaticSource for super::Wide {
        const NAME: &'static str = "wide-static";
    }

    impl<'a> Shortened<'a> for super::Narrow {
        const VALUE: &'a str = <Self as StaticSource>::NAME;
    }

    impl<'a> Shortened<'a> for super::Wide {
        const VALUE: &'a str = <Self as StaticSource>::NAME;
    }

    pub(super) fn values() -> [&'static str; 6] {
        [
            <super::Narrow as Destination<'static, 'static>>::VALUE,
            <super::Wide as Destination<'static, 'static>>::VALUE,
            <super::Narrow as Constrained<'static, 'static>>::VALUE,
            <super::Wide as Constrained<'static, 'static>>::VALUE,
            <super::Narrow as Shortened<'static>>::VALUE,
            <super::Wide as Shortened<'static>>::VALUE,
        ]
    }
}
