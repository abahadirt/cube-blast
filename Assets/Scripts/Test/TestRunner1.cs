using Blast.Core.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters; // Enum çevirici için gerekli
using System;
using System.IO;


namespace Blast.Test
{
    public class TestRunner1
    {
        public GameplayLogic gamePlayLogic;

        public void Test()
        {
            LevelData levelData = GetLevelData("Assets/Scripts/Test/level1.json");

            gamePlayLogic = new GameplayLogic();

            BoardLogic boardLogic = new BoardLogic(levelData.columns, levelData.totalRows, levelData.rows);
            LaunchTrayLogic launchTrayLogic = new LaunchTrayLogic(levelData.launchTrayCapacity);
            ShooterReserveLogic shooterReserveLogic = new ShooterReserveLogic(levelData.reserveColumns);

            TargetSelector targetSelector = new TargetSelector(boardLogic);
            FireCoordinator fireCoordinator = new FireCoordinator(targetSelector, launchTrayLogic);

            gamePlayLogic.InitializeGameplayLogic(boardLogic, launchTrayLogic, shooterReserveLogic, targetSelector, fireCoordinator);

        }



        public LevelData GetLevelData(string filePath)
        {
            // 1. Dosyanýn var olup olmadýðýný kontrol ediyoruz
            if (!File.Exists(filePath))
            {
                // Dosya yoksa duruma göre null dönebilir veya bir hata (exception) fýrlatabilirsiniz.
                throw new FileNotFoundException($"JSON dosyasý bulunamadý. Lütfen yolu kontrol edin: {filePath}");
            }

            // 2. Dosyadaki tüm metni bir string olarak okuyoruz
            string jsonString = File.ReadAllText(filePath);

            // 3. Enum'larý ("Red", "Blue") string olarak okuyabilmesi için Newtonsoft ayarý
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());

            // 4. JSON'ý C# objesine çeviriyoruz (Newtonsoft yöntemi)
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(jsonString, settings);

            Array.Reverse(levelData.rows); // jsondaki görüntüyle logicte görüntü ayný olsun diye rowslarý ters çeviriyoruz
            return levelData;
        }
    }
}
