using System;
using System.Collections.Generic; 
using System.Text;
using System.IO;

namespace TickaTacka
{
	/**
	 * One line in the data file
	 */
	public class TickRecord
	{
		private string user;
		private DateTime date;
		private int minutes;

		public TickRecord()
		{
			date = DateTime.Now.Date;
			minutes = 0;
			user = null;
		}
		
		public TickRecord(string user, DateTime date, int minutes)
		{
			this.user = user;
			this.date = date;
			this.minutes = minutes;
		}
		
		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append(date.ToString(TickConfiguration.DATE_FORMAT));
			buffer.Append('\t');
			buffer.Append(user);
			buffer.Append('\t');
			buffer.Append(minutes);
			return buffer.ToString();
		}
		
		public void parse(string line)
		{
			List<string> words = new List<string>();
			foreach (string word in line.Split(new char[] {'\t', ' '})) {
				if (!String.IsNullOrWhiteSpace(word)) {
					words.Add(word);
				}
			}
			if (words.Count != 3) {
				throw new FormatException("Invalid line " + line);
			}
			try {
				date = DateTime.ParseExact(words[0], TickConfiguration.DATE_FORMAT, null).Date;
			}
			catch (Exception ex) {
				throw new FormatException("Invalid date " + line, ex);
			}
			user = words[1];
			try {
				minutes = Int32.Parse(words[2]);
			}
			catch (Exception ex) {
				throw new FormatException("Invalid minutes " + line, ex);
			}
			if (minutes < 0) {
				throw new FormatException("Invalid minutes " + minutes);
			}
		}
		
		public string Username {
			get { return user; }
			set { user = value; }
		}

		public DateTime Date {
			get { return date; }
			set { date = value; }
		}
	
		public int Minutes {
			get { return minutes; }
			set { minutes = value; }
		}
	}
	
	/**
	 * Data file where current minutes are stored. 
	 * Format of each line: date, username, minutes
	 */
	public class TickDataFile
	{
		private List<TickRecord> records;
		private string dataFileName;
		
		public TickDataFile(string fileName)
		{
			this.dataFileName = fileName;
			records = new List<TickRecord>();
		}
		
		public List<TickRecord> Records {
			get { return records; }
		}
		
		public TickRecord findUserRecord(string user, DateTime date)
		{
			foreach (var rec in records) {
				if (user.Equals(rec.Username) && date.Equals(rec.Date)) {
					return rec;
				}
			}
			return null;
		}
		
		public TickRecord createTickRecord(string user, DateTime date)
		{
			var record = new TickRecord(user, date, 0);
			records.Add(record);
			return record;
		}
		
		public void load()
		{
			records.Clear();
			try {
				using (TextReader reader = new StreamReader(dataFileName)) {
					string line;
					while ((line = reader.ReadLine()) != null) {
						try {
							TickRecord record = new TickRecord();
							record.parse(line);
							records.Add(record);
						}
						catch (FormatException ex) {
							Console.WriteLine(ex.Message);
						}
					}
				}
			}
			catch (Exception) {
				Console.WriteLine(String.Format("Daten in {0} nicht gefunden, neue Datei wird angelegt", dataFileName));
			}
		}
		
		public void save()
		{
			try {
				using (TextWriter writer = new StreamWriter(dataFileName, false)) {
					foreach (TickRecord record in records) {
						writer.WriteLine(record.ToString());
					}
				}
			}
			catch (Exception) {
				Console.WriteLine(String.Format("Datei {0} konnte nicht gespeichert werden!", dataFileName));
			}
		}
	}
}