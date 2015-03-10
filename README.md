# VVVVulture
Plugin to send geometry data directly from Grasshopper (McNeels Rhinoceros PlugIn) to VVVV


Vulture?! -> Just another animal used as plugin name for Grasshopper like it is common.


VultureGH -> Grasshopper Component

In the folder VultureGH you find everything necessary for the Grasshopper PlugIn.
The .gha File in the bin/ folder is the actual plugin. This can easily be added via
drag & drop on the Grasshopper canvas.
Right now it has only 3 inputs. One for the Mesh, one bool to either (true) write the data to a file on disk or (false) in the sharedmemory and
the filepath.
The idea is to grab a lot more data like Camera Positions, Lights, maybe Curves etc.


vulture -> Class for Exchange

This is the Class Library to store the data that will be exchanged between Grasshopper and VVV.
Right now its a bit messy...will clean it up in the near future.


VVVVulture -> VVVV node

In this folder you find the vvvv Node to read the data passed with the vulture class from Grasshopper.
Right now it has 3 inputs. One to enable and update, one bool to either (true) read the file from disk or (false) from sharedmemory and the filepath.
There are two outputs. One for stupid messages and the facecount, one for the constructed Mesh.


Sample File

Here you find a sample file you can use with the vvvv node if you don't have Grasshopper. Its showing a simple "twisted tower"

