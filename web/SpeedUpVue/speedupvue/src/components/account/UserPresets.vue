<template>
    <!-- Delete Account (collapsed by default) -->
    <div class="delete-account-section">
        <div v-if="deleteAccountOpen" class="">
            <!-- Delete account UI -->
            <DeleteAccountForm />
        </div>
    </div>
    <table class="table">
        <thead>
            <tr>
                <th></th>
                <th></th>
                <th></th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td colspan="2">
                    <h3>{{ nickname }}'s Presets</h3>
                </td>
                <td class="desc-cell">
                    <SpeedUpButton @click="onLogout" title="Logout">
                        <template #icon>
                            <IconLogout />
                        </template>
                    </SpeedUpButton>
                </td>
                <td class="desc-cell">
                    <SpeedUpButton @click="toggleDeleteAccount" title="Delete Account" aria-expanded="deleteAccountOpen">
                        <template #icon>
                            <IconPersonCircleMinus />
                        </template>
                    </SpeedUpButton>
                </td>
            </tr>
            <tr v-for="(preset, index) in presets" :key="preset.id">
                <td>{{ index + 1 }}</td>
                <td @click="loadPreset(preset.id)" title="Load" class="title-cell">{{ preset.title }}</td>
                <td class="desc-cell">
                    <SpeedUpButton @click="showDescription(preset.description)" title="Description">
                        <template #icon>
                            <IconReadMe />
                        </template>
                    </SpeedUpButton>
                </td>
                <td class="action-cell">
                    <SpeedUpButton @click="deletePreset(preset.id)" title="Delete">
                        <template #icon>
                            <IconTrash />
                        </template>
                    </SpeedUpButton>
                </td>
            </tr>
            <tr v-if="!isVisible">
                <td colspan="4" style="padding-top: 1rem;">
                    <div class="status-message">
                        You don't have any saved settings yet.
                    </div>
                </td>
            </tr>
        </tbody>
    </table>

    <div v-if="activeDescription" class="alert alert-info mt-3">
        <strong>Description:</strong> {{ activeDescription }}
    </div>
</template>

<script setup>
    import { computed, ref, onMounted } from 'vue'
    import { useStore } from 'vuex'
    import metronomeSettingsService from '@/services/metronomeSettingsService'
    import { useStatusMessage } from '@/services/useStatusMessage'

    import SpeedUpButton from '@/components/ButtonSpeedUp.vue'
    import IconReadMe from '@/components/icons/IconReadMe.vue';
    import IconTrash from '@/components/icons/IconTrash.vue';
    import IconLogout from '@/components/icons/IconLogout.vue';
    import IconPersonCircleMinus from '@/components/icons/IconPersonCircleMinus.vue';
    import { logoutUser } from '@/services/authService'
    import { openAuthTab } from '@/services/authService'
    import DeleteAccountForm from '@/components/account/DeleteAccountForm.vue'

    const store = useStore()
    const { showStatusMessage } = useStatusMessage()
    const presets = ref([])
    const activeDescription = ref(null)
    const isVisible = ref(false)
    const deleteAccountOpen = ref(false)

    // fetch presets on mount
    const fetchPresets = async () => {
        const cachedPresets = localStorage.getItem('userPresets')
        const isAuthenticated = store.getters.isAuthenticated

        if (cachedPresets) {
            try {
                const parsed = JSON.parse(cachedPresets)

                // If cache is a valid non-empty array and user is NOT authenticated,
                // keep using the cache (preserve existing offline/anonymous behavior).
                if (Array.isArray(parsed) && parsed.length > 0 && !isAuthenticated) {
                    presets.value = parsed
                    isVisible.value = parsed.length > 0
                    return
                }

                // Remove bad cache (non-array) so we can fetch fresh data
                if (!Array.isArray(parsed)) {
                    localStorage.removeItem('userPresets')
                }
                // If parsed is an empty array, fall through and fetch from server
            } catch (e) {
                // Corrupt JSON — remove and continue to server fetch
                localStorage.removeItem('userPresets')
            }
        }

        // Fetch from server (used when authenticated, or when cache absent/empty/corrupt)
        try {
            const data = await metronomeSettingsService.getUserPresets()

            // Ensure server result becomes source of truth (even if empty)
            if (!Array.isArray(data) || data.length === 0) {
                presets.value = []
                isVisible.value = false
                localStorage.setItem('userPresets', JSON.stringify([]))
                return
            }

            presets.value = data
            isVisible.value = data.length > 0
            // Update cache to match server data
            localStorage.setItem('userPresets', JSON.stringify(data))
        } catch (error) {
            if (import.meta.env.DEV) {
                console.error('Error fetching presets: ', error)
            }

            // On network error, fall back to any non-empty cache if present
            const fallback = localStorage.getItem('userPresets')
            if (fallback) {
                try {
                    const parsedFallback = JSON.parse(fallback)
                    if (Array.isArray(parsedFallback) && parsedFallback.length > 0) {
                        presets.value = parsedFallback
                        isVisible.value = parsedFallback.length > 0
                        return
                    }
                } catch { /* ignore */ }
            }

            // Otherwise show empty state
            presets.value = []
            isVisible.value = false
        }
    }

    onMounted(() => {
        fetchPresets()
    })

    // methods
    const showDescription = (description) => {
        // Reuse existing status UI to show the description unobtrusively
        showStatusMessage(description || 'No description provided.')
    }

    const loadPreset = async (id) => {
        // Wire to store action for future implementation; currently keeps UI ready
        const success = await store.dispatch('loadPresetById', id)
        if (!success) {
            showStatusMessage('Failed to load preset. Please try again later.', 'error')
        }
    }

    const deletePreset = async (id) => {
        if (!confirm(`Are you sure you want to delete preset with id: ${id}?`)) return;

        try {
            const result = await metronomeSettingsService.deleteSettings(id)

            if (result) {
                // Refreshing from server
                //// Refreshing the list of presets after successful deletion
                //const updatedPresets = await metronomeSettingsService.getUserPresets()
                //presets.value = updatedPresets

                // Deleting from the local array
                presets.value = presets.value.filter(p => p.id !== id)
                localStorage.setItem('userPresets', JSON.stringify(presets.value))
                isVisible.value = presets.value.length > 0
            } else {
                if (import.meta.env.DEV) {
                    console.warn(`Preset with id: ${id} was not found or could not be deleted.`)
                } else {
                    showStatusMessage('Preset with was not found or could not be deleted.')
                }
            }
        } catch (error) {
            if (import.meta.env.DEV) {
                console.error('Error deleting preset: ', error)
            }
        }
    }

    const nickname = computed(() => store.getters.getNickname)

    // Logout handler exposed in the user panel
    const onLogout = async () => {
        try {
            const result = await logoutUser()
            if (result.success) {
                // Do not show a "logged out" status message.
                // Ensure reset flow is cleared and open the auth tab (shows Login/Register).
                store.commit('clearPendingReset')
                openAuthTab()
            } else {
                showStatusMessage(result.message || 'Logout failed.', 'error')
            }
        } catch (err) {
            if (import.meta.env.DEV) console.error('Logout error:', err)
            showStatusMessage('Unexpected error during logout.', 'error')
        }
    }

    // Toggle demo section
    function toggleDeleteAccount() {
        deleteAccountOpen.value = !deleteAccountOpen.value
    }
</script>

<style scoped>
    .presets-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.5rem;
        margin-bottom: 0.5rem;
    }

        .presets-header h2 {
            margin: 0;
            font-size: 1.25rem;
        }

    .logout-btn {
        /* reuse ButtonSpeedUp styling; minimal alignment tweak */
        display: inline-flex;
        align-items: center;
    }

    .table {
        width: 100%;
        border-collapse: collapse;
    }

        .table td {
            border-bottom: 1px solid var(--color-box-shadow);
            padding: 0.2rem 0.4rem;
            text-align: left;
        }

        .table th {
            background: var(--color-btn-back);
            color: var(--color-background-mute-before);
        }

    .title-cell {
        cursor: pointer;
        color: var(--color-box-shadowt);
        text-decoration: underline;
    }

        .title-cell:hover {
            color: var(--color-btn-text);
        }

    .action-cell, .desc-cell {
        width: 30px;
        text-align: center;
    }
</style>