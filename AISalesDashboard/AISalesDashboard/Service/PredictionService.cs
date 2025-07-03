﻿using System.Text.RegularExpressions;

namespace AISalesDashboard
{
    public class PredictionService
    {
        private readonly AzureBaseService _baseAIService;
        private readonly SalesDataService _salesDataService;

        public PredictionService(AzureBaseService baseAIService, SalesDataService salesDataService)
        {
            _baseAIService = baseAIService;
            _salesDataService = salesDataService;
        }

        public async Task<List<SalesPrediction>> GetSalesPredictionsAsync(DateRange historicalPeriod, DateRange futurePeriod)
        {
            // Get historical data for the model
            var historicalData = await _salesDataService.GetSalesDataAsync(historicalPeriod);
            if (!historicalData.Any())
            {
                return new List<SalesPrediction>();
            }

            // Get predictions from OpenAI
            return await _baseAIService.PredictSalesTrend(historicalData, futurePeriod);
        }

        internal async Task<string> GetRandomExplanationAsync(List<SalesPrediction> predictions)
        {
            string prompt = $"Based on the {predictions.Count} sales data points, provide an insightful explanation.";
            string insights = await _baseAIService.GetAnswerFromGPT(prompt);
            return CleanAndFormatOutput(insights);
        }

        private string CleanAndFormatOutput(string aiResponse)
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
                return string.Empty;

            aiResponse = aiResponse.Replace("####", "")
                                   .Replace("###", "")
                                   .Replace("**", "")
                                   .Replace("/", "")
                                   .Replace("[", "")
                                   .Replace("]", "");

            aiResponse = Regex.Replace(aiResponse, @"(\d+\.\s)([A-Za-z\s]+)", 
                m => $"{m.Groups[1].Value}{m.Groups[2].Value.Trim()}");

            return aiResponse.Trim();
        }
    }
}