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

/*
 * New: forgotPassword
 * - POSTs to BFF /api/forgot-password (body: { email })
 * - BFF will construct ClientUri and forward to Identity
 */
export async function forgotPassword(email) {
    try {
        const response = await httpClient.post('api/forgot-password', { email })
        // identity endpoints return a simple message string on success
        return {
            success: response.status >= 200 && response.status < 300,
            message: response.data || 'If an account exists for this email, a reset link was sent.'
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('Forgot password error:', err)
        if (err.response && err.response.data) {
            // return safe message from backend if available
            return { success: false, message: err.response.data || 'Failed to request password reset.' }
        }
        return { success: false, message: 'Unable to request password reset. Try again later.' }
    }
}

/*
 * New: resetPassword
 * - POSTs to BFF /api/reset-password (body: { email, token, password })
 * - BFF forwards to Identity which validates token & sets password.
 */
export async function resetPassword(model) {
    // model should be { email, token, password }
    try {
        const response = await httpClient.post('api/reset-password', model)
        return {
            success: response.status >= 200 && response.status < 300,
            message: response.data || 'Password has been reset.'
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('Reset password error:', err)
        if (err.response && err.response.data) {
            return { success: false, message: err.response.data || 'Failed to reset password.' }
        }
        return { success: false, message: 'Unable to reset password. Try again later.' }
    }
}