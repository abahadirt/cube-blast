using Blast.Core.Data;
using Blast.Core.Event;
using Blast.Core.Logic;
using Blast.GamePresentation.Presenter;
using Blast.GameUnity.Input;
using Blast.GameUnity.View;
using Blast.GameUnity.Registry;

using Blast.Test;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Blast.GameUnity.Boot
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Level Setup")]
        [SerializeField] private TextAsset _levelJsonFile;



        [Header("Views")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ShooterReserveView _reserveView;
        [SerializeField] private LaunchTrayView _trayView;


        [SerializeField] private ProjectileService _projectileService;





        [SerializeField] private InputHandler _inputHandler;

        private GamePresenter _gameplayPresenter;

        private void Awake()
        {
            ShooterViewRegistry registry = new ShooterViewRegistry();
            _reserveView.Construct(registry);
            _trayView.Construct(registry);




            LevelData levelData = ParseLevelData(_levelJsonFile.text);

            // --- Level data (geçici; sonra ScriptableObject'ten gelecek) ---
            var rows = levelData.rows;
            var totalRows = levelData.totalRows;
            var columns = levelData.columns;

            var reserveColumns = levelData.reserveColumns;
            var launchTrayCapacity = levelData.launchTrayCapacity;
            var visibleRows = levelData.visibleRows;


            // --- Event ---
            var eventQueue = new GameEventQueue();


            // --- Logic katmaný ---
            var boardLogic = new BoardLogic(columns, totalRows, rows);
            var trayLogic = new LaunchTrayLogic(launchTrayCapacity, eventQueue);
            var reserveLogic = new ShooterReserveLogic(reserveColumns);
            var targetSelector = new TargetSelector(boardLogic);
            var fireCoord = new FireCoordinator(targetSelector, trayLogic, eventQueue);
            var gameplayLogic = new GameplayLogic(boardLogic, trayLogic, reserveLogic, targetSelector, fireCoord, eventQueue);


            // --- Presenter katmaný ---
            var boardPresenter = new BoardPresenter(boardLogic, _boardView, visibleRows);
            var reservePresenter = new ShooterReservePresenter(reserveLogic, _reserveView);
            var launchTrayPresenter = new LaunchTrayPresenter(trayLogic, _trayView);
           
            _gameplayPresenter = new GamePresenter(gameplayLogic, boardPresenter, reservePresenter, launchTrayPresenter, eventQueue, _projectileService);
            _inputHandler.OnColumnTapped += _gameplayPresenter.TrySendShooter;
            // --- Baþlat ---
            _gameplayPresenter.Initialize();
        }

        private void Update()
        {
            _gameplayPresenter.Tick(Time.deltaTime);
        }



        public LevelData ParseLevelData(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                throw new Exception("JSON metni boþ olamaz!");
            }

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());

            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(jsonString, settings);

            Array.Reverse(levelData.rows);
            return levelData;
        }




        public LevelData GetLevelDataFromPath(string filePath)
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