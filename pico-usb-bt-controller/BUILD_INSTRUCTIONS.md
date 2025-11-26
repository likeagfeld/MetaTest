# Build Instructions

Complete guide to building and flashing the Pico USB-to-Bluetooth Controller Adapter firmware.

## Prerequisites

### Required Software

1. **Raspberry Pi Pico SDK**
2. **CMake** (version 3.13 or later)
3. **GCC ARM Cross-Compiler**
4. **Git**
5. **Python 3** (for Pico SDK tools)

### Platform-Specific Setup

#### Linux (Ubuntu/Debian)

```bash
# Update package list
sudo apt update

# Install build tools
sudo apt install -y cmake gcc-arm-none-eabi libnewlib-arm-none-eabi \
    libstdc++-arm-none-eabi-newlib build-essential git python3

# Install Pico SDK
cd ~
git clone https://github.com/raspberrypi/pico-sdk.git
cd pico-sdk
git submodule update --init
cd ..

# Set environment variable (add to ~/.bashrc for persistence)
export PICO_SDK_PATH=~/pico-sdk
```

#### macOS

```bash
# Install Homebrew (if not already installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install build tools
brew install cmake
brew tap ArmMbed/homebrew-formulae
brew install arm-none-eabi-gcc
brew install git python3

# Install Pico SDK
cd ~
git clone https://github.com/raspberrypi/pico-sdk.git
cd pico-sdk
git submodule update --init
cd ..

# Set environment variable (add to ~/.zshrc for persistence)
export PICO_SDK_PATH=~/pico-sdk
```

#### Windows

**Option 1: Using Visual Studio Code + Pico Extension**
1. Install [Visual Studio Code](https://code.visualstudio.com/)
2. Install the [Raspberry Pi Pico extension](https://marketplace.visualstudio.com/items?itemName=raspberry-pi.raspberry-pi-pico)
3. The extension will guide you through SDK installation

**Option 2: Manual Installation**
1. Install [CMake](https://cmake.org/download/)
2. Install [ARM GCC Compiler](https://developer.arm.com/tools-and-software/open-source-software/developer-tools/gnu-toolchain/gnu-rm/downloads)
3. Install [Git for Windows](https://git-scm.com/download/win)
4. Install [Python 3](https://www.python.org/downloads/)
5. Clone Pico SDK:
   ```cmd
   cd C:\
   git clone https://github.com/raspberrypi/pico-sdk.git
   cd pico-sdk
   git submodule update --init
   ```
6. Set environment variable:
   ```cmd
   setx PICO_SDK_PATH "C:\pico-sdk"
   ```

## Building the Firmware

### Step 1: Clone the Repository

```bash
git clone <your-repo-url>
cd pico-usb-bt-controller
```

### Step 2: Configure the Build

```bash
# Create build directory
mkdir build
cd build

# Run CMake configuration
cmake ..
```

**Expected output:**
```
-- Using PICO_SDK_PATH from environment ('/path/to/pico-sdk')
-- Pico SDK version: 1.5.1
-- Build type is Release
...
-- Configuring done
-- Generating done
```

### Step 3: Compile

```bash
# Build the firmware
make -j4
```

The `-j4` flag uses 4 parallel jobs for faster compilation. Adjust based on your CPU cores.

**Expected output:**
```
Scanning dependencies of target pico_usb_bt_controller
[ 10%] Building C object CMakeFiles/pico_usb_bt_controller.dir/src/main.c.obj
[ 20%] Building C object CMakeFiles/pico_usb_bt_controller.dir/src/usb_host.c.obj
[ 30%] Building C object CMakeFiles/pico_usb_bt_controller.dir/src/bt_hid.c.obj
[ 40%] Building C object CMakeFiles/pico_usb_bt_controller.dir/src/controller_mapper.c.obj
...
[100%] Built target pico_usb_bt_controller
```

### Step 4: Locate the Output Files

After successful compilation, you'll find:

```
build/
├── pico_usb_bt_controller.elf      # ELF binary
├── pico_usb_bt_controller.bin      # Raw binary
├── pico_usb_bt_controller.hex      # Intel HEX format
├── pico_usb_bt_controller.uf2      # UF2 format (for flashing)
└── pico_usb_bt_controller.map      # Memory map
```

The `.uf2` file is what you'll flash to the Pico.

## Flashing to Pico

### Method 1: UF2 Bootloader (Easiest)

1. **Enter Bootloader Mode:**
   - Disconnect the Pico from USB
   - Hold down the BOOTSEL button on the Pico
   - While holding BOOTSEL, connect USB cable to your computer
   - Release BOOTSEL

2. **Flash the Firmware:**
   - The Pico will appear as a USB mass storage device named "RPI-RP2"
   - Copy `pico_usb_bt_controller.uf2` to the RPI-RP2 drive
   - The Pico will automatically reboot and start running your firmware

**Linux/macOS:**
```bash
# Assuming the Pico mounted at /media/RPI-RP2
cp build/pico_usb_bt_controller.uf2 /media/RPI-RP2/
```

**Windows:**
```cmd
# Drag and drop the UF2 file to the RPI-RP2 drive
# Or use command line:
copy build\pico_usb_bt_controller.uf2 E:\
```

### Method 2: Using picotool

```bash
# Install picotool (if not already installed)
cd ~
git clone https://github.com/raspberrypi/picotool.git
cd picotool
mkdir build
cd build
cmake ..
make
sudo make install

# Flash the firmware
picotool load -f pico_usb_bt_controller.uf2
picotool reboot
```

### Method 3: Using OpenOCD + SWD Debugger

For development with a hardware debugger:

```bash
# Start OpenOCD
openocd -f interface/cmsis-dap.cfg -f target/rp2040.cfg

# In another terminal, flash with GDB
gdb-multiarch build/pico_usb_bt_controller.elf
(gdb) target remote localhost:3333
(gdb) load
(gdb) monitor reset init
(gdb) continue
```

## Verification

### Check Serial Debug Output

Connect a USB-to-serial adapter to GPIO 0 (TX) and GPIO 1 (RX):

```bash
# Linux/macOS
screen /dev/ttyUSB0 115200

# Or using minicom
minicom -D /dev/ttyUSB0 -b 115200

# Windows (using PuTTY)
# Set COM port and baud rate to 115200
```

**Expected output:**
```
=== Pico USB-to-Bluetooth Controller Adapter ===
Version: 1.0.0
Target: BlueRetro (Sega Saturn)

[OK] CYW43 wireless chip initialized
[OK] USB host initialized
[OK] Bluetooth HID initialized

Waiting for USB controller connection...
BlueRetro pairing: Put adapter in pairing mode, then connect USB controller
```

### Check LED Status

- **Fast blink**: Firmware running, waiting for connections
- **Slow blink**: One device connected
- **Solid**: Both USB and Bluetooth connected

## Troubleshooting Build Issues

### CMake Can't Find Pico SDK

**Error:**
```
CMake Error: PICO_SDK_PATH is not set
```

**Solution:**
```bash
export PICO_SDK_PATH=/path/to/pico-sdk
# Make it permanent by adding to ~/.bashrc or ~/.zshrc
```

### Missing ARM Compiler

**Error:**
```
CMake Error: Could not find arm-none-eabi-gcc
```

**Solution:**
- Reinstall ARM GCC toolchain
- Ensure it's in your PATH
- On Windows, you may need to restart after installation

### TinyUSB or BTstack Errors

**Error:**
```
fatal error: tusb.h: No such file or directory
```

**Solution:**
```bash
# Update Pico SDK submodules
cd $PICO_SDK_PATH
git submodule update --init --recursive
```

### Out of Memory Errors

**Error:**
```
region 'FLASH' overflowed
```

**Solution:**
- The firmware is too large for Pico's flash
- Remove debug code or optimize with `-Os` flag
- Check that you're building for Pico W (not original Pico)

### Linker Errors

**Error:**
```
undefined reference to 'function_name'
```

**Solution:**
- Ensure all source files are listed in CMakeLists.txt
- Check that all required libraries are linked
- Clean and rebuild: `rm -rf build && mkdir build && cd build && cmake .. && make`

## Customization

### Changing USB Pins

Edit `src/usb_host.c` and modify the pin definitions:

```c
// Default: GPIO 0 (D-) and GPIO 1 (D+)
#define USB_DP_PIN 1
#define USB_DM_PIN 0
```

### Changing Device Name

Edit `src/bt_hid.c`:

```c
#define DEVICE_NAME "Pico Gamepad"  // Change to your preferred name
```

### Adjusting Debug Output

Edit `CMakeLists.txt`:

```cmake
# Enable USB debug output (disable UART)
pico_enable_stdio_usb(pico_usb_bt_controller 1)
pico_enable_stdio_uart(pico_usb_bt_controller 0)
```

## Clean Build

To start fresh:

```bash
# Remove build directory
rm -rf build

# Rebuild
mkdir build
cd build
cmake ..
make -j4
```

## Advanced: Optimizing Build

### Release Build (Smaller, Faster)

```bash
cmake -DCMAKE_BUILD_TYPE=Release ..
make -j4
```

### Debug Build (More Info)

```bash
cmake -DCMAKE_BUILD_TYPE=Debug ..
make -j4
```

### Size Optimization

In `CMakeLists.txt`, add:

```cmake
target_compile_options(pico_usb_bt_controller PRIVATE
    -Os          # Optimize for size
    -ffunction-sections
    -fdata-sections
)

target_link_options(pico_usb_bt_controller PRIVATE
    -Wl,--gc-sections  # Remove unused sections
)
```

## Next Steps

After successful build and flash:
1. Wire up USB host connections (see [WIRING.md](WIRING.md))
2. Connect your USB controller
3. Pair with BlueRetro adapter
4. Start gaming!

## Getting Help

If you encounter build issues:
1. Check that PICO_SDK_PATH is set correctly
2. Verify all prerequisites are installed
3. Try a clean build
4. Check the [Pico SDK documentation](https://raspberrypi.github.io/pico-sdk-doxygen/)
5. Open an issue on the project repository

Happy building! 🛠️
