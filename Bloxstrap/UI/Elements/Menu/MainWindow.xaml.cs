﻿using System.Windows.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Elements.Menu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public MainWindow(bool showAlreadyRunningWarning)
        {
            InitializeComponent();
            ApplyTheme();

            App.Logger.WriteLine("MainWindow::MainWindow", "Initializing menu");

            DataContext = new MainWindowViewModel(this);

#if DEBUG // easier access
            PreInstallNavItem.Visibility = System.Windows.Visibility.Hidden;
            EditorWarningNavItem.Visibility = System.Windows.Visibility.Hidden;
#endif

            if (showAlreadyRunningWarning)
                _ = ShowAlreadyRunningSnackbar();
        }

        private async Task ShowAlreadyRunningSnackbar()
        {
            await Task.Delay(500); // wait for everything to finish loading
            AlreadyRunningSnackbar.Show();
        }

        public void OpenWiki(object? sender, EventArgs e) => Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/wiki");

        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods
    }
}
