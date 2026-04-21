<script setup lang="ts">
import { Button } from '~/components/ui/button'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '~/components/ui/sheet'
import { Skeleton } from '~/components/ui/skeleton'
import { useLang } from '~/composables/useLang'
import { useTopicLinks } from '~/composables/useTopicLinks'

const props = defineProps<{ slug: string }>()

const open = ref(false)
const { lang } = useLang()

const queryOptions = computed(() => ({
  query: {
    slug: props.slug ?? '',
    lang: lang.value,
  },
}))

const { state } = useTopicLinks(queryOptions, open)
</script>

<template>
  <Sheet v-model:open="open">
    <SheetTrigger as-child>
      <Button
        variant="ghost"
        size="sm"
        class="text-muted-foreground gap-1.5"
      >
        <Icon
          name="lucide:link"
          class="size-4"
        />
        Links
      </Button>
    </SheetTrigger>
    <SheetContent
      side="bottom"
      class="max-h-[60dvh]"
    >
      <SheetHeader>
        <SheetTitle>Related Topics</SheetTitle>
      </SheetHeader>
      <div class="mt-4 space-y-2 overflow-y-auto">
        <template v-if="state.isLoading">
          <Skeleton
            v-for="i in 3"
            :key="i"
            class="h-10 w-full"
          />
        </template>
        <template v-else-if="state.links.value.length">
          <button
            v-for="link in state.links.value"
            :key="link.slug"
            class="hover:bg-accent block w-full rounded-lg p-3 text-left text-sm transition-colors"
            @click="
              open = false
              navigateTo(`/topic/${link.slug}`)
            "
          >
            {{ link.title }}
          </button>
        </template>
        <div
          v-else
          class="text-muted-foreground py-4 text-center text-sm"
        >
          No linked topics
        </div>
      </div>
    </SheetContent>
  </Sheet>
</template>
