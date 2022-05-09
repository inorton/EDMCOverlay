"""
Client library for EDMCOverlay
"""

from __future__ import print_function

import sys
import socket
import json
import os
import subprocess
import time


SERVER_ADDRESS = "127.0.0.1"
SERVER_PORT = 5010

HERE = os.path.dirname(os.path.abspath(__file__))
PROG = "EDMCOverlay.exe"

try:
    import monitor
except ImportError:
    monitor = None


def trace(msg):
    """
    Print a trace message
    :param msg:
    :return:
    """
    print("EDMCOverlay: {}".format(msg), file=sys.stderr)
    return msg


_prog = None


def find_server_program():
    """
    Look for EDMCOverlay.exe
    :return:
    """
    global _prog
    if _prog is not None:
        return _prog

    locations = [
        os.path.join(HERE, PROG),
        os.path.join(HERE, "EDMCOverlay", PROG),
        os.path.join(HERE, "EDMCOverlay", "EDMCOverlay", "bin", "Release", PROG),
        os.path.join(HERE, "EDMCOverlay", "EDMCOverlay", "bin", "Debug", PROG),
    ]
    for item in locations:
        if os.path.isfile(item):
            trace("EDMCOverlay: exe found at {}...".format(item))
            _prog = item
            return item
    return None

_service = None


def check_game_running():
    if not monitor:
        return True

    return monitor.monitor.game_running()


def ensure_service(args=[]):
    """
    Start the overlay service program
    :return:
    """
    if HERE not in sys.path:
        sys.path.append(HERE)

    if not check_game_running():
        return

    global _service
    program = find_server_program()
    exedir = os.path.abspath(os.path.dirname(program))

    # see if it is alive
    try:
        internal.connect()
        internal.send_message(0, ".", "black", 0, 0, 1)
        return
    except Exception:
        trace("Overlay server is not running..")
        # if it isnt running, start it
        try:
            if _service:
                if _service.poll() is not None:
                    _service = None

            if not _service:
                if check_game_running():
                    trace("EDMCOverlay is starting {} with {}".format(program, args))
                prog_args = [program]+args
                _service = subprocess.Popen(prog_args, cwd=exedir)
            time.sleep(2)
            if _service.poll() is not None:
                subprocess.check_call(prog_args, cwd=exedir)
                raise Exception("{} exited".format(program))
        except Exception as err:
            if check_game_running():
                trace("error in ensure_service: {}".format(err))


class Overlay(object):
    """
    Client for EDMCOverlay
    """

    def __init__(self, server=SERVER_ADDRESS, port=SERVER_PORT, args=[]):
        self.server = server
        self.port = port
        self.args = args
        self.connection = None

    def connect(self):
        """
        open the connection
        :return:
        """
        connection = socket.socket()
        connection.connect((self.server, self.port))
        self.connection = connection
    
    def send_raw(self, msg):
        """
        Encode a dict and send it to the server
        :param msg:
        :return:
        """
        if not self.connection:
            return None

        assert isinstance(msg, dict)

        try:
            data = json.dumps(msg)
            if sys.version_info.major == 3:
                self.connection.send(data.encode("utf-8"))
                self.connection.send(b"\n")
            else:
                self.connection.send(data)
                self.connection.send("\n")
        except Exception as err:
            self.connection = None
            print(u"send_raw failed with {}".format(err))
            raise
        return None

    def send_shape(self, shapeid, shape, color, fill, x, y, w, h, ttl):
        """
        Send a shape
        :param shapeid:
        :param shape:
        :param color:
        :param fill:
        :param x:
        :param y:
        :param w:
        :param h:
        :param ttl:
        :return:
        """
        if not self.connection:
            ensure_service(self.args)
            self.connect()

        msg = {"id": shapeid,
               "shape": shape,
               "color": color,
               "fill": fill,
               "x": x, "y": y,
               "w": w, "h": h,
               "ttl": ttl
               }
        self.send_raw(msg)

    def send_message(self, msgid, text, color, x, y, ttl=4, size="normal"):
        """
        Send a message
        :param msgid:
        :param text:
        :param color:
        :param x:
        :param y:
        :param ttl:
        :param size:
        :return:
        """
        if not self.connection:
            ensure_service(self.args)
            self.connect()

        msg = {"id": msgid,
               "color": color,
               "text": text,
               "size": size,
               "x": x, "y": y,
               "ttl": ttl}
        self.send_raw(msg)

    def send_shape(self, shapeid, shape, color, fill, x, y, w, h, ttl):
        """
        Send a shape
        :param shapeid:
        :param shape:
        :param color:
        :param fill:
        :param x:
        :param y:
        :param w:
        :param h:
        :param ttl:
        :return:
        """
        if not self.connection:
            ensure_service(self.args)
            self.connect()

        msg = {"id": shapeid,
               "shape": shape,
               "color": color,
               "fill": fill,
               "x": x, "y": y,
               "w": w, "h": h,
               "ttl": ttl
               }
        self.send_raw(msg)


def debugconsole():
    """
    Print stuff
    """
    import load as loader
    loader.plugin_start()

    cl = Overlay()

    while True:
        line = sys.stdin.readline().strip()
        cl.send_message("msg", line, "red", 100, 100)


if __name__ == "__main__":
    debugconsole()

internal = Overlay()
