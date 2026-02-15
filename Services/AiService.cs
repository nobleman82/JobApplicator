using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace JobApplicator2.Services
{
    public class AiService
    {
        private readonly HttpClient _http;
        private readonly DatabaseService _db;

        public AiService(HttpClient http, DatabaseService db)
        {
            _http = http;
            _db = db;
        }

        public async Task<string> CallAiAsync(string prompt, bool useStreaming = false, Action<string>? onStreamChunk = null)
        {
            var config = await _db.GetAiConfigAsync();

            if (config.SelectedProvider == "Ollama")
            {
                return await CallOllamaInternal(config, prompt, useStreaming, onStreamChunk);
            }
            else
            {
                // Hinweis: Gemini Streaming erfordert ein anderes URL-Endsegment. 
                // Für den Editor nutzen wir hier meist den Standard-Call.
                return await CallGeminiInternal(config, prompt);
            }
        }

        /// <summary>
        /// Spezielle Methode für die Analyse von Bildern (Screenshots von Vorlagen)
        /// </summary>
        public async Task<string> CallAiWithImageAsync(string prompt, string base64Image, string mimeType)
        {
            var config = await _db.GetAiConfigAsync();

            // Vision-Features sind aktuell primär über Gemini (Flash/Pro) sinnvoll nutzbar
            return await CallGeminiInternal(config, prompt, base64Image, mimeType);
        }

        private async Task<string> CallOllamaInternal(AiConfig config, string prompt, bool stream, Action<string>? chunkAction)
        {
            var payload = new { model = config.DefaultOllamaModel, prompt = prompt, stream = stream };
            var url = $"{config.OllamaUrl}/api/generate";

            if (!stream)
            {
                var resp = await _http.PostAsJsonAsync(url, payload);
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
                return json.GetProperty("response").GetString() ?? "";
            }

            using var response = await _http.PostAsJsonAsync(url, payload);
            using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            string fullText = "";
            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var jsonLine = JsonDocument.Parse(line);
                var part = jsonLine.RootElement.GetProperty("response").GetString();
                fullText += part;
                chunkAction?.Invoke(part ?? "");
            }
            return fullText;
        }

        private async Task<string> CallGeminiInternal(AiConfig config, string prompt, string? base64Image = null, string? mimeType = null)
        {
            if (string.IsNullOrWhiteSpace(config.GeminiApiKey))
            {
                throw new Exception("Kein Gemini API-Key hinterlegt!");
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{config.GeminiModel}:generateContent?key={config.GeminiApiKey}";

            // Aufbau der Parts (Text + optional Bild)
            var parts = new List<object>();
            parts.Add(new { text = prompt });

            if (!string.IsNullOrEmpty(base64Image))
            {
                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = mimeType ?? "image/png",
                        data = base64Image
                    }
                });
            }

            var payload = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    temperature = config.Temperature,
                    maxOutputTokens = 4096, // Erhöht für komplexe HTML/CSS Generierung
                    responseMimeType = "application/json" // Wir erzwingen JSON für den Template-Generator
                }
            };

            try
            {
                var resp = await _http.PostAsJsonAsync(url, payload);

                if (!resp.IsSuccessStatusCode)
                {
                    var errorBody = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API Fehler: {resp.StatusCode} - {errorBody}");
                }

                var json = await resp.Content.ReadFromJsonAsync<JsonElement>();

                // Gemini Antwortstruktur parsen
                if (json.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var content))
                    {
                        var responseText = content.GetProperty("parts")[0].GetProperty("text").GetString();
                        return responseText ?? "Keine Antwort erhalten.";
                    }
                }

                return "Keine gültige Antwortstruktur von Gemini erhalten.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Verbindung zu Gemini fehlgeschlagen: {ex.Message}");
            }
        }

        public async Task<List<string>> ListGeminiModelsAsync()
        {
            var config = await _db.GetAiConfigAsync();
            if (string.IsNullOrEmpty(config.GeminiApiKey)) return new List<string>();

            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={config.GeminiApiKey}";

            try
            {
                var resp = await _http.GetFromJsonAsync<JsonElement>(url);
                var models = new List<string>();
                if (resp.TryGetProperty("models", out var modelList))
                {
                    foreach (var m in modelList.EnumerateArray())
                    {
                        var name = m.GetProperty("name").GetString() ?? "";
                        var methods = m.GetProperty("supportedGenerationMethods").EnumerateArray();

                        if (methods.Any(x => x.GetString() == "generateContent"))
                        {
                            models.Add(name.Replace("models/", ""));
                        }
                    }
                }
                return models;
            }
            catch (Exception ex)
            {
                return new List<string> { "Fehler beim Laden: " + ex.Message };
            }
        }

        public async Task<List<string>> GetOllamaModelsAsync()
        {
            var config = await _db.GetAiConfigAsync();
            try
            {
                var resp = await _http.GetFromJsonAsync<JsonElement>($"{config.OllamaUrl}/api/tags");
                return resp.GetProperty("models").EnumerateArray().Select(m => m.GetProperty("name").GetString() ?? "").ToList();
            }
            catch { return new List<string> { "llama3.2", "mistral" }; }
        }
    }
}