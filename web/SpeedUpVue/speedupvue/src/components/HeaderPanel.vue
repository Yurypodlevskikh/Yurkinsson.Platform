<template>
    <div class="bar-settings-wrapper">
        <div class="bar-settings-head">
            <button type="button" @click="handleLogoClick()" title="Themes" class="bpm-btn logo-btn">
                <LogoIcon />
            </button>
            <SpeedUpButton @click="isAuthenticated ? handleLogout() : openAuthTab()"
                           :title="isAuthenticated ? 'Logout' : 'Login'">
                <template #icon>
                    <component :is="isAuthenticated ? IconLogout : IconLogin" />
                </template>
            </SpeedUpButton>
            <SpeedUpButton @click="openSettingsTab" title="Settings">
                <template #icon>
                    <GearIcon />
                </template>
            </SpeedUpButton>
        </div>
    </div>
    
</template>
<script setup>
    import { computed } from 'vue'
    import { useStore } from 'vuex'
    import SpeedUpButton from '@/components/ButtonSpeedUp.vue'
    import SignUpIcon from './icons/IconSignUp.vue'
    import GearIcon from './icons/IconGear.vue'
    import LogoIcon from './icons/IconLogo.vue'
    import IconLogin from './icons/IconLogin.vue'
    import IconLogout from './icons/IconLogout.vue'
    import { logoutUser } from '@/services/authService'
    import { useStatusMessage } from '@/services/useStatusMessage'

    const store = useStore()
    const { showStatusMessage } = useStatusMessage()

    const handleLogoClick = () => {
        if (isAuthenticated.value) {
            openPresetsTab()
        }
    }
    // Get a state from the store
    const isOpenSettingsPanel = computed(() => store.getters.getSettingsPanel)
    const isAuthenticated = computed(() => store.getters.isAuthenticated)

    function openPresetsTab() {
        store.commit('openSettingsPanelWithTab', 'presets')
    }

    function openAuthTab() {
        store.commit('openSettingsPanelWithTab', 'auth')
    }

    function openSettingsTab() {
        store.commit('openSettingsPanelWithTab', 'settings')
    }

    function toggleSettingsPanel() {
        store.commit('toggleSettingsPanel')
    }

    async function handleLogout() {
        const result = await logoutUser()

        if (result.success) {
            // Delete Token
            store.commit('setToken', null)
            // Close settings panel
            if (isOpenSettingsPanel.value) {
                toggleSettingsPanel()
            }
        } else {
            showStatusMessage(result.message)
        }
    }
</script>
<style scoped>
    .logo-btn {
        align-items: center;
        background: transparent;
        border: none;
        cursor: pointer;
        display: flex;
        fill: var(--color-btn-text);
        height: 18px;
        justify-content: start;
        padding: .0rem;
    }
    .bar-settings-wrapper {
        background-color: transparent;
        display: block;
        left: 0;
        margin: auto;
        position: absolute;
        right: 0;
        top: 1rem;
        width: 242.3px;
        z-index: 10;
    }

    .bar-settings-head {
        align-items: center;
        background-color: var(--color-btn-back);
        border: none;
        border-radius: 14px;
        box-shadow: 0px 0px 10px 2px var(--color-box-shadow);
        display: flex;
        justify-content: space-around;
        text-align: center;
    }
</style>