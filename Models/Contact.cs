using System.Text.Json.Serialization;

namespace AnyDeskRehber.WinUI3.Models;

public sealed class Contact
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Group { get; set; } = "";
    public string Note { get; set; } = "";
    public bool Favorite { get; set; }
    // Manual status is used until a licensed AnyDesk management API is connected.
    // Values: "unknown", "online", "offline".
    public string Status { get; set; } = "unknown";

    [JsonIgnore]
    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][0].ToString().ToUpperInvariant();
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
        }
    }
}
