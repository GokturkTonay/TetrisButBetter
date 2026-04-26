using Tomino.Audio;
using Tomino.Input;
using Tomino.Model;
using Tomino.View;
using UnityEngine;

namespace Tomino
{
    public class GameController : MonoBehaviour
    {
        public GameConfig gameConfig;
        public AlertView alertView;
        public SettingsView settingsView;
        public AudioPlayer audioPlayer;
        public GameObject screenButtons;
        public AudioSource musicAudioSource;

        private Game _game;
        private UniversalInput _universalInput;

        internal void Awake()
        {
            Application.targetFrameRate = 60;
            HandlePlayerSettings();
            Settings.changedEvent += HandlePlayerSettings;
        }

        internal void Start()
        {
            // Kontrol: GameConfig ve tüm referanslar var mı?
            if (gameConfig == null)
            {
                Debug.LogError("GameController.Start: gameConfig NULL!");
                return;
            }

            if (gameConfig.boardView == null)
            {
                Debug.LogError("GameController.Start: boardView NULL!");
                return;
            }

            if (gameConfig.nextPieceView == null)
            {
                Debug.LogError("GameController.Start: nextPieceView NULL!");
                return;
            }

            if (gameConfig.scoreView == null)
            {
                Debug.LogError("GameController.Start: scoreView NULL!");
                return;
            }

            if (gameConfig.levelView == null)
            {
                Debug.LogError("GameController.Start: levelView NULL!");
                return;
            }

            if (audioPlayer == null)
            {
                Debug.LogError("GameController.Start: audioPlayer NULL!");
                return;
            }

            // Board oluştur
            Board board = new(10, 20);
            Debug.Log("GameController: Board oluşturuldu (10×20)");

            // Deck referanslarını set et (sadece DeckCardsManager)
            // DeckUIView deaktif - oyun normal çalışsın
            gameConfig.boardView.deckCardsManager = gameConfig.deckCardsManager;
            
            // Board'u View'lara set et
            gameConfig.boardView.SetBoard(board, gameConfig.pieceProvider);
            gameConfig.nextPieceView.SetBoard(board);
            
            // DeckCardsManager'ı initialize et
            if (gameConfig.deckCardsManager != null)
            {
                gameConfig.deckCardsManager.Initialize(board);
                Debug.Log("GameController: DeckCardsManager initialized");
            }
            
            // Input sistemi kur
            if (gameConfig.boardView.touchInput == null)
            {
                Debug.LogError("GameController.Start: touchInput NULL!");
                return;
            }

            _universalInput = new UniversalInput(new KeyboardInput(), gameConfig.boardView.touchInput);
            Debug.Log("GameController: UniversalInput oluşturuldu");

            // Game oluştur
            _game = new Game(board, _universalInput);
            Debug.Log("GameController: Game oluşturuldu");

            // View'lara referansları bağla
            gameConfig.levelView.game = _game;
            gameConfig.levelView.board = board;
            gameConfig.scoreView.game = _game;
            gameConfig.scoreView.board = board;
            Debug.Log("GameController: View referansları bağlandı");

            // Event'leri bağla
            _game.FinishedEvent += OnGameFinished;
            _game.PieceFinishedFallingEvent += audioPlayer.PlayPieceDropClip;
            _game.PieceRotatedEvent += audioPlayer.PlayPieceRotateClip;
            _game.PieceMovedEvent += audioPlayer.PlayPieceMoveClip;
            Debug.Log("GameController: Event'ler bağlandı");

            // Oyunu başlat
            _game.Start();
            Debug.Log("GameController: Oyun başlatıldı!");
        }

        public void OnPauseButtonTap()
        {
            _game.Pause();
            ShowPauseView();
        }

        public void OnMoveLeftButtonTap() => _game.SetNextAction(PlayerAction.MoveLeft);
        public void OnMoveRightButtonTap() => _game.SetNextAction(PlayerAction.MoveRight);
        public void OnMoveDownButtonTap() => _game.SetNextAction(PlayerAction.MoveDown);
        public void OnRotateButtonTap() => _game.SetNextAction(PlayerAction.Rotate);

        private void OnGameFinished()
        {
            alertView.SetTitle(TextID.GameFinished);
            alertView.AddButton(TextID.PlayAgain, _game.Start, audioPlayer.PlayNewGameClip);
            alertView.Show();
        }

        internal void Update() 
        {
            if (_game != null)
                _game.Update(Time.deltaTime);
        }

        private void ShowPauseView()
        {
            alertView.SetTitle(TextID.GamePaused);
            alertView.AddButton(TextID.Resume, _game.Resume, audioPlayer.PlayResumeClip);
            alertView.AddButton(TextID.NewGame, _game.Start, audioPlayer.PlayNewGameClip);
            alertView.AddButton(TextID.Settings, ShowSettingsView, audioPlayer.PlayResumeClip);
            alertView.Show();
        }

        private void ShowSettingsView() => settingsView.Show(ShowPauseView);

        private void HandlePlayerSettings()
        {
            screenButtons.SetActive(Settings.ScreenButtonsEnabled);
            gameConfig.boardView.touchInput.Enabled = !Settings.ScreenButtonsEnabled;
            musicAudioSource.gameObject.SetActive(Settings.MusicEnabled);
        }
    }
}