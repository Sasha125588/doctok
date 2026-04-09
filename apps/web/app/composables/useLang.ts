export function useLang() {
  const lang = useCookie('content_lang', { default: () => 'en' })

  function setLang(value: string) {
    lang.value = value
  }

  return { lang, setLang }
}
