<template>
    <label :class="chbxClass">
        <input type="checkbox" 
               :name="chbxName" 
               :checked="modelValue" 
               @change="handleChange"
               :disabled="inptDisabled"/>
        <span :class="chbxBox"></span>
    </label>
</template>

<script setup>
    import { defineProps, defineEmits } from 'vue'

    const props = defineProps({
        modelValue: {
            type: Boolean,
            required: true
        },
        chbxClass: {
            type: String,
            required: true
        },
        inptDisabled: {
            type: Boolean,
            default: true
        },
        chbxBox: {
            type: String,
            requared: true
        },
        chbxName: {
            type: String,
            required: true
        }
    })

    const emit = defineEmits(['update:modelValue'])

    function handleChange(event) {
        emit('update:modelValue', event.target.checked)
    }
</script>

<style scoped>
    /* Repeat Switcher checkbox start */
    .repeat-switcher {
        cursor: pointer;
        display: block;
        font-size: 22px;
        margin-right: 25px;
        position: relative;
        user-select: none;
        -moz-user-select: none;
        -ms-user-select: none;
        -webkit-user-select: none;
    }

        .repeat-switcher input {
            cursor: pointer;
            height: 0;
            opacity: 0;
            position: absolute;
            width: 0;
        }

    .checkmark {
        background-color: var(--color-box-shadow);
        border-radius: 6px;
        height: 19px;
        left: 0px;
        position: absolute;
        top: 2px;
        width: 19px;
    }

    .repeat-switcher:hover input ~ .checkmark {
        box-shadow: 0 0 3px #008CF0;
    }

    .repeat-switcher input:checked ~ .checkmark {
        background-color: #008CF0;
    }

        .repeat-switcher input:checked ~ .checkmark:after {
            display: block;
        }

    .repeat-switcher .checkmark:after {
        border: solid var(--color-back);
        border-width: 0 3px 3px 0;
        content: "";
        display: none;
        height: 10px;
        left: 7px;
        position: absolute;
        top: 3px;
        transform: rotate(45deg);
        -ms-transform: rotate(45deg);
        -webkit-transform: rotate(45deg);
        width: 6px;
    }
    .repeat-switcher input:disabled ~ .checkmark {
        background-color: var(--color-background-mute);
        cursor: not-allowed;
    }
    .repeat-switcher input:checked:disabled ~ .checkmark:after {
        border: solid var(--color-background-mute-before);
        border-width: 0 3px 3px 0;
    }

    /* Repeat Switcher checkbox end */
    /* Reverse switcher checkbox start */
    .switch-reverse {
        display: inline-block;
        height: 22px;
        position: relative;
        width: 40px;
    }

        .switch-reverse input {
            opacity: 0;
            width: 0;
            height: 0;
        }

        .switch-reverse .slider {
            background-color: var(--color-box-shadow);
            border-radius: 6px;
            bottom: 0;
            cursor: pointer;
            left: 0;
            position: absolute;
            right: 0;
            top: 0;
            transition: .4s;
            -webkit-transition: .4s;
        }

            .switch-reverse .slider:before {
                background-color: var(--color-btn-back);
                /*border: 2px solid #1CA9C9; */
                border-radius: 35%;
                bottom: 3px;
                content: "";
                height: 16px;
                position: absolute;
                left: 3px;
                transition: .4s;
                -webkit-transition: .4s;
                width: 16px;
            }

        .switch-reverse input:checked + .slider {
            background-color: #008CF0;
        }

        .switch-reverse input:focus + .slider {
            box-shadow: 0 0 1px #008CF0;
        }

        .switch-reverse input:checked + .slider:before {
            background-color: #0000ff;
            transform: translateX(18px);
            -ms-transform: translateX(18px);
            -webkit-transform: translateX(18px);
        }
        .switch-reverse input:disabled + .slider {
            background-color: var(--color-background-mute);
            cursor: not-allowed;
        }
            .switch-reverse input:disabled + .slider:before {
                background-color: var(--color-background-mute-before);
            }
    /*Reverse switcher checkbox end*/
</style>