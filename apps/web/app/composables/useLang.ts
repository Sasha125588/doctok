export function useLang() {
  const lang = useCookie('content_lang', { default: () => 'ru' })

  function setLang(value: string) {
    lang.value = value
  }

  return { lang: lang as Ref<string>, setLang }
}
