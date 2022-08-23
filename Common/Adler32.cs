public class Adler32
{
	public int value
	{
		get
		{
			return m_A2 << 16 | m_A1;
		}
	}

	public Adler32()
	{
		m_A1 = 1;
		m_A2 = 0;
	}

	public void Update(byte[] _bytes, int _position, int _length)
	{
		int num = _position + _length;
		for (int i = _position; i < num; i++)
		{
			m_A1 = (m_A1 + (int)_bytes[i]) % 65521;
			m_A2 = (m_A2 + m_A1) % 65521;
		}
	}

	public int Make(Stream _stream)
	{
		BinaryReader binaryReader = new(_stream);
		return Make(binaryReader.ReadBytes((int)_stream.Length));
	}

	public int Make(byte[] _bytes)
	{
		m_A1 = 1;
		m_A2 = 0;
		Update(_bytes, 0, _bytes.Length);
		return value;
	}

	private int m_A1;

	private int m_A2;
}