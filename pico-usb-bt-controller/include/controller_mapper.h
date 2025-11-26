#ifndef CONTROLLER_MAPPER_H
#define CONTROLLER_MAPPER_H

#include <stdint.h>
#include "usb_host.h"

// Normalize USB HID report to standard gamepad format
void controller_mapper_normalize(gamepad_report_t *report);

// Map different controller types to standard format
void controller_mapper_process(const uint8_t *raw_report, uint8_t len, gamepad_report_t *output);

#endif // CONTROLLER_MAPPER_H
