Rewrite this existing developer learning card in {lang}.

{style}

Rules:
- Keep the same meaning. Do not add facts that are not supported by the original.
- Preserve markdown: code fences (```lang), **bold**, lists, inline `code`.
- Do not add ## or ### headings to the body — the title field covers that.
- Keep the post self-contained: readable without seeing other cards.

Return ONLY JSON, no markdown wrapper, no explanation:
{ "title": "...", "body": "..." }

Original title: {title}
Original kind: {kind}
Original body:
{body}
