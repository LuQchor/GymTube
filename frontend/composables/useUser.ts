import { ref } from 'vue'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { useRuntimeConfig } from '#app'

export function useUser() {
  const loading = ref(false)
  const error = ref('')

  async function updateUserName(newName: string) {
    loading.value = true
    error.value = ''
    const config = useRuntimeConfig()
    try {
      const token = localStorage.getItem('authToken')
      const response = await fetch(`${config.public.apiBaseUrl}/api/auth/update-name`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ newName: newName.trim() })
      })
      
      const responseText = await response.text()
      const data = responseText ? JSON.parse(responseText) : {}
      
      if (response.ok) {
        if (typeof window !== 'undefined' && (window as any).$notify) {
          (window as any).$notify({ message: 'Korisničko ime uspješno ažurirano!', type: 'success' })
        }
        return { success: true }
      } else {
        throw new Error(data.error || 'Neuspjela promjena imena.')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Došlo je do greške.'
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function updateProfileDetails(profileData: {
    description?: string
    gender?: string
    birthDate?: string
    isProfilePrivate?: boolean
  }) {
    loading.value = true
    error.value = ''
    const config = useRuntimeConfig()
    try {
      const token = localStorage.getItem('authToken')
      const response = await fetch(`${config.public.apiBaseUrl}/api/auth/update-profile`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          description: profileData.description,
          gender: profileData.gender,
          birthDate: profileData.birthDate ? new Date(profileData.birthDate).toISOString() : null,
          isProfilePrivate: profileData.isProfilePrivate
        })
      })
      
      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        throw new Error(data.error || 'Neuspjelo ažuriranje podataka.')
      }

      // If response is OK but has no content, response.json() will fail.
      // We can just proceed assuming success.
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: 'Podaci profila uspješno ažurirani!', type: 'success' })
      }
      return { success: true }

    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Došlo je do greške.'
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function uploadProfileImage(file: File) {
    loading.value = true
    error.value = ''
    const config = useRuntimeConfig()
    try {
      const token = localStorage.getItem('authToken')
      const formData = new FormData()
      formData.append('file', file)
      
      const response = await fetch(`${config.public.apiBaseUrl}/api/auth/upload-profile-image`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      })
      
      const responseText = await response.text()
      const data = responseText ? JSON.parse(responseText) : {}
      
      if (response.ok) {
        if (typeof window !== 'undefined' && (window as any).$notify) {
          (window as any).$notify({ message: data.message || 'Profilna slika je uspješno ažurirana!', type: 'success' })
        }
        return { success: true, data }
      } else {
        throw new Error(data.error || 'Došlo je do greške prilikom uploada slike.')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Došlo je do greške prilikom uploada slike.'
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function removeProfileImage() {
    loading.value = true
    error.value = ''
    const config = useRuntimeConfig()
    try {
      const token = localStorage.getItem('authToken')
      const response = await fetch(`${config.public.apiBaseUrl}/api/auth/remove-profile-image`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })
      
      const responseText = await response.text()
      const data = responseText ? JSON.parse(responseText) : {}
      
      if (response.ok) {
        if (typeof window !== 'undefined' && (window as any).$notify) {
          (window as any).$notify({ message: data.message || 'Profilna slika je uspješno uklonjena!', type: 'success' })
        }
        return { success: true, data }
      } else {
        throw new Error(data.error || 'Došlo je do greške prilikom uklanjanja slike.')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Došlo je do greške prilikom uklanjanja slike.'
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function deleteAccount(password?: string) {
    loading.value = true
    error.value = ''
    const config = useRuntimeConfig()
    try {
      const token = localStorage.getItem('authToken')
      const response = await fetch(`${config.public.apiBaseUrl}/api/auth/delete-account`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ password })
      })
      
      const responseText = await response.text()
      const data = responseText ? JSON.parse(responseText) : {}
      
      if (response.ok) {
        if (typeof window !== 'undefined' && (window as any).$notify) {
          (window as any).$notify({ message: data.message || 'Račun je uspješno obrisan!', type: 'success' })
        }
        return { success: true, data }
      } else {
        throw new Error(data.error || 'Došlo je do greške prilikom brisanja računa.')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Došlo je do greške prilikom brisanja računa.'
      if (typeof window !== 'undefined' && (window as any).$notify) {
        (window as any).$notify({ message: error.value, type: 'error' })
      }
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    updateUserName,
    updateProfileDetails,
    uploadProfileImage,
    removeProfileImage,
    deleteAccount
  }
} 