﻿using BepInEx.Logging;
using Comfort.Common;
using EFT.GlobalEvents;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Packets.GameWorld;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using UnityEngine;

namespace Fika.Core.Coop.Components
{
    internal class CoopHalloweenEventManager : MonoBehaviour
    {
        ManualLogSource Logger;

        private Action summonStartedAction;
        private Action syncStateEvent;
        private Action syncExitsEvent;

        private FikaServer server;
        private NetDataWriter writer = new();

        protected void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("CoopHalloweenEventManager");
        }

        protected void Start()
        {
            //No need to begin to subscribe to events on client as this would cause an infinite loop
            if (FikaBackendUtils.IsClient)
            {
                return;
            }

            server = Singleton<FikaServer>.Instance;

            summonStartedAction = GlobalEventHandlerClass.Instance.SubscribeOnEvent(new Action<HalloweenSummonStartedEvent>(this.OnHalloweenSummonStarted));
            syncStateEvent = GlobalEventHandlerClass.Instance.SubscribeOnEvent(new Action<HalloweenSyncStateEvent>(this.OnHalloweenSyncStateEvent));
            syncExitsEvent = GlobalEventHandlerClass.Instance.SubscribeOnEvent(new Action<HalloweenSyncExitsEvent>(this.OnHalloweenSyncExitsEvent));
        }

        private void OnHalloweenSummonStarted(HalloweenSummonStartedEvent summonStartedEvent)
        {
            Logger.LogDebug("OnHalloweenSummonStarted");

            HalloweenEventPacket packet = new(EHalloweenPacketType.Summon)
            {
                SummonPosition = summonStartedEvent.PointPosition
            };

            writer.Reset();
            server.SendDataToAll(writer, ref packet, DeliveryMethod.ReliableOrdered);
        }

        private void OnHalloweenSyncStateEvent(HalloweenSyncStateEvent syncStateEvent)
        {
            Logger.LogDebug("OnHalloweenSyncStateEvent");

            HalloweenEventPacket packet = new(EHalloweenPacketType.Sync)
            {
                EventState = syncStateEvent.EventState
            };

            writer.Reset();
            server.SendDataToAll(writer, ref packet, DeliveryMethod.ReliableOrdered);
        }

        private void OnHalloweenSyncExitsEvent(HalloweenSyncExitsEvent syncStateEvent)
        {
            Logger.LogDebug("OnHalloweenSyncExitsEvent");

            HalloweenEventPacket packet = new(EHalloweenPacketType.Exit)
            {
                Exit = syncStateEvent.ExitName
            };

            writer.Reset();
            server.SendDataToAll(writer, ref packet, DeliveryMethod.ReliableOrdered);
        }
    }
}
