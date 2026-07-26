import httpClient from '@/services/httpClient'
import store from '../store/store.js'

// Authenticate user
export async function authenticateUser(credentials) {
    try {
        const response = await httpClient.post('api/authenticate', credentials)

        if (!response.data.isSuccess) {
            // Return failure info to component
            return { success: false, message: response.data.message }
        }

        // Save token to the store and to the localStorage if authentication succeeded
        store.commit('setToken', response.data.jwtToken)
        store.commit('setRefreshToken', response.data.refreshToken)

        // Return success info with token and additional data
        return {
            success: true,
            token: response.data.jwtToken,
            hue: response.data.hueDegrees
        }
    } catch (error) {
        // Handle network or unexpected errors
        if (import.meta.env.DEV) {
            console.error('Authentication error:', error)
        }
        
        return {success:false,message: 'Unexpected error occurred.Please try again.'}
    }
}

// Register user
export async function registerUser(credentials) {
    try {
        const response = await httpClient.post('api/register', credentials)

        return {
            success: response.data.success,
            message: response.data.message,
            statusCode: response.status
        }
    } catch (error) {
        if (import.meta.env.DEV) {
            console.error('Registration error:', error)
        }

        // If server returned a response payload
        if (error.response) {
            const data = error.response.data

            return {
                success: data?.success ?? false,
                message: data?.message ?? 'Registration failed. Please try again.',
                statusCode: error.response.status
            }
        }

        // Network/ server down / CORS
        return {
            success: false,
            message: 'Server is unreachable. Please try again later.',
            statusCode: 0
        }
    }
}

export async function confirmEmail({ userId, token }) {
    try {
        const response = await httpClient.post('api/confirm-email', { userId, token });
        return { success: true, ...response.data };
    } catch (err) {
        if (err.response && err.response.data) return { success: false, ...err.response.data };
        return { success: false, message: 'Unable to confirm email. Try again later.' };
    }
}

export async function resendConfirmation(userId) {
    try {
        const response = await httpClient.post('api/resend-confirmation', { userId });
        return { success: true, ...response.data };
    } catch (err) {
        if (err.response && err.response.data) return { success: false, ...err.response.data };
        return { success: false, message: 'Unable to resend confirmation. Try again later.' };
    }
}

export async function logoutUser() {
    try {
        const token = store.state.token
        
        if (!token) {
            return { success: false, message: 'No user is currently logged in.' }
        }

        const response = await httpClient.post('api/logout', null, {
            headers: { Authorization: `Bearer ${token}` }
        })

        if (response.status === 200) {
            // Clear tokens from store and localStorage
            store.commit('clearAuthData')
            return { success: true, message: response.data }
        } else {
            return { success: false, message: 'Logout failed. Please try again.' }
        }
    } catch (error) {
        if (import.meta.env.DEV) {
            console.error('Logout error:', error)
        }
        
        return { success: false, message: 'Unexpected error occurred. Please try again.' }
    }
}

export function openAuthTab() {
    store.commit('openSettingsPanelWithTab', 'auth')
}