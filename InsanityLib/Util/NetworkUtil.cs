using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Vintagestory.Client.NoObf;

namespace InsanityLib.Util;

public  static class NetworkUtil
{
    public static void AfterPacket<T>(this NetworkChannel channel, Action<Packet_CustomPacket> action)
    {
        if (!channel.messageTypes.TryGetValue(typeof(T), out var messageId))
		{
			string str = "No such message type ";
			Type typeFromHandle = typeof(T);
			throw new Exception($"{str} {typeof(T)} registered. Did you forgot to call RegisterMessageType?");
		}

        channel.handlers[messageId] += action;
    }
}
