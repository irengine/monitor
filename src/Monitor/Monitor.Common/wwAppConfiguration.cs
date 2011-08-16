using System;

using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Xml;


namespace Monitor.Common
{
	/// <summary>
	/// Configuration object class that persists its public members to the 
	/// application's .Config file in a type safe manner. This class manages 
	/// reading and writing the Config settings and providing them in a consistent 
	/// and type-safe manner.
	/// 
	/// You implement a configuration object by creating a new subclass of the 
	/// wwAppConfiguration class and adding public Fields to the class. Every new 
	/// member you add will be persisted to the application's .Config file. On 
	/// first use the class also writes out default values into the .Config file so
	///  the data is always there. The data is always returned in a fully typed 
	/// manner - you simply reference the properties of this object.
	/// 
	/// Values are stored in the <<i>>AppSettings<</i>> section of the .Config file
	///  and uses ApplicationSettings to internally retrieve this data. However, 
	/// the data is always returned in the proper type format rather than as string
	///  and null instances are never a problem as there will always be a default 
	/// value returned. This reduces the amount of code that goes along with 
	/// pulling data out of the .Config file.
	/// 
	/// Supported fields type are any simple types (string, decimal, double, int, 
	/// boolean, datetime etc. ) as well as enums. Enums must be persisted into the
	///  front end interface using strings (ie. if you use it in a listbox value 
	/// the value must be string).
	/// 
	/// The class also provides the ability to encrypt keys by implementing a 
	/// custom constructor that passes a field list and an encryption key to be 
	/// used for encrypting one or more keys in the configuration.
	/// </summary>
	public abstract class wwAppConfiguration
	{
		/// <summary>
		/// List of fields that are to be encrypted.
		/// </summary>
		private string EncryptFieldList = "";

		/// <summary>
		/// Key used for encryption. If this key is null the default
		/// of the wwEncrypt class is used.
		/// </summary>
		private string EncryptKey = "";


		/// <summary>
		/// Internally held value that holds the Section to read and write from
		/// </summary>
		private string ConfigSectionName = "";

		/// <summary>
		/// NOTE: If you subclass you must explicitly cause this constructor to fire with:
		/// 	  bool t = WebConfig.ForceConstructor;
		/// or alternate implement it yourself as shown below.
		/// </summary>
		/// <example>
		///	public class App 
		///	{
		///		public static WebStoreConfig Configuration;
		///		
		///		// *** Static causes one time load at application startup
		///		static App() 
		///		{
		///			/// *** Load the properties from the Config file
		///			Configuration = new WebStoreConfig();
		///		}
		///	}
		///	
		///	Throughout the application:
		///	
		///	To read a setting (ASP.Net expression):
		///	<%= Westwind.WebStore.App.Configuration.Setting %>
		///	
		///	To write all property settings:
		///	App.Configuration.WriteKeysToConfig(null)
		/// </example>
		public wwAppConfiguration() 
		{
			/// *** Load the properties from the Config file
			this.ReadKeysFromConfig();
		}

		/// <summary>
		/// Constructor used to pass encryption information to the class.
		/// </summary>
		/// <param name="EncryptFields"></param>
		/// <param name="EncryptKey"></param>
		public wwAppConfiguration(string EncryptFields,string EncryptKey) 
		{
			this.SetEnryption(EncryptFields,EncryptKey);
		
			/// *** Load the properties from the Config file
			this.ReadKeysFromConfig();
		}


		/// <summary>
		/// Does not load keys from the Config file automatically.
		/// Use this if you choose to persist the object on your own
		/// potentially with wwUtils.SerializeObject/DeserializeObject
		/// </summary>
		/// <param name="DontLoadFromConfig"></param>
		public wwAppConfiguration(bool DontLoadFromConfig)
		{
		}

		/// <summary>
		/// Sets the Configuration File Section if the default is section is not used
		/// </summary>
		/// <param name="ConfigurationFileSection"></param>
		public void SetConfigurationSection(string ConfigurationFileSection) 
		{
			this.ConfigSectionName = ConfigurationFileSection;
		}

		/// <summary>
		/// Sets the Configuration File Section if the default is section is not used
		/// <seealso>Class wwAppConfiguration</seealso>
		/// </summary>
		/// <param name="ConfigurationFileSection"></param>
		/// <returns>Void</returns>
		/// <example>
		/// WebStoreConfig Config = new WebStoreConfig(false);
		/// Config.SetConfigurationSection("WebStore");
		/// Config.ReadKeysFromConfig();
		/// </example>
		public void SetEnryption(string EncryptFields,string EncryptKey)  
		{
			this.EncryptFieldList = "," + EncryptFields.ToLower() + ",";
			this.EncryptKey = EncryptKey;
		}


		/// <summary>
		/// Reads all the configuration settings from the .Config file into the public 
		/// fields of this object.
		/// 
		/// If the keys don't exist in the file the values are returned as the default 
		/// values set on the fields. If keys missing they are written into the .Config
		///  file with their default values insuring that the class and the config file
		///  are always in sync.
		/// <seealso>Class wwAppConfiguration</seealso>
		/// </summary>
		/// <returns>void</returns>
		public virtual void ReadKeysFromConfig() 
		{
			Type typeWebConfig = this.GetType();
			MemberInfo[] Fields = typeWebConfig.GetMembers(BindingFlags.Public | 
				BindingFlags.Instance );
		
			// *** Set a flag for missing fields
			// *** If we have any we'll need to write them out 
			bool MissingFields = false;

			foreach(MemberInfo Member in Fields) 
			{
				string TypeName = null;
			
				FieldInfo Field = null;
				PropertyInfo Property = null;

				if (Member.MemberType == MemberTypes.Field)
					Field = (FieldInfo) Member;
				else if (Member.MemberType == MemberTypes.Property)
					Property = (PropertyInfo) Member;
				else
					continue;

				if (Field != null) 
					TypeName = Field.FieldType.Name.ToLower();
				else
					TypeName = Property.PropertyType.Name.ToLower();

				string Fieldname = Member.Name.ToLower();
				
				string Value = null;
				if (this.ConfigSectionName == "")
                    Value = ConfigurationManager.AppSettings[Fieldname];
				else 
				{
                    NameValueCollection NVC = (NameValueCollection)ConfigurationManager.GetSection(this.ConfigSectionName);
					if (NVC != null)
						Value = NVC[Fieldname];
				}

				if (Value == null) 
				{
					MissingFields = true;
					continue;
				}

				// *** If we're encrypting decrypt any field that are encyrpted
				if (Value != "" && this.EncryptFieldList.IndexOf("," + Fieldname + ",") > -1 ) 
					Value = wwEncrypt.DecryptString(Value,this.EncryptKey);
			
				if (TypeName == "string")
					wwUtils.SetPropertyEx(this,Fieldname,Value);
				else if (TypeName.StartsWith("int") )
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToInt32(Value));
				else if (TypeName == "boolean") 
					if (Value.ToLower() == "true") 
						wwUtils.SetPropertyEx(this,Fieldname,true);
					else
						wwUtils.SetPropertyEx(this,Fieldname,false);
				else if (TypeName == "datetime")
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDateTime(Value));
				else if (TypeName == "decimal") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDecimal(Value));
				else if (TypeName == "byte") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToByte(Value));
				else if (TypeName == "float") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDouble(Value));
				else if (TypeName == "double") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDouble(Value));
				else if(Field != null  && Field.FieldType.IsEnum)
					wwUtils.SetPropertyEx(this,Fieldname,Enum.Parse(Field.FieldType,Value) );
				else if (Property != null  && Property.PropertyType.IsEnum)
					wwUtils.SetPropertyEx(this,Fieldname,Enum.Parse( Property.PropertyType,Value) );
			}

			// *** We have to write any missing keys
			if (MissingFields)
				this.WriteKeysToConfig();

		}

		/// <summary>
		/// Version of ReadKeysFromConfig that reads and writes an external Config file
		/// that is not controlled through the ConfigurationSettings class.
		/// </summary>
		/// <param name="Filename">The filename to read from. If the file doesn't exist it is created if permissions are available</param>
		public void ReadKeysFromConfig(string Filename) 
		{
			Type typeWebConfig = this.GetType();
			MemberInfo[] Fields = typeWebConfig.GetMembers(BindingFlags.Public | 
				BindingFlags.Instance );
		
			// *** Set a flag for missing fields
			// *** If we have any we'll need to write them out 
			bool MissingFields = false;

			XmlDocument Dom = new XmlDocument();
			
			try 
			{
				Dom.Load(Filename);
			}
			catch
			{
				this.WriteKeysToConfig(Filename);
				Dom.Load(Filename);
			}

			string ConfigSection = this.ConfigSectionName;
			if (ConfigSection == "") 
				ConfigSection = "appSettings";

			foreach(MemberInfo Member in Fields) 
			{
				string TypeName = null;
			
				FieldInfo Field = null;
				PropertyInfo Property = null;

				if (Member.MemberType == MemberTypes.Field)
					Field = (FieldInfo) Member;
				else if (Member.MemberType == MemberTypes.Property)
					Property = (PropertyInfo) Member;
				else
					continue;

				if (Field != null) 
					TypeName = Field.FieldType.Name.ToLower();
				else
					TypeName = Property.PropertyType.Name.ToLower();

				string Fieldname = Member.Name;
				
				string Value = this.GetNamedValueFromXml(Dom,Fieldname,ConfigSection);
				if (Value == null) 
				{
					MissingFields = true;
					continue;
				}

				Fieldname = Fieldname.ToLower();

				// *** If we're encrypting decrypt any field that are encyrpted
				if (Value != "" && this.EncryptFieldList.IndexOf("," + Fieldname + ",") > -1 ) 
					Value = wwEncrypt.DecryptString(Value,this.EncryptKey);
			
				if (TypeName == "string")
					wwUtils.SetPropertyEx(this,Fieldname,Value);
				else if (TypeName.StartsWith("int") )
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToInt32(Value));
				else if (TypeName == "boolean") 
					if (Value.ToLower() == "true") 
						wwUtils.SetPropertyEx(this,Fieldname,true);
					else
						wwUtils.SetPropertyEx(this,Fieldname,false);
				else if (TypeName == "datetime")
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDateTime(Value));
				else if (TypeName == "decimal") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDecimal(Value));
				else if (TypeName == "byte") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToByte(Value));
				else if (TypeName == "float") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDouble(Value));
				else if (TypeName == "double") 
					wwUtils.SetPropertyEx(this,Fieldname,Convert.ToDouble(Value));
				else if(Field != null  && Field.FieldType.IsEnum)
					wwUtils.SetPropertyEx(this,Fieldname,Enum.Parse(Field.FieldType,Value) );
				else if (Property != null  && Property.PropertyType.IsEnum)
					wwUtils.SetPropertyEx(this,Fieldname,Enum.Parse( Property.PropertyType,Value) );
			}

			// *** We have to write any missing keys
			if (MissingFields)
				this.WriteKeysToConfig(Filename);

		}

		private string GetNamedValueFromXml(XmlDocument Dom,string Key,string ConfigSection) 
		{
				XmlNode Node = Dom.SelectSingleNode(@"/configuration/" + ConfigSection + @"/add[@key='" + Key + "']");
				if (Node == null)
					return null;

				return Node.Attributes["value"].Value;
		}


		/// <summary>
		/// Writes all of the configuration file properties to the configuration file.
		/// 
		/// The keys are written into the standard .Config files (web.config or 
		/// YourApp.exe.config for example).
		/// 
		/// &lt;&lt;b&gt;&gt;Note: &lt;&lt;/b&gt;&gt;
		/// In Web Application updating this call causes the Web application to reload 
		/// itself as a change is made to web.config
		/// <seealso>Class wwAppConfiguration</seealso>
		/// </summary>
		/// <returns>Void</returns>
		public virtual bool WriteKeysToConfig() 
		{
			return WriteKeysToConfig( AppDomain.CurrentDomain.SetupInformation.ConfigurationFile );
		}

		/// <summary>
		/// Writes all of the configuration file properties to a specified 
		/// configuration file.
		/// 
		/// The format written is in standard .Config file format, but this method 
		/// allows writing out to a custom .Config file.
		/// <seealso>Class wwAppConfiguration</seealso>
		/// </summary>
		/// <param name="String Filename">
		/// Name of the file to write out
		/// </param>
		/// <returns>Void</returns>
		/// <example>
		/// // *** Overridden constructor
		/// public WebStoreConfig() : base(false)
		/// {
		///    this.SetEnryption("ConnectionString,MailPassword,
		///                      MerchantPassword","WebStorePassword");
		/// 
		///    // *** Use a custom Config file
		///    this.ReadKeysFromConfig(@"d:\projects\wwWebStore\MyConfig.config");
		/// }
		/// </example>
		public bool WriteKeysToConfig(string Filename) 
		{
			// *** Load the config file into DOM parser
			XmlDocument Dom = new XmlDocument();

			try 
			{
				Dom.Load(Filename);
			}
			catch 
			{
				string Xml = 
					@"<?xml version='1.0'?>
<configuration>
</configuration>";

				Dom.LoadXml(Xml);
			}

			// *** Parse through each of hte properties of the properties
			Type typeWebConfig = this.GetType();
			MemberInfo[] Fields = typeWebConfig.GetMembers();

			foreach(MemberInfo Field in Fields) 
			{
				// *** If we can't find the key - write it out to the document
				string Value = null;
				if (Field.MemberType == MemberTypes.Field)
                {
                    Object v = ((FieldInfo)Field).GetValue(this);
                    Value = v == null ? "" : v.ToString();
                }
                else if (Field.MemberType == MemberTypes.Property)
                {
                    Object v = ((PropertyInfo)Field).GetValue(this, null);
                    Value = v == null ? "" : v.ToString();
                }
                else
                    continue; // not a property or field

				// *** Encrypt the field if in list
				if (this.EncryptFieldList.IndexOf("," + Field.Name.ToLower() + ",") > -1) 
					Value = wwEncrypt.EncryptString(Value,this.EncryptKey);
				
				string ConfigSection = "appSettings";
				if (this.ConfigSectionName != "")
					ConfigSection = this.ConfigSectionName;

                string exp = @"/configuration/" + ConfigSection + @"/add[@key='" + Field.Name + "']";
                XmlNode Node = Dom.SelectSingleNode(exp);
                //XmlNode Node = Dom.SelectSingleNode(@"descendant::configuration:" + ConfigSection + @":add[key='" + Field.Name + "']");
                if (Node == null) 
				{
					// *** Create the node and attributes and write it
					Node = Dom.CreateNode(XmlNodeType.Element,"add",null);
					XmlAttribute Attr2 =Dom.CreateAttribute("key");
					Attr2.Value = Field.Name;
					XmlAttribute Attr = Dom.CreateAttribute("value");
					Attr.Value = Value;

					Node.Attributes.Append(Attr2);
					Node.Attributes.Append(Attr);

					XmlNode Parent = Dom.SelectSingleNode(@"/configuration/" + ConfigSection);
					if (Parent == null) 
						Parent = this.CreateConfigSection(Dom,ConfigSection);
					
					Parent.AppendChild(Node);
				}
				else 
				{
					// *** just write the value into the attribute
					Node.Attributes.GetNamedItem("value").Value =  Value;
				}


				string XML = Node.OuterXml;

			} // for each

			try 
			{
				Dom.Save(Filename);
			}
			catch 
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Creates a Configuration section and also creates a ConfigSections section for new 
		/// non appSettings sections.
		/// </summary>
		/// <param name="Dom"></param>
		/// <param name="ConfigSection"></param>
		/// <returns></returns>
		private XmlNode CreateConfigSection(XmlDocument Dom,string ConfigSection) 
		{
					
			XmlNode AppSettingsNode = Dom.CreateNode(XmlNodeType.Element,ConfigSection,null);
			XmlNode Parent = Dom.DocumentElement.AppendChild(AppSettingsNode);

			// *** Have to check for the Config Handler section
			if (ConfigSection != "appSettings") 
			{
				XmlNode ConfigSectionHeader = Dom.DocumentElement.SelectSingleNode(@"configSections");
				if (ConfigSectionHeader == null) 
				{
					// *** Create the node and attributes and write it
					XmlNode ConfigSectionNode = Dom.CreateNode(XmlNodeType.Element,"configSections",null);
					// *** Insert as first element in DOM
					ConfigSectionHeader = Dom.DocumentElement.InsertBefore(ConfigSectionNode,Dom.DocumentElement.ChildNodes[0]);
				}
							
				// *** Check for the Section
				XmlNode Section = ConfigSectionHeader.SelectSingleNode("section[@name='" + ConfigSection + "']");
				if (Section == null) 
				{
					Section = Dom.CreateNode(XmlNodeType.Element,"section",null);
					XmlAttribute Attr =Dom.CreateAttribute("name");
					Attr.Value = ConfigSection;
					XmlAttribute Attr2 = Dom.CreateAttribute("type");
					Attr2.Value = "System.Configuration.NameValueSectionHandler,System,Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
					Section.Attributes.Append(Attr);
					Section.Attributes.Append(Attr2);
					ConfigSectionHeader.AppendChild(Section);
				}
			}

			return Parent;
		}




	}

	public enum ConnectionTypes 
	{
		SqlServer
	}
}
