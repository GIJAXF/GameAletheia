using ReactiveUI;
using GameAletheiaCross.Models;
using System;
using System.Reactive;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameAletheiaCross.ViewModels
{
    public class DialogueViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly NPC _npc;
        private readonly GameViewModel _game;

        private int _currentLineIndex = 0;
        private int _timeRemaining = 5;
        private CancellationTokenSource? _timerCts;

        public DialogueViewModel(Action<ViewModelBase> navigate, NPC npc, GameViewModel game)
        {
            _navigate = navigate;
            _npc = npc;
            _game = game;

            if (_npc.DialogueList.Count == 0)
                _npc.DialogueList.Add("…");

            ContinueCommand = ReactiveCommand.Create(OnContinue);
            
            StartTimer();
        }

        public string NpcName => _npc.Name;

        public string DialogueText => _npc.DialogueList[_currentLineIndex];

        private int _timeRemainingValue = 5;
        public int TimeRemaining
        {
            get => _timeRemainingValue;
            set => this.RaiseAndSetIfChanged(ref _timeRemainingValue, value);
        }

        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

        private void StartTimer()
        {
            _timerCts?.Cancel();
            _timerCts = new CancellationTokenSource();
            
            TimeRemaining = 5;
            
            Task.Run(async () =>
            {
                for (int i = 5; i >= 0; i--)
                {
                    if (_timerCts.Token.IsCancellationRequested)
                        return;
                    
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        TimeRemaining = i;
                    });
                    
                    if (i == 0)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            OnContinue();
                        });
                        return;
                    }
                    
                    await Task.Delay(1000, _timerCts.Token);
                }
            }, _timerCts.Token);
        }

        private void OnContinue()
        {
            _timerCts?.Cancel();
            
            // Si aún quedan líneas, avanza.
            if (_currentLineIndex < _npc.DialogueList.Count - 1)
            {
                _currentLineIndex++;
                this.RaisePropertyChanged(nameof(DialogueText));
                StartTimer();
                return;
            }

            // Si el diálogo terminó, cerrar
            _game.CloseDialogue();
        }
    }
}