﻿// Author: Dominic Beger (Trade/ProgTrade) 2017

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using nUpdate.Administration.Infrastructure;
using WPFFolderBrowser;

namespace nUpdate.Administration.ViewModels.NewProject
{
    [Description("GeneralDataPageViewModel")]
    public class GeneralDataPageViewModel : PageViewModel
    {
        private readonly NewProjectViewModel _newProjectViewModel;
        private string _location;
        private ICommand _locationSelectCommand;
        private string _name;
        private string _updateDirectory;
        private ICommand _updateDirectorySelectCommand;

        public GeneralDataPageViewModel(NewProjectViewModel viewModel)
        {
            _newProjectViewModel = viewModel;
            CanGoBack = true;

            _location = PathProvider.DefaultProjectDirectory;
            _locationSelectCommand = new RelayCommand(() =>
            {
                var browseDialog = new WPFFolderBrowserDialog
                {
                    Title = "Select the project location..."
                };

                var result = browseDialog.ShowDialog();
                if (result.HasValue && result.Value)
                    Location = browseDialog.FileName;
            });

            _updateDirectorySelectCommand = new RelayCommand(() =>
            {
                var browseDialog = new WPFFolderBrowserDialog
                {
                    Title = "Select the local update directory..."
                };

                var result = browseDialog.ShowDialog();
                if (result.HasValue && result.Value)
                    UpdateDirectory = browseDialog.FileName;
            });
        }

        public string Location
        {
            get => _location;
            set
            {
                SetProperty(value, ref _location, nameof(Location));
                RefreshNavigation();
            }
        }

        public ICommand LocationSelectCommand
        {
            get => _locationSelectCommand;
            set => SetProperty(value, ref _locationSelectCommand, nameof(LocationSelectCommand));
        }

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(value, ref _name, nameof(Name));
                RefreshNavigation();
            }
        }

        public string UpdateDirectory
        {
            get => _updateDirectory;
            set
            {
                SetProperty(value, ref _updateDirectory, nameof(UpdateDirectory));
                RefreshNavigation();
            }
        }

        public ICommand UpdateDirectorySelectCommand
        {
            get => _updateDirectorySelectCommand;
            set => SetProperty(value, ref _updateDirectorySelectCommand, nameof(UpdateDirectorySelectCommand));
        }

        public override void OnNavigated(PageViewModel fromPage, PagedWindowViewModel window)
        {
            if (!Directory.Exists(PathProvider.DefaultProjectDirectory))
                Directory.CreateDirectory(PathProvider.DefaultProjectDirectory);

            var nameAvailable = false;
            var targetDirectory = default(DirectoryInfo);
            int i = 0;
            while (!nameAvailable)
            {
                targetDirectory = new DirectoryInfo(Path.Combine(PathProvider.DefaultProjectDirectory,
                    $"NewProject{(i > 0 ? i.ToString() : string.Empty)}"));
                if (!(nameAvailable = !targetDirectory.Exists))
                    ++i;
            }

            Name = targetDirectory.Name;
        }

        private void RefreshNavigation()
        {
            CanGoForward = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Location) &&
                           Uri.TryCreate(UpdateDirectory, UriKind.Absolute, out Uri _);

            // If the data is okay, we will set it directly.
            if (CanGoForward)
                RefreshProjectData();
        }

        private void RefreshProjectData()
        {
            _newProjectViewModel.ProjectCreationData.Project.Name = Name;
            _newProjectViewModel.ProjectCreationData.Project.UpdateDirectory = new Uri(UpdateDirectory);
            _newProjectViewModel.ProjectCreationData.Location = Location;
        }
    }
}