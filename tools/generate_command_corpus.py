import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "data" / "commands" / "pl-PL.generated.jsonl"
OUT.parent.mkdir(parents=True, exist_ok=True)

apps = {
    "lightroom": ["Lightroom", "Lightroom Classic", "Adobe Lightroom"],
    "resolve": ["DaVinci", "DaVinci Resolve", "Resolve"],
    "obs": ["OBS", "OBS Studio"],
    "chrome": ["Chrome", "Google Chrome"],
    "edge": ["Edge", "Microsoft Edge"],
    "notepad": ["Notatnik", "Notepad"],
    "taskmanager": ["Menedżer zadań", "Task Manager"],
}
websites = {
    "youtube": ["YouTube"], "google": ["Google"], "marktplaats": ["Marktplaats"]
}
open_templates = [
    "{verb} {target}", "Jarvis {verb} {target}", "proszę {verb} {target}",
    "Jarvis proszę {verb} {target}", "możesz {verb} {target}",
    "czy możesz {verb} {target}", "proszę cię {verb} {target}",
    "chcę żebyś {verb} {target}", "spróbuj {verb} {target}",
    "teraz {verb} {target}", "{verb} mi {target}",
    "{verb} dla mnie {target}", "Jarvis {verb} mi {target}",
    "Jarvis {verb} dla mnie {target}", "dobrze {verb} {target}",
    "okej {verb} {target}", "hej Jarvis {verb} {target}",
    "Jarvis możesz {verb} {target}", "Jarvis czy możesz {verb} {target}",
    "Jarvis chcę żebyś {verb} {target}",
]
close_templates = [
    "{verb} {target}", "Jarvis {verb} {target}", "proszę {verb} {target}",
    "Jarvis proszę {verb} {target}", "możesz {verb} {target}",
    "czy możesz {verb} {target}", "proszę cię {verb} {target}",
    "chcę żebyś {verb} {target}", "teraz {verb} {target}",
    "{verb} mi {target}", "Jarvis {verb} mi {target}",
    "hej Jarvis {verb} {target}", "okej {verb} {target}",
]
open_verbs = ["otwórz", "włącz", "uruchom", "odpal", "startuj", "open"]
close_verbs = ["zamknij", "wyłącz", "zakończ", "close"]
suffixes = ["", " proszę", " teraz", " od razu", " jeśli możesz"]
rows = {}

def add(intent, argument, utterance):
    utterance = " ".join(utterance.split()).strip()
    key = (intent, argument or "", utterance.casefold())
    rows[key] = {"intent": intent, "argument": argument, "utterance": utterance}

for canonical, aliases in apps.items():
    for alias in aliases:
        for template in open_templates:
            for verb in open_verbs:
                for suffix in suffixes:
                    add("OpenApplication", canonical, template.format(verb=verb, target=alias) + suffix)
        for template in close_templates:
            for verb in close_verbs:
                for suffix in suffixes:
                    add("CloseApplication", canonical, template.format(verb=verb, target=alias) + suffix)

for canonical, aliases in websites.items():
    for alias in aliases:
        for template in open_templates:
            for verb in open_verbs:
                for suffix in suffixes:
                    add("OpenWebsite", canonical, template.format(verb=verb, target=alias) + suffix)
queries = [
    "Warszawa", "pogoda Helmond", "Sony A7 IV", "DaVinci Resolve tutorial",
    "muzyka włoska", "RDS radio", "Lightroom maski", "trasa do pracy",
    "wiadomości technologiczne", "Canon Selphy CP1500"
]
search_templates = [
    "wyszukaj w Google {query}", "znajdź w Google {query}",
    "Google wyszukaj {query}", "wyszukaj w internecie {query}",
    "Jarvis wyszukaj w Google {query}", "proszę wyszukaj w Google {query}",
    "czy możesz wyszukać w Google {query}", "znajdź mi w Google {query}"
]
youtube_templates = [
    "wyszukaj na YouTube {query}", "znajdź na YouTube {query}",
    "YouTube wyszukaj {query}", "YouTube szukaj {query}",
    "Jarvis wyszukaj na YouTube {query}", "proszę znajdź na YouTube {query}",
    "znajdź mi na YouTube {query}", "czy możesz wyszukać na YouTube {query}"
]
for query in queries:
    for template in search_templates:
        add("SearchWeb", query, template.format(query=query))
    for template in youtube_templates:
        add("SearchYouTube", query, template.format(query=query))
time_phrases = [
    "która godzina", "jaka jest godzina", "powiedz która godzina", "podaj godzinę",
    "ile jest godzina", "jaki mamy czas", "Jarvis która godzina", "Jarvis podaj godzinę"
]
date_phrases = [
    "jaka jest data", "jaki dzisiaj dzień", "jaki mamy dziś dzień", "podaj datę",
    "powiedz jaka jest data", "co dzisiaj za dzień", "Jarvis jaka jest data", "Jarvis podaj datę"
]
day_phrases = [
    "jaki dziś dzień tygodnia", "jaki mamy dzień tygodnia", "powiedz jaki dziś dzień tygodnia",
    "co dzisiaj za dzień tygodnia", "Jarvis jaki dziś dzień tygodnia"
]
for phrase in time_phrases: add("GetTime", None, phrase)
for phrase in date_phrases: add("GetDate", None, phrase)
for phrase in day_phrases: add("GetDayOfWeek", None, phrase)

ordered = sorted(rows.values(), key=lambda x: (x["intent"], x["argument"] or "", x["utterance"].casefold()))
with OUT.open("w", encoding="utf-8", newline="\n") as f:
    for row in ordered:
        f.write(json.dumps(row, ensure_ascii=False) + "\n")
print(f"generated={len(ordered)} path={OUT}")
