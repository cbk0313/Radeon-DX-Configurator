using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Radeon_DX_Configurator.Model;
using System.Collections.ObjectModel;
using Radeon_DX_Configurator.Tools.Ini;

using Radeon_DX_Configurator.ViewModel.Command;

namespace Radeon_DX_Configurator.ViewModel
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		

		private MainWindowModel model = new MainWindowModel();
		public MainWindowModel Model
		{
			get { return model; }
			set { model = value; OnPropertyChanged("Model");}
		}

		private RegistryKey Parentkey = Registry.LocalMachine;

		private readonly string[] RegularDX_LIST = 
		{
			"atiumd64.dll",           
			"atiumd64.dll",
			"atidxx64.dll",
			"atidxx64.dll"
		};

		private readonly string[] DXNAVI_LIST =
		{
			"amdxn64.dll",
			"amdxn64.dll",
			"amdxx64.dll",
			"amdxx64.dll"
		};

		private readonly string[] RegularDXWOW_LIST =
		{
			"atiumdag.dll",
			"atiumdag.dll",
			"atidxx32.dll",
			"atidxx32.dll"
		};

		private readonly string[] DXNAVIWOW_LIST =
		{
			"amdxn32.dll",
			"amdxn32.dll",
			"amdxx32.dll",
			"amdxx32.dll"
		};

		private readonly int[] DX9_Idxs = { 0, 1 };
		private readonly int[] DX11_Idxs = { 2, 3 };
		


		public ICommand RegularDX9Button { get; }
		public ICommand RegularDX11Button { get; }
		public ICommand DXNAVI_DX9Button { get; }
		public ICommand DXNAVI_DX11Button { get; }
		public ICommand RestoreBackupButton { get; }

		private readonly String BackupFile = "backup.ini";

		private readonly String Vender32Name = "D3DVendorNameWoW";
		private readonly String Vender64Name = "D3DVendorName";

		public void ReadRegist()
		{

			// Open the System
			RegistryKey powerKey = Parentkey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", true);
			if (powerKey != null)
			{
				var vender = powerKey.GetValue(Vender64Name);
				Model.CurrentValue = new ObservableCollection<string>(vender as string[]);
			}
			powerKey.Dispose();
		}

		public void ReadWowRegist()
		{
			

			// Open the System
			RegistryKey powerKey = Parentkey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", true);
			if (powerKey != null)
			{
				var vender = powerKey.GetValue(Vender32Name);
				Model.CurrentWOWValue = new ObservableCollection<string>(vender as string[]);
			}

			powerKey.Dispose();
		}

		public void LoadBackup()
		{
			IniFile ini = new IniFile();
			
			if (ini.Load(BackupFile))
			{
				string backupPath = Path.GetDirectoryName(ini[Vender64Name]["0"].GetString());
				string curPath = Path.GetDirectoryName(Model.CurrentValue[0]);
				if (backupPath != curPath)
				{
					var result = MessageBox.Show("The backup recovery function does not work because the current path of the .dll does not match the backup file. Would you like to create a new backup file?", "backup", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes)
					{
						for (int i = 0; i < 4; i++)
						{
							string num = i.ToString();
							ini[Vender64Name][num] = Model.CurrentValue[i];
							ini[Vender32Name][num] = Model.CurrentWOWValue[i];
						}
						ini.Save(BackupFile);
					}
				}
			}
			else
			{
				var result = MessageBox.Show("There is no backup file. Create a backup file?", "backup", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
				{
					for(int i = 0; i < 4; i++)
					{
						string num = i.ToString();
						ini[Vender64Name][num] = Model.CurrentValue[i];
						ini[Vender32Name][num] = Model.CurrentWOWValue[i];
					}
					ini.Save(BackupFile);
				}
				else
				{

				}
			}

		}

		public MainWindowViewModel()
		{
			ReadRegist();
			ReadWowRegist();

			LoadBackup();
			RegularDX9Button = new DelegateCommand<object>(p => RegularDX9ButtonClick(p));
			RegularDX11Button = new DelegateCommand<object>(p => RegularDX11ButtonClick(p));
			DXNAVI_DX9Button = new DelegateCommand<object>(p => DXNAVI_DX9ButtonClick(p));
			DXNAVI_DX11Button = new DelegateCommand<object>(p => DXNAVI_DX11ButtonClick(p));
			RestoreBackupButton = new DelegateCommand<object>(p => RestoreBackupButtonClick(p));
		}

		private void RestoreBackupButtonClick(object p)
		{
			IniFile ini = new IniFile();
			if (ini.Load(BackupFile))
			{
				string backupPath = Path.GetDirectoryName(ini[Vender64Name]["0"].GetString());
				string curPath = Path.GetDirectoryName(Model.CurrentValue[0]);

				if(backupPath == curPath)
                {
					RegistryKey powerKey = Parentkey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", true);
					if (powerKey != null)
					{
						powerKey.SetValue(Vender64Name, new string[] { ini[Vender64Name]["0"].GetString(), ini[Vender64Name]["1"].GetString(), ini[Vender64Name]["2"].GetString(), ini[Vender64Name]["3"].GetString() });
						powerKey.SetValue(Vender32Name, new string[] { ini[Vender32Name]["0"].GetString(), ini[Vender32Name]["1"].GetString(), ini[Vender32Name]["2"].GetString(), ini[Vender32Name]["3"].GetString() });
						MessageBox.Show("Recovery success.");
					}

					powerKey.Dispose();
					
				}
				else
                {
					MessageBox.Show("Recovery failed because the current path of the .dll does not match the backup file");
                }
			}
			ReadRegist();
			ReadWowRegist();
		}

		private void SetupRegist()
        {
			RegistryKey powerKey = Parentkey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", true);

			if (powerKey != null)
			{
				string[] veder32 = new string[] { Model.CurrentWOWValue[0], Model.CurrentWOWValue[1], Model.CurrentWOWValue[2], Model.CurrentWOWValue[3] };
				string[] veder64 = new string[] { Model.CurrentValue[0], Model.CurrentValue[1], Model.CurrentValue[2], Model.CurrentValue[3] };
				powerKey.SetValue(Vender32Name, veder32);
				powerKey.SetValue(Vender64Name, veder64);
			}

			powerKey.Dispose();
		}

		private void SetVender(int[] idxs, string[] list32, string[] list64)
        {
			foreach (var item in idxs)
			{
				var path_32 = Path.GetDirectoryName(Model.CurrentWOWValue[item]);
				var path_64 = Path.GetDirectoryName(Model.CurrentValue[item]);

				Model.CurrentWOWValue[item] = Path.Combine(path_32, list32[item]);
				Model.CurrentValue[item] = Path.Combine(path_64, list64[item]);
			}
		}

		private void RegularDX9ButtonClick(object p)
		{
			SetVender(DX9_Idxs, RegularDXWOW_LIST, RegularDX_LIST);
			SetupRegist();
		}
		private void RegularDX11ButtonClick(object p)
		{
			SetVender(DX11_Idxs, RegularDXWOW_LIST, RegularDX_LIST);
			SetupRegist();
		}
		private void DXNAVI_DX9ButtonClick(object p)
		{
			SetVender(DX9_Idxs, DXNAVIWOW_LIST, DXNAVI_LIST);
			SetupRegist();
		}
		private void DXNAVI_DX11ButtonClick(object p)
		{
			SetVender(DX11_Idxs, DXNAVIWOW_LIST, DXNAVI_LIST);
			SetupRegist();
		}




		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
