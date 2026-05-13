<script setup lang="ts">
import SearchCatalog from './SearchCatalog.vue'
import SearchInput from './SearchInput.vue'
import SearchResults from './SearchResults.vue'
import { filterMockSearchHits } from '~/lib/searchMockData'

const route = useRoute()
const router = useRouter()

const query = ref(typeof route.query.q === 'string' ? route.query.q : '')

const searchQuery = computed(() => query.value.trim())
const isSearching = computed(() => searchQuery.value.length > 0)
const results = computed(() => filterMockSearchHits(searchQuery.value))

watch(
  () => route.query.q,
  (routeQuery) => {
    const nextQuery = typeof routeQuery === 'string' ? routeQuery : ''
    if (nextQuery === query.value) return

    query.value = nextQuery
  }
)

watch(query, (nextQuery) => {
  const normalizedQuery = nextQuery.trim()
  const currentQuery = typeof route.query.q === 'string' ? route.query.q : ''

  if (normalizedQuery === currentQuery) return

  router.replace({
    query: {
      ...route.query,
      q: normalizedQuery || undefined,
    },
  })
})
</script>

<template>
  <section class="search-page">
    <SearchInput v-model="query" />
    <SearchCatalog v-if="!isSearching" />
    <SearchResults
      v-else
      :query="searchQuery"
      :results="results"
    />
  </section>
</template>

<style scoped>
.search-page {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 20px 26px;
  gap: 16px;
  overflow: hidden;
  min-width: 0;
}
</style>
