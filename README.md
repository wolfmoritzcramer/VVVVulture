# VVVVulture
Plugin to send geometry data directly from Grasshopper (McNeels Rhinoceros PlugIn) to VVVV


Vulture?! -> Just another animal used as plugin name for Grasshopper like it is common.


VultureGH -> Grasshopper Component

In the folder VultureGH you find everything necessary for the Grasshopper PlugIn.
The .gha File in the bin/ folder is the actual plugin. This can easily be added via
drag & drop on the Grasshopper canvas.
It has seven inputs:
M: All the meshes you want to pass to vvvv. Its important that the list of meshes is flatten before or in the input (right click on the M -> flatten)
P: List of points you want to pass. E.g. cameras, lights, projector position...
I: List of integer values you want to pass. E.g. number of items belonging to one group in the list of points
T: List of text/string values you want to pass. E.g. Description of the data
RhC: Set to true if you want to send the camera data auf the active viewport. If you want it to constantly update use a TIMER component and connect it.
P: Set the file path. If it is empty, file is written to the memory
On: Enabled or not ;)

vulture -> Class for Exchange

This is the Class Library to store the data that will be exchanged between Grasshopper and VVV.
Still a bit messy...will clean it up in the near future.


VVVVulture -> VVVV node

In this folder you find the vvvv Node to read the data passed with the vulture class from Grasshopper.

Sample File

Here you find a sample file you can use with the vvvv node if you don't have Grasshopper. Its showing a simple "twisted tower" and a box.
