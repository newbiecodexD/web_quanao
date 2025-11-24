using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace web_quanao.Services
{
    public static class InMemoryAuthStore
    {
        // email(lower) -> plain password (DEV ONLY!)
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        public static bool AddUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _users.TryAdd(email.Trim().ToLowerInvariant(), password);
        }

        public static bool Validate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (_users.TryGetValue(email.Trim().ToLowerInvariant(), out var stored))
            {
                return stored == password; // DEV ONLY: no hashing
            }
            return false;
        }

        public static bool Exists(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _users.ContainsKey(email.Trim().ToLowerInvariant());
        }

        // Admin helpers
        public static IEnumerable<(string Email,string Password)> GetAll()
            => _users.Select(kv => (kv.Key, kv.Value)).OrderBy(x => x.Key);

        public static bool Remove(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _users.TryRemove(email.Trim().ToLowerInvariant(), out _);
        }

        public static bool UpdatePassword(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var key = email.Trim().ToLowerInvariant();
            if (!_users.ContainsKey(key)) return false;
            _users[key] = newPassword; // direct set (OK for demo)
            return true;
        }
    }
}
