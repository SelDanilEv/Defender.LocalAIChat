using System.Text;
using AILocalHelper.DB;
using AILocalHelper.Domain;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace AILocalHelper.AI
{
    public class ChatService
    {
        private static string _modelPath = String.Empty;
        private LiteDBService _dBService;
        private static Model? _model;
        private static Tokenizer? _tokenizer;

        private const string SystemPrompt =
            "You are an AI friend, try just to keep any conversation with short answers" +
            "Answer using a direct style. Do not provide more information than requested by user. " +
            "Answer user's question as short as possible.";

        public ChatService(LiteDBService dBService)
        {
            _dBService = dBService;
        }

        public void Ask(string userPrompt)
        {
            if (_dBService.GetConfig().IsLocked) return;
            _dBService.SetLock(true);

            try
            {
                _dBService.AddToHistory(Actor.User, userPrompt);

                if (userPrompt.Length == 0)
                {
                    return;
                }

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
                Task.Run(() => GenerateResponse(context, generator));

                _dBService.AddToHistory(Actor.AI, string.Empty);
            }
            catch (Exception ex)
            {
                _dBService.SetLock(false);

                var message = $"An error occurred: {ex.Message}";

                _dBService.AddToHistory(Actor.System, message);
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

                    _dBService.UpdateLastHistoryRecord(result.ToString());
                }
            }
            finally
            {
                _dBService.SetLock(false);

                _dBService.UpdateContext(context + result);
            }
        }

    }
}
