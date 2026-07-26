import httpClient from '@/services/httpClient'

export default {
    /**
     * Save new settings for the metronome. (Save As)
     * @param {Object} settingsDto - The metronome settings to save.
     * @returns {Promise} The metronome settings.
     */

    async saveAsSettings(settingsDto) {
        try {
            const response = await httpClient.post('api/create-metronome-settings', settingsDto)
            return response.data
        } catch (error) {
            if (import.meta.env.DEV) {
                // Log error in development mode
                console.error('Error saving metronome settings:', error)
            }
            
            throw error
        }
    },

    /**
     * Get all user presets for the metronome.
     * @returns {Promise} An array of user presets.
     * */
    async getUserPresets() {
        try {
            const response = await httpClient.get('api/get-user-presets')
            return response.data
        } catch (error) {
            if (import.meta.env.DEV) {
                // Log error in development mode
                console.error('Error fetching user presets:', error)
            }
            
            throw error
        }
    },

    /**
     * Get user metronome settings (preset) by ID.
     * @param {number} id - The ID of the metronome settings.
     * @returns {Promise} The metronome settings.
     */
    async getUserPresetById(id) {
        try {
            const response = await httpClient.get(`api/get-user-preset-by-id/${id}`)
            return response.data
        } catch (error) {
            if (import.meta.env.DEV) {
                // Log error in development mode
                console.error('Error fetching user preset by ID:', error)
            }
            
            throw error
        }
    },

    /**
     * Update existing metronome settings by ID.
     * @param {number} id - The ID of the metronome settings to update.
     * @param {Object} settingsDto - The updated metronome settings.
     * @returns {Promise} The updated metronome settings.
     * */
    async updateSettings(id, settingsDto) {
        try {
            const response = await httpClient.put(`api/update-metronome-settings/${id}`, settingsDto)
            return response.data
        } catch (error) {
            if (import.meta.env.DEV) {
                console.error('Error updating metronome settings:', error)
            }
            
            throw error
        }
    },

    /**
     * Delete a metronome settings preset by ID.
     * @param {number} id - The ID of the metronome settings to delete.
     * @returns {Promise} The response from the server.
     */
    async deleteSettings(id) {
        try {
            const response = await httpClient.delete(`api/delete-metronome-settings/${id}`)
            return response.data
        } catch (error) {
            if (import.meta.env.DEV) {
                console.error('Error deleting metronome settings:', error);
            }
            
            throw error
        }
    }
}