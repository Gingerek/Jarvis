# Jarvis — docelowy brief interfejsu

Status: obowiązujący kierunek wizualny
Data: 2026-09-08

## Główna metafora
Interfejs ma przypominać futurystyczną płytę główną komputera widzianą w wysokiej jakości 3D.
Centralny rdzeń Jarvisa jest połączony z siecią świetlnych ścieżek i obwodów biegnących w wielu kierunkach.
Całość pozostaje minimalistyczna: mało tekstu, brak zbędnych paneli, brak dekoracyjnych atrap.

## Reakcja na rzeczywisty stan
Każda animacja i zmiana światła musi wynikać z prawdziwego stanu VoiceHosta lub wykonania komendy.
SLEEPING: spokojny, dyskretny stan płyty i rdzenia.
LISTENING: ścieżki reagują na głos użytkownika i widocznie przenoszą energię do rdzenia.
PROCESSING: energia rozchodzi się po innych trasach, sugerując analizę i wybór modułów.
SPEAKING: osobna paleta i ruch energii od rdzenia na zewnątrz, zsynchronizowany z odpowiedzią lektora.
ERROR: krótki czytelny sygnał, bez agresywnej czerwonej dominacji.

## Jakość i głębia
Projekt ma być wyraźnie przestrzenny: warstwy, głębia, subtelne odbicia, światło wolumetryczne i perspektywa.
Nie może wyglądać jak płaska tapeta ani klasyczny dashboard administracyjny.
Ruch powinien być płynny i oszczędny, bez przesadnych efektów gamingowych.

## Reakcja na audio
LISTENING powinien wykorzystywać rzeczywistą energię wejściowego sygnału audio do sterowania intensywnością ścieżek.
SPEAKING powinien reagować na rzeczywistą amplitudę odtwarzanego TTS, a nie na sztuczny timer.
Kolory stanu użytkownika i lektora muszą być od siebie jednoznacznie odróżnione.

## Zasady produktu
Brak ślepych przycisków i atrap funkcjonalności.
Widoczny stan zawsze odpowiada rzeczywistemu stanowi systemu.
UI ma pozostawać szybkie i nie może pogarszać opóźnienia Fast Path.
Finalna implementacja wizualna nastąpi po ustabilizowaniu głównych executorów i telemetrii stanów.
