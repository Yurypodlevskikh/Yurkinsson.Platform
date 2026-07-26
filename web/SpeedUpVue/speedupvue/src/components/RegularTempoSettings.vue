<template>
    <div class="regular-tempo-settings">
        <SpeedUpButton @click="decreaseBpmBtn" title="Decrease tempo">
            <template #icon>
                <MinusIcon />
            </template>
        </SpeedUpButton>

        <SpeedUpInput inputType="range" name="regularBpm" :min="minRang" :max="maxRang"
               v-model="regularBpmValue" class="tepmo-slider" />

        <SpeedUpButton @click="increaseBpmBtn" title="Increase tempo">
            <template #icon>
                <PlusIcon />
            </template>
        </SpeedUpButton>
    </div>
</template>

<script setup>
    // Import required components
    import { computed } from 'vue'
    import { useStore } from 'vuex'

    import SpeedUpButton from '@/components/ButtonSpeedUp.vue'
    import SpeedUpInput from '@/components/InputSpeedUp.vue'
    import MinusIcon from '@/components/icons/IconMinus.vue'
    import PlusIcon from '@/components/icons/IconPlus.vue'

    // Use Vuex store
    const store = useStore()

    // Computed props from getters
    const minRang = computed(() => store.getters.getRegularMinRang)
    const maxRang = computed(() => store.getters.getRegularMaxRang)

    // Computed property to get regular BPM from the store
    // Dispatch an action to update the BPM from the range input
    const regularBpmValue = computed({
        get: () => store.getters.getSourceBpm,
        set: (value) => store.dispatch('updateRangeTempo', value)
    })

    // Dispatch an action to decrease the BPM
    function decreaseBpmBtn() {
        store.dispatch('decreaseBpmBtn')
    }

    // Dispatch an action to increase the BPM
    function increaseBpmBtn() {
        store.dispatch('increaseBpmBtn')
    }
</script>

<style scoped>
    .regular-tempo-settings {
        background: var(--color-btn-back);
        border-radius: 14px;
        box-shadow: 0px 0px 10px 2px var(--color-box-shadow);
        display: flex;
        margin-top: 1rem;
        padding: .6em;
        justify-content: space-between;
        overflow: hidden;
    }
</style>