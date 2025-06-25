<template>
  <div :data-theme="darkMode ? 'dark' : 'light'">
    <Navbar :darkMode="darkMode" @toggle-dark="toggleDark" />
    <UiDialog v-model="dialogVisible" :type="dialogType" :message="dialogMessage" :confirmText="dialogConfirmText" :cancelText="dialogCancelText" :duration="dialogDuration" :password="dialogPassword" @confirm="dialogResolve" @cancel="dialogResolve(false)" />
    <main class="min-h-screen">
      <slot />
    </main>
    <Footer />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Navbar from '~/components/Navbar.vue'
import Footer from '~/components/Footer.vue'
import UiDialog from '~/components/UiDialog.vue'

const darkMode = ref(false)
const dialogVisible = ref(false)
const dialogType = ref<'success'|'error'|'info'|'confirm'|'alert'>('info')
const dialogMessage = ref('')
const dialogConfirmText = ref('Potvrdi')
const dialogCancelText = ref('Odustani')
const dialogDuration = ref<number|undefined>(undefined)
const dialogPassword = ref(false)
let dialogResolve: (v: boolean) => void = () => {}

// Dodajemo inline skriptu za rani set teme
if (typeof window !== 'undefined') {
  try {
    const theme = localStorage.getItem('darkMode') === 'true' ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', theme);
  } catch(e) {}
}

onMounted(() => {
  // Učitaj preferenciju iz localStorage
  darkMode.value = localStorage.getItem('darkMode') === 'true'
})

function toggleDark() {
  darkMode.value = !darkMode.value
  localStorage.setItem('darkMode', darkMode.value.toString())
}

// Global API
if (typeof window !== 'undefined') {
  (window as any).$notify = (opts: { message: string, type?: 'success'|'error'|'info', duration?: number }) => {
    dialogType.value = opts.type || 'info'
    dialogMessage.value = opts.message
    dialogDuration.value = opts.duration ?? 2500
    dialogVisible.value = true
    return new Promise<void>(resolve => {
      dialogResolve = () => resolve()
    })
  }
  (window as any).$confirm = (opts: { message: string, confirmText?: string, cancelText?: string, password?: boolean }) => {
    dialogType.value = 'confirm'
    dialogMessage.value = opts.message
    dialogConfirmText.value = opts.confirmText || 'Potvrdi'
    dialogCancelText.value = opts.cancelText || 'Odustani'
    dialogDuration.value = 0
    dialogVisible.value = true
    dialogPassword.value = !!opts.password
    return new Promise(resolve => {
      dialogResolve = (v: boolean) => resolve(v)
    })
  }
  (window as any).$alert = (opts: { message: string }) => {
    dialogType.value = 'alert'
    dialogMessage.value = opts.message
    dialogDuration.value = 0
    dialogVisible.value = true
    return new Promise<void>(resolve => {
      dialogResolve = () => resolve()
    })
  }
}
</script>
