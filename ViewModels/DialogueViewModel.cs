using ReactiveUI;
using GameAletheiaCross.Models;
using System;
using System.Reactive;
using System.Collections.Generic;

namespace GameAletheiaCross.ViewModels
{
    public class DialogueViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly NPC _npc;
        private readonly GameViewModel _game;

        private int _currentLineIndex = 0;

        public DialogueViewModel(Action<ViewModelBase> navigate, NPC npc, GameViewModel game)
        {
            _navigate = navigate;
            _npc = npc;
            _game = game;

            if (_npc.DialogueList.Count == 0)
                _npc.DialogueList.Add("…");

            ContinueCommand = ReactiveCommand.Create(OnContinue);
        }

        public string NpcName => _npc.Name;

        // Obtiene solo la línea actual del diálogo
        public string DialogueText => _npc.DialogueList[_currentLineIndex];

        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

        private void OnContinue()
        {
            // Si aún quedan líneas, avanza.
            if (_currentLineIndex < _npc.DialogueList.Count - 1)
            {
                _currentLineIndex++;
                this.RaisePropertyChanged(nameof(DialogueText));
                return;
            }

            // Si el diálogo terminó
            _game.ReturnFromSubView();
            _navigate(_game);
        }
    }
}
