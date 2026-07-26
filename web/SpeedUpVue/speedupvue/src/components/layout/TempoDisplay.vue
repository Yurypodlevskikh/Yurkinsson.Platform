<template>
    <div class="flex-table">
        <div class="flex-row">
            <div class="flex-cell cell-1 tempo">
                {{tempo}}
            </div>
            <div class="flex-cell">
                <div class="flex-item bpmDcml">{{twoDecimalPlaces}}</div>
                <div class="flex-item bpm">BPM</div>
            </div>
        </div>
    </div>
    <div class="tempo-text-display">
        <div class="tempo-text">{{tempoText}}</div>
    </div>
</template>

<script>
    export default {
        computed: {
            tempo() {
                return this.$store.getters.getTempo;
            },
            tempoText() {
                return this.$store.getters.getTempoText;
            },
            twoDecimalPlaces() {
                return this.$store.getters.getTwoDecimalPlaces;
            }
        }
    }
</script>

<style scoped>
    .flex-table {
        display: flex;
        flex-direction: row;
    }
    .flex-row {
        display: flex;
        width: 100%;
        padding: 5px 0;
    }
    .flex-cell {
        display: flex;
        flex-direction: column;
        padding: 5px;
        height: 100%;
    }
    .cell-1 {
        line-height: .75;
        align-items: flex-start;
        justify-content: flex-start;
        text-align: left;
    }
    .bpmDcml {
        font-size: 1.4rem;
        line-height: 1;
    }
    .bpm{
        line-height: .8;
    }
    .flex-item {
        flex: 1;
        display: flex;
        align-items: flex-end;
        justify-content: flex-end;
        text-align: right;
    }
    .tempo {
        font-size: 4rem;
        padding: 0;
        height: fit-content;
    }
    .tempo-text-display {
        display: flex;
        align-items: center;
        justify-content: space-around;
        width: 100%;
    }
    .tempo-text{
        text-shadow: 0px 0px 8px var(--color-background);
        font-size: .8em;
        text-align: center;
        text-transform: uppercase;
        display: flex;
    }
</style>