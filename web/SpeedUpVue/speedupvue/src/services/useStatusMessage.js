// src/services/useStatusMessage.js

import { ref } from 'vue'

const statusMessage = ref('')

function showStatusMessage(message, duration = 3000) {
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