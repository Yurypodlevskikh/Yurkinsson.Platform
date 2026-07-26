<template>
    <div class="account-form">
        <div v-if="statusMessage" class="status-message">
            {{statusMessage}}
            <!-- Resend button shown only for expired-token state -->
            <div v-if="showResendArea" class="resend-area">
                <ButtonSpeedUp
                    :title="resendButtonText"
                    :disabled="resendCooldown"
                    @click="onResendClick"
                    class="resend-btn">
                </ButtonSpeedUp>
            </div>
        </div>
        <form @submit.prevent="handleSubmit" class="form-actions">
            <div class="form-group">
                <InputSpeedUp inputType="text"
                              name="email"
                              v-model="formData.email"
                              :error="formErrors.email"
                              placeholderText="Email"
                              @update:modelValue="() => onFieldChange('email')" />

                <InputSpeedUp inputType="password"
                              name="password"
                              v-model="formData.password"
                              :error="formErrors.password"
                              placeholderText="Password"
                              @update:modelValue="() => onFieldChange('password')" />

                <!-- Password confirmation for registration only -->
                <InputSpeedUp v-if="isRegister"
                              inputType="password"
                              name="confirmPassword"
                              v-model="formData.confirmPassword"
                              :error="formErrors.confirmPassword"
                              placeholderText="Confirm Password"
                              @update:modelValue="() => onFieldChange('confirmPassword')" />

                <!-- Nickname for registration only -->
                <InputSpeedUp v-if="isRegister"
                              inputType="text"
                              name="nickname"
                              v-model="formData.nickname"
                              :error="formErrors.nickname"
                              placeholderText="Nickname"
                              @input="onFieldChange('nickname')"/>

                <!-- Hidden field audience -->
                <input type="hidden" v-model="formData.audience" />

                <ButtonSpeedUp :btnType="'submit'" title="Confirm">
                    <template #icon>
                        <IconPaperPlane :class="{ spinning: isSubmitting }" />
                    </template>
                </ButtonSpeedUp>
            </div>
        </form>
    </div>
    <div class="account-btn-footer">
        <ButtonSpeedUp @click="toggleForm"
                       :title="isRegister ? 'Already have an account?' : 'Create an account'">
            <template #icon>
                <component :is="isRegister ? IconLogin : SignUpIcon" />
            </template>
        </ButtonSpeedUp>

        <ButtonSpeedUp @click="forgotPassword"
                       title="Forgot Password">
            <template #icon>
                <KeyIcon />
            </template>
        </ButtonSpeedUp>
    </div>
</template>

<script setup>
    import { ref, nextTick, onMounted, computed } from 'vue';
    import { authenticateUser, registerUser, resendConfirmation } from '@/services/authService'
    import store from '@/store/store.js'
    import InputSpeedUp from '../InputSpeedUp.vue';
    import ButtonSpeedUp from '../ButtonSpeedUp.vue';
    import IconPaperPlane from '../icons/IconPaperPlane.vue';
    import SignUpIcon from '../icons/IconSignUp.vue';
    import KeyIcon from '../icons/IconKey.vue';
    import IconLogin from '../icons/IconLogin.vue';

    const isRegister = ref(false)
    const statusMessage = ref('')
    const isSubmitting = ref(false)

    const formData = ref({
        email: '',
        password: '',
        confirmPassword: '',
        nickname: '',
        audience: 'speedupapi.yurkinsson.com'
    })

    const formErrors = ref({
        email: '',
        password: '',
        confirmPassword: '',
        nickname: '',
    })

    // Resend related state
    const resendCooldown = ref(false)
    const cooldownRemaining = ref(0)
    let cooldownTimer = null
    const resendSent = ref(false) // hide button after successful resend

    // pendingConfirmation may be set by main.js bootstrap; fallback to local values
    const pending = computed(() => store.state?.pendingConfirmation || null)
    const pendingErrorCode = computed(() => pending.value?.errorCode || null)
    const pendingUserId = computed(() => pending.value?.userId || null)

    const showResendArea = computed(() => {
        // Show resend area if errorCode indicates expired token and we haven't already sent
        return !resendSent.value && !isSubmitting.value && (pendingErrorCode.value === 'TOKEN_EXPIRED')
    })

    const resendButtonText = computed(() => {
        if (resendCooldown.value) return `Wait ${cooldownRemaining.value}s`
        return 'Resend confirmation email'
    })

    function startCooldown(seconds = 60) {
        resendCooldown.value = true
        cooldownRemaining.value = seconds
        cooldownTimer = setInterval(() => {
            cooldownRemaining.value -= 1
            if (cooldownRemaining.value <= 0) {
                clearInterval(cooldownTimer)
                cooldownTimer = null
                resendCooldown.value = false
            }
        }, 1000)
    }

    async function onResendClick() {
        // Prefer userId returned by backend; if not available, attempt to use email
        const userId = pendingUserId.value
        if (!userId) {
            // If no userId is available, try using email (requires backend support)
            if (!formData.value.email) {
                statusMessage.value = 'Unable to resend confirmation: missing user information.'
                return
            } else {
                // If your MiniAPI supports resend by email, call resendConfirmation with email.
                // Here we prefer userId; fall back only if your backend supports it.
                statusMessage.value = 'Unable to resend automatically. Please sign in or contact support.'
                return
            }
        }

        resendCooldown.value = true
        startCooldown(60) // example 60s cooldown

        try {
            const result = await resendConfirmation(userId)
            if (result.success) {
                resendSent.value = true
                statusMessage.value = result.message || 'A new confirmation email has been sent. Check your inbox.'
            } else {
                statusMessage.value = result.message || 'Failed to resend confirmation email.'
                // allow retry sooner on explicit failure
                clearInterval(cooldownTimer)
                cooldownTimer = null
                resendCooldown.value = false
            }
        } catch (err) {
            console.error('Resend error:', err)
            statusMessage.value = 'Unable to resend confirmation. Please try again later.'
            clearInterval(cooldownTimer)
            cooldownTimer = null
            resendCooldown.value = false
        }
    }

    function onFieldChange(fieldName) {
        // Vaiting for v-model to update
        nextTick(() => {
            validateField(fieldName)
            // Clear a global status message when user starts typing credentials
            if (statusMessage.value) {
                statusMessage.value = ''
            }
        })
    }

    function validateField(fieldName) {
        const value = formData.value[fieldName]

        switch (fieldName) {
            case 'email':
                const regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
                const trimmedEmail = value.trim()
                if (!trimmedEmail) {
                    formErrors.value.email = 'Email is required.'
                } else if (!regexEmail.test(trimmedEmail)) {
                    formErrors.value.email = 'Invalid email format.'
                } else {
                    formErrors.value.email = ''
                }
                break
            case 'password':
                const trimmedPassword = value.trim()
                if (!trimmedPassword) {
                    formErrors.value.password = 'Password is required.'
                } else if (formData.value.password.length < 6) {
                    formErrors.value.password = 'Minimum 6 characters.'
                } else {
                    formErrors.value.password = ''
                }
                break
            case 'confirmPassword':
                if (value !== formData.value.password) {
                    formErrors.value.confirmPassword = 'Passwords do not match.'
                } else {
                    formErrors.value.confirmPassword = ''
                }
                break
            case 'nickname':
                const regexNickname = /^[A-Za-z0-9_]+$/
                if (!formData.value.nickname) {
                    formErrors.value.nickname = 'Nickname is required.'
                } else if (!regexNickname.test(formData.value.nickname)) {
                    formErrors.value.nickname = 'Only letters, numbers and underscore.'
                } else if (formData.value.nickname.length > 256) {
                    formErrors.value.nickname = 'Max 256 characters.'
                } else {
                    formErrors.value.nickname = ''
                }
                break
            default:
                break
        }
    }

    const hasErrors = () => Object.values(formErrors.value).some(error => error)

    const handleSubmit = async () => {

        const fieldsToValidate = ['email', 'password']

        if (isRegister.value) {
            fieldsToValidate.push('confirmPassword', 'nickname')
        }

        fieldsToValidate.forEach(validateField)

        if (hasErrors()) {
            if (import.meta.env.DEV) {
                console.log('❌ Fix errors.')
                Object.entries(formErrors.value).forEach(([key, value]) => {
                    if (value) console.warn(`• ${key}: ${value}`)
                })
            }
            
            return
        }

        isSubmitting.value = true

        try {
            if (isRegister.value) {
                const result = await registerUser(formData.value)

                if (result.success) {
                    // Show message and toggle form/swish to Login
                    statusMessage.value = result.message
                    isRegister.value = false
                    // Clear fields
                    formData.value.password = ''
                    formData.value.confirmPassword = ''
                    formData.value.nickname = ''
                } else {
                    formErrors.value.email = result.message
                }
            } else {
                const result = await authenticateUser(formData.value)

                if (result.success) {
                    //console.log('🎉 Logged in!', result)

                    // Change tabb to presets
                    store.commit('setActiveSettingsTab', 'presets')
                } else {
                    formErrors.value.email = result.message
                }
            }
        } catch (error) {
            if (import.meta.env.DEV) {
                console.error('Submission error: ', error)
            }
        } finally {
            isSubmitting.value = false
        }
    }

    function toggleForm() {
        isRegister.value = !isRegister.value
        statusMessage.value = ''
    }

    function forgotPassword() {
        alert("Let\'s reset your password :-)")
    }

    // If pending confirmation info exists in store (set by bootstrap), show message
    onMounted(() => {
        const pendingConf = store.state?.pendingConfirmation
        if (pendingConf) {
            statusMessage.value = pendingConf.message || ''
            // If token expired keep userId for resend
            if (pendingConf.errorCode === 'TOKEN_EXPIRED') {
                // keep in store; button reads store.state.pendingConfirmation.userId
            } else if (pendingConf.success) {
                isRegister.value = false
            }
        }
    })
</script>

<style scoped>
    .spinning{
        animation: spin 1s linear infinite;
    }
    @keyframes spin{
        0%{transform: rotate(0deg);}
        100%{transform:rotate(360deg);}
    }
    .account-form {
        padding: .5rem;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
    }
    .status-message{
        background-color: #d1e7dd;
        color: #0f5132;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        margin-bottom: 1rem;
        text-align: center;
    }
    .resend-area {
        margin-top: 0.5rem;
    }
    .resend-btn {
        /* Minimal styling to visually separate the resend action */
        background-color: var(--color-btn-back);
        color: var(--color-btn-text);
        border-radius: 8px;
        padding: 0.4rem 0.8rem;
        font-weight: 600;
    }
    .form-group {
        display: flex;
        flex-direction: column;
        gap: .8rem;
        width: 100%;
    }
    .form-group input{
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