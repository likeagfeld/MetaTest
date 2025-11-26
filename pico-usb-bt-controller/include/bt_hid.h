#ifndef BT_HID_H
#define BT_HID_H

#include <stdint.h>
#include <stdbool.h>
#include "usb_host.h"

// Initialize Bluetooth HID
void bt_hid_init(void);

// Send gamepad report over Bluetooth HID
bool bt_hid_send_report(const gamepad_report_t *report);

// Check if Bluetooth is connected to BlueRetro
bool bt_hid_is_connected(void);

// Make device discoverable for pairing
void bt_hid_start_pairing(void);

// Get connection status string
const char* bt_hid_get_status(void);

#endif // BT_HID_H
