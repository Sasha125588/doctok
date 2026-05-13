export const contentLanguages = [
  {
    code: 'en',
    label: 'English',
    nativeName: 'English',
    source: 'official',
    shortLabel: 'EN',
  },
  {
    code: 'es',
    label: 'Spanish',
    nativeName: 'Español',
    source: 'official',
    shortLabel: 'ES',
  },
  {
    code: 'fr',
    label: 'French',
    nativeName: 'Français',
    source: 'official',
    shortLabel: 'FR',
  },
  {
    code: 'ja',
    label: 'Japanese',
    nativeName: '日本語',
    source: 'official',
    shortLabel: 'JA',
  },
  {
    code: 'ko',
    label: 'Korean',
    nativeName: '한국어',
    source: 'official',
    shortLabel: 'KO',
  },
  {
    code: 'pt-BR',
    label: 'Brazilian Portuguese',
    nativeName: 'Português do Brasil',
    source: 'official',
    shortLabel: 'PT',
  },
  {
    code: 'ru',
    label: 'Russian',
    nativeName: 'Русский',
    source: 'official',
    shortLabel: 'RU',
  },
  {
    code: 'zh-CN',
    label: 'Simplified Chinese',
    nativeName: '简体中文',
    source: 'official',
    shortLabel: 'ZH',
  },
  {
    code: 'zh-TW',
    label: 'Traditional Chinese',
    nativeName: '繁體中文',
    source: 'official',
    shortLabel: 'ZH',
  },
  {
    code: 'de',
    label: 'German',
    nativeName: 'Deutsch',
    source: 'aiGenerated',
    shortLabel: 'DE',
  },
  {
    code: 'it',
    label: 'Italian',
    nativeName: 'Italiano',
    source: 'aiGenerated',
    shortLabel: 'IT',
  },
  {
    code: 'uk',
    label: 'Ukrainian',
    nativeName: 'Українська',
    source: 'aiGenerated',
    shortLabel: 'UK',
  },
  {
    code: 'pl',
    label: 'Polish',
    nativeName: 'Polski',
    source: 'aiGenerated',
    shortLabel: 'PL',
  },
  {
    code: 'tr',
    label: 'Turkish',
    nativeName: 'Türkçe',
    source: 'aiGenerated',
    shortLabel: 'TR',
  },
  {
    code: 'el',
    label: 'Greek',
    nativeName: 'Ελληνικά',
    source: 'aiGenerated',
    shortLabel: 'EL',
  },
] as const

export const uiLanguages = [
  {
    code: 'en',
    label: 'English',
    nativeName: 'English',
    source: 'official',
    shortLabel: 'EN',
  },
  {
    code: 'uk',
    label: 'Ukrainian',
    nativeName: 'Українська',
    source: 'community',
    shortLabel: 'UK',
  },
  {
    code: 'ru',
    label: 'Russian',
    nativeName: 'Русский',
    source: 'community',
    shortLabel: 'RU',
  },
] as const

export type PostsContentLang = (typeof contentLanguages)[number]['code']
export type UiLang = (typeof uiLanguages)[number]['code']
export type ContentLanguageSource = (typeof contentLanguages)[number]['source']
export type UiLanguageSource = (typeof uiLanguages)[number]['source']

export const contentLanguageSourceLabels: Record<ContentLanguageSource, string> = {
  official: 'Official MDN',
  aiGenerated: 'AI generated',
}

export const uiLanguageSourceLabels: Record<UiLanguageSource, string> = {
  official: 'Official',
  community: 'Community',
}

const getPostsContentLanguage = (code: PostsContentLang) =>
  contentLanguages.find((language) => language.code === code) ?? contentLanguages[0]

const getUiLanguage = (code: UiLang) =>
  uiLanguages.find((language) => language.code === code) ?? uiLanguages[0]

export const useLang = () => {
  const postsContentLang = useCookie<PostsContentLang>('doctok_content_lang', {
    default: () => 'en',
  })
  const uiLang = useCookie<UiLang>('doctok_ui_lang', { default: () => 'en' })

  const currentPostsContentLanguage = computed(() =>
    getPostsContentLanguage(postsContentLang.value)
  )
  const currentUiLanguage = computed(() => getUiLanguage(uiLang.value))

  const setPostsContentLang = (value: PostsContentLang) => {
    if (postsContentLang.value === value) return

    postsContentLang.value = value
  }

  const setUiLang = (value: UiLang) => {
    if (uiLang.value === value) return

    uiLang.value = value
  }

  return {
    postsContentLang,
    uiLang,
    contentLanguages,
    uiLanguages,
    currentPostsContentLanguage,
    currentUiLanguage,
    setPostsContentLang,
    setUiLang,
  }
}
