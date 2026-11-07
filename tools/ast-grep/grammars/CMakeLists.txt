cmake_minimum_required(VERSION 4.4)

# --- [COMPOSITION] ----------------------------------------------------------------------

if(CMAKE_SCRIPT_MODE_FILE)
    execute_process(
        COMMAND "${CMAKE_COMMAND}" -S "${CMAKE_CURRENT_LIST_DIR}" -B "${BINARY_DIR}" -DCMAKE_BUILD_TYPE=Release
            "-DGRAMMAR_SOURCE:FILEPATH=${GRAMMAR_SOURCE}" "-DNODE_MODULES:PATH=${NODE_MODULES}" "-DSCANNER=${SCANNER}"
        COMMAND_ERROR_IS_FATAL ANY
    )
    execute_process(COMMAND "${CMAKE_COMMAND}" --build "${BINARY_DIR}" --config Release COMMAND_ERROR_IS_FATAL ANY)
    return()
endif()

project(grammar LANGUAGES C)

set(GRAMMAR_SOURCE "" CACHE FILEPATH "Grammar extension or composition")
set(NODE_MODULES "" CACHE PATH "Installed packages")
set(SCANNER "" CACHE STRING "External scanner")

cmake_path(GET CMAKE_CURRENT_BINARY_DIR PARENT_PATH published_dir)
cmake_path(GET published_dir FILENAME language)
cmake_path(GET published_dir PARENT_PATH library_dir)
set(declared_dir "${CMAKE_CURRENT_BINARY_DIR}/src")
set(generator_options --abi=latest --js-runtime node)

execute_process(COMMAND tree-sitter generate ${generator_options} --no-parser --output "${declared_dir}" "${GRAMMAR_SOURCE}" WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}" COMMAND_ERROR_IS_FATAL ANY)
execute_process(COMMAND tree-sitter --version OUTPUT_VARIABLE generator COMMAND_ERROR_IS_FATAL ANY)
file(READ "${declared_dir}/grammar.json" grammar)
string(SHA256 parser_key "${generator}${generator_options}${grammar}")
set(generated_dir "${published_dir}/parser/${parser_key}")
if(NOT EXISTS "${generated_dir}/parser.c")
    execute_process(COMMAND tree-sitter generate ${generator_options} --output "${CMAKE_CURRENT_BINARY_DIR}/parser" "${declared_dir}/grammar.json" COMMAND_ERROR_IS_FATAL ANY)
    file(MAKE_DIRECTORY "${published_dir}/parser")
    file(RENAME "${CMAKE_CURRENT_BINARY_DIR}/parser" "${generated_dir}" RESULT stored)
endif()
string(JSON name GET "${grammar}" name)
string(JSON dialect GET "${grammar}" inherits)
string(JSON external_count LENGTH "${grammar}" externals)
string(JSON constants ERROR_VARIABLE constantless GET "${grammar}" rules _constants members)

add_library(${PROJECT_NAME} MODULE "${generated_dir}/parser.c" ${SCANNER})
target_include_directories(${PROJECT_NAME} PRIVATE "${generated_dir}" "${NODE_MODULES}" "${CMAKE_CURRENT_BINARY_DIR}")
target_compile_features(${PROJECT_NAME} PRIVATE c_std_11)
set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "" SUFFIX ".so" OUTPUT_NAME "${language}" C_VISIBILITY_PRESET hidden)
set_property(SOURCE ${SCANNER} PROPERTY COMPILE_OPTIONS "$<$<C_COMPILER_ID:GNU>:-fno-strict-aliasing>")
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E make_directory "${published_dir}/src"
    COMMAND "${CMAKE_COMMAND}" -E rename "${declared_dir}/grammar.json" "${published_dir}/src/grammar.json"
    COMMAND "${CMAKE_COMMAND}" -E rename "${declared_dir}/node-types.json" "${published_dir}/src/node-types.json"
    COMMAND "${CMAKE_COMMAND}" -E rename "$<TARGET_FILE:${PROJECT_NAME}>" "${library_dir}/${language}.so"
    VERBATIM
)

if(NOT constantless)
    string(JSON constant_count LENGTH "${constants}")
    math(EXPR constant_last "${constant_count} - 1")
    set(constant_rows "")
    foreach(index RANGE 0 ${constant_last} 2)
        math(EXPR value_at "${index} + 1")
        string(JSON constant GET "${constants}" ${index} value)
        string(JSON value GET "${constants}" ${value_at} value)
        string(APPEND constant_rows "#define ${constant} ${value}\n")
    endforeach()
    file(WRITE "${CMAKE_CURRENT_BINARY_DIR}/grammar.h" "${constant_rows}")
endif()
if(NOT name STREQUAL dialect)
    math(EXPR hole_symbol "${external_count} - 1")
    foreach(function IN ITEMS create destroy serialize deserialize)
        set_property(SOURCE ${SCANNER} APPEND PROPERTY COMPILE_DEFINITIONS "tree_sitter_${dialect}_external_scanner_${function}=tree_sitter_${name}_external_scanner_${function}")
    endforeach()
    set_property(SOURCE ${SCANNER} APPEND PROPERTY COMPILE_DEFINITIONS "tree_sitter_${dialect}_external_scanner_scan=dialect_scan")
    set_property(SOURCE compose.c PROPERTY COMPILE_DEFINITIONS "COMPOSITE_SCAN=tree_sitter_${name}_external_scanner_scan" "HOLE_SYMBOL=${hole_symbol}")
    target_sources(${PROJECT_NAME} PRIVATE compose.c)
endif()
