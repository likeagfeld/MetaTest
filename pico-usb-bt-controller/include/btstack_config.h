#ifndef BTSTACK_CONFIG_H
#define BTSTACK_CONFIG_H

// BTstack configuration for Pico W Bluetooth HID Gamepad

// Port related features
#define HAVE_EMBEDDED_TIME_MS

// BTstack configuration
#define ENABLE_CLASSIC
#define ENABLE_HID_DEVICE

// BTstack features that can be enabled
#define ENABLE_LOG_INFO
#define ENABLE_LOG_ERROR

// Memory configuration
#define MAX_NR_WHITELIST_ENTRIES 1
#define MAX_NR_HCI_CONNECTIONS 1
#define MAX_NR_L2CAP_SERVICES 2
#define MAX_NR_L2CAP_CHANNELS 2
#define MAX_NR_RFCOMM_MULTIPLEXERS 0
#define MAX_NR_RFCOMM_SERVICES 0
#define MAX_NR_RFCOMM_CHANNELS 0
#define MAX_NR_BTSTACK_LINK_KEY_DB_MEMORY_ENTRIES 1

// SDP configuration
#define MAX_NR_SDP_SERVICE_RECORDS 1

// HID configuration
#define ENABLE_HID_DEVICE_REPORT_PROTOCOL

#endif // BTSTACK_CONFIG_H
