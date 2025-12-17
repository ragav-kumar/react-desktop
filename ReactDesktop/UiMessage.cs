using System.Text.Json;

namespace ReactDesktop;

public record UiMessage(string Type, Guid RequestId, JsonElement? Payload);
