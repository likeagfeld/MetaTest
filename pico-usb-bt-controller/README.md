# Pico USB-to-Bluetooth Controller Adapter

Transform any wired USB gamepad into a wireless Bluetooth controller for retro gaming consoles using a Raspberry Pi Pico W or Pico 2 W.

## Overview

This project enables you to use modern USB controllers wirelessly with retro gaming consoles via BlueRetro adapters. Inspired by [DroidAsController](https://github.com/PsychedelicOrange/DroidAsController), this implementation runs entirely on a Raspberry Pi Pico W/2W microcontroller.

### Signal Flow
```
USB Controller → Pico W (USB Host) → Bluetooth HID → BlueRetro → Retro Console
                                                                      (Sega Saturn, etc.)
```

## Features

- **USB Host Support**: Reads input from wired USB controllers
- **Bluetooth HID Gamepad**: Transmits as a standard Bluetooth gamepad
- **BlueRetro Compatible**: Works seamlessly with BlueRetro adapters
- **Low Latency**: Direct hardware-to-hardware communication
- **Status LED**: Visual feedback for connection status
  - Fast blink: Waiting for connections
  - Slow blink: One device connected
  - Solid: Both USB and Bluetooth connected
- **Auto-Pairing**: Automatically pairs with BlueRetro
- **Multiple Controller Support**: Compatible with various USB HID gamepads

## Hardware Requirements

### Required Components

1. **Raspberry Pi Pico W** or **Pico 2 W** (Bluetooth required)
2. **USB OTG Adapter** or custom wiring for USB host mode
3. **USB Controller** (any HID-compliant gamepad)
4. **BlueRetro Adapter** for your retro console
5. **Power Supply**: 5V power source for USB host mode

### Optional Components

- Micro USB cable for programming and power
- Breadboard and jumper wires for prototyping
- Case or enclosure for a permanent installation

## Supported Controllers

This adapter supports most USB HID gamepads including:

- Xbox 360/One controllers (wired)
- PlayStation 3/4/5 controllers (wired)
- Nintendo Switch Pro controllers (wired)
- Generic USB gamepads
- Arcade sticks with USB connectivity
- Any HID-compliant game controller

## Software Requirements

- [Raspberry Pi Pico SDK](https://github.com/raspberrypi/pico-sdk)
- [TinyUSB](https://github.com/hathach/tinyusb) (included in Pico SDK)
- [BTstack](https://github.com/bluekitchen/btstack) (included in Pico SDK)
- CMake 3.13+
- GCC ARM cross-compiler

## Building the Project

### 1. Install Pico SDK

```bash
# Clone the Pico SDK
git clone https://github.com/raspberrypi/pico-sdk.git
cd pico-sdk
git submodule update --init

# Set environment variable
export PICO_SDK_PATH=/path/to/pico-sdk
```

### 2. Build the Firmware

```bash
cd pico-usb-bt-controller
mkdir build
cd build

# Configure with CMake
cmake ..

# Build
make -j4
```

This will generate `pico_usb_bt_controller.uf2` in the build directory.

### 3. Flash to Pico

1. Hold the BOOTSEL button on your Pico W
2. Connect it to your computer via USB
3. Release BOOTSEL - it will appear as a USB drive
4. Copy `pico_usb_bt_controller.uf2` to the Pico drive
5. The Pico will automatically reboot with the new firmware

## Wiring Guide

### USB Host Mode Wiring

The Pico requires specific wiring to act as a USB host. See [WIRING.md](WIRING.md) for detailed diagrams.

**Basic Setup:**
- VBUS (5V) power to Pico pin 40 (VBUS)
- USB D+ and D- to GPIO pins (via level shifters if needed)
- GND connections

**Important:** USB host mode requires 5V power on VBUS. Do not attempt to power from the Pico's 3.3V regulator.

## Usage

### First-Time Setup

1. **Flash the firmware** to your Pico W (see Building section)
2. **Wire the USB host** connections (see Wiring Guide)
3. **Power the Pico** with 5V on VBUS
4. **Put BlueRetro in pairing mode**:
   - Hold the pairing button on your BlueRetro adapter
   - Or use the web interface at https://blueretro.io/
5. **Connect your USB controller** to the Pico
6. The devices should auto-pair (LED will go solid)

### Normal Operation

1. Power on the Pico W
2. Wait for it to initialize (fast blinking LED)
3. Connect your USB controller
4. BlueRetro should reconnect automatically
5. Start gaming!

### LED Status Indicators

| LED Pattern | Meaning |
|-------------|---------|
| Fast blink (250ms) | No connections - waiting for USB and Bluetooth |
| Slow blink (500ms) | One connection active (either USB or Bluetooth) |
| Solid on | Both USB controller and Bluetooth connected - ready! |

### Troubleshooting

**Controller not detected:**
- Ensure USB host wiring is correct
- Check 5V power is supplied to VBUS
- Try a different USB controller
- Check serial debug output (UART on GPIO 0/1)

**Bluetooth won't pair:**
- Put BlueRetro in pairing mode first
- Reset both devices and try again
- Check Bluetooth LED status on Pico W

**Lag or missed inputs:**
- Ensure good power supply (USB host draws significant current)
- Check for USB cable quality issues
- Verify controller is HID-compliant

**Serial Debug Output:**
Connect a USB-to-UART adapter to GPIO 0 (TX) and GPIO 1 (RX) at 115200 baud to see debug messages.

## Technical Details

### Bluetooth HID Profile

This adapter implements a standard Bluetooth HID gamepad profile with:
- 16 buttons
- 2 analog sticks (4 axes)
- 2 analog triggers
- 1 D-pad (hat switch)

### Supported HID Report Format

The adapter automatically maps common USB HID gamepad formats to the standard Bluetooth HID format. Most controllers work out of the box.

### Latency

- USB polling: 1ms
- Bluetooth transmission: ~4ms (typical HID latency)
- Total system latency: <10ms

## BlueRetro Compatibility

This adapter is designed to work with [BlueRetro](https://github.com/darthcloud/BlueRetro) adapters, including:

- BlueRetro for Sega Saturn
- BlueRetro for NES/SNES
- BlueRetro for Sega Genesis/Mega Drive
- BlueRetro for Nintendo 64
- BlueRetro for Dreamcast
- BlueRetro for GameCube
- And other BlueRetro-compatible adapters

### Configuration

BlueRetro can be configured at https://blueretro.io/ to adjust:
- Button mappings
- Analog/digital mode
- Sensitivity settings

## Project Structure

```
pico-usb-bt-controller/
├── CMakeLists.txt           # Build configuration
├── pico_sdk_import.cmake    # SDK import helper
├── README.md                # This file
├── WIRING.md                # Detailed wiring guide
├── include/                 # Header files
│   ├── usb_host.h          # USB host interface
│   ├── bt_hid.h            # Bluetooth HID interface
│   ├── controller_mapper.h  # Controller mapping
│   ├── btstack_config.h    # BTstack configuration
│   └── tusb_config.h       # TinyUSB configuration
└── src/                     # Source files
    ├── main.c              # Main application
    ├── usb_host.c          # USB host implementation
    ├── bt_hid.c            # Bluetooth HID implementation
    └── controller_mapper.c  # Controller format mapping
```

## License

This project is open source. Feel free to modify and distribute.

## Credits

- Inspired by [DroidAsController](https://github.com/PsychedelicOrange/DroidAsController)
- Works with [BlueRetro](https://github.com/darthcloud/BlueRetro) by Jacques Gagnon
- Built with [Raspberry Pi Pico SDK](https://github.com/raspberrypi/pico-sdk)
- Uses [TinyUSB](https://github.com/hathach/tinyusb) for USB host
- Uses [BTstack](https://github.com/bluekitchen/btstack) for Bluetooth

## Contributing

Contributions welcome! Please feel free to submit issues and pull requests.

## Support

For issues and questions:
- Check the Troubleshooting section
- Review the [BlueRetro documentation](https://github.com/darthcloud/BlueRetro/wiki)
- Open an issue on this repository

## Future Enhancements

Potential improvements:
- [ ] Rumble/vibration support
- [ ] Multiple controller support
- [ ] OLED display for status
- [ ] Web configuration interface
- [ ] Custom button mapping
- [ ] Battery power optimization
