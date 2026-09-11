using System.Globalization;
using System.Text.Json;

namespace Jarvis.Execution;

public sealed record WeatherResult(bool Success, string Message);

public sealed class WeatherService
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };

    public async Task<WeatherResult> GetCurrentAsync(string? location = null, CancellationToken cancellationToken = default)
    {
        location = string.IsNullOrWhiteSpace(location) ? "Helmond" : location.Trim();
        try
        {
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1&language=pl&format=json";
            using var geoResponse = await Client.GetAsync(geoUrl, cancellationToken);
            geoResponse.EnsureSuccessStatusCode();
            using var geo = JsonDocument.Parse(await geoResponse.Content.ReadAsStringAsync(cancellationToken));
            if (!geo.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return new(false, $"Nie znalazłem lokalizacji {location}.");
            var place = results[0];
            var lat = place.GetProperty("latitude").GetDouble();
            var lon = place.GetProperty("longitude").GetDouble();
            var name = place.GetProperty("name").GetString() ?? location;
            var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(CultureInfo.InvariantCulture)}&longitude={lon.ToString(CultureInfo.InvariantCulture)}&current=temperature_2m,apparent_temperature,precipitation,weather_code,wind_speed_10m&timezone=auto";
            using var weatherResponse = await Client.GetAsync(forecastUrl, cancellationToken);
            weatherResponse.EnsureSuccessStatusCode();
            using var weather = JsonDocument.Parse(await weatherResponse.Content.ReadAsStringAsync(cancellationToken));
            var current = weather.RootElement.GetProperty("current");
            var temp = current.GetProperty("temperature_2m").GetDouble();
            var feels = current.GetProperty("apparent_temperature").GetDouble();
            var precipitation = current.GetProperty("precipitation").GetDouble();
            var wind = current.GetProperty("wind_speed_10m").GetDouble();
            var code = current.GetProperty("weather_code").GetInt32();
            var rainText = precipitation > 0.05 ? $" Opad {precipitation:F1} milimetra." : string.Empty;
            return new(true, $"W {name} jest {temp:F0} stopni, odczuwalnie {feels:F0}. {Describe(code)}. Wiatr {wind:F0} kilometrów na godzinę.{rainText}");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "Serwis pogodowy nie odpowiedział na czas.");
        }
        catch
        {
            return new(false, "Nie udało się pobrać aktualnej pogody.");
        }
    }

    private static string Describe(int code) => code switch
    {
        0 => "Bezchmurnie",
        1 => "Przeważnie pogodnie",
        2 => "Częściowe zachmurzenie",
        3 => "Pochmurno",
        45 or 48 => "Mgła",
        >= 51 and <= 57 => "Mżawka",
        >= 61 and <= 67 => "Deszcz",
        >= 71 and <= 77 => "Śnieg",
        >= 80 and <= 82 => "Przelotne opady",
        85 or 86 => "Przelotny śnieg",
        >= 95 and <= 99 => "Burza",
        _ => "Warunki pogodowe są zmienne"
    };
}
