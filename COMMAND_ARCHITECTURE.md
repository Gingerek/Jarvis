# Command Architecture

## Principle
Jarvis supports thousands of natural Polish utterances without implementing thousands of functions. Utterances map to structured intents and slots; intents map to reusable executors.

## Command definition
Each registered command must expose at least:
- intent_id
- plugin_id
- action
- description
- utterances / templates / aliases
- required_context
- slots and entities
- preconditions
- postconditions
- risk_level
- executor
- undo_support
- latency_budget
- tests

## Router layers
1. Normalizer
   - Polish diacritics and missing diacritics
   - common ASR substitutions
   - spoken numbers, percentages and times
   - aliases for English application names pronounced in Polish
   - whitespace/punctuation normalization
2. Deterministic matcher
   - exact aliases
   - token/pattern templates
   - high-frequency commands
3. Semantic matcher
   - precomputed canonical representations held in memory
   - provider is replaceable; exact embedding model is selected by benchmark, not assumption
4. Context Resolver
   - current and previous application/window/document/browser/media context
5. Local planner
   - only for multi-action work that cannot be expressed as a deterministic command/workflow
6. Optional cloud reasoning
   - last reasoning tier; never required for ordinary PC control

## Intent examples
`app.launch(app)` handles: "włącz", "otwórz", "uruchom", "odpal" + application alias.

`system.audio.set_volume(percent)` handles explicit values.
`system.audio.adjust_volume(delta)` handles relative requests such as "głośniej" or "trochę ciszej".

`media.youtube.search(query)` may omit YouTube when Browser.YouTube is current context.
`media.youtube.play_result(index)` resolves ordinal expressions such as "pierwszy", "drugi", "ten trzeci".

`creative.lightroom.adjust(parameter, delta_or_value)` resolves commands such as exposure, shadows or temperature only when the plugin capability is verified for the installed Lightroom version.

## Context resolution rules
- Explicit app name wins over inherited context.
- If an intent has required_context and exactly one compatible active/recent context exists, use it without asking.
- "wróć do X" activates the most recent matching context state.
- "zamknij to", "cofnij", "pauza", "wznów" are context-sensitive commands.
- Ambiguous destructive commands never guess a target.

## Corpus design
Target: at least 10,000 verified Polish utterances.

Corpus is generated from intent templates, morphology/synonym sets, application aliases, numeric variants, colloquial forms and context-elided variants. It is then validated, deduplicated and split so that near-identical template families do not leak across train/validation/test partitions.

Required partitions:
- development/train corpus,
- held-out text validation corpus,
- held-out text test corpus,
- real microphone recording test set from the actual user and environment.

The generated corpus is not accepted merely because a generator produced 10,000 rows. Coverage and confusion matrices are required.

## Learning
Synonym learning stores user aliases as data, not code. Workflow learning stores a typed sequence of existing safe actions. Learned data survives updates and is never allowed to inject arbitrary executable code.

## Risk policy
LOW: launch/focus/read/volume/navigation.
MEDIUM: change project/document state, overwrite within reversible workflow.
HIGH: send messages, purchase, destructive file deletion, security configuration, formatting/shutdown policies as defined by Security.

Risk level is part of the command contract, not inferred ad hoc by individual plugins.
