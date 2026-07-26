<template>
    <div>
        <div class="tabs-header">
            <button v-for="tab in tabs" :key="tab" @click="selectTab(tab)" 
                    :class="{ active: tab === modelValue }">
                {{tab}}
            </button>
        </div>
        <div class="tabs-content">
            <slot :name="modelValue"></slot>
        </div>
    </div>
</template>

<script>
    export default {
        props: ['modelValue', 'tabs'],
        emits: ['update:modelValue'],
        methods: {
            selectTab(tab) {
                this.$emit('update:modelValue', tab);
            }
        }
    }
</script>

<style scoped>
    .tabs-header{
        display: flex;
        gap: 8px;
    }
    .tabs-header button.active{
        font-weight: bold;
    }
</style>