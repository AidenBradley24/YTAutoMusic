# YTAutoMusic
A Windows command line YT to mp3 solution that is designed to fill mp3 metadata based on YT descriptions.

## Features (Given a YT playlist URL automatically...)
- Download YouTube playlists to mp3 files
- Create a playlist file
- Append existing playlists without needing to redownload the entire playlist
- Fill metadata of files based on YouTube titles and descriptions

### Supported playlist files
Note: these need to be enabled seperately in the config file 'YTAutoMusic.dll.config'
- VLC (.xspf) -- default
- AIMP (.xspf)

## Quick Start
- Download the latest release in the 'releases' tab
- Customize settings in the 'YTAutoMusic.dll.config' file

## These two programs are used to run application:
- yt-dlp https://github.com/yt-dlp/yt-dlp
- ffmpeg https://ffmpeg.org/

They are integrated into the build files