using System;
using System.Collections.Generic;
using System.Text;

namespace Packets
{

	[Serializable()]
	public abstract class Packet
	{
		public enum PacketType
		{
			ChatMessage,
			PrivateMessage,
			Nickname,
			Disconnect
		}

		private PacketType _packetType;

		public PacketType m_PacketType
		{
			get { return _packetType; }
			protected set { _packetType = value; }
		}
	}

	[Serializable()]
	public class ChatMessagePacket : Packet
	{
		private string message;

		public ChatMessagePacket(string message)
		{
			this.message = message;
			m_PacketType = PacketType.ChatMessage;
		}

		public string Message
		{
			get { return message; }
			protected set { }
		}
	}

	[Serializable()]
	public class NicknamePacket : Packet
	{
		private string name;

		public NicknamePacket(string name)
		{
			this.name = name;
			m_PacketType = PacketType.Nickname;
		}

		public string Name
		{
			get { return name; }
			protected set { }
		}
	}

	[Serializable()]
	public class DisconnectPacket : Packet
	{
		public DisconnectPacket()
		{
			m_PacketType = PacketType.Disconnect;
		}
	}
}
