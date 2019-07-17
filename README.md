# AskForRatingBuddy

This is a quick MonoGame library for asking the user to rate your game on the Goolge Play Store and Apple App Store.

This lib is part of the "Buddy" system, so will only work if you've built your app with MenuBuddy etc.

To use, just add the Nuget package to your MonoGame.Android and MonoGame.iOS projects.

Find a good spot to ask the user if they'd like to review your game. I'd recommend you ask the user for a rating right after they've completed a level. 

Add an instance of the AskForRatingScreen to the ScreenManager with a flag whether or not they won the last round, and some identifiers for your game.

The workflow of this lib is as follows:

- Only pops up rating messages if they won the last round. (More likely to get a good rating!)
- Only pops up rating messages every 5th or so time. (Don't want to constantly bother them!)
- Won't pop up the rating messages if the player has already agreed to rate the app.
- Initially, asks the player if they are enjoying the game.
- If they are enjoying the game, ask if they'd like to rate the game.
- If they agree to rate the game, pop them to the correct app store for your game.
- If they aren't enjoying the game, ask if they'd like to leave some feedback.
- If they agree to leave feedback, pop the user to the native email client of their device, with an email prepopulated with your support email and subject line.

Good luck!
