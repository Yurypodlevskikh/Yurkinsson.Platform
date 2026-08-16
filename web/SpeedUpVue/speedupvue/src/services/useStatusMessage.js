// src/services/useStatusMessage.js

import { ref } from 'vue'

const statusMessage = ref('')

function showStatusMessage(message, durationOrType = 3000) {
  // Backwards-compatible: callers may pass (message, number) or (message, 'success'|'error')
  let duration = 3000

  if (typeof durationOrType === 'number' && Number.isFinite(durationOrType) && durationOrType >= 0) {
    duration = durationOrType
  } else if (typeof durationOrType === 'string') {
    // a string was passed (likely a 'type' like 'success'/'error') — ignore for duration,
    // use default duration. Keep this tolerant to avoid instantly clearing messages.
    duration = 3000
  }

  statusMessage.value = message

  setTimeout(() => {
      statusMessage.value = ''
  }, duration)
}

export function useStatusMessage() {
    return {
        statusMessage,
        showStatusMessage
    }
}