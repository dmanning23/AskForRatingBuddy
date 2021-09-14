using MenuBuddy;
using InputHelper;
using PerpetualEngine.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if __IOS__ || ANDROID
using Xamarin.Essentials;
using Plugin.StoreReview;
#endif

namespace AskForRatingBuddy
{
	public class AskForRatingService : IAskForRatingService
	{
		#region Properties

		ScreenManager ScreenManager { get; set; }


		private const string _RateAskNumberKey = "RateAskNumber";
		private const string _HasRatedKey = "HasRated";

		public int MinAsks { get; set; }

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

		private string GameName { get; set; }

		private string EmailAddress { get; set; }

		#endregion //Properties

		#region Methods

		public AskForRatingService(string gameName, string emailAddress, int minAsks = 2)
		{
			GameName = gameName;
			EmailAddress = emailAddress;
			MinAsks = minAsks;
		}

		public void Initialize()
		{
		}

		public async Task AskForRating(ScreenManager screenManager, bool gameWon)
		{
			ScreenManager = screenManager;

			//Don't do anything if the player had a negative experience or if they've already rated the game
			if (gameWon && !HasRated)
			{
				//update the rate ask number
				RateAskNumber = RateAskNumber + 1;

				//Has the user done enough stuff to ask them to rate?
				if (RateAskNumber >= MinAsks)
				{
					//Reset the amout of stuff the user has done
					RateAskNumber = 0;

					await AskForRating();
				}
			}
		}

		private async Task AskForRating()
		{
			var rateMsg = new MessageBoxScreen($"Are you enjoying {GameName}?", string.Empty)
			{
				OkText = "Yes!",
				CancelText = "Not Really",
			};
			rateMsg.OnSelect += EnjoyingGame;
			rateMsg.OnCancel += NotEnjoyingGame;
			await ScreenManager.AddScreen(rateMsg);
		}

		private async void EnjoyingGame(object sender, ClickEventArgs e)
		{
			var rateMsg = new MessageBoxScreen($"How about a rating on the app store?", string.Empty)
			{
				OkText = "Ok!",
				CancelText = "No Thanks",
			};
			rateMsg.OnSelect += OkRating;
			await ScreenManager.AddScreen(rateMsg);
		}

		private async void NotEnjoyingGame(object sender, ClickEventArgs e)
		{
			var rateMsg = new MessageBoxScreen($"Would you mind giving us some feedback?", string.Empty)
			{
				OkText = "Ok!",
				CancelText = "No Thanks",
			};
			rateMsg.OnSelect += OkFeedback;
			await ScreenManager.AddScreen(rateMsg);
		}

		private async void OkRating(object sender, ClickEventArgs e)
		{
			HasRated = true;

			try
			{
#if ANDROID || __IOS__
				await CrossStoreReview.Current.RequestReview(false);
#endif
			}
			catch (Exception ex)
			{
				// Some other exception occurred
				await ScreenManager.AddScreen(new ErrorScreen(ex));
			}
		}

		private async void OkFeedback(object sender, ClickEventArgs e)
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
				await Email.ComposeAsync(message);
			}
			catch (FeatureNotSupportedException fbsEx)
			{
				// Email is not supported on this device
				await ScreenManager.AddScreen(new ErrorScreen(fbsEx));
			}
			catch (Exception ex)
			{
				// Some other exception occurred
				await ScreenManager.AddScreen(new ErrorScreen(ex));
			}
#endif
		}

		#endregion //Methods
	}
}
