import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import store from './store/store.js'
import { confirmEmail } from '@/services/authService.js' // implement confirmEmail to call your backend API for email confirmation

const token = localStorage.getItem('authToken')
if(token) {
    store.commit('setToken', token)
}

async function handleEmailConfirmationFromUrl() {
    try {
        const params = new URLSearchParams(window.location.search)
        const userId = params.get('userId')
        const token = params.get('token')

        if (userId && token /* optional: && client === 'SpeedUpVue' */) {
            // Open auth tab so user sees result
            store.commit('openSettingsPanelWithTab', 'auth')

            // Call MiniAPI confirm endpoint
            const result = await confirmEmail({ userId, token })

            // Store the result so AccountForm can display it
            store.commit('setPendingConfirmation', { success: result.success, message: result.message })

            if (result.success) {
                // on success switch to login tab
                store.commit('setActiveSettingsTab', 'auth'); // keep auth tab open
                // set to login view in AccountForm via store or event
                // Optionally prefill email if available: use params or result data
            } else {
                // keep auth tab open and show error message; UI can show resend button if errorCode === 'TOKEN_EXPIRED'
            }

            // Remove sensitive params from URL to avoid re-processing and token exposure
            params.delete('userId')
            params.delete('token')
            params.delete('client')
            const newUrl = window.location.pathname + (params.toString() ? `?${params.toString()}` : '')
            history.replaceState(null, '', newUrl)
        }
    } catch (err) {
        if(import.meta.env.DEV) console.error('Error processing confirmation URL:', err)
    }
}

handleEmailConfirmationFromUrl()

const app = createApp(App);

app.use(store);

app.mount('#app')
