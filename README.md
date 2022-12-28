# gamesense_net
Short implementation of SteelSeries GG Gamesense API in .NET 7

As far as I know, it's the whole thing.

# I wanted to make my new keyboard's OLED screen display whatever audio is currently playing.

I know there's already an app that exists to do that. I also just wanted to program something.

# so what's it do

It utilizes WinRT to observe what the "focused" audio session is in the Windows media player. It then just displays the title on the first line, and the artist on the second line.

It does not scroll long text. Sorry.

It can determine what audio is playing from all sorts of applications and websites, like YouTube and Spotify. No unnecessary web calls to external API's, only POSTs to the SteelSeries port.

# Dependencies

This relies on Newtonsoft (what doesn't) and [Dubya](https://www.nuget.org/packages/Dubya.WindowsMediaController/) becuase <i>someone</i> at Microsoft decided to remove the built-in support for WinRT from .NET in .NET 5.0. Loser.
