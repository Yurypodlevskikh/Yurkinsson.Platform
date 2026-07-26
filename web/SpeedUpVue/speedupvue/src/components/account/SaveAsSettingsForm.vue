<template>
    <div class="account-form">
        <form @submit.prevent="handleSubmit" class="form-actions">
            <div class="form-group">
                <InputSpeedUp inputType="text"
                              name="title"
                              v-model="formData.title"
                              :error="formErrors.title"
                              placeholderText="Title"
                              @update:modelValue="() => onFieldChange('title')" />

                <InputSpeedUp inputType="text"
                              name="description"
                              v-model="formData.description"
                              :error="formErrors.description"
                              placeholderText="Description"
                              @update:modelValue="() => onFieldChange('description')" />

                <ButtonSpeedUp :btnType="'submit'" title="Save">
                    <template #icon>
                        <IconPaperPlane />
                    </template>
                </ButtonSpeedUp>
            </div>
        </form>
    </div>
    <div class="account-btn-footer">
        <ButtonSpeedUp @click="handleCancel"
                       title="Cancel">
            <template #icon>
                <IconRotateLeft />
            </template>
        </ButtonSpeedUp>
    </div>
</template>

<script setup>
    import { ref, nextTick, inject } from 'vue';
    import metronomeSettingsService from '@/services/metronomeSettingsService'
    import { useStatusMessage } from '@/services/useStatusMessage'

    import store from '@/store/store.js'
    import InputSpeedUp from '../InputSpeedUp.vue';
    import ButtonSpeedUp from '../ButtonSpeedUp.vue';
    import IconPaperPlane from '../icons/IconPaperPlane.vue';
    import IconRotateLeft from '../icons/IconRotateLeft.vue';

    const { showStatusMessage } = useStatusMessage()

    const formData = ref({
        title: '',
        description: ''
    })

    const formErrors = ref({
        title: '',
        description: '',
    })

    function onFieldChange(fieldName) {
        // Vaiting for v-model to update
        nextTick(() => {
            validateField(fieldName)
        })
    }

    function validateField(fieldName) {
        const value = formData.value[fieldName].trim()

        switch (fieldName) {
            case 'title':
                if (!value) {
                    formErrors.value.title = 'Title is required.'
                } else if (formData.value.title.length < 1) {
                    formErrors.value.title = 'Minimum 1 characters.'
                } else if (formData.value.title.length > 100) {
                    formErrors.value.title = 'Maximum 100 characters.'
                } else {
                    formErrors.value.title = ''
                }
                break
            case 'description':
                if (value.length > 255) {
                    formErrors.value.description = 'Maximum 255 characters.'
                } else {
                    formErrors.value.description = ''
                }
                break
            default:
                break
        }
    }

    const hasErrors = () => Object.values(formErrors.value).some(error => error)

    const handleSubmit = async () => {

        const fieldsToValidate = ['title', 'description']

        fieldsToValidate.forEach(validateField)

        if (hasErrors()) {
            if (import.meta.env.DEV) {
                console.log('❌ Fix errors before submitting.')
            }
            showStatusMessage('Please fix validation errors before submitting.')
            return
        }

        const settingsDto = {
            title: formData.value.title.trim(),
            description: formData.value.description.trim(),
            sourceBpm: store.state.sourceBpm,
            targetBpm: store.state.targetBpm,
            barsPerPattern: store.state.barsPerPattern,
            tempoPeriods: store.state.tempoPeriods,
            isReversed: store.state.isReverse,
            isRepeatPattern: store.state.isRepeatPattern,
            beatsPerBar: store.state.beatsPerBar,
            isTempoIncreace: store.state.isTempoIncreace,
            userId: store.state.userId,
            metronomeCollectionId: null
        }

        try {
            const response = await metronomeSettingsService.saveAsSettings(settingsDto)
            store.commit('setSavedSettingsId', response.id)
            //showStatusMessage('Settings saved successfully!')

            // Back to the presets
            store.commit('openSettingsPanelWithTab', 'presets')
        } catch (error) {
            if (error.response && error.response.status === 400) {
                // Show message from server about limit
                showStatusMessage(error.response.data)
            } else {
                if (import.meta.env.DEV) {
                    console.error('Failed to save settings: ', error)
                }
                
                showStatusMessage('Error saving settings.')
            }
        }
    }

    const handleCancel = () => {
        store.commit('openSettingsPanelWithTab', 'settings')
    }
</script>

<style scoped>
    .account-form {
        padding: .5rem;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
    }

    .form-group {
        display: flex;
        flex-direction: column;
        gap: .8rem;
        width: 100%;
    }

        .form-group input {
            width: 100%;
        }

    .form-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        justify-content: space-between;
        width: 100%;
    }

    .account-btn-footer {
        align-items: center;
        background-color: var(--color-btn-back);
        border: none;
        border-radius: 14px;
        display: flex;
        justify-content: space-around;
        text-align: center;
    }
</style>