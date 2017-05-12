# EDMC Overlay (c) 2017 Ian Norton
## About

EDMC Overlay is a helper program for Elite:Dangerous, It provides a means for 
a program such as EDMC to display simple messages in the game's DirectX window.

## Compatibility

* Currently supports 64Bit Elite:Dangerous (Horizons) on Windows Only.
* 64bit Non-Horizons may work. YMMV.
* Apple support is not likley (I don't have a Mac)

## Installation

This is released as a standard EDMC Plugin, simple unpack the archive into the EDMC
plugin folder.

## License

I intend to distribute this under the BSD Public License, but Overlay.NET, which
forms a large important part of this package does not have a specific defined
license - https://github.com/lolp1/Overlay.NET/issues/13

Since Overlay.NET has been published by it's author to NuGet, and is used widely I
assume that "using" it is expected and encouraged but cannot apply a license to
this derived work yet.

# Protocol

EDMC Overlay offers a very very simple line-json based network protocol.

The service when started will listen on TCP 127.0.0.1:5010.  If EDMCOverlay cannot
detect EliteDangerous64.exe it will exit silently.
 
Assuming EliteDangerous64.exe is running, you may send a single JSON message (on one line)
Like so:

```
{"id": "test1", "text": "You are low on fuel!", "color": "red", "x": 200, "y": 100, "ttl": 8}
```

The server will process this as an instruction to display the message "You are low on fuel!"
in red text at 200,100 for 8 seconds.
 
Be sure to send a newline ("\n") character after your message. You may need to flush the 
socket.

There are (currently) no response values from the service.

