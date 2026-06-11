using Blast.Core.Event;

namespace Blast.Bot.Runner
{
    /// <summary>
    /// Builds a compact deterministic hash from a run's event stream.
    /// </summary>
    public struct RunFingerprint
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        private ulong _h;

        public static RunFingerprint Create() => new RunFingerprint { _h = Offset };

        public void Mix(int value)
        {
            unchecked
            {
                _h ^= (uint)value;
                _h *= Prime;
            }
        }

        public void MixEvent(int tick, IGameEvent gameEvent)
        {
            Mix(tick);
            switch (gameEvent)
            {
                case ShooterSentEvent e:
                    Mix(1); Mix(e.ShooterId); Mix(e.TargetSlotIndex); Mix(e.SourceColumnIndex);
                    break;
                case ShootersMergedEvent e:
                    Mix(2); Mix(e.SurvivorShooterId); Mix(e.ConsumedShooterId1); Mix(e.ConsumedShooterId2); Mix(e.TotalAmmo);
                    break;
                case ShooterFiredEvent e:
                    Mix(3); Mix(e.ShooterId); Mix(e.SlotIndex); Mix((int)e.Color);
                    Mix(e.TargetColumn); Mix(e.TargetLogicalRow); Mix(e.RemainingAmmo);
                    break;
                case LevelCompletedEvent _:
                    Mix(4);
                    break;
                case LevelFailedEvent _:
                    Mix(5);
                    break;
            }
        }

        public string ToHex() => _h.ToString("x16");
    }
}
