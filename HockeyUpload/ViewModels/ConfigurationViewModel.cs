﻿using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using HockeyApp.AppLoader.Extensions;
using Microsoft.Win32;

namespace HockeyApp.AppLoader.ViewModels
{

    public class ConfigurationViewModel:ViewModelBase
    {
        private Configuration _configuration = null;

        public ConfigurationViewModel()
        {
            this._configuration = IoC.Get<Configuration>();
            this.Configurations = new ObservableCollection<UserConfigurationViewModel>();
            foreach (UserConfiguration uc in this._configuration.UserConfigurations)
            {
                this.Configurations.Add(new UserConfigurationViewModel(uc));
            }

            if (this.Configurations.Count > 0)
            {
                this.SelectedUserConfiguration = this.Configurations.First();
            }
        }


        public ObservableCollection<UserConfigurationViewModel> Configurations { get; private set; }
        private UserConfigurationViewModel _selectedUserConfiguration;
        public UserConfigurationViewModel SelectedUserConfiguration
        {
            get { 
                return this._selectedUserConfiguration; 
            }
            set
            {
                if (this._selectedUserConfiguration != null && this._selectedUserConfiguration.WasChanged)
                {

                    if (IoC.Get<IWindowManager>()
                        .ShowMetroMessageBox("Configuration was changed! Continue without saving?", ""
                        , System.Windows.MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.OK)
                    {
                        this._selectedUserConfiguration.Cancel();
                        if (this._selectedUserConfiguration.IsNew)
                        {
                            this.Configurations.Remove(this._selectedUserConfiguration);
                        }
                        this._selectedUserConfiguration = value;
                    }
                }
                else if (this._selectedUserConfiguration != null && !this._selectedUserConfiguration.WasChanged && this._selectedUserConfiguration.IsNew)
                {
                    this.Configurations.Remove(this._selectedUserConfiguration);
                    this._selectedUserConfiguration = value;
                }
                else
                {
                    this._selectedUserConfiguration = value;
                }
                NotifyOfPropertyChange(() => this.SelectedUserConfiguration);
            }
        }

        public string PathToAAPT
        {
            get { return HockeyApp.AppLoader.Properties.Settings.Default.AAPTPath; }
            set
            {
                HockeyApp.AppLoader.Properties.Settings.Default.AAPTPath = value;
                HockeyApp.AppLoader.Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => this.PathToAAPT);
            }
        }

        #region Commands
        public void AddNew()
        {
            UserConfigurationViewModel uc = new UserConfigurationViewModel(null);
            this.Configurations.Add(uc);
            this.SelectedUserConfiguration = uc;
            NotifyOfPropertyChange("");
        }

        public void Delete()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            System.Windows.MessageBoxResult result = wm.ShowMetroMessageBox("If you delete the configuration, all local data will be lost! Delete anyway?", "Delete configuration?", System.Windows.MessageBoxButton.YesNo);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                UserConfigurationViewModel selCfg = this.SelectedUserConfiguration;
                this.Configurations.Remove(selCfg);
                if (this.Configurations.Count > 0)
                {
                    this.SelectedUserConfiguration = this.Configurations.First();
                }
                this._configuration.UserConfigurations.Remove(selCfg.UserConfiguration);
                this._configuration.Save();
            }
            NotifyOfPropertyChange("");
            
        }

        public bool CanDelete { get { return this.SelectedUserConfiguration != null; } }

        public void OpenAPPTFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Executables | *.exe";
            dlg.Multiselect = false;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                this.PathToAAPT = dlg.FileName;
            }
        }

        #endregion
    }
}