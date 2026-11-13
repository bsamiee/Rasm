# [CASES]

## [01]-[UNITS]

Test units include framework headers, import the module under test, and register each enumeration their assertions compare:

```cpp
#include <catch2/catch_template_test_macros.hpp>
#include <catch2/catch_test_macros.hpp>
#include <catch2/generators/catch_generators.hpp>
#include <catch2/matchers/catch_matchers_range_equals.hpp>

import std;
import raster;

CATCH_REGISTER_ENUM(raster::Refusal, raster::Refusal::Empty, raster::Refusal::Oversized, raster::Refusal::Malformed)
```

## [02]-[REJECT_DIRECTION]

Inputs a contract excludes are rows of one table, each with the reason it documents:

```cpp
TEST_CASE("extent text outside its grammar or bounds is refused with its reason", "[raster][extent]") {
    const auto [text, refusal] = GENERATE(table<std::string_view, raster::Refusal>({
        {"", raster::Refusal::Malformed},
        {"640", raster::Refusal::Malformed},
        {"640x", raster::Refusal::Malformed},
        {"x480", raster::Refusal::Malformed},
        {"640x480x3", raster::Refusal::Malformed},
        {"640x480 ", raster::Refusal::Malformed},
        {"+640x480", raster::Refusal::Malformed},
        {"-640x480", raster::Refusal::Malformed},
        {"4294967296x1", raster::Refusal::Malformed},
        {"0x480", raster::Refusal::Empty},
        {"640x0", raster::Refusal::Empty},
        {"1001x1000", raster::Refusal::Oversized},
    }));
    CAPTURE(text);

    const auto parsed = raster::extent_of(text, 1'000'000);

    REQUIRE_FALSE(parsed.has_value());
    CHECK(parsed.error() == refusal);
}
```

`extent_of` documents its text as decimal `width`, an `x`, and decimal `height`, both nonzero, their product within `limit`. A row with a wrong reason fails as `Empty == Malformed` with `text := "0x480"`.

## [03]-[BOUNDS]

Accepted inputs include each bound a contract states:

```cpp
TEST_CASE("extent text at its bounds parses", "[raster][extent]") {
    const auto [text, cells] = GENERATE(table<std::string_view, std::uint64_t>({
        {"1x1", 1},
        {"1000x1000", 1'000'000},
        {"4294967295x1", 4'294'967'295},
        {"0640x480", 307'200},
    }));
    CAPTURE(text);

    CHECK(raster::extent_of(text, 4'294'967'295).transform(&raster::Extent::cells) == cells);
}
```

## [04]-[TYPE_CLASSES]

Templates run one body per class of type argument their contract separates:

```cpp
TEMPLATE_TEST_CASE("pixel bytes span every byte of every pixel", "[raster][bytes]", std::uint8_t, std::uint16_t, raster::Rgb) {
    const std::array<TestType, 3> pixels{};

    CHECK(raster::bytes_of(std::span<const TestType>{pixels}).size() == 3 * sizeof(TestType));
    CHECK(raster::bytes_of(std::span<const TestType>{}).empty());
}
```

Classes `bytes_of` refuses (padded, floating) are probes of `references/compile-time.md`.

## [05]-[WHOLE_VALUES]

Collections compare whole through a matcher:

```cpp
TEST_CASE("pixel bytes follow pixel order", "[raster][bytes]") {
    const std::array<raster::Rgb, 2> pixels{{{.red = 1, .green = 2, .blue = 3}, {.red = 4, .green = 5, .blue = 6}}};

    const auto bytes = raster::bytes_of(std::span<const raster::Rgb>{pixels}) | std::views::transform([](std::byte byte) { return std::to_integer<int>(byte); });

    CHECK_THAT(bytes, Catch::Matchers::RangeEquals(std::array{1, 2, 3, 4, 5, 6}));
}
```
