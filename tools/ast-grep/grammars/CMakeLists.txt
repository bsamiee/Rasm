cmake_minimum_required(VERSION 4.5)

project(grammars LANGUAGES C)

# --- [CONSTANTS] ------------------------------------------------------------------------

find_program(TREE_SITTER tree-sitter REQUIRED)
find_program(NODE node REQUIRED)
cmake_path(GET CMAKE_CURRENT_SOURCE_DIR PARENT_PATH package_dir)
cmake_path(GET package_dir PARENT_PATH tools_dir)
cmake_path(GET tools_dir PARENT_PATH workspace_dir)
set(node_modules "${package_dir}/node_modules")
set(sgconfig "${workspace_dir}/sgconfig.yml")
file(
    GLOB upstream
    CONFIGURE_DEPENDS
    "${node_modules}/tree-sitter-*/grammar.js"
    "${node_modules}/tree-sitter-*/*/grammar.js"
    "${node_modules}/@tree-sitter-grammars/*/grammar.js"
    "${node_modules}/@tree-sitter-grammars/*/*/grammar.js"
)
set(shared
    "${CMAKE_CURRENT_SOURCE_DIR}/derive.js"
    "${CMAKE_CURRENT_SOURCE_DIR}/compose.js"
    "${package_dir}/package.json"
    "${TREE_SITTER}"
    ${upstream}
)
file(GLOB entries RELATIVE "${CMAKE_CURRENT_SOURCE_DIR}" CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*/grammar.js")
file(GLOB catalogs CONFIGURE_DEPENDS "${package_dir}/outline/*.yml")

# --- [COMPOSITION] ----------------------------------------------------------------------

foreach(entry IN LISTS entries)
    cmake_path(GET entry PARENT_PATH language)
    set(source "${CMAKE_CURRENT_SOURCE_DIR}/${language}")
    set(generated "${CMAKE_CURRENT_BINARY_DIR}/${language}")
    add_custom_command(
        OUTPUT "${generated}/grammar.json" "${generated}/node-types.json" "${generated}/parser.c"
        COMMAND "${TREE_SITTER}" generate --output "${generated}" "${source}/grammar.js"
        WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
        DEPENDS "${source}/grammar.js" ${shared}
        VERBATIM
    )
    add_custom_command(
        OUTPUT "${generated}/grammar.h"
        COMMAND "${CMAKE_COMMAND}" "-DGRAMMAR=${generated}/grammar.json" "-DOUTPUT=${generated}/grammar.h" -P "${CMAKE_CURRENT_SOURCE_DIR}/header.cmake"
        DEPENDS "${generated}/grammar.json" "${CMAKE_CURRENT_SOURCE_DIR}/header.cmake"
        VERBATIM
    )
    file(GLOB scanner "${source}/scanner.c")
    add_library(${language} MODULE "${generated}/parser.c" "${generated}/grammar.h" "${CMAKE_CURRENT_SOURCE_DIR}/compose.c" ${scanner})
    target_include_directories(${language} PRIVATE "${generated}" "${node_modules}")
    target_compile_features(${language} PRIVATE c_std_11)
    target_compile_options(${language} PRIVATE "SHELL:-include ${generated}/grammar.h" "$<$<C_COMPILER_ID:GNU>:-fno-strict-aliasing>")
    set_target_properties(
        ${language}
        PROPERTIES PREFIX "" C_VISIBILITY_PRESET hidden LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}"
    )
endforeach()

foreach(catalog IN LISTS catalogs)
    cmake_path(GET catalog FILENAME name)
    set(outline "${CMAKE_CURRENT_BINARY_DIR}/outline/${name}")
    add_custom_command(
        OUTPUT "${outline}"
        COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_CURRENT_BINARY_DIR}/outline"
        COMMAND "${NODE}" "${package_dir}/outline/main.ts" "${sgconfig}" "${catalog}" "${outline}"
        DEPENDS "${catalog}" "${package_dir}/outline/main.ts" "${sgconfig}"
        VERBATIM
    )
    list(APPEND outlines "${outline}")
endforeach()
add_custom_target(outlines ALL DEPENDS ${outlines})
