"""
Client library for EDMCOverlay
"""

import sys
import socket
import json

SERVER_ADDRESS = "127.0.0.1"
SERVER_PORT = 5010


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
        except Exception:
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