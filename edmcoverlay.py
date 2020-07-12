"""
Client library for EDMCOverlay
"""

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
    program = find_server_program()

    if program:
        # if it isnt running, start it
        try:
            if _service:
                if _service.poll() is not None:
                    _service = None
            if not _service:
                exedir = os.path.abspath(os.path.dirname(program))
                _service = subprocess.Popen([program], cwd=exedir)

            time.sleep(2)
            if _service.poll() is not None:
                subprocess.check_call([program], cwd=exedir)
                raise Exception("{} exited".format(program))
        except Exception as err:
            print(u"ensure_service failed with {}".format(err))
            pass


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
