
namespace Blast.Logging
{
    public interface ILog
    {
        void Info(string tag, string message);
        void Warn(string tag, string message);
        void Error(string tag, string message);
    }
}