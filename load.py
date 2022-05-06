"""
Plugin for EDMCOverlay
"""
import os
from edmcoverlay import ensure_service, Overlay, trace

HERE = os.path.dirname(os.path.abspath(__file__))
PLUGDIR = os.path.dirname(HERE)
client = Overlay()


def plugin_start3(plugin_dir):
    return plugin_start()


def plugin_start():
    """
    Start our plugin, add this dir to the search path so others can use our module
    :return:
    """
    ensure_service()
    try:
        client.send_message("edmcintro", trace("EDMC Ready"), "yellow", 30, 165, ttl=6)
    except Exception as err:
        print("Error sending message in plugin_start() : {}".format(err))
    return "EDMCOverlay"


def journal_entry(cmdr, is_beta, system, station, entry, state):
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
