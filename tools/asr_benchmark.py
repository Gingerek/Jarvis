import json, sys, time, types, wave
from pathlib import Path
import numpy as np

# PyAV is blocked by Windows Application Control on this machine.
# Benchmark uses already-normalized PCM WAV, so decoding is unnecessary.
sys.modules.setdefault("av", types.ModuleType("av"))
from faster_whisper import WhisperModel

root = Path(__file__).resolve().parents[1]
audio_path = root / "benchmarks" / "asr" / (sys.argv[1] if len(sys.argv) > 1 else "polish-command-sample.wav")
models_dir = root / ".models-asr"
out_path = root / "benchmarks" / "asr" / "results.json"

with wave.open(str(audio_path), "rb") as w:
    assert w.getframerate() == 16000 and w.getnchannels() == 1 and w.getsampwidth() == 2
    raw = w.readframes(w.getnframes())
    duration = w.getnframes() / w.getframerate()
audio = np.frombuffer(raw, dtype=np.int16).astype(np.float32) / 32768.0

results = []
for name in ["base", "small"]:
    t0 = time.perf_counter()
    model = WhisperModel(
        name,
        device="cpu",
        compute_type="int8",
        cpu_threads=8,
        download_root=str(models_dir),
    )
    load_s = time.perf_counter() - t0
    runs = []

    for i in range(3):
        t1 = time.perf_counter()
        segments, info = model.transcribe(
            audio,
            language="pl",
            beam_size=1,
            vad_filter=False,
            condition_on_previous_text=False,
            initial_prompt="Jarvis. Lightroom. YouTube. DaVinci Resolve. Photoshop. OBS Studio. Marktplaats.",
        )
        text = " ".join(s.text.strip() for s in list(segments)).strip()
        elapsed = time.perf_counter() - t1
        run = {
            "seconds": elapsed,
            "rtf": elapsed / duration,
            "text": text,
            "language": info.language,
            "language_probability": info.language_probability,
        }
        runs.append(run)
        print(f"{name} run{i+1}: {elapsed:.3f}s RTF={elapsed/duration:.3f} | {text}")

    results.append({
        "model": name,
        "load_seconds": load_s,
        "audio_seconds": duration,
        "runs": runs,
    })

out_path.write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8")
print(f"Saved {out_path}")


