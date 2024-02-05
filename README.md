# EDMC Overlay (c) 2020 Ian Norton
## About

EDMC Overlay is a helper program for Elite:Dangerous, It provides a means for 
a program such as EDMC to display simple messages in the game's DirectX window.

## Compatibility

* Currently supports 64Bit Elite:Dangerous (Horizons) on Windows Only.
* 64bit Non-Horizons may work. YMMV.
* Apple support is not likley (I don't have a Mac)
* "Windowed" or "Bordless Fullscreen" mode only.

## Installation

This is released as an installable EDMC Plugin, download the msi file from the 
[Releases page](https://github.com/inorton/EDMCOverlay/releases) and run the 
installer as the same windows user you use to play Elite:Dangerous

## Trusting EDMC Overlay

The New installer and server program are now signed.  EDMCOverlay has a certificate
authority (CA) that signs our own authenticode key.

The CA certificate thumbprint is 0c2120b504788afd322dc7c45a8a023ca6850787
and is located at [https://github.com/inorton/EDMCOverlay/blob/master/edmcoverlay-rootca.cer](https://github.com/inorton/EDMCOverlay/blob/master/edmcoverlay-rootca.cer)

## MIT License

Copyright 2017 Ian Norton

Permission is hereby granted, free of charge, to any person obtaining a copy of this 
software and associated documentation files (the "Software"), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
permit persons to whom the Software is furnished to do so, subject to the following 
conditions:

The above copyright notice and this permission notice shall be included in all copies 
or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

# Protocol

EDMC Overlay offers a very very simple line-json based network protocol.

The service when started will listen on TCP 127.0.0.1:5010.  If EDMCOverlay cannot
detect EliteDangerous64.exe it will exit silently.
 
Assuming EliteDangerous64.exe is running, you may send a single JSON message (on one line)
Like so:

```
{"id": "test1", "text": "You are low on fuel!", "size": "normal", "color": "red", "x": 200, "y": 100, "ttl": 8}
```
Supported colors values are:
 "red", "green", "yellow", "blue" or "#rrggbb".

Supported size values are: 
 "normal" and "large"

Additionally, you may draw rectangles by setting the "shape" to "rect" and setting the "color" and/or "fill" values.

```
{"id": "fred", "shape": "rect", "x": 100, "y": 10, "w": 30:, "h": 5, "fill": "red", "color", "#ccff00"}
```

The server will process this as an instruction to display the message "You are low on fuel!"
in red text at 200,100 for 8 seconds.
 
Be sure to send a newline ("\n") character after your message. You may need to flush the 
socket.

There are (currently) no response values from the service.

