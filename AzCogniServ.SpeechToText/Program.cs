using AzCogniServ.SpeechToText;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

/*
 * Based on the articles: https://learn.microsoft.com/en-us/azure/cognitive-services/speech-service/how-to-use-codec-compressed-audio-input-streams?tabs=windows%2Cdebian%2Cjava-android%2Cterminal&pivots=programming-language-csharp
 *  and https://learn.microsoft.com/es-ES/azure/cognitive-services/language-service/key-phrase-extraction/quickstart?pivots=programming-language-csharp
 * And on the Github code samples at: https://github.com/Azure-Samples/cognitive-services-speech-sdk.git
 */
if (args.Length == 0)
{
    Console.WriteLine("Missing audio file complete filename");
    return 1;
}

var speechConfig = SpeechConfig.FromSubscription("6556e50f4aaf4c7f9398ce9a2e0b63fa", "westeurope");
var streamFormat = AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.ANY);
var callback = new AudioCallback(args[0]);
var pushStream = AudioInputStream.CreatePullStream(callback, streamFormat);
var audioConfig = AudioConfig.FromStreamInput(pushStream);

using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
var result = await recognizer.RecognizeOnceAsync();

Console.Write("Result: {0}{1}", result.Text, Environment.NewLine);

var languageCredentials = new AzureKeyCredential("9304983474cd40fc8d6a200991b2a5e3");
var client = new TextAnalyticsClient(new Uri("https://lang-kandu.cognitiveservices.azure.com/"), languageCredentials);
var keyPhrasesResponse = client.ExtractKeyPhrases(result.Text);
var sentimentResponse = client.AnalyzeSentiment(result.Text);
var entitiesResponse = client.RecognizeEntities(result.Text);

Console.WriteLine("Key phrases:");

foreach (var keyphrase in keyPhrasesResponse.Value)
    Console.WriteLine($"\t{keyphrase}");

Console.WriteLine($"Sentiment: {sentimentResponse.Value.Sentiment}:P={sentimentResponse.Value.ConfidenceScores.Positive},N={sentimentResponse.Value.ConfidenceScores.Negative},X={sentimentResponse.Value.ConfidenceScores.Neutral}");

Console.WriteLine("Entities:");

foreach (var entity in entitiesResponse.Value)
    Console.WriteLine($"\t{entity.Text}:{entity.ConfidenceScore} / {entity.Category} / {entity.SubCategory}");

return 0;