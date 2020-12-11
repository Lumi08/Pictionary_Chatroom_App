﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;

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
			Disconnect,
			Login,
			KeyPacket,
			ClientList,
			GameConnectionPacket,
			PictionaryChatMessage,
			PictionaryPaint,
			PictionaryClearCanvas
		}

		public enum GameType
		{
			Pictionary
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
		private byte[] message;

		public ChatMessagePacket(byte[] message)
		{
			this.message = message;
			m_PacketType = PacketType.ChatMessage;
		}

		public byte[] Message
		{
			get { return message; }
			protected set { message = value; }
		}
	}

	[Serializable()]
	public class PrivateMessagePacket : Packet
	{
		private byte[] message;
		private string targetUser;

		public PrivateMessagePacket(byte[] message, string targetUser)
		{
			this.message = message;
			this.targetUser = targetUser;
			m_PacketType = PacketType.PrivateMessage;
		}

		public byte[] Message
		{
			get { return message; }
			set { message = value; }
		}

		public string TargetUser
		{
			get { return targetUser; }
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

	[Serializable()]
	public class ClientsPacket : Packet
	{
		private string[] clients;

		public ClientsPacket(string[] clients)
		{
			m_PacketType = PacketType.ClientList;
			this.clients = clients;
		}

		public string[] Clients
		{
			get { return clients; }
			set { }
		}
	}

	[Serializable()]
	public class LoginPacket : Packet
	{
		private string nickname;
		private IPEndPoint endPoint;
		private RSAParameters publicKey;

		public LoginPacket(string nickname, IPEndPoint endPoint, RSAParameters publicKey)
		{
			m_PacketType = PacketType.Login;
			this.nickname = nickname;
			this.endPoint = endPoint;
			this.publicKey = publicKey;
		}

		public IPEndPoint Endpoint
		{
			get { return endPoint; }
			set { }
		}

		public RSAParameters PublicKey
		{
			get { return publicKey; }
			set { }
		}

		public string Nickname
		{
			get { return nickname; }
			set { }
		}
	}

	[Serializable()]
	public class KeyPacket : Packet
	{
		private RSAParameters key;

		public KeyPacket(RSAParameters key)
		{
			m_PacketType = PacketType.KeyPacket;
			this.key = key;
		}

		public RSAParameters Key
		{
			get { return key; }
			set { }
		}
	}

	[Serializable()]
	public class GameConnectionPacket : Packet
	{
		GameType gameToPlay;
		bool connection;

		public GameConnectionPacket(GameType game, bool connected)
		{
			m_PacketType = PacketType.GameConnectionPacket;
			gameToPlay = game;
			this.connection = connected;
		}

		public GameType GameToPlay
		{
			get { return gameToPlay; }
			set { }
		}

		public bool Connected
		{
			get { return connection; }
			set { }
		}
	}

	//Pictionary Packets
	[Serializable()]
	public class PictionaryChatMessagePacket : Packet
	{
		public byte[] message;

		public PictionaryChatMessagePacket(byte[] message)
		{
			this.message = message;
			m_PacketType = PacketType.PictionaryChatMessage;
		}

		public byte[] Message
		{
			get { return message; }
			protected set { message = value; }
		}
	}

	[Serializable()]
	public class PictionaryPaintPacket : Packet
	{
		private double[] xPositions;
		private double[] yPositions;
		private float[] penColor = new float[4];

		public PictionaryPaintPacket(List<double> xPos, List<double> yPos)
		{
			xPositions = new double[xPos.Count];
			yPositions = new double[yPos.Count];
			m_PacketType = PacketType.PictionaryPaint;
			for (int i = 0; i < xPos.Count; i++)
			{
				xPositions[i] = xPos[i];
			}
			for (int i = 0; i < yPos.Count; i++)
			{
				yPositions[i] = yPos[i];
			}
		}

		public double[] XPositions
		{
			get { return xPositions; }
			set { }
		}

		public double[] YPositions
		{
			get { return yPositions; }
			set { }
		}

		public void SetPenColour(float r, float g, float b, float a)
		{
			penColor[0] = r;
			penColor[1] = g;
			penColor[2] = b;
			penColor[3] = a;
		}

		public float[] PenColor
		{
			get { return penColor; }
			set { }
		}
	}

	[Serializable()]
	public class PictionaryClearCanvasPacket : Packet
	{
		public PictionaryClearCanvasPacket()
		{
			m_PacketType = PacketType.PictionaryClearCanvas;
		}
	}
}
