using UnityEngine;
using Blast.Logging;
namespace Blast.GameUnity.Logging
{
    public sealed class UnityLogger : ILog
    {
        public void Info(string tag, string message) => Debug.Log($"[{tag}] {message}");
        public void Warn(string tag, string message) => Debug.LogWarning($"[{tag}] {message}");
        public void Error(string tag, string message) => Debug.LogError($"[{tag}] {message}");
    }
}