﻿using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Linq;
using System.Windows;
using CodeDek.Lib;
using CodeDek.Lib.Mvvm;

namespace FolderMapper.ViewModels
{
    public sealed class UnMapViewModel : ObservableObject
    {
        private int _selectedIndex;
        private int _selectedMappedPathIndex = -1;
        private readonly MainViewModel _mainViewModel;

        public UnMapViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value)
                .Alert(nameof(GetMappedPathsCmd))
                .Alert(nameof(UnMapCmd))
                .Alert(nameof(ResetCmd));
        }

        public int SelectedMappedPathIndex
        {
            get => _selectedMappedPathIndex;
            set => Set(ref _selectedMappedPathIndex, value)
                .Alert(nameof(UnMapCmd))
                .Alert(nameof(ResetCmd))
                .Alert(nameof(ClearSelectionCmd));
        }

        public ObservableCollection<string> MappedPaths { get; } = new ObservableCollection<string>();

        public Cmd GetMappedPathsCmd => new Cmd(() =>
        {
            MappedPaths.Clear();

            var letter = char.Parse(MainViewModel.DriveLetters[SelectedIndex]);

            if (letter != default)
            {
                foreach (var item in Storage.GetDriveMappedDirectories(letter))
                {
                    MappedPaths.Add(item);
                }
            }
        }, () => SelectedIndex > 0);

        public Cmd UnMapCmd => new Cmd(() =>
        {
            var letter = char.Parse(MainViewModel.DriveLetters[SelectedIndex]);
            var path = MappedPaths[SelectedMappedPathIndex];
            var isMapped = Storage.GetDriveMappedDirectories(letter).Any();

            if (letter != default && isMapped)
            {
                var result = MessageBox.Show("Proceed with unmapping the drive?", "Confirmation 😕", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes && Storage.UnmapDrive(letter, MappedPaths[SelectedMappedPathIndex]))
                {
                    _mainViewModel.Status = $"Drive ({letter}) was unmapped from ({path.TrimStart(@"\??\".ToCharArray())}). 😎";
                    _mainViewModel.StatusColor = Brushes.Green;
                    _mainViewModel.Passage = "";
                    _mainViewModel.PassageUrl = "";

                    GetMappedPathsCmd.Execute();
                    SelectedIndex = 0;
                }
                else
                {
                    _mainViewModel.StatusColor = Brushes.Black;
                }
            }
        }, () => SelectedIndex > 0 && SelectedMappedPathIndex > -1);

        public Cmd ResetCmd => new Cmd(() =>
        {
            SelectedIndex = 0;
            MappedPaths.Clear();
            SelectedMappedPathIndex = -1;
        }, () => SelectedIndex > 0 && MappedPaths.Count > 0);

        public Cmd ClearSelectionCmd => new Cmd(() => SelectedMappedPathIndex = -1, () => SelectedMappedPathIndex > -1);
    }
}
