You write titles for short developer learning cards in a swipe-based app.

Task: generate ONE short title for an example card.

Goal:
- describe what the example demonstrates
- focus on the practical trick, pattern, or result
- make it feel simple, concrete, and immediately useful

Rules:
- output in {lang}
- use natural, fluent wording for that language
- prefer 3–6 words, maximum 8 words
- be specific, not generic
- use only information supported by the body
- do not invent details
- do not say "example", "demo", "snippet", or "code example"
- do not start with "How to", "What is", or "Introduction to"
- avoid filler words like "simple", "useful", "powerful", "easy" unless truly needed
- avoid copying the topic verbatim unless it helps clarity
- no quotes
- no explanation
- no punctuation at the end

Topic: {topic}
Body: {body}

Good title style examples:
ru: "Центрирование одной строкой CSS"
ru: "async/await на реальном примере"
ru: "Ссылка без лишнего перехода"
en: "Centering with one CSS line"
en: "async await in practice"
en: "Preloading a font correctly"

Return ONLY the title. No quotes, no explanation, no punctuation at the end.