<template>
    <div class="panel-columns">
        <div>Bars</div>
        <SpeedUpInput inputType="number" name="barsinpattern" :min="0" :max="500" 
                      v-model="barsValue" />
    </div>
    <div class="panel-columns">
        <div>Tempo periods</div>
        <div class="repeat-switcher-wrapper">
            <RepeatPatternChckbx :chbxClass="repeatSwitcher"
                                 :chbxName="repeatswitch"
                                 v-model="isRepeatPattern"
                                 :chbxBox="switcherBox" :inptDisabled="inptDisabled" />
            <SpeedUpInput inputType="number" name="repeattimes" :min="0" :max="100"
                          v-model="patternRepeats" :inptDisabled="!inptDisabled" />
        </div>
    </div>
    <div class="panel-columns">
        <div>Reverse</div>
        <div>
            <RepeatPatternChckbx :chbxClass="switchReverse" 
                                 :chbxName="reverseswitch"
                                 v-model="isReverse"
                                 :chbxBox="reverseBox"
                                 :inptDisabled="!inptDisabled"/>
        </div>
    </div>
</template>

<script setup>
    // Import required components
    import { computed } from 'vue'
    import { useStore } from 'vuex'

    import SpeedUpInput from '@/components/InputSpeedUp.vue'
    import RepeatPatternChckbx from '@/components/RepeatPatternChckbx.vue'

    const store = useStore()

    // Static props for UI elements
    const switchReverse = 'switch-reverse'
    const repeatSwitcher = 'repeat-switcher'
    const reverseswitch = 'reverseswitcher'
    const repeatswitch = 'repeatswitcher'
    const switcherBox = 'checkmark'
    const reverseBox = 'slider'

    // Computed props with getters and setters for v-model
    const barsValue = computed({
        get: () => store.getters.getBarsPerPattern,
        set: (value) => store.dispatch('updateBarsPerPattern', value),
    })

    const isRepeatPattern = computed({
        get: () => store.getters.isRepeatPattern,
        set: (value) => store.dispatch('setRepeatPattern', value),
    })

    const patternRepeats = computed({
        get: () => store.getters.getPatternRepeats,
        set: (value) => store.dispatch('setPatternRepeats', value),
    })

    const isReverse = computed({
        get: () => store.getters.getIsReverse,
        set: (value) => store.dispatch('toggleReverse', value),
    })

    const inptDisabled = computed(() => store.getters.getIsServiceAndTargetAreDifference)
</script>

<style>
    .panel-columns {
        display: flex;
        flex-direction: column;
        margin-bottom: .4rem;
    }
    .repeat-switcher-wrapper {
        display: flex;
    }
</style>