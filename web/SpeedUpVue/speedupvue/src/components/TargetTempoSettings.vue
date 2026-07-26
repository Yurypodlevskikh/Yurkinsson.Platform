<template>
    <SpeedUpButton @click="decreaseTargetBpmBtn" title="Decrease target tempo">
        <template #icon>
            <MinusIcon />
        </template>
    </SpeedUpButton>
    <!--<input type="range" min="20" max="280" step="1" value="120" class="target-slider" />-->
    <SpeedUpInput inputType="range" name="targetRangeBpm" :min="20" :max="280"
                  v-model="targetBpmValue" />
    <SpeedUpButton @click="increaseTargetBpmBtn" title="Increase target tempo">
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
    const targetBpmValue = computed({
        get: () => store.getters.getTargetBpm,
        set: (value) => store.dispatch('updateRangeTargetTempo', value)
    })

    // Dispatch an action to decrease the BPM
    function decreaseTargetBpmBtn() {
        store.dispatch('decreaseTargetBpmBtn')
    }

    // Dispatch an action to increase the BPM
    function increaseTargetBpmBtn() {
        store.dispatch('increaseTargetBpmBtn')
    }
</script>

<style></style>