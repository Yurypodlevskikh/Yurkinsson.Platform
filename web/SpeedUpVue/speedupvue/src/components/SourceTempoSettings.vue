<template>
    <SpeedUpButton @click="decreaseSourceBpmBtn" title="Decrease source tempo">
        <template #icon>
            <MinusIcon />
        </template>
    </SpeedUpButton>

    <!-- Range input for source BPM -->
    <SpeedUpInput inputType="range" name="sourceRangeBpm" :min="20" :max="280"
                  v-model="sourceBpmValue" />
    <SpeedUpButton @click="increaseSourceBpmBtn" title="Increase source tempo">
        <template #icon>
            <PlusIcon />
        </template>
    </SpeedUpButton>
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

    // Computed property to get source BPM from the store
    // Dispatch an action to update the BPM from the range input
    const sourceBpmValue = computed({
        get: () => store.getters.getSourceBpm,
        set: (value) => store.dispatch('updateRangeSourceTempo', value)
    })
    
    // Dispatch an action to decrease the source BPM
    function decreaseSourceBpmBtn() {
        store.dispatch('decreaseSourceBpmBtn')
    }

    // Dispatch an action to increase the source BPM
    function increaseSourceBpmBtn() {
        store.dispatch('increaseSourceBpmBtn')
    }
</script>

<style></style>