/*
 * USB Host Implementation
 * Handles USB HID gamepad input using TinyUSB host stack
 */

#include "usb_host.h"
#include "controller_mapper.h"
#include <stdio.h>
#include <string.h>
#include "tusb.h"
#include "pico/stdlib.h"

// Maximum number of controllers
#define MAX_CONTROLLERS 1

// State tracking
static struct {
    bool connected;
    uint8_t dev_addr;
    uint8_t instance;
    gamepad_report_t last_report;
    bool report_available;
} controller_state = {0};

// USB Host initialization
void usb_host_init(void) {
    // TinyUSB host stack will be initialized by tusb_init()
    tusb_init();
    memset(&controller_state, 0, sizeof(controller_state));
}

// Process USB host events
void usb_host_task(void) {
    tuh_task();
}

// Get the latest gamepad report
bool usb_host_get_report(gamepad_report_t *report) {
    if (!controller_state.report_available) {
        return false;
    }

    memcpy(report, &controller_state.last_report, sizeof(gamepad_report_t));
    controller_state.report_available = false;
    return true;
}

// Check if controller is connected
bool usb_host_controller_connected(void) {
    return controller_state.connected;
}

//--------------------------------------------------------------------+
// TinyUSB Callbacks
//--------------------------------------------------------------------+

// Invoked when device with hid interface is mounted
void tuh_hid_mount_cb(uint8_t dev_addr, uint8_t instance, uint8_t const* desc_report, uint16_t desc_len) {
    (void)desc_report;
    (void)desc_len;

    printf("HID device mounted: addr=%d, instance=%d\n", dev_addr, instance);

    // Check if this is a gamepad
    uint8_t const itf_protocol = tuh_hid_interface_protocol(dev_addr, instance);

    if (itf_protocol == HID_ITF_PROTOCOL_NONE) {
        printf("Device protocol: Generic HID (will attempt gamepad mapping)\n");
    } else if (itf_protocol == HID_ITF_PROTOCOL_KEYBOARD) {
        printf("Device protocol: Keyboard (not supported)\n");
        return;
    } else if (itf_protocol == HID_ITF_PROTOCOL_MOUSE) {
        printf("Device protocol: Mouse (not supported)\n");
        return;
    }

    controller_state.connected = true;
    controller_state.dev_addr = dev_addr;
    controller_state.instance = instance;

    // Request to receive report
    if (!tuh_hid_receive_report(dev_addr, instance)) {
        printf("ERROR: Failed to request report\n");
    }
}

// Invoked when device with hid interface is unmounted
void tuh_hid_umount_cb(uint8_t dev_addr, uint8_t instance) {
    printf("HID device unmounted: addr=%d, instance=%d\n", dev_addr, instance);

    if (controller_state.dev_addr == dev_addr &&
        controller_state.instance == instance) {
        controller_state.connected = false;
        controller_state.report_available = false;
    }
}

// Invoked when received report from device via interrupt endpoint
void tuh_hid_report_received_cb(uint8_t dev_addr, uint8_t instance, uint8_t const* report, uint16_t len) {
    if (controller_state.dev_addr == dev_addr &&
        controller_state.instance == instance) {

        // Process and map the raw HID report to our standard format
        controller_mapper_process(report, len, &controller_state.last_report);
        controller_state.report_available = true;

        // Continue to request to receive report
        tuh_hid_receive_report(dev_addr, instance);
    }
}

//--------------------------------------------------------------------+
// USB Host Configuration
//--------------------------------------------------------------------+

// USB Host configuration for Pico using PIO-USB
// This requires specific PIO pin configuration

// Note: For USB host on Pico, you typically need:
// - GPIO 0/1 for D+/D- or use PIO-USB on other pins
// - VBUS power on pin 24 (or external power)
// See Pico-PIO-USB documentation for detailed pin configuration
