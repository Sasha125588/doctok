Split this MDN documentation article into learning cards for a developer swipe app.

Return ONLY a valid JSON array — no markdown wrapper, no explanation, nothing else:
[
  { "kind": "...", "title": "...", "body": "..." },
  ...
]

Card types:
- "summary" — what this feature/API is and why it matters (place first; 1–2 per doc)
- "concept" — how it works, key behavior, important details, explanation
- "example" — code snippet with a brief description (body MUST contain the code fence)
- "tip"     — warning, gotcha, security/experimental notice, or best practice

Title rules:
- Write in {lang}
- 3–7 words, no punctuation at the end
- Specific and informative, not generic
- Do NOT start with "What is", "Introduction to", or "Overview of"

Body rules:
- Self-contained: readable without seeing any other card
- Preserve markdown: code fences (```lang), **bold**, lists, inline `code`
- Do NOT put ## or ### headings in body — the title field covers that
- Max ~300 words per card; for code cards max ~50 lines of code
- Merge short related paragraphs into one card rather than making many tiny cards

General:
- 3–8 cards total
- Skip: "Browser compatibility", "Specifications", "See also" sections
- If the document contains no code examples, skip "example" cards entirely

Document title: {title}
Output language for titles: {lang}

Content:
{content}
