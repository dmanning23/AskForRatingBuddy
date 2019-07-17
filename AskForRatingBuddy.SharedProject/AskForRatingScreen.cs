using MenuBuddy;
using InputHelper;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using GameTimer;
using PerpetualEngine.Storage;
using System;
using System.Collections.Generic;
#if __IOS__ || ANDROID
using Xamarin.Essentials;
using Plugin.StoreReview;
#endif

namespace AskForRatingBuddy
{
	public class AskForRatingScreen : Screen
	{
		#region Properties

		CountdownTimer timer;

		private const string _RateAskNumberKey = "RateAskNumber";
		private const string _HasRatedKey = "HasRated";

		private const int MinAsks = 5;



		protected int RateAskNumber
		{
			get
			{
				var storage = SimpleStorage.EditGroup("AskForReviewService");
				return storage.Get(_RateAskNumberKey, 0);
			}
			set
			{
				var storage = SimpleStorage.EditGroup("AskForReviewService");
				storage.Put(_RateAskNumberKey, value);
			}
		}

		protected bool HasRated
		{
			get
			{
				var storage = SimpleStorage.EditGroup("AskForReviewService");
				return storage.Get(_HasRatedKey, false);
			}
			set
			{
				var storage = SimpleStorage.EditGroup("AskForReviewService");
				storage.Put(_HasRatedKey, value);
			}
		}

		private bool GameWon { get; set; }
		private string GameName { get; set; }
		private string PackageName { get; set; }

		private string EmailAddress { get; set; }

		#endregion //Properties

		#region Methods

		public AskForRatingScreen(bool gameWon, string gameName, string packageName, string emailAddress) : base("AskForRatingScreen")
		{
			GameWon = gameWon;
			GameName = gameName;
			PackageName = packageName;
			EmailAddress = emailAddress;
			timer = new CountdownTimer();
		}

		public override void LoadContent()
		{
			base.LoadContent();

			//Don't do anything if the player had a negative experience or if they've already rated the game
			if (!GameWon || HasRated)
			{
				ExitScreen();
			}
			else
			{
				//update the rate ask number
				RateAskNumber = RateAskNumber + 1;

				//Has the user done enough stuff to ask them to rate?
				if (RateAskNumber < MinAsks)
				{
					ExitScreen();
				}
				else
				{
					//Reset the amout of stuff the user has done
					RateAskNumber = 0;

					//Start asking for a rating in a second
					timer.Start(1f);
				}
			}
		}

		public override void Update(GameTime gameTime, bool otherWindowHasFocus, bool covered)
		{
			base.Update(gameTime, otherWindowHasFocus, covered);
			timer.Update(gameTime);

			//just ask for a rating and exit the screen
			if (!timer.HasTimeRemaining && !IsExiting)
			{
				AskForRating();
				ExitScreen();
			}
		}

		public void AskForRating()
		{
			var rateMsg = new MessageBoxScreen($"Are you enjoying ${GameName}?", string.Empty)
			{
				OkText = "Yes!",
				CancelText = "Not Really",
			};
			rateMsg.OnSelect += EnjoyingGame;
			rateMsg.OnCancel += NotEnjoyingGame;
			ScreenManager.AddScreen(rateMsg);
		}

		private void EnjoyingGame(object sender, ClickEventArgs e)
		{
			var rateMsg = new MessageBoxScreen($"How about a rating on the app store?", string.Empty)
			{
				OkText = "Ok!",
				CancelText = "No Thanks",
			};
			rateMsg.OnSelect += OkRating;
			ScreenManager.AddScreen(rateMsg);
		}

		private void NotEnjoyingGame(object sender, ClickEventArgs e)
		{
			var rateMsg = new MessageBoxScreen($"Would you mind giving us some feedback?", string.Empty)
			{
				OkText = "Ok!",
				CancelText = "No Thanks",
			};
			rateMsg.OnSelect += OkFeedback;
			ScreenManager.AddScreen(rateMsg);
		}

		private void OkRating(object sender, ClickEventArgs e)
		{
			HasRated = true;

			try
			{
#if ANDROID
				var ratePlugin = new StoreReviewImplementation();
				ratePlugin.OpenStoreReviewPage($"{PackageName}");
#elif __IOS__
				var ratePlugin = new StoreReviewImplementation();
				ratePlugin.RequestReview();
#endif
			}
			catch (Exception ex)
			{
				// Some other exception occurred
				ScreenManager.AddScreen(new ErrorScreen(ex));
			}
		}

		private void OkFeedback(object sender, ClickEventArgs e)
		{
#if ANDROID || __IOS__
			try
			{
				var message = new EmailMessage
				{
					Subject = $"{GameName} Feedback",
					To = new List<string> { EmailAddress },
					//Cc = ccRecipients,
					//Bcc = bccRecipients
				};
				Email.ComposeAsync(message);
			}
			catch (FeatureNotSupportedException fbsEx)
			{
				// Email is not supported on this device
				ScreenManager.AddScreen(new ErrorScreen(fbsEx));
			}
			catch (Exception ex)
			{
				// Some other exception occurred
				ScreenManager.AddScreen(new ErrorScreen(ex));
			}
#endif
		}

		#endregion //Methods
	}
}
