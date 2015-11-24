using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Winterdom.IO.FileMap;

namespace winterdom_filemap
{
	public class MemoryMappedFilesEditor<T>
	{
		private Mutex _mutex;
		private bool _mutexCreated = false;
		private readonly string _fileName;
		
		public MemoryMappedFilesEditor(string fileName)
		{
			_fileName = fileName;
			CreateOrOpenMutex();
		}
		
		private void CreateOrOpenMutex()
		{
			try
			{
				_mutex = Mutex.OpenExisting(_fileName+"Mutex");
				_mutexCreated = true;
			}
			catch
			{
				try
				{
					_mutex = new Mutex(false, _fileName + "Mutex", out _mutexCreated);
				}
				catch
				{
					_mutexCreated = false;
				}
			}
		}
		
		public void WriteOnMemory(T data)
		{
			if (_mutexCreated == false)
			{
				return;
			}
			
			WriteData(data);
		}
		
		private void WriteData(T data)
		{
			Stream writer = null;
			
			try
			{
				_mutex.WaitOne();
				writer = OpenOrCreateMemoryMapFile().MapView(MapAccess.FileMapWrite, 0, 8 * 1024);
				
				char[] charArrayOfObject = serializeToXml(data).ToCharArray();
				for (int i = 0; i < charArrayOfObject.Length; i++)
				{
					writer.WriteByte((byte)charArrayOfObject[i]);
				}
			}
			finally
			{
				if (writer != null)
				{
					writer.Close();
				}
				
				_mutex.ReleaseMutex();
			}
		}
		
		public T ReadFromMemory()
		{
			if (_mutexCreated == false)
			{
				return default(T);
			}
			
			return ReadData();
		}
		
		//Read 
		private T ReadData()
		{
			Stream _reader = null;
			try
			{
				_mutex.WaitOne();
				_reader = OpenOrCreateMemoryMapFile().MapView(MapAccess.FileMapRead, 0, 8 * 1024);
				return GetObjectFromMmf(_reader);
			}
			finally
			{
				if (_reader != null)
				{
					_reader.Close();
					//OpenOrCreateMemoryMapFile().Close();
				}
				
				_mutex.ReleaseMutex();
			}
		}
		
		private T GetObjectFromMmf(Stream _reader)
		{
			int a;
			List<char> d = new List<char>();
			
			while (true)
			{
				a = _reader.ReadByte();
				if (a <= 0)
				{
					break;
				}
				d.Add((char)a);
			}
			try
			{
				return deserializeFromXML<T>(new string(d.ToArray()));
			}
			catch
			{
				return default(T);
			}
			
			
		}
		
		private MemoryMappedFile OpenOrCreateMemoryMapFile()
		{
			try
			{
				return MemoryMappedFile.Open(MapAccess.FileMapAllAccess, _fileName);
			}
			catch
			{
				return MemoryMappedFile.Create(MapProtection.PageReadWrite, 8 * 1024, _fileName);
			}
			
		}
		
		private string serializeToXml<T>(T data)
		{
			XmlSerializer serializer = new XmlSerializer(data.GetType());
			StringWriter stringWriter = new StringWriter();
			serializer.Serialize(stringWriter, data);
			
			string xmlData = stringWriter.ToString();
			//Debug.WriteLine("\n\nWriting" + xmlData);
			stringWriter.Close();
			
			return xmlData;
		}
		
		private T deserializeFromXML<T>(string xmlData)
		{
			//Debug.WriteLine("\n\nReading" + xmlData);
			XmlSerializer deserializer = new XmlSerializer(typeof(T));
			StringReader stringReader = new StringReader(xmlData);
			T data = (T)deserializer.Deserialize(stringReader);
			stringReader.Close();
			
			return data;
		}
	}
}
