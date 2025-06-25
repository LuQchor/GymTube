import tailwindcss from "@tailwindcss/vite";
export default defineNuxtConfig({
  compatibilityDate: '2025-06-21',
  vite: {
    plugins: [tailwindcss()],
    optimizeDeps: {
      exclude: ['@nuxt/kit']
    }
  },
  css: ["~/assets/app.css"],
  nitro: {
    experimental: {
      wasm: true
    }
  },
  experimental: {
    payloadExtraction: false
  },
  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.API_BASE_URL || 'http://localhost:5011',
      googleClientId: process.env.GOOGLE_CLIENT_ID || '',
      googleRedirectUri: process.env.GOOGLE_REDIRECT_URI || '',
      stripePublicKey: process.env.STRIPE_PUBLIC_KEY || ''
    }
  }
});