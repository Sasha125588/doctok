import path from 'node:path'

function resolve(filePath: string) {
  return path.resolve(__dirname, filePath)
}

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  alias: {
    '#api': resolve('./generated/api'),
  },

  vite: {
    optimizeDeps: {
      include: ['swiper/modules', 'swiper/vue', '@tanstack/vue-query', 'valibot'],
    },
  },

  compatibilityDate: '2025-07-15',
  devtools: {
    enabled: true,

    timeline: {
      enabled: true,
    },
  },
  modules: [
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@nuxtjs/tailwindcss',
    'shadcn-nuxt',
    '@nuxt/icon',
    'nuxt-svgo',
    '@peterbud/nuxt-query',
    '@nuxtjs/mdc',
  ],

  experimental: {
    typedPages: true,
  },

  nitro: {
    preset: 'bun',
  },

  routeRules: {
    '/api/**': {
      proxy: 'http://localhost:5005/api/**',
    },
  },

  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.NUXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5005',
    },
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

  app: {
    head: {
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        {
          rel: 'preconnect',
          href: 'https://fonts.gstatic.com',
          crossorigin: '',
        },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Syne:wght@400;700;800&family=DM+Mono:ital,wght@0,400;0,500;1,400&display=swap',
        },
      ],
    },
  },
})
