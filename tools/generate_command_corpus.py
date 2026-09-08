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

system_bases = {
    "VolumeUp": ["głośniej", "podgłoś", "zwiększ głośność", "podnieś głośność"],
    "VolumeDown": ["ciszej", "ścisz", "zmniejsz głośność", "obniż głośność"],
    "Mute": ["wycisz", "wycisz dźwięk", "wyłącz dźwięk"],
    "Unmute": ["odcisz", "włącz dźwięk", "przywróć dźwięk"],
    "MinimizeWindow": ["minimalizuj okno", "zminimalizuj okno", "schowaj okno"],
    "MaximizeWindow": ["maksymalizuj okno", "zmaksymalizuj okno", "powiększ okno"],
    "RestoreWindow": ["przywróć okno", "normalne okno"],
    "CloseWindow": ["zamknij okno", "zamknij aktywne okno"],
    "ShowDesktop": ["pokaż pulpit", "przejdź na pulpit"],
    "LockComputer": ["zablokuj komputer", "zablokuj ekran"]
}
polite_prefixes = ["", "Jarvis ", "proszę ", "Jarvis proszę ", "czy możesz ", "możesz ", "hej Jarvis "]
for intent, phrases in system_bases.items():
    for phrase in phrases:
        for prefix in polite_prefixes:
            for suffix in suffixes:
                add(intent, None, prefix + phrase + suffix)
for value in range(0, 101, 10):
    for phrase in [f"ustaw głośność na {value} procent", f"głośność na {value} procent", f"ustaw dźwięk na {value} procent"]:
        for prefix in polite_prefixes:
            add("SetVolume", str(value), prefix + phrase)

folder_aliases = {
    "downloads": ["Pobrane", "Pobierane", "Downloads"],
    "documents": ["Dokumenty", "Moje dokumenty", "Documents"],
    "pictures": ["Zdj?cia", "Obrazy", "Pictures"],
    "music": ["Muzyka", "Music"],
    "videos": ["Wideo", "Filmy", "Videos"],
    "desktop": ["Pulpit", "Desktop"],
    "jarvis": ["Jarvis", "folder Jarvis", "projekt Jarvis"]
}
folder_templates = [
    "otw?rz {target}", "poka? {target}", "poka? mi {target}",
    "przejd? do {target}", "wejd? do {target}", "otw?rz folder {target}"
]
for canonical, aliases in folder_aliases.items():
    for alias in aliases:
        for template in folder_templates:
            for prefix in polite_prefixes:
                for suffix in suffixes:
                    add("OpenFolder", canonical, prefix + template.format(target=alias) + suffix)

settings_pages = {
    "ms-settings:": ["ustawienia"],
    "ms-settings:display": ["ekran", "ekranu", "wy?wietlanie"],
    "ms-settings:sound": ["d?wi?k", "d?wi?ku", "audio"],
    "ms-settings:apps-volume": ["mikser g?o?no?ci"],
    "ms-settings:bluetooth": ["Bluetooth"],
    "ms-settings:network-wifi": ["Wi-Fi", "WiFi"],
    "ms-settings:network-status": ["sie?", "sieci", "internet", "internetu"],
    "ms-settings:windowsupdate": ["aktualizacje", "aktualizacji", "Windows Update"],
    "ms-settings:appsfeatures": ["aplikacje", "aplikacji"],
    "ms-settings:storagesense": ["pami??", "pami?ci", "miejsce na dysku"],
    "ms-settings:powersleep": ["zasilanie", "zasilania"],
    "ms-settings:personalization": ["personalizacja", "personalizacji"],
    "ms-settings:privacy": ["prywatno??", "prywatno?ci"],
    "ms-settings:privacy-microphone": ["mikrofon", "mikrofonu"],
    "ms-settings:privacy-webcam": ["kamera", "kamery"],
    "ms-settings:notifications": ["powiadomienia", "powiadomie?"],
    "ms-settings:clipboard": ["schowek", "schowka"],
    "ms-settings:about": ["informacje o systemie"]
}
for uri, aliases in settings_pages.items():
    for alias in aliases:
        forms = ["ustawienia"] if uri == "ms-settings:" else [f"otw?rz ustawienia {alias}", f"poka? ustawienia {alias}", f"ustawienia {alias}", f"przejd? do ustawie? {alias}"]
        for phrase in forms:
            for prefix in polite_prefixes:
                for suffix in suffixes:
                    add("OpenSettings", uri, prefix + phrase + suffix)


browser_bases = {
    "BrowserNewTab": ["nowa karta", "otw\u00f3rz now\u0105 kart\u0119", "nowa zak\u0142adka"],
    "BrowserCloseTab": ["zamknij kart\u0119", "zamknij zak\u0142adk\u0119", "zamknij aktywn\u0105 kart\u0119"],
    "BrowserNextTab": ["nast\u0119pna karta", "nast\u0119pna zak\u0142adka"],
    "BrowserPreviousTab": ["poprzednia karta", "poprzednia zak\u0142adka"],
    "BrowserReload": ["od\u015bwie\u017c", "od\u015bwie\u017c stron\u0119", "prze\u0142aduj stron\u0119"],
    "BrowserBack": ["wstecz", "cofnij stron\u0119"],
    "BrowserForward": ["dalej", "do przodu"],
    "BrowserDuplicateTab": ["duplikuj kart\u0119", "duplikuj zak\u0142adk\u0119"],
    "BrowserScrollDown": ["przewi\u0144 w d\u00f3\u0142", "przewijaj w d\u00f3\u0142", "ni\u017cej"],
    "BrowserScrollUp": ["przewi\u0144 w g\u00f3r\u0119", "przewijaj w g\u00f3r\u0119", "wy\u017cej"],
    "BrowserScrollTop": ["na g\u00f3r\u0119 strony", "pocz\u0105tek strony"],
    "BrowserScrollBottom": ["na d\u00f3\u0142 strony", "koniec strony"],
    "BrowserToggleMedia": ["play pause", "pauza", "wzn\u00f3w odtwarzanie", "zatrzymaj odtwarzanie"],
    "BrowserMuteTab": ["wycisz kart\u0119", "wycisz zak\u0142adk\u0119"],
    "BrowserUnmuteTab": ["odcisz kart\u0119", "w\u0142\u0105cz d\u017awi\u0119k karty"],
    "BrowserContext": ["jaka strona jest otwarta", "co mam otwarte w przegl\u0105darce", "jaka jest aktywna karta"]
}
for intent, phrases in browser_bases.items():
    for phrase in phrases:
        for prefix in polite_prefixes:
            for suffix in suffixes:
                add(intent, None, prefix + phrase + suffix)

ordered = sorted(rows.values(), key=lambda x: (x["intent"], x["argument"] or "", x["utterance"].casefold()))
with OUT.open("w", encoding="utf-8", newline="\n") as f:
    for row in ordered:
        f.write(json.dumps(row, ensure_ascii=False) + "\n")
print(f"generated={len(ordered)} path={OUT}")
