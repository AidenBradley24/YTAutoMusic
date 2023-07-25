# YTAutoMusic
A Windows command line YT to mp3 solution that is designed to fill mp3 metadata based on YT descriptions.

## Features (Given a YT playlist URL automatically...)
- Download YouTube playlists to mp3 files
- Create a playlist file
- Append existing playlists without needing to redownload the entire playlist
- Fill metadata of files based on YouTube titles and descriptions

### Supported playlist files
Note: these need to be enabled seperately in the config file 'YTAutoMusic.dll.config'
- XSPF -- default
- M3U8 -- default
- M3U
- VLC (.xspf extension)
- AIMP (.xspf extension)

## Quick Start
- Download the latest release in the 'releases' tab
- Customize settings in the 'YTAutoMusic.dll.config' file
- Create a YouTube playlist
- Pick a directory to place your music into (such as the 'Music' folder)
- Open YTAutoMusic.exe
- New
- Input your music directory and your YT playlist
- Wait for finish
- Open the playlist file in your media player of choice

## These two programs are used to run the application:
- yt-dlp https://github.com/yt-dlp/yt-dlp
- ffmpeg https://ffmpeg.org/

When downloading a release from the releases tab, they are included.
However, they are not included in this repository and must be downloaded seperately if you wish to clone.
