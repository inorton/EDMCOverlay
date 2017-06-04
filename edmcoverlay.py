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
        os.path.join(HERE, "EDMCOverlay", "EDMCOverlay", "bin", "Debug", PROG),
        os.path.join(HERE, "EDMCOverlay", "EDMCOverlay", "bin", "Release", PROG),
        os.path.join(HERE, "EDMCOverlay", PROG)
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
                if not _service.poll():
                    _service = None
            if not _service:
                _service = subprocess.Popen([program], cwd=HERE)
            time.sleep(2)
        except Exception as err:
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
            self.connect()

        msg = {"id": msgid,
               "color": color,
               "text": text,
               "size": size,
               "x": x, "y": y,
               "ttl": ttl}
        try:
            self.connection.send(json.dumps(msg))
            self.connection.send("\n")
        except Exception as err:
            print err
            self.connection = None
            raise


def testconsole():
    """
    Print stuff
    """
    import load as loader
    
    print >> sys.stderr, "Loading..\n"
    loader.plugin_start()

    cl = Overlay()
    cl.connect()

    print >> sys.stderr, "Reading..\n"
    while True:
        line = sys.stdin.readline().strip()
        print >> sys.stderr, "sending... {}".format(line)
        cl.send_message("msg", line, "red", 100, 100)


if __name__ == "__main__":
    testconsole()
