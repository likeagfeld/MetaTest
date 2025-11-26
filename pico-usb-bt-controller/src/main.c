/*
 * Pico USB-to-Bluetooth Controller Adapter
 *
 * Bridges wired USB controllers to Bluetooth HID for BlueRetro adapters
 * Compatible with Raspberry Pi Pico W / Pico 2 W
 */

#include <stdio.h>
#include "pico/stdlib.h"
#include "pico/cyw43_arch.h"
#include "hardware/gpio.h"
#include "usb_host.h"
#include "bt_hid.h"

// Status LED on Pico W
#define LED_PIN CYW43_WL_GPIO_LED_PIN

// Status update interval (ms)
#define STATUS_INTERVAL_MS 1000

int main() {
    // Initialize stdio for debugging
    stdio_init_all();

    printf("\n\n=== Pico USB-to-Bluetooth Controller Adapter ===\n");
    printf("Version: 1.0.0\n");
    printf("Target: BlueRetro (Sega Saturn)\n\n");

    // Initialize CYW43 wireless chip (required for Bluetooth on Pico W)
    if (cyw43_arch_init()) {
        printf("ERROR: Failed to initialize CYW43\n");
        return -1;
    }
    printf("[OK] CYW43 wireless chip initialized\n");

    // Initialize USB host for controller input
    usb_host_init();
    printf("[OK] USB host initialized\n");

    // Initialize Bluetooth HID
    bt_hid_init();
    printf("[OK] Bluetooth HID initialized\n");

    printf("\nWaiting for USB controller connection...\n");
    printf("BlueRetro pairing: Put adapter in pairing mode, then connect USB controller\n\n");

    // Main loop
    uint32_t last_status_time = 0;
    bool last_usb_connected = false;
    bool last_bt_connected = false;
    gamepad_report_t report;

    while (true) {
        // Process USB host events
        usb_host_task();

        // Check connection status
        bool usb_connected = usb_host_controller_connected();
        bool bt_connected = bt_hid_is_connected();

        // Print status updates
        uint32_t now = to_ms_since_boot(get_absolute_time());
        if (now - last_status_time >= STATUS_INTERVAL_MS) {
            if (usb_connected != last_usb_connected || bt_connected != last_bt_connected) {
                printf("Status: USB=%s BT=%s | %s\n",
                       usb_connected ? "CONNECTED" : "disconnected",
                       bt_connected ? "CONNECTED" : "disconnected",
                       bt_hid_get_status());

                last_usb_connected = usb_connected;
                last_bt_connected = bt_connected;
            }
            last_status_time = now;
        }

        // Update LED based on connection status
        if (usb_connected && bt_connected) {
            // Both connected - solid LED
            cyw43_arch_gpio_put(LED_PIN, 1);
        } else if (usb_connected || bt_connected) {
            // One connected - blink slowly
            cyw43_arch_gpio_put(LED_PIN, (now / 500) % 2);
        } else {
            // None connected - blink fast
            cyw43_arch_gpio_put(LED_PIN, (now / 250) % 2);
        }

        // Forward controller data if both connections are active
        if (usb_connected && bt_connected) {
            if (usb_host_get_report(&report)) {
                if (!bt_hid_send_report(&report)) {
                    printf("WARNING: Failed to send BT report\n");
                }
            }
        }

        // Small delay to prevent CPU hogging
        sleep_ms(1);
    }

    return 0;
}
