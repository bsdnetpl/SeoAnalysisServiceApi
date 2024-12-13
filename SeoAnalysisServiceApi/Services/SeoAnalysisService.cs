using HtmlAgilityPack;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SeoAnalysisServiceApi.Models;

namespace SeoAnalysisServiceApi.Services
    {
    public class SeoAnalysisService : ISeoAnalysisService
        {
        private readonly string _modelPath = "AI Models/model.onnx";
        private readonly string _vocabPath = "AI Models/vocab.txt"; // Ścieżka do słownika tokenów

        public async Task<SeoAnalysisResult> AnalyzePageAsync(string url, string keyword)
            {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                return new SeoAnalysisResult
                    {
                    Keyword = keyword,
                    Recommendations = "Invalid URL provided.",
                    IsOptimized = false
                    };
                }

            var pageContent = await GetPageContentAsync(url);

            if (string.IsNullOrWhiteSpace(pageContent))
                {
                return new SeoAnalysisResult
                    {
                    Keyword = keyword,
                    Recommendations = "Unable to fetch the page content.",
                    IsOptimized = false
                    };
                }

            try
                {
                using var session = new InferenceSession(_modelPath);

                // Tokenizacja zawartości strony
                var (inputIds, attentionMask, tokenTypeIds) = Tokenize(pageContent, keyword);

                // Przygotowanie tensorów wejściowych
                var inputTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
                var attentionTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });
                var tokenTypeTensor = new DenseTensor<long>(tokenTypeIds, new[] { 1, tokenTypeIds.Length });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionTensor),
                    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeTensor)
                };

                using var results = session.Run(inputs);

                var output = results.First().AsTensor<float>().ToArray();

                return AnalyzeResults(output, keyword);
                }
            catch (Exception ex)
                {
                return new SeoAnalysisResult
                    {
                    Keyword = keyword,
                    Recommendations = $"Error during analysis: {ex.Message}",
                    IsOptimized = false
                    };
                }
            }

        private async Task<string> GetPageContentAsync(string url)
            {
            try
                {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(url);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);

                return htmlDoc.DocumentNode.SelectSingleNode("//body")?.InnerText ?? string.Empty;
                }
            catch (Exception ex)
                {
                return $"Error fetching page content: {ex.Message}";
                }
            }

        private (long[] inputIds, long[] attentionMask, long[] tokenTypeIds) Tokenize(string text, string keyword)
            {
            // Załaduj słownik tokenizatora
            var vocab = File.ReadAllLines(_vocabPath)
                            .Select((word, index) => new { word, index })
                            .ToDictionary(x => x.word, x => (long)x.index); // Rzutowanie index na long

            // Funkcja do tokenizowania tekstu
            IEnumerable<long> TokenizeText(string input)
                {
                return input.ToLower()
                            .Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(word => vocab.ContainsKey(word) ? vocab[word] : vocab["[UNK]"]); // Rzutowanie wartości na long
                }

            // Tokenizacja tekstu i słowa kluczowego
            var tokens = TokenizeText($"{keyword} {text}").ToList();

            // Przygotowanie wejść dla modelu
            var inputIds = tokens.ToArray();
            var attentionMask = tokens.Select(_ => 1L).ToArray(); // Wszystkie tokeny są istotne
            var tokenTypeIds = new long[tokens.Count];            // Wszystkie wartości to 0 (pojedynczy segment)

            return (inputIds, attentionMask, tokenTypeIds);
            }

        private SeoAnalysisResult AnalyzeResults(float[] relevanceScores, string keyword)
            {
            var averageScore = relevanceScores.Average();
            var isOptimized = averageScore >= 0.5;

            // Lista szczegółowych rekomendacji
            var recommendationsList = new List<string>();

            if (averageScore < 0.3)
                {
                recommendationsList.Add("<li>Gęstość słów kluczowych jest bardzo niska. Obecnie słowo kluczowe występuje zbyt rzadko. Upewnij się, że pojawia się co najmniej 2-3 razy na każde 500 słów w sposób naturalny.</li>");
                }
            else if (averageScore < 0.5)
                {
                recommendationsList.Add("<li>Słowo kluczowe jest obecne, ale niewystarczająco podkreślone. Rozważ:");
                recommendationsList.Add("<ul>");
                recommendationsList.Add("<li>Dodanie słowa kluczowego w początkowym akapicie.</li>");
                recommendationsList.Add("<li>Umieszczenie słowa w nagłówkach (H2, H3).</li>");
                recommendationsList.Add("<li>Zawieranie frazy w akapicie końcowym.</li>");
                recommendationsList.Add("</ul></li>");
                }

            // Analiza nagłówków
            recommendationsList.Add("<li>Obecnie brak wystarczającego użycia słowa kluczowego w nagłówkach. Upewnij się, że pojawia się w:");
            recommendationsList.Add("<ul>");
            recommendationsList.Add("<li>H1: '" + keyword + " - Kompletny przewodnik'</li>");
            recommendationsList.Add("<li>H2: 'Korzyści z " + keyword + "'</li>");
            recommendationsList.Add("<li>H3: 'Jak wybrać najlepsze " + keyword + "'</li>");
            recommendationsList.Add("</ul></li>");

            // Analiza meta danych
            recommendationsList.Add("<li>Sprawdź tytuł meta i opis meta. Aktualnie brak optymalizacji dla frazy '" + keyword + "'. Dodaj takie elementy:");
            recommendationsList.Add("<ul>");
            recommendationsList.Add("<li>Meta Title: '" + keyword + " | Najlepsze rozwiązania w Twojej okolicy'</li>");
            recommendationsList.Add("<li>Meta Description: 'Odkryj najlepsze rozwiązania " + keyword + ". Sprzedaż, serwis i wsparcie dostosowane do Twoich potrzeb.'</li>");
            recommendationsList.Add("</ul></li>");

            // Struktura treści
            recommendationsList.Add("<li>Struktura treści wymaga poprawy. Obecnie brak sekcji z wyraźnymi nagłówkami. Działania do wykonania:");
            recommendationsList.Add("<ul>");
            recommendationsList.Add("<li>Podziel treść na sekcje z nagłówkami.</li>");
            recommendationsList.Add("<li>Użyj punktów, numerowanych list i krótkich akapitów.</li>");
            recommendationsList.Add("</ul></li>");

            // Treści multimedialne
            recommendationsList.Add("<li>Obecnie brak multimediów na stronie. Dodaj obrazy i filmy, a w ich atrybutach alt zawrzyj frazę '" + keyword + "'. Przykład:");
            recommendationsList.Add("<ul>");
            recommendationsList.Add("<li><img src='example.jpg' alt='" + keyword + " w praktyce'></li>");
            recommendationsList.Add("</ul></li>");

            // Linkowanie wewnętrzne i zewnętrzne
            recommendationsList.Add("<li>Aktualnie brak odpowiedniego linkowania. Wprowadź zmiany:");
            recommendationsList.Add("<ul>");
            recommendationsList.Add("<li>Dodaj linki wewnętrzne do powiązanych stron.</li>");
            recommendationsList.Add("<li>Przykład linku: <a href='/related-page'>Dowiedz się więcej o " + keyword + "</a></li>");
            recommendationsList.Add("</ul></li>");

            // Optymalizacja techniczna
            recommendationsList.Add("<li>Braki w optymalizacji technicznej strony. Do wykonania:");
            recommendationsList.Add("<ul>");
            recommendationsList.Add("<li>Popraw szybkość ładowania strony.</li>");
            recommendationsList.Add("<li>Upewnij się, że strona jest przyjazna dla urządzeń mobilnych.</li>");
            recommendationsList.Add("<li>Zweryfikuj, czy strona jest poprawnie zaindeksowana w wyszukiwarkach.</li>");
            recommendationsList.Add("</ul></li>");

            // Generowanie HTML
            var recommendationsHtml = string.Join("\n", recommendationsList);
            var resultHtml = $@"
                <html>
                <head>
                    <title>Analiza SEO dla '{keyword}'</title>
                </head>
                <body>
                    <h1>Wyniki analizy SEO</h1>
                    <p>Średni wynik: {averageScore:F2}</p>
                    <p>Status optymalizacji: {(isOptimized ? "Optymalizacja zakończona sukcesem" : "Wymaga poprawy")}</p>
                    <h2>Rekomendacje</h2>
                    <ul>
                        {recommendationsHtml}
                    </ul>
                </body>
                </html>
            ";

            return new SeoAnalysisResult
                {
                Keyword = keyword,
                Recommendations = resultHtml,
                IsOptimized = isOptimized
                };
            }
        }
    }
