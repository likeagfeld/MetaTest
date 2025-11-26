cmake_minimum_required(VERSION 3.13)

# Pull in SDK (must be before project)
include(pico_sdk_import.cmake)

project(pico_usb_bt_controller C CXX ASM)
set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)

# Initialize the SDK
pico_sdk_init()

add_executable(pico_usb_bt_controller
    src/main.c
    src/usb_host.c
    src/bt_hid.c
    src/controller_mapper.c
)

# Pull in common dependencies
target_include_directories(pico_usb_bt_controller PRIVATE
    ${CMAKE_CURRENT_LIST_DIR}/include
)

# Enable USB output, disable UART output
pico_enable_stdio_usb(pico_usb_bt_controller 0)
pico_enable_stdio_uart(pico_usb_bt_controller 1)

# Create map/bin/hex/uf2 file etc.
pico_add_extra_outputs(pico_usb_bt_controller)

# Link libraries
target_link_libraries(pico_usb_bt_controller
    pico_stdlib
    pico_cyw43_arch_none  # For Bluetooth on Pico W
    pico_btstack_classic
    pico_btstack_ble
    tinyusb_host
    tinyusb_board
    hardware_pio
)

# Configure BTstack
target_compile_definitions(pico_usb_bt_controller PRIVATE
    PICO_STDIO_UART_TX_PIN=0
    PICO_STDIO_UART_RX_PIN=1
    CYW43_LWIP=0
    PICO_BTSTACK_CONFIG=1
)
