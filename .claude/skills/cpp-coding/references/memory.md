# [MEMORY]

## [01]-[LAYOUT]

Fields sit in descending alignment:

```cpp
struct Loose {
    bool dirty;
    std::uint64_t bytes;
    std::uint8_t level;
    std::uint32_t column;
};

struct Dense {
    std::uint64_t bytes;
    std::uint32_t column;
    std::uint8_t level;
    bool dirty;
};

static_assert(sizeof(Loose) == 24 && sizeof(Dense) == 16);
```

`clang -fsyntax-only -Xclang -fdump-record-layouts-simple <file>` prints size, alignment, and field offsets in bits per used record, and `-Wpadded` names each padded field.

## [02]-[COLUMNS]

Fields a pass reads without their neighbors are one array each, behind a type that keeps their lengths equal:

```cpp
class Tiles {
public:
    constexpr auto add(std::uint64_t bytes, std::uint32_t column, std::uint8_t level) -> void {
        bytes_.push_back(bytes);
        column_.push_back(column);
        level_.push_back(level);
    }

    [[nodiscard]] constexpr auto at_level(std::uint8_t wanted) const noexcept -> std::uint64_t {
        return std::ranges::fold_left(
            std::views::zip(level_, bytes_) | std::views::filter([wanted](const auto& tile) { return std::get<0>(tile) == wanted; }) | std::views::values, std::uint64_t{0}, std::plus{});
    }

private:
    std::vector<std::uint64_t> bytes_;
    std::vector<std::uint32_t> column_;
    std::vector<std::uint8_t> level_;
};
```

`at_level` reads `level_` and `bytes_` and leaves `column_` out of the cache.

## [03]-[HANDLES]

References into a pool that reuses its slots are an index with a generation:

```cpp
struct Slot {
    std::uint32_t index;
    std::uint32_t generation;

    friend constexpr auto operator==(Slot, Slot) noexcept -> bool = default;
};

template <typename Value>
class Pool {
public:
    [[nodiscard]] constexpr auto add(Value value) -> Slot {
        if (free_.empty()) {
            values_.push_back(std::move(value));
            generations_.push_back(0);
            return {.index = static_cast<std::uint32_t>(values_.size() - 1), .generation = 0};
        }
        const auto index = free_.back();
        free_.pop_back();
        values_[index] = std::move(value);
        return {.index = index, .generation = generations_[index]};
    }

    constexpr auto remove(Slot slot) -> void {
        if (find(slot)) {
            generations_[slot.index] += 1;
            free_.push_back(slot.index);
        }
    }

    [[nodiscard]] constexpr auto find(Slot slot) noexcept [[clang::lifetimebound]] -> std::optional<Value&> {
        return slot.index < values_.size() && generations_[slot.index] == slot.generation ? std::optional<Value&>{values_[slot.index]} : std::nullopt;
    }

private:
    std::vector<Value> values_;
    std::vector<std::uint32_t> generations_;
    std::vector<std::uint32_t> free_;
};
```

Slots survive growth of `values_`, and a slot removed and reused answers `std::nullopt` to its old holder.

## [04]-[SHARED_WRITES]

Values different threads write sit on separate cache lines:

```cpp
struct alignas(std::hardware_destructive_interference_size) Tally {
    std::atomic<std::uint64_t> done{0};
};
```

## [05]-[BUFFERS]

Text for a hot path is formatted into storage its caller owns, and a text longer than that storage is an absence:

```cpp
[[nodiscard]] auto name_of(std::span<char> buffer, std::uint32_t column, std::uint32_t row) -> std::optional<std::string_view> {
    const auto written = std::format_to_n(buffer.begin(), std::ssize(buffer), "{}/{}", column, row);
    return written.size > std::ssize(buffer) ? std::nullopt : std::optional{std::string_view{buffer.begin(), written.out}};
}
```

## [06]-[ARITHMETIC]

Sizes multiply in the wide type, and an operation that can overflow it goes through a checked member:

```cpp
[[nodiscard]] constexpr auto bytes_for(std::uint32_t width, std::uint32_t height, std::uint32_t channels) noexcept -> std::optional<std::size_t> {
    std::size_t bytes{};
    return ckd_mul(&bytes, std::uint64_t{width} * height, channels) ? std::nullopt : std::optional{bytes};
}
```

`ckd_mul` is a macro of `<stdckdint.h>`.

## [07]-[CONSTANT_TABLES]

Constant lookups are a sorted `constexpr` array searched by projection:

```cpp
inline constexpr std::array<std::pair<std::string_view, std::uint8_t>, 3> levels{{{"coarse", 0}, {"fine", 2}, {"full", 4}}};

static_assert(std::ranges::is_sorted(levels, {}, &std::pair<std::string_view, std::uint8_t>::first));

[[nodiscard]] constexpr auto level_of(std::string_view name) noexcept -> std::optional<std::uint8_t> {
    const auto found = std::ranges::lower_bound(levels, name, {}, &std::pair<std::string_view, std::uint8_t>::first);
    return found != levels.end() && found->first == name ? std::optional{found->second} : std::nullopt;
}
```
