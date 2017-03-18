using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TickaTacka
{
	/**
	 * Dialog with conveniently readable usage data
	 */
	public class Viewer : Form
	{
		private TickConfiguration configuration;
		private TickDataFile dataFile;
		
		private DataGridView dataGrid;
		private ComboBox userBox;
		
		public Viewer(TickConfiguration configuration, TickDataFile dataFile)
		{
			this.configuration = configuration;
			this.dataFile = dataFile;
			this.Icon = resources.stock_lock_32;
			this.Size = new Size(600, 400);
			this.MinimumSize = new Size(400, 300);
			this.Text = "Zeitbegrenzung";
			
			createUI();
			loadData();
		}
		
		private void createUI()
		{
			GroupBox bottomGroup = new GroupBox() { Dock = DockStyle.Fill, 
				Text = "Täglicher Zeitverbrauch des ausgewählten Benutzers in Minuten" };
			this.Controls.Add(bottomGroup);

			GroupBox topGroup = new GroupBox() { Dock = DockStyle.Top, Height = 80 };
			this.Controls.Add(topGroup);
			
			Label imageLabel = new Label() { Top = 8, Left = 8, Size = new Size(64, 64) };
			imageLabel.Image = new Bitmap(resources.stock_lock_128.ToBitmap(), 64, 64);
			imageLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			topGroup.Controls.Add(imageLabel);
			
			Button closeButton = new Button() { Text = "Schließen", Size = new Size(100, 28) };
			closeButton.Location = new Point(topGroup.Size.Width - closeButton.Size.Width - 8, 14);
			closeButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
			closeButton.Click += (sender, e) => { this.Close(); };
			topGroup.Controls.Add(closeButton);
			
			Label titleLabel = new Label() { Text = "Aktueller Zeitverbrauch", Location = new Point(80, 8), 
				AutoSize = true };
			titleLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			titleLabel.Font = new Font("Arial", 12.0f, FontStyle.Bold);
			topGroup.Controls.Add(titleLabel);
			
			Label userLabel = new Label() { Text = "Benutzer auswählen:", Location = new Point(80, 54), Width = 150 };
			topGroup.Controls.Add(userLabel);
			
			userBox = new ComboBox() { Location = new Point(230, 50), Size = new Size(topGroup.Width - 238, 25), 
				AllowDrop = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
			userBox.DropDownStyle = ComboBoxStyle.DropDownList;
			userBox.SelectedIndexChanged += LoadUserData;
			topGroup.Controls.Add(userBox);
			
			dataGrid = new DataGridView();
			dataGrid.Dock = DockStyle.Fill;
			bottomGroup.Controls.Add(dataGrid);
			dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGrid.MultiSelect = false;
			dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dataGrid.AllowUserToAddRows = false;
			dataGrid.AllowUserToDeleteRows = false;
			
			DataGridViewColumn dayColumn = new DataGridViewTextBoxColumn() { HeaderText = "Tag", 
				Width = 150, ReadOnly = true };
			dataGrid.Columns.Add(dayColumn);
			DataGridViewColumn limitColumn = new DataGridViewTextBoxColumn() { HeaderText = "Begrenzung", 
				Width = 100, ReadOnly = true };
			dataGrid.Columns.Add(limitColumn);
			DataGridViewColumn usageColumn = new DataGridViewTextBoxColumn() { HeaderText = "Verbrauch", 
				Width = 100, ReadOnly = true };
			dataGrid.Columns.Add(usageColumn);
			DataGridViewColumn remainderColumn = new DataGridViewTextBoxColumn() { HeaderText = "Restbetrag", 
				Width = 100, ReadOnly = true };
			dataGrid.Columns.Add(remainderColumn);
		}
		
		private int getUserLimitForDate(string username, DateTime date)
		{
			int limit = 0;
			TickUserElement userElement = configuration.Users[username];
			if (userElement != null){
				int dayOfWeek = (int)date.DayOfWeek;
				if (dayOfWeek == 0) {
					dayOfWeek = 7;
				}
				limit = userElement[dayOfWeek];
				if (userElement.Exceptions != null) {
					if (userElement.Exceptions[date] != null) {
						limit = userElement.Exceptions[date].Minutes;
					}
				}
			}
			return limit;
		}

		private void LoadUserData(object sender, EventArgs e)
		{
			dataGrid.Rows.Clear();
			string selectedUser = userBox.SelectedItem.ToString();
			DateTime date = dataFile.Records.Count > 0 ? dataFile.Records[0].Date : DateTime.Today;
			while (date <= DateTime.Now){
				TickRecord record = dataFile.findUserRecord(selectedUser, date);
				int limit = getUserLimitForDate(selectedUser, date);
				int used = record != null ? record.Minutes : 0;
				int rest = limit > used ? limit - used : 0;
				int restPercent = limit > 0 ?rest * 100 / limit : 0;
				dataGrid.Rows.Insert(0, new object[] { date.ToShortDateString(), 
					limit, used, String.Format("{0} Min ({1}%)", rest, restPercent) 
				});
				DataGridViewRow row = dataGrid.Rows[0];
				if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
					row.DefaultCellStyle.BackColor = Color.LightSalmon;
				}
				date = date.AddDays(1);
			}
		}
		
		private void loadData()
		{
			foreach (TickRecord record in dataFile.Records){
				if (!userBox.Items.Contains(record.Username)){
					userBox.Items.Add(record.Username);
				}
			}
			foreach (TickUserElement userElement in configuration.Users){
				if (!userBox.Items.Contains(userElement.Name)){
					userBox.Items.Add(userElement.Name);
				}
			}
			userBox.SelectedIndex = 0;
		}
	}
}

