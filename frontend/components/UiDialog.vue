<template>
  <transition name="dialog-fade">
    <div v-if="visible" class="fixed inset-0 z-[9999] flex items-center justify-center bg-base-300/40 backdrop-blur-sm">
      <div :class="[
        'bg-base-200 text-base-content border border-base-300 shadow-2xl rounded-2xl px-10 py-7 flex flex-col items-center gap-5 min-w-[340px] max-w-xl text-xl',
        type === 'success' ? 'bg-success text-success-content border-success/30' :
        type === 'error' ? 'bg-error text-error-content border-error/30' :
        type === 'info' ? 'bg-base-200 text-base-content border-base-300' :
        ''
      ]">
        <span v-if="type === 'success'">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-success-content" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>
        </span>
        <span v-else-if="type === 'error'">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-error-content" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
        </span>
        <span v-else-if="type === 'info'">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-base-content" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        </span>
        <span v-else-if="type === 'confirm'">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        </span>
        <div class="flex-1 text-lg font-semibold text-center w-full break-words">{{ message }}</div>
        <div v-if="type === 'confirm' && props.password" class="w-full flex flex-col gap-2 mt-2">
          <input
            v-model="passwordValue"
            type="password"
            class="input input-bordered w-full"
            placeholder="Unesite lozinku za potvrdu"
            autocomplete="current-password"
          />
        </div>
        <div class="flex gap-4 justify-center w-full mt-2">
          <button v-if="type === 'confirm'" class="btn btn-primary min-w-[100px]" @click="onConfirm">{{ confirmText }}</button>
          <button v-if="type === 'confirm'" class="btn btn-outline min-w-[100px]" @click="onCancel">{{ cancelText }}</button>
          <button v-if="type === 'alert'" class="btn btn-primary min-w-[100px]" @click="onConfirm">OK</button>
          <button v-if="type === 'success' || type === 'error' || type === 'info'" class="btn btn-ghost min-w-[100px]" @click="onClose">Zatvori</button>
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'

const props = defineProps<{
  modelValue: boolean,
  type: 'success' | 'error' | 'info' | 'confirm' | 'alert',
  message: string,
  confirmText?: string,
  cancelText?: string,
  duration?: number,
  password?: boolean
}>()
const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const visible = ref(props.modelValue)
const type = ref(props.type)
const message = ref(props.message)
const confirmText = ref(props.confirmText || 'Potvrdi')
const cancelText = ref(props.cancelText || 'Odustani')
const passwordValue = ref('')

watch(() => props.modelValue, v => visible.value = v)
watch(() => props.type, v => type.value = v)
watch(() => props.message, v => message.value = v)

let autoHideTimeout: any = null

onMounted(() => {
  if (['success', 'error', 'info'].includes(type.value) && props.duration !== 0) {
    autoHideTimeout = setTimeout(() => {
      onClose()
    }, props.duration || 2500)
  }
})

function onClose() {
  visible.value = false
  emit('update:modelValue', false)
}
function onConfirm() {
  if (props.password) {
    emit('confirm', passwordValue.value)
  } else {
    emit('confirm', true)
  }
  onClose()
}
function onCancel() {
  emit('cancel', false)
  onClose()
}
</script>

<style scoped>
.dialog-fade-enter-active, .dialog-fade-leave-active {
  transition: all 0.35s cubic-bezier(.4,2,.6,1);
}
.dialog-fade-enter-from, .dialog-fade-leave-to {
  opacity: 0;
  transform: scale(0.92) translateY(24px);
}
</style> 