"""
Test the overlay server message handling
"""

import edmcoverlay
import random
import time


def test_raw_messages():
    """
    Test sending short messages, this is to test that the server gets all messages (nothing should be left onscreen)
    :return:
    """

    overlay = edmcoverlay.Overlay()
    overlay.connect()
    for i in range(1, 200):
        overlay.send_raw({
            "id": "foo{}".format(i),
            "x": random.randint(10, 1024),
            "y": (6 * i) % 960,
            "color": "#ff0000",
            "text": "msg{}".format(i),
            "ttl": 60,
        })

    time.sleep(2)
    for i in range(1, 200):
        overlay.send_raw({
            "id": "foo{}".format(i),
            "ttl": 0,
        })
    time.sleep(2)