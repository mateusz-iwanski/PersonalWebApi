using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Events;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Content;
using System.Threading.Tasks;

namespace PersonalWebApi.Processes.FileStorage.Steps
{
    public static class FileMetadataComposerFunction
    {
        public const string Collect = nameof(Collect);
    }

    /// <summary>
    /// CollectAsync Method: This method collects metadata from the uploaded file, computes the SHA-256 hash, calculates readability scores, and emits an event with the collected metadata.
    /// CountWords Method: Counts the number of words in the given text using a regular expression.
    /// CountSentences Method: Counts the number of sentences in the given text using a regular expression.
    /// CountSyllables Method: Counts the number of syllables in the given text by splitting it into words and counting syllables in each word.
    /// CountSyllablesInWord Method: Counts the number of syllables in a single word by checking for vowels and handling special cases.
    /// CalculateFleschReadingEase Method: Calculates the Flesch Reading Ease score using the formula.
    /// CalculateFleschKincaidGradeLevel Method: Calculates the Flesch-Kincaid Grade Level score using the formula.
    /// </summary>
    [Experimental("SKEXP0080")]
    public class FileMetadataComposer : KernelProcessStep
    {
        /// <summary>
        /// Collects metadata from the uploaded file and emits an event with the collected metadata.
        /// </summary>
        /// <param name="context">The context of the kernel process step.</param>
        /// <param name="kernel">The kernel instance.</param>
        /// <param name="documentStepDto">The document step data transfer object containing the file and metadata.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the uploaded file is null.</exception>
        [KernelFunction(FileMetadataComposerFunction.Collect)]
        public async ValueTask CollectAsync(KernelProcessStepContext context, Kernel kernel, DocumentStepDto documentStepDto)
        {
            if (documentStepDto.iFormFile is null)
            {
                throw new ArgumentNullException(nameof(documentStepDto.iFormFile));
            }

            documentStepDto.Metadata.Add("name", documentStepDto.iFormFile.FileName);
            documentStepDto.Metadata.Add("content_type", documentStepDto.iFormFile.ContentType);
            documentStepDto.Metadata.Add("length_bytes", documentStepDto.iFormFile.Length.ToString());
            documentStepDto.Metadata.Add("file_name", documentStepDto.iFormFile.FileName);
            documentStepDto.Metadata.Add("content_disposition", documentStepDto.iFormFile.ContentDisposition);
            documentStepDto.Metadata.Add("header", documentStepDto.iFormFile.Headers?.ToString() ?? "");
            documentStepDto.Metadata.Add("uploaded_at", DateTime.UtcNow.ToString("o"));
            documentStepDto.Metadata.Add("i_form_file", "True");
            documentStepDto.Metadata.Add("header_name", documentStepDto.iFormFile.Name);

            // Compute SHA-256 hash of the file content
            using (var sha256 = SHA256.Create())
            {
                using (var stream = documentStepDto.iFormFile.OpenReadStream())
                {
                    var hashBytes = await sha256.ComputeHashAsync(stream);
                    var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    documentStepDto.Metadata.Add("sha256_hash", hashString);
                }
            }

            // Add all headers to metadata
            if (documentStepDto.iFormFile.Headers != null)
            {
                foreach (var header in documentStepDto.iFormFile.Headers)
                {
                    documentStepDto.Metadata.Add($"header_{header.Key.ToLower()}", header.Value.ToString());
                }
            }

            // Calculate Flesch Reading Ease and Flesch-Kincaid Grade Level
            string content = documentStepDto.Content;
            int wordCount = CountWords(content);
            int sentenceCount = CountSentences(content);
            int syllableCount = CountSyllables(content);

            if (wordCount > 0 && sentenceCount > 0)
            {
                double fleschReadingEase = CalculateFleschReadingEase(wordCount, sentenceCount, syllableCount);
                double fleschKincaidGradeLevel = CalculateFleschKincaidGradeLevel(wordCount, sentenceCount, syllableCount);

                documentStepDto.Metadata.Add("flesch_reading_ease", fleschReadingEase.ToString("F2"));
                documentStepDto.Metadata.Add("flesch_kincaid_grade_level", fleschKincaidGradeLevel.ToString("F2"));
            }
            else
            {
                documentStepDto.Metadata.Add("flesch_reading_ease", "N/A");
                documentStepDto.Metadata.Add("flesch_kincaid_grade_level", "N/A");
            }

            await context.EmitEventAsync(
                new()
                {
                    Id = FileEvents.MetadataCollected,
                    Data = documentStepDto
                });
        }


        /// <summary>
        /// Counts the number of words in the given text.
        /// </summary>
        /// <param name="text">The text to count words in.</param>
        /// <returns>The number of words in the text.</returns>
        private int CountWords(string text)
        {
            return Regex.Matches(text, @"\b\w+\b").Count;
        }

        /// <summary>
        /// Counts the number of sentences in the given text.
        /// </summary>
        /// <param name="text">The text to count sentences in.</param>
        /// <returns>The number of sentences in the text.</returns>
        private int CountSentences(string text)
        {
            return Regex.Matches(text, @"[.!?]").Count;
        }

        /// <summary>
        /// Counts the number of syllables in the given text.
        /// </summary>
        /// <param name="text">The text to count syllables in.</param>
        /// <returns>The number of syllables in the text.</returns>
        private int CountSyllables(string text)
        {
            int syllableCount = 0;
            foreach (var word in text.Split(' '))
            {
                syllableCount += CountSyllablesInWord(word);
            }
            return syllableCount;
        }

        /// <summary>
        /// Counts the number of syllables in a single word.
        /// </summary>
        /// <param name="word">The word to count syllables in.</param>
        /// <returns>The number of syllables in the word.</returns>
        private int CountSyllablesInWord(string word)
        {
            word = word.ToLower().Trim();
            if (word.Length == 0) return 0;

            int count = 0;
            bool lastWasVowel = false;
            string vowels = "aeiouy";

            foreach (char c in word)
            {
                if (vowels.Contains(c))
                {
                    if (!lastWasVowel)
                    {
                        count++;
                        lastWasVowel = true;
                    }
                }
                else
                {
                    lastWasVowel = false;
                }
            }

            if (word.EndsWith("e")) count--;
            if (count == 0) count = 1;

            return count;
        }

        /// <summary>
        /// Calculates the Flesch Reading Ease score for the given text.
        /// </summary>
        /// <param name="wordCount">The number of words in the text.</param>
        /// <param name="sentenceCount">The number of sentences in the text.</param>
        /// <param name="syllableCount">The number of syllables in the text.</param>
        /// <returns>The Flesch Reading Ease score.</returns>
        private double CalculateFleschReadingEase(int wordCount, int sentenceCount, int syllableCount)
        {
            return 206.835 - (1.015 * ((double)wordCount / sentenceCount)) - (84.6 * ((double)syllableCount / wordCount));
        }

        /// <summary>
        /// Calculates the Flesch-Kincaid Grade Level score for the given text.
        /// </summary>
        /// <param name="wordCount">The number of words in the text.</param>
        /// <param name="sentenceCount">The number of sentences in the text.</param>
        /// <param name="syllableCount">The number of syllables in the text.</param>
        /// <returns>The Flesch-Kincaid Grade Level score.</returns>
        private double CalculateFleschKincaidGradeLevel(int wordCount, int sentenceCount, int syllableCount)
        {
            return (0.39 * ((double)wordCount / sentenceCount)) + (11.8 * ((double)syllableCount / wordCount)) - 15.59;
        }
    }
}
