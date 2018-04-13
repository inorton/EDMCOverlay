"""
Test the overlay server message handling
"""

import edmcoverlay


def test_raw_messages():
    """
    Test sending short messages, this is to test that the server gets all three empty messages
    :return:
    """

    overlay = edmcoverlay.Overlay()
    overlay.connect()
    overlay.send_raw({})
    overlay.send_raw({})
    overlay.send_raw({})