<template>
  <section class="container mx-auto px-4 py-8">
    <!-- Loading State -->
    <div v-if="!user" class="flex justify-center items-center py-12">
      <div class="loading loading-spinner loading-lg"></div>
      <span class="ml-4">Učitavam...</span>
    </div>
    
    <div v-else class="max-w-4xl mx-auto space-y-8">
      <!-- Header Section -->
      <div class="text-center mb-8">
        <h1 class="text-4xl font-bold mb-2">Upload Video</h1>
        <p class="text-lg text-base-content/70">Podijeli svoj fitness sadržaj s GymTube zajednicom</p>
      </div>

      <!-- Upload Form Card -->
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-2xl mb-6">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
            Novi Video
          </h2>
          
          <form @submit.prevent="handleUpload" class="space-y-6">
            <!-- Video Information -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <!-- Title and Description -->
              <div class="space-y-4">
                <div class="form-control">
                  <label class="label">
                    <span class="label-text font-medium">Naslov videa *</span>
                  </label>
                  <input 
                    v-model="title" 
                    type="text" 
                    placeholder="Unesite naslov videa" 
                    class="input input-bordered w-full" 
                    required 
                  />
                </div>
                
                <div class="form-control">
                  <label class="label">
                    <span class="label-text font-medium">Opis</span>
                  </label>
                  <textarea 
                    v-model="description" 
                    placeholder="Unesite opis videa (opcionalno)" 
                    class="textarea textarea-bordered w-full" 
                    rows="4"
                  ></textarea>
                </div>
                
                <!-- Premium Toggle -->
                <div v-if="user?.is_premium" class="form-control">
                  <label class="label cursor-pointer">
                    <span class="label-text font-medium">Premium Video</span> 
                    <input v-model="isPremium" type="checkbox" class="toggle toggle-primary" />
                  </label>
                  <label class="label">
                    <span class="label-text-alt text-base-content/60">Označite ako je ovo premium sadržaj dostupan samo pretplatnicima.</span>
                  </label>
                </div>
                <div class="form-control">
                  <label class="label cursor-pointer">
                    <span class="label-text font-medium">Privatni video</span>
                    <input v-model="isPrivate" type="checkbox" class="toggle toggle-secondary" />
                  </label>
                  <label class="label">
                    <span class="label-text-alt text-base-content/60">Ako je uključeno, video je vidljiv samo vama (na dashboardu).</span>
                  </label>
                </div>
              </div>

              <!-- File Upload Area -->
              <div class="space-y-4">
                <div class="form-control">
                  <label class="label">
                    <span class="label-text font-medium">Video datoteka *</span>
                  </label>
                  
                  <!-- File Upload Zone -->
                  <div 
                    class="border-2 border-dashed border-base-300 rounded-lg p-8 text-center hover:border-primary transition-colors cursor-pointer"
                    @click="triggerFileInput"
                    @dragover.prevent
                    @drop.prevent="handleFileDrop"
                  >
                    <input 
                      ref="fileInput" 
                      type="file" 
                      accept="video/*" 
                      class="hidden" 
                      @change="onFileSelected" 
                    />
                    
                    <div v-if="!selectedFile" class="space-y-4">
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-base-content/30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                      </svg>
                      <div>
                        <p class="text-lg font-medium">Odaberite video datoteku</p>
                        <p class="text-sm text-base-content/60">ili prevucite ovdje</p>
                      </div>
                      <button type="button" class="btn btn-outline btn-sm">
                        Odaberi datoteku
                      </button>
                    </div>
                    
                    <div v-else class="space-y-4">
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12 mx-auto text-success" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <div>
                        <p class="font-medium">{{ selectedFile.name }}</p>
                        <p class="text-sm text-base-content/60">{{ formatFileSize(selectedFile.size) }}</p>
                      </div>
                      <button type="button" class="btn btn-outline btn-sm" @click="removeFile">
                        Ukloni datoteku
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Upload Progress -->
            <div v-if="uploadProgress > 0" class="space-y-2">
              <div class="flex justify-between text-sm">
                <span>Upload u tijeku...</span>
                <span>{{ Math.round(uploadProgress) }}%</span>
              </div>
              <progress class="progress progress-primary w-full" :value="uploadProgress" max="100"></progress>
            </div>

            <!-- Error and Success Messages -->
            <div v-if="error" class="alert alert-error">
              <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>{{ error }}</span>
            </div>
            
            <div v-if="successMessage" class="alert alert-success">
              <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>{{ successMessage }}</span>
            </div>

            <!-- Action Buttons -->
            <div class="flex gap-4 pt-4">
              <button 
                type="submit" 
                class="btn btn-primary flex-1" 
                :disabled="uploading || !selectedFile || !title.trim()"
              >
                <span v-if="uploading" class="loading loading-spinner loading-sm"></span>
                <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                {{ uploading ? 'Uploadam...' : 'Upload Video' }}
              </button>
              
              <NuxtLink to="/dashboard" class="btn btn-outline">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                </svg>
                Odustani
              </NuxtLink>
            </div>
          </form>
        </div>
      </div>

      <!-- Upload Guidelines -->
      <div class="card bg-base-200 shadow-lg">
        <div class="card-body">
          <h3 class="card-title text-lg mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Smjernice za upload
          </h3>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="space-y-2">
              <h4 class="font-medium">Podržani formati:</h4>
              <ul class="text-sm text-base-content/70 space-y-1">
                <li>• MP4, MOV, AVI, WMV</li>
                <li>• Maksimalna veličina: 2GB</li>
                <li>• Preporučena rezolucija: 720p ili više</li>
              </ul>
            </div>
            
            <div class="space-y-2">
              <h4 class="font-medium">Preporučeno:</h4>
              <ul class="text-sm text-base-content/70 space-y-1">
                <li>• Kratki i jasni naslovi</li>
                <li>• Detaljni opisi sadržaja</li>
                <li>• Kvalitetna video i audio</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { useRuntimeConfig } from '#app'

interface User {
  id: string;
  name: string;
  email: string;
  is_premium: boolean;
  profileImageUrl?: string;
}

const router = useRouter()
const user = ref<User | null>(null)
const uploading = ref(false)
const uploadProgress = ref(0)
const title = ref('')
const description = ref('')
const isPremium = ref(false)
const isPrivate = ref(false)
const selectedFile = ref<File | null>(null)
const error = ref('')
const successMessage = ref('')
const fileInput = ref<HTMLInputElement | null>(null)
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl

onMounted(() => {
  const token = localStorage.getItem('authToken')
  const userData = localStorage.getItem('user')
  
  if (!token || !userData) {
    router.push('/login')
    return
  }
  
  try {
    const userObj = JSON.parse(userData)
    user.value = mapUserApiToFrontend(userObj)
  } catch (e) {
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
    router.push('/login')
  }
})

function triggerFileInput() {
  fileInput.value?.click()
}

function onFileSelected(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files[0]) {
    selectedFile.value = target.files[0]
    error.value = ''
  }
}

function handleFileDrop(event: DragEvent) {
  event.preventDefault()
  const files = event.dataTransfer?.files
  if (files && files[0]) {
    selectedFile.value = files[0]
    error.value = ''
  }
}

function removeFile() {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

function formatFileSize(bytes: number) {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

async function handleUpload() {
  if (!selectedFile.value || !title.value.trim()) {
    error.value = "Molimo unesite naslov i odaberite video datoteku."
    if (typeof window !== 'undefined' && (window as any).$notify) {
      (window as any).$notify({ message: error.value, type: 'error' })
    }
    return
  }

  if (!selectedFile.value.type.startsWith('video/')) {
    await (window as any).$alert({ message: 'Molimo odaberite video datoteku.' })
    return
  }
  if (selectedFile.value.size > 2 * 1024 * 1024 * 1024) {
    await (window as any).$alert({ message: 'Video datoteka ne smije biti veća od 2GB.' })
    return
  }

  uploading.value = true
  error.value = ''
  successMessage.value = ''
  uploadProgress.value = 0

  try {
    // 1. Iniciraj upload s našim backendom
    const token = localStorage.getItem('authToken')
    const initiateResponse = await fetch(`${apiBaseUrl}/api/videos/initiate-upload`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        title: title.value.trim(),
        description: description.value.trim(),
        isPremium: isPremium.value,
        isPrivate: isPrivate.value
      })
    })

    if (!initiateResponse.ok) {
      const responseText = await initiateResponse.text()
      throw new Error(`Backend error: ${responseText}`)
    }

    const responseText = await initiateResponse.text()
    
    let responseData
    try {
      responseData = JSON.parse(responseText)
    } catch (e) {
      throw new Error(`Neispravan JSON odgovor: ${responseText}`)
    }

    const { uploadUrl } = responseData

    if (!uploadUrl) {
      throw new Error('Backend nije vratio upload URL.')
    }

    // 2. Upload the file directly to Mux using the signed URL
    const xhr = new XMLHttpRequest()
    xhr.open('PUT', uploadUrl, true)
    
    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) {
        const percentComplete = (event.loaded / event.total) * 100
        uploadProgress.value = percentComplete
      }
    }

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        successMessage.value = "Video uspješno uploadan! Obrađuje se..."
        if (typeof window !== 'undefined' && (window as any).$notify) {
          (window as any).$notify({ message: successMessage.value, type: 'success' })
        }
        // Reset form
        title.value = ''
        description.value = ''
        isPremium.value = false
        isPrivate.value = false
        removeFile()
        uploadProgress.value = 0
        
        // Redirect to dashboard after a short delay
        setTimeout(() => {
          router.push('/dashboard')
        }, 2000)
      } else {
        error.value = `Greška pri uploadu na Mux: ${xhr.statusText}`
        if (typeof window !== 'undefined' && (window as any).$notify) {
          (window as any).$notify({ message: error.value, type: 'error' })
        }
      }
      uploading.value = false
    }

    xhr.onerror = () => {
      error.value = "Došlo je do mrežne greške prilikom uploada."
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
      uploading.value = false
    }
    
    xhr.send(selectedFile.value)

  } catch (e) {
    if (e instanceof Error) {
      error.value = e.message
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
    } else {
      error.value = "Dogodila se nepoznata greška."
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
    }
    uploading.value = false
  }
}
</script>