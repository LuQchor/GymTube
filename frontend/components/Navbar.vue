<template>
  <nav class="navbar bg-base-100 shadow mb-4">
    <div class="flex-1">
      <NuxtLink class="btn btn-ghost text-xl" to="/">GymTube</NuxtLink>
    </div>
    
    <!-- Search Bar (only for authenticated users) -->
    <div v-if="isAuthenticated" class="flex-1 max-w-md mx-4">
      <div class="relative">
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Pretraži korisnike..." 
          class="input input-bordered w-full pr-10"
          @input="debounceSearch"
          @focus="showSearchResults = true"
        />
        <button class="absolute right-2 top-1/2 -translate-y-1/2 btn btn-ghost btn-sm" @click="performSearch">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </button>
        
        <!-- Search Results Dropdown -->
        <div v-if="showSearchResults && (searchResults.length > 0 || searchError)" class="absolute top-full left-0 right-0 bg-base-100 border border-base-300 rounded-lg shadow-lg z-50 max-h-60 overflow-y-auto">
          <div v-if="searchLoading" class="p-4 text-center">
            <div class="loading loading-spinner loading-sm"></div>
            <span class="ml-2">Pretražujem...</span>
          </div>
          <div v-else-if="searchError" class="p-4 text-error text-sm">
            {{ searchError }}
          </div>
          <div v-else-if="searchResults.length > 0" class="py-2">
            <div 
              v-for="userResult in searchResults" 
              :key="userResult.id" 
              class="px-4 py-2 hover:bg-base-200 cursor-pointer"
              @click="selectUser(userResult)"
            >
              <div class="flex items-center gap-3">
                <UserAvatar :key="userResult.profileImageUrl || 'no-image'" :user="userResult" size="sm" />
                <div class="flex-1">
                  <div class="font-medium">{{ userResult.name }}</div>
                  <div class="text-sm text-base-content/70">{{ userResult.email }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Desktop links -->
    <div class="hidden md:flex flex-none gap-2 items-center">
      <NuxtLink class="btn btn-ghost" to="/dashboard">Dashboard</NuxtLink>
      <NuxtLink class="btn btn-ghost" to="/upload">Upload</NuxtLink>
      <NuxtLink class="btn btn-ghost" to="/profile">Profil</NuxtLink>
      <NuxtLink v-if="isAuthenticated" class="btn btn-ghost" to="/subscription">Pretplata</NuxtLink>
      <button class="btn btn-square btn-ghost" @click="$emit('toggle-dark')">
        <span v-if="darkMode">🌙</span>
        <span v-else>☀️</span>
      </button>
      <template v-if="isAuthenticated">
        <!-- Avatar with dropdown -->
        <div class="dropdown dropdown-end">
          <label tabindex="0" class="btn btn-ghost btn-circle avatar">
            <UserAvatar :key="user?.profileImageUrl || 'no-image'" :user="user" size="sm" />
          </label>
          <ul tabindex="0" class="mt-3 z-[1] p-2 shadow menu menu-sm dropdown-content bg-base-100 rounded-box w-40">
            <li><NuxtLink to="/profile">Profil</NuxtLink></li>
            <li><button @click="logout">Logout</button></li>
          </ul>
        </div>
      </template>
      <template v-else>
        <NuxtLink class="btn btn-outline btn-sm" to="/login">Login</NuxtLink>
      </template>
    </div>
    <!-- Mobile hamburger -->
    <div class="md:hidden flex-none">
      <div class="dropdown dropdown-end">
        <label tabindex="0" class="btn btn-ghost btn-circle">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" /></svg>
        </label>
        <ul tabindex="0" class="menu menu-sm dropdown-content mt-3 z-[1] p-2 shadow bg-base-100 rounded-box w-52">
          <li><NuxtLink to="/dashboard">Dashboard</NuxtLink></li>
          <li><NuxtLink to="/upload">Upload</NuxtLink></li>
          <li><NuxtLink to="/profile">Profil</NuxtLink></li>
          <li v-if="isAuthenticated"><NuxtLink to="/subscription">Pretplata</NuxtLink></li>
          <li>
            <a @click.prevent="$emit('toggle-dark')">
              <span class="flex-1">Tema</span>
              <span v-if="darkMode">🌙</span>
              <span v-else>☀️</span>
            </a>
          </li>
          <template v-if="isAuthenticated">
            <li><button @click="logout">Logout</button></li>
          </template>
          <template v-else>
            <li><NuxtLink to="/login">Login</NuxtLink></li>
          </template>
        </ul>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { useAuth } from '~/composables/useAuth'
import UserAvatar from './UserAvatar.vue'
import { useRuntimeConfig } from '#app'

const props = defineProps<{ darkMode: boolean }>()
defineEmits(['toggle-dark'])

// Use auth composable
const { isAuthenticated, user, logout } = useAuth()

// Search state
const searchQuery = ref('')
const searchResults = ref<any[]>([])
const searchLoading = ref(false)
const searchError = ref('')
const showSearchResults = ref(false)
let searchTimeout: NodeJS.Timeout | null = null

// Use runtimeConfig
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl
// Search functions
function debounceSearch() {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
  searchTimeout = setTimeout(() => {
    performSearch()
  }, 300)
}

async function performSearch() {
  if (searchQuery.value.length < 2) {
    searchResults.value = []
    searchError.value = ''
    return
  }
  
  searchLoading.value = true
  searchError.value = ''
  
  try {
    const response = await fetch(`${apiBaseUrl}/api/auth/search?query=${encodeURIComponent(searchQuery.value)}`)
    if (response.ok) {
      const data = await response.json()
      searchResults.value = data.map(mapUserApiToFrontend)
      if (data.length === 0) {
        searchError.value = 'Nema rezultata.'
    }
  } else {
      const errorData = await response.json()
      searchError.value = errorData.error || 'Greška pri pretraživanju.'
    }
  } catch (err) {
    searchError.value = 'Došlo je do neočekivane greške.'
  } finally {
    searchLoading.value = false
  }
}

function selectUser(userResult: any) {
  // Navigate to user profile
  window.location.href = `/users/${userResult.email}`
  // Clear search
  searchQuery.value = ''
  searchResults.value = []
  showSearchResults.value = false
}

// Close search results when clicking outside
function handleClickOutside(event: Event) {
  const target = event.target as Element
  if (!target.closest('.relative')) {
    showSearchResults.value = false
  }
}

// Load user data from localStorage on mount
onMounted(() => {
  // Add click outside listener
  document.addEventListener('click', handleClickOutside)
  
  // Listen for user data updates
  window.addEventListener('user-data-updated', (event: any) => {
    // Force reactivity update by triggering a re-render
    // The user ref from useAuth should already be updated, but this ensures the navbar re-renders
  })
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  window.removeEventListener('user-data-updated', () => {})
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
})
</script>