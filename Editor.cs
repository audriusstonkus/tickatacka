using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TickaTacka
{
	/**
	 * Small dialog for entering user name
	 */
	public class Prompt
	{
		public static string ShowDialog(string text, string caption)
		{
			Form prompt = new Form() { Width = 400, Height = 135, Text = caption};
			prompt.Icon = resources.stock_lock_16;
			prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
			GroupBox groupBox = new GroupBox() { Top = 0, Left = 2, Width = 390, Height = 70 };
			prompt.Controls.Add(groupBox);
			
			Label textLabel = new Label() { Left = 8, Top = 16, Text = text, AutoSize = true };
			TextBox textBox = new TextBox() { Left = 8, Top = 40, Width = groupBox.Width - 16 };
			Button okButton = new Button() { Text = "Los", Left = 172, Width = 100, Top = 78 };
			okButton.Click += (sender, e) => {
				prompt.DialogResult = DialogResult.OK;
				prompt.Close();
			};
			Button cancelButton = new Button() { Text = "Cancel", Left = 280, Width = 100, Top = 78 };
			cancelButton.Click += (sender, e) => {
				prompt.DialogResult = DialogResult.Cancel;
				prompt.Close();
			};
			
			prompt.AcceptButton = okButton;
			okButton.Enabled = false;
			textBox.TextChanged += (sender, e) => {
				okButton.Enabled = !String.IsNullOrEmpty(textBox.Text); 
			};

			prompt.Controls.Add(okButton);
			prompt.Controls.Add(cancelButton);
			groupBox.Controls.Add(textLabel);
			groupBox.Controls.Add(textBox);
			
			DialogResult result = prompt.ShowDialog();
			return result != DialogResult.Cancel ? textBox.Text : null;
		}
	}
	
	/**
	 * Dialog for editing exceptions for selected user
	 */
	class ExceptionDialog : Form
	{
		private DataGridView dataGrid;
		private DateTimePicker datePicker;
		private TickUserElement userElement;
		
		public ExceptionDialog(TickUserElement userElement) : base()
		{
			this.Icon = resources.stock_lock_32;
			this.Size = new Size(350, 350);
			this.Text = "Ausnahmen";
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.userElement = userElement;
			
			createUI();
			loadData();
		}
		
		private void createUI()
		{
			GroupBox topGroup = new GroupBox() { Dock = DockStyle.Fill, Text = "Ausnahmen für Benutzer " + userElement.Name };
			this.Controls.Add(topGroup);
			Panel bottomPanel = new Panel() { Dock = DockStyle.Bottom, Height = 45 };
			this.Controls.Add(bottomPanel);

			dataGrid = new DataGridView() { Dock = DockStyle.Fill };
			topGroup.Controls.Add(dataGrid);
			dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGrid.AllowUserToDeleteRows = true;
			dataGrid.AllowUserToAddRows = true;
			dataGrid.CellValidating += new DataGridViewCellValidatingEventHandler(OnCellValidating);
			dataGrid.CellBeginEdit += new DataGridViewCellCancelEventHandler(OnCellBeginEdit);

			DataGridViewColumn dateColumn = new DataGridViewTextBoxColumn() { HeaderText = "Datum", Width = 120 };
			dataGrid.Columns.Add(dateColumn);
			DataGridViewColumn limitColumn = new DataGridViewTextBoxColumn() { HeaderText = "Minuten", Width = 120 };
			dataGrid.Columns.Add(limitColumn);

			Button cancelButton = new Button() { Text = "Abbrechen", Size = new Size(100, 28) };
			cancelButton.Location = new Point(this.Size.Width - cancelButton.Size.Width - 16, 8);
			bottomPanel.Controls.Add(cancelButton);
			this.CancelButton = cancelButton;

			Button okButton = new Button() { Text = "Fertig", Size = new Size(100, 28) };
			okButton.Location = new Point(cancelButton.Left - okButton.Size.Width - 16, 8);
			bottomPanel.Controls.Add(okButton);
			this.AcceptButton = okButton;
			okButton.Click += (sender, e) => { saveData(); };
			
			datePicker = new DateTimePicker();
			topGroup.Controls.Add(datePicker);
			datePicker.Visible = false;
			datePicker.ValueChanged += (sender, e) => { dataGrid.CurrentCell.Value = datePicker.Value.ToShortDateString(); };
			datePicker.Leave += (sender, e) => { datePicker.Hide(); };
		}
		
		private void loadData()
		{
			foreach (TickExceptionElement exceptionElement in userElement.Exceptions){
				dataGrid.Rows.Add(new object[] { exceptionElement.Day.ToShortDateString(), 
					Convert.ToString(exceptionElement.Minutes) });
			} 
		}
		
		private void saveData() 
		{
			List<DateTime> dates = new List<DateTime>();
			userElement.Exceptions.Clear();
			foreach (DataGridViewRow row in dataGrid.Rows){
				if (row.IsNewRow) {
					continue;
				}
				try {
					DateTime date = Convert.ToDateTime(row.Cells[0].Value);
					int minutes = Convert.ToInt32(row.Cells[1].Value);
					if (!dates.Contains(date)){
						TickExceptionElement exceptionElement = new TickExceptionElement();
						exceptionElement.Day = date;
						exceptionElement.Minutes = minutes;
						userElement.Exceptions.Add(exceptionElement);
					}
				}
				catch (Exception) {
					// Skip this row if empty
				}
			}
			this.DialogResult = DialogResult.OK;
			Close();
		}
		
		private void OnCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) 
		{
			if (e.ColumnIndex == 0){
				Point location = dataGrid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Location;
				location.Offset(dataGrid.Location);
				datePicker.Location = location;
				datePicker.Size = dataGrid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Size;
				datePicker.Value = dataGrid.CurrentCell.Value != null ? Convert.ToDateTime(dataGrid.CurrentCell.Value) : DateTime.Now;
				datePicker.Visible = true;
				datePicker.Select();
				datePicker.BringToFront();
				e.Cancel = true;
			}
		}
	
		private void OnCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			int newValue = 0;
			DateTime newDate;
			DataGridViewRow row = dataGrid.CurrentRow;
			row.ErrorText = "";
			if (!dataGrid.CurrentRow.IsNewRow) {
				if (e.ColumnIndex == 0) {
					if (!DateTime.TryParse(e.FormattedValue.ToString(), out newDate)){
						e.Cancel = true;
						row.ErrorText = "Bitte ein gültiges Datum eingeben!";
					}
				}
				else if (e.ColumnIndex == 1){
					if (!int.TryParse(e.FormattedValue.ToString(), out newValue) || newValue < 0) {
						e.Cancel = true;
						row.ErrorText = "Bitte eine positive Minutenzahl eingeben";
					}
				}
			}
		}
	}
	
	/**
	 * Main editor form.
	 */
	public class Editor : Form
	{
		private TickConfiguration configuration;
		private DataGridView dataGrid;
		private Boolean saveNeeded;
		
		public Editor(TickConfiguration configuration)
		{
			this.configuration = configuration;
			this.Icon = resources.stock_lock_32;
			this.Size = new Size(700, 400);
			this.MinimumSize = new Size(400, 300);
			this.Text = "Zeitbegrenzung";
			this.Closing += OnFormClosing;
			
			createUI();
			loadData();
		}
		
		private void createUI()
		{
			GroupBox bottomGroup = new GroupBox();
			bottomGroup.Dock = DockStyle.Fill;
			bottomGroup.Text = "Zeitbegrenzungen in Minuten für folgende Benutzer aktivieren";
			this.Controls.Add(bottomGroup);

			GroupBox topGroup = new GroupBox() { Dock = DockStyle.Top, Height = 80 };
			this.Controls.Add(topGroup);
			
			Label imageLabel = new Label() { Top = 8, Left = 8, Size = new Size(64, 64)};
			imageLabel.Image = new Bitmap(resources.stock_lock_128.ToBitmap(), 64, 64);
			imageLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			topGroup.Controls.Add(imageLabel);
			
			Button closeButton = new Button();
			closeButton.Text = "Schließen";
			closeButton.Size = new Size(100, 28);
			closeButton.Location = new Point(topGroup.Width - closeButton.Width - 8, 12);
			closeButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
			closeButton.Click += OnCloseButtonClick;
			topGroup.Controls.Add(closeButton);

			Button saveButton = new Button();
			saveButton.Text = "Speichern";
			saveButton.Size = new Size(100, 28);
			saveButton.Location = new Point(topGroup.Width - saveButton.Width - 8, 18 + saveButton.Height);
			saveButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
			saveButton.Click += OnSaveButtonClick;
			topGroup.Controls.Add(saveButton);
			
			Button addUserButton = new Button();
			addUserButton.Text = "Hinzufügen";
			addUserButton.Size = new Size(100, 28);
			addUserButton.Location = new Point(saveButton.Location.X - addUserButton.Width - 8, saveButton.Location.Y);
			addUserButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
			addUserButton.Click += OnAddUserButtonClick;
			topGroup.Controls.Add(addUserButton);
			
			Button removeUserButton = new Button();
			removeUserButton.Text = "Löschen";
			removeUserButton.Size = new Size(100, 28);
			removeUserButton.Location = new Point(addUserButton.Location.X - removeUserButton.Size.Width - 8, saveButton.Location.Y);
			removeUserButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
			removeUserButton.Click += OnRemoveUserButtonClick;
			topGroup.Controls.Add(removeUserButton);

			Label titleLabel = new Label();
			titleLabel.Text = "Zeitbegrenzung einrichten";
			titleLabel.Location = new Point(80, 8);
			titleLabel.AutoSize = true;
			titleLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			titleLabel.Font = new Font("Arial", 12.0f, FontStyle.Bold);
			topGroup.Controls.Add(titleLabel);
			
			dataGrid = new DataGridView();
			dataGrid.Dock = DockStyle.Fill;
			bottomGroup.Controls.Add(dataGrid);
			dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGrid.MultiSelect = false;
			dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dataGrid.AllowUserToAddRows = false;
			dataGrid.AllowUserToDeleteRows = true;
			dataGrid.UserDeletedRow += new DataGridViewRowEventHandler(OnRowDeleted);
			dataGrid.CellValidating += new DataGridViewCellValidatingEventHandler(OnCellValidating);
			dataGrid.CellEndEdit += new DataGridViewCellEventHandler(OnCellEndEdit);
			dataGrid.CellContentClick += new DataGridViewCellEventHandler(OnCellContentClick);
			
			DataGridViewColumn userNameColumn = new DataGridViewTextBoxColumn();
			userNameColumn.HeaderText = "Benutzer";
			userNameColumn.Width = 100;
			userNameColumn.ReadOnly = true;
			dataGrid.Columns.Add(userNameColumn);
			
			for (int i = 1; i <= 7; i++) {
				string dayName = TickUserElement.DAYS[i - 1];
				dayName = dayName[0].ToString().ToUpper() + dayName.Substring(1);
				dataGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = dayName, Width = 60 });
			}
			
			DataGridViewButtonColumn exceptionColumn = new DataGridViewButtonColumn();
			exceptionColumn.Width = 100;
			exceptionColumn.Text = "Ausnahmen";
			exceptionColumn.UseColumnTextForButtonValue = true;
			dataGrid.Columns.Add(exceptionColumn);
		}

		private void OnCellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			TickUserElement userElement = (TickUserElement)dataGrid.CurrentRow.Tag;
			if (e.ColumnIndex == dataGrid.ColumnCount - 1){
				ExceptionDialog dialog = new ExceptionDialog(userElement);
				dialog.ShowDialog(this);
				saveNeeded = true;
			}
		}

		private void OnSaveButtonClick(object sender, EventArgs e)
		{
			try {
				configuration.Save();
				saveNeeded = false;
				MessageBox.Show("Die Daten wurden erfolgreich gespeichert", 
					"Zeitbegrenzung", MessageBoxButtons.OK, MessageBoxIcon.Information);
			} catch (Exception){
				MessageBox.Show("Speichern fehlgeschlagen.\nVerfügen Sie über Administratorrechte?", 
					"Fehler", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void OnRemoveUserButtonClick(object sender, EventArgs e)
		{
			DataGridViewRow row = dataGrid.CurrentRow;
			if (row != null) {
				TickUserElement userElement = (TickUserElement)row.Tag;
				DialogResult confirmation = MessageBox.Show(
					String.Format("Die Zeitbegrenzung für Benutzer '{0}' wirklich entfernen?", userElement.Name), 
					"Bestätigung", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (confirmation == DialogResult.Yes) {
					dataGrid.Rows.Remove(row);
					configuration.Users.Remove(userElement);
					saveNeeded = true;
				}
			}
		}

		private void OnAddUserButtonClick(object sender, EventArgs e)
		{
			string userName = Prompt.ShowDialog("Bitte Benutzername eingeben", "Neuer Benutzer");
			if (!String.IsNullOrEmpty(userName)) {
				TickUserElement userElement = new TickUserElement();
				userElement.Name = userName;
				configuration.Users.Add(userElement);
				int index = dataGrid.Rows.Add(new String[] { userElement.Name, userElement.Montag,
					userElement.Dienstag, userElement.Mittwoch, userElement.Donnerstag, 
					userElement.Freitag, userElement.Samstag, userElement.Sonntag
				});
				DataGridViewRow row = dataGrid.Rows[index];
				row.Tag = userElement;
				dataGrid.CurrentCell = row.Cells[1];
				dataGrid.BeginEdit(true);
				saveNeeded = true;
			}
		}

		private void OnCloseButtonClick(object sender, EventArgs e)
		{
			this.Close();
		}
		
		private void OnCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex > 0 && e.ColumnIndex < 8) {
				int newValue = 0;
				DataGridViewRow row = dataGrid.CurrentRow;
				row.ErrorText = "";
				if (!int.TryParse(e.FormattedValue.ToString(), out newValue) || newValue < 0) {
					e.Cancel = true;
					row.ErrorText = "Bitte eine positive Minutenzahl eingeben";
				}
			}
		}
		
		private void OnCellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			DataGridViewRow row = dataGrid.CurrentRow;
			DataGridViewCell cell = dataGrid.CurrentCell;
			TickUserElement userElement = (TickUserElement)row.Tag;
			row.ErrorText = "";
			userElement[e.ColumnIndex] = Int32.Parse(cell.Value.ToString());
			saveNeeded = true;
		}
		
		private void OnRowDeleted(object sender, DataGridViewRowEventArgs e)
		{
			DataGridViewRow row = e.Row;
			TickUserElement userElement = (TickUserElement)row.Tag;
			if (userElement != null) {
				configuration.Users.Remove(userElement);
			}
			saveNeeded = true;
		}


		private void OnFormClosing(object sender, CancelEventArgs e)
		{
			if (saveNeeded){
				DialogResult answer = MessageBox.Show("Die Daten wurden geändert.\nSpeichern?", 
					"Zeitbegrenzung", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (answer == DialogResult.Yes){
					OnSaveButtonClick(sender, e);
					e.Cancel = false;
				}
				else if (answer == DialogResult.No) {
					e.Cancel = false;
				}
				else if (answer == DialogResult.Cancel) {
					e.Cancel = true;
				}
			}
		}

		private void loadData()
		{
			foreach (TickUserElement userElement in configuration.Users) {
				int index = dataGrid.Rows.Add(new String[8] { userElement.Name, userElement.Montag,
					userElement.Dienstag, userElement.Mittwoch, userElement.Donnerstag, 
					userElement.Freitag, userElement.Samstag, userElement.Sonntag
				});
				DataGridViewRow row = dataGrid.Rows[index];
				row.Tag = userElement;
			}
			saveNeeded = false;
		}
	}
}

