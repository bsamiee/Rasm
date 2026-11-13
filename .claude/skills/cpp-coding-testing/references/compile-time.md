# [COMPILE_TIME]

## [01]-[BOTH_PHASES]

Behavior a constant expression decides is asserted once by the compiler and once on a run-time value:

```cpp
TEST_CASE("extent text of two nonzero sides parses to them", "[raster][extent]") {
    constexpr std::uint64_t limit{1'000'000};
    STATIC_REQUIRE(raster::extent_of("640x480", limit) == raster::Extent::make(640, 480));

    const std::string text{"640x480"};
    const auto parsed = raster::extent_of(text, limit);
    REQUIRE(parsed.has_value());
    CHECK(parsed->width() == 640U);
    CHECK(parsed->height() == 480U);
}
```

`STATIC_REQUIRE` fails its build on a wrong value and on undefined behavior inside that call. `text` holds a run-time value, so a second call executes where a sanitizer observes it.

## [02]-[COMPILER_FACTS]

Facts with no run-time form are `static_assert`s at namespace scope:

```cpp
static_assert(noexcept(raster::extent_of(std::string_view{}, std::uint64_t{})));
static_assert(std::same_as<decltype(raster::extent_of(std::string_view{}, std::uint64_t{})), std::expected<raster::Extent, raster::Refusal>>);
static_assert(std::is_trivially_copyable_v<raster::Extent>);
static_assert(!std::constructible_from<raster::Extent, std::uint32_t, std::uint32_t>);
```

Each argument has its parameter's own type. A literal in place of `std::string_view{}` adds a conversion libc++ declares without `noexcept`, and that assertion then states a fact about libc++.

## [03]-[PROBES]

Negative probes sit in a concept over a dependent type, each beside a positive probe differing in its refused property alone:

```cpp
struct Filled {
    std::uint8_t level;
    std::array<std::uint8_t, 3> unused;
    std::uint32_t weight;
};

struct Padded {
    std::uint8_t level;
    std::uint32_t weight;
};

template <typename Pixel>
concept ReadsBytes = requires(std::span<const Pixel> pixels) { raster::bytes_of(pixels); };

static_assert(ReadsBytes<Filled> && !ReadsBytes<Padded>);
static_assert(ReadsBytes<std::uint32_t> && !ReadsBytes<float>);
```

`bytes_of` accepts a type with one object representation per value. `Filled` and `Padded` differ in padding alone, `std::uint32_t` and `float` in representation alone, so a constraint on another property fails one pair. `requires { raster::Extent{1U, 2U}; }` outside a template is ill-formed in place of `false`.
