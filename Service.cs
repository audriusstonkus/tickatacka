using System;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Windows.Forms;

namespace TickaTacka
{
	/**
	 * Windows service for time limitation.
	 */
	public class TickService : ServiceBase
	{
		const int DEFAULT_INTERVAL = 1; // Minuten
		const string DEFAULT_FILE_NAME = ".tickTackData";
		private TickJob job;
		private System.Threading.Timer stateTimer;
		private TimerCallback timerDelegate;
		private int interval;
		private string dataFileName;
		
		public TickService()
		{
			this.ServiceName = "TickaTacka";
			this.CanStop = true;
			this.CanPauseAndContinue = false;
			this.AutoLog = true;
			loadConfig();
			this.job = new TickJob(interval, dataFileName);
		}
		
		private string getDefaultDataFileName()
		{
			string dir = Environment.SystemDirectory;
			return dir + Path.PathSeparator + DEFAULT_FILE_NAME;
		}
		
		private void loadConfig()
		{
			try {
				TickConfiguration config = TickConfiguration.Instance;
				interval = config.TickInterval;
				dataFileName = config.DataFile;
			}
			catch (Exception ex) {
				Console.WriteLine(ex.Message);
				if (ex.InnerException != null) {
					Console.WriteLine(ex.InnerException.Message);
				}
				interval = DEFAULT_INTERVAL;
				dataFileName = getDefaultDataFileName();
			}
		}
		
		protected override void OnStart(string [] args)
		{
			int intervalMilis = 60 * 1000 * interval;
			timerDelegate = new TimerCallback(job.tick);
			stateTimer = new System.Threading.Timer(timerDelegate, null, 
								intervalMilis, intervalMilis);
		}
	
		protected override void OnStop()
		{
			stateTimer.Dispose();
		}
		
		public static void RestartService(string serviceName, int timeoutMilliseconds)
		{
			ServiceController service = new ServiceController(serviceName);
			try {
				int millisec1 = Environment.TickCount;
				TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

				service.Stop();
				service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

				// count the rest of the timeout
				int millisec2 = Environment.TickCount;
				timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

				service.Start();
				service.WaitForStatus(ServiceControllerStatus.Running, timeout);
			}
			catch (Exception ex) {
				MessageBox.Show("Fehler: " + ex.Message);
			}
		}
		
		protected static void Usage()
		{
			Console.WriteLine("Usage: TickaTacka.exe (service|notifier|standalone)");
		}
	
		/**
		 * Entry point. Run as service, standalone (for testing), or as notifier.
		 */
		[STAThread]
		public static void Main(string[] args)
		{
			if (args.Length != 1) {
				Usage();
				return;
			}
			TickService service = new TickService();
			if (args[0].Contains("standalone")) {
				Console.WriteLine("Starting job in main ...");
				service.job.tick(null);
			}
			else if (args[0].Contains("service")) {
				Console.WriteLine("Starting service ...");
				System.ServiceProcess.ServiceBase.Run(service);
			}
			else if (args[0].Contains("notifier")) {
				Console.WriteLine("Starting UI notifier ...");
				Application.Run(new TickTrayForm());
			}
			else {
				Usage();
			}
		}
	}
}