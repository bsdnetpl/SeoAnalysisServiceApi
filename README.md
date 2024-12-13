SeoAnalysisServiceApi

Opis projektu

SeoAnalysisServiceApi to projekt umożliwiający analizę SEO (Search Engine Optimization) za pomocą API. Główne funkcjonalności obejmują zbieranie danych z wybranych stron internetowych oraz generowanie raportów dotyczących widoczności i optymalizacji stron.

Funkcjonalności

Analiza kluczowych wskaźników SEO, takich jak:

Tagi meta (tytuł, opis, słowa kluczowe).

Struktura nagłówków (H1, H2, H3).

Liczba słów na stronie.

Wewnętrzne i zewnętrzne linki.

Weryfikacja indeksacji w wyszukiwarkach.

Integracja z zewnętrznymi narzędziami SEO.

Generowanie raportów w formacie JSON.

Wymagania systemowe

Platforma: .NET 6.0 lub nowsza

Zależności:

ASP.NET Core

Biblioteki do obsługi HTTP

Newtonsoft.Json

Instrukcja instalacji

Sklonuj repozytorium:

git clone https://github.com/bsdnetpl/SeoAnalysisServiceApi.git

Przejdź do katalogu projektu:

cd SeoAnalysisServiceApi

Przygotuj środowisko:

Zainstaluj wymagane zależności przy użyciu polecenia:

dotnet restore

Skonfiguruj aplikację:

Utwórz plik konfiguracyjny appsettings.json lub skorzystaj z domyślnych ustawień w repozytorium.

Uruchom projekt:

dotnet run

Endpoints API

1. GET /api/seo/analysis

Opis: Analiza podanej strony internetowej.

Parametry:

url (query) - Adres URL strony do analizy.

Przykład zapytania:

curl -X GET "http://localhost:5000/api/seo/analysis?url=https://example.com"

2. POST /api/seo/report

Opis: Generowanie raportu SEO.

Body:

{
  "url": "https://example.com",
  "options": {
    "includeMeta": true,
    "checkLinks": true
  }
}

Przykład zapytania:

curl -X POST "http://localhost:5000/api/seo/report" \
-H "Content-Type: application/json" \
-d '{"url":"https://example.com","options":{"includeMeta":true,"checkLinks":true}}'

Struktura projektu

/Controllers - Kontrolery API.

/Services - Logika biznesowa.

/Models - Definicje modeli danych.

/wwwroot - Pliki statyczne.

Testy

Uruchomienie testów jednostkowych:

dotnet test

Weryfikacja endpointów:

Możesz skorzystać z narzędzi takich jak Postman lub cURL.

Autor

Projekt stworzony przez bsdnetpl.

Licencja

Projekt objęty licencją MIT. Szczegóły znajdziesz w pliku LICENSE.
