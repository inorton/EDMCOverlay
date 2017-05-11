"""
Plugin for EDMCOverlay
"""
import sys
import os
import subprocess
import time

HERE = os.path.dirname(os.path.abspath(__file__))
PROG = "EDMCOverlay.exe"

_started = False
_client = None


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


def plugin_start():
    """
    Start our plugin, add this dir to the search path so others can use our module
    :return:
    """
    global _started
    global _client
    if HERE not in sys.path:
        sys.path.append(HERE)

    # start our thingy
    program = find_server_program()
    if program:
        subprocess.Popen([program])
        time.sleep(10)
        _started = True
        import edmcoverlay
        _client = edmcoverlay.Overlay()
    return "EDMCOverlay"


def journal_entry(cmdr, system, station, entry, state):
    """
    Process a journal item
    :param cmdr:
    :param system:
    :param station:
    :param entry:
    :param state:
    :return:
    """
    global _client
    _client.send_message("sys", system, "green", 100, 100)