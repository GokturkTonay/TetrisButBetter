using Tomino.Model;
using Tomino.Input; 
using UnityEngine;
using System.Collections;

namespace Tomino
{
    public class Game
    {
        public delegate void GameEventHandler();
        public event GameEventHandler FinishedEvent = delegate { };
        public event GameEventHandler PieceMovedEvent = delegate { };
        public event GameEventHandler PieceRotatedEvent = delegate { };
        public event GameEventHandler PieceFinishedFallingEvent = delegate { };

        public Score Score { get; private set; }
        public Level Level { get; private set; }
        private readonly Board _board;
        private readonly IPlayerInput _input;
        private float _elapsedTime;
        private bool _isPlaying;
        private bool _isExploding = false; 

        public Game(Board b, IPlayerInput i) { _board = b; _input = i; PieceFinishedFallingEvent += i.Cancel; }

        public void Start() { _isPlaying = true; Score = new Score(); Level = new Level(); _board.RemoveAllBlocks(); AddPiece(); }
        public void Pause() => _isPlaying = false;
        public void Resume() => _isPlaying = true;
       public void SetNextAction(PlayerAction a) 
{ 
    if (_input is Tomino.Input.TouchInput ti) 
    {
        ti.SetNextAction(a); 
    }
}

        public void AddPiece() { _board.AddPiece(); if (_board.HasCollisions()) { _isPlaying = false; FinishedEvent(); } }

        public void Update(float dt)
        {
            if (!_isPlaying || _isExploding) return;
            _input.Update();
            var action = _input.GetPlayerAction();
            if (action.HasValue) HandlePlayerAction(action.Value);
            else HandleAutomaticFalling(dt);
        }

        private void HandleAutomaticFalling(float dt)
        {
            _elapsedTime += dt;
            if (_elapsedTime < Level.FallDelay) return;
            if (!_board.MovePieceDown()) PieceFinishedFalling();
            _elapsedTime = 0;
        }

        private void HandlePlayerAction(PlayerAction a)
        {
            bool moved = false;
            if (a == PlayerAction.MoveLeft) moved = _board.MovePieceLeft();
            else if (a == PlayerAction.MoveRight) moved = _board.MovePieceRight();
            else if (a == PlayerAction.MoveDown) { if (_board.MovePieceDown()) moved = true; else PieceFinishedFalling(); _elapsedTime = 0; }
            else if (a == PlayerAction.Rotate) { if (_board.RotatePiece()) PieceRotatedEvent(); }
            else if (a == PlayerAction.Fall) { _board.FallPiece(); PieceFinishedFalling(); _elapsedTime = 0; }
            if (moved) PieceMovedEvent();
        }

        private void PieceFinishedFalling()
        {
            PieceFinishedFallingEvent();
            if (_board.Piece != null && _board.Piece.IsBomb) HandleBombExplosion();
            else HandleNormalRowClear();
        }

        private void HandleBombExplosion()
        {
            if (_isExploding) return;
            _isExploding = true;
            this.Pause();
            var (count, _) = _board.ExplodeContactBomb(_board.Piece);
            _board.Piece = null; 
            var mm = Object.FindFirstObjectByType<MenuManager>();
            if (mm != null) mm.StartCoroutine(SafeBombSequence(count));
        }

        private IEnumerator SafeBombSequence(int c) {
            var mm = Object.FindFirstObjectByType<MenuManager>();
            if (mm != null) yield return mm.StartCoroutine(mm.CalculateMultiplierSequence(this, c));
            _isExploding = false;
            if (_isPlaying) AddPiece();
        }

        private void HandleNormalRowClear()
        {
            var (rows, _) = _board.RemoveFullRows();
            if (rows > 0) {
                this.Pause();
                var mm = Object.FindFirstObjectByType<MenuManager>();
                if (mm != null) mm.StartCoroutine(SafeRowClearSequence(rows));
                Level.RowsCleared(rows);
            } else if (_isPlaying) AddPiece();
        }

        private IEnumerator SafeRowClearSequence(int rows)
        {
            var mm = Object.FindFirstObjectByType<MenuManager>();
            if (mm != null) yield return mm.StartCoroutine(mm.CalculateMultiplierSequence(this, rows));
            if (_isPlaying) AddPiece();
        }
    }
}