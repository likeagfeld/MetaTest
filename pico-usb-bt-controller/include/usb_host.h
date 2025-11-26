#ifndef USB_HOST_H
#define USB_HOST_H

#include <stdint.h>
#include <stdbool.h>

// USB HID Gamepad Report Structure
typedef struct {
    uint8_t buttons[2];     // 16 buttons max
    int8_t left_x;          // Left stick X axis
    int8_t left_y;          // Left stick Y axis
    int8_t right_x;         // Right stick X axis
    int8_t right_y;         // Right stick Y axis
    uint8_t left_trigger;   // Left trigger
    uint8_t right_trigger;  // Right trigger
    uint8_t dpad;           // D-pad (hat switch)
} gamepad_report_t;

// Initialize USB host
void usb_host_init(void);

// Task to process USB host events
void usb_host_task(void);

// Get the latest gamepad report
bool usb_host_get_report(gamepad_report_t *report);

// Check if a controller is connected
bool usb_host_controller_connected(void);

#endif // USB_HOST_H
