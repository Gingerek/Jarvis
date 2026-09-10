import base64, json, sys, time, types
from pathlib import Path
import numpy as np

# PyAV binary is blocked by Windows Application Control on this machine.
# Jarvis sends normalized PCM directly, so decoding is not required.
sys.modules.setdefault("av", types.ModuleType("av"))
from faster_whisper import WhisperModel

model_name = sys.argv[1] if len(sys.argv) > 1 else "base"
models_dir = Path(sys.argv[2]) if len(sys.argv) > 2 else None

t0 = time.perf_counter()
model = WhisperModel(
    model_name,
    device="cpu",
    compute_type="int8",
    cpu_threads=8,
    download_root=str(models_dir) if models_dir else None,
)
print(json.dumps({"type": "ready", "model": model_name,
                  "load_ms": (time.perf_counter() - t0) * 1000}), flush=True)
for line in sys.stdin:
    try:
        req = json.loads(line)
        pcm = np.frombuffer(base64.b64decode(req["pcm16_b64"]), dtype=np.int16)
        audio = pcm.astype(np.float32) / 32768.0
        started = time.perf_counter()
        segments, info = model.transcribe(
            audio,
            language=req.get("language", "pl"),
            beam_size=int(req.get("beam_size", 3)),
            vad_filter=False,
            condition_on_previous_text=False,
            no_speech_threshold=None,
            initial_prompt=req.get("initial_prompt"),
            hotwords=req.get("hotwords"),
        )
        text = " ".join(s.text.strip() for s in list(segments)).strip()
        print(json.dumps({
            "id": req.get("id"), "text": text, "language": info.language,
            "language_probability": info.language_probability,
            "inference_ms": (time.perf_counter() - started) * 1000,
        }, ensure_ascii=False), flush=True)
    except Exception as exc:
        print(json.dumps({
            "id": req.get("id") if 'req' in locals() else None,
            "error": str(exc)
        }, ensure_ascii=False), flush=True)
