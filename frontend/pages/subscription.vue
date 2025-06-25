<template>
  <section class="container mx-auto px-4 py-8">
    <h2 class="text-3xl font-bold mb-8 text-center">Upravljanje pretplatom</h2>

    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center items-center py-12">
      <div class="loading loading-spinner loading-lg"></div>
      <span class="ml-4">Učitavam podatke o pretplati...</span>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="alert alert-error mb-8">
      <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <span>{{ error }}</span>
    </div>

    <!-- No Subscription - Show Upgrade Options -->
    <div v-else-if="!subscription" class="max-w-4xl mx-auto">
      <!-- Back to subscription details button (if user had a subscription before) -->
      <div v-if="hadSubscription" class="text-center mb-6">
        <button @click="showSubscriptionDetails" class="btn btn-outline btn-sm">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          Povratak na detalje pretplate
        </button>
      </div>

      <div class="text-center mb-12">
        <div class="mb-6">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-24 w-24 mx-auto text-primary mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="text-2xl font-bold mb-2">Nadogradite na Premium</h3>
          <p class="text-lg text-base-content/70">Dobijte pristup ekskluzivnom sadržaju i naprednim funkcijama!</p>
        </div>

        <!-- Premium Features -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
          <div class="card bg-base-100 shadow-lg border border-base-300">
            <div class="card-body text-center">
              <div class="text-4xl mb-4">🎥</div>
              <h4 class="card-title justify-center">Neograničeni upload</h4>
              <p class="text-sm text-base-content/70">Uploadajte koliko god videa želite</p>
            </div>
          </div>
          <div class="card bg-base-100 shadow-lg border border-base-300">
            <div class="card-body text-center">
              <div class="text-4xl mb-4">⚡</div>
              <h4 class="card-title justify-center">Brza obrada</h4>
              <p class="text-sm text-base-content/70">Prioritetna obrada vaših videa</p>
            </div>
          </div>
          <div class="card bg-base-100 shadow-lg border border-base-300">
            <div class="card-body text-center">
              <div class="text-4xl mb-4">🔒</div>
              <h4 class="card-title justify-center">Ekskluzivni sadržaj</h4>
              <p class="text-sm text-base-content/70">Pristup premium video sadržaju</p>
            </div>
          </div>
        </div>

        <!-- Subscription Plans -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-2xl mx-auto">
          <!-- Monthly Plan -->
          <div class="card bg-base-100 shadow-xl border-2 hover:border-primary transition-all duration-300">
            <div class="card-body text-center">
              <div class="badge badge-primary mb-4">Najpopularnije</div>
              <h3 class="card-title text-2xl justify-center mb-2">Mjesečna pretplata</h3>
              <div class="text-4xl font-bold text-primary mb-2">10 €</div>
              <div class="text-sm text-base-content/70 mb-6">mjesečno</div>
              
              <ul class="text-left space-y-2 mb-6">
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Neograničeni upload videa
                </li>
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Brza obrada videa
                </li>
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Ekskluzivni sadržaj
                </li>
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Otkaži bilo kada
                </li>
              </ul>

              <button 
                @click="upgradeToPremium('price_1RcXTzFw5kYkJsV5DuVSt2IT')" 
                class="btn btn-primary w-full" 
                :disabled="loadingPremium === 'monthly'"
              >
                <span v-if="loadingPremium === 'monthly'" class="loading loading-spinner loading-sm"></span>
                Odaberi mjesečnu
              </button>
            </div>
          </div>

          <!-- Yearly Plan -->
          <div class="card bg-base-100 shadow-xl border-2 hover:border-accent transition-all duration-300">
            <div class="card-body text-center">
              <div class="badge badge-accent mb-4">Ušteda 20%</div>
              <h3 class="card-title text-2xl justify-center mb-2">Godišnja pretplata</h3>
              <div class="text-4xl font-bold text-accent mb-2">99
                 €</div>
              <div class="text-sm text-base-content/70 mb-6">godišnje</div>
              
              <ul class="text-left space-y-2 mb-6">
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Sve iz mjesečne pretplate
                </li>
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Ušteda 20% godišnje
                </li>
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Prioritetna podrška
                </li>
                <li class="flex items-center">
                  <svg class="w-4 h-4 text-success mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  </svg>
                  Otkaži bilo kada
                </li>
              </ul>

              <button 
                @click="upgradeToPremium('price_1RcXUmFw5kYkJsV5sWZDT8cM')" 
                class="btn btn-accent w-full" 
                :disabled="loadingPremium === 'yearly'"
              >
                <span v-if="loadingPremium === 'yearly'" class="loading loading-spinner loading-sm"></span>
                Odaberi godišnju
              </button>
            </div>
          </div>
        </div>

        <div v-if="paymentError" class="alert alert-error mt-6 max-w-2xl mx-auto">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span>{{ paymentError }}</span>
        </div>
      </div>
    </div>

    <!-- Existing Subscription Details -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Current Subscription -->
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body">
          <h3 class="card-title text-xl mb-4">Trenutna pretplata</h3>
          
          <div class="space-y-4">
            <div class="flex justify-between items-center">
              <span class="font-medium">Status:</span>
              <div class="badge" :class="getStatusBadgeClass(subscription.status)">
                {{ getStatusText(subscription.status) }}
              </div>
            </div>

            <div class="flex justify-between items-center">
              <span class="font-medium">Plan:</span>
              <span class="capitalize">{{ subscription.planType }} pretplata</span>
            </div>

            <div class="flex justify-between items-center">
              <span class="font-medium">Cijena:</span>
              <span>{{ formatPrice(subscription.planAmount, subscription.planCurrency) }}</span>
            </div>

            <div class="flex justify-between items-center">
              <span class="font-medium">Trenutni period:</span>
              <span v-if="subscription.currentPeriodStart && subscription.currentPeriodEnd">
                {{ formatDate(subscription.currentPeriodStart) }} - {{ formatDate(subscription.currentPeriodEnd) }}
              </span>
              <span v-else>N/A - N/A</span>
            </div>

            <div v-if="subscription.status === 'canceled' || subscription.status === 'cancel_at_period_end'" class="alert alert-warning">
              <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
              <span v-if="subscription.status === 'canceled'">
                Pretplata je otkazana. Premium status će prestati na kraju trenutnog perioda.
              </span>
              <span v-else>
                Pretplata će biti otkazana na kraju trenutnog perioda.
              </span>
            </div>

            <div v-if="subscription.status === 'canceled' || subscription.status === 'cancel_at_period_end'" class="alert alert-info">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="stroke-current shrink-0 w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
              <span>
                Kliknite "Produži pretplatu" da odaberete novi plan i nastavite s premium pristupom.
              </span>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="card-actions justify-end mt-6 flex flex-row items-center gap-2">
            <button 
              v-if="subscription.status === 'active'" 
              @click="cancelSubscription" 
              class="btn btn-error"
              :disabled="canceling"
            >
              <span v-if="canceling" class="loading loading-spinner loading-sm"></span>
              Otkaži pretplatu
            </button>
            <button 
              v-if="subscription.status === 'canceled'" 
              @click="reactivateSubscription" 
              class="btn btn-primary"
              :disabled="reactivating"
            >
              <span v-if="reactivating" class="loading loading-spinner loading-sm"></span>
              Produži pretplatu
            </button>
            <button 
              v-if="subscription.status === 'cancel_at_period_end'" 
              @click="reactivateSubscription" 
              class="btn btn-primary"
              :disabled="reactivating"
            >
              <span v-if="reactivating" class="loading loading-spinner loading-sm"></span>
              Produži pretplatu
            </button>
            <a
              :href="refundMailtoLink"
              class="btn btn-outline"
              style="min-width: 180px;"
              target="_blank"
              rel="noopener"
            >
              Zatraži povrat novca
            </a>
          </div>
        </div>
      </div>

      <!-- Payment History -->
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body">
          <h3 class="card-title text-xl mb-4">Povijest plaćanja</h3>
          
          <div v-if="paymentHistory.length === 0" class="text-center py-8">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-base-content/30 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <p class="text-base-content/70">Nema povijesti plaćanja</p>
          </div>

          <div v-else class="space-y-3 max-h-96 overflow-y-auto">
            <div 
              v-for="payment in paymentHistory" 
              :key="payment.id" 
              class="flex justify-between items-center p-3 bg-base-200 rounded-lg"
            >
              <div class="flex-1">
                <div class="flex items-center gap-2">
                  <span class="font-medium">{{ formatPrice(payment.amount, payment.currency) }}</span>
                  <div class="badge badge-sm" :class="getPaymentStatusBadgeClass(payment.status)">
                    {{ getPaymentStatusText(payment.status) }}
                  </div>
                </div>
                <div class="text-sm text-base-content/70">
                  {{ formatDate(payment.created) }}
                </div>
                <div v-if="payment.description" class="text-sm text-base-content/60">
                  {{ payment.description }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>

  <UiDialog
    v-model="showCancelSubscriptionModal"
    type="confirm"
    :message="'Jeste li sigurni da želite otkazati pretplatu? Pretplata će biti aktivna do kraja trenutnog perioda.'"
    confirm-text="Otkaži pretplatu"
    cancel-text="Odustani"
    @confirm="cancelSubscriptionConfirmed"
  />
  <UiDialog
    v-model="showInfoModal"
    type="info"
    :message="infoModalMessage"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import UiDialog from '~/components/UiDialog.vue'
import { useRuntimeConfig } from '#app'

const router = useRouter()
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl
const loading = ref(true)
const error = ref('')
const subscription = ref<any>(null)
const paymentHistory = ref<any[]>([])
const canceling = ref(false)
const reactivating = ref(false)
const loadingPremium = ref<string | boolean>(false) // 'monthly', 'yearly', or false
const paymentError = ref('')
const hadSubscription = ref(false)
const userEmail = ref('')
const showCancelSubscriptionModal = ref(false)
const showInfoModal = ref(false)
const infoModalMessage = ref('')

onMounted(async () => {
  // Check if user is authenticated
  const token = localStorage.getItem('authToken')
  const userData = localStorage.getItem('user')
  
  if (!token || !userData) {
    router.push('/login')
    return
  }

  try {
    await Promise.all([
      loadSubscription(),
      loadPaymentHistory()
    ])
  } catch (e) {
    error.value = 'Došlo je do greške pri učitavanju pretplate.'
  } finally {
    loading.value = false
  }

  if (userData) {
    try {
      const userObj = JSON.parse(userData)
      userEmail.value = userObj.email || ''
    } catch {}
  }
})

async function loadSubscription() {
  try {
    const token = localStorage.getItem('authToken')
    const response = await fetch(`${apiBaseUrl}/api/payments/subscription`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })

    if (response.ok) {
      subscription.value = await response.json()
      hadSubscription.value = true
    } else if (response.status === 404) {
      // No subscription found, that's okay
      subscription.value = null
    } else {
      throw new Error('Neuspjelo dohvaćanje pretplate.')
    }
  } catch (e) {
    // Don't throw here, just handle the error silently
  }
}

async function loadPaymentHistory() {
  try {
    const token = localStorage.getItem('authToken')
    const response = await fetch(`${apiBaseUrl}/api/payments/payment-history`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })

    if (response.ok) {
      paymentHistory.value = await response.json()
    } else {
      // Handle error silently
    }
  } catch (e) {
    error.value = 'Došlo je do greške pri učitavanju povijesti plaćanja.'
  } finally {
    loading.value = false
  }
}

async function upgradeToPremium(priceId: string) {
    loadingPremium.value = priceId === 'price_1RcXTzFw5kYkJsV5DuVSt2IT' ? 'monthly' : 'yearly';
    paymentError.value = '';
    try {
        const token = localStorage.getItem('authToken');
        const response = await fetch(`${apiBaseUrl}/api/payments/create-checkout-session`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ priceId: priceId })
        });
        if (response.ok) {
            const data = await response.json();
            window.location.href = data.url;
        } else {
            const data = await response.json();
            throw new Error(data.error || 'Neuspjelo pokretanje plaćanja.');
        }
    } catch (e) {
        paymentError.value = e instanceof Error ? e.message : 'Došlo je do greške.';
    } finally {
        loadingPremium.value = false;
    }
}

async function cancelSubscription() {
  showCancelSubscriptionModal.value = true
}

async function cancelSubscriptionConfirmed() {
  showCancelSubscriptionModal.value = false
  canceling.value = true
  try {
    const token = localStorage.getItem('authToken')
    const response = await fetch(`${apiBaseUrl}/api/payments/cancel-subscription`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })

    if (response.ok) {
      await loadSubscription() // Reload subscription data
      infoModalMessage.value = 'Pretplata će biti otkazana na kraju trenutnog perioda.'
      showInfoModal.value = true
    } else {
      const data = await response.json()
      infoModalMessage.value = data.error || 'Neuspjelo otkazivanje pretplate.'
      showInfoModal.value = true
    }
  } catch (e) {
    infoModalMessage.value = e instanceof Error ? e.message : 'Došlo je do greške.'
    showInfoModal.value = true
  } finally {
    canceling.value = false
  }
}

async function reactivateSubscription() {
  // Instead of redirecting to profile, show the subscription plans
  // by setting subscription to null, which will show the upgrade options
  subscription.value = null
  // Also clear any error messages
  error.value = ''
  paymentError.value = ''
}

function getStatusBadgeClass(status: string) {
  switch (status) {
    case 'active': return 'badge-success'
    case 'canceled': return 'badge-error'
    case 'cancel_at_period_end': return 'badge-warning'
    case 'past_due': return 'badge-warning'
    case 'unpaid': return 'badge-error'
    default: return 'badge-neutral'
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'active': return 'Aktivna'
    case 'canceled': return 'Otkazana'
    case 'cancel_at_period_end': return 'Otkazana (aktivna do kraja perioda)'
    case 'past_due': return 'Prekoračen rok'
    case 'unpaid': return 'Neplaćena'
    default: return status
  }
}

function getPaymentStatusBadgeClass(status: string) {
  switch (status) {
    case 'succeeded': return 'badge-success'
    case 'processing': return 'badge-warning'
    case 'requires_payment_method': return 'badge-error'
    default: return 'badge-neutral'
  }
}

function getPaymentStatusText(status: string) {
  switch (status) {
    case 'succeeded': return 'Uspješno'
    case 'processing': return 'Obrađuje se'
    case 'requires_payment_method': return 'Zahtijeva plaćanje'
    default: return status
  }
}

function formatPrice(amount: number, currency: string) {
  if (!amount) return 'N/A'
  return new Intl.NumberFormat('hr-HR', {
    style: 'currency',
    currency: currency.toUpperCase()
  }).format(amount / 100) // Stripe amounts are in cents
}

function formatDate(timestamp: number | string) {
  if (!timestamp) return 'N/A'
  let date: Date
  if (typeof timestamp === 'number') {
    date = new Date(timestamp * 1000)
  } else {
    date = new Date(timestamp)
  }
  if (isNaN(date.getTime())) return 'N/A'
  return date.toLocaleDateString('hr-HR', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function showSubscriptionDetails() {
  // Reload subscription data to show the details again
  loadSubscription()
}

const refundMailtoLink = computed(() => {
  const email = 'GymTubeAdmin@gmail.com';
  const subject = encodeURIComponent('Zahtjev za povrat novca - GymTube pretplata');
  const body = encodeURIComponent(
    `Poštovani GymTube tim,

Želim zatražiti povrat novca za svoju pretplatu.

Razlog povrata:
[ovdje napišite razlog]

Molimo unesite ime i email koji se nalaze na vašem GymTube računu.
Email adresa s koje šaljete zahtjev mora odgovarati vašoj GymTube email adresi.

Moje ime: [vaše ime]
Moj email: [vaš email]

Hvala!`
  );
  return `mailto:${email}?subject=${subject}&body=${body}`;
});
</script> 