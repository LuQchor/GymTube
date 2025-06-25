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
        <h1 class="text-4xl font-bold mb-2">Dashboard</h1>
        <p class="text-lg text-base-content/70">Upravljaj svojim fitness video sadržajem</p>
      </div>

      <!-- User Welcome Card -->
      <div v-if="user" class="card bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20">
        <div class="card-body">
          <div class="flex flex-col lg:flex-row items-center lg:items-start gap-6">
            <!-- User Avatar -->
            <UserAvatar :user="user" size="xl" :show-ring="true" />
            
            <!-- User Info -->
            <div class="flex-1 text-center lg:text-left">
              <h2 class="text-2xl font-bold mb-2">Dobrodošao, {{ user.name }}!</h2>
              <p class="text-lg text-base-content/70 mb-3">{{ user.email }}</p>
              
              <div class="flex flex-wrap gap-3 justify-center lg:justify-start items-center">
                <div v-if="user.is_premium" class="badge badge-lg badge-success h-10 px-4 flex items-center text-base">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                  </svg>
                  Premium korisnik
                </div>
                <div v-else class="badge badge-lg badge-neutral h-10 px-4 flex items-center text-base">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5.121 17.804A9 9 0 1112 21a9 9 0 01-6.879-3.196z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  Basic korisnik
                </div>
                
                <div class="badge badge-outline h-10 flex items-center text-base">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                  {{ commonCounts.video(videos.length) }}
                </div>
              </div>
            </div>
            
            <!-- Action Buttons -->
            <div class="flex flex-col gap-3">
              <button @click="loadVideos" class="btn btn-outline btn-sm" :disabled="loading">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                Osvježi
              </button>
              <NuxtLink to="/upload" class="btn btn-primary btn-sm">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
                </svg>
                Upload Video
              </NuxtLink>
            </div>
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
      <div v-if="videos.length > 0">
        <div class="flex justify-between items-center mb-6">
          <h3 class="text-2xl font-bold">Moji Videi</h3>
          <div class="text-sm text-base-content/60">
            {{ commonCounts.video(videos.length) }}
          </div>
        </div>

        <!-- Videos Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          <VideoCard
            v-for="video in videos"
            :key="video.id"
            :video="video"
            mode="dashboard"
            @play="playVideo"
            @edit="openEditModal"
            @delete="deleteVideo"
            @copy-link="copyVideoLink"
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
            <p class="text-base-content/70 mb-6">Učitaj svoj prvi videozapis i počni dijeliti svoj fitness napredak!</p>
            <NuxtLink to="/upload" class="btn btn-primary">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              Upload
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Modals -->
    <EditVideoModal 
      v-if="editingVideo" 
      :visible="!!editingVideo"
      :video="editingVideo" 
      :can-edit-premium="user?.is_premium"
      @save="handleVideoSave"
      @cancel="closeEditModal"
      @delete="handleVideoDelete"
    />
    
    <!-- Video Player Overlay -->
    <VideoPlayerModal 
      :visible="showPlayer"
      :playback-id="currentPlaybackId"
      @close="closePlayer"
    />
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { mapVideoApiToFrontend } from '../utils/videoMapper'
import { commonCounts } from '~/utils/grammar'
import { useAuth } from '~/composables/useAuth'
import UserAvatar from '~/components/UserAvatar.vue'
import VideoCard from '~/components/VideoCard.vue'
import EditVideoModal from '~/components/EditVideoModal.vue'
import VideoPlayerModal from '~/components/VideoPlayerModal.vue'
import PremiumBadge from '~/components/PremiumBadge.vue'
import { useRuntimeConfig } from '#app'

// Koristi auth composable
const { user, refreshUserData } = useAuth()

// Stanje komponente
const videos = ref<any[]>([])
const loading = ref(true)
const error = ref('')
const editingVideo = ref<any | null>(null)
const showPlayer = ref(false)
const currentPlaybackId = ref<string | null>(null)
const router = useRouter()
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl

// Funkcije za video
const loadVideos = async () => {
  loading.value = true
  error.value = ''
  try {
    const token = localStorage.getItem('authToken')
    if (!token) {
      router.push('/login')
      return
    }
    const response = await fetch(`${apiBaseUrl}/api/videos/my-videos`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    if (response.ok) {
      const data = await response.json()
      videos.value = data.map(mapVideoApiToFrontend)
    } else if (response.status === 401) {
      router.push('/login')
    } else {
      error.value = 'Greška pri dohvaćanju videa.'
    }
  } catch (e) {
    error.value = 'Mrežna greška. Pokušajte ponovno.'
  } finally {
    loading.value = false
  }
}

const openEditModal = (video: any) => {
  editingVideo.value = { ...video }
}

const closeEditModal = () => {
  editingVideo.value = null
}

const handleVideoSave = async (updatedData: any) => {
  if (!editingVideo.value) return
  
  try {
    const token = localStorage.getItem('authToken')
    if (!token) return

    const response = await fetch(`${apiBaseUrl}/api/videos/${editingVideo.value.id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(updatedData)
    })

    if (response.ok) {
      const updatedVideo = await response.json()
      const mappedVideo = mapVideoApiToFrontend(updatedVideo)
      
      const index = videos.value.findIndex(v => v.id === editingVideo.value.id)
      if (index !== -1) {
        videos.value[index] = mappedVideo
      }
      
      closeEditModal()
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: 'Video uspješno ažuriran!', type: 'success' })
      }
    } else {
      throw new Error('Ažuriranje nije uspjelo.')
    }
  } catch (e) {
    if (typeof window !== 'undefined' && (window as any).$notify) {
      (window as any).$notify({ message: 'Došlo je do greške prilikom ažuriranja.', type: 'error' })
    }
  }
}

const handleVideoDelete = async () => {
  if (!editingVideo.value) return;
  const password = await (window as any).$confirm({
    message: `Jeste li sigurni da želite obrisati video "${editingVideo.value.title}"? Ova radnja je nepovratna.`,
    confirmText: 'Obriši',
    cancelText: 'Odustani',
    password: true
  });
  if (!password) return;
  const token = localStorage.getItem('authToken');
  try {
    const response = await fetch(`${apiBaseUrl}/api/videos/${editingVideo.value.id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ password })
    });
    if (response.ok) {
      videos.value = videos.value.filter(v => v.id !== editingVideo.value.id);
      closeEditModal();
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: 'Video uspješno obrisan!', type: 'success' });
      }
    } else {
      const data = await response.json();
      throw new Error(data.error || 'Brisanje nije uspjelo.');
    }
  } catch (e) {
    if (typeof window !== 'undefined' && (window as any).$notify) {
      (window as any).$notify({ message: e instanceof Error ? e.message : 'Došlo je do greške prilikom brisanja.', type: 'error' });
    }
  }
};

const playVideo = (video: any) => {
  if (video.muxPlaybackId) {
    currentPlaybackId.value = video.muxPlaybackId
    showPlayer.value = true
  } else if (typeof window !== 'undefined' && (window as any).$notify) {
    (window as any).$notify({
      message: 'Ovaj video se još uvijek obrađuje. Molimo pokušajte ponovo kasnije.',
      type: 'info'
    })
  }
}

const closePlayer = () => {
  showPlayer.value = false
  currentPlaybackId.value = null
}

const deleteVideo = async (video: any) => {
  const password = await (window as any).$confirm({
    message: `Jeste li sigurni da želite obrisati video "${video.title}"? Ova radnja je nepovratna.`,
    confirmText: 'Obriši',
    cancelText: 'Odustani',
    password: true
  });
  
  if (!password) return;
  
  const token = localStorage.getItem('authToken');
  try {
    const response = await fetch(`${apiBaseUrl}/api/videos/${video.id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ password })
    });
  
    if (response.ok) {
      videos.value = videos.value.filter(v => v.id !== video.id);
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: 'Video uspješno obrisan!', type: 'success' });
      }
    } else {
      const data = await response.json();
      throw new Error(data.error || 'Brisanje nije uspjelo.');
    }
  } catch (e) {
    if (typeof window !== 'undefined' && (window as any).$notify) {
      (window as any).$notify({ message: e instanceof Error ? e.message : 'Došlo je do greške prilikom brisanja.', type: 'error' });
    }
  }
};

const copyVideoLink = (video: any) => {
  if (video.muxPlaybackId) {
    const link = `https://stream.mux.com/${video.muxPlaybackId}`;
    navigator.clipboard.writeText(link);
    if (typeof window !== 'undefined' && (window as any).$notify) {
      (window as any).$notify({ message: 'Direktni link je kopiran!', type: 'info' });
    }
  } else {
    if (typeof window !== 'undefined' && (window as any).$notify) {
      (window as any).$notify({ message: 'Video se još uvijek obrađuje.', type: 'warning' })
    }
  }
}

// Lifecycle Hooks
onMounted(async () => {
  if (!user.value) {
    router.push('/login')
    return
  }
  await refreshUserData()
  loadVideos()
  window.addEventListener('videos-updated', loadVideos)
})
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  overflow: hidden;
}
</style>