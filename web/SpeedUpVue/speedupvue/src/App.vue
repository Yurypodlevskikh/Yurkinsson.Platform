<script>
import ThemeSwitcher from './components/ThemeSwitcher.vue'
    import SettingsPanel from './components/SettingsPanel.vue'
    import HeaderPanel from '@/components/HeaderPanel.vue'
    import TempoChangesDisplay from '@/components/layout/TempoChangesDisplay.vue'
    import TempoDisplay from '@/components/layout/TempoDisplay.vue'
    import RegularTempoSettings from '@/components/RegularTempoSettings.vue'
    import StartStopBtn from '@/components/StartStopBtn.vue'
    import BeatsPerBar from '@/components/BeatsPerBar.vue'

    import GearIcon from '@/components/icons/IconGear.vue'

    import httpClient from '@/services/httpClient.js'
    
    export default {
        name: "SpeedUp",
        mounted() {
            this.$store.commit('updateIntervalTime');

            if (!this.$store.getters.isAuthenticated){
                if (import.meta.env.DEV) {
                    console.log('No token - user is not authorized.')
                }
                return
            }

            httpClient.get('api/check-auth', { isSilentCheck: true })
                .then(response => {
                    if (import.meta.env.DEV) {
                        console.log("User is authorized")
                    }
                })
                .catch(() => {
                    if (import.meta.env.DEV) {
                        console.log('Not authorized on startup')
                    }
                });
        },
        components: {
            GearIcon, HeaderPanel, SettingsPanel, TempoChangesDisplay,
            TempoDisplay, RegularTempoSettings, StartStopBtn, BeatsPerBar
        },
        computed: {
            interval() {
                return this.$store.getters.getInterval;
            },
            isOpenSettingsPanel() {
                return this.$store.getters.getSettingsPanel;
            },
            firstPlayerSrc() {
                return this.$store.getters.getFirstPlayerSrc;
            },
            audioContextOne() {
                return this.$store.getters.getAudioContextOne;
            }
        }, 
        methods: {
            // Change state
            increment() {
                this.$store.commit('increment');
            },
            decrement() {
                this.$store.commit('decrement');
            },
            incrementAsync() {
                this.$store.dispatch('incrementAsync');
            },
            toggleSettingsPanel() {
                this.$store.commit('toggleSettingsPanel');
            },
            defineAudioPlayer() {
                this.$refs.clickPizda.load();
                this.$refs.clickPizda.play();
            },
            initAudioContextOne() {
                this.$store.dispatch('initAudioContextOne');
            }
        },
        beforeDestroy() {
            clearTimeout(this.interval);
            this.$store.commit('setIntervalId', null);
            this.$store.dispatch('closeAudioContext');
        }
    }
</script>

<template>
    <!-- Buttons for bar settings panel START -->
    <HeaderPanel />
    <!-- Buttons for bar settings panel END -->
    <div class="metronome-wrapper">
        <div class="metronome">

            <SettingsPanel />

            <!--Display BPM start-->
            <div class="bpm-display">
                <div>

                    <TempoChangesDisplay />

                    <TempoDisplay />

                </div>
            </div>

            <RegularTempoSettings />

            <StartStopBtn />

            <BeatsPerBar />

            <div class="display-bpm">
            </div>
            <!--Display BPM end-->
        </div>
    </div>
</template>

<style scoped>
    .bpm-display {
        background: var(--color-btn-back);
        border-radius: 14px;
        box-shadow: 0px 0px 10px 2px var(--color-box-shadow);
        display: flex;
        font-weight: bold;
        margin-bottom: .4rem;
        padding: .6em;
        text-shadow: 0px 0px 8px var(--color-background);
    }
    .metronome-wrapper {
        box-sizing: border-box;
        font-family: Calibri, sans-serif;
        margin: 0 auto;
        max-width: 1146px;
        user-select: none;
        width: 262.3px;
    }
    .metronome {
        align-items: center;
        color: var(--color-btn-text);
        display: flex;
        flex-direction: column;
        justify-content: center;
        padding: 10px;
    }
    
    @media (min-width: 1024px) {
        header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
