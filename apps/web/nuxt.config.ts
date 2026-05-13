import tailwindcss from '@tailwindcss/vite'
import path from 'node:path'

import type { NuxtPage } from 'nuxt/schema'

const apiBaseUrl = process.env.NUXT_API_BASE_URL ?? 'http://localhost:5005'
const publicApiBaseUrl = process.env.NUXT_PUBLIC_API_BASE_URL

const resolve = (filePath: string) => path.resolve(__dirname, filePath)

const removePagesMatching = (pattern: RegExp, pages: NuxtPage[] = []) => {
  const pagesToRemove: NuxtPage[] = []

  for (const page of pages) {
    if (page.file && pattern.test(page.file)) {
      pagesToRemove.push(page)
      continue
    }

    removePagesMatching(pattern, page.children)
  }

  for (const page of pagesToRemove) {
    pages.splice(pages.indexOf(page), 1)
  }
}

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  alias: {
    '#api': resolve('./generated/api'),
  },

  vite: {
    optimizeDeps: {
      include: [
        '@tanstack/vue-query',
        'valibot',
        'lucide-vue-next',
        'clsx',
        'tailwind-merge',
        'date-fns',
        '@vue/devtools-core',
        '@vue/devtools-kit',
        'reka-ui',
      ],
    },
    plugins: [tailwindcss()],
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
    'shadcn-nuxt',
    '@nuxt/icon',
    'nuxt-svgo',
    '@peterbud/nuxt-query',
    'motion-v/nuxt',
    'vue-sonner/nuxt',
  ],

  experimental: {
    typedPages: true,
  },

  nitro: {
    preset: process.env.VERCEL ? 'vercel' : 'bun',
  },

  runtimeConfig: {
    apiBaseUrl,
    public: {
      apiBaseUrl: publicApiBaseUrl,
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

  css: ['./app/assets/css/tailwind.css'],

  vueSonner: {
    css: true, // true by default to include css file
  },

  hooks: {
    'pages:extend'(pages) {
      removePagesMatching(/\/-components\//, pages)
      removePagesMatching(/\/-composables\//, pages)
      removePagesMatching(/\/-types\//, pages)
      removePagesMatching(/\/-constants\//, pages)
    },
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
