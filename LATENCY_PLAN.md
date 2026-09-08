# Latency Plan

## Measurement principle
Measure Jarvis decision latency separately from application completion latency. Never report an application's cold start as if it were Jarvis routing time.

## Timestamp boundaries
T0 = last speech frame accepted by endpointing/VAD.
T1 = final transcript available.
T2 = normalized transcript and intent decision available.
T3 = executor dispatch issued.
T4 = target application acknowledges/observably begins action.
T5 = requested visible/semantic result confirmed.
T6 = first TTS audio frame ready for playback when a spoken response is required.

## Metrics
- ASR finalization = T1-T0
- routing = T2-T1
- dispatch = T3-T2
- executor acknowledgement = T4-T3
- application completion = T5-T4
- TTS first-audio = T6-response_text_ready
- end-to-end action = T5-T0

Collect p50, p90, p95, p99, max, error rate and sample count.

## Initial budgets
| Segment | Target |
|---|---|
| intent routing for known command | p50 <= 50 ms; p95 <= 150 ms |
| known-command dispatch after transcript | p95 <= 250 ms |
| focus already-running app | target p95 <= 300 ms when Windows allows it |
| local command path | no network dependency |
| TTS | benchmark time-to-first-audio separately; never include vendor model-only latency as measured end-to-end latency |

## ASR benchmark
Minimum candidates:
1. NVIDIA Parakeet TDT 0.6B v3 when the hardware audit proves a compatible NVIDIA path.
2. faster-whisper with a model/compute type chosen from measured hardware capability.
3. one additional current local candidate only if it has a credible reason to outperform the first two.

Test sets:
- 100+ short Polish commands,
- 50+ longer commands,
- application names and English product names pronounced naturally by the user,
- numbers/percent/time values,
- room-noise conditions,
- repeated real microphone samples.

Report WER/CER plus command-intent accuracy. Command-intent accuracy is the primary product metric; a transcript can be imperfect yet still route correctly.

## VAD / endpoint benchmark
Compare endpoint delay, false cuts, missed speech starts and background false activations. Silero VAD is the initial benchmark candidate. Tune endpointing for short commands separately from dictation.

## Wake-word benchmark
For each candidate measure:
- false rejects per 100 true wake attempts,
- false accepts per hour of realistic background audio,
- CPU/RAM utilization,
- time wake event -> ASR session ready,
- pronunciation robustness for the actual user's "Jarvis".

A wake engine cannot win if licensing is incompatible, regardless of accuracy.

## Browser/OBS/application benchmarks
Use warm persistent connections. Measure command receipt to semantic acknowledgement. Do not reconnect Native Messaging or obs-websocket for every command.

## TTS benchmark
Initial candidate: ElevenLabs Flash v2.5 streaming/WebSocket. Measure from finalized response text to first playable PCM/audio frame at the machine. Compare at least the final selected voice and one default/synthetic control voice because vendor documentation notes voice type can affect latency.

## Benchmark storage
Diagnostics must emit structured records containing timestamp, command id, route tier, context, plugin, executor, durations, success/failure and machine profile hash. LATENCY_REPORT.md is generated from these records; raw secrets/transcripts marked private are excluded from Git.

## Acceptance
No latency claim is accepted from a single sample. Each hot-path scenario requires enough repetitions for meaningful percentiles and must be rerun after material changes to ASR, VAD, IPC, routing or an executor.
