namespace Blast.Logging
{
    public static class Log
    {
        private static ILog _impl;

        public static void Configure(ILog impl) => _impl = impl;

        public static void Info(string tag, string m) => _impl?.Info(tag, m);

        public static void Warn(string tag, string m) => _impl?.Warn(tag, m);
        public static void Error(string tag, string m) => _impl?.Error(tag, m);
    }
}