using System.Text;
using AILocalHelper.DB;
using AILocalHelper.Domain;
using AILocalHelper.Messaging;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace AILocalHelper.AI
{
    public class ChatService
    {
        private static string _modelPath = String.Empty;
        private LiteDBService _dBService;
        private static Model? _model;
        private static Tokenizer? _tokenizer;
        private readonly CommunicationService _communicationService;

        private const string SystemPrompt =
            "You are an AI friend, try just to keep any conversation with short answers" +
            "Answer using a direct style. Do not provide more information than requested by user. " +
            "Answer user's question as short as possible.";

        public ChatService(LiteDBService dBService, CommunicationService communicationService)
        {
            _dBService = dBService;
            _communicationService = communicationService;
        }

        public async Task Ask(string userPrompt)
        {
            try
            {
                await _communicationService.AddHistoryRecord(
                    HistoryRecord.CreateUserRecord(userPrompt));

                if (_model == null || _tokenizer == null ||
                    _dBService.GetConfig().PathToModel != _modelPath)
                {
                    _modelPath = _dBService.GetConfig().PathToModel;
                    _model = new Model(_modelPath);
                    _tokenizer = new Tokenizer(_model);
                }

                var context = GetOrInitializeContext(userPrompt);

                var tokens = _tokenizer.Encode(context);
                var generatorParams = new GeneratorParams(_model);
                generatorParams.SetSearchOption("max_length", 1024);
                generatorParams.SetSearchOption("past_present_share_buffer", false);
                generatorParams.SetInputSequences(tokens);

                var generator = new Generator(_model, generatorParams);
                GenerateResponse(context, generator);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred: {ex.Message}";

                await _communicationService.AddHistoryRecord(
                    HistoryRecord.CreateSystemRecord(message));
            }
        }

        private string GetOrInitializeContext(string userPrompt)
        {
            var context = _dBService.GetConfig().Context;
            if (string.IsNullOrWhiteSpace(context))
            {
                context = $"<|system|>{SystemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";
            }
            else
            {
                context += $"<|user|>{userPrompt}<|end|><|assistant|>";
            }
            return context;
        }

        private void GenerateResponse(
            string context,
            Generator generator)
        {
            var result = new StringBuilder();

            var record = HistoryRecord.CreateAIRecord(String.Empty);

            try
            {
                while (!generator.IsDone())
                {
                    generator.ComputeLogits();
                    generator.GenerateNextToken();
                    var outputTokens = generator.GetSequence(0);
                    var newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
                    var output = _tokenizer.Decode(newToken);
                    result.Append(output);

                    record.Message = result.ToString();
                    _communicationService.SetPartialAIResponse(record)
                        .GetAwaiter().GetResult();
                }
            }
            finally
            {  
                _dBService.UpdateContext(context + result);

                record.Message = result.ToString();
                _communicationService.AddHistoryRecord(record)
                        .GetAwaiter().GetResult();
            }
        }

    }
}
