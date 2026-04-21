export function useLang() {
  const lang = useCookie('doctoc_content_lang', { default: () => 'en' })

  const setLang = (value: string) => {
    lang.value = value
    reloadNuxtApp()
  }

  return { lang, setLang }
}
