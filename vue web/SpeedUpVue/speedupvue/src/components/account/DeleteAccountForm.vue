<template>
    <div class="account-form">
        <div v-if="status" class="status-message">{{ status }}</div>

        <form @submit.prevent="onSubmit" class="form-actions">
            <div class="form-group">
                <InputSpeedUp inputType="password"
                              name="currentPassword"
                              v-model="password"
                              :error="passwordError"
                              placeholderText="Enter current password" />
                <ButtonSpeedUp :btnType="'submit'" title="Request account deletion" :disabled="isSubmitting">
                    <template #icon>
                        <IconPaperPlane :class="{ spinning: isSubmitting }" />
                    </template>
                </ButtonSpeedUp>
            </div>
        </form>

        <div class="account-btn-footer">
            <ButtonSpeedUp @click="onCancel" title="Cancel">
                <template #icon>
                    <IconRotateLeft />
                </template>
            </ButtonSpeedUp>
        </div>
    </div>
</template>

<script setup>import { ref } from 'vue'
import { useStatusMessage } from '@/services/useStatusMessage'
import { startDeleteAccount } from '@/services/authService'
import store from '@/store/store.js'

import InputSpeedUp from '../InputSpeedUp.vue'
import ButtonSpeedUp from '../ButtonSpeedUp.vue'
import IconPaperPlane from '../icons/IconPaperPlane.vue'
import IconRotateLeft from '../icons/IconRotateLeft.vue'

const { showStatusMessage } = useStatusMessage()

const password = ref('')
const passwordError = ref('')
const status = ref('')
const isSubmitting = ref(false)

function onCancel() {
  // return to auth tab view
  store.commit('setActiveSettingsTab', 'auth')
  status.value = ''
  password.value = ''
  passwordError.value = ''
}

async function onSubmit() {
  passwordError.value = ''
  status.value = ''

  if (!password.value || password.value.trim().length === 0) {
    passwordError.value = 'Password is required.'
    return
  }

  isSubmitting.value = true
  try {
    const result = await startDeleteAccount(password.value.trim())
    if (result.success) {
      // Show friendly message that confirmation email is sent.
      showStatusMessage(result.message || 'Confirmation email sent. Check your inbox.')
      // Keep the form open or navigate — we keep user on auth tab and clear the password
      password.value = ''
    } else {
      // Show server-provided message
      showStatusMessage(result.message || 'Failed to request account deletion.')
    }
  } catch (err) {
    if (import.meta.env.DEV) console.error('Unexpected error requesting account deletion:', err)
    showStatusMessage('Unexpected error. Try again later.')
  } finally {
    isSubmitting.value = false
  }
}</script>

<style scoped>
    /* Reuse existing .status-message and button styles; no new global styles */
</style>