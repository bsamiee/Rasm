# [COMPILE_TIME]

## [01]-[TWINS]

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

`STATIC_REQUIRE` fails the build on a wrong value and on undefined behavior inside the call. `text` holds a run-time value, and the second call executes where a sanitizer observes it.

## [02]-[COMPILER_FACTS]

Facts with no run-time form are `static_assert`s at namespace scope:

```cpp
static_assert(noexcept(raster::extent_of(std::string_view{}, 1)));
static_assert(std::same_as<decltype(raster::extent_of(std::string_view{}, 1)), std::expected<raster::Extent, raster::Refusal>>);
static_assert(std::is_trivially_copyable_v<raster::Extent>);
static_assert(!std::constructible_from<raster::Extent, std::uint32_t, std::uint32_t>);
```

Arguments have the parameter's own type: `noexcept(raster::extent_of("1x1", 1))` is `false`, the conversion from a literal to `std::string_view` being part of the expression.

## [03]-[PROBES]

Negative probes sit in a concept over a dependent type, each beside a positive probe differing in the refused property alone:

```cpp
struct Padded {
    std::uint8_t level;
    std::uint32_t weight;
};

template <typename Pixel>
concept ReadsBytes = requires(std::span<const Pixel> pixels) { raster::bytes_of(pixels); };

static_assert(ReadsBytes<std::uint32_t>);
static_assert(!ReadsBytes<Padded>);
static_assert(!ReadsBytes<float>);
```

`bytes_of` accepts a pixel type with no padding and unique object representations. `requires { raster::Extent{1U, 2U}; }` outside a template is ill-formed in place of `false`.

## [04]-[REJECTIONS]

Refusals a `requires` expression reads as valid take a source compiled once per guard:

```cpp
import std;
import raster;

[[nodiscard]] auto accepted() -> std::uint32_t { return raster::tint("ff8800"); }

#ifdef REJECT_SHORT_LITERAL
[[nodiscard]] auto rejected() -> std::uint32_t { return raster::tint("ff880"); }
#endif
```

`tint` refuses a literal that is not six hexadecimal digits through a `consteval` constructor. Sources compile with no guard defined and fail with one defined, at the guarded statement, with the diagnostic naming `refused`.
