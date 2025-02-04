// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Text.Json;
using Azure.Core;

namespace Azure.AI.Language.Conversations.Models
{
    public partial class DeepstackResult
    {
        internal static DeepstackResult DeserializeDeepstackResult(JsonElement element)
        {
            string query = default;
            Optional<string> detectedLanguage = default;
            DeepstackPrediction prediction = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("query"))
                {
                    query = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("detectedLanguage"))
                {
                    detectedLanguage = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("prediction"))
                {
                    prediction = DeepstackPrediction.DeserializeDeepstackPrediction(property.Value);
                    continue;
                }
            }
            return new DeepstackResult(query, detectedLanguage.Value, prediction);
        }
    }
}
