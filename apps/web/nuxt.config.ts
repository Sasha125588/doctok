// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: [
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@nuxtjs/tailwindcss',
    'shadcn-nuxt',
    '@nuxt/icon',
    'nuxt-svgo',
    '@peterbud/nuxt-query',
  ],

  experimental: {
    typedPages: true,
  },

  nitro: {
    preset: 'bun',
  },

  nuxtQuery: {
    devtools: true,
    queryClientOptions: {
      defaultOptions: {
        queries: {
          refetchOnWindowFocus: false,
          staleTime: 5000,
        },
      },
    },
  },

  shadcn: {
    // Prefix for all the imported component.
    prefix: '',

    // @link https://nuxt.com/docs/api/nuxt-config#alias
    componentDir: '@/components/ui',
  },

  icon: {
    mode: 'svg',
    localApiEndpoint: '/_nuxt_icon',
    clientBundle: {
      includeCustomCollections: true,
    },
  },
})
