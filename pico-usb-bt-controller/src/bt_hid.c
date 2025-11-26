/*
 * Bluetooth HID Gamepad Implementation
 * Implements Bluetooth Classic HID for BlueRetro compatibility
 */

#include "bt_hid.h"
#include <stdio.h>
#include <string.h>
#include "pico/stdlib.h"
#include "pico/cyw43_arch.h"
#include "btstack.h"

// Device name visible to BlueRetro
#define DEVICE_NAME "Pico Gamepad"

// HID Descriptor for a standard gamepad
// This descriptor defines the gamepad as having:
// - 16 buttons
// - 4 analog axes (2 sticks)
// - 2 triggers
// - 1 D-pad (hat switch)
static const uint8_t hid_descriptor_gamepad[] = {
    0x05, 0x01,        // Usage Page (Generic Desktop)
    0x09, 0x05,        // Usage (Game Pad)
    0xA1, 0x01,        // Collection (Application)

    // Buttons (16 buttons)
    0x05, 0x09,        //   Usage Page (Button)
    0x19, 0x01,        //   Usage Minimum (Button 1)
    0x29, 0x10,        //   Usage Maximum (Button 16)
    0x15, 0x00,        //   Logical Minimum (0)
    0x25, 0x01,        //   Logical Maximum (1)
    0x75, 0x01,        //   Report Size (1)
    0x95, 0x10,        //   Report Count (16)
    0x81, 0x02,        //   Input (Data, Variable, Absolute)

    // Left Stick X/Y
    0x05, 0x01,        //   Usage Page (Generic Desktop)
    0x09, 0x30,        //   Usage (X)
    0x09, 0x31,        //   Usage (Y)
    0x15, 0x81,        //   Logical Minimum (-127)
    0x25, 0x7F,        //   Logical Maximum (127)
    0x75, 0x08,        //   Report Size (8)
    0x95, 0x02,        //   Report Count (2)
    0x81, 0x02,        //   Input (Data, Variable, Absolute)

    // Right Stick X/Y
    0x09, 0x33,        //   Usage (Rx)
    0x09, 0x34,        //   Usage (Ry)
    0x15, 0x81,        //   Logical Minimum (-127)
    0x25, 0x7F,        //   Logical Maximum (127)
    0x75, 0x08,        //   Report Size (8)
    0x95, 0x02,        //   Report Count (2)
    0x81, 0x02,        //   Input (Data, Variable, Absolute)

    // Triggers
    0x09, 0x32,        //   Usage (Z) - Left Trigger
    0x09, 0x35,        //   Usage (Rz) - Right Trigger
    0x15, 0x00,        //   Logical Minimum (0)
    0x25, 0xFF,        //   Logical Maximum (255)
    0x75, 0x08,        //   Report Size (8)
    0x95, 0x02,        //   Report Count (2)
    0x81, 0x02,        //   Input (Data, Variable, Absolute)

    // D-Pad (Hat Switch)
    0x09, 0x39,        //   Usage (Hat Switch)
    0x15, 0x00,        //   Logical Minimum (0)
    0x25, 0x07,        //   Logical Maximum (7)
    0x35, 0x00,        //   Physical Minimum (0)
    0x46, 0x3B, 0x01,  //   Physical Maximum (315)
    0x65, 0x14,        //   Unit (Degrees)
    0x75, 0x04,        //   Report Size (4)
    0x95, 0x01,        //   Report Count (1)
    0x81, 0x42,        //   Input (Data, Variable, Absolute, Null State)

    // Padding
    0x75, 0x04,        //   Report Size (4)
    0x95, 0x01,        //   Report Count (1)
    0x81, 0x01,        //   Input (Constant)

    0xC0               // End Collection
};

// Connection state
static struct {
    bool connected;
    uint16_t hid_cid;
    bd_addr_t device_addr;
    char status_msg[64];
} bt_state = {
    .connected = false,
    .hid_cid = 0,
    .status_msg = "Initializing..."
};

// Forward declarations
static void packet_handler(uint8_t packet_type, uint16_t channel, uint8_t *packet, uint16_t size);

// Initialize Bluetooth HID
void bt_hid_init(void) {
    // Initialize BTstack
    l2cap_init();
    sdp_init();

    // Initialize HID Device service
    hid_device_init(0, sizeof(hid_descriptor_gamepad), hid_descriptor_gamepad);

    // Set device name
    gap_set_local_name(DEVICE_NAME);

    // Set device class (Gamepad/Joystick)
    gap_set_class_of_device(0x2508); // Gamepad/Joystick

    // Make discoverable
    gap_discoverable_control(1);
    gap_set_page_scan_type(PAGE_SCAN_MODE_INTERLACED);

    // Register packet handler
    hid_device_register_packet_handler(packet_handler);

    // Turn on Bluetooth
    hci_power_control(HCI_POWER_ON);

    strcpy(bt_state.status_msg, "Ready for pairing");
}

// Send gamepad report over Bluetooth HID
bool bt_hid_send_report(const gamepad_report_t *report) {
    if (!bt_state.connected) {
        return false;
    }

    // Build HID report matching our descriptor
    uint8_t hid_report[10];

    // Buttons (16 bits / 2 bytes)
    hid_report[0] = report->buttons[0];
    hid_report[1] = report->buttons[1];

    // Left stick
    hid_report[2] = (uint8_t)report->left_x;
    hid_report[3] = (uint8_t)report->left_y;

    // Right stick
    hid_report[4] = (uint8_t)report->right_x;
    hid_report[5] = (uint8_t)report->right_y;

    // Triggers
    hid_report[6] = report->left_trigger;
    hid_report[7] = report->right_trigger;

    // D-pad and padding
    hid_report[8] = report->dpad & 0x0F;

    // Send the report
    return hid_device_send_input_report(bt_state.hid_cid, hid_report, sizeof(hid_report)) == ERROR_CODE_SUCCESS;
}

// Check if connected
bool bt_hid_is_connected(void) {
    return bt_state.connected;
}

// Start pairing mode
void bt_hid_start_pairing(void) {
    gap_discoverable_control(1);
    strcpy(bt_state.status_msg, "Pairing mode active");
}

// Get status
const char* bt_hid_get_status(void) {
    return bt_state.status_msg;
}

// BTstack packet handler
static void packet_handler(uint8_t packet_type, uint16_t channel, uint8_t *packet, uint16_t size) {
    UNUSED(channel);
    UNUSED(size);

    uint8_t status;
    bd_addr_t event_addr;

    switch (packet_type) {
        case HCI_EVENT_PACKET:
            switch (hci_event_packet_get_type(packet)) {
                case HCI_EVENT_USER_CONFIRMATION_REQUEST:
                    // Auto-accept pairing
                    printf("BT: Auto-accepting pairing request\n");
                    hci_event_user_confirmation_request_get_bd_addr(packet, event_addr);
                    gap_ssp_confirmation_response(event_addr);
                    break;

                case HCI_EVENT_HID_META:
                    switch (hci_event_hid_meta_get_subevent_code(packet)) {
                        case HID_SUBEVENT_CONNECTION_OPENED:
                            status = hid_subevent_connection_opened_get_status(packet);
                            if (status == ERROR_CODE_SUCCESS) {
                                bt_state.connected = true;
                                bt_state.hid_cid = hid_subevent_connection_opened_get_hid_cid(packet);
                                hid_subevent_connection_opened_get_bd_addr(packet, bt_state.device_addr);
                                printf("BT: Connected to %02X:%02X:%02X:%02X:%02X:%02X\n",
                                       bt_state.device_addr[0], bt_state.device_addr[1],
                                       bt_state.device_addr[2], bt_state.device_addr[3],
                                       bt_state.device_addr[4], bt_state.device_addr[5]);
                                strcpy(bt_state.status_msg, "Connected to BlueRetro");
                            } else {
                                printf("BT: Connection failed, status 0x%02x\n", status);
                                strcpy(bt_state.status_msg, "Connection failed");
                            }
                            break;

                        case HID_SUBEVENT_CONNECTION_CLOSED:
                            bt_state.connected = false;
                            printf("BT: Disconnected\n");
                            strcpy(bt_state.status_msg, "Disconnected");
                            // Re-enable pairing
                            gap_discoverable_control(1);
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
            break;

        default:
            break;
    }
}
