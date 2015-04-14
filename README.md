ScreenCaptureWrapper
======================

# What's this?

The GUI frontend of ffmpeg that focused to capture videos of the screen (screencast).

![Screenshot](https://raw.github.com/sunnyone/ScreenCaptureWrapper/master/Screenshot.png)

# Requirements

* .NET Framework 4.5
* ffmpeg (that supports gdigrab if you use it)
* [screen-capture-recorder](https://github.com/rdp/screen-capture-recorder-to-video-windows-free) DirectShow filter (Optional: if you needs audio)

# How to use?

1. Select the path of ffmpeg.
2. Select the preset of recording. This is just a templated text of command-line argument (see config.yml).
3. Select the path to output a video.
4. Pick the position to record.
5. Press the record button.

