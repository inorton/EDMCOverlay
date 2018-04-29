#!env python
"""
Prompt for a password for signtool
"""

import subprocess
import sys
import getpass
import os
import shutil

from Tkinter import *


def getpwd(prompt):
    store = {}
    root = Tk()
    pwdbox = Entry(root, show='*')

    def onpwdentry(evt):
        store["pass"] = pwdbox.get()
        root.destroy()

    def onokclick():
        onpwdentry(None)

    Label(root, text=prompt).pack(side='top')

    pwdbox.pack(side='top')
    pwdbox.bind('<Return>', onpwdentry)
    Button(root, command=onokclick, text='OK').pack(side='top')

    root.mainloop()
    return store["pass"]


TIMESTAMP_SERVER = "http://timestamp.comodoca.com/authenticode"
SIGNTOOL = os.path.join(os.path.abspath(os.sep),
                        "Program Files (x86)", "Windows Kits", "8.1", "bin", "x64", "signtool.exe")


def execute(pfxfile, exefile, gui=False, copy=None):
    """
    Run the signer
    :param args:
    :return:
    """
    print("Running in {}".format(os.getcwd()))
    assert os.path.exists(pfxfile), "Can't find pfx file {}".format(pfxfile)
    assert os.path.exists(exefile), "Can't find exe file {}".format(exefile)
    assert os.path.exists(SIGNTOOL), "Can't find signtool"

    if not gui:
        password = getpass.getpass("Enter password for {}:".format(pfxfile))
    else:
        password = getpwd("Enter password for {}:".format(pfxfile))

    cmd = [SIGNTOOL, "sign",
           "/t", TIMESTAMP_SERVER,
           "/p", password,
           "/f", pfxfile,
           "/v", exefile]
    subprocess.check_call(cmd)
    if copy:
        folder = os.path.dirname(copy)
        if not os.path.exists(folder):
            os.makedirs(folder)
        shutil.copy(exefile, copy)


if __name__ == "__main__":
    execute(sys.argv[1], sys.argv[2], copy=sys.argv[3], gui="--gui" in sys.argv)
