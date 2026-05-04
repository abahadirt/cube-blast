using Blast.Core.Logic;
using System.Collections.Generic;

public class FireCoordinator
{

    private readonly TargetSelector _targetSelector;

    //private List<ShooterLogic> _shooters = new List<ShooterLogic>();

    private LaunchTrayLogic _launchTrayLogic;
    public FireCoordinator(TargetSelector targetSelector, LaunchTrayLogic launchTrayLogic)
    {
        _targetSelector = targetSelector;
        _launchTrayLogic = launchTrayLogic;
    }



    public void Tick(float deltaTime)
    {
        foreach (LaunchTraySlotLogic slot in _launchTrayLogic.slotLogics)
        {
            var shooter = slot.ShooterLogic;
            if (shooter == null) continue;

            shooter.Tick(deltaTime); // cooldown'u kendi içinde ilerletir

            if (!shooter.IsActive || shooter.IsDepleted) continue;

            if (!shooter.CanFire) continue;

            var targetResult = _targetSelector.FindTarget(shooter.Color);
            if (!targetResult.HasTarget) continue;

            // BI YOLU BULUNUR... 
            // adapterde columndan pos buluruz ball fýrlatýrýz

            int targetColumn = targetResult.Column;
            shooter.Fire();
        }

        /*
        for (int i = 0; i < _shooters.Count; i++)
        {
            var shooter = _shooters[i];
            shooter.Tick(deltaTime); // cooldown'u kendi içinde ilerletir

            if (!shooter.IsActive || shooter.IsDepleted) continue;
            if (!shooter.CanFire) continue;

            var targetResult = _targetSelector.FindTarget(shooter.Color);
            if (!targetResult.HasTarget) continue;

            // BI YOLU BULUNUR... 
            // adapterde columndan pos buluruz ball fýrlatýrýz
            int targetColumn = targetResult.Column;
            shooter.TryFire();
        }
        */

    }





}








/*

using Blast.Core.Logic;
using System.Collections.Generic;

public class FireCoordinator
{

    private readonly TargetSelector _targetSelector;

    private List<ShooterLogic> _shooters = new List<ShooterLogic>();

    private LaunchTrayLogic _launchTrayLogic;
    public FireCoordinator(TargetSelector targetSelector)
    {
        _targetSelector = targetSelector;
    }

 

    public void Tick(float deltaTime)
    {
        //_launhTrayLogic.Slots.
        for (int i = 0; i < _shooters.Count; i++)
        {
            var shooter = _shooters[i];
            shooter.Tick(deltaTime); // cooldown'u kendi içinde ilerletir

            if (!shooter.IsActive || shooter.IsDepleted) continue;
            if (!shooter.CanFire) continue;

            var targetResult = _targetSelector.FindTarget(shooter.Color);
            if (!targetResult.HasTarget) continue;

            // BI YOLU BULUNUR... 
            // adapterde columndan pos buluruz ball fýrlatýrýz
            int targetColumn = targetResult.Column;
            shooter.TryFire();
        }
    }
}


*/