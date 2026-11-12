cmake_minimum_required(VERSION 4.5)

project(rasm-extent-tests LANGUAGES CXX)

# --- [DEPENDENCIES] ---------------------------------------------------------------------

find_package(Catch2 3 CONFIG REQUIRED)
include(Catch)

# --- [TARGETS] --------------------------------------------------------------------------

add_executable(rasm-extent-tests extent.cpp)
target_compile_features(rasm-extent-tests PRIVATE cxx_std_26)
target_link_libraries(rasm-extent-tests PRIVATE rasm-extent::extent Catch2::Catch2WithMain)
catch_discover_tests(rasm-extent-tests ADD_TAGS_AS_LABELS)
