<template>
    <div class="measure-beats">{{beatsPerBar}}</div>
</template>

<script>
    export default {
        computed: {
            beatsPerBar() {
                return this.$store.getters.getBeatsPerBar;
            }
        }
    }
</script>

<style scoped>
    .measure-beats {
        background: transparent;
        fill: var(--color-btn-text);
        padding: 0 1em;
    }
</style>