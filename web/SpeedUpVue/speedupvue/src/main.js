import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import store from './store/store.js'
import { confirmEmail } from '@/services/authService.js' // existing
import { openAuthTab } from '@/services/authService.js'

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
            store.commit('setPendingConfirmation', { success: result.success, message: result.message, errorCode: result.errorCode, userId: result.userId })

            if (result.success) {
                store.commit('setActiveSettingsTab', 'auth');
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

function handleResetPasswordFromUrl() {
    try {
        const params = new URLSearchParams(window.location.search)
        const token = params.get('token')
        const email = params.get('email')
        if (token && email && window.location.pathname && window.location.pathname.toLowerCase().includes('reset-password')) {
            // open auth tab so user can use ResetPasswordForm
            store.commit('openSettingsPanelWithTab', 'auth')
            // store pending reset details
            store.commit('setPendingReset', { email, token })
            // remove sensitive params from URL
            params.delete('token')
            params.delete('email')
            const newUrl = window.location.pathname + (params.toString() ? `?${params.toString()}` : '')
            history.replaceState(null, '', newUrl)
        }
    } catch (err) {
        if(import.meta.env.DEV) console.error('Error processing reset-password URL:', err)
    }
}

handleEmailConfirmationFromUrl()
handleResetPasswordFromUrl()

const app = createApp(App);

app.use(store);

app.mount('#app')
