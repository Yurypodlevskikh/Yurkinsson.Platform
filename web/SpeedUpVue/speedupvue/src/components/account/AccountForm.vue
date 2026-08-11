<template>
    <div class="account-form">
        <div v-if="statusMessage" class="status-message">
            {{statusMessage}}
            <!-- Resend button shown only for expired-token state -->
            <div v-if="showResendArea" class="resend-area">
                <ButtonSpeedUp :title="resendButtonText"
                               :disabled="resendCooldown"
                               @click="onResendClick"
                               class="resend-btn">
                </ButtonSpeedUp>
            </div>
        </div>

        <!-- If pending reset exists, show ResetPasswordForm inside same card -->
        <ResetPasswordForm v-if="hasPendingReset" ref="resetFormRef" />

        <!-- Inline forgot-password view (tab-style, not a modal) -->
        <div v-else-if="showForgot" class="form-actions">
            <div v-if="forgotMessage" class="status-message">{{ forgotMessage }}</div>

            <div class="form-group">
                <InputSpeedUp inputType="text"
                              name="forgotEmail"
                              v-model="forgotEmail"
                              :error="forgotError"
                              placeholderText="Enter your email" />
            </div>
        </div>

        <!-- Otherwise show standard login/register form -->
        <form v-else @submit.prevent="handleSubmit" class="form-actions">
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
                              @input="onFieldChange('nickname')" />

                <input type="hidden" v-model="formData.audience" />

                <ButtonSpeedUp :btnType="'submit'" title="Confirm">
                    <template #icon>
                        <IconPaperPlane :class="{ spinning: isSubmitting }" />
                    </template>
                </ButtonSpeedUp>
            </div>
        </form>
    </div>
    <!-- Footer: hide the footer buttons while showing Reset or Forgot views -->
    <div class="account-btn-footer">
        <template v-if="hasPendingReset">
            <!-- Reset-specific footer: call methods on child via ref -->
            <ButtonSpeedUp @click="onResetCancel" title="Cancel">
                <template #icon>
                    <IconRotateLeft />
                </template>
            </ButtonSpeedUp>

            <ButtonSpeedUp @click="onResetSubmit" :disabled="resetBusy" title="Set password">
                <template #icon>
                    <IconPaperPlane :class="{ spinning: resetBusy }" />
                </template>
            </ButtonSpeedUp>
        </template>

        <template v-else-if="showForgot">
            <!-- Forgot inline footer: use same layout as reset/default -->
            <ButtonSpeedUp @click="onForgotCancel" title="Cancel">
                <template #icon>
                    <IconRotateLeft />
                </template>
            </ButtonSpeedUp>

            <ButtonSpeedUp @click="onForgotSubmit" :disabled="forgotSubmitting" title="Send reset link">
                <template #icon>
                    <IconPaperPlane :class="{ spinning: forgotSubmitting }" />
                </template>
            </ButtonSpeedUp>
        </template>

        <template v-else>
            <!-- Default footer (login/register) -->
            <ButtonSpeedUp @click="toggleForm"
                           :title="isRegister ? 'Already have an account?' : 'Create an account'">
                <template #icon>
                    <component :is="isRegister ? IconLogin : SignUpIcon" />
                </template>
            </ButtonSpeedUp>

            <ButtonSpeedUp @click="openForgot"
                           title="Forgot Password">
                <template #icon>
                    <KeyIcon />
                </template>
            </ButtonSpeedUp>
        </template>
    </div>
</template>

<script setup>
    import { ref, nextTick, onMounted, computed } from 'vue';
    import { authenticateUser, registerUser, resendConfirmation, forgotPassword } from '@/services/authService'
    import store from '@/store/store.js'
    import InputSpeedUp from '../InputSpeedUp.vue';
    import ButtonSpeedUp from '../ButtonSpeedUp.vue';
    import IconPaperPlane from '../icons/IconPaperPlane.vue';
    import SignUpIcon from '../icons/IconSignUp.vue';
    import KeyIcon from '../icons/IconKey.vue';
    import IconLogin from '../icons/IconLogin.vue';
    import IconRotateLeft from '../icons/IconRotateLeft.vue';
    import ResetPasswordForm from './ResetPasswordForm.vue'

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

    const showForgot = ref(false)

    // Forgot-password state
    const forgotEmail = ref('')
    const forgotError = ref('')
    const forgotMessage = ref('')
    const forgotSubmitting = ref(false)

    // Resend related state
    const resendCooldown = ref(false)
    const cooldownRemaining = ref(0)
    let cooldownTimer = null
    const resendSent = ref(false) // hide button after successful resend
    const resetFormRef = ref(null)

    // pendingConfirmation may be set by main.js bootstrap; fallback to local values
    const pending = computed(() => store.state?.pendingConfirmation || null)
    const pendingErrorCode = computed(() => pending.value?.errorCode || null)
    const pendingUserId = computed(() => pending.value?.userId || null)

    const hasPendingReset = computed(() => !!store.state?.pendingReset)

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
            if (!formData.value.email) {
                statusMessage.value = 'Unable to resend confirmation: missing user information.'
                return
            } else {
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
        nextTick(() => {
            validateField(fieldName)
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
                    statusMessage.value = result.message
                    isRegister.value = false
                    formData.value.password = ''
                    formData.value.confirmPassword = ''
                    formData.value.nickname = ''
                } else {
                    formErrors.value.email = result.message
                }
            } else {
                const result = await authenticateUser(formData.value)

                if (result.success) {
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

    function openForgot() {
        // switch to inline forgot view in the same card
        forgotEmail.value = formData.value.email || ''
        forgotError.value = ''
        forgotMessage.value = ''
        showForgot.value = true
    }

    function onForgotCancel() {
        showForgot.value = false
        forgotEmail.value = ''
        forgotError.value = ''
        forgotMessage.value = ''
    }

    async function onForgotSubmit() {
        forgotError.value = ''
        forgotMessage.value = ''
        if (!forgotEmail.value || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(forgotEmail.value.trim())) {
            forgotError.value = 'Please enter a valid email'
            return
        }

        forgotSubmitting.value = true
        try {
            const result = await forgotPassword(forgotEmail.value.trim())
            if (result.success) {
                forgotMessage.value = result.message || 'If an account exists, a reset link was sent.'
                // keep the form open with success message briefly
            } else {
                forgotError.value = result.message || 'Failed to request password reset.'
            }
        } catch (e) {
            forgotError.value = 'Unexpected error. Try again later.'
        } finally {
            forgotSubmitting.value = false
        }
    }

    // Called by parent footer to cancel reset (delegates to child)
    function onResetCancel() {
        if (resetFormRef.value && typeof resetFormRef.value.onCancel === 'function') {
            resetFormRef.value.onCancel()
        }
    }

    // Called by parent footer to submit reset (delegates to child)
    function onResetSubmit() {
        if (resetFormRef.value && typeof resetFormRef.value.onSubmit === 'function') {
            resetFormRef.value.onSubmit()
        }
    }

    // reactive flag for child submitting state (used to disable submit button)
    const resetBusy = computed(() => {
        return !!(resetFormRef.value && resetFormRef.value.isSubmitting)
    })

    onMounted(() => {
        const pendingConf = store.state?.pendingConfirmation
        if (pendingConf) {
            statusMessage.value = pendingConf.message || ''
            if (pendingConf.errorCode === 'TOKEN_EXPIRED') {
            } else if (pendingConf.success) {
                isRegister.value = false
            }
        }
    })
</script>

<style scoped>
    .status-message {
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

    .forgot-inline {
        /* Specific styles for the inline forgot-password view */
        padding: 1rem;
        border-top: 1px solid #ccc;
        margin-top: 1rem;
    }
</style>