using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Quiclose {
    internal static class Persistence {

        private static Dictionary<string, string>? _values = null;
        private static string _persistenceFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(Quiclose),
            "persistence.json");

        public static bool TryLoadValue<T> (string key, [NotNullWhen(true)] out T? value) {
            EnsureValuesLoaded();
            if (!_values.TryGetValue(key, out var text)) {
                value = default;
                return false;
            }
            try {
                value = JsonSerializer.Deserialize<T>(text);
            } catch (JsonException) {
                // thrown when the types don't match
                value = default;
                return false;
            }
            return value is not null && !EqualityComparer<T>.Default.Equals(value, default);
        }

        public static void SetValue (string key, object value) {
            EnsureValuesLoaded();
            _values[key] = JsonSerializer.Serialize(value);
        }

        public static void SaveValues () {
            EnsureValuesLoaded();
            var dir = Path.GetDirectoryName(_persistenceFile) ?? throw new ApplicationException("Failed to get AppData directory?");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_persistenceFile, JsonSerializer.Serialize(_values));
        }

        [MemberNotNull(nameof(_values))]
        private static void EnsureValuesLoaded () {
            if (_values is not null) return;
            if (!File.Exists(_persistenceFile)) {
                _values = [];
                return;
            }
            _values = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_persistenceFile)) ?? [];
        }

    }
}
