using Bocifus.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Bocifus.DataManagement
{
    using Model;

    internal class DataManager
    {
        public static ConversationManager LoadConversationsFromJson()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dataDirectory = Path.Combine(documentsPath, "OpenAIOnWPF", "ConversationHistory");

            var manager = new ConversationManager();
            manager.Histories = new ObservableCollection<ConversationHistory>();

            Directory.CreateDirectory(dataDirectory);

            var files = Directory.GetFiles(dataDirectory, "Conversation_*.json");

            foreach (var file in files)
            {
                var jsonString = File.ReadAllText(file);
                var conversation = System.Text.Json.JsonSerializer.Deserialize<ConversationHistory>(jsonString);

                if (conversation != null)
                {
                    manager.Histories.Add(conversation);
                }
            }
            return manager;
        }
        public static void SaveConversationsAsJson(ConversationManager manager)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dataDirectory = Path.Combine(documentsPath, "OpenAIOnWPF", "ConversationHistory");

            Directory.CreateDirectory(dataDirectory);

            foreach (var file in Directory.EnumerateFiles(dataDirectory, "*.json"))
            {
                File.Delete(file);
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 非ASCII文字をエスケープしない
            };

            foreach (var conversation in manager.Histories)
            {
                var formattedLastUpdated = conversation.LastUpdated.ToString("yyyyMMddHHmmss");
                var filePath = Path.Combine(dataDirectory, $"Conversation_{formattedLastUpdated}_{conversation.ID}.json");
                var jsonString = System.Text.Json.JsonSerializer.Serialize(conversation, options);

                File.WriteAllText(filePath, jsonString);
            }
        }
        public static void SavePromptTemplateAsJson(PromptTemplateManager manager)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dataDirectory = Path.Combine(documentsPath, "OpenAIOnWPF", "PromptTemplate");

            Directory.CreateDirectory(dataDirectory);

            foreach (var file in Directory.EnumerateFiles(dataDirectory, "*.json"))
            {
                File.Delete(file);
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 非ASCII文字をエスケープしない
            };

            foreach (var template in manager.Templates)
            {
                var formattedLastUpdated = template.LastUpdated.ToString("yyyyMMddHHmmss");
                var filePath = Path.Combine(dataDirectory, $"PromptTemplate_{template.SortOrder}_{formattedLastUpdated}_{template.ID}.json");
                var jsonString = System.Text.Json.JsonSerializer.Serialize(template, options);

                File.WriteAllText(filePath, jsonString);
            }
        }
        public static PromptTemplateManager LoadPromptTemplateFromJson()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dataDirectory = Path.Combine(documentsPath, "OpenAIOnWPF", "PromptTemplate");

            var manager = new PromptTemplateManager();
            manager.Templates = new ObservableCollection<PromptTemplate>();

            Directory.CreateDirectory(dataDirectory);

            var files = Directory.GetFiles(dataDirectory, "PromptTemplate_*.json");

            foreach (var file in files)
            {
                var jsonString = File.ReadAllText(file);
                var templates = System.Text.Json.JsonSerializer.Deserialize<PromptTemplate>(jsonString);

                if (templates != null)
                {
                    manager.Templates.Add(templates);
                }
            }
            return manager;
        }
    }
}
