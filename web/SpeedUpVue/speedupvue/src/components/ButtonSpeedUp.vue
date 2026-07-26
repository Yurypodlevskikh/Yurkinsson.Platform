<template>
    <button :type="btnType" :title="title" :disabled="disabled" class="settings-btn">
        <slot name="icon"></slot>
        <slot></slot>
    </button>
</template>

<script setup>
    const props = defineProps({
        title: {
            type: String,
            required: false
        },
        btnType: {
            type: String,
            default: 'button',
            validator: value => ['button', 'submit','reset'].includes(value)
        },
        disabled: {
            type: Boolean,
            default: false
        }
    })
</script>

<style scoped>
    .settings-btn {
        align-items: center;
        background: var(--color-radial-grad);
        border: none;
        border-radius: 35%;
        cursor: pointer;
        color: var(--color-btn-text); 
        display: flex;
        fill: var(--color-btn-text);
        font-size: 1em;
        flex-grow: 1;
        justify-content: center;
        padding: .4rem .5rem;
        transition: all 0.2s ease-in-out;
    }

        .settings-btn:hover {
            background: var(--color-radial-grad-hover);
            color: var(--color-background);
            fill: var(--color-background);
            text-shadow: 0px 0px 6px var(--color-text-shadow);
        }

        .settings-btn:disabled{
            background: var(--color-radial-grad-disabled);
            cursor: not-allowed;
            opacity: 0.7;
        }
</style>