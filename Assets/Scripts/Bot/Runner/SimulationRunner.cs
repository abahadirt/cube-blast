using Blast.Bot.Observation;
using Blast.Bot.Policy;
using Blast.Core.Config;
using Blast.Core.Event;
using Blast.Core.Logic;
using Blast.Level;
using System;

namespace Blast.Bot.Runner
{
    /// <summary>
    /// Runs a level in a fixed-step, headless simulation and returns the final result.
    /// </summary>
    public sealed class SimulationRunner
    {
        public RunResult Run(LevelData levelData, SimConfig cfg, CoreConfig coreConfig, IBotPolicy policy, int seed, string levelId, IRunObserver observer = null)
        {
            var game = new HeadlessGame(levelData, coreConfig);
            var rng = new Random(seed);
            var observation = new GameObservation(game, cfg.Mode, game.VisibleRows);
            var fingerprint = RunFingerprint.Create();
            int initialCubes = CountCubes(game.Board);

            var result = new RunResult
            {
                LevelId = levelId,
                Policy = policy.Name,
                Seed = seed,
                Outcome = RunOutcome.Timeout,
            };

            int tick = 0;
            bool resolved = false;

            for (; tick < cfg.MaxTicks && !resolved; tick++)
            {
                if (tick % cfg.DecisionEveryNTicks == 0)
                {
                    int? col = policy.Decide(observation, rng);
                    observer?.OnDecision(tick, col);
                    if (col.HasValue)
                    {
                        result.TotalTaps++;
                        game.Gameplay.SendShooterToLaunchTray(col.Value);
                    }
                }

                game.Gameplay.Tick(cfg.Dt);

                while (game.Queue.TryDequeue(out IGameEvent ev))
                {
                    fingerprint.MixEvent(tick, ev);
                    observer?.OnEvent(tick, ev);
                    switch (ev)
                    {
                        case ShooterSentEvent _: result.TotalSends++; break;
                        case ShooterFiredEvent _: result.TotalShots++; break;
                        case ShootersMergedEvent _: result.TotalMerges++; break;
                        case LevelCompletedEvent _: result.Outcome = RunOutcome.Win; resolved = true; break;
                        case LevelFailedEvent _: result.Outcome = RunOutcome.Lose; resolved = true; break;
                    }
                }
            }

            result.Ticks = tick;
            result.SimTime = tick * cfg.Dt;
            result.TrayFullStalls = result.TotalTaps - result.TotalSends;
            result.FinalCubesRemaining = CountCubes(game.Board);
            result.TotalCubesCleared = initialCubes - result.FinalCubesRemaining;
            result.ReserveExhausted = game.Reserve.IsEmpty();
            result.Fingerprint = fingerprint.ToHex();
            observer?.OnRunEnd(result);
            return result;
        }


        private static int CountCubes(BoardLogic board)
        {
            int total = 0;
            for (int col = 0; col < board.Columns; col++)
                total += board.GetColumnHeight(col);
            return total;
        }
    }
}
