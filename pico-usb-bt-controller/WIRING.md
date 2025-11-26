# Wiring Guide - Pico USB-to-Bluetooth Controller Adapter

This guide explains how to wire your Raspberry Pi Pico W for USB host functionality.

## Important Safety Notes

⚠️ **WARNING**:
- USB operates at 5V, while Pico GPIO operates at 3.3V
- Direct connection of 5V USB signals to GPIO can damage your Pico
- Always use proper level shifting or follow the recommended wiring methods below

## Option 1: Using Pico-PIO-USB (Recommended)

This method uses the PIO (Programmable I/O) to implement USB host on standard GPIO pins with software-based level shifting.

### Required Components
- Raspberry Pi Pico W or Pico 2 W
- USB Type-A female breakout board or connector
- 2x 27Ω resistors (for D+/D- lines)
- Breadboard and jumper wires
- 5V power source

### Wiring Diagram

```
USB Connector                  Pico W
┌─────────────┐               ┌──────────────┐
│             │               │              │
│  VBUS (5V)  ├───────────────┤ Pin 40 VBUS  │ (Power input)
│             │               │              │
│     D-      ├────[27Ω]──────┤ GPIO 0       │ (Data -)
│             │               │              │
│     D+      ├────[27Ω]──────┤ GPIO 1       │ (Data +)
│             │               │              │
│     GND     ├───────────────┤ Pin 38 GND   │
│             │               │              │
└─────────────┘               └──────────────┘

Power Supply (5V)
      │
      ├──────────────────────────┤ Pin 40 VBUS  │
      │                          │              │
      └──────────────────────────┤ Pin 38 GND   │
```

### Pin Assignments

| Function | Pico Pin | GPIO | Notes |
|----------|----------|------|-------|
| USB D- | Pin 1 | GPIO 0 | Also used for UART TX (debug) |
| USB D+ | Pin 2 | GPIO 1 | Also used for UART RX (debug) |
| USB VBUS | Pin 40 | VBUS | 5V power input |
| Ground | Pin 38 | GND | Common ground |

### Step-by-Step Instructions

1. **Connect the USB data lines:**
   - Solder a 27Ω resistor to USB D- (white or green wire)
   - Connect the resistor to Pico GPIO 0 (Pin 1)
   - Solder a 27Ω resistor to USB D+ (green or white wire)
   - Connect the resistor to Pico GPIO 1 (Pin 2)

2. **Connect power:**
   - USB VBUS (red wire) to Pico Pin 40 (VBUS)
   - USB GND (black wire) to Pico Pin 38 (GND)

3. **Connect external 5V power:**
   - Connect 5V power supply positive to Pico Pin 40 (VBUS)
   - Connect 5V power supply ground to Pico Pin 38 (GND)

4. **Optional - Debug UART:**
   - If you want serial debug output, you cannot use GPIO 0/1 for USB
   - Instead, use GPIO 2/3 for USB and GPIO 0/1 for UART
   - Modify the code accordingly

## Option 2: USB OTG Adapter Method

A simpler approach using a USB OTG adapter cable.

### Required Components
- Raspberry Pi Pico W or Pico 2 W
- Micro USB OTG adapter with female USB-A port
- USB hub with external power (recommended)
- 5V power supply

### Setup

1. **Using Powered USB Hub:**
   ```
   5V Power ──→ Powered USB Hub ──→ USB Controller
                       │
                       └──→ Pico W (via OTG adapter on micro USB port)
   ```

2. **Direct Connection:**
   ```
   5V Power ──→ Pico W VBUS (Pin 40)

   USB Controller ──→ OTG Adapter ──→ Pico W (micro USB)
   ```

**Note:** This method may have limitations depending on the OTG adapter. The PIO-USB method is more reliable.

## Option 3: Custom PCB Design

For a permanent installation, consider designing a custom PCB:

### Features to Include
- Pico W socket or solder pads
- USB Type-A female connector
- Proper ESD protection (TPD4E001 or similar)
- 5V power regulation
- Level shifting circuit (if needed)
- Status LED circuit
- Mounting holes

### Reference Schematic Components
```
USB Data Lines:
- 27Ω series resistors on D+/D-
- 15kΩ pull-down resistors to ground
- ESD protection diodes

Power:
- 5V input
- Bulk capacitors (10µF, 100µF)
- Ferrite bead for noise filtering
```

## Power Considerations

### Power Budget

| Component | Current Draw | Notes |
|-----------|--------------|-------|
| Pico W | ~150mA | Peak with WiFi/BT active |
| USB Controller | 100-500mA | Varies by controller |
| **Total** | **250-650mA** | Use 1A+ power supply |

### Power Supply Options

1. **USB Wall Adapter (5V 1A+)**: Recommended for desktop use
2. **Power Bank**: For portable setups
3. **Bench Power Supply**: For development/testing

**Important:** Do NOT power from a computer USB port when USB controller is connected - may exceed current limits.

## Testing Your Wiring

### Basic Continuity Test
1. Power off all devices
2. Use a multimeter to verify:
   - No shorts between VBUS and GND
   - Correct connections between USB pins and Pico GPIO
   - Resistors are correct value (27Ω)

### Power-On Test
1. Connect 5V power to VBUS (without USB controller)
2. Pico LED should blink (indicating code is running)
3. Measure voltage at VBUS: should be ~5V
4. Measure voltage at GPIO pins: should be 0V (idle) or 3.3V max

### USB Controller Test
1. Connect powered Pico with firmware loaded
2. Connect serial debug cable (if available)
3. Plug in USB controller
4. Check serial output for "HID device mounted" message
5. LED should change blink pattern

## Troubleshooting Wiring Issues

### Controller Not Detected

**Check:**
- [ ] VBUS is receiving 5V
- [ ] D+/D- connections are correct (not swapped)
- [ ] 27Ω resistors are in place
- [ ] No cold solder joints
- [ ] USB cable is data-capable (not power-only)

### Pico Won't Boot

**Check:**
- [ ] No shorts between power and ground
- [ ] Power supply provides adequate current
- [ ] GPIO 0/1 not shorted to ground or VBUS

### Intermittent Connection

**Check:**
- [ ] Solid solder joints on all connections
- [ ] USB cable quality
- [ ] Power supply stability
- [ ] Capacitors for power filtering

### Pico Gets Hot

**⚠️ Immediately disconnect power!**
- Check for shorts between VBUS and GND
- Verify voltage is not exceeding 5.5V
- Check USB controller current draw
- Inspect for reversed polarity

## Alternative GPIO Pins

If GPIO 0/1 are not suitable (e.g., you need UART debug), you can use other pins:

### Alternate Pin Options

| Function | Alternative GPIO | Pin |
|----------|------------------|-----|
| USB D- | GPIO 2 | Pin 4 |
| USB D+ | GPIO 3 | Pin 5 |
| USB D- | GPIO 6 | Pin 9 |
| USB D+ | GPIO 7 | Pin 10 |

**Note:** You must update the code to match your pin selection.

## Debug UART Connection

For serial debug output (optional but recommended for development):

```
Pico W                    USB-to-Serial Adapter
┌──────────────┐         ┌──────────────┐
│              │         │              │
│ GPIO 0 (TX)  ├─────────┤ RX           │
│              │         │              │
│ GPIO 1 (RX)  ├─────────┤ TX           │
│              │         │              │
│ GND          ├─────────┤ GND          │
│              │         │              │
└──────────────┘         └──────────────┘
```

Serial settings: 115200 baud, 8N1

**Note:** If you use GPIO 0/1 for USB, you cannot use them for UART. Choose GPIO 2/3 for USB instead.

## Enclosure Recommendations

Once wiring is complete, consider housing the project:

### 3D Printed Case
- Design files can include cutouts for USB ports
- Include mounting for Pico W
- Ventilation holes for heat dissipation
- Status LED light pipe

### Commercial Enclosures
- Hammond 1551 series (small plastic enclosures)
- Adafruit Raspberry Pi cases (with modifications)
- Custom laser-cut acrylic cases

## Final Assembly Checklist

- [ ] All solder joints inspected and tested
- [ ] Continuity verified on all connections
- [ ] No shorts between power rails
- [ ] Correct resistor values confirmed
- [ ] Power supply tested (5V, adequate current)
- [ ] Firmware flashed successfully
- [ ] USB controller detected (check serial output)
- [ ] Bluetooth pairing successful
- [ ] Secure all connections (hot glue, heat shrink, etc.)
- [ ] Enclosure assembled (if using)
- [ ] Final functionality test completed

## Safety Reminders

⚠️ **Always:**
- Disconnect power before wiring changes
- Double-check polarity before applying power
- Use appropriate wire gauge for current
- Insulate exposed connections
- Test in a safe environment first

⚠️ **Never:**
- Exceed 5.5V on VBUS
- Connect 5V directly to GPIO without level shifting
- Use power supplies without current limiting
- Leave exposed high-voltage connections

## Need Help?

If you encounter issues:
1. Recheck all connections against this guide
2. Test with a simple USB device first (like a mouse)
3. Verify power supply with a multimeter
4. Check serial debug output for error messages
5. Post pictures of your wiring in the project issues

Happy building! 🎮
