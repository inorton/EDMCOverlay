"""
Plugin for EDMCOverlay
"""
import time
import os
from edmcoverlay import ensure_service, Overlay, trace


HERE = os.path.dirname(os.path.abspath(__file__))
PLUGDIR = os.path.dirname(HERE)
client = Overlay()


def notify_old_hits():
    """
    If an old copy of HITS is installed, notify the user
    :return:
    """
    if notify_old_hits.done:
        return
    notify_old_hits.done = True
    hits_overlay_dist = os.path.join(PLUGDIR, "HITS", "EDMCOverlay", "EDMCOverlay.exe")
    if os.path.exists(hits_overlay_dist):
        client.send_message("warning",
                            trace("An older version of the HITS plugin is installed, please update it"),
                            "#ffcc00", 30, 30, ttl=10
                            )
notify_old_hits.done = False


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
        notify_old_hits()
    except Exception:
        pass
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
