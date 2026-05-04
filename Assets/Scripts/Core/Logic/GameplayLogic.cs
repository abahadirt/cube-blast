using Blast.Core.Data;
using Blast.Core.Logic;
using System.Collections.Generic;

namespace Blast.Core.Logic
{
    public class GameplayLogic
    {
        private BoardLogic _boardLogic;
        private LaunchTrayLogic _launchTrayLogic;
        private ShooterReserveLogic _shooterReserveLogic;
        
        private TargetSelector _targetSelector;
       
        private FireCoordinator _fireCoordinator;

        public void InitializeGameplayLogic
            (BoardLogic boardLogic, 
            LaunchTrayLogic launchTraylogic, 
            ShooterReserveLogic shooterReserveLogic, 
            TargetSelector targetSelector,
            FireCoordinator fireCoordinator)
        {
            _boardLogic = boardLogic;
            _launchTrayLogic = launchTraylogic;
            _shooterReserveLogic = shooterReserveLogic;
            _targetSelector = targetSelector;
            _fireCoordinator = fireCoordinator;
        }


        public void SendShooterToLaunchTray(int columnIndex)
        {
            if (!_launchTrayLogic.HasSpace()) return;

            if (columnIndex == -1) return;

            ShooterLogic shooter = _shooterReserveLogic.GetNextShooter(columnIndex);

            if (shooter != null)
            {
                _launchTrayLogic.AddShooter(shooter);
            }
        }
        public void Tick(float deltaTime)
        {
            
            List<MergeResult> mergeResults = _launchTrayLogic.Tick(deltaTime);
            _fireCoordinator.Tick(deltaTime);
           
        }





    }
}


/* 
// Initialize BOARD LOGIC
            //board logic'e col ve row vermeye gerek yok.
            // gridrow da vermeye gerek yok direkt 2d array verebiliriz,
            // gridrow 2d array dönüþümü baþka yerde yapýlýr
            //
int tempcolumns = 7;
int temptotalRows = 6;
GridRow[] tempgridrows = new GridRow[temptotalRows];
_boardLogic = new BoardLogic(tempcolumns, temptotalRows, tempgridrows);

// Initialize LAUNCH TRAY LOGIC
int tempcapacity = 5;
_launchTrayLogic = new LaunchTrayLogic(tempcapacity);

// Initialize SHOOTER RESERVE LOGIC
List<ReserveColumnData> tempreserveColumns = new List<ReserveColumnData>();
_shooterReserveLogic = new ShooterReserveLogic(tempreserveColumns);

// Initialize FIRE COORDINATOR LOGIC
_targetSelector = new TargetSelector(_boardLogic);

// Initialize FIRE COORDINATOR LOGIC
_fireCoordinator = new FireCoordinator(_targetSelector, _launchTrayLogic);
*/