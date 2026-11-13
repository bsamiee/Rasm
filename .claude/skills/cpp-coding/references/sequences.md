# [SEQUENCES]

## [01]-[PROJECTIONS]

Algorithms take a projection in place of a comparator, and reductions name their fold:

```cpp
[[nodiscard]] constexpr auto row_major(std::span<const Tile> tiles) noexcept -> bool {
    return std::ranges::is_sorted(tiles, {}, [](const Tile& tile) { return std::pair{tile.row, tile.column}; });
}

[[nodiscard]] constexpr auto total(std::span<const Tile> tiles) noexcept -> std::uint64_t { return std::ranges::fold_left(tiles | std::views::transform(&Tile::bytes), std::uint64_t{0}, std::plus{}); }
```

`Tile` is a record of `column`, `row`, `bytes`, and `name`.

## [02]-[PIPELINES]

Views transform a sequence and one `std::ranges::to` collects it:

```cpp
[[nodiscard]] constexpr auto heaviest(std::span<const Tile> tiles, std::size_t count) -> std::vector<std::string_view> {
    auto order = tiles | std::views::transform([](const Tile& tile) { return std::pair{tile.bytes, std::string_view{tile.name}}; }) | std::ranges::to<std::vector>();
    const auto kept = std::ranges::subrange{order.begin(), std::ranges::next(order.begin(), static_cast<std::ptrdiff_t>(count), order.end())};
    std::ranges::partial_sort(order, kept.end(), std::ranges::greater{});
    return kept | std::views::values | std::ranges::to<std::vector>();
}
```

`std::ranges::next` with a bound stops at the end of a shorter sequence, and `partial_sort` orders the kept elements alone.

## [03]-[POSITIONS]

Neighbors, positions, and joined sequences come from adaptors in place of an index variable:

```cpp
[[nodiscard]] constexpr auto steps(std::span<const Tile> tiles) -> std::vector<std::int64_t> {
    return tiles | std::views::transform(&Tile::column) | std::views::pairwise_transform([](std::int64_t left, std::int64_t right) { return right - left; }) | std::ranges::to<std::vector>();
}

[[nodiscard]] auto labelled(std::span<const Tile> front, std::span<const Tile> back) -> std::vector<std::string> {
    return std::views::concat(front, back) | std::views::enumerate | std::views::transform([](const auto& entry) {
               const auto& [position, tile] = entry;
               return std::format("{}:{}", position, tile.name);
           })
           | std::ranges::to<std::vector>();
}
```

## [04]-[MOVES]

Elements leave an owned sequence through `std::views::as_rvalue`:

```cpp
[[nodiscard]] constexpr auto names(std::vector<Tile> tiles) -> std::vector<std::string> { return tiles | std::views::transform(&Tile::name) | std::views::as_rvalue | std::ranges::to<std::vector>(); }
```

`names(std::move(tiles))` moves each name out of its tile, and `names(tiles)` copies the sequence once at the call.
