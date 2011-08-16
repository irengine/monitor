using System;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.Win32;

using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

using System.Text.RegularExpressions;
using System.Web;

namespace Monitor.Common
{

	/// <summary>
	/// wwUtils class which contains a set of common utility classes for 
	/// Formatting strings
	/// Reflection Helpers
	/// Object Serialization
	/// </summary>
	public class wwUtils
	{

		#region String Helper Functions

		/// <summary>
		/// Replaces and  and Quote characters to HTML safe equivalents.
		/// </summary>
		/// <param name="lcHTML">HTML to convert</param>
		/// <returns>Returns an HTML string of the converted text</returns>
		public static string FixHTMLForDisplay( string lcHTML )
		{
			lcHTML = lcHTML.Replace("<","&lt;");
			lcHTML = lcHTML.Replace(">","&gt;");
			lcHTML = lcHTML.Replace("\"","&quote;");
			return lcHTML;
		}

		/// <summary>
		/// Fixes a plain text field for display as HTML by replacing carriage returns 
		/// with the appropriate br and p tags for breaks.
		/// </summary>
		/// <param name="String Text">Input string</param>
		/// <returns>Fixed up string</returns>
		public static string DisplayMemo(string Text) 
		{				
			Text = Text.Replace("\r\n","\r");
			Text = Text.Replace("\n","\r");
			Text = Text.Replace("\r\r","<p>");
			Text = Text.Replace("\r","<br>");
			return Text;
		}

		/// <summary>
		/// Extracts a string from between a pair of delimiters. Only the first 
		/// instance is found.
		/// </summary>
		/// <param name="Source">Input String to work on</param>
		/// <param name="StartDelim">Beginning delimiter</param>
		/// <param name="EndDelim">ending delimiter</param>
		/// <param name="CaseInsensitive">Determines whether the search for delimiters is case sensitive</param>
		/// <returns>Extracted string or ""</returns>
		public static string ExtractString(string Source, string BeginDelim, string EndDelim, bool CaseInSensitive) 
		{
			int At1, At2;

			
			if (CaseInSensitive) 
			{
				At1 = Source.IndexOf(BeginDelim);
				At2 = Source.IndexOf(EndDelim,At1+ BeginDelim.Length );
			}
			else 
			{
				string Lower = Source.ToLower();
				At1 =Lower.IndexOf( BeginDelim.ToLower() );
				At2 = Lower.IndexOf( EndDelim.ToLower(),At1+ BeginDelim.Length);
			}
			  
			if (At1 > -1 && At2 > 1) 
			{
				return Source.Substring(At1 + BeginDelim.Length,At2-At1 - BeginDelim.Length);
			}

			return "";
		}

		/// <summary>
		/// Extracts a string from between a pair of delimiters. Only the first 
		/// instance is found. Search is case insensitive.
		/// </summary>
		/// <param name="Source">
		/// Input String to work on
		/// </param>
		/// <param name="StartDelim">
		/// Beginning delimiter
		/// </param>
		/// <param name="EndDelim">
		/// ending delimiter
		/// </param>
		/// <returns>Extracted string or ""</returns>
		public static string ExtractString(string Source, string BeginDelim, string EndDelim) 
		{
			return wwUtils.ExtractString(Source,BeginDelim,EndDelim,false);
		}
	

		/// <summary>
		/// Determines whether a string is empty (null or zero length)
		/// </summary>
		/// <param name="String">Input string</param>
		/// <returns>true or false</returns>
		public static bool Empty(string String) 
		{
			if (String == null || String.Trim().Length == 0)
				return true;

			return false;
		}
		
		/// <summary>
		/// Determines wheter a string is empty (null or zero length)
		/// </summary>
		/// <param name="StringValue">Input string (in object format)</param>
		/// <returns>true or false/returns>
		public static bool Empty(object StringValue) 
		{
			string String = (string) StringValue;
			if ( String == null || String.Trim().Length == 0)
				return true;
			
			return false;
		}

		/// <summary>
		/// Returns an abstract of the provided text by returning up to Length characters
		/// of a text string. If the text is truncated a ... is appended.
		/// </summary>
		/// <param name="Text">Text to abstract</param>
		/// <param name="Length">Number of characters to abstract to</param>
		/// <returns>string</returns>
		public static string TextAbstract(string Text, int Length) 
		{
			if (Text.Length <= Length)
				return Text;

			Text = Text.Substring(0,Length);

			Text = Text.Substring(0,Text.LastIndexOf(" "));
			return Text + "..."; 
		}

		/// <summary>
		/// Parses the text of a Soap Exception and returns just the error message text
		/// Ideally you'll want to have a SoapException fire on the server, otherwise
		/// this method will try to parse out the inner exception error message.
		/// </summary>
		/// <param name="SoapExceptionText"></param>
		/// <returns></returns>
		public static string ParseSoapExceptionText(string SoapExceptionText) 
		{
			string Message = wwUtils.ExtractString(SoapExceptionText,"SoapException: ","\n");
			if (Message != "")
				return Message;

			Message = wwUtils.ExtractString(SoapExceptionText,"SoapException: "," --->");
			if (Message == "Server was unable to process request.") 
			{
				Message = wwUtils.ExtractString(SoapExceptionText,"-->","\n");
				Message = Message.Substring(Message.IndexOf(":")+1);
			}

			if (Message == "")
				return "An error occurred on the server.";

			return Message;
		}
		#endregion

	

		#region Reflection Helper Code
		/// <summary>
		/// Binding Flags constant to be reused for all Reflection access methods.
		/// </summary>
		public const BindingFlags MemberAccess = 
			BindingFlags.Public | BindingFlags.NonPublic | 
			BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase ;



		/// <summary>
		/// Retrieve a dynamic 'non-typelib' property
		/// </summary>
		/// <param name="Object">Object to make the call on</param>
		/// <param name="Property">Property to retrieve</param>
		/// <returns></returns>
		public static object GetProperty(object Object,string Property)
		{
			return Object.GetType().GetProperty(Property,wwUtils.MemberAccess).GetValue(Object,null);
		}

		/// <summary>
		/// Retrieve a dynamic 'non-typelib' field
		/// </summary>
		/// <param name="Object">Object to retreve Field from</param>
		/// <param name="Property">name of the field to retrieve</param>
		/// <returns></returns>
		public static object GetField(object Object,string Property)
		{
			return Object.GetType().GetField(Property,wwUtils.MemberAccess).GetValue(Object);
		}

		/// <summary>
		/// Returns a property or field value using a base object and sub members including . syntax.
		/// For example, you can access: this.oCustomer.oData.Company with (this,"oCustomer.oData.Company")
		/// </summary>
		/// <param name="Parent">Parent object to 'start' parsing from.</param>
		/// <param name="Property">The property to retrieve. Example: 'oBus.oData.Company'</param>
		/// <returns></returns>
		public static object GetPropertyEx(object Parent, string Property) 
		{
			MemberInfo Member = null;

			Type Type = Parent.GetType();

			int lnAt = Property.IndexOf(".");
			if ( lnAt < 0) 
			{
				if (Property == "this" || Property == "me")
					return Parent;

				// *** Get the member
				Member = Type.GetMember(Property,wwUtils.MemberAccess)[0];
				if (Member.MemberType == MemberTypes.Property )
					return ((PropertyInfo) Member).GetValue(Parent,null);
				else
					return ((FieldInfo) Member).GetValue(Parent);
			}

			// *** Walk the . syntax - split into current object (Main) and further parsed objects (Subs)
			string Main = Property.Substring(0,lnAt);
			string Subs = Property.Substring(lnAt+1);

			// *** Retrieve the current property
			Member = Type.GetMember(Main,wwUtils.MemberAccess)[0];

			object Sub;
			if (Member.MemberType == MemberTypes.Property )
			{
				// *** Get its value
				Sub = ((PropertyInfo) Member).GetValue(Parent,null);

			}
			else
			{
				Sub = ( (FieldInfo) Member).GetValue(Parent);

			}

			// *** Recurse further into the sub-properties (Subs)
			return wwUtils.GetPropertyEx(Sub,Subs);
		}

		/// <summary>
		/// Sets the property on an object.
		/// </summary>
		/// <param name="Object">Object to set property on</param>
		/// <param name="Property">Name of the property to set</param>
		/// <param name="Value">value to set it to</param>
		public static void SetProperty(object Object,string Property,object Value)
		{
			Object.GetType().GetProperty(Property,wwUtils.MemberAccess).SetValue(Object,Value,null);
		}

		/// <summary>
		/// Sets the field on an object.
		/// </summary>
		/// <param name="Object">Object to set property on</param>
		/// <param name="Property">Name of the field to set</param>
		/// <param name="Value">value to set it to</param>
		public static void SetField(object Object,string Property,object Value)
		{
			Object.GetType().GetField(Property,wwUtils.MemberAccess).SetValue(Object,Value);
		}

		/// <summary>
		/// Sets the value of a field or property via Reflection. This method alws 
		/// for using '.' syntax to specify objects multiple levels down.
		/// 
		/// wwUtils.SetPropertyEx(this,"Invoice.LineItemsCount",10)
		/// 
		/// which would be equivalent of:
		/// 
		/// this.Invoice.LineItemsCount = 10;
		/// </summary>
		/// <param name="Object Parent">
		/// Object to set the property on.
		/// </param>
		/// <param name="String Property">
		/// Property to set. Can be an object hierarchy with . syntax.
		/// </param>
		/// <param name="Object Value">
		/// Value to set the property to
		/// </param>
		public static object SetPropertyEx(object Parent, string Property,object Value) 
		{
			Type Type = Parent.GetType();
			MemberInfo Member = null;

			// *** no more .s - we got our final object
			int lnAt = Property.IndexOf(".");
			if ( lnAt < 0) 
			{
				Member = Type.GetMember(Property,wwUtils.MemberAccess)[0];
				if ( Member.MemberType == MemberTypes.Property ) 
				{

					((PropertyInfo) Member).SetValue(Parent,Value,null);
					return null;
				}
				else 
				{
					((FieldInfo) Member).SetValue(Parent,Value);
					return null;				   
				}
			}	

			// *** Walk the . syntax
			string Main = Property.Substring(0,lnAt);
			string Subs = Property.Substring(lnAt+1);
			Member = Type.GetMember(Main,wwUtils.MemberAccess)[0];

			object Sub;
			if (Member.MemberType == MemberTypes.Property)
				Sub = ((PropertyInfo) Member).GetValue(Parent,null);
			else
				Sub = ((FieldInfo) Member).GetValue(Parent);

			// *** Recurse until we get the lowest ref
			SetPropertyEx(Sub,Subs,Value);
			return null;
		}

		/// <summary>
		/// Wrapper method to call a 'dynamic' (non-typelib) method
		/// on a COM object
		/// </summary>
		/// <param name="Params"></param>
		/// 1st - Method name, 2nd - 1st parameter, 3rd - 2nd parm etc.
		/// <returns></returns>
		public static object CallMethod(object Object,string Method, params object[] Params)
		{

			return Object.GetType().InvokeMember(Method,wwUtils.MemberAccess | BindingFlags.InvokeMethod,null,Object,Params);
			//return Object.GetType().GetMethod(Method,wwUtils.MemberAccess | BindingFlags.InvokeMethod).Invoke(Object,Params);
		}

		#endregion

		#region Object Serialization routines
		/// <summary>
		/// Returns a string of all the field value pairs of a given object.
		/// Works only on non-statics.
		/// </summary>
		/// <param name="Obj"></param>
		/// <param name="Separator"></param>
		/// <returns></returns>
		public static string ObjectToString(object Obj, string Separator) 
		{
			FieldInfo[] fi = Obj.GetType().GetFields();
			
			string lcOutput = "";
			foreach (FieldInfo Field in fi) 
			{
				lcOutput = lcOutput + Field.Name + ": " + Field.GetValue(Obj).ToString() + Separator;
			}
			return lcOutput;
		}

		/// <summary>
		/// Serializes an object instance to a file.
		/// </summary>
		/// <param name="Instance">the object instance to serialize</param>
		/// <param name="Filename"></param>
		/// <param name="BinarySerialization">determines whether XML serialization or binary serialization is used</param>
		/// <returns></returns>
		public static bool SerializeObject(object Instance, string Filename, bool BinarySerialization) 
		{
			bool retVal = true;

			if (!BinarySerialization) 
			{
				XmlTextWriter writer = null;
				try
				{
					XmlSerializer serializer = 
						new XmlSerializer(Instance.GetType());
		
					// Create an XmlTextWriter using a FileStream.
					Stream fs = new FileStream(Filename, FileMode.Create);
					writer = 	new XmlTextWriter(fs, new UTF8Encoding());
					writer.Formatting = Formatting.Indented;
					writer.IndentChar = ' ';
					writer.Indentation = 3;
						
					// Serialize using the XmlTextWriter.
					serializer.Serialize(writer,Instance);
				}
				catch(Exception) 
				{
					retVal = false;
				}
				finally
				{
					if (writer != null)
						writer.Close();
				}
			}
			else 
			{
				Stream fs = null;
				try
				{
					BinaryFormatter serializer = new BinaryFormatter();
					fs = new FileStream(Filename, FileMode.Create);
					serializer.Serialize(fs,Instance);
				}
				catch 
				{
					retVal = false;
				}
				finally
				{
					if (fs != null)
						fs.Close();
				}
			}
		
			return retVal;
		}

		/// <summary>
		/// Deserializes an object from file and returns a reference.
		/// </summary>
		/// <param name="Filename">name of the file to serialize to</param>
		/// <param name="ObjectType">The Type of the object. Use typeof(yourobject class)</param>
		/// <param name="BinarySerialization">determines whether we use Xml or Binary serialization</param>
		/// <returns>Instance of the deserialized object or null. Must be cast to your object type</returns>
		public static object DeSerializeObject(string Filename,Type ObjectType,bool BinarySerialization) 
		{
			object Instance = null;
			
			if (!BinarySerialization) 
			{

				XmlReader reader = null;
				XmlSerializer serializer = null;
				FileStream fs = null;
				try 
				{
					// Create an instance of the XmlSerializer specifying type and namespace.
					serializer = new XmlSerializer(ObjectType);

					// A FileStream is needed to read the XML document.
					fs = new FileStream(Filename, FileMode.Open);
					reader = new XmlTextReader(fs);
				
					Instance = serializer.Deserialize(reader);

				}
				catch 
				{
					return null;
				}
				finally
				{
					if (fs != null)
						fs.Close();

					if (reader != null)
						reader.Close();
				}
			}
			else 
			{

				BinaryFormatter serializer = null;
				FileStream fs = null;

				try 
				{
					serializer = new BinaryFormatter();
					fs = new FileStream(Filename, FileMode.Open);
					Instance = serializer.Deserialize(fs);

				}
				catch 
				{
					return null;
				}
				finally
				{
					if (fs != null)
						fs.Close();
				}
			}

			return Instance;
		}
		#endregion

		#region Miscellaneous Routines 
		/// <summary>
		/// Returns the logon password stored in the registry if Auto-Logon is used.
		/// This function is used privately for demos when I need to specify a login username and password.
		/// </summary>
		/// <param name="GetUserName"></param>
		/// <returns></returns>
		public static string GetSystemPassword(bool GetUserName) 
		{
			RegistryKey RegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon");
			if (RegKey == null)
				return "";
			
			string Password;
			if (!GetUserName)
				Password = (string) RegKey.GetValue("DefaultPassword");
			else
				Password = (string) RegKey.GetValue("DefaultUsername");

			if (Password == null) 
				return "";

			return (string) Password;
		}
		#endregion


		[DllImport("Shell32.dll")]
		private static extern int ShellExecute(int hwnd, string lpOperation, 
			string lpFile, string lpParameters, 
			string lpDirectory, int nShowCmd);

		/// <summary>
		/// Uses the Shell Extensions to launch a program based or URL moniker.
		/// </summary>
		/// <param name="lcUrl">Any URL Moniker that the Windows Shell understands (URL, Word Docs, PDF, Email links etc.)</param>
		/// <returns></returns>
		public static int GoUrl(string lcUrl)
		{
			string lcTPath = Path.GetTempPath();

			string Username = Environment.UserName;
			

			int lnResult = ShellExecute(0,"OPEN",lcUrl, "",lcTPath,1);
			return lnResult;
		}

		/// <summary>
		/// Displays an HTML string in a browser window
		/// </summary>
		/// <param name="HtmlString"></param>
		/// <returns></returns>
		public static int ShowHtml(string HtmlString) 
		{
			string File = Path.GetTempPath() + "\\__preview.htm";

			StreamWriter sw = new StreamWriter(File,false);
			sw.Write(HtmlString);
			sw.Close();

			return GoUrl(File);
		}
		/// <summary>
		/// Displays a large Text string as a text file.
		/// </summary>
		/// <param name="TextString"></param>
		/// <returns></returns>
		public static int ShowText(string TextString) 
		{
			string File = Path.GetTempPath() + "\\__preview.txt";

			StreamWriter sw = new StreamWriter(File,false);
			sw.Write(TextString);
			sw.Close();

			return GoUrl(File);
		}


	}

}



