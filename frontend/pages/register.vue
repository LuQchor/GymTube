<template>
  <section class="flex flex-col items-center justify-center min-h-screen px-4">
    <div class="w-full max-w-md bg-base-100 rounded-xl shadow p-8">
      <h2 class="text-2xl font-bold mb-6 text-center">Registracija</h2>
      <form class="flex flex-col gap-4" @submit.prevent="onRegister">
        <input v-model="name" type="text" placeholder="Ime" class="input input-bordered w-full" required />
        <input v-model="email" type="email" placeholder="Email" class="input input-bordered w-full" required />
        <input v-model="password" type="password" placeholder="Lozinka" class="input input-bordered w-full" required minlength="6" />
        <input v-model="confirmPassword" type="password" placeholder="Ponovi lozinku" class="input input-bordered w-full" required minlength="6" />
        <div v-if="error" class="alert alert-error py-2">{{ error }}</div>
        <button class="btn btn-primary w-full" :disabled="loading">
          <span v-if="loading" class="loading loading-spinner"></span>
          Registriraj se
        </button>
      </form>
      <div class="text-center mt-4">
        <span>Imaš račun?</span>
        <NuxtLink to="/login" class="link link-primary ml-1">Prijavi se</NuxtLink>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useRuntimeConfig } from '#app'

const name = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const error = ref('')
const loading = ref(false)
const router = useRouter()
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl

async function onRegister() {
  error.value = ''
  if (password.value !== confirmPassword.value) {
    error.value = 'Lozinke se ne podudaraju.'
    return
  }
  loading.value = true
  try {
    const response = await fetch(`${apiBaseUrl}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: name.value,
        email: email.value,
        password: password.value
      })
    })
    
    const data = await response.json()
    
    if (response.ok) {
      // Registration successful, redirect to login
      router.push('/login')
    } else {
      error.value = data.error || 'Došlo je do greške prilikom registracije.'
    }
  } catch (e) {
    error.value = 'Došlo je do greške. Pokušaj ponovno.'
  } finally {
    loading.value = false
  }
}
</script>
