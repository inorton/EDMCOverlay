"""
Plugin for EDMCOverlay
"""
import sys
import os
import subprocess
import time

HERE = os.path.dirname(os.path.abspath(__file__))
PROG = "EDMCOverlay.exe"

_client = None
_service = None


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

    if HERE not in sys.path:
        sys.path.append(HERE)
        ensure_service()
        time.sleep(2)
        try:
            _client.send_message("edmcintro", "EDMC Ready", "yellow", 30, 165, ttl=6)
        except Exception:
            pass
    return "EDMCOverlay"


def ensure_service():
    """
    Start the overlay service program
    :return:
    """
    global _client
    global _service
    program = find_server_program()

    if program:
        # if it isnt running, start it, if it has died, restart
        if _service:
            if not _service.poll():
                _service = None
        if not _service:
            _service = subprocess.Popen([program], cwd=HERE)

        if not _client:
            import edmcoverlay
            _client = edmcoverlay.Overlay()

    return _client


def journal_entry(cmdr, system, station, entry, state):
    """
    Make sure the service is up and running
    :param cmdr:
    :param system:
    :param station:
    :param entry:
    :param state:
    :return:
    """
    ensure_service()
