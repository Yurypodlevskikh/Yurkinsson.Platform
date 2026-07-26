<template>
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
</style>