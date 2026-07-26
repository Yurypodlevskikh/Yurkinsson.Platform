<template>
    <input v-bind="attrs" :type="inputType" :name="name" :min="min" :max="max"
           :value="modelValue" @focus="highlightValue" @input="onInput"
           ref="inputField" :disabled="inptDisabled"
           :placeholder="inputType !== 'range' ? placeholderText : ''" />
    <span v-if="error" class="error">{{error}}</span>
</template>

<script setup>
    import { ref } from 'vue'
    import { useAttrs, defineEmits, defineProps } from 'vue'

    // Collect all external attributes such as class, style, id, etc.
    const attrs = useAttrs()

    // Props
    const props = defineProps({
        modelValue: [String, Number],
        inputType: { type: String, required: true },
        name: { type: String, required: true },
        min: Number,
        max: Number,
        placeholderText: String,
        inptDisabled: Boolean,
        error: String
    })

    // Emits
    const emit = defineEmits(['update:modelValue'])

    // Refs
    const inputField = ref(null)

    // Methods
    function highlightValue() {
        if (props.inputType === 'number') {
            inputField.value.select()
        }
    }

    function onInput(event) {
        const value = event.target.value
        const newValue = props.inputType === 'number' || props.inputType === 'range' ? Number(value) : value
        emit('update:modelValue', newValue)
    }
</script>

<style scoped>
    .error {
        color: #f33;
        font-size: 0.8rem;
        margin-top: 0.2rem;
    }
</style>