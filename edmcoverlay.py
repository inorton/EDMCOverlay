"""
Client library for EDMCOverlay
"""

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

    def send_message(self, msgid, text, color, x, y):
        """
        Send a message
        :param msgid:
        :param text:
        :param color:
        :param x:
        :param y:
        :return:
        """
        if not self.connection:
            self.connect()

        msg = {"Id": msgid, "Color": color, "Text": text, "X": x, "Y": y}
        self.connection.send(json.dumps(msg))
        self.connection.send("\n")
        self.connection.flush()
