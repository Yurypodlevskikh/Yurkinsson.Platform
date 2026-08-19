<template>
    <!-- Preset title (optional, subtle) -->
    <div v-if="presetTitle" class="preset-title">{{ presetTitle }}</div>
    <div class="tempo-changes-display">
        <span class="source-icon-target">
            {{sourceBpm}}
        </span>
        <span class="source-icon-target">
            <CircleArrowRightIcon />
        </span>
        <span class="source-icon-target">
            {{targetBpm}}
        </span>
    </div>
</template>

<script>
    import CircleArrowRightIcon from '@/components/icons/IconCircleArrowRight.vue'

    export default {
        components: {
            CircleArrowRightIcon
        },
        computed: {
            sourceBpm() {
                return this.$store.getters.getSourceBpm;
            },
            targetBpm() {
                return this.$store.getters.getTargetBpm;
            },
            presetTitle() {
                return this.$store.getters.getCurrentPresetTitle || '';
            }
        }
    }
</script>

<style scoped>
    .tempo-changes-display {
        display: flex;
        align-items: center;
        justify-content: space-around;
        width: 100%;
    }

    .source-icon-target {
        display: flex;
        fill: var(--color-shadow-light);
    }
    /* Subtle title styling consistent with existing labels */
    .preset-title {
        font-size: 0.7rem;
        color: var(--color-btn-text);
        opacity: 0.9;
        text-align: center;
        text-transform: none;
        margin-bottom: 0.2rem;
    }
</style>