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


def trace(msg):
    """
    Print a trace message
    :param msg:
    :return:
    """
    print("EDMCOverlay: {}".format(msg), file=sys.stderr)
    return msg


def find_server_program():
    """
    Look for EDMCOverlay.exe
    :return:
    """

    locations = [
        os.path.join(HERE, PROG),
        os.path.join(HERE, "EDMCOverlay", PROG),
        os.path.join(HERE, "EDMCOverlay", "EDMCOverlay", "bin", "Release", PROG),
        os.path.join(HERE, "EDMCOverlay", "EDMCOverlay", "bin", "Debug", PROG),
    ]
    for item in locations:
        if os.path.isfile(item):
            print("EDMCOverlay: exe found: {}...".format(item))
            return item
    return None


_service = None


def ensure_service():
    """
    Start the overlay service program
    :return:
    """
    if HERE not in sys.path:
        sys.path.append(HERE)

    global _service

    # if it isnt running, start it
    try:
        if _service:
            if _service.poll() is not None:
                _service = None
        if not _service:
            program = find_server_program()
            trace("EDMCOverlay is starting {}".format(program))
            exedir = os.path.abspath(os.path.dirname(program))
            _service = subprocess.Popen([program], cwd=exedir)
        time.sleep(2)
        if _service.poll() is not None:
            subprocess.check_call([program], cwd=exedir)
            raise Exception("{} exited".format(program))
    except Exception as err:
        trace("error in ensure_service: {}".format(err))


class Overlay(object):
    """
    Client for EDMCOverlay
    """

    def __init__(self, server=SERVER_ADDRESS, port=SERVER_PORT):
        self.server = server
        self.port = port
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
        assert isinstance(msg, dict)

        try:
            data = json.dumps(msg)
            self.connection.send(data.encode())
            self.connection.send(b"\n")
        except Exception as err:
            print("EDMCOverlay: error in send_raw: {}".format(err))
            self.connection = None
            raise
        return None

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
            ensure_service()
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
            ensure_service()
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

    print("EDMCOverlay: Loading..\n", file=sys.stderr)
    loader.plugin_start()

    cl = Overlay()

    print("EDMCOverlay: Reading..\n", file=sys.stderr)
    while True:
        line = sys.stdin.readline().strip()
        print("EDMCOverlay: sending... {}".format(line), file=sys.stderr)
        cl.send_message("msg", line, "red", 100, 100)


if __name__ == "__main__":
    debugconsole()
