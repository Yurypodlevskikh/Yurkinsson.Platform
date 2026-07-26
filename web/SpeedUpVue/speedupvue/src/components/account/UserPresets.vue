<template>
    <h2>{{ nickname }}'s Presets</h2>
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
        </tbody>
    </table>

    <div v-if="activeDescription" class="alert alert-info mt-3">
        <strong>Description:</strong> {{ activeDescription }}
    </div>
    <!--<div v-if="token">
        <p><strong>Your token:</strong></p>
        <pre>{{token}}</pre>
    </div>
    <div v-else>
        <p>No token found. Please log in first.</p>
    </div>-->
</template>

<script setup>
    import { computed, ref, onMounted }from 'vue'
    import { useStore } from 'vuex'
    import metronomeSettingsService from '@/services/metronomeSettingsService'
    import { useStatusMessage } from '@/services/useStatusMessage'

    import SpeedUpButton from '@/components/ButtonSpeedUp.vue'
    import IconReadMe from '@/components/icons/IconReadMe.vue';
    import IconTrash from '@/components/icons/IconTrash.vue';

    const store = useStore()
    const { showStatusMessage } = useStatusMessage()
    const presets = ref([])
    const activeDescription = ref(null)
    const isVisible = ref(false)

    // fetch presets on mount
    const fetchPresets = async () => {
        const cachedPresets = localStorage.getItem('userPresets')
        if (cachedPresets){
            presets.value = JSON.parse(cachedPresets)
            if (cachedPresets.length > 0) isVisible.value = true
            if (import.meta.env.DEV) {
                console.log("Cached presets is sown")
            }
        }else {
            try {
                const data = await metronomeSettingsService.getUserPresets()

                if (!Array.isArray(data)) {
                    if (import.meta.env.DEV) {
                        console.error('Invalid data format received for presets:', data);
                    }
                    return;
                }

                presets.value = data
                if (data.length > 0) isVisible.value = true
            } catch (error) {
                if (import.meta.env.DEV) {
                    console.error('Error fetching presets: ', error)
                }
            }
        }
    }

    onMounted(() => {
        fetchPresets()
    })

    // methods
    const showDescription = (description) => {
        //alert(description || 'No description provided.');
        showStatusMessage(description || 'No description provided.')
    }

    const loadPreset = async (id) => {
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

    //// Computed token value from Vuex store
    //const token = computed(() => store.getters.getToken)
    const nickname = computed(() => store.getters.getNickname)
</script>

<style scoped>
    .table{
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
    .title-cell:hover{
        color: var(--color-btn-text);
    }
    .action-cell, .desc-cell {
        width: 30px;
        text-align: center;
    }
</style>