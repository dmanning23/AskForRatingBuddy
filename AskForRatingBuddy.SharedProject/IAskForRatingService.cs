using MenuBuddy;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace AskForRatingBuddy
{
	public interface IAskForRatingService : IGameComponent
	{
		int MinAsks { get; set; }

		Task AskForRating(ScreenManager screenManager, bool gameWon);
	}
}
