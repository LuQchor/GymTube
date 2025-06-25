<template>
  <section class="container mx-auto px-4 py-8">
    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center items-center py-12">
      <div class="loading loading-spinner loading-lg"></div>
      <span class="ml-4">Učitavam videe...</span>
    </div>
    
    <div v-else class="space-y-8">
      <!-- Header Section -->
      <div class="text-center mb-8">
        <h1 class="text-4xl font-bold mb-2">GymTube</h1>
        <p class="text-lg text-base-content/70">Otkrij najbolje fitness video sadržaje</p>
      </div>

      <!-- Search and Filters Section -->
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body">
          <!-- Search Bar -->
          <div class="form-control mb-6">
            <div class="input-group">
              <input 
                v-model="searchQuery" 
                type="text" 
                placeholder="Pretraži videe, autore..." 
                class="input input-bordered flex-1"
                @keyup.enter="performSearch"
              />
              <button class="btn btn-square btn-primary" @click="performSearch">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Filter Buttons -->
          <div class="flex flex-wrap gap-3 justify-center">
            <button 
              @click="setFilter('all')" 
              class="btn" 
              :class="currentFilter === 'all' ? 'btn-primary' : 'btn-outline'"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
              Svi videi
            </button>
            
            <button 
              @click="setFilter('premium')" 
              class="btn" 
              :class="currentFilter === 'premium' ? 'btn-primary' : 'btn-outline'"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
              </svg>
              Premium
            </button>
            
            <button 
              @click="setFilter('free')" 
              class="btn" 
              :class="currentFilter === 'free' ? 'btn-primary' : 'btn-outline'"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Besplatni
            </button>
          </div>

          <!-- Sort Options -->
          <div class="flex flex-wrap gap-3 justify-center mt-4">
            <button 
              @click="setSort('recent')" 
              class="btn btn-sm" 
              :class="currentSort === 'recent' ? 'btn-primary' : 'btn-outline'"
            >
              Najnoviji
            </button>
            
            <button 
              @click="setSort('popular')" 
              class="btn btn-sm" 
              :class="currentSort === 'popular' ? 'btn-primary' : 'btn-outline'"
            >
              Najpopularniji
            </button>
            
            <button 
              @click="setSort('oldest')" 
              class="btn btn-sm" 
              :class="currentSort === 'oldest' ? 'btn-primary' : 'btn-outline'"
            >
              Najstariji
            </button>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div v-if="error" class="alert alert-error">
        <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <span>{{ error }}</span>
      </div>

      <!-- Videos Section -->
      <div v-if="filteredVideos.length > 0">
        <div class="flex justify-between items-center mb-6">
          <h3 class="text-2xl font-bold">Video sadržaj</h3>
          <div class="text-sm text-base-content/60">
            {{ commonCounts.video(pagination.totalCount) }}
          </div>
        </div>

        <!-- Videos Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          <VideoCard
            v-for="video in filteredVideos"
            :key="video.id"
            :video="video"
            mode="public"
            :user-is-premium="userIsPremium"
            :is-authenticated="isAuthenticated"
            @play="playVideo"
            @vote="voteOnVideo"
          />
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="text-center py-16">
        <div class="card bg-base-100 shadow-xl max-w-md mx-auto">
          <div class="card-body text-center">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-24 w-24 mx-auto text-base-content/30 mb-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
            <h3 class="text-2xl font-bold mb-3">Nema videa</h3>
            <p class="text-base-content/70 mb-6">
              {{ searchQuery ? 'Nema rezultata za vašu pretragu.' : 'Još nema uploadanih videa.' }}
            </p>
            <div v-if="isAuthenticated" class="space-y-3">
              <NuxtLink to="/upload" class="btn btn-primary btn-lg">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
                </svg>
                Upload prvi video
              </NuxtLink>
            </div>
            <div v-else class="flex justify-center gap-4 pt-4">
              <NuxtLink to="/register" class="btn btn-primary btn-lg">
                Registriraj se
              </NuxtLink>
              <NuxtLink to="/login" class="btn btn-outline btn-lg">
                Prijavi se
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Video Player Modal -->
    <VideoPlayerModal 
      :visible="showPlayer"
      :playback-id="currentPlaybackId"
      @close="closePlayer"
    />

    <!-- Premium Modal -->
    <div v-if="showPremiumModal" class="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4">
      <div class="modal-box text-center">
        <h3 class="font-bold text-lg text-primary">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 inline-block mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6.364-6.364l-1.414-1.414M2.32 12h2m15.36 0h2M12 2.32V4.32m7.07-1.95l-1.414 1.414M5.636 5.636L4.222 4.222m12.122 12.122l1.414 1.414M18.364 5.636l-1.414 1.414" />
          </svg>
          Premium Sadržaj
        </h3>
        <p class="py-4">Ovaj video je dostupan samo za premium korisnike. Nadogradite svoj račun za pristup ekskluzivnom sadržaju!</p>
        <div class="modal-action justify-center gap-4">
          <NuxtLink to="/subscription" class="btn btn-primary">Nadogradi na Premium</NuxtLink>
          <button @click="showPremiumModal = false" class="btn btn-outline">Zatvori</button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { commonCounts } from '~/utils/grammar'
import { useAuth } from '~/composables/useAuth'
import VideoCard from '~/components/VideoCard.vue'
import VideoPlayerModal from '~/components/VideoPlayerModal.vue'
import { useRuntimeConfig } from '#app'

const router = useRouter()

// Koristi auth composable
const { isAuthenticated, userIsPremium, refreshUserData } = useAuth()

// Stanje
const videos = ref<any[]>([])
const loading = ref(true)
const error = ref('')
const searchQuery = ref('')
const currentFilter = ref('all')
const currentSort = ref('recent')
const currentPage = ref(1)
const showPlayer = ref(false)
const showPremiumModal = ref(false)
const currentPlaybackId = ref<string | null>(null)

// Paginacija
const pagination = ref({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0,
  pageSize: 12,
  hasNextPage: false,
  hasPreviousPage: false
})

const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl

onMounted(async () => {
  // Refresh user data to get latest premium status
  await refreshUserData()
  
  // Load videos
  await loadVideos()
  
  // Listen for visibility change to refresh data when user returns to the app
  document.addEventListener('visibilitychange', handleVisibilityChange)

  // Dodaj listener za globalni refresh
  window.addEventListener('videos-updated', loadVideos)
})

onUnmounted(() => {
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  window.removeEventListener('videos-updated', loadVideos)
})

function handleVisibilityChange() {
  if (!document.hidden) {
    // User returned to the app, refresh user data
    refreshUserData()
  }
}

async function loadVideos() {
  loading.value = true
  error.value = ''
  
  try {
    const params = new URLSearchParams({
      sortBy: currentSort.value,
      page: currentPage.value.toString(),
      pageSize: '12'
    })
    
    if (searchQuery.value.trim()) {
      params.append('search', searchQuery.value.trim())
    }
    
    const token = localStorage.getItem('authToken')
    let fetchOptions = {}
    if (token) {
      fetchOptions = { headers: { 'Authorization': `Bearer ${token}` } }
    }
    const response = await fetch(`${apiBaseUrl}/api/videos/all?${params}`, fetchOptions)
    
    if (response.ok) {
      const data = await response.json()
      videos.value = data.videos.map(mapVideoApiToFrontend)
      pagination.value = data.pagination
    } else {
      throw new Error('Neuspjelo dohvaćanje videa.')
    }
  } catch (e) {
    error.value = 'Došlo je do greške pri učitavanju videa.'
  } finally {
    loading.value = false
  }
}

function setFilter(filter: string) {
  currentFilter.value = filter
  currentPage.value = 1
  loadVideos()
}

function setSort(sort: string) {
  currentSort.value = sort
  currentPage.value = 1
  loadVideos()
}

function changePage(page: number) {
  currentPage.value = page
  loadVideos()
}

function performSearch() {
  currentPage.value = 1
  loadVideos()
}

function getVisiblePages() {
  const pages = []
  const totalPages = pagination.value.totalPages
  const current = pagination.value.currentPage
  
  if (totalPages <= 7) {
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 4) {
      for (let i = 1; i <= 5; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(totalPages)
    } else if (current >= totalPages - 3) {
      pages.push(1)
      pages.push('...')
      for (let i = totalPages - 4; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      pages.push(1)
      pages.push('...')
      for (let i = current - 1; i <= current + 1; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(totalPages)
    }
  }
  
  return pages
}

function playVideo(video: any) {
  if (video.isPremium && !userIsPremium.value) {
    showPremiumModal.value = true
    return
  }

  if (video.muxPlaybackId) {
    currentPlaybackId.value = video.muxPlaybackId
    showPlayer.value = true
  }
}

function closePlayer() {
  showPlayer.value = false
  currentPlaybackId.value = null
}

async function voteOnVideo(videoId: string, voteType: 'like' | 'dislike') {
  if (!isAuthenticated.value) {
    alert('Morate biti prijavljeni da možete glasati.')
    return
  }
  
  try {
    const token = localStorage.getItem('authToken')
    const response = await fetch(`${apiBaseUrl}/api/videos/${videoId}/vote`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ voteType })
    })
    
    if (response.ok) {
      const result = await response.json()
      // Ažuriraj video u listi
      const videoIndex = videos.value.findIndex(v => v.id === videoId)
      if (videoIndex !== -1) {
        videos.value[videoIndex].likes = result.likes
        videos.value[videoIndex].dislikes = result.dislikes
        videos.value[videoIndex].userVote = result.userVote
      }
    } else {
      const data = await response.json()
      alert(data.error || 'Greška pri glasanju.')
    }
  } catch (e) {
    alert('Došlo je do greške pri glasanju.')
  }
}

const filteredVideos = computed(() => {
  if (currentFilter.value === 'premium') {
    return videos.value.filter(v => v.isPremium)
  } else if (currentFilter.value === 'free') {
    return videos.value.filter(v => !v.isPremium)
  }
  return videos.value
})
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-clamp: 2;
}
</style>
