using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.User.Controllers
{
    public class AiController : Controller
    {
        
        

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetResponse(HttpPostedFileBase audioFile, string userText)
        {
            try
            {
                string recognizedText = userText;

                // 1. SESİ YAZIYA ÇEVİR (WHISPER)
                if (audioFile != null && audioFile.ContentLength > 0)
                {
                    var filePath = Path.Combine(Server.MapPath("~/App_Data/Uploads"), "temp_input.wav");
                    if (!Directory.Exists(Path.GetDirectoryName(filePath))) Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    audioFile.SaveAs(filePath);
                    recognizedText = await TranscribeAudioAsync(filePath);

                    // İş bitince sil
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                if (string.IsNullOrEmpty(recognizedText))
                    return Json(new { success = false, message = "Ses algılanamadı." });

                // 2. DOKTOR GİBİ CEVAP ÜRET (GPT-4o veya GPT-3.5)
                string aiResponse = await AskDoctorAiAsync(recognizedText);

                // 3. YAZIYI GERÇEKÇİ SESE ÇEVİR (OPENAI TTS)
                // Bu kısım "Wow" dedirtecek kısım.
                string audioBase64 = await TextToSpeechAsync(aiResponse);

                return Json(new
                {
                    success = true,
                    userText = recognizedText,
                    aiResponse = aiResponse,
                    audioData = audioBase64 // Sesi Base64 string olarak dönüyoruz
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // --- YARDIMCI METOTLAR ---

        private async Task<string> TranscribeAudioAsync(string audioFilePath)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiKey);
                using (var form = new MultipartFormDataContent())
                using (var fs = System.IO.File.OpenRead(audioFilePath))
                {
                    form.Add(new StreamContent(fs), "file", "audio.wav");
                    form.Add(new StringContent("whisper-1"), "model");

                    var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);
                    var json = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(json))
                        return doc.RootElement.TryGetProperty("text", out var t) ? t.GetString() : "";
                }
            }
        }
        // Mevcut OpenAI key'inin hemen altına Gemini Key'ini ekle
        private readonly string OpenAiKey = ConfigurationManager.AppSettings["OpenAiApiKey"];
        private readonly string GeminiApiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
        // Sadece bu metodu aşağıdakilerle değiştir:
        private async Task<string> AskDoctorAiAsync(string userMessage)
        {
            // Gemini 1.5 Pro endpoint'i
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={GeminiApiKey}";

            using (var client = new HttpClient())
            {
                // İstediğin o harika Başhekim Prompt'u (Aynı kalıyor)
                var systemPrompt = @"
        ROLE:
        You are 'Dr. Nova', the Senior Chief Physician at Medinova Hospital. 
        You are professional, empathetic, authoritative, and medically cautious.

        LANGUAGE RULE (CRITICAL):
        - DETECT the language of the user's input.
        - REPLY ONLY IN THAT DETECTED LANGUAGE.
        - Do not mix languages.

        RESPONSE STRUCTURE (Follow this order strictly):
        1. EMPATHY & ANALYSIS: 
           - Start with a short, calming sentence. 
           - List 2-3 possible causes based on symptoms (use terms like 'Possible causes include...').
        
        2. WHAT TO DO (Treatment):
           - List 3 specific, home-care actions the patient should take immediately.
        
        3. WHAT NOT TO DO (Contraindications):
           - List 2 things they must strictly avoid.

        4. URGENCY & CONCLUSION (The 'Red Flag'):
           - Clearly state: 'If you experience [X, Y, Z symptoms], go to the nearest emergency room immediately.'
           - End with: 'This is an AI analysis. Please consult a real doctor for a final diagnosis.'

        TONE GUIDELINES:
        - Keep sentences short and clear (optimized for Text-to-Speech).
        - Do not be vague. Be specific but safe.
        - Total response length should be under 150 words.";

                // Gemini API'nin beklediği JSON formatı
                var payload = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents = new[]
                    {
                new {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                }
            },
                    generationConfig = new
                    {
                        temperature = 0.7
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                using (var doc = JsonDocument.Parse(result))
                {
                    try
                    {
                        // Gemini API'nin JSON yanıtından sadece metni çekiyoruz
                        return doc.RootElement
                                  .GetProperty("candidates")[0]
                                  .GetProperty("content")
                                  .GetProperty("parts")[0]
                                  .GetProperty("text").GetString();
                    }
                    catch (Exception ex)
                    {
                        // API'nin döndürdüğü gerçek json hatasını da mesaja ekleyelim
                        return $"Bir hata oluştu. API Yanıtı: {result} | Sistem Hatası: {ex.Message}";
                    }
                }
            }
        }
        private async Task<string> TextToSpeechAsync(string text)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiKey);

                var payload = new
                {
                    model = "tts-1",       // "tts-1-hd" daha kaliteli ama yavaş, tts-1 hızlı.
                    input = text,
                    voice = "onyx",        // Seçenekler: alloy, echo, fable, onyx (erkek-tok), nova (kadın-profesyonel), shimmer
                    response_format = "mp3"
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.openai.com/v1/audio/speech", content);

                if (response.IsSuccessStatusCode)
                {
                    var audioBytes = await response.Content.ReadAsByteArrayAsync();
                    return Convert.ToBase64String(audioBytes);
                }
                return null;
            }
        }
    }
}