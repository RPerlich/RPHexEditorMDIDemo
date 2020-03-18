using System;
using System.Collections.Generic;
using System.IO;

namespace RPHexEditor
{
	internal abstract class BytePart
	{
		public abstract long Length { get; }
		public abstract void RemoveBytes(long index, long length, BytePartList bytePartList);
	}

	internal class BytePartList : LinkedList<BytePart>
	{ }

	internal sealed class MemoryBytePart : BytePart
    {
        private byte[] _bytes;

		public MemoryBytePart(byte value)
        {
            _bytes = new byte[] { value };
        }

		public MemoryBytePart(byte[] value)
        {
			_bytes = value.Clone() as byte[];
        }

		public byte[] Bytes
		{
			get { return _bytes; }
		}

		public override long Length
		{
			get	{ return _bytes.LongLength; }
		}

		public void AddByteToStart(byte value)
		{
			byte[] newBytes = new byte[_bytes.LongLength + 1];
			newBytes[0] = value;
			_bytes.CopyTo(newBytes, 1);
			_bytes = newBytes;
		}

		public void AddByteToEnd(byte value)
		{
			byte[] newBytes = new byte[_bytes.LongLength + 1];
			_bytes.CopyTo(newBytes, 0);
			newBytes[newBytes.LongLength - 1] = value;
			_bytes = newBytes;
		}

		public void InsertBytes(long index, byte[] value)
		{
			byte[] newBytes = new byte[_bytes.LongLength + value.LongLength];
			
			if (index > 0)
				Array.Copy(_bytes, 0, newBytes, 0, index);

			Array.Copy(value, 0, newBytes, index, value.LongLength);
			
			if (index < _bytes.LongLength)
				Array.Copy(_bytes, index, newBytes, index + value.LongLength, _bytes.LongLength - index);

			_bytes = newBytes;
		}

		public override void RemoveBytes(long index, long length, BytePartList bytePartList)
		{
			byte[] newBytes = new byte[_bytes.LongLength - length];

			if (index > 0)
				Array.Copy(_bytes, 0, newBytes, 0, index);

			if (index + length < _bytes.LongLength)
				Array.Copy(_bytes, index + length, newBytes, index, newBytes.LongLength - index);

			_bytes = newBytes;
		}
	}

	internal sealed class FileBytePart : BytePart
	{
		private long _filePartLength;
		private long _filePartIndex;

		public FileBytePart(long index, long length)
		{
			_filePartIndex = index;
			_filePartLength = length;
		}

		public long FilePartIndex
		{
			get { return _filePartIndex; }
		}

		public override long Length
		{
			get { return _filePartLength; }
		}

		public void SetFilePartIndex(long value)
		{
			_filePartIndex = value;
		}

		public void RemoveBytesFromEnd(long length)
		{
			if (length > _filePartLength)
				throw new ArgumentOutOfRangeException("length", "FileBytePart.RemoveBytesFromEnd: No more elements can be removed than are present.");

			_filePartLength -= length;
		}

		public void RemoveBytesFromStart(long length)
		{
			if (length > _filePartLength)
				throw new ArgumentOutOfRangeException("length", "FileBytePart.RemoveBytesFromStart: No more elements can be removed than are present.");

			_filePartIndex += length;
			_filePartLength -= length;
		}

		public override void RemoveBytes(long index, long length, BytePartList bytePartList)
		{
			if (bytePartList == null)
				throw new ArgumentNullException("bytePartList", "FileBytePart.RemoveBytes: The parameter cannot be NULL.");

			if (index > _filePartLength)
				throw new ArgumentOutOfRangeException("index", "FileBytePart.RemoveBytes: The index cannot be greater than the length.");

			if (index + length > _filePartLength)
				throw new ArgumentOutOfRangeException("length", "FileBytePart.RemoveBytes: No more elements can be removed than are present.");

			long startIndex = _filePartIndex;
			long startLength = index;

			long endIndex = _filePartIndex + index + length;
			long endLength = _filePartLength - length - startLength;


			if (startLength > 0 && endLength > 0)
			{
				_filePartIndex = startIndex;
				_filePartLength = startLength;

				LinkedListNode<BytePart> bp = bytePartList.Find(this);
				bytePartList.AddAfter(bp, new FileBytePart(endIndex, endLength));
			}
			else
			{
				if (startLength > 0)
				{
					_filePartIndex = startIndex;
					_filePartLength = startLength;
				}
				else
				{
					_filePartIndex = endIndex;
					_filePartLength = endLength;
				}
			}
		}
	}

	public interface IByteData
	{
		byte ReadByte(long index);
		byte ReadByte(long index, out bool isChanged);
		void WriteByte(long index, byte value, bool track_Change = true);
		void InsertBytes(long index, byte[] value, bool track_Change = true);
		void DeleteBytes(long index, long length, bool track_Change = true);
		long Length { get; }
		bool IsReadOnly { get; }
		bool IsChanged();
		void CommitChanges();
		void Undo();
		bool IsUndoAvailable { get; }

		event EventHandler DataChanged;
		event EventHandler DataLengthChanged;
	}

	public sealed class FileByteData : IByteData, IDisposable
	{
		Stream _fileDataStream = null;
		BytePartList _bytePartList = null;
		UndoList _undoList = null;
		const int BLOCK_SIZE = 4096;

		public FileByteData()
		{
			FileName = Path.GetTempFileName();
			ReadOnly = false;

			_fileDataStream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite);

			InitFileByteData();
		}

		public FileByteData(string fileName, bool readOnly = false)
		{
			FileName = fileName;
			ReadOnly = readOnly;

			try
			{
				_fileDataStream = File.Open(fileName, FileMode.Open,
									ReadOnly ? FileAccess.Read : FileAccess.ReadWrite,
									ReadOnly ? FileShare.ReadWrite : FileShare.Read);
			}
			catch (IOException)
			{
				ReadOnly = true;
				_fileDataStream = File.Open(fileName, FileMode.Open,
									ReadOnly ? FileAccess.Read : FileAccess.ReadWrite,
									ReadOnly ? FileShare.ReadWrite : FileShare.Read);
			}

			InitFileByteData();
		}

		public FileByteData(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream", "FileByteData.FileByteData: The parameter cannot be NULL.");

			if (!stream.CanSeek)
				throw new ArgumentException("stream", "FileByteData.FileByteData: The stream does not support search operations.");

			_fileDataStream = stream;
			ReadOnly = !stream.CanWrite;
			
			InitFileByteData();
		}

		~FileByteData()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (_fileDataStream != null)
			{
				_fileDataStream.Dispose();
				_fileDataStream = null;
			}
			
			GC.SuppressFinalize(this);
		}

		void InitFileByteData()
		{
			_bytePartList = new BytePartList();
			_bytePartList.AddFirst(new FileBytePart(0, _fileDataStream.Length));
			Length = _fileDataStream.Length;
			_undoList = new UndoList();
		}

		public string FileName { get; } = string.Empty;

		public bool ReadOnly { get; } = false;

		public long Length { get; private set; } = 0;

		public bool IsReadOnly
		{
			get { return ReadOnly; }
		}

		public bool IsChanged()
		{
			if (ReadOnly)
				return false;

			if (Length != _fileDataStream.Length)
				return true;

			long index = 0;

			for (LinkedListNode<BytePart> bp = _bytePartList.First; bp != null; bp = bp.Next)
			{
				FileBytePart fileBytePart = bp.Value as FileBytePart;

				if (fileBytePart == null || fileBytePart.FilePartIndex != index)
					return true;

				index += fileBytePart.Length;
			}

			return (index != _fileDataStream.Length);
		}

		public void CommitChanges()
		{
			if (ReadOnly)
				throw new OperationCanceledException("FileByteData.CommitChanges: The data cannot be saved when the file is in read only mode.");
			
			if (Length > _fileDataStream.Length)
				_fileDataStream.SetLength(Length);

			long index = 0;

			// run through the whole bytePart list
			for (LinkedListNode<BytePart> bp = _bytePartList.First; bp != null; bp = bp.Next)
			{
				// try to convert it to a FileBytePart
				FileBytePart fileBytePart = bp.Value as FileBytePart;

				// if  it's a FileBytePart and the orig file position is different, move the FileBytePart to the new (expected) position
				if (fileBytePart != null && fileBytePart.FilePartIndex != index)
					RearrangeFileBytePart(fileBytePart, index);

				// increment the index to the next expected BytePart position for file - and memory parts
				index += bp.Value.Length;
			}

			index = 0;

			// run through the whole bytePart list again, now look at the memory parts
			for (LinkedListNode<BytePart> bp = _bytePartList.First; bp != null; bp = bp.Next)
			{
				// try to convert it to a MemoryBytePart
				MemoryBytePart memoryBytePart = bp.Value as MemoryBytePart;

				// if it's a MemoryBytePart
				if (memoryBytePart != null)
				{
					_fileDataStream.Position = index;

					// write the MemoryBytePart into the stream
					for (int memoryIndex = 0; memoryIndex < memoryBytePart.Length; memoryIndex += BLOCK_SIZE)
						_fileDataStream.Write(memoryBytePart.Bytes, memoryIndex, (int)Math.Min(BLOCK_SIZE, memoryBytePart.Length - memoryIndex));
				}

				// increment the index to the next expected BytePart position for file - and memory parts
				index += bp.Value.Length;
			}

			_fileDataStream.Flush();

			InitFileByteData();
		}

		FileBytePart GetNextFileBytePart(BytePart bytePart, long index, out long nextIndex)
		{
			FileBytePart nextFileBytePart = null;
			nextIndex = index + bytePart.Length;

			LinkedListNode<BytePart> bp = _bytePartList.Find(bytePart);

			// run through the whole bytePart list from the current position + 1, to find the next FileBytePart
			for (LinkedListNode<BytePart> bp1 = bp.Next; bp1 != null; bp1 = bp1.Next)
			{
				FileBytePart fileBytePart = bp1.Value as FileBytePart;
			
				if (fileBytePart != null)
				{
					nextFileBytePart = fileBytePart;
					break;
				}

				nextIndex += bp1.Value.Length;
			}

			return nextFileBytePart;
		}

		void RearrangeFileBytePart(FileBytePart bytePart, long index)
		{
			long nextIndex = 0;
			byte[] buffer = new byte[BLOCK_SIZE];

			FileBytePart nextFileBlock = GetNextFileBytePart(bytePart, index, out nextIndex);

			if (nextFileBlock != null && index + bytePart.Length > nextFileBlock.FilePartIndex)
				RearrangeFileBytePart(nextFileBlock, nextIndex);

			if (bytePart.FilePartIndex > index)
			{
				Array.Clear(buffer, 0, buffer.Length);

				for (long relIndex = 0; relIndex < bytePart.Length; relIndex += buffer.Length)
				{
					int bytesToRead = (int)Math.Min(buffer.Length, bytePart.Length - relIndex);
					_fileDataStream.Position = bytePart.FilePartIndex + relIndex;
					_fileDataStream.Read(buffer, 0, bytesToRead);
					_fileDataStream.Position = index + relIndex;
					_fileDataStream.Write(buffer, 0, bytesToRead);
				}
			}
			else
			{
				Array.Clear(buffer, 0, buffer.Length);

				for (long relIndex = 0; relIndex < bytePart.Length; relIndex += buffer.Length)
				{
					int bytesToRead = (int)Math.Min(buffer.Length, bytePart.Length - relIndex);
					long readOffset = bytePart.FilePartIndex + bytePart.Length - relIndex - bytesToRead;
					_fileDataStream.Position = readOffset;
					_fileDataStream.Read(buffer, 0, bytesToRead);

					long writeOffset = index + bytePart.Length - relIndex - bytesToRead;
					_fileDataStream.Position = writeOffset;
					_fileDataStream.Write(buffer, 0, bytesToRead);
				}
			}

			bytePart.SetFilePartIndex(index);
		}

		public event EventHandler DataLengthChanged;
		public event EventHandler DataChanged;

		void OnDataChanged(EventArgs e)
		{
			if (DataChanged != null) DataChanged(this, e);
		}

		void OnDataLengthChanged(EventArgs e)
		{
			if (DataLengthChanged != null) DataLengthChanged(this, e);
		}

		BytePart BytePartGetInfo(long index, out long startIndex)
		{
			BytePart bytePart = null;
			startIndex = 0;

			if (index < 0 || index > Length)
				throw new ArgumentOutOfRangeException("index", "FileByteData.BytePartGetInfo: The parameter is outside the file limits.");

			for (LinkedListNode<BytePart> bp = _bytePartList.First; bp != null; bp = bp.Next)
			{
				if ((startIndex <= index && startIndex + bp.Value.Length > index) || bp.Next == null)
				{
					bytePart = bp.Value as BytePart;
					break;
				}

				startIndex += bp.Value.Length;
			}

			return bytePart;
		}

		public byte ReadByte(long index)
		{
			long startIndex = 0;

			BytePart bytePart = BytePartGetInfo(index, out startIndex);
			
			FileBytePart fileBytePart = bytePart as FileBytePart;
			MemoryBytePart memoryBytePart = bytePart as MemoryBytePart;

			if (fileBytePart != null)
			{
				if (_fileDataStream.Position != fileBytePart.FilePartIndex + index - startIndex)
					_fileDataStream.Position = fileBytePart.FilePartIndex + index - startIndex;

				return (byte)_fileDataStream.ReadByte();
			}
			
			if (memoryBytePart != null)
				return memoryBytePart.Bytes[index - startIndex];
			else
				throw new ArgumentNullException("index", "FileByteData.ReadByte: The internal BytePartList is corrupt.");
		}

		public byte ReadByte(long index, out bool isChanged)
		{
			long startIndex = 0;
			isChanged = false;

			BytePart bytePart = BytePartGetInfo(index, out startIndex);

			FileBytePart fileBytePart = bytePart as FileBytePart;
			MemoryBytePart memoryBytePart = bytePart as MemoryBytePart;

			if (fileBytePart != null)
			{
				if (_fileDataStream.Position != fileBytePart.FilePartIndex + index - startIndex)
					_fileDataStream.Position = fileBytePart.FilePartIndex + index - startIndex;

				return (byte)_fileDataStream.ReadByte();
			}

			if (memoryBytePart != null)
			{
				isChanged = true;
				return memoryBytePart.Bytes[index - startIndex];
			}
			else
				throw new ArgumentNullException("index", "FileByteData.ReadByte: The internal BytePartList is corrupt.");
		}

		public void WriteByte(long index, byte value, bool track_Change = true)
		{
			long startIndex = 0;
			byte oldValue = ReadByte(index);

			if (track_Change)
				_undoList.Add(new UndoAction(new byte[] { oldValue }, index, mod_type.mod_replace));

			BytePart bytePart = BytePartGetInfo(index, out startIndex);

			FileBytePart fileBytePart = bytePart as FileBytePart;
			MemoryBytePart memoryBytePart = bytePart as MemoryBytePart;

			if (memoryBytePart != null)
			{
				memoryBytePart.Bytes[index - startIndex] = value;
				OnDataChanged(EventArgs.Empty);
				
				return;
			}

			if (fileBytePart != null)
			{
				LinkedListNode<BytePart> bp = _bytePartList.Find(bytePart);
				
				if (index == startIndex && bp.Previous != null)
				{
					MemoryBytePart prevMemoryBytePart = bp.Previous.Value as MemoryBytePart;
					
					if (prevMemoryBytePart != null)
					{
						prevMemoryBytePart.AddByteToEnd(value);
						fileBytePart.RemoveBytesFromStart(1);
						
						if (fileBytePart.Length == 0)
							_bytePartList.Remove(fileBytePart);

						OnDataChanged(EventArgs.Empty);
						
						return;
					}
				}

				if (index == startIndex + fileBytePart.Length - 1 && bp.Next != null)
				{
					MemoryBytePart nextMemoryBytePart = bp.Next.Value as MemoryBytePart;
					if (nextMemoryBytePart != null)
					{
						nextMemoryBytePart.AddByteToStart(value);
						fileBytePart.RemoveBytesFromEnd(1);
						
						if (fileBytePart.Length == 0)
							_bytePartList.Remove(fileBytePart);

						OnDataChanged(EventArgs.Empty);

						return;
					}
				}

				FileBytePart newPrevFileBytePart = null;
				FileBytePart newNextFileBytePart = null;

				if (index > startIndex)
					newPrevFileBytePart = new FileBytePart(fileBytePart.FilePartIndex, index - startIndex);
				
				if (index < startIndex + fileBytePart.Length - 1)
				{
					newNextFileBytePart = new FileBytePart(
						fileBytePart.FilePartIndex + index - startIndex + 1,
						fileBytePart.Length - (index - startIndex + 1));
				}

				BytePart newMemoryBP = new MemoryBytePart(value);
				_bytePartList.Find(bytePart).Value = newMemoryBP;
				bytePart = newMemoryBP;

				if (newPrevFileBytePart != null)
				{
					LinkedListNode<BytePart> bp1 = _bytePartList.Find(bytePart);
					_bytePartList.AddBefore(bp1, newPrevFileBytePart);
				}

				if (newNextFileBytePart != null)
				{
					LinkedListNode<BytePart> bp2 = _bytePartList.Find(bytePart);
					_bytePartList.AddAfter(bp2, newNextFileBytePart);
				}

				OnDataChanged(EventArgs.Empty);
			}
			else
				throw new ArgumentNullException("index", "FileByteData.WriteByte: The internal BytePartList is corrupt.");
		}

		public void InsertBytes(long index, byte[] value, bool track_Change = true)
		{
			if (track_Change)
				_undoList.Add(new UndoAction(value, index, mod_type.mod_insert));

			BytePart bytePart = BytePartGetInfo(index, out long startIndex);

			FileBytePart fileBytePart = bytePart as FileBytePart;
			MemoryBytePart memoryBytePart = bytePart as MemoryBytePart;

			if (memoryBytePart != null)
			{
				memoryBytePart.InsertBytes(index - startIndex, value);

				Length += value.Length;
				OnDataLengthChanged(EventArgs.Empty);
				OnDataChanged(EventArgs.Empty);

				return;
			}
			
			LinkedListNode<BytePart> bp = _bytePartList.Find(bytePart);
			if (startIndex == index && bp.Previous != null)
			{
				MemoryBytePart prevMemoryBytePart = bp.Previous.Value as MemoryBytePart;
				if (prevMemoryBytePart != null)
				{
					prevMemoryBytePart.InsertBytes(prevMemoryBytePart.Length, value);

					Length += value.Length;
					OnDataLengthChanged(EventArgs.Empty);
					OnDataChanged(EventArgs.Empty);

					return;
				}
			}

			if (fileBytePart != null)
			{
				FileBytePart newPrevFileBytePart = null;
				FileBytePart newNextFileBytePart = null;

				if (index > startIndex)
					newPrevFileBytePart = new FileBytePart(fileBytePart.FilePartIndex, index - startIndex);
				
				if (index < startIndex + fileBytePart.Length)
				{
					newNextFileBytePart = new FileBytePart(
						fileBytePart.FilePartIndex + index - startIndex,
						fileBytePart.Length - (index - startIndex));
				}

				BytePart newBP = new MemoryBytePart(value);
				_bytePartList.Find(bytePart).Value = newBP;
				bytePart = newBP;

				if (newPrevFileBytePart != null)
				{
					LinkedListNode<BytePart> bp1 = _bytePartList.Find(bytePart);
					_bytePartList.AddBefore(bp1, newPrevFileBytePart);
				}

				if (newNextFileBytePart != null)
				{
					LinkedListNode<BytePart> bp2 = _bytePartList.Find(bytePart);
					_bytePartList.AddAfter(bp2, newNextFileBytePart);
				}

				Length += value.Length;
				OnDataLengthChanged(EventArgs.Empty);
				OnDataChanged(EventArgs.Empty);
			}
			else
				throw new ArgumentNullException("index", "FileByteData.InsertBytes: The internal BytePartList is corrupt.");
		}

		public void DeleteBytes(long index, long length, bool track_Change = true)
		{
			long tempLength = length;
			Byte[] oldData = new Byte[length];

			for (long l = 0; l < length; l++)
				oldData[l] = ReadByte(l + index);

			if (track_Change)
				_undoList.Add(new UndoAction(oldData, index, mod_type.mod_delete));

			BytePart bytePart = BytePartGetInfo(index, out long startIndex);

			while ((tempLength > 0) && (bytePart != null))
			{
				LinkedListNode<BytePart> bp = _bytePartList.Find(bytePart);
				BytePart nextBytePart = (bp.Next != null) ? bp.Next.Value : null;

				long byteCount = Math.Min(tempLength, bytePart.Length - (index - startIndex));

				System.Diagnostics.Debug.Assert(byteCount > 0);

				bytePart.RemoveBytes(index - startIndex, byteCount, _bytePartList);

				if (bytePart.Length == 0)
				{
					_bytePartList.Remove(bytePart);
					
					if (_bytePartList.First == null)
						_bytePartList.AddFirst(new MemoryBytePart(new byte[0]));
				}

				tempLength -= byteCount;
				startIndex += bytePart.Length;
				bytePart = (tempLength > 0) ? nextBytePart : null;
			}

			Length -= length;
			OnDataLengthChanged(EventArgs.Empty);
			OnDataChanged(EventArgs.Empty);
		}

		public void Undo()
		{
			if (_undoList.Count > 0)
			{
				UndoAction ua = _undoList[_undoList.Count - 1];
				_undoList.RemoveAt(_undoList.Count - 1);

				switch (ua.GetModType())
				{
					case mod_type.mod_delete:
						{
							InsertBytes(ua.GetAddress(), ua.GetData(), false);
							break;
						}
					case mod_type.mod_insert:
						{
							DeleteBytes(ua.GetAddress(), ua.GetData().Length, false);
							break;
						}
					case mod_type.mod_replace:
						{
							WriteByte(ua.GetAddress(), ua.GetData()[0], false);
							break;
						}
				}
			}
			else
				System.Diagnostics.Debug.WriteLine("FileByteData.Undo: Undo list is empty.");
		}

		public bool IsUndoAvailable
		{
			get { return (_undoList.Count > 0) && !IsReadOnly; }
		}
	}

	public class MemoryByteData : IByteData
	{
		bool _memoryDataIsReadOnly = false;
		bool _hasChanges;
		List<byte> _bytes;

		public MemoryByteData(byte[] data) : this(new List<Byte>(data))
		{
		}

		public MemoryByteData(List<Byte> bytes)
		{
			_bytes = bytes;
		}

		public List<Byte> Bytes
		{
			get { return _bytes; }
		}

		public long Length
		{
			get { return _bytes.Count; }
		}

		public bool IsReadOnly
		{
			get { return _memoryDataIsReadOnly; }
		}

		public bool IsChanged()
		{
			return _hasChanges;
		}

		public void CommitChanges()
		{
			_hasChanges = false;
		}

		public event EventHandler DataLengthChanged;
		public event EventHandler DataChanged;

		void OnDataLengthChanged(EventArgs e)
		{
			if (DataLengthChanged != null)
				DataLengthChanged(this, e);
		}

		void OnDataChanged(EventArgs e)
		{
			_hasChanges = true;

			if (DataChanged != null)
				DataChanged(this, e);
		}

		public byte ReadByte(long index)
		{ 
			return _bytes[(int)index]; 
		}

		public byte ReadByte(long index, out bool isChanged)
		{
			isChanged = true;
			return _bytes[(int)index];
		}

		public void WriteByte(long index, byte value, bool track_Change = true)
		{
			_bytes[(int)index] = value;
			OnDataChanged(EventArgs.Empty);
		}

		public void InsertBytes(long index, byte[] value, bool track_Change = true)
		{
			_bytes.InsertRange((int)index, value);

			OnDataLengthChanged(EventArgs.Empty);
			OnDataChanged(EventArgs.Empty);
		}

		public void DeleteBytes(long index, long length, bool track_Change = true)
		{
			int internal_index = (int)Math.Max(0, index);
			int internal_length = (int)Math.Min((int)Length, length);
			_bytes.RemoveRange(internal_index, internal_length);

			OnDataLengthChanged(EventArgs.Empty);
			OnDataChanged(EventArgs.Empty);
		}

		public void Undo()
		{
			throw new NotImplementedException();
		}

		public bool IsUndoAvailable
		{
			get { return false; }
		}
	}

	enum mod_type
	{
		mod_insert = 0,
		mod_replace,
		mod_delete
	};

	internal sealed class UndoAction
	{
		mod_type _modType;
		readonly long _address;
		readonly byte[] _data;

		public UndoAction(byte[] data, long address, mod_type modType)
		{
			_address = address;
			_data = data.Clone() as byte[];
			_modType = modType;
		}

		public byte[] GetData()
		{
			return _data;
		}

		public long GetAddress()
		{
			return _address;
		}

		public mod_type GetModType()
		{
			return _modType;
		}
	}

	internal class UndoList : List<UndoAction>
	{ }

	public enum SearchDirection
	{
		Direction_Down = 0,
		Direction_Up
	};

	public class FindByteDataEventArgs : EventArgs
	{
		public long FoundPosition { get; set; }
	}

	public sealed class FindByteDataOption
	{
		System.Text.ASCIIEncoding _enc = null;
		byte[] _searchBytes = null;
		string _searchText = string.Empty;
		long _searchStartIndex = 0;
		bool _matchCase = false;

		public FindByteDataOption()
		{
			_enc = new System.Text.ASCIIEncoding();
		}

		public byte[] SearchBytes
		{
			get { return _searchBytes; }
			set
			{
				if (value != null)
				{
					_searchBytes = value;
					_matchCase = false;
					_searchText = new string(_enc.GetChars(_searchBytes));
				}
			}
		}

		public string SearchText
		{
			get { return _searchText; }
			set
			{
				if (value != null && value.Length > 0)
				{
					_searchText = value;

					if (!_matchCase)
						_searchText = _searchText.ToLower();

					_searchBytes = _enc.GetBytes(_searchText);
				}
			}
		}

		public SearchDirection SearchDirection { get; set; } = SearchDirection.Direction_Down;

		public long SearchStartIndex
		{
			get { return _searchStartIndex; }
			set
			{
				if (value >= 0)
					_searchStartIndex = value;
			}
		}

		public bool MatchCase
		{
			get { return _matchCase; }
			set
			{
				if (_matchCase == value)
					return;

				_matchCase = value;

				if (!_matchCase)
				{
					_searchText = SearchText.ToLower();
					_searchBytes = _enc.GetBytes(_searchText);
				}
				else
				{
					_searchText = new string(_enc.GetChars(_searchBytes));
					_searchBytes = _enc.GetBytes(_searchText);
				}
			}
		}
	}

	internal class FindByteData
	{
		byte[] _buffer = null;
		IByteData _byteData = null;
		long _startIndex = 0;
		bool _matchCase = false;

		public FindByteData(IByteData byteData)
		{
			_buffer = new byte[4096];
			_byteData = byteData ?? throw new ArgumentNullException("byteData", "FindByteData.FindByteData: The parameter cannot be NULL.");
		}

		public void Find(FindByteDataOption findByteDataOption)
		{
			_startIndex = findByteDataOption.SearchStartIndex;
			_matchCase = findByteDataOption.MatchCase;

			FindByteDataEventArgs args = new FindByteDataEventArgs();

			if (findByteDataOption.SearchBytes == null)
			{
				args.FoundPosition = -1;
				OnFindPositionFound(args);
				return;
			}

			if (findByteDataOption.SearchDirection == SearchDirection.Direction_Up)
				args.FoundPosition = FindUp(findByteDataOption);
			else
				args.FoundPosition = FindDown(findByteDataOption);

			OnFindPositionFound(args);
		}

		public long FindDown(FindByteDataOption findByteDataOption)
		{
			while (_startIndex < _byteData.Length)
			{
				long bufferSize = FillBuffer(_buffer, _startIndex);

				int i = FindBuffer(_buffer, findByteDataOption.SearchBytes);

				if (i >= 0)
					return _startIndex + i;
				else
					_startIndex += _buffer.Length - findByteDataOption.SearchBytes.Length + 1;
			}

			return -1;
		}

		public long FindUp(FindByteDataOption findByteDataOption)
		{
			while (_startIndex > 0)
			{
				long bufferSize = FillBufferUp(_buffer, _startIndex);

				int i = FindBuffer(_buffer, findByteDataOption.SearchBytes);

				if (i >= 0)
					//return _startIndex - (_buffer.Length - i) + 1;
					return _startIndex - (bufferSize - i) + 1;
				else
					_startIndex -= _buffer.Length - findByteDataOption.SearchBytes.Length + 1;
			}

			return -1;
		}

		private long FillBuffer(byte[] buffer, long idx)
		{
			Array.Clear(buffer, 0, buffer.Length);

			long _maxBufferToRead = buffer.Length;

			if (_byteData.Length < buffer.Length + idx)
				_maxBufferToRead = _byteData.Length - idx;

			for (long i = 0; i < _maxBufferToRead; i++)
				buffer[i] = _byteData.ReadByte(i + idx);
			
			System.Diagnostics.Debug.WriteLine("Search: Fill buffer from {0:x} to {1:x}. StreamSize: {2:x}", idx, (idx + _maxBufferToRead - 1), _byteData.Length);

			return _maxBufferToRead;
		}

		private long FillBufferUp(byte[] buffer, long idx)
		{
			Array.Clear(buffer, 0, buffer.Length);

			long _maxBufferToRead = buffer.Length;

			if (idx - buffer.Length < 0)
				_maxBufferToRead = idx + 1;

			for (long i = 0; i < _maxBufferToRead; i++)
				buffer[_maxBufferToRead - 1 - i] = _byteData.ReadByte(idx - i);

			System.Diagnostics.Debug.WriteLine("Search: Fill buffer from {0:x} to {1:x}. StreamSize: {2:x}", idx, (idx + 1 - _maxBufferToRead), _byteData.Length);

			return _maxBufferToRead;
		}

		private int FindBuffer(byte[] buffer, byte[] searchForBytes)
		{
			if (buffer == null || searchForBytes == null ||
				buffer.Length == 0 || searchForBytes.Length == 0 ||
				searchForBytes.Length > buffer.Length)
				return -1;

			var list = new List<int>();

			for (int i = 0; i < buffer.Length; i++)
			{
				if (!IsMatch(buffer, i, searchForBytes))
					continue;

				return i;
			}

			return -1;
		}

		private bool IsMatch(byte[] buffer, int idx, byte[] searchForBytes)
		{
			if (searchForBytes.Length > (buffer.Length - idx))
				return false;

			for (int i = 0; i < searchForBytes.Length; i++)
			{
				if (_matchCase)
				{
					if (buffer[idx + i] != searchForBytes[i])
						return false;
				}
				else
				{
					if (buffer[idx + i] != searchForBytes[i] && (buffer[idx + i] != (searchForBytes[i]) - 0x20))
						return false;
				}
			}

			return true;
		}

		protected virtual void OnFindPositionFound(FindByteDataEventArgs e)
		{
			PositionFound?.Invoke(this, e);
		}

		public event EventHandler<FindByteDataEventArgs> PositionFound;
	}
}
