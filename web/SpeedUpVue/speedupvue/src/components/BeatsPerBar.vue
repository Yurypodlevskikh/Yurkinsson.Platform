<template>
    <span class="beats-per-measure-text">
        Beats per bar
    </span>
    
    <div class="measures">
        <SpeedUpButton @click="decrease" title="Remove a beat">
            <template #icon>
                <MinusIcon />
            </template>
        </SpeedUpButton>

        <DisplayBeatsPerBar />

        <SpeedUpButton @click="increase" title="Add a beat">
            <template #icon>
                <PlusIcon />
            </template>
        </SpeedUpButton>
    </div>
</template>

<script>
    import SpeedUpButton from '@/components/ButtonSpeedUp.vue'
    import DisplayBeatsPerBar from '@/components/DisplayBeatsPerBar.vue'
    import MinusIcon from '@/components/icons/IconMinus.vue'
    import PlusIcon from '@/components/icons/IconPlus.vue'

    export default {
        components: {
            MinusIcon, PlusIcon, SpeedUpButton, DisplayBeatsPerBar
        },
        computed: {
            audioContext() {
                return this.$store.getters.getAudioContext;
            }
        },
        methods: {
            increase() {
                if (this.audioContext) {
                    this.$store.dispatch('initSounds');
                }

                this.$store.commit('increaseBeatPerBar');
            },
            decrease() {
                if (this.audioContext) {
                    this.$store.dispatch('initSounds');
                }

                this.$store.commit('decreaseBeatPerBar');
            }
        }
    }
</script>

<style scoped>
    .beats-per-measure-text {
        text-shadow: 0px 0px 8px var(--color-back);
        font-size: .9em;
        margin-bottom: .4em;
        text-transform: uppercase;
    }
    .measures {
        background: var(--color-btn-back);
        align-items: center;
        border-radius: 14px;
        box-shadow: 0px 0px 10px 2px var(--color-box-shadow);
        color: var(--color-btn-text);
        display: flex;
        font-size: 1.5em;
        padding: .2rem .4rem;
    }
</style>