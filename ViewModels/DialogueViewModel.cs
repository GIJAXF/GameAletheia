using ReactiveUI;
using GameAletheiaCross.Models;
using System;
using System.Reactive;
using GameAletheiaCross.ViewModels;

namespace GameAletheiaCross.ViewModels
{
    public class DialogueViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly NPC _npc;
        private readonly GameViewModel _game;

        public DialogueViewModel(Action<ViewModelBase> navigate, NPC npc, GameViewModel game)
        {
            _navigate = navigate;
            _npc = npc;
            _game = game;

            ContinueCommand = ReactiveCommand.Create(OnContinue);
        }

        public string NpcName => _npc.Name;
        public string DialogueText => _npc.Dialogue;

        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

        private void OnContinue()
        {
            _game.ReturnFromSubView();
            _navigate(_game);
        }
    }
}
