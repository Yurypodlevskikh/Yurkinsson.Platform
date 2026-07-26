<template>
    <div :class="['settings_panel', {settings_panel_show: isOpenSettingsPanel}]">
        <div v-if="statusMessage" class="status-message">
            {{statusMessage}}
        </div>
        <div class="bar-settings-panel">
            <div v-if="activeTab === 'settings'">
                <div class="bars-panel">
                    <!-- Manage Bars and Repeats by inputs START -->
                    <BarsPanel />
                    <!-- Manage Bars and Repeats by inputs END -->
                </div>
                <!-- Manage BPM and Repeats by inputs START -->
                <div class="tempo-settings">
                    <SourceTempo />
                </div>
                <div class="tempo-settings">
                    <TargetTempo />
                </div>
                <!-- Manage BPM and Repeats by inputs END -->
                <div class="bar-settings-footer">
                    <SpeedUpButton @click="setDefaultSettings" title="Set default settings"
                                   :disabled="!isSettingsChanged">
                        <template #icon>
                            <BroomIcon />
                        </template>
                    </SpeedUpButton>
                    <SpeedUpButton v-show="isAuthenticated" @click="handleSaveAs" title="Save As"
                                   :disabled="!isSettingsChanged">
                        <template #icon>
                            <IconSaveAs />
                        </template>
                    </SpeedUpButton>
                    <SpeedUpButton v-show="isSavedSettingsId" @click="saveSettings" title="Save"
                                   :disabled="!isSettingsChanged">
                        <template #icon>
                            <IconSave />
                        </template>
                    </SpeedUpButton>
                </div>
            </div>

            <div v-if="activeTab === 'auth'">
                <AccountForm />
            </div>

            <div v-if="activeTab === 'presets'">
                <UserPresets />
            </div>

            <div v-if="activeTab === 'saveSettingsAs'">
                <SaveAsSettingsForm />
            </div>
        </div>
    </div>
</template>

<script setup>
    import { computed, ref } from 'vue'
    import { useStore } from 'vuex'
    import { useStatusMessage } from '@/services/useStatusMessage'

    import BarsPanel from '@/components/BarsPanel.vue'
    import SourceTempo from '@/components/SourceTempoSettings.vue'
    import TargetTempo from '@/components/TargetTempoSettings.vue'
    import SpeedUpButton from '@/components/ButtonSpeedUp.vue'
    import IconSave from '@/components/icons/IconSave.vue';
    import IconSaveAs from '@/components/icons/IconSaveAs.vue';
    import BroomIcon from '@/components/icons/IconBroom.vue';
    import MinusIcon from '@/components/icons/IconMinus.vue'
    import PlusIcon from '@/components/icons/IconPlus.vue'
    import GearIcon from '@/components/icons/IconGear.vue'
    import AccountForm from '@/components/account/AccountForm.vue'
    import UserPresets from '@/components/account/UserPresets.vue'
    import SaveAsSettingsForm from '@/components/account/SaveAsSettingsForm.vue'
    import metronomeSettingsService from '@/services/metronomeSettingsService'

    const { statusMessage, showStatusMessage } = useStatusMessage()
    // Access the store
    const store = useStore()

    // Computed values from the store
    const isOpenSettingsPanel = computed(() => store.getters.getSettingsPanel)
    const activeTab = computed(() => store.getters.getActiveSettingsTab)
    const isSettingsChanged = computed(() => store.getters.isSettingsChanged)
    const isAuthenticated = computed(() => !!store.state.token)
    const isSavedSettingsId = computed(() => !!store.state.savedSettingsId)

    // Methods
    function setDefaultSettings() {
        store.dispatch('setDefaultSettings')
    }

    const handleSaveAs = () => {
        store.commit('openSettingsPanelWithTab', 'saveSettingsAs')
    }

    async function saveSettings() { 
        const id = store.getters.getSavedSettingsId
        
        if (!id) return

        const settingsDto = {
            sourceBpm: store.state.sourceBpm,
            targetBpm: store.state.targetBpm,
            barsPerPattern: store.state.barsPerPattern,
            tempoPeriods: store.state.tempoPeriods,
            isReversed: store.state.isReverse,
            isRepeatPattern: store.state.isRepeatPattern,
            beatsPerBar: store.state.beatsPerBar,
            isTempoIncreace: store.state.isTempoIncreace,
            userId: store.state.userId,
            metronomeCollectionId: null
        }

        try {
            await metronomeSettingsService.updateSettings(id, settingsDto)
            showStatusMessage('Settings updated successfully!')
        } catch (error) {
            if (import.meta.env.DEV) {
                console.error("Failed to update settigs: ", error)
            }
            showStatusMessage('Error updating the preset.')
        }
    }
</script>

<style scoped>
    .status-message {
        background-color: var(--color-box-shadow);
        color: var(--color-btn-text);
        padding: 0.3rem .3rem;
        border-radius: 8px;
        margin-bottom: .3em;
        text-align: center;
    }
    .settings_panel {
        background: var(--color-btn-back);
        border-radius: 14px;
        margin-top: 3.5rem;
        display: grid;
        /*box-shadow: 0px 0px 10px 2px var(--color-box-shadow);
        grid-template-rows: 1fr;
        margin-bottom: 1.4em;
        padding: .6em;*/
        box-shadow: 0px 0px 0px 0px var(--color-box-shadow);
        grid-template-rows: 0fr;
        margin-bottom: 0em;
        padding: 0em;
        /*transition: grid-template-rows 1s ease, padding 1s ease, box-shadow 1s ease, margin-bottom 1.5s ease-out;*/
        width: 100%;
        opacity: 0;
        transition: transform 0.5s ease, opacity 0.5s ease;
        pointer-events: none;
    }

    .settings_panel_show {
        box-shadow: 0px 0px 10px 2px var(--color-box-shadow);
        grid-template-rows: 1fr;
        margin-bottom: 1.4em;
        padding: .6em;
        opacity: 1;
        pointer-events: auto;
    }

    .bar-settings-panel {
        overflow: hidden;
    }

    .bars-panel {
        align-items: center;
        display: flex;
        text-shadow: var(--color-back);
        justify-content: space-between;
        margin-bottom: .4rem;
        text-align: center;
    }
    /* Tempo Settings start */
    .tempo-settings {
        display: flex;
        justify-content: space-between;
        margin-top: .4rem;
        overflow: hidden;
    }
    /* Tempo Settings end */
    .bar-settings-footer {
        align-items: center;
        background-color: var(--color-btn-back);
        border: none;
        border-radius: 14px;
        display: flex;
        justify-content: space-around;
        margin-top: 14px;
        text-align: center;
    }
</style>