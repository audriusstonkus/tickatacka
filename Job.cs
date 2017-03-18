using System;
using System.Threading;
using System.Security.Principal;
using System.Collections.Generic; 
using System.Linq; 
using System.Text;
using System.IO;
using System.Management;
using System.Diagnostics;

namespace TickaTacka
{	
	/**
	 * Job which gets executed every few minutes and updates minute count for every logged in user.
	 * If minute count exceeds limits in configuration, the specified command is run (i.e. shutdown)
	 */
	public class TickJob
	{
		private TickDataFile dataFile;
		private int interval;
		
		public TickJob(int interval, string dataFileName)
		{
			this.interval = interval;
			dataFile = new TickDataFile(dataFileName);
		}
		
		private List<String> GetLoggedInUsers()
		{ 
			List<string> usernames = new List<string>();
			try {
				ManagementObjectSearcher userSearcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
				ManagementObjectCollection objectCollection = userSearcher.Get();
				foreach (var managementObject in objectCollection.Cast<ManagementBaseObject>()) {
					string username = (string)managementObject["UserName"];
					username = username.ToLower();
					// Strip windows domain
					if (username.Contains("\\")) {
						username = username.Substring(username.IndexOf("\\") + 1);
					}
					usernames.Add(username);
				}
			}
			catch (Exception ex) {
				// Just take current user
				Console.WriteLine(ex.Message);
				usernames.Add(Environment.UserName);
			}
			return usernames;
		}
		
		private void runShutdownCommand()
		{
			TickConfiguration configuration = TickConfiguration.Instance;
			string shutdownCommand = configuration.ShutdownCommand;
			string shutdownArguments = configuration.ShutdownArguments;
			Process.Start(shutdownCommand, shutdownArguments);
		}
		
		public void tick(object stateObject)
		{
			dataFile.load();
			List<string> loggedInUsers = GetLoggedInUsers();
			DateTime today = DateTime.Now.Date;
			TickConfiguration configuration = TickConfiguration.Instance;
			foreach (var user in loggedInUsers) {
				var configUser = configuration.Users[user] as TickUserElement;
				if (configUser != null) {
					int dayOfWeek = (int)today.DayOfWeek;
					if (dayOfWeek == 0) {
						dayOfWeek = 7;
					}
					int maxMinutes = configUser[dayOfWeek];
					if (configUser.Exceptions != null) {
						if (configUser.Exceptions[today] != null) {
							maxMinutes = configUser.Exceptions[today].Minutes;
						}
					}
					var userRecord = dataFile.findUserRecord(user, today);
					if (userRecord == null) {
						userRecord = dataFile.createTickRecord(user, today);
					}
					userRecord.Minutes += interval;
					Console.WriteLine(String.Format("Benutzer {0}: {1} Minuten von {2} sind vorbei", user, userRecord.Minutes, maxMinutes));
					if (userRecord.Minutes > maxMinutes) {
						runShutdownCommand();
					}
				}
			}
			dataFile.save();
		}
	}
}
