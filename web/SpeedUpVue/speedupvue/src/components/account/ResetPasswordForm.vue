<template>
    <div class="form-actions">
        <div v-if="status" class="status-message">
            {{ status }}
        </div>

        <div class="form-group">
            <InputSpeedUp inputType="text"
                          name="email"
                          v-model="email"
                          :readonly="true"
                          placeholderText="Email" />

            <InputSpeedUp inputType="password"
                          name="password"
                          v-model="password"
                          :error="passwordError"
                          placeholderText="New password" />
            
            <InputSpeedUp inputType="password"
                          name="confirm"
                          v-model="confirm"
                          :error="confirmError"
                          placeholderText="Confirm password" />
        </div>
    </div>
</template>

<script setup>
    import { ref, computed } from 'vue'
    import { resetPassword } from '@/services/authService.js'
    import InputSpeedUp from '../InputSpeedUp.vue'
    import ButtonSpeedUp from '../ButtonSpeedUp.vue';
    import store from '@/store/store.js'
    import IconPaperPlane from '../icons/IconPaperPlane.vue';
    import IconRotateLeft from '../icons/IconRotateLeft.vue';

    const pending = computed(() => store.state.pendingReset)
    const email = ref(pending.value?.email ?? '')
    const token = ref(pending.value?.token ?? '')
    const password = ref('')
    const confirm = ref('')
    const passwordError = ref('')
    const confirmError = ref('')
    const status = ref('')
    const isSubmitting = ref(false)

    function onCancel() {
        store.commit('clearPendingReset')
        // close auth tab optionally
        store.commit('openSettingsPanelWithTab', 'auth')
    }

    async function onSubmit() {
        passwordError.value = ''
        confirmError.value = ''
        status.value = ''

        if (!password.value || password.value.length < 6) {
            passwordError.value = 'Password must be at least 6 characters.'
            return
        }
        if (password.value !== confirm.value) {
            confirmError.value = 'Passwords do not match.'
            return
        }

        isSubmitting.value = true
        try {
            const result = await resetPassword({ email: email.value, token: token.value, password: password.value })
            if (result.success) {
                status.value = result.message || 'Password successfully reset.'
                // clear pending reset so UI returns to normal (login)
                store.commit('clearPendingReset')
                // optionally switch tab to login
                store.commit('setActiveSettingsTab', 'auth')
                // you may clear fields
                password.value = ''
                confirm.value = ''
            } else {
                status.value = result.message || 'Failed to reset password.'
            }
        } catch (err) {
            status.value = 'Unexpected error. Try again later.'
        } finally {
            isSubmitting.value = false
        }
    }

    // Expose methods and state so parent can render the footer and call submit/cancel
    defineExpose({
        onSubmit,
        onCancel,
        isSubmitting
    })
</script>

<style scoped>
    .reset-form {
        display: flex;
        flex-direction: column;
        gap: 0.6rem;
    }

    .actions {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
        margin-top: 0.6rem;
    }
</style>