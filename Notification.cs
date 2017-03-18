using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
 
namespace TickaTacka
{
	/**
	 * Notification utility for system tray. Reads data file every minute and compares number of minutes
	 * there with maximum in configuration. Shows warnings if it gets too close.
	 */
	public class TickTrayForm : Form
	{
		const int TIMER_INTERVAL = 60000;
		readonly List<int> WARNING_MINUTES = new List<int>(){10, 5, 2};
		private NotifyIcon trayIcon;
		private ContextMenu trayMenu;
		private TickConfiguration configuration;
		private TickDataFile dataFile;
		private Timer timer;
		private Int32? passedMinutes, allowedMinutes;
 
		public TickTrayForm()
		{
			this.Visible = false;
			this.Icon = resources.stock_lock_32;

			trayMenu = new ContextMenu();
			trayMenu.MenuItems.Add("Neustarten", OnReload);
			trayMenu.MenuItems.Add("Zeitbegrenzung einrichten...", OnEdit);
			trayMenu.MenuItems.Add("Aktuellen Verbrauch anzeigen...", OnView);
			trayMenu.MenuItems.Add("-");
			trayMenu.MenuItems.Add("Beenden", OnExit);
 
			trayIcon = new NotifyIcon();
			trayIcon.Icon = resources.stock_lock_16;
			trayIcon.MouseClick += ShowBallonInfo;
			//trayIcon.MouseMove += ShowBallonInfo;
			trayIcon.ContextMenu = trayMenu;
			trayIcon.Visible = true;
			
			configuration = TickConfiguration.Instance;
			dataFile = new TickDataFile(configuration.DataFile);

			timer = new Timer();
			timer.Enabled = true;
			timer.Interval = TIMER_INTERVAL;
			timer.Tick += HandleTimerTick;
		}

		private void HandleTimerTick(object sender, EventArgs e)
		{
			dataFile.load();
			string username = System.Environment.UserName.ToLower();
			DateTime today = DateTime.Now.Date;
			var tickRecord = dataFile.findUserRecord(username, today);
			var configUser = configuration.Users[username] as TickUserElement;
			if (configUser != null && tickRecord != null) {
				int dayOfWeek = (int)today.DayOfWeek;
				if (dayOfWeek == 0) {
					dayOfWeek = 7;
				}
				allowedMinutes = configUser[dayOfWeek];
				if (configUser.Exceptions != null && configUser.Exceptions[today] != null) {
					allowedMinutes = configUser.Exceptions[today].Minutes;
				}
				passedMinutes = tickRecord.Minutes;
				
				// Warnings
				int remainingMinutes = allowedMinutes.Value - passedMinutes.Value;
				if (WARNING_MINUTES.Contains(remainingMinutes)) {
					MessageBox.Show(String.Format("Es sind nur {0} Minuten geblieben!", remainingMinutes), 
						"Zeitbegrenzung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else if (remainingMinutes == 1) {
					MessageBox.Show(String.Format("Dir bleibt nur eine Minute!", remainingMinutes), 
						"Zeitbegrenzung", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			else {
				passedMinutes = null;
				allowedMinutes = null;
			}
		}

		private void ShowBallonInfo(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				return;
			}
			if (passedMinutes.HasValue && allowedMinutes.HasValue) {
				int min = Math.Min(passedMinutes.Value, allowedMinutes.Value);
				string message = min > 1 ? String.Format("{0} Minuten von {1} sind vergangen", min, allowedMinutes) 
					: String.Format("Heutige Begrenzung: {0} Minuten", allowedMinutes);
				trayIcon.BalloonTipText = message;
			}
			else {
				trayIcon.BalloonTipText = "Keine Panik, Zeitbegrenzung ist nicht aktiviert!";
			}
			trayIcon.BalloonTipTitle = "Zeitbegrenzung";
			trayIcon.BalloonTipIcon = ToolTipIcon.Info;
			trayIcon.ShowBalloonTip(1);
		}
 
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Visible = false; // Hide form window.
			ShowInTaskbar = false; // Remove from taskbar.
			timer.Start();
			HandleTimerTick(null, null);
		}
		 
		protected override void OnShown(EventArgs e)
		{ 
			base.OnShown(e);
			Visible = false; // Hide form window.
		}

		private void OnExit(object sender, EventArgs e)
		{
			Application.Exit();
		}
 
		private void OnReload(object sender, EventArgs e)
		{
			configuration = TickConfiguration.Instance;
			HandleTimerTick(null, null);
		}
		
		private void OnEdit(object sender, EventArgs e)
		{
			try {
				Editor editor = new Editor(configuration);
				editor.ShowDialog();
			} catch (Exception ex){
				MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				configuration = TickConfiguration.Instance;
				HandleTimerTick(null, null);
			}
		}

		private void OnView(object sender, EventArgs e)
		{
			try {
				Viewer viewer = new Viewer(configuration, dataFile);
				viewer.ShowDialog();
			} catch (Exception ex){
				MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing) {
				trayIcon.Dispose();
			} 
			base.Dispose(isDisposing);
		}
	}
}
