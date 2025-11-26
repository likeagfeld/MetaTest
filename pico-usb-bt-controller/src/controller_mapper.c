/*
 * Controller Mapper
 * Maps various USB HID controller formats to a standard gamepad format
 */

#include "controller_mapper.h"
#include <string.h>
#include <stdio.h>

// D-pad to hat switch mapping
// Hat switch values: 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW, 8=Center
static uint8_t dpad_to_hat(bool up, bool down, bool left, bool right) {
    if (up && !left && !right) return 0;  // N
    if (up && right) return 1;             // NE
    if (right && !up && !down) return 2;   // E
    if (down && right) return 3;           // SE
    if (down && !left && !right) return 4; // S
    if (down && left) return 5;            // SW
    if (left && !up && !down) return 6;    // W
    if (up && left) return 7;              // NW
    return 8;                              // Center (no direction)
}

// Normalize a gamepad report
void controller_mapper_normalize(gamepad_report_t *report) {
    // Ensure values are within valid ranges
    // Axes should be -127 to 127
    // Triggers should be 0 to 255
    // Hat should be 0-8

    if (report->dpad > 8) {
        report->dpad = 8; // Center if invalid
    }
}

// Process raw HID report and map to standard format
void controller_mapper_process(const uint8_t *raw_report, uint8_t len, gamepad_report_t *output) {
    // Zero out the output
    memset(output, 0, sizeof(gamepad_report_t));

    // Basic validation
    if (raw_report == NULL || len == 0) {
        return;
    }

    // Try to detect controller type and map accordingly
    // This is a generic mapper that works with common HID gamepads

    // Most generic HID gamepads follow a similar format:
    // Byte 0-1: Buttons (16 bits)
    // Byte 2: Left X axis
    // Byte 3: Left Y axis
    // Byte 4: Right X axis (if present)
    // Byte 5: Right Y axis (if present)
    // Byte 6: Triggers or D-pad
    // Byte 7+: Additional data

    if (len >= 2) {
        // Map buttons (first 16 bits)
        output->buttons[0] = raw_report[0];
        output->buttons[1] = (len > 1) ? raw_report[1] : 0;
    }

    if (len >= 4) {
        // Map left stick
        // Convert from 0-255 range to -127 to 127 range
        output->left_x = (int8_t)(raw_report[2] - 128);
        output->left_y = (int8_t)(raw_report[3] - 128);
    }

    if (len >= 6) {
        // Map right stick
        output->right_x = (int8_t)(raw_report[4] - 128);
        output->right_y = (int8_t)(raw_report[5] - 128);
    }

    if (len >= 7) {
        // Triggers - some controllers send them as buttons, others as axes
        // Try to detect and map appropriately
        output->left_trigger = raw_report[6];
    }

    if (len >= 8) {
        output->right_trigger = raw_report[7];
    }

    // D-pad detection
    // Some controllers send D-pad as buttons (bits in button bytes)
    // Others send it as a hat switch (separate byte)
    if (len >= 9) {
        // Check if byte 8 looks like a hat switch (value 0-8)
        if (raw_report[8] <= 8) {
            output->dpad = raw_report[8];
        }
    }

    // If no hat switch detected, try to construct from button bits
    // Common mapping: D-pad buttons are often in the first 8 buttons
    // Button 0-3 or 12-15 are often D-pad
    if (output->dpad == 0 || output->dpad == 8) {
        // Extract potential D-pad buttons
        // Try buttons 12-15 first (common for Xbox-style controllers)
        bool up = (output->buttons[1] & 0x10) != 0;    // Bit 12
        bool down = (output->buttons[1] & 0x20) != 0;  // Bit 13
        bool left = (output->buttons[1] & 0x40) != 0;  // Bit 14
        bool right = (output->buttons[1] & 0x80) != 0; // Bit 15

        if (up || down || left || right) {
            output->dpad = dpad_to_hat(up, down, left, right);
        } else {
            // Try bits 0-3
            up = (output->buttons[0] & 0x01) != 0;
            down = (output->buttons[0] & 0x02) != 0;
            left = (output->buttons[0] & 0x04) != 0;
            right = (output->buttons[0] & 0x08) != 0;

            if (up || down || left || right) {
                output->dpad = dpad_to_hat(up, down, left, right);
            } else {
                output->dpad = 8; // Center
            }
        }
    }

    // Normalize the output
    controller_mapper_normalize(output);

    // Debug output (can be disabled in production)
    static uint32_t report_count = 0;
    if (++report_count % 100 == 0) { // Print every 100th report
        printf("Controller: Btns=%02X%02X LStick=(%d,%d) RStick=(%d,%d) Trig=(%d,%d) DPad=%d\n",
               output->buttons[1], output->buttons[0],
               output->left_x, output->left_y,
               output->right_x, output->right_y,
               output->left_trigger, output->right_trigger,
               output->dpad);
    }
}
