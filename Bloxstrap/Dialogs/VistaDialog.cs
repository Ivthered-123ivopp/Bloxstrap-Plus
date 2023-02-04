﻿using System;
using System.Windows.Forms;

using Bloxstrap.Enums;

namespace Bloxstrap.Dialogs
{
    // https://youtu.be/h0_AL95Sc3o?t=48

    // a bit hacky, but this is actually a hidden form
    // since taskdialog is part of winforms, it can't really be properly used without a form
    // for example, cross-threaded calls to ui controls can't really be done outside of a form

    public partial class VistaDialog : BootstrapperDialogForm
    {
        private TaskDialogPage Dialog;

        protected override string _message
        {
            get => Dialog.Heading ?? "";
            set => Dialog.Heading = value;
        }

        protected override ProgressBarStyle _progressStyle
        {
            set
            {
                if (Dialog.ProgressBar is null)
                    return;

                switch (value)
                {
                    case ProgressBarStyle.Continuous:
                    case ProgressBarStyle.Blocks:
                        Dialog.ProgressBar.State = TaskDialogProgressBarState.Normal;
                        break;

                    case ProgressBarStyle.Marquee:
                        Dialog.ProgressBar.State = TaskDialogProgressBarState.Marquee;
                        break;
                }
            }
        }

        protected override int _progressValue
        {
            get => Dialog.ProgressBar is null ? 0 : Dialog.ProgressBar.Value;
            set
            {
                if (Dialog.ProgressBar is null)
                    return;

                Dialog.ProgressBar.Value = value;
            }
        }

        protected override bool _cancelEnabled
        {
            get => Dialog.Buttons[0].Enabled;
            set => Dialog.Buttons[0].Enabled = value;
        }

        public VistaDialog(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            Bootstrapper = bootstrapper;

            Dialog = new TaskDialogPage()
            {
                Icon = new TaskDialogIcon(App.Settings.Prop.BootstrapperIcon.GetIcon()),
                Caption = App.ProjectName,

                Buttons = { TaskDialogButton.Cancel },
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                }
            };

            _message = "Please wait...";
            _cancelEnabled = false;

            Dialog.Buttons[0].Click += (sender, e) => ButtonCancel_Click(sender, e);

            SetupDialog();
        }

        public override void ShowSuccess(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(ShowSuccess, message);
            }
            else
            {
                TaskDialogPage successDialog = new()
                {
                    Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                    Caption = App.ProjectName,
                    Heading = message,
                    Buttons = { TaskDialogButton.OK }
                };

                successDialog.Buttons[0].Click += (sender, e) => App.Terminate();

                if (!App.IsQuiet)
                    Dialog.Navigate(successDialog);

                Dialog = successDialog;
            }
        }

        public override void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(ShowError, message);
            }
            else
            {
                TaskDialogPage errorDialog = new()
                {
                    Icon = TaskDialogIcon.Error,
                    Caption = App.ProjectName,
                    Heading = "An error occurred while starting Roblox",
                    Buttons = { TaskDialogButton.Close },
                    Expander = new TaskDialogExpander()
                    {
                        Text = message,
                        CollapsedButtonText = "See details",
                        ExpandedButtonText = "Hide details",
                        Position = TaskDialogExpanderPosition.AfterText
                    }
                };

                errorDialog.Buttons[0].Click += (sender, e) => App.Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);

                if (!App.IsQuiet)
                    Dialog.Navigate(errorDialog);

                Dialog = errorDialog;
            }
        }

        public override void CloseDialog()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(CloseDialog);
            }
            else
            {
                if (Dialog.BoundDialog is null)
                    return;
                
                Dialog.BoundDialog.Close();
            }
        }


        private void VistaDialog_Load(object sender, EventArgs e)
        {
            if (!App.IsQuiet)
                TaskDialog.ShowDialog(Dialog);
        }
    }
}
