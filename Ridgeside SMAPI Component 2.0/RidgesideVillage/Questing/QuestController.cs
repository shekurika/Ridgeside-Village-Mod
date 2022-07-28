﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Quests;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace RidgesideVillage.Questing
{
    internal class QuestController
    {

		static IModHelper Helper;
		static IMonitor Monitor;
		//static Lazy<Texture2D> questionMarkSprite = new Lazy<Texture2D>(() => Helper.Content.Load<Texture2D>(PathUtilities.NormalizePath("assets/questMark.png"), ContentSource.ModFolder));

		internal static readonly PerScreen<QuestData> dailyQuestData = new PerScreen<QuestData>();
		private static LocationForMarkers CurrentLocationFormarkers = LocationForMarkers.Other;

		//is board unlocked in the current map (if there is one)
		private static readonly PerScreen<bool> SOBoardUnlocked = new();
		private static readonly PerScreen<bool> QuestBoardUnlocked = new();

		private enum LocationForMarkers
        {
			RSVVillage,
			NinjaHouse,
			Other
        }

		internal static void Initialize(IMod ModInstance)
		{
			Helper = ModInstance.Helper;
			Monitor = ModInstance.Monitor;
			TileActionHandler.RegisterTileAction("RSVQuestBoard", OpenQuestBoard);
			TileActionHandler.RegisterTileAction("RSVSpecialOrderBoard", OpenSOBoard);
			Helper.ConsoleCommands.Add("RSVrefresh", "", (s1, s2) => RSVSpecialOrderBoard.UpdateAvailableRSVSpecialOrders(force_refresh: true));

			Helper.Events.GameLoop.DayStarted += OnDayStarted;
			Helper.Events.Player.Warped += OnWarped;
			Helper.Events.Display.RenderedWorld += RenderQuestMarkersIfNeeded;
		}

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name.Equals("Custom_Ridgeside_RidgesideVillage"))
            {
				CurrentLocationFormarkers = LocationForMarkers.RSVVillage;
				SOBoardUnlocked.Value = Game1.player.eventsSeen.Contains(75160207);

			}
			else if (e.NewLocation.Name.Equals("Custom_Ridgeside_RSVNinjaHouse"))
			{
				CurrentLocationFormarkers = LocationForMarkers.NinjaHouse;
				SOBoardUnlocked.Value = Game1.player.eventsSeen.Contains(75160264);
				QuestBoardUnlocked.Value = Game1.player.eventsSeen.Contains(75160187);
			}
            else
			{
				CurrentLocationFormarkers = LocationForMarkers.Other;
				SOBoardUnlocked.Value = false;
				QuestBoardUnlocked.Value = false;
			}
        }

        private static void RenderQuestMarkersIfNeeded(object sender, RenderedWorldEventArgs e)
        {
			SpriteBatch sb = e.SpriteBatch;
            switch (CurrentLocationFormarkers)
            {
				case LocationForMarkers.RSVVillage:
					float offset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					if (!dailyQuestData.Value.acceptedDailyQuest)
					{
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(109f * 64f + 32f, 38.5f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);

					}
					if (SOBoardUnlocked.Value && !Game1.player.team.acceptedSpecialOrderTypes.Contains("RSVTownSO"))
					{
						Vector2 questMarkPosition = new Vector2(119f * 64f + 27f, 39f * 64f);
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(119f * 64f + 32f, 39.5f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);
					}
					
					break;
				case LocationForMarkers.NinjaHouse:
					offset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					if (SOBoardUnlocked.Value && !dailyQuestData.Value.acceptedDailyNinjaHouseQuest)
					{
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(6f * 64f + 32f, 5.5f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);

					}
					if (!Game1.player.team.acceptedSpecialOrderTypes.Contains("RSVNinjaSO"))
					{
						sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3f * 64f + 32f, 3f * 64f + offset)),
							new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - offset / 16f), SpriteEffects.None, 1f);
					}
					break;
			}
			return;
		}
        private static void OpenQuestBoard(string name, Vector2 position)
		{
			dailyQuestData.Value.dailyTownQuest =  QuestFactory.GetDailyQuest();
			string type = name.Split()[^1];
			Log.Trace($"Opening RSVQuestBoard {type}");
			Game1.activeClickableMenu = new RSVQuestBoard(dailyQuestData.Value, type);
		}


		private static void OpenSOBoard(string name, Vector2 position)
		{
			string type = name.Split()[^1];
			Log.Trace($"Opening RSVSOBoard {type}");
			Game1.activeClickableMenu = new RSVSpecialOrderBoard(type);
		}

		private static void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			Game1.addHUDMessage(new HUDMessage(SDate.Now().ToLocaleString(true)));
			Log.Error(SDate.Now().ToLocaleString(true));
			dailyQuestData.Value = new QuestData(QuestFactory.GetDailyQuest(), QuestFactory.GetDailyQuest());
			//if monday, update special orders
			if (Game1.dayOfMonth % 7 == 1 && Game1.player.IsMainPlayer)
			{
				RSVSpecialOrderBoard.UpdateAvailableRSVSpecialOrders(force_refresh: true);
			}


			Quest townQuest = QuestFactory.GetDailyQuest();
			Quest ninjaQuest = QuestFactory.GetDailyNinjaQuest(); 
			dailyQuestData.Value = new QuestData(townQuest, ninjaQuest);
		}
	}


	internal class QuestData
	{
		internal Quest dailyTownQuest;
		internal bool acceptedDailyQuest;
		internal Quest dailyNinjaHouseQuest;
		internal bool acceptedDailyNinjaHouseQuest;

		internal QuestData(Quest dailyTownQuest, Quest dailyNinjaHouseQuest)
		{
			this.dailyTownQuest = dailyTownQuest;
			this.acceptedDailyQuest = false;
			this.dailyNinjaHouseQuest = dailyNinjaHouseQuest;
			this.acceptedDailyNinjaHouseQuest = false;
		}
	}
}

