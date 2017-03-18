using System;
using System.Configuration;

namespace TickaTacka
{		
	/**
	 * Configuration section
	 */
	public class TickConfiguration : ConfigurationSection
	{
		public const string DATE_FORMAT = "yyyy-MM-dd";
		
		private Configuration configuration;
		
		public static TickConfiguration Instance {
			get {
				Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				TickConfiguration section = config.GetSection("TickSettings") as TickConfiguration;
				section.configuration = config;
				return section;
			}
		}
		
		public void Save()
		{
			configuration.Save();
		}
		
		[ConfigurationProperty("tickInterval", DefaultValue = 1000, IsRequired = true)]
		public int TickInterval {
			get { return (int)this["tickInterval"]; }
			set { this["tickInterval"] = value; }
		}
		
		[ConfigurationProperty("dataFile", IsRequired=true)]
		[StringValidator(InvalidCharacters = "!#%^&*(){};’")]
		public string DataFile {
			get { return (string)this["dataFile"]; }
			set { this["dataFile"] = value; }
		}
		
		[ConfigurationProperty("shutdownCommand", IsRequired=false)]
		public string ShutdownCommand {
			get { return (string)this["shutdownCommand"]; }
			set { this["shutdownCommand"] = value; }
		}

		[ConfigurationProperty("shutdownArguments", IsRequired=false)]
		public string ShutdownArguments {
			get { return (string)this["shutdownArguments"]; }
			set { this["shutdownArguments"] = value; }
		}

		[ConfigurationProperty("users", IsRequired=true)]
		public TickUserElementCollection Users {
			get { return (TickUserElementCollection)this["users"]; }
			set { this["users"] = value; }
		}
	}
	
	/**
	 * Collection of "user" elements
	 */
	[ConfigurationCollection(typeof(TickUserElement), 
		CollectionType=ConfigurationElementCollectionType.BasicMap, 
	 	AddItemName="user"
	)]
	public class TickUserElementCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new TickUserElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as TickUserElement).Name;
		}
		
		public new TickUserElement this[string index] {
			get { return (TickUserElement)base.BaseGet(index); }
		}
		
		public void Remove(TickUserElement userElement)
		{
			base.BaseRemove(userElement.Name);
		}
		
		public void Add(TickUserElement userElement)
		{
			base.BaseAdd(userElement);
		}
	}
	
	/**
	 * Single "user" element
	 */
	public class TickUserElement : ConfigurationElement
	{
		public static string[] DAYS = {"montag", "dienstag", "mittwoch", "donnerstag", "freitag", "samstag", "sonntag"};
		
		[ConfigurationProperty("name", IsRequired=true)]
		public string Name {
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty("montag", IsRequired=true)]
		public string Montag {
			get { return (string)this["montag"]; }
			set { this["montag"] = value; }
		}

		[ConfigurationProperty("dienstag", IsRequired=true)]
		public string Dienstag {
			get { return (string)this["dienstag"]; }
			set { this["dienstag"] = value; }
		}

		[ConfigurationProperty("mittwoch", IsRequired=true)]
		public string Mittwoch {
			get { return (string)this["mittwoch"]; }
			set { this["mittwoch"] = value; }
		}

		[ConfigurationProperty("donnerstag", IsRequired=true)]
		public string Donnerstag {
			get { return (string)this["donnerstag"]; }
			set { this["donnerstag"] = value; }
		}

		[ConfigurationProperty("freitag", IsRequired=true)]
		public string Freitag {
			get { return (string)this["freitag"]; }
			set { this["freitag"] = value; }
		}

		[ConfigurationProperty("samstag", IsRequired=true)]
		public string Samstag {
			get { return (string)this["samstag"]; }
			set { this["samstag"] = value; }
		}

		[ConfigurationProperty("sonntag", IsRequired=true)]
		public string Sonntag {
			get { return (string)this["sonntag"]; }
			set { this["sonntag"] = value; }
		}

		[ConfigurationProperty("exceptions", IsRequired=false)]
		public TickExceptionElementCollection Exceptions {
			get { return (TickExceptionElementCollection)this["exceptions"]; }
			set { this["exceptions"] = value; }
		}

		public int this[int index] {
			get { 
				if (index <= 0 || index > DAYS.Length) {
					throw new ArgumentOutOfRangeException();
				}
				try {
					return Int32.Parse((string)this[DAYS[index - 1]]);
				}
				catch (Exception) {
					return 0;
				}
			}
			
			set {
				if (index <= 0 || index > DAYS.Length) {
					throw new ArgumentOutOfRangeException();
				}
				this[DAYS[index - 1]] = value;
			}
		}
	}

	/**
	 * Collection of "exception" elements
	 */
	[ConfigurationCollection(typeof(TickExceptionElement), 
		CollectionType=ConfigurationElementCollectionType.BasicMap, 
	 	AddItemName="exception"
	)]
	public class TickExceptionElementCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new TickExceptionElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as TickExceptionElement).Day;
		}
		
		public TickExceptionElement this[DateTime index] {
			get { return (TickExceptionElement)base.BaseGet(index); }
		}
		
		public void Remove(TickExceptionElement exceptionElement)
		{
			base.BaseRemove(exceptionElement);
		}
		
		public void Clear()
		{
			base.BaseClear();
		}
		
		public void Add(TickExceptionElement exceptionElement)
		{
			base.BaseAdd(exceptionElement);
		}
	}

	/**
	 * Single "exception" element
	 */
	public class TickExceptionElement : ConfigurationElement
	{
		[ConfigurationProperty("day", IsRequired=true)]
		public DateTime Day {
			get { return DateTime.Parse(this["day"].ToString()); }
			set { this["day"] = value.ToString(TickConfiguration.DATE_FORMAT); }
		}

		[ConfigurationProperty("minutes", IsRequired=true)]
		[IntegerValidator(MinValue = 0, MaxValue = 24 * 60)]
		public int Minutes {
			get { return (int)this["minutes"]; }
			set { this["minutes"] = value; }
		}
	}
}
