<template>
  <section class="container mx-auto px-4 py-8">
    <!-- Loading State -->
    <div v-if="!user" class="flex justify-center items-center py-12">
      <div class="loading loading-spinner loading-lg"></div>
      <span class="ml-4">Učitavam profil...</span>
    </div>
    
    <div v-else class="space-y-8">
      <!-- Header Section -->
      <div class="text-center mb-8">
        <h1 class="text-4xl font-bold mb-2">Moj Profil</h1>
        <p class="text-lg text-base-content/70">Upravljaj svojim računom i postavkama</p>
      </div>

      <!-- Profile Overview Card -->
      <div class="card bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20">
        <div class="card-body">
          <div class="flex flex-col lg:flex-row items-center lg:items-start gap-6">
            <!-- Profile Image Section -->
            <div class="flex flex-col items-center space-y-4">
              <UserAvatar :user="user" size="3xl" :show-ring="true" />
              
              <!-- Image Upload Controls -->
              <div class="flex flex-col gap-2">
                <input ref="fileInput" type="file" accept="image/*" class="hidden" @change="onFileSelected" />
                <button @click="triggerFileInput" class="btn btn-primary btn-sm" :disabled="userLoading">
                  <span v-if="userLoading" class="loading loading-spinner loading-xs"></span>
                  <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ user.profileImageUrl ? 'Promijeni sliku' : 'Dodaj sliku' }}
                </button>
                <button v-if="user.profileImageUrl" @click="removeProfileImage" class="btn btn-outline btn-sm" :disabled="userLoading">
                  <span v-if="userLoading" class="loading loading-spinner loading-xs"></span>
                  <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                  Ukloni sliku
                </button>
              </div>
            </div>

            <!-- User Info Section -->
            <div class="flex-1 text-center lg:text-left">
              <div class="space-y-4">
                <div>
                  <h2 class="text-3xl font-bold mb-2">{{ user.name }}</h2>
                  <p class="text-lg text-base-content/70">{{ user.email }}</p>
                </div>
                
                <div class="flex flex-wrap gap-3 justify-center lg:justify-start items-center">
                  <div v-if="user.is_premium" :class="['badge', 'badge-lg', 'badge-success', 'h-10', 'px-4', 'flex', 'items-center', 'text-base']">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                    </svg>
                    Premium korisnik
                  </div>
                  <div v-else :class="['badge', 'badge-lg', 'badge-neutral', 'h-10', 'px-4', 'flex', 'items-center', 'text-base']">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5.121 17.804A9 9 0 1112 21a9 9 0 01-6.879-3.196z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    Basic korisnik
                  </div>
                  
                  <NuxtLink to="/subscription" class="btn btn-outline btn-sm h-10">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    Upravljaj pretplatom
                  </NuxtLink>
                </div>

                <div v-if="user.is_premium && user.premium_expires_at" class="bg-base-200 rounded-lg p-3">
                  <p class="text-sm text-base-content/70">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 inline mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    Pretplata vrijedi do: <span class="font-semibold">{{ formatDate(user.premium_expires_at) }}</span>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Profile Details & Settings -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Change User Data (Name & Password) -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h3 class="card-title text-xl mb-4">Promjena korisničkih podataka</h3>
            <form v-if="user.hasPassword" @submit.prevent="updateUserName" class="space-y-4">
              <div class="form-control">
                <label for="userName" class="label">
                  <span class="label-text font-medium">Novo korisničko ime</span>
                </label>
                <input id="userName" v-model="editableName" type="text" class="input input-bordered w-full" required minlength="2" maxlength="50" />
                <div class="label">
                  <span class="label-text-alt text-base-content/60">Ime mora biti jedinstveno.</span>
                </div>
              </div>
              <!-- Change Password Fields -->
              <div class="form-control">
                <label for="currentPassword" class="label">
                  <span class="label-text font-medium">Trenutna lozinka</span>
                </label>
                <input id="currentPassword" v-model="currentPassword" type="password" class="input input-bordered w-full" minlength="6" />
              </div>
              <div class="form-control">
                <label for="newPassword" class="label">
                  <span class="label-text font-medium">Nova lozinka</span>
                </label>
                <input id="newPassword" v-model="newPassword" type="password" class="input input-bordered w-full" minlength="6" />
              </div>
              <div class="form-control">
                <label for="confirmNewPassword" class="label">
                  <span class="label-text font-medium">Potvrda nove lozinke</span>
                </label>
                <input id="confirmNewPassword" v-model="confirmNewPassword" type="password" class="input input-bordered w-full" minlength="6" />
              </div>
              <div v-if="passwordError" class="alert alert-error py-2">{{ passwordError }}</div>
              <div v-if="passwordSuccess" class="alert alert-success py-2">{{ passwordSuccess }}</div>
              <div class="flex gap-3 pt-2">
                <button type="submit" class="btn btn-primary flex-1" :disabled="userLoading || passwordLoading">
                  <span v-if="userLoading || passwordLoading" class="loading loading-spinner loading-sm"></span>
                  Spremi promjene
                </button>
              </div>
            </form>
            <div v-else class="space-y-4">
              <div class="alert alert-info">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                Trenutno nemate postavljenu lozinku. Postavite lozinku kako biste mogli brisati videe i koristiti dodatne sigurnosne opcije poput promjene korisnickog imena.
              </div>
              <form @submit.prevent="setPasswordForGoogleUser" class="space-y-4">
                <div class="form-control">
                  <label for="setNewPassword" class="label">
                    <span class="label-text font-medium">Nova lozinka</span>
                  </label>
                  <input id="setNewPassword" v-model="setNewPassword" type="password" class="input input-bordered w-full" minlength="6" required />
                </div>
                <div class="form-control">
                  <label for="setConfirmNewPassword" class="label">
                    <span class="label-text font-medium">Potvrda nove lozinke</span>
                  </label>
                  <input id="setConfirmNewPassword" v-model="setConfirmNewPassword" type="password" class="input input-bordered w-full" minlength="6" required />
                </div>
                <div v-if="setPasswordError" class="alert alert-error py-2">{{ setPasswordError }}</div>
                <div v-if="setPasswordSuccess" class="alert alert-success py-2">{{ setPasswordSuccess }}</div>
                <div class="flex gap-3 pt-2">
                  <button type="submit" class="btn btn-primary flex-1" :disabled="setPasswordLoading">
                    <span v-if="setPasswordLoading" class="loading loading-spinner loading-sm"></span>
                    Postavi lozinku
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <!-- Additional Profile Info -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h3 class="card-title text-xl mb-4">Dodatni podaci</h3>
            <form @submit.prevent="updateProfileDetails" class="space-y-4">
              <div class="form-control">
                <label for="userDescription" class="label">
                  <span class="label-text font-medium">Opis profila</span>
                </label>
                <textarea id="userDescription" v-model="profile.description" class="textarea textarea-bordered w-full" rows="3" placeholder="Napišite nešto o sebi..."></textarea>
              </div>
              
              <div class="grid grid-cols-2 gap-4">
                <div class="form-control">
                  <label for="userGender" class="label">
                    <span class="label-text font-medium">Spol</span>
                  </label>
                  <select id="userGender" v-model="profile.gender" class="select select-bordered w-full">
                    <option disabled value="">Odaberi...</option>
                    <option value="Male">Muško</option>
                    <option value="Female">Žensko</option>
                    <option value="Other">Drugo</option>
                  </select>
                </div>
                <div class="form-control">
                  <label for="userBirthDate" class="label">
                    <span class="label-text font-medium">Datum rođenja</span>
                  </label>
                  <input id="userBirthDate" v-model="profile.birthDate" type="date" class="input input-bordered w-full" />
                </div>
              </div>

              <div class="form-control">
                <label class="label cursor-pointer">
                  <span class="label-text font-medium">Privatni profil</span>
                  <input type="checkbox" v-model="profile.isProfilePrivate" class="toggle toggle-secondary" />
                </label>
              </div>

              <div class="flex gap-3 pt-2">
                <button type="submit" class="btn btn-primary flex-1" :disabled="userLoading">
                  <span v-if="userLoading" class="loading loading-spinner loading-sm"></span>
                  Spremi podatke
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- Account Actions Card -->
      <div class="card bg-base-100 shadow-xl max-w-md w-full mx-auto mt-6">
        <div class="card-body">
          <h3 class="card-title text-xl mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            Akcije računa
          </h3>
          
          <div class="space-y-4">
            <NuxtLink to="/dashboard" class="btn btn-outline w-full justify-start">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h4a2 2 0 012 2v6H8V5z" />
              </svg>
              Povratak na Dashboard
            </NuxtLink>
            
            <NuxtLink to="/upload" class="btn btn-outline w-full justify-start">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
              Upload novi video
            </NuxtLink>
            
            <div class="divider">Opasne akcije</div>
            
            <button @click="showDeleteAccountModal = true" class="btn btn-error w-full justify-start">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
              Obriši profil
            </button>
            <UiDialog
              v-model="showDeleteAccountModal"
              type="confirm"
              :message="'Jeste li sigurni da želite obrisati svoj profil? Ova radnja je nepovratna i svi vaši videi i podaci bit će trajno obrisani. Ako ste premium korisnik, prvo vam mora isteći pretplata, mozete ju otkazati ako želite'"
              confirm-text="Obriši profil"
              cancel-text="Odustani"
              :password="user?.hasPassword"
              @confirm="handleDeleteAccount"
            />
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuth } from '~/composables/useAuth'
import { useUser } from '~/composables/useUser'
import UserAvatar from '~/components/UserAvatar.vue'
import UiDialog from '~/components/UiDialog.vue'
import { useRuntimeConfig } from '#app'

const router = useRouter()
const route = useRoute()

// Koristi composables
const { user, refreshUserData, logout } = useAuth()
const { loading: userLoading, updateUserName: updateUserNameApi, updateProfileDetails: updateProfileDetailsApi, uploadProfileImage: uploadProfileImageApi, removeProfileImage: removeProfileImageApi, deleteAccount: deleteAccountApi } = useUser()

const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl

// Lokalno stanje (kao EditVideoModal)
const profile = ref({
  description: '',
  gender: '',
  birthDate: '',
  isProfilePrivate: false
})
const editableName = ref('')
const fileInput = ref<HTMLInputElement | null>(null)

const paymentSuccess = ref(false)
const paymentCanceled = ref(false)
const polling = ref(false)

const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const passwordError = ref('')
const passwordSuccess = ref('')
const passwordLoading = ref(false)

const setNewPassword = ref('')
const setConfirmNewPassword = ref('')
const setPasswordError = ref('')
const setPasswordSuccess = ref('')
const setPasswordLoading = ref(false)

const showDeleteAccountModal = ref(false)

// Watcher da održava lokalno stanje sinkronizirano s user.value (kao EditVideoModal)
watch(() => user.value, (newUser) => {
  if (newUser) {
    profile.value = {
      description: newUser.description || '',
      gender: newUser.gender || '',
      birthDate: newUser.birthDate ? new Date(newUser.birthDate).toISOString().split('T')[0] : '',
      isProfilePrivate: newUser.isProfilePrivate || false,
    }
    editableName.value = newUser.name
  } else {
    // Reset values when user is null
    profile.value = {
      description: '',
      gender: '',
      birthDate: '',
      isProfilePrivate: false
    }
    editableName.value = ''
  }
}, { immediate: true })

onMounted(() => {
  // Provjeri status plaćanja iz URL-a
  paymentSuccess.value = route.query.payment_success === 'true'
  paymentCanceled.value = route.query.payment_canceled === 'true'
  
  // Provjeri autentifikaciju
  if (!user.value) {
    router.push('/login')
    return
  }
  
  try {
    // Rukuj polling-om za uspjeh plaćanja
    if (paymentSuccess.value && user.value && !user.value.is_premium) {
      startPollingForPremiumStatus()
    }
    
    // Očisti URL
    if (paymentSuccess.value || paymentCanceled.value) {
      router.replace({ query: {} })
    }
    
    // Osvježi korisničke podatke da dohvatiš najnovije informacije
    refreshUserData()
    
    // Slušaj promjenu vidljivosti da osvježiš podatke kad se korisnik vrati u aplikaciju
    document.addEventListener('visibilitychange', handleVisibilityChange)
  } catch (e) {
    // Neispravni korisnički podaci, preusmjeri na login
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
    router.push('/login')
  }
})

onUnmounted(() => {
  document.removeEventListener('visibilitychange', handleVisibilityChange)
})

function handleVisibilityChange() {
  if (!document.hidden) {
    // Korisnik se vratio u aplikaciju, osvježi korisničke podatke
    refreshUserData()
  }
}

function startPollingForPremiumStatus() {
    polling.value = true;
    const pollInterval = setInterval(async () => {
        try {
            await refreshUserData()
            if (user.value?.is_premium) {
                clearInterval(pollInterval);
                polling.value = false;
                // The watcher will automatically update profile.value and editableName.value
            }
        } catch (e) {
            // Handle polling error
        }
    }, 3000);
    setTimeout(() => {
        if (polling.value) {
            clearInterval(pollInterval);
            polling.value = false;
        }
    }, 30000);
}

function formatDate(dateValue: string | number) {
    if (!dateValue) return '';
    let date: Date;
    if (typeof dateValue === 'number') {
        date = new Date(dateValue * 1000);
    } else {
        date = new Date(dateValue);
    }
    if (isNaN(date.getTime())) return '';
    return date.toLocaleDateString('hr-HR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
}

async function onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    if (!target.files || !target.files[0]) return;
    const file = target.files[0];
    if (!file.type.startsWith('image/')) {
        await (window as any).$notify({ message: 'Molimo odaberite sliku.', type: 'error' });
        return;
    }
    if (file.size > 5 * 1024 * 1024) {
        await (window as any).$notify({ message: 'Slika ne smije biti veća od 5MB.', type: 'error' });
        return;
    }
    await uploadProfileImage(file);
}

async function uploadProfileImage(file: File) {
    const result = await uploadProfileImageApi(file)
    if (result.success) {
        await refreshUserData()
    }
    if (fileInput.value) fileInput.value.value = '';
}

async function removeProfileImage() {
    const confirmed = await (window as any).$confirm({ message: 'Jeste li sigurni da želite ukloniti profilnu sliku?' });
    
    if (!confirmed) {
        return;
    }
    
    try {
    const result = await removeProfileImageApi()
    if (result.success) {
        await refreshUserData()
            // Notifikacija se već prikazuje u removeProfileImageApi funkciji
        } else {
            // Ako je došlo do greške, prikaži je
        }
    } catch (error) {
        // Handle unexpected error
    }
}

async function updateUserName() {
  passwordError.value = ''
  passwordSuccess.value = ''
  // Prvo promjena imena
  if (editableName.value.trim() !== user.value.name) {
    const result = await updateUserNameApi(editableName.value)
    if (result.success) {
        await refreshUserData()
    }
  }
  // Zatim promjena lozinke ako su polja popunjena
  if (currentPassword.value || newPassword.value || confirmNewPassword.value) {
    if (!currentPassword.value || !newPassword.value || !confirmNewPassword.value) {
      passwordError.value = 'Sva polja za promjenu lozinke moraju biti popunjena.'
      return
    }
    if (newPassword.value !== confirmNewPassword.value) {
      passwordError.value = 'Nove lozinke se ne podudaraju.'
      return
    }
    if (newPassword.value.length < 6) {
      passwordError.value = 'Nova lozinka mora imati barem 6 znakova.'
      return
    }
    passwordLoading.value = true
    try {
      const token = localStorage.getItem('authToken')
      const response = await fetch(`${apiBaseUrl}/api/auth/update-password`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          currentPassword: currentPassword.value,
          newPassword: newPassword.value
        })
      })
      const data = await response.json()
      if (response.ok) {
        passwordSuccess.value = data.message || 'Lozinka je uspješno promijenjena!'
        // Ako backend vrati novi token, spremi ga
        if (data.token) {
          localStorage.setItem('authToken', data.token)
          await refreshUserData()
        }
        currentPassword.value = ''
        newPassword.value = ''
        confirmNewPassword.value = ''
      } else {
        passwordError.value = data.error || 'Promjena lozinke nije uspjela.'
      }
    } catch (e) {
      passwordError.value = 'Došlo je do greške. Pokušajte ponovno.'
    } finally {
      passwordLoading.value = false
    }
    }
}

async function updateProfileDetails() {
  const result = await updateProfileDetailsApi(profile.value)
  if (result.success) {
    await refreshUserData()
    // Watcher će automatski ažurirati profile.value kad se promijeni user.value
  }
}

function triggerFileInput() {
  fileInput.value?.click()
}

async function setPasswordForGoogleUser() {
  setPasswordError.value = ''
  setPasswordSuccess.value = ''
  if (!setNewPassword.value || !setConfirmNewPassword.value) {
    setPasswordError.value = 'Sva polja su obavezna.'
    return
  }
  if (setNewPassword.value !== setConfirmNewPassword.value) {
    setPasswordError.value = 'Lozinke se ne podudaraju.'
    return
  }
  if (setNewPassword.value.length < 6) {
    setPasswordError.value = 'Lozinka mora imati barem 6 znakova.'
    return
  }
  setPasswordLoading.value = true
  try {
    const token = localStorage.getItem('authToken')
    const response = await fetch(`${apiBaseUrl}/api/auth/set-password`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ newPassword: setNewPassword.value })
    })
    const data = await response.json()
    if (response.ok) {
      setPasswordSuccess.value = data.message || 'Lozinka je uspješno postavljena!'
      if (data.token) {
        localStorage.setItem('authToken', data.token)
        await refreshUserData()
      }
      setNewPassword.value = ''
      setConfirmNewPassword.value = ''
    } else {
      setPasswordError.value = data.error || 'Postavljanje lozinke nije uspjelo.'
    }
  } catch (e) {
    setPasswordError.value = 'Došlo je do greške. Pokušajte ponovno.'
  } finally {
    setPasswordLoading.value = false
  }
}

async function handleDeleteAccount(password?: string) {
  const result = await deleteAccountApi(password)
  
  if (result.success) {
    // Uspješno brisanje - odjavi korisnika i redirect na početnu
    logout()
    router.push('/')
  } else {
    // Error je već prikazan kroz useUser composable
  }
}
</script>
