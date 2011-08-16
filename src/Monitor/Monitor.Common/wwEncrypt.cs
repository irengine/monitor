using System;
using System.Text;
using System.Security.Cryptography;

namespace Monitor.Common
{
	/// <summary>
	/// A simple encryption class that can be used to two-encode/decode strings and byte buffers
	/// with single method calls.
	/// </summary>
	public class wwEncrypt
	{
		/// <summary>
		/// Replace this value with some unique key of your own
		/// Best set this in your App start up in a Static constructor
		/// </summary>
		public static string Key = "0a1f131c";

		/// <summary>
		/// Encodes a stream of bytes using DES encryption with a pass key. Lowest level method that 
		/// handles all work.
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static byte[] EncryptBytes(byte[] InputString, string EncryptionKey) 
		{
			if (EncryptionKey == null)
				EncryptionKey = Key;

			TripleDESCryptoServiceProvider des =  new TripleDESCryptoServiceProvider();
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

			des.Key = hashmd5.ComputeHash(Encoding.ASCII.GetBytes(EncryptionKey));
			des.Mode = CipherMode.ECB;
			
			ICryptoTransform Transform = des.CreateEncryptor();

			byte[] Buffer = InputString;
			return Transform.TransformFinalBlock(Buffer,0,Buffer.Length);
		}
		
		/// <summary>
		/// Encrypts a string into bytes using DES encryption with a Passkey. 
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static byte[] EncryptBytes(string DecryptString, string EncryptionKey) 
		{
			return EncryptBytes(Encoding.ASCII.GetBytes(DecryptString),EncryptionKey);
		}

		/// <summary>
		/// Encrypts a string using Triple DES encryption with a two way encryption key.String is returned as Base64 encoded value
		/// rather than binary.
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static string EncryptString(string InputString, string EncryptionKey) 
		{
			return Convert.ToBase64String( EncryptBytes(Encoding.ASCII.GetBytes(InputString),EncryptionKey) );
		}


		
		/// <summary>
		/// Decrypts a Byte array from DES with an Encryption Key.
		/// </summary>
		/// <param name="DecryptBuffer"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static byte[] DecryptBytes(byte[] DecryptBuffer, string EncryptionKey) 
		{
			if (EncryptionKey == null)
				EncryptionKey = Key;

			TripleDESCryptoServiceProvider des =  new TripleDESCryptoServiceProvider();
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

			des.Key = hashmd5.ComputeHash(Encoding.ASCII.GetBytes(EncryptionKey));
			des.Mode = CipherMode.ECB;

			ICryptoTransform Transform = des.CreateDecryptor();
			
			return  Transform.TransformFinalBlock(DecryptBuffer,0,DecryptBuffer.Length);
		}
		
		public static byte[] DecryptBytes(string DecryptString, string EncryptionKey) 
		{	
				return DecryptBytes(Convert.FromBase64String(DecryptString),EncryptionKey);
		}

		/// <summary>
		/// Decrypts a string using DES encryption and a pass key that was used for 
		/// encryption.
		/// <seealso>Class wwEncrypt</seealso>
		/// </summary>
		/// <param name="DecryptString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns>String</returns>
		public static string DecryptString(string DecryptString, string EncryptionKey) 
		{
			try 
			{
				return Encoding.ASCII.GetString( DecryptBytes(Convert.FromBase64String(DecryptString),EncryptionKey));
			}
			catch { return ""; }  // Probably not encoded
		}
	}
}
