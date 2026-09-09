# Command Catalog

Status: live catalog in progress; supported families below are backed by real executors and verification.

## Registry conventions
Every command entry must define:
- intent_id
- plugin_id
- action
- required_context
- slots/entities
- aliases/templates
- preconditions/postconditions
- risk_level
- undo_support
- latency_budget
- executor
- tests

## Initial command families
- app.launch / app.focus / app.close / app.switch
- window.minimize / maximize / restore / move / close
- system.audio.get / set / adjust / mute
- system.time.get / system.date.get
- file.open / copy / move / rename / delete-safe
- clipboard.read / write
- browser.tab.open / close / switch / back / forward / reload / scroll / find
- media.youtube.open / search / play_result / pause / resume / seek / volume / fullscreen
- obs.record.start / stop / pause / resume
- obs.stream.start / stop
- obs.scene.get / set
- obs.input.mute / unmute
- obs.source.visibility.show / hide
- lightroom.navigation.next / previous
- lightroom.rating.set / flag / reject
- lightroom.settings.copy / paste / undo / redo
- lightroom.export
- lightroom.adjust.* only after installed-version capability validation
- davinci.project.* / timeline.* / media.* / playback.* / render.* only after installed-edition API audit


## Verified live OBS support
OBS Studio 32.2.2 / obs-websocket 5.7.4: status, record/stream status, scene get/list/set, record start/stop/pause/resume, stream start/stop, input list/mute/unmute and scene-source visibility are implemented. Destructive/live-output commands are not fired automatically in smoke tests; source/input state tests are reversible and restore their initial state.

## Corpus requirement
The final command corpus must contain at least 10,000 verified Polish utterances distributed across these intent families, including colloquial variants, ASR-confusion variants, numbers, percentages, time expressions and context-elided commands.

## Rule
This catalog records supported behavior, not desired behavior. A command is marked SUPPORTED only when its executor and end-to-end tests pass against the relevant application/version.
