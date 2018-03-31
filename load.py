"""
Plugin for EDMCOverlay
"""
import time

from edmcoverlay import ensure_service, Overlay

client = Overlay()


def plugin_start():
    """
    Start our plugin, add this dir to the search path so others can use our module
    :return:
    """
    ensure_service()
    time.sleep(2)
    try:
        client.send_message("edmcintro", "EDMC Ready", "yellow", 30, 165, ttl=6)
    except Exception:
        pass
    return "EDMCOverlay"


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


def plugin_stop():
    """
    Edmc is going to exit.
    :return:
    """
    client.send_raw({
        "command": "exit"
    })
