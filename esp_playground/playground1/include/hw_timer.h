#ifndef HW_TIMER_H
#define HW_TIMER_H

#include "driver/timer.h"
#include "esp_log.h"
#include "pcnt_lib.h"
#include "TLBFISLib.h"

// Declare the function if it's defined elsewhere
void drawSpeedFromPulses(int16_t pulses);
int pcnt_get(int16_t *count);
void pcnt_clear(void);
void pcnt_init_and_start(void);

#define TIMER_GROUP TIMER_GROUP_0 // Use hardware timer group 0
#define TIMER_INDEX TIMER_0       // Use timer 0 in the group
#define TIMER_INTERVAL_SEC 1.0    // Timer interval in seconds
#define QUEUE_LENGTH 10         // Length of the queue for pulse counts

static const char *TAG = "HW_TIMER";

static QueueHandle_t pulseQueue = NULL; // Queue to hold pulse counts

// Timer ISR callback
bool IRAM_ATTR timer_isr_callback(void *arg) {
    int16_t count = 0;

    // Get the current counter value
    pcnt_get(&count);

    // Send pulse count to the queue
    if (pulseQueue != NULL) {
        BaseType_t xHigherPriorityTaskWoken = pdFALSE;
        xQueueSendFromISR(pulseQueue, &count, &xHigherPriorityTaskWoken);

        // Yield to a higher-priority task if necessary
        if (xHigherPriorityTaskWoken == pdTRUE) {
            portYIELD_FROM_ISR();
        }
    }
    

    // Log the pulse count
    //ESP_EARLY_LOGI(TAG, "Pulse count: %d", count);

    // Optional: Clear the counter if needed
    pcnt_clear();

    // Re-enable the timer alarm
    timer_group_clr_intr_status_in_isr(TIMER_GROUP, TIMER_INDEX);
    timer_group_enable_alarm_in_isr(TIMER_GROUP, TIMER_INDEX);

    return true; // Indicate successful ISR execution
}

void pulse_processing_task(void *pvParameters) {
    int16_t count;

    while (true) 
    {
        if (xQueueReceive(pulseQueue, &count, portMAX_DELAY))
        {
            drawSpeedFromPulses(count); // Update the display with the pulse count
        }
    }
    
}

// Initialize the hardware timer
void init_hw_timer() {

    // Create the queue
    pulseQueue = xQueueCreate(QUEUE_LENGTH, sizeof(int16_t));
    if (pulseQueue == NULL) {
        ESP_LOGE(TAG, "Failed to create pulse queue.");
        return;
    }

    // Create the pulse processing task
    xTaskCreate(pulse_processing_task, "Pulse Processing Task", 2048, NULL, 5, NULL);

    // Configure the timer
    timer_config_t config = {
        .alarm_en = TIMER_ALARM_EN,
        .counter_en = TIMER_PAUSE,
        .counter_dir = TIMER_COUNT_UP,
        .auto_reload = TIMER_AUTORELOAD_EN,
        .divider = 80, // Timer clock divider (80 MHz / 80 = 1 MHz)
    };
    timer_init(TIMER_GROUP, TIMER_INDEX, &config);

    // Set the timer's alarm value
    timer_set_counter_value(TIMER_GROUP, TIMER_INDEX, 0);
    timer_set_alarm_value(TIMER_GROUP, TIMER_INDEX, TIMER_INTERVAL_SEC * 1000000); // Convert seconds to microseconds
    timer_enable_intr(TIMER_GROUP, TIMER_INDEX);

    // Attach the ISR
    timer_isr_callback_add(TIMER_GROUP, TIMER_INDEX, timer_isr_callback, NULL, 0);

    // Start the timer
    timer_start(TIMER_GROUP, TIMER_INDEX);

    ESP_LOGI(TAG, "Hardware timer initialized with interval: %.2f seconds", TIMER_INTERVAL_SEC);
}

#endif // HW_TIMER_H