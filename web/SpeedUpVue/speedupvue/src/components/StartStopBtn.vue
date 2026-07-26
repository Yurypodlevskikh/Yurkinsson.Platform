<template>
    <button type="button" class="start-stop" @click="togglePlayStop">
        <StopIcon v-if="isPlaying" />
        <PlayIcon v-else />
    </button>
</template>

<script>
    import PlayIcon from '@/components/icons/IconPlay.vue'
    import StopIcon from '@/components/icons/IconStop.vue'
    import { mapState } from 'vuex';
    
    export default {
        components: {
            PlayIcon, StopIcon
        },
        computed: {
            ...mapState({
                nextTempoMs: state => state.nextTempoMs,
                isPlaying: state => state.isPlaying,
                allFilesReady: state => state.allFilesReady,
                audioContext: state => state.audioContext
            })
        },
        watch: {
            nextTempoMs(newValue, oldValue){
                if (this.isPlaying) {
                    this.$store.commit('clearSpeedUpInterval');
                    this.$store.commit('notPlaying');
                    this.$store.commit('setIntervalTime', newValue);
                    this.$store.dispatch('playMetronome');
                }
            }
        },
        methods: {
            togglePlayStop() {
                if (!this.audioContext) {
                    this.$store.dispatch('initSounds');
                }

                if (this.allFilesReady) {
                    if (this.isPlaying) {
                        this.$store.dispatch('stopMetronome');
                    } else {
                        this.$store.dispatch('prePlayClick');
                        this.$store.dispatch('playMetronome');
                    }
                }
            }
        }
    }
</script>

<style scoped>
    .start-stop {
        background: var(--color-play-radial-grad);
        border: none;
        border-radius: 14px;
        fill: var(--color-shadow-light);
        text-shadow: 0px 0px 6px var(--color-background);
        cursor: pointer;
        margin-bottom: .4rem;
        margin-top: .8rem;
        padding: 1rem 4rem;
        text-align: center;
        outline: none;
        -webkit-tap-highlight-color: transparent;
        user-select: none;
    }
        .start-stop:hover {
            background: var(--color-play-radial-grad-hover);
        }
        .start-stop:hover > svg {
            fill: var(--color-box-shadow);
            filter: drop-shadow(0px 0px 6px var(--color-text-shadow));
        }
</style>