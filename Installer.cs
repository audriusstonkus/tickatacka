using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Text;

namespace TickaTacka
{
	/**
	 * Installer for windows service
	 * To install service use "InstallUtil.exe" utility
	 */
	[RunInstaller(true)]
	public class TickInstaller : Installer
	{
		private ServiceProcessInstaller processInstaller;
		private ServiceInstaller serviceInstaller;

		public TickInstaller()
		{
			processInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();

			processInstaller.Account = ServiceAccount.LocalSystem;
			serviceInstaller.StartType = ServiceStartMode.Automatic;
			serviceInstaller.ServiceName = "TickaTacka";

			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}

		public override void Install(System.Collections.IDictionary stateSaver)
		{
			var path = new StringBuilder(Context.Parameters["assemblypath"]);
			if (path[0] != '"') {
				path.Insert(0, '"');
				path.Append('"');
			}
			path.Append(" -service");
			Context.Parameters["assemblypath"] = path.ToString();
			base.Install(stateSaver);

		}
	}
}