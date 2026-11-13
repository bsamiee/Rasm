# [PROPERTIES]

## [01]-[ROUND_TRIPS]

Properties visit each bound of their domain by name, then draws from an engine seeded by a literal:

```cpp
TEST_CASE("extent text formatted from two sides parses back to them", "[raster][extent]") {
    constexpr auto most = std::numeric_limits<std::uint32_t>::max();
    std::mt19937 engine{1};
    std::vector<std::uint32_t> sides{1, 9, 10, most - 1, most};
    std::ranges::generate_n(std::back_inserter(sides), 40, [&] { return std::max(static_cast<std::uint32_t>(engine()), 1U); });

    for (const auto width : sides) {
        for (const auto height : sides) {
            CAPTURE(width, height);

            const auto parsed = raster::extent_of(std::format("{}x{}", width, height), std::numeric_limits<std::uint64_t>::max());

            REQUIRE(parsed.has_value());
            CHECK(parsed->width() == width);
            CHECK(parsed->height() == height);
        }
    }
}
```

`std::mt19937` yields one sequence per seed on every standard library, where `std::uniform_int_distribution` maps it differently on each.

## [02]-[REFERENCE_PATHS]

Optimized paths agree with a plain reference written in their test over one set of inputs:

```cpp
TEST_CASE("level totals over columns equal totals over records", "[raster][tiles]") {
    struct Record {
        std::uint64_t bytes;
        std::uint8_t level;
    };

    std::mt19937 engine{1};
    const auto records =
        std::views::iota(0, 4'096) | std::views::transform([&](int) { return Record{.bytes = engine(), .level = static_cast<std::uint8_t>(engine() % 4)}; }) | std::ranges::to<std::vector>();
    raster::Tiles tiles;
    for (const auto& record : records) {
        tiles.add(record.bytes, 0, record.level);
    }

    for (const auto level : {std::uint8_t{0}, std::uint8_t{3}, std::uint8_t{4}}) {
        CAPTURE(int{level});
        std::uint64_t expected{0};
        for (const auto& record : records) {
            if (record.level == level) {
                expected += record.bytes;
            }
        }

        CHECK(tiles.at_level(level) == expected);
    }
}
```

`Tiles` keeps `bytes`, `column`, and `level` as one array each. Level `4` holds no tile, and both paths total `0`.

## [03]-[FUZZ_TARGETS]

Fuzz targets enter at a function taking untrusted bytes, view their input buffer in place, and end their process when an oracle fails:

```cpp
extern "C" auto LLVMFuzzerTestOneInput(const std::uint8_t* data, std::size_t size) -> int {
    constexpr std::uint64_t limit{1'000'000};
    const std::string_view text{reinterpret_cast<const char*>(data), size};
    const auto digits = [](std::string_view side) { return !side.empty() && std::ranges::all_of(side, [](char letter) { return letter >= '0' && letter <= '9'; }); };
    const auto split = text.find('x');
    const auto shaped = split != std::string_view::npos && digits(text.substr(0, split)) && digits(text.substr(split + 1));

    const auto parsed = raster::extent_of(text, limit);

    if (parsed && (!shaped || parsed->cells() > limit || raster::extent_of(std::format("{}x{}", parsed->width(), parsed->height()), limit) != parsed)) {
        std::abort();
    }
    return 0;
}
```

`shaped` restates `extent_of`'s documented grammar and calls no parser. Accepted text is shaped, within `limit`, and equal to the parse of its own formatted extent. A `std::string` copy of `data` adds a terminator and spare capacity, which hide a read past `size` from AddressSanitizer.
